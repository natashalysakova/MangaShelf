# Parsing Job State Machine Documentation

## Overview

The Parsing Job State Machine is a centralized system for managing the lifecycle of parsing jobs in MangaShelf. It enforces valid state transitions, prevents invalid state changes, and provides an event-driven architecture for decoupled handling of state changes.

**Key Benefits:**
- ✅ **Strict State Validation** — Impossible states are prevented at runtime
- ✅ **Audit Trail** — Every transition is logged with timestamp and context
- ✅ **Event-Driven** — Subscribers can react to state changes without coupling
- ✅ **Single Source of Truth** — All transition logic is centralized

---

## State Machine Architecture

### State Diagram

```
					┌─────────────┐
					│   CREATED   │
					└──────┬──────┘
						   │
					  JobCreated
						   │
						   ▼
					┌─────────────┐
					│   WAITING   │  (Initial State)
					└──────┬──────┘
						   │
		   ┌───────────────┼───────────────┐
		   │               │               │
	StartGathering   CancelJob       JobFailed
		   │               │               │
		   ▼               ▼               ▼
	┌──────────────┐  ┌──────────────┐  ┌──────────────┐
	│GATHERING     │  │  CANCELLED   │  │  ERROR       │
	│VOLUMES       │  │ (Terminal)   │  │ (Terminal)   │
	└──────┬───────┘  └──────────────┘  └──────────────┘
		   │
	 ┌─────┼─────┐
	 │     │     │
  BeginParsing   │  CancelJob
	 │     │     │
	 │  JobFailed │
	 │     │     │
	 ▼     ▼     ▼
  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
  │  RUNNING     │  │  ERROR       │  │  CANCELLED   │
  │              │  │ (Terminal)   │  │ (Terminal)   │
  └──────┬───────┘  └──────────────┘  └──────────────┘
		 │
		 │ UpdateProgress (internal)
		 ├─ (no state change)
		 │
		 ├─ Complete ─────────┐
		 │                    ▼
		 ├─ JobFailed ────┐  ┌──────────────┐
		 │                └─>│  ERROR       │
		 └─ CancelJob ──-─┐  │ (Terminal)   │
		  				  └─>└──────────────┘
								┌──────────────┐
								│  FINISHED    │
								│ (Terminal)   │
								└──────────────┘
```

---

## States and Meanings

| State | Description | Duration | Exit Action |
|-------|-------------|----------|------------|
| **Created** | Job object created but not queued | Immediate | Transition to Waiting |
| **Waiting** | Job created, not started | Variable | Mark as started, init progress |
| **GatheringVolumes** | Discovering volumes from website | Minutes | Record volume count |
| **Running** | Parsing individual volumes | Minutes/Hours | Mark completion % |
| **Finished** | ✅ Successfully completed | - | Update next run schedule |
| **Error** | ❌ Failed due to exception | - | Record error details |
| **Cancelled** | ⛔ Cancelled by user/system | - | Clean up resources |

---

## Valid Transitions

### From CREATED
```
CREATED → WAITING  via JobCreated
```

### From WAITING
```
WAITING → GATHERING_VOLUMES  via StartGathering
WAITING → CANCELLED          via CancelJob
WAITING → ERROR              via JobFailed
```

### From GATHERING_VOLUMES
```
GATHERING_VOLUMES → RUNNING   via BeginParsing
GATHERING_VOLUMES → ERROR     via JobFailed (gathering failed)
GATHERING_VOLUMES → CANCELLED via CancelJob
```

### From RUNNING
```
RUNNING → FINISHED  via Complete
RUNNING → ERROR     via JobFailed
RUNNING → CANCELLED via CancelJob
RUNNING → RUNNING   via UpdateProgress (internal transition, no state change)
```

### From Terminal States (No Transitions Allowed)
```
FINISHED → (no transitions)
ERROR    → (no transitions)
CANCELLED → (no transitions)
```

---

## Triggers Explained

### `JobCreated`
- **When:** Initial state machine setup for a job
- **From:** `Created`
- **To:** `Waiting`
- **Context:** "Job created and queued"
- **Called by:** `ParseJobManagerService.QueueJob()` or similar initialization method

### `StartGathering`
- **When:** Job runner picks up a job from the queue
- **From:** `Waiting`
- **To:** `GatheringVolumes`
- **Context:** "Starting volume gathering phase"
- **Called by:** `ParseJobManagerService.RunJob()`

### `BeginParsing`
- **When:** Volume discovery completes successfully
- **From:** `GatheringVolumes`
- **To:** `Running`
- **Context:** "Starting to parse {N} volumes"
- **Called by:** `ParseJobManagerService.SetToParsingStatus()`

### `UpdateProgress`
- **When:** Progress update during parsing (no state change)
- **From:** `Running`
- **To:** `Running` (internal transition)
- **Context:** "Progress update: {X}% complete" or similar
- **Purpose:** Allows progress tracking without changing state; triggers event subscribers to update progress in DB
- **Called by:** `ParseJobManagerService.UpdateJobProgress()`

### `Complete`
- **When:** All volumes parsed successfully
- **From:** `Running`
- **To:** `Finished`
- **Context:** "Job parsing completed successfully"
- **Called by:** `ParseJobManagerService.SetToFinishedStatus()`

### `JobFailed`
- **When:** An exception occurs during any phase
- **From:** `Waiting`, `GatheringVolumes`, or `Running`
- **To:** `Error`
- **Context:** Error message or reason
- **ExceptionDetails:** Stack trace (optional)
- **Called by:** `ParseJobManagerService.SetToErrorStatus()`, exception handlers

### `CancelJob`
- **When:** User cancels the job manually or system-initiated cancellation
- **From:** `Waiting`, `GatheringVolumes`, or `Running`
- **To:** `Cancelled`
- **Context:** Cancellation reason
- **Called by:** `ParseJobManagerService.CancelJob()`, `ParseJobManagerService.SetToCancelledStatus()`

---

## Event-Driven Architecture

### Event Flow

```
State Machine                Publisher              Handlers
	│                            │                      │
	├─ FireTrigger()             │                      │
	│   - Validates transition   │                      │
	│   - Changes state          │                      │
	│   - Raises event           │                      │
	│                            │                      │
	└─────────────────────────── JobStateTransition ───────┐
								  │                        │
								  ├─ SaveJobStateHistoryHandler
								  │   └─ Persists to JobStateHistories table
								  │
								  ├─ NotifyJobStatusChangedHandler
								  │   └─ Updates Runs.Status
								  │
								  └─ HandleJobErrorHandler
									  └─ Records error details (if Error state)
```

### Event Publishing Process

1. **State machine fires trigger**
   - Validates transition is allowed
   - Changes internal state
   - Raises `OnStateTransition` event

2. **Publisher receives event**
   - Wraps it in `JobStateTransition` DTO
   - Calls all subscribed handlers sequentially

3. **Handlers process transition**
   - Save to audit trail
   - Update job status in DB
   - Record error details if needed
   - Send notifications (future enhancement)

### Error Handling in Publishers

- **Handler Exception:** Logged but doesn't stop other handlers
- **Cancellation:** Propagates immediately, stops processing
- **Database Error:** Retried by application retry policy

---

## Event Handlers

The following handlers subscribe to state transitions and perform side effects:

### 1. SaveJobStateHistoryHandler
**Purpose:** Maintains audit trail of all state transitions  
**Triggers on:** All state transitions  
**Actions:**
- Creates `JobStateHistory` record in database
- Records: FromState, ToState, Trigger, TransitionTime, Context, ExceptionDetails
- Logs debug message for each transition saved

**Execution Order:** First (should run before other handlers)  
**Dependencies:** `IDbContextFactory<MangaSystemDbContext>`

```csharp
// Saves to JobStateHistories table
new JobStateHistory
{
    JobId = transition.JobId,
    FromState = transition.FromState,
    ToState = transition.ToState,
    Trigger = transition.Trigger,
    TransitionTime = transition.TransitionTime,
    Context = transition.Context,
    ExceptionDetails = transition.ExceptionDetails
};
```

### 2. NotifyJobStatusChangedHandler
**Purpose:** Updates job's current status in the Runs table  
**Triggers on:** All state transitions (excluding internal transitions like UpdateProgress)  
**Actions:**
- Loads job from database
- Updates `Run.Status` to new state
- Maps `RunStatus` to `ParserStatus.Status` and updates parser status
- Persists changes to database
- Logs info message with state change details

**Execution Order:** Second  
**Dependencies:** `IDbContextFactory<MangaSystemDbContext>`

**State Mappings:**
```
RunStatus.Waiting → ParserStatus.Idle
RunStatus.GatheringVolumes → ParserStatus.Busy
RunStatus.Running → ParserStatus.Busy
RunStatus.Finished → ParserStatus.Idle
RunStatus.Error → ParserStatus.Error
RunStatus.Cancelled → ParserStatus.Idle
```

### 3. HandleJobErrorHandler
**Purpose:** Performs error-specific processing when job transitions to Error state  
**Triggers on:** Transitions where `ToState == RunStatus.Error`  
**Actions:**
- Loads job from database
- Sets `Progress = -1` to indicate error state
- Records error details in job's error collection (if present)
- Updates associated parser status to Error state
- Persists changes to database
- Logs warning with error context and exception details

**Execution Order:** Third  
**Dependencies:** `IDbContextFactory<MangaSystemDbContext>`

**Key Logic:**
- Skips if transition is not to Error state (returns early)
- Logs warning if job not found (doesn't throw)
- Records exception details with UTC timestamp

### 4. ProgressChangeHandler
**Purpose:** Updates job progress during Running state  
**Triggers on:** Only when `Trigger == ParseJobTrigger.UpdateProgress`  
**Actions:**
- Loads job from database
- Parses progress value from `transition.Context` (expects double 0-100)
- Updates `Run.Progress` field
- Persists changes to database
- Logs debug message on success, warning on invalid progress value

**Execution Order:** Fourth  
**Dependencies:** `IDbContextFactory<MangaSystemDbContext>`

**Progress Format:**
```
transition.Context = "42.5"  // Valid: parsed as double
transition.Context = "invalid"  // Invalid: logs warning, skips update
```

**Usage Example:**
```csharp
// Fire UpdateProgress trigger with progress value as context
stateMachine.FireTrigger(
    ParseJobTrigger.UpdateProgress, 
    context: "42.5",  // Progress percentage
    exceptionDetails: null
);
```

### Handler Subscription Order

The handlers are registered in DI container in this order for optimal execution:

1. **SaveJobStateHistoryHandler** — Persist audit trail first
2. **NotifyJobStatusChangedHandler** — Update current status
3. **HandleJobErrorHandler** — Handle error-specific logic
4. **ProgressChangeHandler** — Update progress (only for UpdateProgress trigger)

Each handler is independent and isolated. If one throws an exception, others continue executing.

### Adding New Handlers

To add a new handler:

1. Implement `IJobStateTransitionHandler` interface
2. Add constructor with `IDbContextFactory<MangaSystemDbContext>` and `ILogger<YourHandler>`
3. Implement `HandleAsync(JobStateTransition transition, CancellationToken cancellationToken)` method
4. Register in DI container in `Program.cs`:
   ```csharp
   services.AddScoped<IJobStateTransitionHandler, YourNewHandler>();
   ```

Example:
```csharp
public class NotifySlackOnJobErrorHandler : IJobStateTransitionHandler
{
    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        if (transition.ToState != RunStatus.Error)
            return;

        // Send Slack notification
        await _slackService.NotifyAsync($"Job {transition.JobId} failed: {transition.Context}");
    }
}
```

---

## Integration Points

### ParseJobManagerService

Refactored to use state machine:

```csharp
// Old: Direct status update
await SetStatusInternal(jobId, RunStatus.Running, volumeCount);

// New: State machine-based
await TransitionJobState(jobId, ParseJobTrigger.BeginParsing, 
	context: $"Starting to parse {volumeCount} volumes");
```

### ParseJobRunner (No changes needed)

Already uses manager service methods which now internally use state machine:

```csharp
var parseService = scope.ServiceProvider.GetRequiredService<IParseService>();
await parseService.RunParseJob(jobId, cancellationTokenSource.Token);
```

### ParserService (No changes needed)

Calls manager service methods for all state transitions:

```csharp
await _jobManagerService.RunJob(jobId, token);
await _jobManagerService.SetToParsingStatus(jobId, volumesToParse, token);
await _jobManagerService.SetToFinishedStatus(jobId, token);
```

---

## Database Audit Trail

### JobStateHistories Table

```sql
CREATE TABLE JobStateHistories (
	Id CHAR(36) PRIMARY KEY,
	JobId CHAR(36) NOT NULL,
	FromState INT NOT NULL,  -- RunStatus enum value
	ToState INT NOT NULL,    -- RunStatus enum value
	Trigger LONGTEXT NOT NULL,
	TransitionTime DATETIME(6) NOT NULL,
	Context LONGTEXT,
	ExceptionDetails LONGTEXT,
	FOREIGN KEY (JobId) REFERENCES Runs(Id) ON DELETE CASCADE
);
```

### Sample History Records

```
JobId: {guid}
| FromState | ToState | Trigger        | Context | TransitionTime |
|-----------|---------|----------------|---------|----------------|
| Waiting   | Gathering | StartGathering | ... | 2026-08-15 10:00:00 |
| Gathering | Running | BeginParsing   | 42 vols | 2026-08-15 10:05:00 |
| Running   | Finished | Complete      | Done    | 2026-08-15 10:30:00 |
```

---

## Testing Strategy

### Unit Tests

**ParseJobStateMachineTests** (40+ tests)
- ✅ Valid transitions allowed
- ✅ Invalid transitions throw
- ✅ Event args populated correctly
- ✅ Multiple subscribers called
- ✅ Full lifecycle scenarios
- ✅ Error recovery paths

**JobStateTransitionPublisherTests** (16+ tests)
- ✅ Handler subscription/unsubscription
- ✅ All handlers called on publish
- ✅ Error isolation (one handler failing doesn't stop others)
- ✅ Cancellation token propagation
- ✅ Transition details preserved

Run tests with:
```powershell
dotnet test MangaShelf.Parser.Tests.csproj --filter ParseJobStateMachine
dotnet test MangaShelf.Parser.Tests.csproj --filter JobStateTransitionPublisher
```

---

## Usage Examples

### Transition with Context

```csharp
try
{
	await _jobManagerService.SetToParsingStatus(jobId, volumesToParse, token);
	// State transition happens:
	// 1. Create state machine with current job state
	// 2. Fire BeginParsing trigger
	// 3. Publish JobStateTransition event
	// 4. Handlers persist to DB, update status, etc.
}
catch (InvalidOperationException ex)
{
	// Job is not in a state where parsing can begin
	_logger.LogError(ex, "Cannot start parsing job {JobId}", jobId);
}
```

### Handle Errors

```csharp
try
{
	result = await ParsePageInternal(jobId, volume, parser, token);
}
catch (Exception ex)
{
	// Transition to Error state
	await _jobManagerService.SetToErrorStatus(jobId, token);
	// This fires RecordError trigger, publishes event,
	// handlers record error details in DB
}
```

### Check Before Transitioning

```csharp
var stateMachine = new ParseJobStateMachine(currentJobState);

if (stateMachine.CanFire(ParseJobTrigger.BeginParsing))
{
	// Safe to proceed
	await _jobManagerService.SetToParsingStatus(jobId, volumes, token);
}
else
{
	_logger.LogWarning("Cannot start parsing: job not in Gathering state");
}
```

---

## Future Enhancements

1. **Persistence of State Machines**
   - Store state machine configuration in DB
   - Allow dynamic state configuration

2. **Notifications**
   - Handler for Slack/email notifications on state changes
   - Real-time UI updates via SignalR

3. **Metrics and Monitoring**
   - Track average time in each state
   - Alert on stuck jobs (too long in Running state)

4. **Retry Logic**
   - Automatic retry from Error state
   - Exponential backoff

5. **Job History Dashboard**
   - UI to view job state transitions
   - Timeline view of state changes

---

## Troubleshooting

### "Cannot transition from X via trigger Y"

**Cause:** Invalid state transition attempted  
**Solution:** Check current job state and allowed transitions from that state

### "Job {id} not found"

**Cause:** Transition attempted for non-existent job  
**Solution:** Verify job was created and exists in database

### Handler not being called

**Cause:** Handler not subscribed to publisher  
**Solution:** Verify handler registration in DI container

### Transition history missing

**Cause:** SaveJobStateHistoryHandler failed silently  
**Solution:** Check application logs for handler errors

---

## Glossary

| Term | Meaning |
|------|---------|
| **Trigger** | Action that causes a state transition |
| **Transition** | Movement from one state to another |
| **Terminal State** | State with no outgoing transitions (Finished, Error, Cancelled) |
| **Event Handler** | Subscriber that reacts to state transitions |
| **Audit Trail** | Historical record of all state changes |
| **Context** | Metadata about why a transition occurred |

---

## Related Files

- **State Machine:** `MangaShelf.BL/Services/Parsing/ParseJobStateMachine.cs`
- **Publisher:** `MangaShelf.BL/Services/Parsing/JobStateTransitionPublisher.cs`
- **Handlers:** `MangaShelf.BL/Services/Parsing/Handlers/`
- **Contracts:** `MangaShelf.BL/Contracts/IJobStateTransitionPublisher.cs`
- **DTOs:** `MangaShelf.BL/Dto/JobStateTransition.cs`
- **Entity:** `MangaShelf.DAL.System/Models/JobStateHistory.cs`
- **Tests:** `MangaShelf.Parser.Tests/ParseJobStateMachineTests.cs`

---

## Contact & Questions

For questions about the state machine implementation, refer to the inline code comments or the comprehensive test suites which serve as usage examples.

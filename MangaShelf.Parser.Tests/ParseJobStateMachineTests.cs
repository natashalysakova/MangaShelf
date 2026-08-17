using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaShelf.Parser.Tests;

/// <summary>
/// Unit tests for ParseJobStateMachine state transitions and validation.
/// </summary>
[TestClass]
public class ParseJobStateMachineTests
{
    [TestMethod]
    public void Constructor_WithWaitingState_InitializesCorrectly()
    {
        // Arrange & Act
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Assert
        Assert.AreEqual(RunStatus.Waiting, stateMachine.CurrentState);
    }

    [TestMethod]
    public void Constructor_WithDefaultParameter_StartsInWaitingState()
    {
        // Arrange & Act
        var stateMachine = new ParseJobStateMachine();

        // Assert
        Assert.AreEqual(RunStatus.Waiting, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_WaitingToGatheringVolumes_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.StartGathering);

        // Assert
        Assert.AreEqual(RunStatus.GatheringVolumes, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_GatheringVolumesToRunning_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.GatheringVolumes);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);

        // Assert
        Assert.AreEqual(RunStatus.Running, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_RunningToFinished_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.Complete);

        // Assert
        Assert.AreEqual(RunStatus.Finished, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_RunningToError_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.JobFailed, "Test error", "Exception details");

        // Assert
        Assert.AreEqual(RunStatus.Error, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_GatheringVolumesToError_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.GatheringVolumes);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.JobFailed);

        // Assert
        Assert.AreEqual(RunStatus.Error, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_RunningToCancelled_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.CancelJob, "User cancelled");

        // Assert
        Assert.AreEqual(RunStatus.Cancelled, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_GatheringVolumesToCancelled_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.GatheringVolumes);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.CancelJob);

        // Assert
        Assert.AreEqual(RunStatus.Cancelled, stateMachine.CurrentState);
    }

    [TestMethod]
    public void FireTrigger_WaitingToRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        bool exceptionThrown = false;

        // Act
        try
        {
            stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "InvalidOperationException should have been thrown");
    }

    [TestMethod]
    public void FireTrigger_WaitingToFinished_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        bool exceptionThrown = false;

        // Act
        try
        {
            stateMachine.FireTrigger(ParseJobTrigger.Complete);
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "InvalidOperationException should have been thrown");
    }

    [TestMethod]
    public void FireTrigger_FinishedToRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Finished);
        bool exceptionThrown = false;

        // Act
        try
        {
            stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "InvalidOperationException should have been thrown");
    }

    [TestMethod]
    public void FireTrigger_ErrorToRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Error);
        bool exceptionThrown = false;

        // Act
        try
        {
            stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "InvalidOperationException should have been thrown");
    }

    [TestMethod]
    public void FireTrigger_CancelledToRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Cancelled);
        bool exceptionThrown = false;

        // Act
        try
        {
            stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown, "InvalidOperationException should have been thrown");
    }

    [TestMethod]
    public void CanFire_WaitingWithStartGathering_ReturnsTrue()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.StartGathering);

        // Assert
        Assert.IsTrue(canFire);
    }

    [TestMethod]
    public void CanFire_WaitingWithBeginParsing_ReturnsFalse()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.BeginParsing);

        // Assert
        Assert.IsFalse(canFire);
    }

    [TestMethod]
    public void CanFire_RunningWithComplete_ReturnsTrue()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.Complete);

        // Assert
        Assert.IsTrue(canFire);
    }

    [TestMethod]
    public void CanFire_RunningWithRecordError_ReturnsTrue()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.JobFailed);

        // Assert
        Assert.IsTrue(canFire);
    }

    [TestMethod]
    public void CanFire_RunningWithCancelJob_ReturnsTrue()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.CancelJob);

        // Assert
        Assert.IsTrue(canFire);
    }

    [TestMethod]
    public void CanFire_FinishedWithAnyTrigger_ReturnsFalse()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Finished);

        // Act & Assert
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.StartGathering));
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.BeginParsing));
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.Complete));
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.JobFailed));
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.CancelJob));
    }

    [TestMethod]
    public void OnStateTransition_EventRaisedOnTransition()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        JobStateTransitionEventArgs? capturedArgs = null;

        stateMachine.OnStateTransition += (sender, args) =>
        {
            capturedArgs = args;
        };

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.StartGathering, "Test context");

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual(RunStatus.Waiting, capturedArgs.FromState);
        Assert.AreEqual(RunStatus.GatheringVolumes, capturedArgs.ToState);
        Assert.AreEqual(ParseJobTrigger.StartGathering, capturedArgs.Trigger);
        Assert.AreEqual("Test context", capturedArgs.Context);
    }

    [TestMethod]
    public void OnStateTransition_MultipleHandlers_AllInvoked()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        int handlerCount = 0;

        stateMachine.OnStateTransition += (sender, args) => handlerCount++;
        stateMachine.OnStateTransition += (sender, args) => handlerCount++;
        stateMachine.OnStateTransition += (sender, args) => handlerCount++;

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.StartGathering);

        // Assert
        Assert.AreEqual(3, handlerCount);
    }

    [TestMethod]
    public void FullJobLifecycle_Waiting_GatheringVolumes_Running_Finished_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act & Assert
        Assert.AreEqual(RunStatus.Waiting, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.StartGathering);
        Assert.AreEqual(RunStatus.GatheringVolumes, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.BeginParsing);
        Assert.AreEqual(RunStatus.Running, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.Complete);
        Assert.AreEqual(RunStatus.Finished, stateMachine.CurrentState);
    }

    [TestMethod]
    public void ErrorRecoveryPath_Waiting_GatheringVolumes_Error_CannotRecover()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.StartGathering);
        Assert.AreEqual(RunStatus.GatheringVolumes, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.JobFailed, "Volume gathering failed");
        Assert.AreEqual(RunStatus.Error, stateMachine.CurrentState);

        // Assert - no transitions allowed from error
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.BeginParsing));
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.StartGathering));
    }

    [TestMethod]
    public void CancellationPath_FromRunning_StopsExecution()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.CancelJob, "User initiated cancellation");

        // Assert
        Assert.AreEqual(RunStatus.Cancelled, stateMachine.CurrentState);
        Assert.IsFalse(stateMachine.CanFire(ParseJobTrigger.Complete));
    }

    [TestMethod]
    public void FireTrigger_WithContext_PreservesContextInEvent()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Running);
        const string testContext = "Found 42 volumes to parse";
        JobStateTransitionEventArgs? capturedArgs = null;

        stateMachine.OnStateTransition += (sender, args) => capturedArgs = args;

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.JobFailed, testContext, "Stack trace here");

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual(testContext, capturedArgs.Context);
        Assert.AreEqual("Stack trace here", capturedArgs.ExceptionDetails);
    }

    [TestMethod]
    public void FireTrigger_SetsTransitionTimeToNowUtc()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        var beforeTime = DateTimeOffset.UtcNow;
        JobStateTransitionEventArgs? capturedArgs = null;

        stateMachine.OnStateTransition += (sender, args) => capturedArgs = args;

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.StartGathering);
        var afterTime = DateTimeOffset.UtcNow;

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.IsTrue(capturedArgs.TransitionTime >= beforeTime);
        Assert.IsTrue(capturedArgs.TransitionTime <= afterTime);
    }

    [TestMethod]
    public void FireTrigger_WaitingToRunningViaSkipGathering_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.SkipGathering);

        // Assert
        Assert.AreEqual(RunStatus.Running, stateMachine.CurrentState);
    }

    [TestMethod]
    public void CanFire_WaitingWithSkipGathering_ReturnsTrue()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act
        var canFire = stateMachine.CanFire(ParseJobTrigger.SkipGathering);

        // Assert
        Assert.IsTrue(canFire);
    }

    [TestMethod]
    public void FireTrigger_WaitingToRunningViaSkipGathering_RaisesEvent()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);
        JobStateTransitionEventArgs? capturedArgs = null;

        stateMachine.OnStateTransition += (sender, args) => capturedArgs = args;

        // Act
        stateMachine.FireTrigger(ParseJobTrigger.SkipGathering, "SingleUrl job");

        // Assert
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual(RunStatus.Waiting, capturedArgs.FromState);
        Assert.AreEqual(RunStatus.Running, capturedArgs.ToState);
        Assert.AreEqual(ParseJobTrigger.SkipGathering, capturedArgs.Trigger);
        Assert.AreEqual("SingleUrl job", capturedArgs.Context);
    }

    [TestMethod]
    public void FullJobLifecycle_SingleUrl_Waiting_Running_Finished_Succeeds()
    {
        // Arrange
        var stateMachine = new ParseJobStateMachine(RunStatus.Waiting);

        // Act & Assert
        Assert.AreEqual(RunStatus.Waiting, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.SkipGathering);
        Assert.AreEqual(RunStatus.Running, stateMachine.CurrentState);

        stateMachine.FireTrigger(ParseJobTrigger.Complete);
        Assert.AreEqual(RunStatus.Finished, stateMachine.CurrentState);
    }
}

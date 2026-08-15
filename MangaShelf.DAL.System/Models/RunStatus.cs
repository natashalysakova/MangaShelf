namespace MangaShelf.DAL.System.Models;

public enum RunStatus
{
    Waiting = 0,
    GatheringVolumes = 1,
    Running = 2,
    Finished = 3,
    Error = 4,
    Cancelled = 5
}


public static class RunStatusExtensions
{
    /// <summary>
    /// Determines whether the specified RunStatus is active (Waiting, Running, or GatheringVolumes).
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsActive(this RunStatus status)
    {
        return status == RunStatus.Waiting || status == RunStatus.Running || status == RunStatus.GatheringVolumes;
    }

    /// <summary>
    /// Determines whether the specified RunStatus is completed (Finished, Error, or Cancelled).
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsCompleted(this RunStatus status)
    {
        return status == RunStatus.Finished || status == RunStatus.Error || status == RunStatus.Cancelled;
    }

    public static bool IsError(this RunStatus status)
    {
        return status == RunStatus.Error;
    }

    public static bool IsCancelled(this RunStatus status)
    {
        return status == RunStatus.Cancelled;
    }

    public static bool IsWaiting(this RunStatus status)
    {
        return status == RunStatus.Waiting;
    }

    public static bool IsFinished(this RunStatus status)
    {
        return status == RunStatus.Finished;
    }

    /// <summary>
    /// Determines whether the specified RunStatus is running (Running or GatheringVolumes).
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsRunning(this RunStatus status)
    {
        return status == RunStatus.Running || status == RunStatus.GatheringVolumes;
    }
}
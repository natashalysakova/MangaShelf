namespace MangaShelf.DAL.System.Models;

public enum RunStatus
{
    Created = 0,
    Waiting = 1,
    GatheringVolumes = 2,
    Running = 3,
    Finished = 4,
    Error = 5,
    Cancelled = 6,
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

    /// <summary>
    /// Determines whether the specified RunStatus is running (Running or GatheringVolumes).
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsRunning(this RunStatus status)
    {
        return status == RunStatus.Running || status == RunStatus.GatheringVolumes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsIdle(this RunStatus status)
    {
        return status == RunStatus.Created || status == RunStatus.Waiting;
    }

    public static bool NotSuccessful(this RunStatus status)
    {
        return status == RunStatus.Error || status == RunStatus.Cancelled;
    }
}
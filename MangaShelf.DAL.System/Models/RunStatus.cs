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

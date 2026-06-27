namespace MangaShelf.DAL.System.Models;

public enum RunStatus
{
    Waiting,
    GatheringVolumes,
    Running,
    Finished,
    Error,
    Cancelled
}

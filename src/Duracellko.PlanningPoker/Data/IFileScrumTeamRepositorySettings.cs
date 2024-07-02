namespace Duracellko.PlanningPoker.Data;

/// <summary>
/// Settings for scrum team repository using file system.
/// </summary>
public interface IFileScrumTeamRepositorySettings
{
    /// <summary>
    /// Gets the folder storing scrum team files.
    /// </summary>
    /// <value>
    /// The storage folder.
    /// </value>
    string Folder { get; }
}

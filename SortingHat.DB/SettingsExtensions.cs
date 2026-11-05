using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public static class SettingsExtensions
    {
        extension(ISettings settings)
        {
            /// <summary>
            /// The project database in use: the one selected with switch-project, otherwise the configured default project.
            /// </summary>
            public string ActiveProject(DatabaseSettings databaseSettings)
                => settings[Constants.ProjectDatabaseKey] ?? databaseSettings.DefaultProject;
        }
    }
}

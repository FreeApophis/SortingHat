using SortingHat.API;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public static class SettingsExtensions
    {
        /// <summary>
        /// The project database in use: the one selected with switch-project, otherwise the configured default project.
        /// </summary>
        public static string ActiveProject(this ISettings settings, DatabaseSettings databaseSettings)
            => settings[Constants.ProjectDatabaseKey] ?? databaseSettings.DefaultProject;
    }
}

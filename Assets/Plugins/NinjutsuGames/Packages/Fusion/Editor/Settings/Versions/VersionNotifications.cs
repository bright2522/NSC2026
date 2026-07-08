using System;
using System.Globalization;
using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    internal static class VersionsNotifications
    {
        private static readonly CultureInfo CULTURE = CultureInfo.InvariantCulture;
        private const int CHECK_FREQUENCY = 6;
        
        private const string KEY_CHECK_DATE = "fusion:versions-check-date";
        private const string KEY_REMIND_UPDATES = "fusion:versions-remind-updates";
        private const string KEY_VERSION_SEEN = "fusion:versions-{0}-number";
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public static bool RemindUpdates
        {
            get => EditorPrefs.GetBool(KEY_REMIND_UPDATES, true);
            set => EditorPrefs.SetBool(KEY_REMIND_UPDATES, value);
        }

        // INITIALIZERS: --------------------------------------------------------------------------
        
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            SettingsWindow.InitRunners.Add(new InitRunner(
                SettingsWindow.INIT_PRIORITY_DEFAULT,
                CanInitializeRemindUpdates,
                InitializeRemindUpdates
            ));
        }
        
        private static bool CanInitializeRemindUpdates()
        {
            if (!RemindUpdates) return false;
            var minDate = DateTime.MinValue.ToString(CULTURE);
            
            var currentDate = DateTime.Now;
            var checkDate = DateTime.Parse(
                EditorPrefs.GetString(KEY_CHECK_DATE, minDate),
                CULTURE
            );

            var timeDifference = currentDate - checkDate;
            return timeDifference.TotalHours >= CHECK_FREQUENCY;
        }

        private static void InitializeRemindUpdates()
        {
            var currentDate = DateTime.Now;
            EditorPrefs.SetString(KEY_CHECK_DATE, currentDate.ToString(CULTURE));

            VersionsManager.EventDone -= OnFetchComplete;
            VersionsManager.EventDone += OnFetchComplete;

            VersionsManager.Initialize();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static void OnFetchComplete()
        {
            VersionsManager.EventDone -= OnFetchComplete;
            var showSettingsUpdates = false;

            foreach (var entry in VersionsManager.LatestEntries)
            {
                var installedVersion = VersionsManager.GetInstalledVersion(entry.Key);
                if (installedVersion.Empty) continue;

                if (installedVersion.IsOlderThan(entry.Value.Version))
                {
                    var keySavedVersion = string.Format(KEY_VERSION_SEEN, entry.Key);
                    var savedVersionString = EditorPrefs.GetString(keySavedVersion);
                    var savedVersion = new AssetVersion(savedVersionString);

                    if (entry.Value.Version.IsNewerThan(savedVersion))
                    {
                        showSettingsUpdates = true;
                        EditorPrefs.SetString(keySavedVersion, entry.Value.Version.ToString());
                    }
                }
            }

            if (showSettingsUpdates)
            {
                SettingsWindow.OpenWindow(FusionRepository.REPOSITORY_ID);
            }
        }
    }
}
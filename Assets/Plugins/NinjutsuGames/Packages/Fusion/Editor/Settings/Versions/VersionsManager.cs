using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace NinjutsuGames.FusionNetwork.Editor
{
    internal static class VersionsManager
    {
        private static readonly TextInfo TXT = CultureInfo.InvariantCulture.TextInfo;
        
        // CONSTANTS: -----------------------------------------------------------------------------
        
        private const string URI = "https://raw.githubusercontent.com/hjupter/documentation/main/game-creator-2/fusion-module/releases.json";

        private const string KEY_HASH = "fusion:versions-latest-hash";
        private const string KEY_LATEST = "fusion:versions-latest-data";

        private const string KEY_ASSET = "fusion:versions-{0}-data";
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        private static UnityWebRequest RequestLatest;

        // PROPERTIES: ----------------------------------------------------------------------------

        private static bool IsInitialized { get; set; }
        
        public static LatestData Latest { get; private set; }
        public static Dictionary<string, AssetEntry> LatestEntries { get; private set; }

        private static int FetchCount = 0;

        // EVENTS: --------------------------------------------------------------------------------

        public static event Action EventChange;
        public static event Action EventDone;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            
            Latest = new LatestData();
            LatestEntries = new Dictionary<string, AssetEntry>();

            if (EditorPrefs.HasKey(KEY_LATEST))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(KEY_LATEST), Latest);
                foreach (var entry in Latest.List)
                {
                    if (string.IsNullOrEmpty(entry.Id)) continue;

                    var entryKey = string.Format(KEY_ASSET, entry.Id);
                    if (!EditorPrefs.HasKey(entryKey)) continue;

                    var jsonEntry = new AssetEntry(State.Ready);
                    EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(entryKey), jsonEntry);
                    
                    LatestEntries.Add(entry.Id, jsonEntry);
                }
                
                Latest.State = State.Ready;
                EventDone?.Invoke();
            }
            
            FetchLatest();
        }

        public static AssetVersion GetInstalledVersion(string id)
        {
            var path = RuntimePaths.PACKAGES + TXT.ToTitleCase(id);
            if(id.Contains("fusion-"))
            {
                var title = TXT.ToTitleCase(id).Replace("-", "");
                path = $"{RuntimePaths.SUB_MODULES}{title}";
            }
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path + "/Editor/Version.txt");
            var version = asset?.text;
            return version != null ? new AssetVersion(version) : AssetVersion.None;
        }
        
        // FETCH METHODS: -------------------------------------------------------------------------
        
        private static void FetchLatest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            Latest.State = State.Loading;
            
            RequestLatest = UnityWebRequest.Get(URI);
            RequestLatest.SetRequestHeader("ContentType", "application/json");

            var operation = RequestLatest.SendWebRequest();
            operation.completed += OnLatestReceive;
        }
        
        private static void FetchAsset(string id, string uri)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                FetchCount += 1;
                return;
            }

            var request = UnityWebRequest.Get(uri);
            request.SetRequestHeader("ContentType", "application/json");
            var operation = request.SendWebRequest();
            
            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    // Debug.LogError(request.error);
                    LatestEntries[id] = null;

                    EventChange?.Invoke();

                    FetchCount += 1;
                    if (FetchCount >= LatestEntries.Count)
                    {
                        EventDone?.Invoke();
                    }

                    return;
                }

                var json = ExtractLatestReleaseAsJson(request.downloadHandler.text);
                var data = new AssetEntry();

                EditorJsonUtility.FromJsonOverwrite(json, data);
                var entryKey = string.Format(KEY_ASSET, id);
                
                EditorPrefs.SetString(entryKey, EditorJsonUtility.ToJson(data));
                LatestEntries[id] = data;
                
                EventChange?.Invoke();
            };
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static void OnLatestReceive(AsyncOperation asyncOperation)
        {
            if (RequestLatest.result != UnityWebRequest.Result.Success)
            {
                // Debug.LogWarning(RequestLatest.error);

                Latest.State = State.Error;
                LatestEntries.Clear();
                
                EventChange?.Invoke();
                return;
            }

            var json = RequestLatest.downloadHandler.text;
            var data = new LatestData(State.Ready);
            EditorJsonUtility.FromJsonOverwrite(json, data);

            var dataJson = EditorJsonUtility.ToJson(data, false);
            
            EditorPrefs.SetString(KEY_LATEST, dataJson);

            var currentHash = EditorPrefs.GetInt(KEY_HASH, 0);
            if (currentHash == json.GetHashCode()) return;
            
            EditorPrefs.SetInt(KEY_HASH, currentHash);

            Latest.State = State.Ready;
            LatestEntries.Clear();
            
            foreach (var entry in data.List)
            {
                LatestEntries.Add(entry.Id, new AssetEntry(State.Loading));
            }
            
            EventChange?.Invoke();
            FetchCount = 0;

            foreach (var entry in data.List)
            {
                FetchAsset(entry.Id, entry.Path);
            }
        }

        private static string ExtractLatestReleaseAsJson(string markdownContent)
        {
            var lines = markdownContent.Split('\n');

            var newList = new List<string>();
            var enhancedList = new List<string>();
            var changedList = new List<string>();
            var removedList = new List<string>();
            var fixedList = new List<string>();

            var version = string.Empty;
            var date = string.Empty;
            var inLatestSection = false;
            string currentCategory = null;

            // Regex that matches version lines with or without heading hashes, e.g.:
            // "## 1.3.8 (14th September 2025)" or "1.3.8 (14th September 2025)"
            var versionRegex = new Regex(@"^#*\s*(\d+\.\d+\.\d+)\s*\((\d+)(?:st|nd|rd|th)?\s+([A-Za-z]+)\s+(\d{4})\)");

            foreach (var raw in lines)
            {
                var t = raw.Trim();

                // Detect version header (latest section) even if it doesn't start with "##"
                var versionMatch = versionRegex.Match(t);
                if (versionMatch.Success)
                {
                    if (!string.IsNullOrEmpty(version)) break; // Next version reached

                    version = versionMatch.Groups[1].Value;
                    date = $"{versionMatch.Groups[2].Value} {versionMatch.Groups[3].Value} {versionMatch.Groups[4].Value}";
                    inLatestSection = true;
                    continue;
                }

                if (!inLatestSection) continue;

                // Normalize potential category headers like "New", "**New**", "### New:", etc.
                var header = t.TrimStart('#', ' ', '\t');
                var headerStripped = header.Trim('*', ' ', '\t');
                if (Regex.IsMatch(headerStripped, @"^(New|Enhanced|Changed|Removed|Fixed)\s*:?$", RegexOptions.IgnoreCase))
                {
                    currentCategory = headerStripped.Replace(":", string.Empty).Trim().ToLowerInvariant();
                    continue;
                }

                // Collect bullet items under the current category
                if ((t.StartsWith("- ") || t.StartsWith("* ")) && !string.IsNullOrEmpty(currentCategory))
                {
                    var item = t.Substring(2).Trim();
                    switch (currentCategory)
                    {
                        case "new": newList.Add(item); break;
                        case "enhanced": enhancedList.Add(item); break;
                        case "changed": changedList.Add(item); break;
                        case "removed": removedList.Add(item); break;
                        case "fixed": fixedList.Add(item); break;
                        default: newList.Add(item); break;
                    }
                }
            }

            var changes = new AssetChanges(newList.ToArray(), enhancedList.ToArray(), changedList.ToArray(), removedList.ToArray(), fixedList.ToArray());
            var changelogData = new AssetEntry(version, date, changes);
            return EditorJsonUtility.ToJson(changelogData, false);
        }
    }
}
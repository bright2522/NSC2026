using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    /// <summary>
    /// A profanity filter that loads banned words from a CSV file (or a TextAsset)
    /// and then checks (or censors) messages using a compiled regex that matches whole words only.
    /// </summary>
    [Serializable]
    public class ProfanityFilter
    {
        [SerializeField] private TextAsset csvAsset;
        [SerializeField] private string replacement = "***";
        
        public TextAsset CsvAsset
        {
            get => csvAsset;
            set => csvAsset = value;
        }
        
        // The set of banned words (ignoring case).
        private HashSet<string> _bannedWords;

        // The regex built from the banned words.
        private Regex _profanityRegex;

        // Track if the filter has been initialized to avoid redundant initialization
        private bool _initialized;
        
        // Cache the comma character for string splitting to avoid allocations
        private static readonly char[] CommaDelimiter = { ',' };
        
        // StringBuilder for pattern construction to avoid string concatenation allocations
        private readonly StringBuilder _patternBuilder = new(1024);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var settings = FusionRepository.Get.ProfanityFilter;
            settings._initialized = true;
            settings.LoadBannedWordsFromTextAsset(settings.csvAsset);
            settings.BuildRegex();
        }

        /// <summary>
        /// Loads banned words from a CSV file at the given file path.
        /// </summary>
        private void LoadBannedWordsFromFile(string csvFilePath)
        {
            if (_bannedWords == null)
                _bannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            else
                _bannedWords.Clear();
                
            try
            {
                var lines = File.ReadAllLines(csvFilePath);
                ProcessLines(lines);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading banned words from file: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads banned words from a TextAsset.
        /// </summary>
        private void LoadBannedWordsFromTextAsset(TextAsset csvAsset)
        {
            if (csvAsset == null)
            {
                Debug.LogError("CSV TextAsset is null.");
                return;
            }

            if (_bannedWords == null)
                _bannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            else
                _bannedWords.Clear();

            // Split the text into lines once to avoid multiple string operations
            var lines = csvAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ProcessLines(lines);
        }

        /// <summary>
        /// Process an array of lines to extract banned words
        /// </summary>
        private void ProcessLines(string[] lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Split the line by comma
                var parts = line.Split(CommaDelimiter, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    var word = parts[0].Trim();
                    if (!string.IsNullOrEmpty(word))
                        _bannedWords.Add(word);
                }
            }
        }

        /// <summary>
        /// Builds a compiled regular expression that matches any banned word as a whole word.
        /// </summary>
        private void BuildRegex()
        {
            // If no banned words were loaded, use a regex that matches nothing.
            if (_bannedWords == null || _bannedWords.Count == 0)
            {
                _profanityRegex = new Regex("$^", RegexOptions.Compiled);
                return;
            }

            // Clear the pattern builder
            _patternBuilder.Clear();
            _patternBuilder.Append(@"\b(");

            // Add each banned word to the pattern
            var isFirst = true;
            foreach (var word in _bannedWords)
            {
                if (!isFirst)
                    _patternBuilder.Append('|');
                
                // Escape the word for regex
                _patternBuilder.Append(Regex.Escape(word));
                isFirst = false;
            }
            
            _patternBuilder.Append(@")\b");
            
            // Create the regex with the compiled pattern
            _profanityRegex = new Regex(_patternBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Checks whether the specified message contains any banned (profanity) words.
        /// </summary>
        public bool ContainsProfanity(string message)
        {
            if (!_initialized || string.IsNullOrEmpty(message)) 
                return false;
                
            return _profanityRegex.IsMatch(message);
        }

        /// <summary>
        /// Returns a new string in which any banned words have been replaced with the given replacement.
        /// Default replacement is "***" (three asterisks).
        /// </summary>
        public string Censor(string message)
        {
            if (!_initialized || string.IsNullOrEmpty(message)) 
                return message;
                
            return _profanityRegex.Replace(message, replacement);
        }
    }
}
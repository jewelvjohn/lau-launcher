using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AppSearcher
{
    /// <summary>
    /// Searches through a list of AppInfo objects with versatile string matching.
    /// Features:
    /// - Case insensitive
    /// - Ignores special characters and spaces
    /// - Partial matching (contains)
    /// - Fuzzy matching tolerance
    /// </summary>
    public static List<AppInfo> SearchApps(List<AppInfo> appList, string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return appList;

        // Normalize the search query
        string normalizedQuery = NormalizeString(searchQuery);
        
        // Return apps sorted by relevance
        return appList
            .Select(app => new { App = app, Score = CalculateMatchScore(app.appName, normalizedQuery) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.App)
            .ToList();
    }

    /// <summary>
    /// Normalizes a string by removing special characters, extra spaces, and converting to lowercase
    /// </summary>
    private static string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Convert to lowercase
        string normalized = input.ToLower();
        
        // Remove special characters, keep only alphanumeric and spaces
        normalized = Regex.Replace(normalized, @"[^a-z0-9\s]", "");
        
        // Replace multiple spaces with single space
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        
        return normalized;
    }

    /// <summary>
    /// Calculates a match score between the app name and search query
    /// Higher score = better match
    /// </summary>
    private static int CalculateMatchScore(string appName, string normalizedQuery)
    {
        string normalizedAppName = NormalizeString(appName);
        
        // Exact match (highest priority)
        if (normalizedAppName == normalizedQuery)
            return 1000;
        
        // Starts with query (high priority)
        if (normalizedAppName.StartsWith(normalizedQuery))
            return 500;
        
        // Contains query (medium priority)
        if (normalizedAppName.Contains(normalizedQuery))
            return 250;
        
        // Check if all characters of query appear in order in the app name
        if (ContainsInOrder(normalizedAppName, normalizedQuery))
            return 100;
        
        // Word-by-word matching
        string[] queryWords = normalizedQuery.Split(' ');
        string[] appWords = normalizedAppName.Split(' ');
        
        int wordMatchScore = 0;
        foreach (string queryWord in queryWords)
        {
            if (string.IsNullOrEmpty(queryWord))
                continue;
                
            foreach (string appWord in appWords)
            {
                if (appWord.StartsWith(queryWord))
                    wordMatchScore += 50;
                else if (appWord.Contains(queryWord))
                    wordMatchScore += 25;
            }
        }
        
        if (wordMatchScore > 0)
            return wordMatchScore;
        
        // Fuzzy match - calculate similarity
        int similarity = CalculateSimilarity(normalizedAppName, normalizedQuery);
        if (similarity > 60) // 60% similarity threshold
            return similarity;
        
        return 0; // No match
    }

    /// <summary>
    /// Checks if all characters from query appear in order in the target string
    /// </summary>
    private static bool ContainsInOrder(string target, string query)
    {
        int queryIndex = 0;
        
        for (int i = 0; i < target.Length && queryIndex < query.Length; i++)
        {
            if (target[i] == query[queryIndex])
                queryIndex++;
        }
        
        return queryIndex == query.Length;
    }

    /// <summary>
    /// Calculates similarity percentage between two strings using Levenshtein distance
    /// </summary>
    private static int CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0;

        int maxLen = System.Math.Max(s1.Length, s2.Length);
        int distance = LevenshteinDistance(s1, s2);
        
        return (int)(((maxLen - distance) / (double)maxLen) * 100);
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        int[,] d = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            d[0, j] = j;

        for (int j = 1; j <= s2.Length; j++)
        {
            for (int i = 1; i <= s1.Length; i++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = System.Math.Min(
                    System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }
}
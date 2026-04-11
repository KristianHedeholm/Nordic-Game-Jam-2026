using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Generates riddles via OpenAI. Falls back to hardcoded riddles if no API key.
/// </summary>
public class RiddleGenerator : MonoBehaviour
{
    [Header("OpenAI Settings")]
    public string apiKey = ""; // Set at runtime or via GameManager
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    // Hardcoded fallback riddles (used when no API key is set)
    private static readonly Dictionary<string, string[]> FallbackRiddles = new Dictionary<string, string[]>
    {
        // Clothing
        { "Gown", new[] {
            "I sweep the floor without a broom, I fill a princess or a queen's room. I trail behind in silk or lace — what am I?",
            "I flow from shoulder down to ground, in royal halls I can be found."
        }},
        { "Tuxedo", new[] {
            "Black and white, sharp as night, I'm worn when gentlemen must look just right.",
            "I come with a bow and a jacket so fine, worn to impress at dinner or wine."
        }},
        { "Dress", new[] {
            "I hug the form and flare below, the ladies wear me for a show.",
            "One piece for warmth and elegance too — summer or winter, I'll do for you."
        }},
        { "Shirt", new[] {
            "I cover your chest and button up neat, collar and cuffs make my look complete.",
            "With sleeves and buttons, I start each day, wrapped around you in every way."
        }},
        { "Cape", new[] {
            "I flow behind like a hero's pride, no sleeves — just drama on every side.",
            "Heroes and villains both know my name, I drape from shoulders to play the game."
        }},

        // Colors
        { "Royal Blue", new[] {
            "I speak without a sound, I stretch both up and down, I hold the thousand skies to see — what color could I be?",
            "I am the color of the deep wide sea, the sky at noon belongs to me."
        }},
        { "Crimson", new[] {
            "I blush like roses, bold and bright, I am the color of passion's light.",
            "Deeper than pink, warmer than wine — I am the color that makes hearts pine."
        }},
        { "Gold", new[] {
            "The sun at noon and coins of kings, I gleam and glitter on royal things.",
            "I am the color of wealth and worth, the richest hue upon this earth."
        }},
        { "White", new[] {
            "Pure as snow and blank as page, I've been the canvas of every age.",
            "I contain all colors yet show none, I am the shade of morning sun."
        }},
        { "Emerald", new[] {
            "The forest wears me, jealousy too, I'm the deep rich green that feels brand new.",
            "I am the gem, I am the grass, I am the color time cannot pass."
        }},

        // Materials
        { "Silk", new[] {
            "I'm smooth as truth with a gentle shine, I come from a thread so you know I'm fine.",
            "Spun by worms, I feel like water, kings have sought me every quarter."
        }},
        { "Velvet", new[] {
            "Touch me softly, feel the pile, I've lined the thrones of kings in style.",
            "I am smooth but thick with sheen, the grandest fabric ever seen."
        }},
        { "Cotton", new[] {
            "I grow in fields under open sky, soft and breathable as days go by.",
            "Simple and humble, I clothe the masses, comfortable through all the classes."
        }},
        { "Leather", new[] {
            "I was once a skin, now tough and bold, I keep you warm against the cold.",
            "Tanned and treated, strong and sleek, warriors and riders always seek."
        }},
        { "Iron", new[] {
            "The blacksmith loves me, cold and hard, I guard the knight out in the yard.",
            "No warmth in me, no gentle drape — I am the armor that gives shape."
        }},
    };

    public static RiddleGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Get a riddle for the given item. Uses API if key is set, otherwise fallback.
    /// </summary>
    public void GetRiddle(string item, string category, Action<string> onComplete)
    {
        if (!string.IsNullOrEmpty(apiKey))
            StartCoroutine(FetchRiddleFromAPI(item, category, onComplete));
        else
            onComplete(GetFallbackRiddle(item));
    }

    private string GetFallbackRiddle(string item)
    {
        if (FallbackRiddles.TryGetValue(item, out var riddles))
            return riddles[UnityEngine.Random.Range(0, riddles.Length)];
        return $"I am the finest of its kind — can you guess what fills the King's mind?";
    }

    private IEnumerator FetchRiddleFromAPI(string item, string category, Action<string> onComplete)
    {
        string prompt = $"You are the King's herald. Write a short 2-line riddle (like a poem) " +
                        $"about a {category} called '{item}'. " +
                        $"Do NOT mention the word '{item}' directly. " +
                        $"Make it playful, slightly absurd, and fit for a royal court. " +
                        $"Return ONLY the riddle, nothing else.";

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 100,
            temperature = 0.9
        };

        string json = JsonUtility.ToJson(requestBody);
        // JsonUtility doesn't handle anonymous types well — use manual JSON
        json = $"{{\"model\":\"gpt-4o-mini\",\"messages\":[{{\"role\":\"user\",\"content\":\"{EscapeJson(prompt)}\"}}],\"max_tokens\":100,\"temperature\":0.9}}";

        using var request = new UnityWebRequest(ApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                onComplete(response.choices[0].message.content.Trim());
            }
            catch
            {
                onComplete(GetFallbackRiddle(item));
            }
        }
        else
        {
            Debug.LogWarning($"OpenAI request failed: {request.error}. Using fallback.");
            onComplete(GetFallbackRiddle(item));
        }
    }

    private string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");

    // Minimal response structs for JsonUtility
    [Serializable] private class OpenAIResponse { public Choice[] choices; }
    [Serializable] private class Choice { public Message message; }
    [Serializable] private class Message { public string content; }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Generates riddles via OpenRouter. Falls back to hardcoded riddles if no API key.
/// </summary>
public class RiddleGenerator : MonoBehaviour
{
    [Header("OpenRouter Settings")]
    public string apiKey = "";
    public string model = "openai/gpt-4o-mini";
    private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";

    private static readonly Dictionary<string, string[]> FallbackRiddles = new Dictionary<string, string[]>
    {
        { "Gown",     new[] { "I sweep the floor without a broom,\nI fill a princess or a queen's room.", "I trail behind in silk or lace,\nFor royal balls I set the pace." } },
        { "Tuxedo",   new[] { "Black and white, sharp as night,\nI'm worn when gentlemen must look just right.", "I come with a bow and a jacket so fine,\nWorn to impress at dinner and wine." } },
        { "Dress",    new[] { "I hug the form and flare below,\nThe ladies wear me for a show.", "One piece for warmth and elegance too,\nSummer or winter, I'll do for you." } },
        { "Shirt",    new[] { "I cover your chest and button up neat,\nCollar and cuffs make my look complete.", "With sleeves and buttons I start each day,\nWrapped around you in every way." } },
        { "Cape",     new[] { "I flow behind like a hero's pride,\nNo sleeves — just drama on every side.", "Heroes and villains both know my name,\nI drape from shoulders to play the game." } },
        { "Royal Blue", new[] { "I speak without a sound, I stretch up and down,\nI hold a thousand skies — what color am I now?", "I am the color of the deep wide sea,\nThe sky at noon belongs to me." } },
        { "Crimson",  new[] { "I blush like roses, bold and bright,\nI am the color of passion's light.", "Deeper than pink, warmer than wine,\nI am the color that makes hearts pine." } },
        { "Gold",     new[] { "The sun at noon and coins of kings,\nI gleam and glitter on royal things.", "I am the color of wealth and worth,\nThe richest hue upon this earth." } },
        { "White",    new[] { "Pure as snow and blank as page,\nI've been the canvas of every age.", "I contain all colors yet show none,\nI am the shade of morning sun." } },
        { "Emerald",  new[] { "The forest wears me, jealousy too,\nI'm the deep rich green that feels brand new.", "I am the gem, I am the grass,\nI am the color time cannot pass." } },
        { "Silk",     new[] { "Smooth as truth with a gentle shine,\nI come from a thread so you know I'm fine.", "Spun by worms, I feel like water,\nKings have sought me every quarter." } },
        { "Velvet",   new[] { "Touch me softly, feel the pile,\nI've lined the thrones of kings in style.", "I am smooth but thick with sheen,\nThe grandest fabric ever seen." } },
        { "Cotton",   new[] { "I grow in fields under open sky,\nSoft and breathable as days go by.", "Simple and humble, I clothe the masses,\nComfortable through all the classes." } },
        { "Leather",  new[] { "I was once a skin, now tough and bold,\nI keep you warm against the cold.", "Tanned and treated, strong and sleek,\nWarriors and riders always seek." } },
        { "Iron",     new[] { "The blacksmith loves me, cold and hard,\nI guard the knight out in the yard.", "No warmth in me, no gentle drape,\nI am the armor that gives shape." } },
    };

    public static RiddleGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void GetRiddle(string item, string category, Action<string> onComplete)
    {
        if (!string.IsNullOrEmpty(apiKey))
            StartCoroutine(FetchRiddle(item, category, onComplete));
        else
            onComplete(GetFallback(item));
    }

    private string GetFallback(string item)
    {
        if (FallbackRiddles.TryGetValue(item, out var riddles))
            return riddles[UnityEngine.Random.Range(0, riddles.Length)];
        return "I am the finest of its kind,\nCan you guess what fills the King's mind?";
    }

    private IEnumerator FetchRiddle(string item, string category, Action<string> onComplete)
    {
        string prompt =
            $"You are a royal herald in a medieval court. Write a SHORT 2-line riddle (rhyming couplet) " +
            $"that hints at the {category.ToLower()} '{item}' without saying the word directly. " +
            $"Make it playful and slightly absurd. Return ONLY the two lines, nothing else.";

        string escaped = prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        string json = $"{{\"model\":\"{model}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{escaped}\"}}],\"max_tokens\":80,\"temperature\":1.0}}";

        using var req = new UnityWebRequest(ApiUrl, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        req.SetRequestHeader("HTTP-Referer", "https://nordic-game-jam-2026");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var r = JsonUtility.FromJson<OAIResponse>(req.downloadHandler.text);
                onComplete(r.choices[0].message.content.Trim());
                yield break;
            }
            catch (Exception e) { Debug.LogWarning("Parse error: " + e.Message); }
        }
        else Debug.LogWarning("OpenRouter error: " + req.error);

        onComplete(GetFallback(item));
    }

    [Serializable] class OAIResponse { public Choice[] choices; }
    [Serializable] class Choice { public Msg message; }
    [Serializable] class Msg { public string content; }
}

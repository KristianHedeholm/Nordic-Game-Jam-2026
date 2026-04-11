using System;
using System.Collections;
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

    private static readonly System.Collections.Generic.Dictionary<string, string[]> FallbackRiddles =
        new System.Collections.Generic.Dictionary<string, string[]>
    {
        // ── CLOTHING ──────────────────────────────────────────────────────
        { "Pants", new[] {
            "Two tubes for two legs, a royal disgrace,\nThe King insists they frame his kingly grace.",
            "I cover the lower half, or so they say,\nThough the King wears me in his own special way."
        }},
        { "Mankini", new[] {
            "A single strip of pride from shoulder to sea,\nThe boldest garment a monarch can be.",
            "Daring, skimpy, barely there at best,\nThe King calls this his very finest dressed."
        }},
        { "Armor", new[] {
            "I guard the knight in steel and might,\nA clanking suit for battle's fight.",
            "Iron plates from head to toe,\nThe King believes this steals the show."
        }},
        { "Maid's Dress", new[] {
            "Ruffles, apron, a bow tied neat,\nThe King thinks this makes his look complete.",
            "I am worn while scrubbing floors,\nThe King calls me fit for royal chores."
        }},
        { "Cape", new[] {
            "I billow back with heroic flair,\nThe King insists I flow beyond compare.",
            "No sleeves — just drama trailing behind,\nThe grandest garment you'll ever find."
        }},
        { "Crocs", new[] {
            "Holes in rubber, clogs of fame,\nThe King believes no shoe beats my name.",
            "Spongy comfort on royal feet,\nWith little charms to make the look complete."
        }},
        { "Bath Robe", new[] {
            "I'm worn at morning, damp from the shower,\nThe King declares me the fashion of the hour.",
            "Fluffy, cosy, tied at the waist,\nThe King wears me with impeccable taste."
        }},
        { "Fingerless Gloves", new[] {
            "I cover the palm but leave fingers free,\nThe King says I ooze sophistication, you see.",
            "Half a glove, a rebel's choice,\nThe King wears me and raises his voice."
        }},

        // ── COLORS ────────────────────────────────────────────────────────
        { "Blue", new[] {
            "I am the sky, the sea, the deep,\nThe color of the dreams you keep.",
            "I speak without a sound, I stretch both up and down,\nI hold a thousand skies — what color am I now?"
        }},
        { "Red", new[] {
            "I blush like roses, bold and bright,\nI am the color of passion's light.",
            "Danger, love, and royal rage,\nI am the boldest on any stage."
        }},
        { "Yellow", new[] {
            "Sunny side up, bright as a yolk,\nI am the happiest color that folk invoke.",
            "Cheerful and loud, I shine like the sun,\nThe King says in yellow, I am the one."
        }},
        { "Orange", new[] {
            "Half of red, half of yellow's cheer,\nI am the color of autumn, crisp and clear.",
            "A fruit bears my name, a sunset too,\nThe King picked me — what a bold debut."
        }},
        { "Purple", new[] {
            "Kings and queens have worn me with pride,\nA noble hue from history's wide side.",
            "Between red and blue I boldly reign,\nRoyal and rare, I stake my claim."
        }},
        { "Gold", new[] {
            "The sun at noon and coins of kings,\nI gleam and glitter on royal things.",
            "I am the color of wealth and worth,\nThe richest hue upon this earth."
        }},
        { "White", new[] {
            "Pure as snow and blank as page,\nI have been the canvas of every age.",
            "I contain all colors yet show none,\nI am the shade of morning sun."
        }},
        { "Green", new[] {
            "The forest wears me, envy too,\nI am the shade of grass and dew.",
            "I spring from earth each April morn,\nFresh and lively, newly born."
        }},
        { "Pink", new[] {
            "Softer than red, bolder than white,\nI am the color of flamingo flight.",
            "Bubblegum, roses, a blush on the cheek,\nThe King thinks I'm the fashion they seek."
        }},
        { "Silver", new[] {
            "I am the moon's light, cool and bright,\nA metallic shimmer in the night.",
            "Not quite gold but gleaming still,\nI reflect your face against your will."
        }},
        { "Brown", new[] {
            "Earth and mud and bark of trees,\nI am the color carried on the breeze.",
            "Humble and sturdy, rich as soil,\nThe King chose me despite the toil."
        }},
        { "Black", new[] {
            "I absorb all light and give back none,\nMystery and elegance, all in one.",
            "Darkest night and sharpest suit,\nThe King says black is truly absolute."
        }},

        // ── MATERIALS ─────────────────────────────────────────────────────
        // Note: Gold color key is "Gold", Gold material key is "Gold (material)"
        { "Gold (material)", new[] {
            "Mined from earth by sweating hands,\nI glitter bright across all lands.",
            "Coins and crowns are made from me,\nThe heaviest fabric you will see."
        }},
        { "Iron", new[] {
            "The blacksmith loves me, cold and hard,\nI guard the knight out in the yard.",
            "No warmth in me, no gentle drape,\nI am the armor that gives shape."
        }},
        { "Silk", new[] {
            "Smooth as truth with a gentle shine,\nI come from a thread so you know I'm fine.",
            "Spun by worms, I feel like water,\nKings have sought me every quarter."
        }},
        { "Cotton", new[] {
            "I grow in fields under open sky,\nSoft and breathable as days go by.",
            "Simple and humble, I clothe the masses,\nComfortable through all the classes."
        }},
        { "Fur", new[] {
            "Once worn by beasts across the plain,\nNow the King wears me in sun and rain.",
            "Fluffy, warm, and wild at heart,\nThe King thinks I am true fashion art."
        }},
        { "Leather", new[] {
            "Tanned and treated, strong and sleek,\nWarriors and riders always seek.",
            "I was once a skin, now tough and bold,\nI keep you warm against the cold."
        }},
        { "Feathers", new[] {
            "Once I helped a bird take flight,\nNow I adorn the King each night.",
            "Plucked from wings, I tickle and float,\nThe King drapes me as his royal coat."
        }},
        { "Polyester", new[] {
            "Born in a lab, not field or farm,\nI keep you dry but lack all charm.",
            "Synthetic and proud, I never wrinkle,\nThe King says in me his eyes will twinkle."
        }},
        { "Vegan Leather", new[] {
            "I look like leather but harmed no beast,\nThe King calls me his ethical feast.",
            "Plant-based pride from sole to seam,\nThe eco-friendly royal dream."
        }},
        { "Faux Fur", new[] {
            "I mimic the beast but come from a factory,\nThe King finds my fluffiness satisfactory.",
            "Fake and fabulous, fluffy and fun,\nThe King says real fur? No, this one's done."
        }},
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
            onComplete(GetFallback(item, category));
    }

    private string GetFallback(string item, string category = "")
    {
        // Handle Gold disambiguation: same word appears in Colors and Materials
        string key = (item == "Gold" && category == "Material") ? "Gold (material)" : item;
        if (FallbackRiddles.TryGetValue(key, out var riddles))
            return riddles[UnityEngine.Random.Range(0, riddles.Length)];
        return "I am the finest of its kind,\nCan you guess what fills the King's mind?";
    }

    private IEnumerator FetchRiddle(string item, string category, Action<string> onComplete)
    {
        string prompt =
            $"You are a royal herald in a medieval court. Write a SHORT 2-line riddle (rhyming couplet) " +
            $"that hints at the {category.ToLower()} '{item}' without saying the word directly. " +
            $"Make it playful, slightly absurd, and fit for a silly self-obsessed king. " +
            $"Return ONLY the two lines, nothing else.";

        string escaped = prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        string json = $"{{\"model\":\"{model}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{escaped}\"}}],\"max_tokens\":80,\"temperature\":1.0}}";

        using var req = new UnityWebRequest(ApiUrl, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        req.SetRequestHeader("HTTP-Referer", "https://fashion-royal.itch.io");

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

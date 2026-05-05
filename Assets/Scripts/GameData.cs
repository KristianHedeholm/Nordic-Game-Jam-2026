using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All predefined game data — clothing, colors, materials.
/// Each round shows 5 random options (always including the correct answer).
/// </summary>
public static class GameData
{
    private static readonly List<string> Clothing = new List<string>
    {
        "Pants", "Mankini", "Armor", "Maid's Dress", "Cape",
        "Crocs", "Bath Robe", "Fingerless Gloves"
    };

    private static readonly List<string> Colors = new List<string>
    {
        "Blue", "Red", "Yellow", "Orange", "Purple",
        "Gold", "White", "Green", "Pink", "Silver", "Brown", "Black"
    };

    private static readonly List<string> Materials = new List<string>
    {
        "Gold", "Iron", "Silk", "Cotton", "Fur",
        "Leather", "Feathers", "Polyester", "Vegan Leather", "Faux Fur"
    };
    
    private const int NumberOfOptionsForRiddle = 4;
    
    public static string GetRandomAnswer(RiddleKind riddleKind)
    {
	    return riddleKind switch
	    {
		    RiddleKind.Garment => Clothing[Random.Range(0, Clothing.Count)],
		    RiddleKind.Color => Colors[Random.Range(0, Colors.Count)],
		    RiddleKind.Material => Materials[Random.Range(0, Materials.Count)],
		    _ => string.Empty,
	    };
    }

    public static List<string> GetRiddleAnswerOptions(RiddleKind riddleKind, RiddleData state)
    {
	    var allAnswers = GetAllAnswersForRiddle(riddleKind);
	    var correctAnswer = state.GetCorrectAnswer(riddleKind);
	    return GetOptions(allAnswers, correctAnswer, NumberOfOptionsForRiddle);
    }

    private static List<string> GetAllAnswersForRiddle(RiddleKind riddleKind)
    {
	    return riddleKind switch
	    {
		    RiddleKind.Garment => Clothing,
		    RiddleKind.Color => Colors,
		    RiddleKind.Material => Materials,
		    _ => new List<string>(),
	    };
    }

    /// <summary>
    /// Returns 5 options from the list, always including the correct answer, shuffled.
    /// </summary>
    private static List<string> GetOptions(List<string> pool, string correctAnswer, int count)
    {
        // Build a copy without the correct answer
        var others = new List<string>(pool);
        others.Remove(correctAnswer);

        // Shuffle
        for (int i = others.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = others[i]; others[i] = others[j]; others[j] = tmp;
        }

        // Take count-1 random others + correct answer
        var result = others.GetRange(0, Mathf.Min(count - 1, others.Count));
        result.Add(correctAnswer);

        // Shuffle result
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = result[i]; result[i] = result[j]; result[j] = tmp;
        }

        return result;
    }
}

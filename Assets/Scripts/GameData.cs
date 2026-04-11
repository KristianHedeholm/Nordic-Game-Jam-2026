using System.Collections.Generic;

/// <summary>
/// All predefined game data — clothing, colors, materials.
/// </summary>
public static class GameData
{
    public static readonly List<string> Clothing = new List<string>
    {
        "Gown", "Tuxedo", "Dress", "Shirt", "Cape"
    };

    public static readonly List<string> Colors = new List<string>
    {
        "Royal Blue", "Crimson", "Gold", "White", "Emerald"
    };

    public static readonly List<string> Materials = new List<string>
    {
        "Silk", "Velvet", "Cotton", "Leather", "Iron"
    };

    // The King is always naked — this is the joke.
    // The "correct" outfit is randomly picked each round but never actually worn.
    public static string RandomClothing() => Clothing[UnityEngine.Random.Range(0, Clothing.Count)];
    public static string RandomColor()    => Colors[UnityEngine.Random.Range(0, Colors.Count)];
    public static string RandomMaterial() => Materials[UnityEngine.Random.Range(0, Materials.Count)];
}

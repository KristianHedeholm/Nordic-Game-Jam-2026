/// <summary>
/// Tracks what phase of the game we're in.
/// </summary>
public enum GamePhase
{
    Intro,          // King introduces himself
    GuessClothing,  // Player guesses the clothing type
    GuessColor,     // Player guesses the color
    GuessMaterial,  // Player guesses the material
    Reveal,         // King reveals himself (naked, obviously)
    FinalJudgment,  // Player must respond to the King
    WinScreen,      // Player flattered the King
    DeathScreen     // Player told the truth — off with their head
}

/// <summary>
/// Central game state. One instance lives on GameManager.
/// </summary>
public class GameState
{
    public GamePhase Phase = GamePhase.Intro;

    // What the King THINKS he's wearing (randomly chosen)
    public string TargetClothing;
    public string TargetColor;
    public string TargetMaterial;

    // What the player guessed
    public string GuessedClothing;
    public string GuessedColor;
    public string GuessedMaterial;

    // Current riddle text (set by RiddleGenerator)
    public string CurrentRiddle;

    public void NewGame()
    {
        Phase = GamePhase.Intro;
        TargetClothing = GameData.RandomClothing();
        TargetColor    = GameData.RandomColor();
        TargetMaterial = GameData.RandomMaterial();
        GuessedClothing = null;
        GuessedColor    = null;
        GuessedMaterial = null;
        CurrentRiddle   = null;
    }
}

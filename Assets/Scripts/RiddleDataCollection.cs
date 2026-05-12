using System;
using System.Collections.Generic;

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
    DeathScreen     // Player told the truth — off with their head
}

public struct RiddleData
{
	public AnswersCollection Answers;
	public string GuessedAnswer;
	public string Riddle;

	public bool IsCorrect()
	{
		return Answers.CorrectAnswer == GuessedAnswer;
	}
}

public class RiddleDataCollection
{
    private readonly Dictionary<RiddleKind, RiddleData> _riddleData = new();

    public void CreateNewRiddleAnswers()
    {
        var riddleKinds = Enum.GetValues(typeof(RiddleKind)) as  RiddleKind[];
        foreach (var riddleKind in riddleKinds)
        {
	        var riddleAnswer = new RiddleData
	        {
		        Answers = GameData.GetAnswerCollection(riddleKind),
		        GuessedAnswer = string.Empty
	        };
	        _riddleData[riddleKind] = riddleAnswer;
        }
    }

    public string GetCorrectAnswer(RiddleKind riddleKind)
    {
	    return _riddleData[riddleKind].Answers.CorrectAnswer;
    }

    public string[] GetAvailableAnswers(RiddleKind riddleKind)
    {
	    return _riddleData[riddleKind].Answers.AvailableAnswers;
    }

    public void SetGuessedAnswer(RiddleKind riddleKind, string guess)
    {
	    var riddleData = _riddleData[riddleKind];
	    riddleData.GuessedAnswer = guess;
	    _riddleData[riddleKind] = riddleData;
    }

    public void SetRiddle(RiddleKind riddleKind, string riddle)
    {
	    var riddleData = _riddleData[riddleKind];
	    riddleData.Riddle = riddle;
	    _riddleData[riddleKind] = riddleData;
    }

    public string GetRiddle(RiddleKind riddleKind)
    {
	    return _riddleData[riddleKind].Riddle;
    }
    
    public int GetNumberOfCorrectAnswers()
    {
	    var count = 0;
	    foreach (var answer in _riddleData)
	    {
		    if (answer.Value.IsCorrect())
		    {
			    count++;
		    }
	    }
	    return count;
    }
    
    public bool IsAnswerCorrect(RiddleKind riddleKind)
    {
	    return _riddleData[riddleKind].IsCorrect();
    }

    public bool AreAllAnswersCorrect()
    {
	    foreach (var answer in _riddleData)
	    {
		    if (!answer.Value.IsCorrect())
		    {
			    return false;
		    }
	    }
	    return true;
    }
}

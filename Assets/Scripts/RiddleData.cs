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

public struct RiddleAnswer
{
	public AnswersCollection Answers;
	public string GuessedAnswer;

	public bool IsCorrect()
	{
		return Answers.CorrectAnswer == GuessedAnswer;
	}
}

public class RiddleData
{
    private readonly Dictionary<RiddleKind, RiddleAnswer> _riddleAnswers = new();

    public void CreateNewRiddleAnswers()
    {
        var riddleKinds = Enum.GetValues(typeof(RiddleKind)) as  RiddleKind[];
        foreach (var riddleKind in riddleKinds)
        {
	        var riddleAnswer = new RiddleAnswer
	        {
		        Answers = GameData.GetAnswerCollection(riddleKind),
		        GuessedAnswer = string.Empty
	        };
	        _riddleAnswers[riddleKind] = riddleAnswer;
        }
    }

    public string GetCorrectAnswer(RiddleKind riddleKind)
    {
	    return _riddleAnswers[riddleKind].Answers.CorrectAnswer;
    }

    public string[] GetAvailableAnswers(RiddleKind riddleKind)
    {
	    return _riddleAnswers[riddleKind].Answers.AvailableAnswers;
    }

    public void SetGuessedAnswer(RiddleKind riddleKind, string guess)
    {
	    var answer = _riddleAnswers[riddleKind];
	    answer.GuessedAnswer = guess;
	    _riddleAnswers[riddleKind] = answer;
    }
    
    public int GetNumberOfCorrectAnswers()
    {
	    var count = 0;
	    foreach (var answer in _riddleAnswers)
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
	    return _riddleAnswers[riddleKind].IsCorrect();
    }

    public bool AreAllAnswersCorrect()
    {
	    foreach (var answer in _riddleAnswers)
	    {
		    if (!answer.Value.IsCorrect())
		    {
			    return false;
		    }
	    }
	    return true;
    }
}

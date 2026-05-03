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
	public string CorrectAnswer;
	public string GuessedAnswer;

	public bool IsCorrect()
	{
		return CorrectAnswer == GuessedAnswer;
	}
}

public class RiddleData
{
    private Dictionary<RiddleKind, RiddleAnswer> _answers = new();

    public void CreateNewRiddleAnswers()
    {
        var riddleKinds = Enum.GetValues(typeof(RiddleKind)) as  RiddleKind[];
        foreach (var riddleKind in riddleKinds)
        {
	        var newAnswer = new RiddleAnswer();
	        newAnswer.CorrectAnswer = GameData.GetRandomAnswer(riddleKind);
	        newAnswer.GuessedAnswer = string.Empty;
	        _answers[riddleKind] = newAnswer;
        }
    }

    public string GetCorrectAnswer(RiddleKind riddleKind)
    {
	    return _answers[riddleKind].CorrectAnswer;
    }

    public void SetGuessedAnswer(RiddleKind riddleKind, string guess)
    {
	    var answer = _answers[riddleKind];
	    answer.GuessedAnswer = guess;
	    _answers[riddleKind] = answer;
    }
    
    public int GetNumberOfCorrectAnswers()
    {
	    var count = 0;
	    foreach (var answer in _answers)
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
	    return _answers[riddleKind].IsCorrect();
    }

    public bool AreAllAnswersCorrect()
    {
	    foreach (var answer in _answers)
	    {
		    if (!answer.Value.IsCorrect())
		    {
			    return false;
		    }
	    }
	    return true;
    }
}

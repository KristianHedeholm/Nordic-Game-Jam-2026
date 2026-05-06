using System.Collections.Generic;
using RawPowerLabs.DynamicAI;
using UnityEngine;

public struct AnswersCollection
{
	public string[] AvailableAnswers;
	public string CorrectAnswer;
}

public static class GameData
{
    private const int NumberOfOptionsForRiddle = 4;
    
    public static AnswersCollection GetAnswerCollection(RiddleKind riddleKind, int numberOfOptions = NumberOfOptionsForRiddle)
    {
	    var enumType = GetInputCategorical(riddleKind);
	    var allAnswers = new List<string>();
	    var enumValues = CategoricalInputCollection.AllCollections[enumType];
	    foreach (var enumValue in enumValues)
	    {
		    var name = enumValue.Value;
		    allAnswers.Add(name);
	    }
	    
	    // Shuffle all answers.
	    for (int i = allAnswers.Count - 1; i > 0; i--)
	    {
		    int j = Random.Range(0, i + 1);
		    (allAnswers[i], allAnswers[j]) = (allAnswers[j], allAnswers[i]);
	    }
	    
	    // populate with number of selected answers.
	    var selectedAnswers = new string[numberOfOptions];
	    for (int i = 0; i < numberOfOptions; i++)
	    {
		    selectedAnswers[i] = allAnswers[i];
	    }
	    
	    // select correct answer at random.
	    var randomIndex = Random.Range(0, numberOfOptions);
	    var correctAnswer = selectedAnswers[randomIndex];

	    var answerCollection = new AnswersCollection()
	    {
		    AvailableAnswers = selectedAnswers,
		    CorrectAnswer = correctAnswer,
	    };

	    return answerCollection;
    }

    private static System.Type GetInputCategorical(RiddleKind riddleKind)
    {
	    return riddleKind switch
	    {
		    RiddleKind.Garment => typeof(GarmentAnswer),
		    RiddleKind.Color => typeof(ColorAnswer),
		    RiddleKind.Material => typeof(MaterialAnswer),
		    _ => null,
	    };
    }
}

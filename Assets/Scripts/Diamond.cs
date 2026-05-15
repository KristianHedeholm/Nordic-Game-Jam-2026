using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RawPowerLabs.DynamicAI;
using RawPowerLabs.DynamicAI.Utility;
using Color = RawPowerLabs.DynamicAI.Color;
using Material  = RawPowerLabs.DynamicAI.Material;
using Type = RawPowerLabs.DynamicAI.Type;

public class Diamond : MonoBehaviour
{
	[HideInInspector]
    [SerializeField]
    private string _diamondName;
    
    private TextModule  _textModule;
    
    public void Init()
    {
	    var diamondPath = DiamondUtility.GetPathFromDiamondName(_diamondName);
	    var context = new RawPowerLabs.DynamicAI.Context();
	    var parameters = TextModuleParameters.GetDefault();
	    _textModule = context.CreateTextModule(parameters, diamondPath.modelPath, diamondPath.templatePath);
	    if (_textModule == null)
	    {
		    UnityEngine.Debug.LogError("Something went wrong with the Diamond");
	    }
    }
    
    public async Task GenerateRiddles(RiddleDataCollection riddleDataCollection)
    {
	    var riddleKinds = Enum.GetValues(typeof(RiddleKind)) as  RiddleKind[];
	    foreach (var riddleKind in riddleKinds)
	    {
		    var answer = riddleDataCollection.GetCorrectAnswer(riddleKind);
		    UnityEngine.Debug.Log($"RiddleKind: {riddleKind.ToString()} Answer: {answer}");
	    }
	    
	    var riddles = new Dictionary<RiddleKind, string>();
	    try
	    {
		    riddles = await InvokeReplyAsync(riddleDataCollection);
	    }
	    catch (Exception e)
	    {
		    UnityEngine.Debug.LogException(e);
	    }

	    foreach (var riddle in riddles)
	    {
		    UnityEngine.Debug.Log(riddle.Key + " " + riddle.Value);
		    riddleDataCollection.SetRiddle(riddle.Key, riddle.Value);
	    }
    }
    
	private async Task<Dictionary<RiddleKind, string>> InvokeReplyAsync(RiddleDataCollection riddleDataCollection)
	{
		return await Task.Run(() => InvokeReply(riddleDataCollection));
	}
	
	private Dictionary<RiddleKind, string> InvokeReply(RiddleDataCollection riddleDataCollection)
	{
		if (_textModule == null)
		{
			UnityEngine.Debug.LogError("_textModule is null");
			return null;
		}
		
		using var textModuleInput = _textModule.CreateInput();
		
		var typeAnswer = riddleDataCollection.GetCorrectAnswer(RiddleKind.Garment);
		var typeCategory = CategoricalInputCollection.TypeNames[typeof(Type)];
		textModuleInput.Set(typeCategory, typeAnswer);
		
		var colorAnswer = riddleDataCollection.GetCorrectAnswer(RiddleKind.Color);
		var colorCategory = CategoricalInputCollection.TypeNames[typeof(Color)];
		textModuleInput.Set(colorCategory, colorAnswer);
		
		var materialAnswer = riddleDataCollection.GetCorrectAnswer(RiddleKind.Material);
		var materialCategory = CategoricalInputCollection.TypeNames[typeof(Material)];
		textModuleInput.Set(materialCategory, materialAnswer);
		
		var invokeParameters = TextModuleInvokeParameters.GetDefault();
		invokeParameters.PredictCount = 4096;

		var random = new System.Random();
		invokeParameters.Seed = (uint) random.Next(0, int.MaxValue);
		using var textResult = _textModule.Invoke(invokeParameters, textModuleInput);
		
		var riddles = new Dictionary<RiddleKind, string>();
		foreach (var outputValues in CategoricalOutputCollection.StringOutputValues)
		{
			var result = textResult.GetString(outputValues.Value);
			var riddleKey = ConvertFromCategoricalOutput(outputValues.Key);
			riddles.Add(riddleKey, result);
		}
	    
		return riddles;
	}

	private RiddleKind ConvertFromCategoricalOutput(CategoricalOutput categoricalOutput)
	{
		return categoricalOutput switch
		{
			CategoricalOutput.TypeRiddle =>  RiddleKind.Garment,
			CategoricalOutput.ColorRiddle => RiddleKind.Color,
			CategoricalOutput.MaterialRiddle => RiddleKind.Material,
			_ => RiddleKind.Garment,
		};
	}
	
}

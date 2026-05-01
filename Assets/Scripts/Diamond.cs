using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RawPowerLabs.DynamicAI;
using RawPowerLabs.DynamicAI.Utility;
using Color = RawPowerLabs.DynamicAI.Color;
using Material  = RawPowerLabs.DynamicAI.Material;

public class Diamond : MonoBehaviour
{
	public Dictionary<CategoricalOutput, string>  Riddles => _riddles;
	
	[HideInInspector]
    [SerializeField]
    private string _diamondName;
    
    private TextModule  _textModule;
    private Dictionary<CategoricalOutput, string> _riddles = new Dictionary<CategoricalOutput, string>();
    
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
    
    public async void PrintReplies(string type, string color, string material)
    {
	    UnityEngine.Debug.Log($"Answers {type}, {color}, {material}");
	    try
	    {
		    _riddles = await InvokeReplyAsync(type, color, material);
	    }
	    catch (Exception e)
	    {
		    UnityEngine.Debug.LogException(e);
	    }

	    foreach (var reply in _riddles)
	    {
		    UnityEngine.Debug.Log($"Key: {reply.Key}, Value: {reply.Value}");
	    }
    }
    
    private async Task<Dictionary<CategoricalOutput, string>> InvokeReplyAsync(string type, string color, string material)
	{
		return await Task.Run(() => InvokeReply(type, color, material));
	}
	
	private Dictionary<CategoricalOutput, string> InvokeReply(string type, string color, string material)
	{
		if (_textModule == null)
		{
			UnityEngine.Debug.LogError("_textModule is null");
			return null;
		}
		
		using var textModuleInput = _textModule.CreateInput();

		textModuleInput.Set("Type", type);
		
		var colorCategory = CategoricalInputCollection.TypeNames[typeof(Color)];
		textModuleInput.Set(colorCategory, color);
		
		var materialCategory = CategoricalInputCollection.TypeNames[typeof(Material)];
		textModuleInput.Set(materialCategory, material);
		
		
		var invokeParameters = TextModuleInvokeParameters.GetDefault();
		invokeParameters.PredictCount = 4048;

		var random = new System.Random();
		invokeParameters.Seed = (uint) random.Next(0, int.MaxValue);
		using var textResult = _textModule.Invoke(invokeParameters, textModuleInput);
		
		var replies = new Dictionary<CategoricalOutput, string>();
		foreach (var outputValues in CategoricalOutputCollection.StringOutputValues)
		{
			var result = textResult.GetString(outputValues.Value);
			replies.Add(outputValues.Key, result);
		}
	    
		return replies;
	}
}

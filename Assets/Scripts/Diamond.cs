using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RawPowerLabs.DynamicAI;
using RawPowerLabs.DynamicAI.Utility;
using Type = RawPowerLabs.DynamicAI.Type;
using Color = RawPowerLabs.DynamicAI.Color;
using Material  = RawPowerLabs.DynamicAI.Material;

public class Diamond : MonoBehaviour
{
	[HideInInspector]
    [SerializeField]
    private string _diamondName;
    
    private TextModule  _textModule;

    private void Awake()
    {
	    var diamondPath = DiamondUtility.GetPathFromDiamondName(_diamondName);
	    var context = new RawPowerLabs.DynamicAI.Context();
	    var parameters = TextModuleParameters.GetDefault();
	    _textModule = context.CreateTextModule(parameters, diamondPath.modelPath, diamondPath.templatePath);
	    if (_textModule == null)
	    {
		    UnityEngine.Debug.LogError("Something went wrong with the Diamond");
	    }

	    var types = Enum.GetValues(typeof(Type)) as Type[];
	    var typeIndex = UnityEngine.Random.Range(0, types.Length);
	    var randomType = types[typeIndex].ToString();
	    UnityEngine.Debug.Log(randomType);
	    
	    var colors = Enum.GetValues(typeof(Color)) as Color[];
	    var colorIndex = UnityEngine.Random.Range(0, colors.Length);
	    var randomColor = colors[colorIndex].ToString();
	    
	    var materials = Enum.GetValues(typeof(Material)) as Material[];
	    var materialIndex = UnityEngine.Random.Range(0, materials.Length);
	    var randomMaterial = materials[materialIndex].ToString();
	    
	    PrintReplies(randomType, randomColor, randomMaterial);
    }
    
    private async void PrintReplies(string type, string color, string material)
    {
	    var replies = new Dictionary<string, string>();

	    try
	    {
		    replies = await InvokeReplyAsync(type, color, material);
	    }
	    catch (Exception e)
	    {
		    UnityEngine.Debug.LogException(e);
	    }

	    foreach (var reply in replies)
	    {
		    UnityEngine.Debug.Log($"Key: {reply.Key}, Value: {reply.Value}");
	    }
    }
    
    private async Task<Dictionary<string, string>> InvokeReplyAsync(string type, string color, string material)
	{
		return await Task.Run(() => InvokeReply(type, color, material));
	}
	
	private Dictionary<string, string> InvokeReply(string type, string color, string material)
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
		
		var replies = new Dictionary<string, string>();
		
		foreach (var outputValues in CategoricalOutputCollection.StringOutputValues)
		{
			var result = textResult.GetString(outputValues.Value);
			replies.Add(outputValues.Value, result);
		}
	    
		return replies;
	}
}

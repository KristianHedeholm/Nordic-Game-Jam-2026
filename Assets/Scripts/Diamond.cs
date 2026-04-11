using System;
using UnityEngine;
using RawPowerLabs.DynamicAI;
using RawPowerLabs.DynamicAI.Utility;

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
    }
}

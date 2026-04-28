namespace RawPowerLabs.DynamicAI.Editor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using Humanizer;
	using RawPowerLabs.DynamicAI.Utility;
	using Utility;
	
	
	[System.Serializable]
	public struct Template
	{
		public string name;
		public string description;
		public string prompt;
		public string version;
		public List<TemplateInput> input;
		public List<TemplateOutput> output;
	}

	[System.Serializable]
	public struct TemplateInput
	{
		public string type;
		public int min;
		public int max;
		public List<string> options;
		public string name;
		public string description;
		public string value;
	}

	[System.Serializable]
	public struct TemplateOutput
	{
		public string type;
		public int min;
		public int max;
		public string name;
		public string description;
		public string value;
	}
	
	// TODO Simplify and use IReadOnlyDictionaries
	public readonly struct UniqueEnumsCollection
	{
		public readonly Dictionary<string, string> InputEnumStringMapping;
		public readonly Dictionary<string, Dictionary<string, string>> InputEnumValueStringMapping;
		public readonly Dictionary<string, string> OutputEnumStringMapping;
		
		public UniqueEnumsCollection
		(
			Dictionary<string, string> inputEnumStringMapping,
			Dictionary<string, Dictionary<string, string>> inputEnumValueStringMapping,
			Dictionary<string, string> outputEnumStringMapping
		)
		{
			InputEnumStringMapping = inputEnumStringMapping;
			InputEnumValueStringMapping = inputEnumValueStringMapping;
			OutputEnumStringMapping = outputEnumStringMapping;
		}
	}
	
	public static class DiamondEditor
	{
		private const string CategoricalInputType = "categorical";
		private const string StringOutputType = "string";
		
		public static void GenerateSourceCodeBasedOnDiamond(string diamondName)
		{
			if (!TryGetTemplateFromDiamondName(diamondName, out Template template))
			{
				UnityEngine.Debug.LogError($"Not able to find diamond with name {diamondName}");
				return;
			}
			
			var enumCollection = GetUniqueEnumCollection(template);
			DiamondCodeGeneration.GenerateDiamondSourceCode(enumCollection);
		}
		
		private static bool TryGetTemplateFromDiamondName(string diamondName, out Template template)
		{
			template = new Template();
			if (!DiamondEditorUtility.ValidateDiamondName(diamondName))
			{
				return false;
			}
			
			var templateAsJson = GetTemplateAsJsonFromDiamondName(diamondName);
			if (string.IsNullOrEmpty(templateAsJson))
			{
				return false;
			}
			
			try
			{
				template = JsonUtility.FromJson<Template>(templateAsJson);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				return false;
			}
			
			return true;
		}
		
		private static string GetTemplateAsJsonFromDiamondName(string diamondName)
		{
			var diamondPath = DiamondUtility.GetPathFromDiamondName(diamondName);
			var jsonText = File.ReadAllText(diamondPath.templatePath);
			return jsonText;
		}
		
		private static UniqueEnumsCollection GetUniqueEnumCollection(Template template)
		{
			var inputEnumStringMapping = new Dictionary<string, string>();
			var inputEnumValueStringMapping = new Dictionary<string, Dictionary<string, string>>();
			foreach (var input in template.input)
			{
				if (!input.type.Contains(CategoricalInputType))
				{
					continue;
				}
				
				AddNameIfUnique(input.name, inputEnumStringMapping);
				
				var enumValueStringMapping = new Dictionary<string, string>();
				foreach (var enumValue in input.options)
				{
					AddNameIfUnique(enumValue, enumValueStringMapping);
				}
				
				AddNameIfUnique(input.name, enumValueStringMapping, inputEnumValueStringMapping);
			}
			
			var outputEnumStringMapping = new Dictionary<string, string>();
			foreach (var output in template.output)
			{
				if (!output.type.Contains(StringOutputType))
				{
					continue;
				}
				AddNameIfUnique(output.name, outputEnumStringMapping);
			}
			
			return new UniqueEnumsCollection(inputEnumStringMapping, inputEnumValueStringMapping, outputEnumStringMapping);
		}
		
		private static void AddNameIfUnique(string nameValue, Dictionary<string, string> dictionary,
			Dictionary<string, Dictionary<string, string>> collection = null)
		{
			if (string.IsNullOrEmpty(nameValue) || dictionary == null)
			{
				return;
			}
			
			var nameKey = nameValue.Dehumanize();
			var successfulAdded = false;
			
			// if no collection has been given, we just need to try adding
			// the name to the dictionary.
			if (collection == null)
			{
				successfulAdded = dictionary.TryAdd(nameKey, nameValue);
			}
			else
			{
				// if a collection has been given, we should add the dictionary
				// to the collection.
				successfulAdded = collection.TryAdd(nameKey, dictionary);
			}
			
			if (!successfulAdded)
			{
				UnityEngine.Debug.LogError($"The following name {nameKey} already exists as a key! Skipping to avoid compile error");
			}
		}
	}
}
namespace RawPowerLabs.DynamicAI.Editor
{
	using System.IO;
	using System.Text;
	using UnityEditor;
	using UnityEditor.Compilation;
	
	public static class DiamondCodeGeneration
	{
		private const string CodeGenerateTemplateFolderPath = "Assets/DynamicAI/Editor/CodeGeneration/Templates";
		private const string SourceCodeDestinationFolderPath = "Assets/DynamicAI/Runtime";
		
		private const string CategoricalInputTemplateName = "CategoricalInputCollection.cs.txt";
		private const string CategoricalOutputsTemplateName = "CategoricalOutputCollection.cs.txt";
		private const string InsertEnumKeyword = "#INSERTENUMS";
		private const string InsertEnumStringKeyword = "#INSERTSTRINGENUMS";
		private const string InsertEnumMappingKeyword = "#INSERTMAPPINGENUMS";
		private const string InsertCategoricalsKeyword = "#INSERTCATEGORICALS";
		
		public static void GenerateDiamondSourceCode(UniqueEnumsCollection enumsCollection)
		{
			var inputEnumSourceCode = GetCodeTemplate(CategoricalInputTemplateName);
			if (inputEnumSourceCode == null)
			{
				return;
			}
			
			GenerateAndSaveInputEnumSourceFile(enumsCollection, inputEnumSourceCode);
			
			var outputEnumSourceCode = GetCodeTemplate(CategoricalOutputsTemplateName);
			if (outputEnumSourceCode == null)
			{
				return;
			}
			
			GenerateAndSaveOutputEnumsSourceFile(enumsCollection, outputEnumSourceCode);
			
			AssetDatabase.Refresh();
			CompilationPipeline.RequestScriptCompilation();
		}

		private static StringBuilder GetCodeTemplate(string templateName)
		{
			var path = Path.Combine(CodeGenerateTemplateFolderPath,  templateName);
			if (!File.Exists(path))
			{
				return null;
			}
			
			var file = File.ReadAllText(path);
			return new StringBuilder(file);
		}

		private static void GenerateAndSaveInputEnumSourceFile(UniqueEnumsCollection enumsCollection, StringBuilder sourceCode)
		{
			var stringBuilder = new StringBuilder();
			var index = 0;
			foreach (var enumName in enumsCollection.InputEnumStringMapping)
			{
				stringBuilder.AppendLine($"\tpublic enum {enumName.Key}");
				stringBuilder.AppendLine("\t{");
				
				var enumValueIndex = 0;
				var enumStringValues = enumsCollection.InputEnumValueStringMapping[enumName.Key];
				foreach (var enumStringValue in enumStringValues)
				{
					stringBuilder.AppendLine($"\t\t{enumStringValue.Key} = {enumValueIndex},");
					enumValueIndex++;
				}
				
				stringBuilder.Append("\t}");
				if (index < enumsCollection.InputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n\n");
				}
				index++;
			}
			sourceCode.Replace(InsertEnumKeyword, stringBuilder.ToString());

			stringBuilder.Clear();
			index = 0;
			
			foreach (var enumName in enumsCollection.InputEnumStringMapping)
			{
				stringBuilder.AppendLine($"\t\tpublic static IReadOnlyDictionary<Enum, string> {enumName.Key}StringValues =");
				stringBuilder.AppendLine($"\t\tnew Dictionary<Enum, string>()");
				stringBuilder.AppendLine("\t\t{");
				
				var enumStringValues = enumsCollection.InputEnumValueStringMapping[enumName.Key];
				foreach (var enumStringValue in enumStringValues)
				{
					stringBuilder.AppendLine("\t\t\t{ " + $"{enumName.Key}.{enumStringValue.Key}, \"{enumStringValue.Value}\"" + " },");
				}
				
				stringBuilder.AppendLine("\t\t};");
				if (index < enumsCollection.InputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n");
				}
				index++;
			}
			sourceCode.Replace(InsertEnumStringKeyword, stringBuilder.ToString());

			stringBuilder.Clear();
			index = 0;
			
			foreach (var enumName in enumsCollection.InputEnumStringMapping)
			{
				stringBuilder.Append("\t\t\t{ " + $"typeof({enumName.Key}), {enumName.Key}StringValues" + " },");
				if (index < enumsCollection.InputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n");
				}
				index++;
			}
			sourceCode.Replace(InsertEnumMappingKeyword, stringBuilder.ToString());
			
			stringBuilder.Clear();
			index = 0;
			foreach (var enumName in enumsCollection.InputEnumStringMapping)
			{
				stringBuilder.Append("\t\t\t{ " + $"typeof({enumName.Key}), \"{enumName.Value}\"" + " },");
				if (index < enumsCollection.InputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n");
				}
				index++;
			}
			sourceCode.Replace(InsertCategoricalsKeyword, stringBuilder.ToString());
			
			var sourceFile = GetSourceFileNameFromTemplate(CategoricalInputTemplateName);
			var path = Path.Combine(SourceCodeDestinationFolderPath, sourceFile);
			File.WriteAllText(path, sourceCode.ToString());
		}

		private static void GenerateAndSaveOutputEnumsSourceFile(UniqueEnumsCollection enumsCollection, StringBuilder sourceCode)
		{
			var stringBuilder = new StringBuilder();
			var index = 0;
			
			foreach (var enumName in enumsCollection.OutputEnumStringMapping)
			{
				stringBuilder.Append($"\t\t{enumName.Key} = {index},");
				
				if (index < enumsCollection.OutputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n");
				}
				index++;
			}
			sourceCode.Replace(InsertEnumKeyword, stringBuilder.ToString());

			stringBuilder.Clear();
			index = 0;
			foreach (var stringValues in enumsCollection.OutputEnumStringMapping)
			{
				stringBuilder.Append("\t\t\t{ " + $"{nameof(CategoricalOutput)}.{stringValues.Key}, \"{stringValues.Value}\"" + " },");
				if (index < enumsCollection.OutputEnumStringMapping.Count - 1)
				{
					stringBuilder.Append("\n");
				}
				index++;
			}
			
			sourceCode.Replace(InsertEnumMappingKeyword, stringBuilder.ToString());
			var sourceFile = GetSourceFileNameFromTemplate(CategoricalOutputsTemplateName);
			var path = Path.Combine(SourceCodeDestinationFolderPath, sourceFile);
			File.WriteAllText(path, sourceCode.ToString());
		}
		
		private static string GetSourceFileNameFromTemplate(string templateName)
		{
			return templateName.Replace(".txt", string.Empty);
		}
	}
}
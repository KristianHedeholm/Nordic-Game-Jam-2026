namespace RawPowerLabs.DynamicAI.Utility
{
	using System.IO;
	using UnityEngine;
	
	public static class DiamondUtility
	{
		/// <summary>
		/// The path that we suggest you use inside of Unity for the Diamonds
		/// This path is assumed to be inside the Assets/StreamingAssets folder.
		/// </summary>
		public static readonly string DiamondPath = "DynamicAI/Diamonds";
		
		/// <summary>
		/// Based on the name of the folder that contains the model (.gguf) and template (.json) files
		/// inside the Streaming Assets folder in Unity, get the absolute path to the files.
		/// This will work for both editor and builds. 
		/// </summary>
		/// <param name="diamondName">The name of the folder that contains the Diamond you want the path.</param>
		/// <returns>The absolute path to both the model (.gguf) and template (.json) file</returns>
		public static (string modelPath, string templatePath) GetPathFromDiamondName(string diamondName)
		{
			if (string.IsNullOrEmpty(diamondName))
			{
				UnityEngine.Debug.LogError("Diamond Name is not set, please set the name.");
				return (string.Empty, string.Empty);
			}
        
			var diamondFolderPath = Path.Combine(Application.streamingAssetsPath, DiamondPath, diamondName);
			var modelPath = Path.Combine(diamondFolderPath, "model.gguf");
			var templatePath = Path.Combine(diamondFolderPath, "template.json");
        
			return (modelPath, templatePath);
		}
	}
}
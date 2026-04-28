namespace RawPowerLabs.DynamicAI.Editor.Utility
{
	using System.IO;
	using UnityEngine;
	using UnityEditor;
	using RawPowerLabs.DynamicAI.Utility;

	public static class DiamondEditorUtility
	{
		private const string AssetFolder = "Assets";
		private static bool DoesFolderWithDiamondsExist()
		{
			var diamondFolderPath = Path.Combine(Application.streamingAssetsPath, DiamondUtility.DiamondPath);
			return Directory.Exists(diamondFolderPath);
		}
		
		private static bool DoesDiamondFolderWithNameExist(string diamondName)
		{
			var diamondFolderPath = Path.Combine(Application.streamingAssetsPath, DiamondUtility.DiamondPath, diamondName);
			return Directory.Exists(diamondFolderPath);
		}

		[MenuItem("Raw Power Labs/Dynamic AI/Create Streaming Assets Diamond Folder", false)]
		public static void CreateDiamondFolder()
		{
			var fullPath = Path.Combine(Application.streamingAssetsPath, DiamondUtility.DiamondPath);
			var diamondFolderPath  = Path.GetRelativePath(AssetFolder, fullPath);
			var diamondFolderPathParts = diamondFolderPath.Split(Path.DirectorySeparatorChar);
			
			var parent = AssetFolder;
			for (int i = 0; i < diamondFolderPathParts.Length; i++)
			{
				var currentFolder = diamondFolderPathParts[i];
				var currentPath = Path.Combine(parent, currentFolder);
				
				if(!AssetDatabase.IsValidFolder(currentPath))
				{
					AssetDatabase.CreateFolder(parent, currentFolder);
				}

				parent = currentPath;
			}
		}

		[MenuItem("Raw Power Labs/Dynamic AI/Create Streaming Assets Diamond Folder", true)]
		public static bool CheckIfDiamondFolderExist()
		{
			return !DoesFolderWithDiamondsExist();
		}
		
		public static bool ValidateDiamondName(string diamondName)
		{
			if (string.IsNullOrEmpty(diamondName))
			{
				UnityEngine.Debug.LogError($"diamondName is null or empty");
				return false;
			}
	
			if (!DoesDiamondFolderWithNameExist(diamondName))
			{
				UnityEngine.Debug.LogError($"the folder for {diamondName} doesn't exist");
				return false;
			}

			return true;
		}
	}
}
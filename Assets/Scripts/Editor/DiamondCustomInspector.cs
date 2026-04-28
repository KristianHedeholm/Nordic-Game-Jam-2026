using UnityEngine;
using UnityEditor;
using RawPowerLabs.DynamicAI.Editor;

[CustomEditor(typeof(Diamond))]
public class GameSetupCustomInspector : Editor
{
	private SerializedProperty _diamondName;
    
	void OnEnable()
	{
		_diamondName = serializedObject.FindProperty("_diamondName");
	}
    
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
        
		GUILayout.Label($"Please write the name of the Diamond,\nfound in the StreamingAssets folder" );
		_diamondName.stringValue = EditorGUILayout.DelayedTextField("Diamond Name", _diamondName.stringValue);
		_diamondName.serializedObject.ApplyModifiedProperties();

		if (GUILayout.Button("Generate values based on Diamond"))
		{
			var diamondName = _diamondName.stringValue;
			DiamondEditor.GenerateSourceCodeBasedOnDiamond(diamondName);
		}
	}
}
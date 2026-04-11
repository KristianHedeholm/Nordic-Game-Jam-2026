namespace RawPowerLabs.DynamicAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	
	[CustomPropertyDrawer(typeof(CategoricalName))]
	public class CategoricalNameCustomInspector : PropertyDrawer
	{
		private string _selectedName;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!CategoricalEditorUtility.HasCategoricalBeenInitialized())
			{
				var noLabelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.LabelField(noLabelRect, "No categoricals available.");
				return;
			}

			var nameProperty = property.FindPropertyRelative("Name");
			_selectedName = CategoricalEditorUtility.GetSelectedCategoricalTypeName(_selectedName, nameProperty.stringValue);
			
			EditorGUI.BeginProperty(position, label, property);
			
			// calculate position for label of the field.
			var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(labelRect, label.text);
			
			// calculate position for the dropdown field.
			var dropdownRect = new Rect(labelRect.x, labelRect.y + labelRect.height, position.width, EditorGUIUtility.singleLineHeight);
			if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(_selectedName), FocusType.Passive))
			{
				var menu = CategoricalEditorUtility.CreateCategoricalNameMenu(position, _selectedName, OnSelectedCategoricalName);
				menu.ShowAsContext();
			}
			
			EditorGUI.EndProperty();

			if (nameProperty.stringValue != _selectedName)
			{
				nameProperty.stringValue = _selectedName;
				nameProperty.serializedObject.ApplyModifiedProperties();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!CategoricalEditorUtility.HasCategoricalBeenInitialized())
			{
				return EditorGUIUtility.singleLineHeight;
			}
			
			return EditorGUIUtility.singleLineHeight * 2;
		}
		
		private void OnSelectedCategoricalName(object selectedName)
		{
			_selectedName = (string)selectedName; 
		}
	}
}


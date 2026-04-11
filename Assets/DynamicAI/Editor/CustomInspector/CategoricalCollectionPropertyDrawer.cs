namespace RawPowerLabs.DynamicAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	
	[CustomPropertyDrawer(typeof(CategoricalCollection<>))]
	public class CategoricalCollectionPropertyDrawer : PropertyDrawer
	{
		private string _selectedName;
		private bool _isFoldout;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			
			var foldOutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			
			_isFoldout = EditorGUI.Foldout(foldOutPosition, _isFoldout, label);
			if (!_isFoldout)
			{
				return;
			} 
			
			if (!CategoricalEditorUtility.HasCategoricalBeenInitialized())
			{
				var noLabelRect = CategoricalEditorUtility.GetNextLineRect(foldOutPosition);
				EditorGUI.LabelField(noLabelRect, "No categoricals available.");
				return;
			}
			
			var nameProperty = property.FindPropertyRelative("Name");
			_selectedName = CategoricalEditorUtility.GetSelectedCategoricalTypeName(_selectedName, nameProperty.stringValue);
			
			var dropdownRect = CategoricalEditorUtility.GetNextLineRect(foldOutPosition);
			if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(_selectedName), FocusType.Passive))
			{
				var menu = CategoricalEditorUtility.CreateCategoricalNameMenu(position, _selectedName, OnSelectedCategoricalName);
				menu.ShowAsContext();
			}
			
			if (nameProperty.stringValue != _selectedName)
			{
				ResetCollection(property);
				
				nameProperty.stringValue = _selectedName;
				nameProperty.serializedObject.ApplyModifiedProperties();
			}

			SetCollection(dropdownRect, property, _selectedName);
			
			EditorGUI.EndProperty();
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!_isFoldout)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			
			if (!CategoricalEditorUtility.HasCategoricalBeenInitialized())
			{
				return EditorGUIUtility.singleLineHeight * 2;
			}

			var mappings = property.FindPropertyRelative("Mappings");
			return EditorGUIUtility.singleLineHeight * (mappings.arraySize + 2);
		}
		
		private void OnSelectedCategoricalName(object selectedName)
		{
			_selectedName = (string)selectedName; 
		}

		private void SetCollection(Rect position, SerializedProperty property, string selectedName)
		{
			var fieldType = fieldInfo.FieldType;
			if (!fieldType.IsGenericType)
			{
				return;
			}
			
			var genericArguments = fieldType.GetGenericArguments();
			var objectType = genericArguments[0];
			
			var categoricalValueNames = CategoricalEditorUtility.GetCategoricalValueNames(selectedName);
			var mappingsProperty = property.FindPropertyRelative("Mappings");

			if (mappingsProperty == null)
			{
				return;
			}

			if (mappingsProperty.arraySize != categoricalValueNames.Count)
			{
				mappingsProperty.arraySize = categoricalValueNames.Count;
			}
			
			var y = position.y + EditorGUIUtility.singleLineHeight;

			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < categoricalValueNames.Count; i++)
			{
				var valueName = categoricalValueNames[i];
				var mappingElement = mappingsProperty.GetArrayElementAtIndex(i);
				var valueProp = mappingElement.FindPropertyRelative("Value");
				var nameProp = mappingElement.FindPropertyRelative("Name");
				
				nameProp.stringValue = valueName;
				
				var row = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.ObjectField(
					row,
					valueProp,
					objectType,
					new GUIContent(valueName)
				);

				y += EditorGUIUtility.singleLineHeight;
			}

			if (EditorGUI.EndChangeCheck())
			{
				mappingsProperty.serializedObject.ApplyModifiedProperties();
			}
		}

		private void ResetCollection(SerializedProperty property)
		{
			var mappingsProperty = property.FindPropertyRelative("Mappings");
			if (mappingsProperty == null)
			{
				return;
			}
			
			mappingsProperty.arraySize = 0;
		}
	}
}

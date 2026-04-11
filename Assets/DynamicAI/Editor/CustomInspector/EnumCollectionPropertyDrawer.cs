using System;

namespace RawPowerLabs.DynamicAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomPropertyDrawer(typeof(EnumCollection<,>))]
	public class EnumCollectionPropertyDrawer : PropertyDrawer
	{
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
			
			SetCollection(foldOutPosition, property);
			
			EditorGUI.EndProperty();
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!_isFoldout)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			
			var mappings = property.FindPropertyRelative("_mappings");
			return EditorGUIUtility.singleLineHeight * (mappings.arraySize + 1) ;
		}

		private void SetCollection(Rect position, SerializedProperty property)
		{
			var fieldType = fieldInfo.FieldType;
			if (!fieldType.IsGenericType)
			{
				return;
			}
			
			var genericArguments = fieldType.GetGenericArguments();
			var enumType = genericArguments[0];
			var enumValues = Enum.GetValues(enumType);
			var objectType = genericArguments[1];
			
			var mappingsProperty = property.FindPropertyRelative("_mappings");

			if (mappingsProperty == null)
			{
				return;
			}
			
			if (mappingsProperty.arraySize != enumValues.Length)
			{
				mappingsProperty.arraySize = enumValues.Length;
			}
			
			var y = position.y + EditorGUIUtility.singleLineHeight;
			
			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < enumValues.Length; i++)
			{
				var name = enumValues.GetValue(i).ToString();
				var fieldName = ObjectNames.NicifyVariableName(name);
				var enumIntValue = (int)enumValues.GetValue(i);
				var mappingElement = mappingsProperty.GetArrayElementAtIndex(i);
				var valueProp = mappingElement.FindPropertyRelative("Value");
				var idProp = mappingElement.FindPropertyRelative("ID");
				
				idProp.intValue = enumIntValue;
				
				var row = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.ObjectField(
					row,
					valueProp,
					objectType,
					new GUIContent(fieldName)
				);

				y += EditorGUIUtility.singleLineHeight;
			}
			
			if (EditorGUI.EndChangeCheck())
			{
				mappingsProperty.serializedObject.ApplyModifiedProperties();
			}
		}
	}
	
	
}

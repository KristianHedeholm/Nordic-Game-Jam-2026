using System.Linq;

namespace RawPowerLabs.DynamicAI.Editor
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	
	public static class CategoricalEditorUtility
	{
		public static bool HasCategoricalBeenInitialized()
		{
			return CategoricalInputCollection.TypeNames.Count > 0; 
		}

		public static GenericMenu CreateCategoricalNameMenu(Rect position, string selectedName, GenericMenu.MenuFunction2 selectedNameCallback)
		{
			var menu = new GenericMenu();
			menu.allowDuplicateNames = false;
			menu.DropDown(position);
			
			var categoricals = CategoricalInputCollection.TypeNames;
			
			foreach (var categorical in categoricals)
			{
				var isSelected = categorical.Value == selectedName;
				menu.AddItem(new GUIContent(categorical.Value), isSelected, selectedNameCallback, categorical.Value);
			}

			return menu;
		}

		public static string GetSelectedCategoricalTypeName(string currentTypeName, string serializedTypeName)
		{
			if (!string.IsNullOrEmpty(currentTypeName))
			{
				return currentTypeName;
			}

			if (string.IsNullOrEmpty(serializedTypeName))
			{
				return CategoricalInputCollection.TypeNames.Values.First();
			}
			
			return serializedTypeName;
		}

		public static Rect GetNextLineRect(Rect position)
		{
			return new Rect(position.x, position.y + position.height, position.width, EditorGUIUtility.singleLineHeight);
		}
		
		public static List<string> GetCategoricalValueNames(string name)
		{
			var valueNames = new List<string>();
			var valueMapping = GetCategoricalValues(name);
			if (valueMapping == null)
			{
				return valueNames;
			}
			
			foreach (var valueMap in valueMapping)
			{
				valueNames.Add(valueMap.Value);
			}

			return valueNames;
		}

		private static IReadOnlyDictionary<Enum, string> GetCategoricalValues(string name)
		{
			var categoricals = CategoricalInputCollection.TypeNames;
			foreach (var categorical in categoricals)
			{
				if (categorical.Value != name)
				{
					continue;
				}

				return CategoricalInputCollection.AllCollections[categorical.Key];
			}

			return null;
		}
	}
}

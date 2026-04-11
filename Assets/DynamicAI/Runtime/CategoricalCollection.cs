namespace RawPowerLabs.DynamicAI
{
	using System;
	using UnityEngine;
	
	[Serializable]
	public class CategoricalCollectionBase
	{
		public string Name;
	}
	
	[Serializable]
	public struct CategoricalMapping<T> where T : UnityEngine.Object
	{
		public string Name;
		public T Value;
	}

	[Serializable]
	public class CategoricalCollection<T> : CategoricalCollectionBase where T : UnityEngine.Object
	{
		[SerializeField]
		private CategoricalMapping<T>[] Mappings;

		public T GetValue(string name)
		{
			for (int i = 0; i < Mappings.Length; i++)
			{
				if (Mappings[i].Name == name)
				{
					return Mappings[i].Value;
				}
			}

			return null;
		}
	}
}

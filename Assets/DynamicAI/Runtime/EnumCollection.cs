namespace RawPowerLabs.DynamicAI
{
	using System;
	using UnityEngine;
	
	[Serializable]
	public class EnumCollectionBase
	{
		
	}
	
	[Serializable]
	public struct EnumMapping<T> where T : UnityEngine.Object
	{
		public int ID;
		public T Value;
	}
	
	[Serializable]
	public class EnumCollection<Enum, T> : EnumCollectionBase where T : UnityEngine.Object
	{
		[SerializeField]
		private EnumMapping<T>[] _mappings;
		
		public T GetValue(int id)
		{
			for (int i = 0; i < _mappings.Length; i++)
			{
				if (_mappings[i].ID == id)
				{
					return _mappings[i].Value;
				}
			}

			return null;
		}
	}
}

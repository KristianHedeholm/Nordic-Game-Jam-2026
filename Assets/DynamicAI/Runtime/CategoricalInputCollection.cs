// This source file is generated from the editor tool
// provided by Raw Power Labs. It is advised not to
// make any direct changes to this source file,
// since these will be overwritten by the tool.
namespace RawPowerLabs.DynamicAI
{
    using System;
    using System.Collections.Generic;
    
	public enum Color
	{
		Blue = 0,
		Red = 1,
		Yellow = 2,
		Orange = 3,
		Purple = 4,
		Gold = 5,
		White = 6,
		Green = 7,
		Pink = 8,
		Silver = 9,
		Brown = 10,
		Black = 11,
	}

	public enum Material
	{
		Gold = 0,
		Iron = 1,
		Silk = 2,
		Cotton = 3,
		Fur = 4,
		Leather = 5,
		Feathers = 6,
		Polyester = 7,
		VeganLeather = 8,
		FauxFur = 9,
	}

	public enum Type
	{
		Pants = 0,
		Armor = 1,
		Cape = 2,
		Bathrobe = 3,
		Mankini = 4,
		MaidSDress = 5,
		Crocs = 6,
		FingerlessGloves = 7,
	}

    public static class CategoricalInputCollection
    {
		public static IReadOnlyDictionary<Enum, string> ColorStringValues =
		new Dictionary<Enum, string>()
		{
			{ Color.Blue, "Blue" },
			{ Color.Red, "Red" },
			{ Color.Yellow, "Yellow" },
			{ Color.Orange, "Orange" },
			{ Color.Purple, "Purple" },
			{ Color.Gold, "Gold" },
			{ Color.White, "White" },
			{ Color.Green, "Green" },
			{ Color.Pink, "Pink" },
			{ Color.Silver, "Silver" },
			{ Color.Brown, "Brown" },
			{ Color.Black, "Black" },
		};

		public static IReadOnlyDictionary<Enum, string> MaterialStringValues =
		new Dictionary<Enum, string>()
		{
			{ Material.Gold, "Gold" },
			{ Material.Iron, "Iron" },
			{ Material.Silk, "Silk" },
			{ Material.Cotton, "Cotton" },
			{ Material.Fur, "Fur" },
			{ Material.Leather, "Leather" },
			{ Material.Feathers, "Feathers" },
			{ Material.Polyester, "Polyester" },
			{ Material.VeganLeather, "Vegan Leather" },
			{ Material.FauxFur, "Faux Fur" },
		};

		public static IReadOnlyDictionary<Enum, string> TypeStringValues =
		new Dictionary<Enum, string>()
		{
			{ Type.Pants, "Pants" },
			{ Type.Armor, "Armor" },
			{ Type.Cape, "Cape" },
			{ Type.Bathrobe, "Bathrobe" },
			{ Type.Mankini, "Mankini" },
			{ Type.MaidSDress, "Maid's Dress" },
			{ Type.Crocs, "Crocs" },
			{ Type.FingerlessGloves, "Fingerless Gloves" },
		};


        public static IReadOnlyDictionary<System.Type, IReadOnlyDictionary<Enum, string>> AllCollections =
        new Dictionary<System.Type, IReadOnlyDictionary<Enum, string>>()
        {
			{ typeof(Color), ColorStringValues },
			{ typeof(Material), MaterialStringValues },
			{ typeof(Type), TypeStringValues },
        };
        
        public static IReadOnlyDictionary<System.Type, string> TypeNames =
        new Dictionary<System.Type, string>()
        {
			{ typeof(Color), "Color" },
			{ typeof(Material), "Material" },
			{ typeof(Type), "Type" },
        };
    }
}

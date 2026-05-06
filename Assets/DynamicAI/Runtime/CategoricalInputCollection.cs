// This source file is generated from the editor tool
// provided by Raw Power Labs. It is advised not to
// make any direct changes to this source file,
// since these will be overwritten by the tool.
namespace RawPowerLabs.DynamicAI
{
    using System;
    using System.Collections.Generic;
    
	public enum ColorAnswer
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

	public enum MaterialAnswer
	{
		Gold = 0,
		Iron = 1,
		Silk = 2,
		Cotton = 3,
		Fur = 4,
		Leather = 5,
		Feathers = 6,
		Polyester = 7,
		FauxFur = 8,
	}

	public enum GarmentAnswer
	{
		Pants = 0,
		Armor = 1,
		Cape = 2,
		Bathrobe = 3,
		Mankini = 4,
		MaidSDress = 5,
		Crocs = 6,
		Gloves = 7,
	}

    public static class CategoricalInputCollection
    {
		public static IReadOnlyDictionary<Enum, string> ColorAnswerStringValues =
		new Dictionary<Enum, string>()
		{
			{ ColorAnswer.Blue, "Blue" },
			{ ColorAnswer.Red, "Red" },
			{ ColorAnswer.Yellow, "Yellow" },
			{ ColorAnswer.Orange, "Orange" },
			{ ColorAnswer.Purple, "Purple" },
			{ ColorAnswer.Gold, "Gold" },
			{ ColorAnswer.White, "White" },
			{ ColorAnswer.Green, "Green" },
			{ ColorAnswer.Pink, "Pink" },
			{ ColorAnswer.Silver, "Silver" },
			{ ColorAnswer.Brown, "Brown" },
			{ ColorAnswer.Black, "Black" },
		};

		public static IReadOnlyDictionary<Enum, string> MaterialAnswerStringValues =
		new Dictionary<Enum, string>()
		{
			{ MaterialAnswer.Gold, "Gold" },
			{ MaterialAnswer.Iron, "Iron" },
			{ MaterialAnswer.Silk, "Silk" },
			{ MaterialAnswer.Cotton, "Cotton" },
			{ MaterialAnswer.Fur, "Fur" },
			{ MaterialAnswer.Leather, "Leather" },
			{ MaterialAnswer.Feathers, "Feathers" },
			{ MaterialAnswer.Polyester, "Polyester" },
			{ MaterialAnswer.FauxFur, "Faux Fur" },
		};

		public static IReadOnlyDictionary<Enum, string> GarmentAnswerStringValues =
		new Dictionary<Enum, string>()
		{
			{ GarmentAnswer.Pants, "Pants" },
			{ GarmentAnswer.Armor, "Armor" },
			{ GarmentAnswer.Cape, "Cape" },
			{ GarmentAnswer.Bathrobe, "Bathrobe" },
			{ GarmentAnswer.Mankini, "Mankini" },
			{ GarmentAnswer.MaidSDress, "Maid's Dress" },
			{ GarmentAnswer.Crocs, "Crocs" },
			{ GarmentAnswer.Gloves, "Gloves" },
		};


        public static IReadOnlyDictionary<System.Type, IReadOnlyDictionary<Enum, string>> AllCollections =
        new Dictionary<System.Type, IReadOnlyDictionary<Enum, string>>()
        {
			{ typeof(ColorAnswer), ColorAnswerStringValues },
			{ typeof(MaterialAnswer), MaterialAnswerStringValues },
			{ typeof(GarmentAnswer), GarmentAnswerStringValues },
        };
        
        public static IReadOnlyDictionary<System.Type, string> TypeNames =
        new Dictionary<System.Type, string>()
        {
			{ typeof(ColorAnswer), "color_answer" },
			{ typeof(MaterialAnswer), "material_answer" },
			{ typeof(GarmentAnswer), "garment_answer" },
        };
    }
}

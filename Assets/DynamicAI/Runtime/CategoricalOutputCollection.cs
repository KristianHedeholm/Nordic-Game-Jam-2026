// This source file is generated from the editor tool
// provided by Raw Power Labs. It is advised not to
// make any direct changes to this source file,
// since these will be overwritten by the tool.
namespace RawPowerLabs.DynamicAI
{
    using System.Collections.Generic;
    
    public enum CategoricalOutput
    {
		TypeRiddle = 0,
		ColorRiddle = 1,
		MaterialRiddle = 2,
    }
	
    public static class CategoricalOutputCollection
    {
        public static IReadOnlyDictionary<CategoricalOutput, string> StringOutputValues =
        new Dictionary<CategoricalOutput, string>()
        {
			{ CategoricalOutput.TypeRiddle, "Type_Riddle" },
			{ CategoricalOutput.ColorRiddle, "Color_Riddle" },
			{ CategoricalOutput.MaterialRiddle, "Material_Riddle" },
        };
	}
}

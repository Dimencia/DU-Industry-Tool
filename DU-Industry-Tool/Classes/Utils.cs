using System;
using System.Collections.Generic;

namespace DU_Industry_Tool
{
    /// <summary>
    /// Summary description for Utils
    /// </summary>
    public static class Utils
    {
        public static SortedDictionary<string, DuLuaItem> LuaItems; // List of items from du-lua.dev
        public static SortedDictionary<string, DuLuaRecipe> LuaRecipes; // List of recipes from du-lua.dev

        static Utils()
        {
            //
        }
    }
}

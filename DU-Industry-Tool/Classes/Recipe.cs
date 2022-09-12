using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace DU_Industry_Tool
{
    public class SchematicRecipe
    {
        public int Level { get; set; }
        public string Name { get; set; }
        [DataMember(Name="id")]
        public ulong Id { get; set; }
        public List<ProductDetail> Products { get; } = new List<ProductDetail>();
        public float Time { get; set; }
        public List<ProductDetail> Ingredients { get; } = new List<ProductDetail>();
        public Guid GroupId { get; set; }
        public string ParentGroupName { get; set; }
        [JsonIgnore]
        public string Key { get; set; }
        [JsonIgnore]
        public TreeNode Node { get; set; }
        public ulong NqId { get; set; } // Different from Id... for markets
        public string SchemaType { get; set; }
        public double SchemaPrice { get; set; }
        public double? UnitMass { get; set; }
        public double? UnitVolume { get; set; }
        public bool Nanocraftable { get; set; }
        public string Size { get; set; }
        public string Industry { get; set; }
    }

    public class IngredientRecipe : SchematicRecipe
    {
        public double Quantity { get; set; }
    }

    public class ProductDetail
    {
        public string Type { get; set; }
        public double Quantity { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string SchemaType { get; set; }
        public int Level { get; set; }
    }

    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
    }

    public class Talent
    {
        public string Name { get; set; }
        public double Multiplier { get; set; }
        public int Addition { get; set; }
        public List<string> ApplicableRecipes { get; set; } = new List<string>();
        public int Value { get; set; } // Contains the effective talent level that we're using.
        public bool InputTalent { get; set; } = false;
    }

    // For saving settings, a simple lookup
    public class Ore
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public int Level { get; set; }
    }

    public class Schematic
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public double Cost { get; set; }   // cost per 1 copy
        public int Level { get; set; }
        public int BatchSize { get; set; } // copies per batch
        public int BatchTime { get; set; } // seconds
    }

    // external/3rd party json structures:

    public class DuLuaSchematic
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name="displayNameWithSize")]
        public string DisplayNameWithSize { get; set; }
    }

    public class DuLuaItem
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name="tier")]
        public int Tier { get; set; }
        [DataMember(Name="displayNameWithSize")]
        public string DisplayNameWithSize { get; set; }
        [DataMember(Name="unitMass")]
        public double UnitMass { get; set; }
        [DataMember(Name="unitVolume")]
        public double UnitVolume { get; set; }
        [DataMember(Name="size")]
        public string Size { get; set; }
        [DataMember(Name="iconPath")]
        public string IconPath { get; set; }
        [DataMember(Name="description")]
        public string Description { get; set; }
        [DataMember(Name="schematics")]
        public List<DuLuaSchematic> Schematics { get; set; }
    }

    public class DuLuaRecipe
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name="tier")]
        public int Tier { get; set; }
        [DataMember(Name="time")]
        public int Time { get; set; }
        [DataMember(Name="nanocraftable")]
        public bool Nanocraftable { get; set; }
        [DataMember(Name="ingredients")]
        public List<DuLuaSubItem> Ingredients { get; set; }
        [DataMember(Name="products")]
        public List<DuLuaSubItem> Products { get; set; }
    }

    public class DuLuaSubItem
    {
        [DataMember(Name="quantity")]
        public double Quantity { get; set; }
        [DataMember(Name="id")]
        public ulong Id { get; set; }
        [DataMember(Name="displayNameWithSize")]
        public string DisplayNameWithSize { get; set; }
    }

    public class FactGenRecipe
    {
        [DataMember(Name="tier")]
        public int Tier { get; set; }
        [DataMember(Name="type")]
        public string ItemType { get; set; }
        [DataMember(Name="volume")]
        public decimal Volume { get; set; }
        [DataMember(Name="outputQuantity")]
        public decimal OutputQty { get; set; }
        [DataMember(Name="time")]
        public int Time { get; set; }
        [JsonIgnore]
        [DataMember(Name="byproducts")]
        public List<Tuple<string,decimal>> Byproducts { get; set; }
        [DataMember(Name="industry")]
        public string Industry { get; set; }
        [JsonIgnore]
        [DataMember(Name="input")]
        public List<Tuple<string,decimal>> Inputs { get; set; }
    }

    public class ProductionItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}

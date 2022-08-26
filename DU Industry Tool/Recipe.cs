using Newtonsoft.Json;

using System;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Xml;
// ReSharper disable InconsistentNaming

namespace DU_Industry_Tool
{
    public class Recipe
    {
        public string Name { get; set; }
        public int Tier { get; set; }
        public string Type { get; set; }
        public double Mass { get; set; }
        public double Volume { get; set; }
        public double OutputQuantity { get; set; }
        public double Time { get; set; }
        public Dictionary<string, double> Byproducts { get; set; } = new Dictionary<string, double>();
        public List<string> Industries { get; set; } = new List<string>();
        public Dictionary<string, double> Input { get; set; } = new Dictionary<string, double>();
        public double Price { get; set; } // Buy and Sell prices are assumedly pretty much the same
    }

    public class SchematicRecipe
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public ulong id { get; set; }
        public List<ProductDetail> Products { get; set; } = new List<ProductDetail>();
        public float Time { get; set; }
        public List<ProductDetail> Ingredients { get; set; } = new List<ProductDetail>();
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
}

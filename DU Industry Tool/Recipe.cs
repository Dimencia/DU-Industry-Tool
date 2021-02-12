using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public int id { get; set; }
        public List<ProductDetail> Products { get; set; } = new List<ProductDetail>();
        public float Time { get; set; }
        public List<ProductDetail> Ingredients { get; set; } = new List<ProductDetail>();
        public Guid GroupId { get; set; }
        public string ParentGroupName { get; set; }
        public string Key { get; set; }
        [JsonIgnore]
        public TreeNode Node { get; set; }
        public ulong NqId { get; set; } // Different from Id... for markets
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


    public class TempSheet
    {
        public string Name { get; set; }
        public string RecordId { get; set; }
        public List<ProductDetail> Products { get; set; }
        public string Key { get; set; }
    }

    public class Injection
    {
        public ulong NqId { get; set; }
    }
}

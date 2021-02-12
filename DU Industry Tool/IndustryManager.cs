using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Industry_Tool
{

    public class IndustryManager
    {
        public Dictionary<string, SchematicRecipe> _recipes; // Global list of all our recipes, from the json
        public Dictionary<string, Group> Groups = new Dictionary<string, Group>();
        public List<Ore> Ores = new List<Ore>();

        public List<Talent> Talents = new List<Talent>();



        public IndustryManager(ProgressBar progressBar = null)
        {
            // On initialization, read all our data from files
            var json = File.ReadAllText("RecipesGroups.json");
            _recipes = JsonConvert.DeserializeObject<Dictionary<string, SchematicRecipe>>(json);


            if (progressBar != null)
                progressBar.Value = 20;

            json = File.ReadAllText("Groups.json");
            Groups = JsonConvert.DeserializeObject<Dictionary<string, Group>>(json);
            if (progressBar != null)
                progressBar.Value = 30;

            foreach (var recipe in _recipes.Values) // Set parent names
            {
                recipe.ParentGroupName = GetParentGroupName(recipe.GroupId);
            }

            // Populate Keys
            foreach (var kvp in _recipes)
            {
                kvp.Value.Key = kvp.Key;
                // Fix names
                var product = kvp.Value.Products.SingleOrDefault(p => p.Type == kvp.Key);
                if (product == null && kvp.Value.Products.Count > 0)
                { // Key was wrong, happens sometimes on honeycombs
                    product = kvp.Value.Products.First();
                }
                if (product != null)
                    product.Name = kvp.Value.Name;
            }
            if (progressBar != null)
                progressBar.Value = 50;
            // Populate Ores
            // Check if they have an ore values file and load that
            if (File.Exists("oreValues.json"))
            {
                Ores = JsonConvert.DeserializeObject<List<Ore>>(File.ReadAllText("oreValues.json"));
            }
            else
            {
                foreach (var recipe in _recipes.Values.Where(r => r.ParentGroupName == "Ore"))
                {
                    Ores.Add(new Ore() { Key = recipe.Key, Name = recipe.Name, Value = 25 * recipe.Level, Level = recipe.Level }); // BS some values
                }
                SaveOreValues();
            }
            if (progressBar != null)
                progressBar.Value = 60;


            // Generate Talents

            // Check if they have a talent file already to load values from instead
            if (File.Exists("talentSettings.json"))
            {
                Talents = JsonConvert.DeserializeObject<List<Talent>>(File.ReadAllText("talentSettings.json"));
            }
            else
            {
                var multiplierTalentGroups = new string[] { "Pure", "Scraps", "Product" }; // Setup the names of the item-specific multiplier talents

                List<Talent> genericScrapTalents = new List<Talent>() // Setup the scrap talents so we can add each scrap to their applicableRecipe later
                {
                    new Talent() { Name = "Basic Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Uncommon Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Advanced Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Rare Scrap Refinery", Addition = -1, InputTalent = true }
                };

                foreach (var kvp in _recipes.Where(r => multiplierTalentGroups.Any(t => t == r.Value.ParentGroupName)))
                { // Iterate over every recipe that is part of one of the multiplierTalentGroups, "Pure", "Scraps", or "Product"
                    var recipe = kvp.Value;
                    var talent = new Talent() { Name = recipe.Name + " Productivity", Addition = 0, Multiplier = 0.03 }; // They all have 3% multiplier
                    talent.ApplicableRecipes.Add(kvp.Key); // Each of these only applies to its one specific element
                    Talents.Add(talent);
                    if (recipe.ParentGroupName == "Pure" || recipe.ParentGroupName == "Product")
                    {
                        // Pures and products have an input reduction of 0.03 multiplier as well as the output multiplier
                        string nameString = recipe.Name;
                        if (recipe.ParentGroupName == "Pure")
                            nameString += " Ore";
                        talent = new Talent() { Name = nameString + " Refining", Addition = 0, Multiplier = -0.03, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        Talents.Add(talent);
                    }
                    else if (recipe.ParentGroupName == "Scraps")
                    {
                        // And scraps get a flat -1L general, and -2 specific
                        talent = new Talent() { Name = recipe.Name + " Scrap Refinery", Addition = -2, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        Talents.Add(talent);
                        if (recipe.Level < 5) // Exotics don't have one
                            genericScrapTalents[recipe.Level - 1].ApplicableRecipes.Add(kvp.Key);
                    }
                }
                Talents.AddRange(genericScrapTalents);

                // Fuel talents
                // Generic gets -2% per level to inputs
                var genericRefineryTalent = new Talent() { Name = "Fuel Refinery", Addition = 0, Multiplier = -0.02, InputTalent = true };

                foreach (var groupList in _recipes.Values.Where(r => r.ParentGroupName == "Fuels").GroupBy(r => r.GroupId))
                {
                    var groupName = Groups.Values.Where(g => g.Id == groupList.Key).FirstOrDefault()?.Name;
                    var outTalent = new Talent() { Name = groupName + " Productivity", Addition = 0, Multiplier = 0.05 };
                    var inTalent = new Talent() { Name = groupName + " Refinery", Addition = 0, Multiplier = -0.03, InputTalent = true };
                    // Specific gets -3% per level to inputs

                    // Store that everything in this group is applicable to all of these recipes (mostly catches the kergon varieties)
                    foreach (var recipe in groupList)
                    {
                        outTalent.ApplicableRecipes.Add(recipe.Key);
                        inTalent.ApplicableRecipes.Add(recipe.Key);
                        genericRefineryTalent.ApplicableRecipes.Add(recipe.Key);
                    }

                    Talents.Add(outTalent);
                    Talents.Add(inTalent);
                }
                Talents.Add(genericRefineryTalent);

                // Intermediary talents
                Talents.Add(new Talent() { Name = "Basic Intermediary Part Productivity", Addition = 1, Multiplier = 0, ApplicableRecipes = _recipes.Values.Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 1).Select(r => r.Key).ToList() });
                Talents.Add(new Talent() { Name = "Uncommon Intermediary Part Productivity", Addition = 1, Multiplier = 0, ApplicableRecipes = _recipes.Values.Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 2).Select(r => r.Key).ToList() });
                Talents.Add(new Talent() { Name = "Advanced Intermediary Part Productivity", Addition = 1, Multiplier = 0, ApplicableRecipes = _recipes.Values.Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 3).Select(r => r.Key).ToList() });

                // Ammo talents... they have  uncommon, advanced of each of xs, s, m, l
                var sizeList = new string[] { "XS", "S", "M", "L" };
                var typeList = new string[] { "Uncommon", "Advanced" };
                foreach (var type in typeList)
                    foreach (var size in sizeList)
                        Talents.Add(new Talent() { Name = type + " Ammo " + size + " Productivity", Addition = 1, Multiplier = 0, ApplicableRecipes = _recipes.Values.Where(r => r.ParentGroupName.Contains("Ammo") && r.Level == (type == "Uncommon" ? 2 : 3) && r.Name.ToLower().EndsWith(" ammo " + size.ToLower())).Select(r => r.Key).ToList() });
                SaveTalents();
            }
            if (progressBar != null)
                progressBar.Value = 100;
        }

        public void SaveTalents()
        {
            File.WriteAllText("talentSettings.json", JsonConvert.SerializeObject(Talents));
        }
        public void SaveOreValues()
        {
            File.WriteAllText("oreValues.json", JsonConvert.SerializeObject(Ores));
        }


        public List<IngredientRecipe> GetIngredientRecipes(string key, double quantity = 1)
        {
            var results = new List<IngredientRecipe>();

            double inputMultiplier = 0;
            double outputMultiplier = 0;
            double outputAdder = 0;

            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.Where(g => g.Id == recipe.GroupId).FirstOrDefault()?.Name == "Catalyst")
                return results;

            foreach (var talent in Talents.Where(t => t.ApplicableRecipes.Contains(recipe.Key)))
            {
                if (talent.InputTalent)
                {
                    inputMultiplier += talent.Multiplier * talent.Value;
                    //Console.WriteLine("Applying talent " + talent.Name + " to " + recipe.Name + " for input mult " + (talent.Multiplier * talent.Value));
                }
                else
                {
                    //Console.WriteLine("Applying talent " + talent.Name + " to " + recipe.Name + " for output mult " + (talent.Multiplier * talent.Value) + " and adder " + (talent.Addition * talent.Value));
                    outputMultiplier += talent.Multiplier * talent.Value;
                    outputAdder += talent.Addition * talent.Value;
                }
            }

            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                var resRecipe = JsonConvert.DeserializeObject<IngredientRecipe>(JsonConvert.SerializeObject(_recipes[ingredient.Type]));
                if (Groups.Values.Where(g => g.Id == resRecipe.GroupId).FirstOrDefault()?.Name != "Catalyst")
                {
                    resRecipe.Quantity = (ingredient.Quantity * inputMultiplier / (recipe.Products.First().Quantity * outputMultiplier + outputAdder)) * quantity;
                    results.Add(resRecipe);
                    if (resRecipe.Ingredients.Count > 0)
                        results.AddRange(GetIngredientRecipes(ingredient.Type, resRecipe.Quantity));
                }

            }

            return results;
        }

        public double GetBaseCost(string key)
        {
            // Just like the other one, but ignore talents.
            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.Where(g => g.Id == recipe.GroupId).FirstOrDefault()?.Name == "Catalyst")
                return 0;

            double totalCost = 0;

            var inputMultiplier = 1;
            var outputMultiplier = 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (_recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    //Console.WriteLine(ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + ingredient.Quantity + " to make " + recipe.Products.First().Quantity + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity) / recipe.Products.First().Quantity));

                    //Console.WriteLine("Talents: " + ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + (ingredient.Quantity * inputMultiplier) + " to make " + (recipe.Products.First().Quantity * outputMultiplier) + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity * inputMultiplier) / (recipe.Products.First().Quantity * outputMultiplier)));

                    totalCost += (ingredient.Quantity * inputMultiplier * Ores.Where(o => o.Key == ingredient.Type).First().Value) / (recipe.Products.First().Quantity * outputMultiplier);
                }
                else
                {
                    totalCost += GetBaseCost(ingredient.Type) * ingredient.Quantity * inputMultiplier / (recipe.Products.First().Quantity * outputMultiplier);
                }
            }

            return totalCost;
        }

        public double GetTotalCost(string key)
        {

            double totalCost = 0;
            double inputMultiplier = 1; // We start at 1x
            double outputMultiplier = 1;
            double outputAdder = 0; // And +0
            double inputAdder = 0;

            var recipe = _recipes[key];

            // Skip catalysts entirely, because they are reusable and shouldn't be included in cost
            if (Groups.Values.Where(g => g.Id == recipe.GroupId).FirstOrDefault()?.Name == "Catalyst")
                return 0;

            foreach (var talent in Talents.Where(t => t.ApplicableRecipes.Contains(recipe.Key)))
            {
                if (talent.InputTalent)
                {
                    inputMultiplier += talent.Multiplier * talent.Value; // Add each talent's multipler and adder so that we get values like 1.15 or 0.85, for pos/neg multipliers
                    inputAdder += talent.Addition * talent.Value;
                }
                else
                {
                    outputMultiplier += talent.Multiplier * talent.Value;
                    outputAdder += talent.Addition * talent.Value;
                }
            }

            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (_recipes[ingredient.Type].ParentGroupName == "Ore") // If it's an ore, add its value directly
                { // We multiply talentedQuantity * value, divided by the number of products we produce per batch, to give the value of a single one of these products
                    totalCost += ((ingredient.Quantity * inputMultiplier + inputAdder) * Ores.Where(o => o.Key == ingredient.Type).First().Value) / (recipe.Products.First().Quantity * outputMultiplier + outputAdder);
                }
                else // If it's not, get the total cost of the thing and add that
                { // Since this is the value of a single one, we multiply by the quantity we require (modified by talents), again divided by the number of products we produce, for the value of 1 of them
                    totalCost += GetTotalCost(ingredient.Type) * (ingredient.Quantity * inputMultiplier + inputAdder) / (recipe.Products.First().Quantity * outputMultiplier + outputAdder);
                }
            }

            return totalCost;
        }

        public List<ProductDetail> GetOreComponents(string key)
        { // Un-talented only

            List<ProductDetail> products = new List<ProductDetail>();
            double inputMultiplier = 0;
            double outputMultiplier = 0;

            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.Where(g => g.Id == recipe.GroupId).FirstOrDefault()?.Name == "Catalyst")
                return new List<ProductDetail>();


            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (_recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    products.Add(new ProductDetail() { Name = ingredient.Name, Quantity = ingredient.Quantity / recipe.Products[0].Quantity, Type = ingredient.Type });
                }
                else
                {
                    foreach (var result in GetOreComponents(ingredient.Type))
                    {
                        products.Add(new ProductDetail() { Name = result.Name, Quantity = (result.Quantity * ingredient.Quantity) / recipe.Products[0].Quantity, Type = result.Type });
                    }
                }
            }
            // Now flatten them into totals

            List<ProductDetail> results = new List<ProductDetail>();
            foreach (var group in products.GroupBy(p => p.Type))
            {
                results.Add(new ProductDetail() { Name = group.First().Name, Type = group.Key, Quantity = group.Sum(p => p.Quantity) });
            }
            return results;
        }


        // This was going to be recursive but those are way too generic.  We just want one parent up
        private string GetParentGroupName(Guid id)
        {
            var group = Groups.Values.Where(g => g.Id == id).FirstOrDefault();
            if (group != null)
            {
                if (group.ParentId != Guid.Empty)
                {
                    return Groups.Values.Where(g => g.Id == group.ParentId).FirstOrDefault()?.Name;
                }
                return group.Name;
            }
            return null;
        }
    }
}

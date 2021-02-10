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


            // Temporary - fill out the CSV
            /*
            if (File.Exists("Part_Cost_Calculator-Input_View.csv"))
            {
                List<TempSheet> sheetValues = new List<TempSheet>();
                using (StreamReader reader = new StreamReader("Part_Cost_Calculator-Input_View.csv"))
                {
                    reader.ReadLine(); // Skip header line

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var pieces = line.Split(',');
                        // Try to find the recipe
                        if (pieces.Length > 0)
                        {
                            var recipe = _recipes.Values.Where(r => r.Name.ToLower() == pieces[0].ToLower()).FirstOrDefault();
                            if (recipe != null)
                                sheetValues.Add(new TempSheet() { Name = pieces[0], RecordId = pieces[1], Products = GetOreComponents(recipe.Key), Key = recipe.Key });
                        }

                    }
                }

                List<string> oreNames = new List<string>()
                {
                    "Hematite","Bauxite","Coal","Quartz","Limestone","Chromite","Malachite","Natron","Acanthite","Garnierite","Pyrite","Petalite","Gold Nuggets","Cobaltite","Kolbeckite","Cryolite","Columbite","Rhodinite","Illmenite","Vanadinite","Thoramine"
                };

                using (StreamWriter writer = new StreamWriter("PartCostOutput.csv"))
                {
                    writer.WriteLine("Item Name,RecordID," + string.Join(",",oreNames));
                    foreach (var value in sheetValues)
                    {
                        writer.Write(value.Name + "," + value.RecordId + ",");
                        if (_recipes.ContainsKey(value.Key) && _recipes[value.Key].Products.Count > 0)
                        {
                            foreach (var ore in oreNames)
                            {
                                var finalValue = value.Products.Where(p => p.Name.ToLower() == ore.ToLower()).FirstOrDefault();
                                if (finalValue != null)
                                    writer.Write(finalValue.Quantity + ",");
                                else
                                    writer.Write("0,");
                            }
                        }
                        else
                        {
                            foreach (var ore in oreNames)
                                writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }
            }
            */

            // Generate Talents

            // Check if they have a talent file already to load values from instead
            if (File.Exists("talentSettings.json"))
            {
                Talents = JsonConvert.DeserializeObject<List<Talent>>(File.ReadAllText("talentSettings.json"));
            }
            else
            {
                foreach (var group in _recipes.Values.GroupBy(r => r.ParentGroupName))
                {
                    Console.WriteLine(group.Key);
                }
                var multiplierTalentGroups = new string[] { "Pure", "Scraps", "Product" };

                List<Talent> genericScrapTalents = new List<Talent>()
                {
                    new Talent() { Name = "Basic Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Uncommon Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Advanced Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Rare Scrap Refinery", Addition = -1, InputTalent = true }
                };

                foreach (var kvp in _recipes.Where(r => multiplierTalentGroups.Any(t => t == r.Value.ParentGroupName)))
                {
                    var recipe = kvp.Value;
                    var talent = new Talent() { Name = recipe.Name + " Productivity", Addition = 0, Multiplier = 0.03 };
                    talent.ApplicableRecipes.Add(kvp.Key); // Each of these only applies to its one thing... 
                    Talents.Add(talent);
                    if (recipe.ParentGroupName == "Pure" || recipe.ParentGroupName == "Product")
                    {
                        // Pures and products have an input rediction of 0.03 multiplier
                        talent = new Talent() { Name = recipe.Name + " Ore Refining", Addition = 0, Multiplier = -0.03, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        Talents.Add(talent);
                    }
                    else if (recipe.ParentGroupName == "Scraps")
                    {
                        // And scraps get a flat -1L general, and -2 specific
                        talent = new Talent() { Name = recipe.Name + " Scrap Refinery", Addition = -2, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        if (recipe.Level < 5) // Exotics don't have one
                            genericScrapTalents[recipe.Level - 1].ApplicableRecipes.Add(kvp.Key);
                    }
                }

                // Fuel talents
                var genericRefineryTalent = new Talent() { Name = "Fuel Refinery", Addition = 0, Multiplier = -0.02, InputTalent = true };

                foreach (var groupList in _recipes.Values.Where(r => r.ParentGroupName == "Fuels").GroupBy(r => r.GroupId))
                {
                    var groupName = Groups.Values.Where(g => g.Id == groupList.Key).FirstOrDefault()?.Name;
                    var outTalent = new Talent() { Name = groupName + " Productivity", Addition = 0, Multiplier = 0.05 };
                    var inTalent = new Talent() { Name = groupName + " Refinery", Addition = 0, Multiplier = -0.03, InputTalent = true };

                    // Fuels also have a -3% input cost set of talents for each group

                    // We also need to store what things are applicable to these talents
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

        public double GetTotalCost(string key)
        {

            double totalCost = 0;
            double inputMultiplier = 0;
            double outputMultiplier = 0;
            double outputAdder = 0;

            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.Where(g => g.Id == recipe.GroupId).FirstOrDefault()?.Name == "Catalyst")
                return 0;

            foreach (var talent in Talents.Where(t => t.ApplicableRecipes.Contains(recipe.Key)))
            {
                if (talent.InputTalent)
                {
                    inputMultiplier += talent.Multiplier * talent.Value;
                    Console.WriteLine("Applying talent " + talent.Name + " to " + recipe.Name + " for input mult " + (talent.Multiplier * talent.Value));
                }
                else
                {
                    Console.WriteLine("Applying talent " + talent.Name + " to " + recipe.Name + " for output mult " + (talent.Multiplier * talent.Value) + " and adder " + (talent.Addition * talent.Value));
                    outputMultiplier += talent.Multiplier * talent.Value;
                    outputAdder += talent.Addition * talent.Value;
                }
            }

            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (_recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    Console.WriteLine(ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + ingredient.Quantity + " to make " + recipe.Products.First().Quantity + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity) / recipe.Products.First().Quantity));

                    Console.WriteLine("Talents: " + ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + (ingredient.Quantity * inputMultiplier) + " to make " + (recipe.Products.First().Quantity * outputMultiplier) + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity * inputMultiplier) / (recipe.Products.First().Quantity * outputMultiplier + outputAdder)));

                    totalCost += (ingredient.Quantity * inputMultiplier * Ores.Where(o => o.Key == ingredient.Type).First().Value) / (recipe.Products.First().Quantity * outputMultiplier + outputAdder);
                }
                else
                {
                    totalCost += GetTotalCost(ingredient.Type) * ingredient.Quantity * inputMultiplier / (recipe.Products.First().Quantity * outputMultiplier + outputAdder);
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

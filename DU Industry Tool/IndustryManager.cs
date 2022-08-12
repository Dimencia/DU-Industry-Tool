using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DU_Industry_Tool
{
    public class IndustryManager
    {
        private readonly List<string> _sizeList  = new List<string> { "XS", "S", "M", "L", "XL" };
        private SortedDictionary<string, double> _sumProducts; // Sums for each individual ingredient Product
        private SortedDictionary<string, double> _sumPures; // Sums for each individual ingredient Pure
        private SortedDictionary<string, double> _sumOres; // Sums for each individual ingredient Ore
        private SortedDictionary<string, Tuple<int,double>> _sumSchemClass; // Sums for each schematic class
        private double _schematicsCost = 0;

        public SortedDictionary<string, SchematicRecipe> _recipes; // Global list of all our recipes, from the json
        public readonly Dictionary<string, Group> Groups;
        public List<Ore> Ores { get; } = new List<Ore>();
        public SortedDictionary<string, Schematic> Schematics { get; } = new SortedDictionary<string, Schematic>();

        public List<Talent> Talents { get; } = new List<Talent>();

        public List<string> Groupnames { get; } = new List<string>(200);

        public StringBuilder CostResults { get; private set; }

        public int ProductQuantity { get; set; } = 10;

        public IndustryManager(ProgressBar progressBar = null)
        {
            if (!File.Exists("RecipesGroups.json") || !File.Exists("Groups.json"))
            {
                return;
            }

            // On initialization, read all our data from files
            var json = File.ReadAllText("RecipesGroups.json");
            _recipes = JsonConvert.DeserializeObject<SortedDictionary<string, SchematicRecipe>>(json);

            if (progressBar != null)
                progressBar.Value = 20;

            json = File.ReadAllText("Groups.json");
            Groups = JsonConvert.DeserializeObject<Dictionary<string, Group>>(json);
            if (progressBar != null)
                progressBar.Value = 30;

            if (_recipes?.Any() == true)
            {
                foreach (var recipe in _recipes.Values) // Set parent names
                {
                    recipe.ParentGroupName = GetParentGroupName(recipe.GroupId);
                    if (!Groupnames.Contains(recipe.ParentGroupName))
                        Groupnames.Add(recipe.ParentGroupName);
                }
            }
            Groupnames.Sort();
            if (progressBar != null)
                progressBar.Value = 30;

            // Schematics and prices
            // mostly by formula as of patch 0.31.3

            var csPriceList = new int[] { 150,  375,  1000,   3000, 0 }; // Construct Support
            var cuPriceList = new int[] { 250, 5000, 62500, 725000, 0 }; // Core Units

            if (File.Exists("schematicValues.json"))
            {
                Schematics = JsonConvert.DeserializeObject<SortedDictionary<string, Schematic>>(File.ReadAllText("schematicValues.json"));
            }
            else
            {
                // Generate:
                // U  = Pures
                // P  = Products
                // HU = Honeycomb Pures
                // HP = Honeycomb Products
                // SC = Scraps
                var groups  = new[] {       "U",     "P",  "HU", "HP", "SC" };
                var prices  = new double[] { 187.5d, 180d, 125d, 50d,  30d }; // only first price for each group
                var scraps  = new double[] { 30d, 75d, 187.48d, 468.72 };
                var factors = new double[] { 3.125d, 3.3333333d, 3.75d, 5.0d };
                for(byte i = 0; i <= 4; i++)
                {
                    var g = groups[i];
                    var title = g == "SC" ? "Scrap" : (g.EndsWith("U") ? "Pure" : "Product");
                    title += g.StartsWith("H") ? " Honeycomb" : (g == "SC" ? "" :" Material");
                    var price = g == "SC" ? scraps[i-1] : prices[i];
                    for(byte tier = 1; tier <= 5; tier++)
                    {
                        if ((g == "SC" || g.EndsWith("U")) && tier < 2) continue; // no schematics for T1 pures/scraps/hc-pures
                        var schem = new Schematic {
                            Name = $"Tier {tier} {title} Schematic",
                            Key = $"T{tier}{g}",
                            Level = tier,
                            Cost = Math.Round(price, 2)
                        };
                        Schematics.Add(schem.Key, schem);
                        if (tier < 5)
                        {
                            price *= (g.StartsWith("H") ? 2.5d : factors[tier-1]);
                        }
                    }
                }

                // Element schematics
                prices  = new double[] { 375d, 937.5d, 2343.7d, 5859.3d, 14648.4d };
                factors = new double[] { 4d, 3.3333333d, 3d, 4d };
                for(byte tier = 1; tier <= 5; tier++)
                {
                    var price = prices[tier-1];
                    for(byte size = 0; size < _sizeList.Count; size++)
                    {
                        var schem = new Schematic {
                            Name  = $"Tier {tier} {_sizeList[size]} Element Schematic",
                            Key   = $"T{tier}E{_sizeList[size]}",
                            Level = tier,
                            Cost  = Math.Round(price, 2)
                        };
                        Schematics.Add(schem.Key, schem);
                        if (size < 4)
                        {
                            price = Math.Round(price * factors[size], 2);
                        }
                    }
                }

                // Ammo schematics
                prices = new double[] { 600d, 1500d, 3750d };
                for(byte i = 2; i <= 4; i++) // Ammo T2 - T4, XS - L
                {
                    var price = prices[i - 2];
                    foreach (var size in _sizeList)
                    {
                        if (size == "XL") continue;
                        var key = $"T{i}A{size}";
                        Schematics.Add(key, new Schematic()
                        {
                            Key = key,
                            Name = $"Tier {i} Ammo {size} Schematic",
                            Level = i,
                            Cost = price
                        });
                        price *= 2;
                    }
                }

                // Fuel schematics
                var schema = new Schematic {
                    Name = "Atmospheric Fuel Schematic",
                    Key = "AtmoFuel",
                    Level = 1,
                    Cost = 60
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic {
                    Name = "Space Fuels Schematic",
                    Key = "SpaceFuels",
                    Level = 0,
                    Cost = 150
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic {
                    Name = "Rocket Fuels Schematic",
                    Key = "RocketFuels",
                    Level = 0,
                    Cost = 375
                };
                Schematics.Add(schema.Key, schema);
                // Standalone schematics
                schema = new Schematic {
                    Name = "Bonsai Schematic",
                    Key = "Bonsai",
                    Level = 0,
                    Cost = 10000000
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic {
                    Name = "Territory Unit Schematic",
                    Key = "TerritoryUnit",
                    Level = 0,
                    Cost = 20000
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic {
                    Name = "Warp Cell Schematic",
                    Key = "WarpCell",
                    Level = 3,
                    Cost = 5000
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic {
                    Name = "Warp Beacon Schematic",
                    Key = "WarpBeacon",
                    Level = 0,
                    Cost = 5000000
                };
                Schematics.Add(schema.Key, schema);

                // Add Construct Support and Core Units schematics
                for (var idx = 0; idx <= 3; idx++) // XS - L
                {
                    var size = _sizeList[idx];
                    // Construct Support
                    var key = "CS-"+size;
                    Schematics.Add(key, new Schematic()
                    {
                        Name = $"Construct Support {size} Schematic",
                        Key = key,
                        Cost = csPriceList[idx]
                    });
                    // Core Units
                    key = "CU-"+size;
                    Schematics.Add(key, new Schematic()
                    {
                        Name = $"Core Unit {size} Schematic",
                        Key = key,
                        Cost = cuPriceList[idx]
                    });
                }
                SaveSchematicValues();
            }
            if (progressBar != null)
                progressBar.Value = 50;

            // Populate Keys, fix products and assign schematics
            int ucount = 0;
            int pcount = 0;
            foreach (var kvp in _recipes)
            {
                kvp.Value.Key = kvp.Key;
                // Fix names and some other details
                if (kvp.Value.Name.Equals("Territory Unit", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Value.Name.Equals("Territory Scanner", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Value.Name.Equals("Sanctuary Territory Unit", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.Name += " XL";
                }
                if (kvp.Value.Name.Equals("Anti-Gravity Pulsor", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.Name += " M";
                }
                if (kvp.Value.Name.Equals("Emergency controller", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Value.Name.Equals("Programming board", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.Name += " XS";
                }
                if (kvp.Key.Equals("AmmoMissileExtraSmallAntimatterUncommon", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.Level = 3;
                }
                if (kvp.Key.Equals("TerritoryUnitBasicSanctuary", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.Level = 5;
                }
                var product = kvp.Value.Products.SingleOrDefault(p => p.Type == kvp.Key);
                if (product == null && kvp.Value.Products.Count > 0)
                {
                    // Key was wrong, happens for many honeycombs
                    product = kvp.Value.Products.First();
                    //Console.WriteLine(kvp.Value.Name +" key fixed.");
                }
                if (product != null)
                {
                    product.Name = kvp.Value.Name;
                }

                if (string.IsNullOrEmpty(kvp.Value.ParentGroupName))
                {
                    if (kvp.Key.IndexOf("shield", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        kvp.Value.ParentGroupName = "Shield Generators";
                        Console.WriteLine(kvp.Value.Name + ": ParentGroupName fixed!");
                    }
                    else
                    {
                        Console.WriteLine(kvp.Value.Name + ": ParentGroupName missing!?");
                        kvp.Value.ParentGroupName = "INVALID";
                        continue;
                    }
                }

                if (kvp.Key.StartsWith("Catalyst") ||
                    kvp.Value.ParentGroupName == "Ore" ||
                    (kvp.Value.ParentGroupName == "Pure" && kvp.Value.Level <= 1))
                {
                    kvp.Value.SchemaPrice = 0;
                    kvp.Value.SchemaType = null;
                    continue;
                }
                // Preset "ParentGroupName" and "GroupId" for containers
                if (kvp.Key.IndexOf("chemicalcont", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    kvp.Value.ParentGroupName = "Functional Parts";
                    kvp.Value.GroupId = new Guid("08d8a31f-5127-4f25-8138-779a7f0e5c8d");
                }
                else if (kvp.Key.StartsWith("AmmoContainer", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4fcc-40fe-8e92-9fd90394d5c2");
                }
                else if (kvp.Key.StartsWith("MissionContainer", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4fcc-40fe-8e92-9fd90394d5c3");
                }
                else if (kvp.Key.IndexOf("singularitycontainer", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    kvp.Value.ParentGroupName = "Complex parts";
                    kvp.Value.GroupId = new Guid("08d8a31f-5116-4472-84d8-1cf2de11b3a3");
                }
                else if (kvp.Key.IndexOf("container", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4ff1-4b54-8484-9d05d3885b52");
                }

                // IF schematic is already assigned, skip further processing
                if (!string.IsNullOrEmpty(kvp.Value.SchemaType) && kvp.Value.SchemaPrice > 0)
                {
                    continue;
                }
                // Skip entries without level and Catalysts
                if (kvp.Value.Level < 1 ||
                    kvp.Value.Name.StartsWith("Catalyst", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var idx = "";
                var isElement = false;
                var parentName = kvp.Value.ParentGroupName;

                if (kvp.Key.IndexOf("deep space asteroid", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    var r = Schematics.First(x => x.Key == "T1EXL");
                    kvp.Value.SchemaType = r.Key;
                    kvp.Value.SchemaPrice = r.Value.Cost;
                    continue;
                }
                if (kvp.Key.Equals("WarpCellStandard", StringComparison.InvariantCultureIgnoreCase))
                {
                    var r = Schematics.First(x => x.Key == "WarpCell");
                    kvp.Value.SchemaType = r.Key;
                    kvp.Value.SchemaPrice = r.Value.Cost;
                    continue;
                }
                if (kvp.Key.Equals("WarpBeacon", StringComparison.InvariantCultureIgnoreCase))
                {
                    var r = Schematics.First(x => x.Key == "WarpBeacon");
                    kvp.Value.SchemaType = r.Key;
                    kvp.Value.SchemaPrice = r.Value.Cost;
                    continue;
                }
                if (kvp.Key.Equals("Bonsai", StringComparison.InvariantCultureIgnoreCase))
                {
                    var r = Schematics.First(x => x.Key == "Bonsai");
                    kvp.Value.Level = 5;
                    kvp.Value.SchemaType = r.Key;
                    kvp.Value.SchemaPrice = r.Value.Cost;
                    continue;
                }
                // Core Units
                if (parentName != null && parentName.Equals("Core Units", StringComparison.InvariantCultureIgnoreCase))
                {
                    var size = GetElementSize(kvp.Value.Name);
                    idx = "CU-" + size;
                    kvp.Value.SchemaType = idx;
                    kvp.Value.SchemaPrice = cuPriceList[_sizeList.IndexOf(size)];
                    continue;
                }
                // Ammo
                if (kvp.Value.Level > 1 && parentName != null &&
                    parentName.IndexOf(" ammo", StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    idx = "A" + GetElementSize(kvp.Value.Name);
                }

                if (idx == "")
                {
                    var skipGroups = new List<string>
                    {
                        "Displays",
                        "Decorative Element",
                        " Parts",
                        "Logic Operators",
                        //"Furniture", ??
                    };
                    if (parentName != null && skipGroups.Any(x => parentName.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    if (idx == "")
                    {
                        var upGrp = GetTopLevelGroup(kvp.Value.ParentGroupName);
                        if (upGrp == "Elements")
                        {
                            isElement = true;
                        }
                    }

                    var elemGroups = new List<string>
                    {
                        "Atmospheric Brakes",
                        "Atmospheric Engines",
                        "Cannons",
                        "Containers",
                        "Control Units",
                        "Electronics",
                        "Engines",
                        //"Furniture & Appliances",
                        "Ground Engines",
                        "High-Tech Transportation",
                        "Industry",
                        "Laser Ammo M",
                        "Lasers",
                        "Mining Units",
                        "Missile Pods",
                        "Radar",
                        "Railguns",
                        "Relic Plasmas",
                        "Screens",
                        "Sensors",
                        "Shield Generators",
                        "Space Brakes",
                        "Space Engines",
                        "Space Radars",
                        "Support Tech",
                        "Triggers",
                    };

                    // Construct Support
                    var csGroups = new List<string>
                    {
                        "Adjustor",
                        "Aileron",
                        "Compact Aileron",
                        "Stabilizer",
                        "Wing",
                    };
                    if (csGroups.Any(x => kvp.Value.Name.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var size = GetElementSize(kvp.Value.Name);
                        idx = "CS-" + size;
                        kvp.Value.SchemaType = idx;
                        kvp.Value.SchemaPrice = csPriceList[_sizeList.IndexOf(size)];
                        continue;
                    }

                    if (isElement ||
                        elemGroups.Any(x => x.Equals(parentName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        idx = "E" + GetElementSize(kvp.Value.Name);
                    }
                    else
                    if (parentName.Equals("Product", StringComparison.InvariantCultureIgnoreCase))
                    {
                        idx = "P";
                    }
                    else
                    if ((parentName.Equals("Ore", StringComparison.InvariantCultureIgnoreCase) ||
                         parentName.Equals("Pure", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (kvp.Value.Level <= 1)
                        {
                            kvp.Value.SchemaType = null;
                            kvp.Value.SchemaPrice = 0;
                            continue; // no T1
                        }
                        idx = "U";
                    }
                    else
                    if (parentName.Equals("Scraps", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (kvp.Value.Level <= 1)
                        {
                            kvp.Value.SchemaType = null;
                            kvp.Value.SchemaPrice = 0;
                            continue; // no T1
                        }
                        idx = "SC";
                    }
                    else
                    if (parentName.StartsWith("Product Honeycomb", StringComparison.InvariantCultureIgnoreCase))
                    {
                        idx = "HP";
                    }
                    else
                    if (parentName.StartsWith("Pure Honeycomb", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (kvp.Value.Level <= 1)
                        {
                            kvp.Value.SchemaType = null;
                            kvp.Value.SchemaPrice = 0;
                            continue; // no T1
                        }
                        idx = "HU";
                    }
                    else
                    if (parentName == "Fuels")
                    {
                        if (kvp.Key.StartsWith("Kergon", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "SpaceFuels");
                            kvp.Value.SchemaType  = schemaClass.Key;
                            kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        }
                        else if (kvp.Key.Equals("Nitron", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "AtmoFuel");
                            kvp.Value.SchemaType  = schemaClass.Key;
                            kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        }
                        else if (kvp.Key.Equals("Xeron", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "RocketFuels");
                            kvp.Value.SchemaType  = schemaClass.Key;
                            kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        }
                        continue;
                    }
                }

                // Apply schematic price if T1+ product or T2+ pure
                if (idx != ""
                    && kvp.Value.ParentGroupName != "Decorative Element"
                    && !kvp.Value.ParentGroupName.EndsWith(" Parts", StringComparison.InvariantCultureIgnoreCase))
                {
                    var schemaClass = Schematics.FirstOrDefault(x => x.Key == $"T{kvp.Value.Level}{idx}");
                    if (schemaClass.Value != null)
                    {
                        kvp.Value.SchemaType  = schemaClass.Key;
                        kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        Console.WriteLine($"{kvp.Value.Name} = {kvp.Value.SchemaPrice} ({parentName})");
                        ucount++;
                    }
                }
                else
                if (kvp.Value.ParentGroupName == "Refined Materials" ||
                    (kvp.Value.Level >= 1 && kvp.Value.Name.EndsWith("product", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var schemaClass = Schematics.FirstOrDefault(x => x.Key == $"T{kvp.Value.Level}P");
                    if (schemaClass.Value != null)
                    {
                        kvp.Value.SchemaType  = schemaClass.Key;
                        kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        Console.WriteLine($"{kvp.Value.Name} = {kvp.Value.SchemaPrice} ({parentName})");
                        pcount++;
                    }
                }
            }

            SaveRecipes();

            if (progressBar != null)
                progressBar.Value = 70;

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
            }
            // Add plasmas (if missing) to the ore list so a cost can be assigned
            var plasmas = new List<string>
            {
                "Relic Plasma Unus l",
                "Relic Plasma Duo l",
                "Relic Plasma Tres l",
                "Relic Plasma Quattuor l",
                "Relic Plasma Quinque l",
                "Relic Plasma Sex l",
                "Relic Plasma Septem l",
                "Relic Plasma Octo l",
                "Relic Plasma Novem l",
                "Relic Plasma Decem l"
            };
            var changed = false;
            for (var i = 1; i <= 10; i++)
            {
                var plasmaKey = $"Plasma{i}";
                if (Ores.Exists(x => x.Key == plasmaKey))
                    continue;
                Ores.Add(new Ore() { Key = plasmaKey,
                    Name = plasmas[i-1],
                    Value = 10000000,
                    Level = 0
                });
                changed = true;
            }
            if (changed)
            {
                SaveOreValues();
            }
            if (progressBar != null)
                progressBar.Value = 80;

            // Generate Talents

            // Check if they have a talent file already to load values from instead
            if (File.Exists("talentSettings.json"))
            {
                Talents = JsonConvert.DeserializeObject<List<Talent>>(File.ReadAllText("talentSettings.json"));
            }
            else
            {
                var multiplierTalentGroups = new[] { "Pure", "Scraps", "Product" }; // Setup the names of the item-specific multiplier talents

                var genericScrapTalents = new List<Talent>() // Setup the scrap talents so we can add each scrap to their applicableRecipe later
                {
                    new Talent() { Name = "Basic Scrap Refinery",    Addition = -1, InputTalent = true },
                    new Talent() { Name = "Uncommon Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Advanced Scrap Refinery", Addition = -1, InputTalent = true },
                    new Talent() { Name = "Rare Scrap Refinery",     Addition = -1, InputTalent = true }
                };

                foreach (var kvp in _recipes.Where(r => multiplierTalentGroups.Any(t => t == r.Value.ParentGroupName)))
                {
                    // Iterate over every recipe that is part of one of the multiplierTalentGroups, "Pure", "Scraps", or "Product"
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
                    var groupName = Groups.Values.FirstOrDefault(g => g.Id == groupList.Key)?.Name;
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
                var typeList = new string[] { "Uncommon", "Advanced" };
                foreach (var type in typeList)
                foreach (var size in _sizeList)
                {
                    if (size == "XL") continue;
                    Talents.Add(new Talent()
                    {
                        Name = type + " Ammo " + size + " Productivity", Addition = 1, Multiplier = 0,
                        ApplicableRecipes = _recipes.Values
                            .Where(r => r.ParentGroupName.Contains("Ammo") && r.Level == (type == "Uncommon" ? 2 : 3) &&
                                        r.Name.ToLower().EndsWith(" ammo " + size.ToLower())).Select(r => r.Key)
                            .ToList()
                    });
                }

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
        public void SaveRecipes()
        {
            File.WriteAllText("RecipesGroups.json", JsonConvert.SerializeObject(_recipes));
        }
        public void SaveSchematicValues()
        {
            File.WriteAllText("schematicValues.json", JsonConvert.SerializeObject(Schematics));
        }

        private string GetElementSize(string elemName)
        {
            foreach (var size in _sizeList)
            {
                if (elemName.EndsWith(" "+size, StringComparison.InvariantCultureIgnoreCase))
                {
                    return size;
                }
            }
            return "";
        }

        private string FindParent(Guid groupId)
        {
            if (groupId == Guid.Empty) return "";
            var grp = Groups.FirstOrDefault(x => x.Value.Id == groupId);
            if (grp.Value == null || grp.Key == "" || grp.Key == "FurnituresAppliances") return "";
            if (grp.Key == "Ammo" || grp.Value.Name == "Elements") return "Elements";
            if (grp.Value.ParentId == Guid.Empty) return "";
            return FindParent(grp.Value.ParentId);
        }

        private string GetTopLevelGroup(string groupName)
        {
            if (groupName == "" || groupName.IndexOf("honeycomb", StringComparison.InvariantCultureIgnoreCase) >= 0) return "";
            var grp = Groups.FirstOrDefault(x => x.Value.Name == groupName);
            if (string.IsNullOrEmpty(grp.Key) || grp.Value.ParentId == Guid.Empty) return "";
            var tmp = FindParent(grp.Value.ParentId);
            return tmp;
        }

        public List<IngredientRecipe> GetIngredientRecipes(string key, double quantity = 1)
        {
            var results = new List<IngredientRecipe>();

            double inputMultiplier = 0;
            double outputMultiplier = 0;
            double outputAdder = 0;

            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.FirstOrDefault(g => g.Id == recipe.GroupId)?.Name == "Catalyst")
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
                if (resRecipe == null)
                    continue;

                var entry = Groups.Values.FirstOrDefault(g => resRecipe != null && g.Id == resRecipe.GroupId);
                if (entry?.Name == null || entry.Name == "Catalyst")
                    continue;

                resRecipe.Quantity = (ingredient.Quantity * inputMultiplier / (recipe.Products.First().Quantity * outputMultiplier + outputAdder)) * quantity;
                results.Add(resRecipe);
                if (resRecipe.Ingredients.Count > 0)
                    results.AddRange(GetIngredientRecipes(ingredient.Type, resRecipe.Quantity));
            }

            return results;
        }

        public double GetBaseCost(string key)
        {
            // Just like the other one, but ignore talents.
            if (!_recipes.Keys.Contains(key))
                return 0;
            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.FirstOrDefault(g => g.Id == recipe.GroupId)?.Name == "Catalyst")
                return 0;

            double totalCost = 0;

            var inputMultiplier = 1;
            var outputMultiplier = 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_recipes.Keys.Contains(ingredient.Type))
                {
                    Console.WriteLine($"Schematic {ingredient.Type} not found!");
                    continue;
                }
                if (_recipes.Keys.Contains(ingredient.Type) && _recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    //Console.WriteLine(ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + ingredient.Quantity + " to make " + recipe.Products.First().Quantity + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity) / recipe.Products.First().Quantity));
                    //Console.WriteLine("Talents: " + ingredient.Name + " value: " + Ores.Where(o => o.Key == ingredient.Type).First().Value + "; Requires " + (ingredient.Quantity * inputMultiplier) + " to make " + (recipe.Products.First().Quantity * outputMultiplier) + " for a total of " + ((Ores.Where(o => o.Key == ingredient.Type).First().Value * ingredient.Quantity * inputMultiplier) / (recipe.Products.First().Quantity * outputMultiplier)));
                    totalCost += (ingredient.Quantity * inputMultiplier * Ores.First(o => o.Key == ingredient.Type).Value) /
                                 (recipe.Products.First().Quantity * outputMultiplier);
                }
                else
                {
                    totalCost += GetBaseCost(ingredient.Type) * ingredient.Quantity * inputMultiplier /
                                 (recipe.Products.First().Quantity * outputMultiplier);
                }
            }
            return totalCost;
        }

        // Hold for each Product and Pure the Litres sum to afterwards calculate the needed schematics.
        // Example: if in the end we need 380 L of Product "Polycalcite", we need to find out
        // the required amount of schematics for each Calcium and Polycalcite
        // (with an avatar having all T5 skills).
        // Polycalcite:  per batch: Input 85.00 L Calcium,   Output: 86.25 L
        // -> 380/86.25 = 4.40 batches -> rounded up to 5 schematics for the Polycalcite (Chemical).
        // Pure Calcium: per batch: Input 55.25 L Limestone, Output: 51.75 L
        // So for above 5 batches we need to calculate the number of needed Calcium batches (Refiner):
        // -> 5 * 85 L = 425 L total -> 425 L / 51.75 L = 8.21 -> rounded to 9 schematics for the Calcium.
        // The refiner requires Tier 2 Pure schematics, the chemical Tier 2 Product schematics.
        // -> Refiner  187.50 q * 9 = 1687.50 q
        // -> Chemical 562.50 q * 5 = 2812,50 q
        // -> SUM for schematics: 4500 quanta
        // :grimace:

        public double GetTotalCost(string key, double amount = 0, string level = "", int depth = 0, bool silent = false)
        {
            if (depth == 0)
            {
                amount = ProductQuantity;
                CostResults     = new StringBuilder();
                _sumProducts    = new SortedDictionary<string, double>();
                _sumPures       = new SortedDictionary<string, double>();
                _sumOres        = new SortedDictionary<string, double>();
                _sumSchemClass  = new SortedDictionary<string, Tuple<int,double>>();
                _schematicsCost = 0;
            }
            double totalCost = 0;
            if (key.StartsWith("Hydrogen") || key.StartsWith("Oxygen") || key.StartsWith("Catalyst"))
                return 0;

            if (!_recipes.Keys.Contains(key))
            {
                Debug.WriteLine($"*** MISSING: {key} !!!");
                return 0;
            }
            var recipe  = _recipes[key];
            var product = recipe.Products?.FirstOrDefault();
            if (product == null) return 0;
            if (recipe.Ingredients == null)
            {
                return 0;
            }

            GetTalentsForKey(recipe.Key, out var inputMultiplier, out var inputAdder, out var outputMultiplier, out var outputAdder);

            var productQty = product.Quantity;
            var curLevel = level;
            level += "     ";
            Debug.WriteLineIf(!silent, $"{curLevel}> {product.Name} ({amount:N2})");

            inputMultiplier  += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrEmpty(ingredient.Type) || !_recipes.ContainsKey(ingredient.Type))
                {
                    Debug.WriteLine($"{curLevel}     MISSING: {recipe.Key}->{ingredient.Name} !!!");
                    continue;
                }

                var cost     = 0d;
                var qty      = 0d;
                double factor = 1;
                var myRecipe = _recipes[ingredient.Type];
                if (depth > 0 && ingredient.Quantity > productQty && myRecipe.ParentGroupName == "Ore")
                {
                    factor = ((productQty + outputAdder) * outputMultiplier) /
                              ((ingredient.Quantity + inputAdder) * inputMultiplier);
                }
                else
                {
                    factor = ((ingredient.Quantity + inputAdder) * inputMultiplier) /
                             ((productQty + outputAdder) * outputMultiplier);
                }
                var isPlasma = myRecipe.ParentGroupName == "Consumables" && myRecipe.Key.StartsWith("Plasma");
                if (myRecipe.ParentGroupName == "Ore" || isPlasma)
                {
                    qty = amount / factor;
                    cost = qty * Ores.First(o => o.Key == ingredient.Type).Value;
                    totalCost += cost;
                    var prefix = ingredient.Type;
                    if (isPlasma)
                    {
                        qty = 1;
                    }
                    else
                    {
                        prefix = "T"+(myRecipe.Level < 2 ? "1" : myRecipe.Level.ToString())+" "+prefix;
                    }
                    if (_sumOres.ContainsKey(prefix))
                        _sumOres[prefix] += qty;
                    else
                        _sumOres.Add(prefix, qty);
                    Debug.WriteLineIf(!silent && qty > 0, $"{curLevel}     ({ingredient.Name}: {qty:N2} = {cost:N2}q)");
                    continue;
                }

                if (myRecipe.ParentGroupName == "Pure")
                {
                    qty = amount / factor;
                    cost = GetTotalCost(ingredient.Type, qty, level, depth + 1, silent);
                    totalCost += cost;
                    if (!string.IsNullOrEmpty(myRecipe.SchemaType))
                    {
                        if (_sumPures.ContainsKey(ingredient.Type))
                            _sumPures[ingredient.Type] += qty;
                        else
                            _sumPures.Add(ingredient.Type, qty);
                    }
                    continue;
                }

                // Any other part/product
                qty = depth < 1 ? factor : factor * amount;
                cost = GetTotalCost(ingredient.Type, qty, level, depth+1, silent);
                totalCost += cost;
                Debug.WriteLineIf(!silent && cost > 0, $"{curLevel}     = {cost:N2}q");

                if (string.IsNullOrEmpty(myRecipe.SchemaType))
                    continue;
                if (_sumProducts.ContainsKey(ingredient.Type))
                    _sumProducts[ingredient.Type] += qty;
                else
                    _sumProducts.Add(ingredient.Type, qty);
            }

            if (depth != 0) return totalCost;

            // Only on top-level we calculate total cost for all schematics for each ingredient
            CostResults.AppendLine($"Cost for {amount} {recipe.Name}");
            if (!string.IsNullOrEmpty(recipe.SchemaType))
            {
                var schemaPrice = amount * recipe.SchemaPrice;
                _schematicsCost += schemaPrice;
                CostResults.AppendLine($"Item schematic(s) {recipe.SchemaType}: {schemaPrice:N1}q");
            }

            CostResults.AppendLine("Totals:");
            _schematicsCost += CalculateItemCost(_sumOres, "U", 1, amount);
            _schematicsCost += CalculateItemCost(_sumPures, "U", 2, amount);
            _schematicsCost += CalculateItemCost(_sumProducts, "P", 1, amount);
            CostResults.AppendLine("Schematics:".PadRight(16)+$"{_schematicsCost:N1}q".PadLeft(20));
            totalCost *= amount;
            CostResults.AppendLine("Pures/Products:".PadRight(16)+$"{totalCost:N1}q".PadLeft(20));
            totalCost += _schematicsCost;
            var costx1 = totalCost / amount;
            CostResults.AppendLine("Total Cost:".PadRight(16)+$"{totalCost:N1}q".PadLeft(20));
            CostResults.AppendLine("Cost x1:".PadRight(16)+$"{costx1:N1}q".PadLeft(20));
            Debug.WriteLineIf(!silent, CostResults.ToString());

            CostResults.AppendLine();
            CostResults.AppendLine("Schematics details:");
            foreach (var schem in _sumSchemClass)
            {
                CostResults.AppendLine(schem.Key.PadRight(4)+
                                       $"x{schem.Value.Item1:N0}".PadLeft(11)+
                                       $"{schem.Value.Item2:N1}q".PadLeft(21));
            }
            return costx1;
        }

        private void GetTalentsForKey(string key, out double inputMultiplier, out double inputAdder, out double outputMultiplier, out double outputAdder)
        {
            inputMultiplier = 1;
            inputAdder = 0;
            outputMultiplier = 1;
            outputAdder = 0;
            foreach (var talent in Talents.Where(t => t.ApplicableRecipes.Contains(key)))
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
        }

        private double CalculateItemCost(SortedDictionary<string, double> itemPrices, string listType = "P",
                                      byte minLevel = 1, double quantity = 1, bool outputDetails = true)
        {
            double outSum = 0;
            foreach (var item in itemPrices)
            {
                var key = item.Key;
                if (listType == "U" && key.StartsWith("T"))
                {
                    key = key.Substring(3);
                }
                if (!_recipes.ContainsKey(key))
                {
                    continue;
                }
                var recipe   = _recipes[key];
                var isOre    = recipe.ParentGroupName == "Ore";
                var isT1Ore  = listType == "U" && isOre && recipe?.Level <= 1;
                var isPlasma = recipe.ParentGroupName == "Consumables" && recipe.Key.StartsWith("Plasma");
                if (listType == "U" && !isOre && recipe.Level < minLevel)
                {
                    continue;
                }

                double orePrice = 0;
                if (listType == "U" && isOre)
                {
                    orePrice = Ores.FirstOrDefault(o => o.Key == key)?.Value ?? 0;
                }
                double tmp = item.Value * quantity;
                var output = $"{recipe.Name}: {tmp:N2}"+(isPlasma ? "" : " L");
                if (isOre && orePrice > 0.00d)
                {
                    tmp *= orePrice;
                    output += $" = {tmp:N2} q";
                }

                var idx = recipe.SchemaType;
                var lvl = recipe.Level < 2 ? 1 : recipe.Level;
                if (string.IsNullOrEmpty(idx))
                {
                    idx = $"T{lvl}{listType}";
                }
                var orePrefix = idx.Substring(0, 2) + " ";
                if (isT1Ore || isPlasma || !Schematics.ContainsKey(idx))
                {
                    if (outputDetails)
                    {
                        CostResults.AppendLine((isPlasma ? "" : orePrefix)+output);
                    }
                    continue;
                }
                // for every "started" batch we need a schematic and DU ignores talents
                var schem = Schematics[idx];
                var allQuantity = item.Value * quantity;
                int cnt = (int)Math.Ceiling(allQuantity / 100);
                tmp = schem.Cost * cnt;
                if (_sumSchemClass.ContainsKey(idx))
                {
                    var item1 = _sumSchemClass[idx].Item1 + cnt;
                    var item2 = _sumSchemClass[idx].Item2 + tmp;
                    _sumSchemClass.Remove(idx);
                    _sumSchemClass.Add(idx, new Tuple<int, double>(item1, item2));
                }
                else
                    _sumSchemClass.Add(idx, new Tuple<int, double>(cnt, tmp));
                if (outputDetails)
                {
                    output += $" | {cnt} sch. = {tmp:N2}q";
                    CostResults.AppendLine(orePrefix+output);
                }
                outSum += tmp;
            }
            return outSum;
        }

        public List<ProductDetail> GetOreComponents(string key)
        { // Un-talented only
            var products = new List<ProductDetail>();
            var recipe = _recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.FirstOrDefault(g => g.Id == recipe.GroupId)?.Name == "Catalyst")
                return products;

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
            var results = new List<ProductDetail>();
            foreach (var group in products.GroupBy(p => p.Type))
            {
                results.Add(new ProductDetail() { Name = group.First().Name, Type = group.Key, Quantity = group.Sum(p => p.Quantity) });
            }
            return results;
        }

        // This was going to be recursive but those are way too generic. We just want one parent up.
        private string GetParentGroupName(Guid id)
        {
            var group = Groups.Values.FirstOrDefault(g => g.Id == id);
            if (group == null) return null;
            return group.ParentId != Guid.Empty ? Groups.Values.FirstOrDefault(g => g.Id == group.ParentId)?.Name : group.Name;
        }
    }
}

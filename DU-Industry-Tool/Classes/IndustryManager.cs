﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DU_Industry_Tool
{
    public class IndustryManager
    {
        private readonly bool SkipProcessing = true;
        private readonly string CompoundName = "COMPOUNDLIST";

        public readonly SortedDictionary<string, SchematicRecipe> Recipes; // Global list of all our recipes, from the json
        public List<string> RecipeNames { get; private set; } = new List<string>();
        public BindingList<ProductionItem> ProductionBindingList { get; set; }

        public readonly Dictionary<string, Group> Groups;
        public List<Ore> Ores { get; } = new List<Ore>();
        public SortedDictionary<string, Schematic> Schematics { get; } = new SortedDictionary<string, Schematic>();
        public List<string> ApplicableTalents { get; private set; }
        public List<Talent> Talents { get; } = new List<Talent>();
        public List<string> Groupnames { get; } = new List<string>(370);
        public StringBuilder CostResults { get; set; }

        // Used products quantity for cost calculations
        public int ProductQuantity { get; set; } = 1;

        // True, if CompoundRecipe is to be used as calculation target,
        // which can contain any amount of items and is created with the
        // help of the Production List dialogue
        public bool ProductionListMode { get; set; }
        public SchematicRecipe CompoundRecipe { get; private set; }

        // Constructor
        public IndustryManager(ProgressBar progressBar = null)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-us");
            if (!File.Exists(@"RecipesGroups.json") || !File.Exists("Groups.json"))
            {
                MessageBox.Show(@"Files RecipesGroups.json and/or Groups.json are missing! Please re-download!");
                return;
            }

            // On initialization, read all our data from files
            var json = File.ReadAllText(@"RecipesGroups.json");
            Recipes = JsonConvert.DeserializeObject<SortedDictionary<string, SchematicRecipe>>(json);

            if (progressBar != null)
                progressBar.Value = 20;

            json = File.ReadAllText(@"Groups.json");
            Groups = JsonConvert.DeserializeObject<Dictionary<string, Group>>(json);
            if (progressBar != null)
                progressBar.Value = 30;

            if (Recipes?.Any() == true)
            {
                foreach (var recipe in Recipes.Values) // Set parent names
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
            // mostly by formula as of patch 0.31.6

            var csPriceList = new [] { 150, 375, 1000, 3000, 0 }; // Construct Support
            var cuPriceList = new [] { 250, 5000, 62500, 725000, 0 }; // Core Units

            if (File.Exists("schematicValues.json"))
            {
                Schematics =
                    JsonConvert.DeserializeObject<SortedDictionary<string, Schematic>>(
                        File.ReadAllText("schematicValues.json"));
            }
            else
            {
                // Generate (prefixed with Tx with x = tier):
                // U  = Pures, e.g. T2U
                // P  = Products, e.g. T3P
                // HU = Honeycomb Pures, e.g. T2HC
                // HP = Honeycomb Products, e.g. T3HP
                // SC = Scraps, e.g. T2SC
                // TxEy = Tier x, Element size y, e.g. T2XL tier 2, XL
                var groups = new[] { "U", "P", "HU", "HP", "SC" };
                var prices = new [] { 187.5d, 180d, 125d, 50d, 30d }; // only first price for each group
                var scraps = new [] { 30d, 75d, 187.48d, 468.72 };
                var factors = new [] { 3.125d, 3.3333333d, 3.75d, 5.0d };
                var batchTimes = new [] { 1500, 1800, 625, 250, 150 };

                for (byte i = 0; i <= 4; i++)
                {
                    var g = groups[i];
                    var isHc = g.StartsWith("H");
                    var isSc = g == "SC";
                    var title = isSc ? "Scrap" : (g.EndsWith("U") ? "Pure" : "Product");
                    title += isHc ? " Honeycomb" : (g == "SC" ? "" : " Material");
                    var price = g == "SC" ? scraps[i - 1] : prices[i];
                    var bTime = batchTimes[i];
                    for (byte tier = 1; tier <= 5; tier++)
                    {
                        var bs = (isHc || isSc) ? 25 : (60 - tier * 10);
                        if ((g == "SC" || g.EndsWith("U")) && tier < 2)
                            continue; // no schematics for T1 pures/scraps/hc-pures
                        var schem = new Schematic
                        {
                            Name = $"Tier {tier} {title} Schematic",
                            Key = $"T{tier}{g}",
                            Level = tier,
                            Cost = Math.Round(price, 2),
                            BatchSize = bs,
                            BatchTime = bTime
                        };
                        Schematics.Add(schem.Key, schem);
                        if (tier < 5)
                        {
                            bTime = (int)Math.Round(bTime * 2.5d, 2);
                            price *= (g.StartsWith("H") ? 2.5d : factors[tier - 1]);
                        }
                    }
                }

                // Element schematics
                prices = new [] { 375d, 937.5d, 2343.7d, 5859.3d, 14648.4d };
                factors = new [] { 4.000085d, 3.3333333d, 3d, 4d };
                var batchSizes = new [] { 10, 5, 3, 2, 1 };
                batchTimes = new [] { 750, 1875, 4687, 11700, 29280 }; // seconds for XS per tier 1-5
                var batchTimeFactors = new List<double[]>
                {
                    new [] { 2.0, 2.0, 2.0, 2.0 }, // factor of time between T1 sizes (XS->S->M...)
                    new [] { 2.5, 2.4997333, 2.496266, 2.502564 }, // factor of time between T2 sizes (XS->S->M...)
                    new [] { 1.997013, 1.997863, 1.996805, 2 }, // factor of time between T3 sizes (XS->S->M...)
                    new [] { 2.005128, 1.997442, 2.001280, 1.996161 }, // factor of time between T4 sizes (XS->S->M...)
                    new [] { 2.002049, 1.998976, 1.996927, 2.0 } // factor of time between T5 sizes (XS->S->M...)
                };
                for (byte tier = 1; tier <= 5; tier++)
                {
                    var timeFactors = batchTimeFactors[tier - 1];
                    var price = prices[tier - 1];
                    var btime = batchTimes[tier - 1];
                    for (byte size = 0; size < Consts.SizeList.Count; size++)
                    {
                        var schem = new Schematic
                        {
                            Name = $"Tier {tier} {Consts.SizeList[size]} Element Schematic",
                            Key = $"T{tier}E{Consts.SizeList[size]}",
                            Level = tier,
                            Cost = Math.Round(price, 2),
                            BatchSize = batchSizes[size],
                            BatchTime = btime
                        };
                        Schematics.Add(schem.Key, schem);
                        if (size >= 4) continue;
                        btime = (int)Math.Round(btime * timeFactors[size], 2);
                        price = Math.Round(price * factors[size], 2);
                    }
                }

                // Add Construct Support and Core Units schematics
                var csTime = 750;
                var batchSizesCs = new [] { 25, 20, 15, 10 };
                var batchSizesCu = new [] { 10, 5, 2, 1 };
                var batchTimesCu = new [] { 750, 5000, 25020, 145020 };
                for (var idx = 0; idx <= 3; idx++) // XS - L
                {
                    var size = Consts.SizeList[idx];
                    // Construct Support
                    var key = "CS-" + size;
                    Schematics.Add(key, new Schematic()
                    {
                        Name = $"Construct Support {size} Schematic",
                        Key = key,
                        Cost = csPriceList[idx],
                        BatchSize = batchSizesCs[idx],
                        BatchTime = csTime
                    });
                    csTime *= 2;
                    // Core Units
                    key = "CU-" + size;
                    Schematics.Add(key, new Schematic()
                    {
                        Name = $"Core Unit {size} Schematic",
                        Key = key,
                        Cost = cuPriceList[idx],
                        BatchSize = batchSizesCu[idx],
                        BatchTime = batchTimesCu[idx]
                    });
                }

                // Ammo schematics, tiers T2-T4, sizes XS-L
                prices = new [] { 600d, 1500d, 3750d }; // starting prices per tier' XS
                batchTimes = new [] { 3000, 7500, 18780 }; // starting duration per tier' XS
                for (byte i = 2; i <= 4; i++)
                {
                    var time = batchTimes[i - 2];
                    var price = prices[i - 2];
                    foreach (var size in Consts.SizeList)
                    {
                        if (size == "XL") continue;
                        var key = $"T{i}A{size}";
                        Schematics.Add(key, new Schematic()
                        {
                            Key = key,
                            Name = $"Tier {i} Ammo {size} Schematic",
                            Level = i,
                            Cost = price,
                            BatchSize = 25,
                            BatchTime = time
                        });
                        time *= 2;
                        price *= 2;
                    }
                }

                // Fuel schematics
                var schema = new Schematic
                {
                    Name = "Atmospheric Fuel Schematic",
                    Key = "AtmoFuel",
                    Level = 1,
                    Cost = 60,
                    BatchSize = 50,
                    BatchTime = 600
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic
                {
                    Name = "Space Fuels Schematic",
                    Key = "SpaceFuels",
                    Level = 0,
                    Cost = 150,
                    BatchSize = 50,
                    BatchTime = 1500
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic
                {
                    Name = "Rocket Fuels Schematic",
                    Key = "RocketFuels",
                    Level = 0,
                    Cost = 375,
                    BatchSize = 50,
                    BatchTime = 3750
                };
                Schematics.Add(schema.Key, schema);
                // Standalone schematics
                schema = new Schematic
                {
                    Name = "Bonsai Schematic",
                    Key = "Bonsai",
                    Level = 0,
                    Cost = 10000000,
                    BatchSize = 1,
                    BatchTime = 1
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic
                {
                    Name = "Territory Unit Schematic",
                    Key = "TerritoryUnit",
                    Level = 0,
                    Cost = 20000,
                    BatchSize = 5,
                    BatchTime = 19980
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic
                {
                    Name = "Warp Cell Schematic",
                    Key = "WarpCell",
                    Level = 3,
                    Cost = 5000,
                    BatchSize = 10,
                    BatchTime = 10020
                };
                Schematics.Add(schema.Key, schema);
                schema = new Schematic
                {
                    Name = "Warp Beacon Schematic",
                    Key = "WarpBeacon",
                    Level = 0,
                    Cost = 5000000,
                    BatchSize = 1,
                    BatchTime = 1000800
                };
                Schematics.Add(schema.Key, schema);

                SaveSchematicValues();
            }

            if (progressBar != null)
                progressBar.Value = 50;

            // Load @jericho's awesome list of ID's to be able to cross-check our recipes
            // Item dump from https://du-lua.dev/#/items
            var dmp = false;
            var dmpRcp = false;

            if (File.Exists("items_api_dump.json"))
            {
                try
                {
                    Utils.LuaItems = JsonConvert.DeserializeObject<SortedDictionary<string, DuLuaItem>>(
                                    File.ReadAllText("items_api_dump.json"));
                    dmp = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (File.Exists("recipes_api_dump.json"))
            {
                try
                {
                    Utils.LuaRecipes = JsonConvert.DeserializeObject<SortedDictionary<string, DuLuaRecipe>>(
                                    File.ReadAllText("recipes_api_dump.json"));
                    dmpRcp = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // Populate Keys, fix products and assign schematics
            foreach (var kvp in Recipes)
            {
                kvp.Value.Key = kvp.Key;
                var itemId = kvp.Value.NqId.ToString();

                // Check against Items from du-lua.dev (optional)
                if (dmp)
                {
                    var dmpItem = Utils.LuaItems.FirstOrDefault(x => x.Key == itemId);
                    if (dmpItem.Value != null)
                    {
                        // Transfer updateable values and only protocol to debug console
                        if (!string.IsNullOrEmpty(dmpItem.Value.DisplayNameWithSize) &&
                            kvp.Value.Name != dmpItem.Value.DisplayNameWithSize &&
                            !dmpItem.Value.DisplayNameWithSize.EndsWith("hydraulics") &&
                            !dmpItem.Value.DisplayNameWithSize.Equals("Silver Pure",StringComparison.InvariantCultureIgnoreCase) &&
                            !dmpItem.Value.DisplayNameWithSize.Equals("Nickel Pure",StringComparison.InvariantCultureIgnoreCase))
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Name to {dmpItem.Value.DisplayNameWithSize}");
                            kvp.Value.Name = dmpItem.Value.DisplayNameWithSize;
                        }

                        if (kvp.Value.Level != dmpItem.Value.Tier)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Level {kvp.Value.Level} to {dmpItem.Value.Tier}");
                        }
                        kvp.Value.Level = dmpItem.Value.Tier;

                        if (!string.IsNullOrEmpty(dmpItem.Value.Size) && kvp.Value.Size != dmpItem.Value.Size)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Size {kvp.Value.Size} to {dmpItem.Value.Size}");
                            kvp.Value.Size = dmpItem.Value.Size;
                        }

                        if (kvp.Value.UnitMass != 0 && kvp.Value.UnitMass != dmpItem.Value.UnitMass)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} UnitMass {kvp.Value.UnitMass:N2} to {dmpItem.Value.UnitMass:N2}");
                        }
                        kvp.Value.UnitMass = dmpItem.Value.UnitMass;

                        if (kvp.Value.UnitVolume != 0 && kvp.Value.UnitVolume != dmpItem.Value.UnitVolume)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} UnitVolume {kvp.Value.UnitVolume:N2} to {dmpItem.Value.UnitVolume:N2}");
                        }
                        kvp.Value.UnitVolume = dmpItem.Value.UnitVolume;
                    }
                    else
                    {
                        Debug.WriteLine("NqId NOT FOUND: " + itemId + " Key: " + kvp.Key);
                        itemId = kvp.Value.Id.ToString();
                        if (dmp && !Utils.LuaItems.ContainsKey(itemId))
                        {
                            Debug.WriteLine("Id NOT FOUND: " + itemId + " Key: " + kvp.Key);
                        }
                    }
                }

                // Check against Recipes from du-lua.dev (optional)
                // and transfer usable values
                if (dmpRcp)
                {
                    var dmpItem = Utils.LuaRecipes.FirstOrDefault(x =>
                        x.Value.Products?.Count > 0 &&
                        x.Value.Products[0].Id == kvp.Value.NqId);
                    if (dmpItem.Value != null)
                    {
                        // Transfer updateable values and only protocol to debug console
                        if (kvp.Value.Level != dmpItem.Value.Tier)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Level {kvp.Value.Level} to {dmpItem.Value.Tier}");
                        }
                        kvp.Value.Level = dmpItem.Value.Tier;

                        if ((int)kvp.Value.Time != dmpItem.Value.Time)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Time {kvp.Value.Time} to {dmpItem.Value.Time}");
                        }
                        kvp.Value.Time = dmpItem.Value.Time;

                        if (kvp.Value.Nanocraftable != dmpItem.Value.Nanocraftable)
                        {
                            Debug.WriteLine($"{kvp.Value.Name} Nanocraftable {kvp.Value.Nanocraftable} to {dmpItem.Value.Nanocraftable}");
                        }
                        kvp.Value.Nanocraftable = dmpItem.Value.Nanocraftable;
                    }
                }

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

                var product = kvp.Value.Products?.SingleOrDefault(p => p.Type == kvp.Key);
                if (product == null && kvp.Value.Products.Count > 0)
                {
                    // Key was wrong, happens for many honeycombs
                    product = kvp.Value.Products?.First();
                }

                if (product != null)
                {
                    if (!string.IsNullOrEmpty(kvp.Value?.Name) && product.Name != kvp.Value.Name)
                    {
                        product.Name = kvp.Value.Name;
                    }

                    if (!string.IsNullOrEmpty(product.Type) && product.Type != kvp.Key)
                    {
                        product.Type = kvp.Key;
                    }
                }

                // If item dump exists, do a special honeycomb check for wrong ID's
                if (dmp && kvp.Key.StartsWith("hc"))
                {
                    var luaItem = Utils.LuaItems.FirstOrDefault(x => x.Value.Description.StartsWith("Honeycomb") &&
                                                                    !x.Value.DisplayNameWithSize.EndsWith("Schematic Copy") &&
                                                                     x.Value.DisplayNameWithSize.Equals(kvp.Value.Name,
                                                                     StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(luaItem.Key) && luaItem.Key != kvp.Value.NqId.ToString())
                    {
                        if (ulong.TryParse(luaItem.Key, out var uTmp))
                        {
                            kvp.Value.NqId = uTmp;
                            kvp.Value.Id = uTmp;
                        }
                    }
                }

                if (string.IsNullOrEmpty(kvp.Value.ParentGroupName) || kvp.Value.GroupId == Guid.Empty)
                {
                    kvp.Value.ParentGroupName = "INVALID";
                    var err = kvp.Value.Name + ": ParentGroupName/GroupId missing!";
                    Console.WriteLine(err);
                    MessageBox.Show(err);
                    continue;
                }

                //if (kvp.Value.Nanocraftable) Debug.WriteLine($"{kvp.Value.Name}");

                if (kvp.Key.StartsWith("Catalyst") ||
                    kvp.Value.ParentGroupName == "Ore" ||
                    (kvp.Value.ParentGroupName == "Pure" && kvp.Value.Level <= 1))
                {
                    kvp.Value.SchemaPrice = 0;
                    kvp.Value.SchemaType = null;
                    continue;
                }

                // LOTS of code below to cleanup old data, which might be removed again eventually --tobitege
                /*
                // Preset "ParentGroupName" and "GroupId" for containers
                if (kvp.Key.StartsWith("chemicalcont", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("combustionchamber", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("electricengine", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("firingsystem", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("gazcylinder", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("laserchamber", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("light_", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("magneticrail", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("motherboard", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("orescanner", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("powerconvertor", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("roboticarm", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("silo_", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("controlsystem", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Functional Parts";
                    kvp.Value.GroupId = new Guid("08d8a31f-5127-4f25-8138-779a7f0e5c8d");
                }
                else if (kvp.Key.StartsWith("ammocontainer", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4fcc-40fe-8e92-9fd90394d5c2");
                }
                else if (kvp.Key.StartsWith("missioncontainer", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4fcc-40fe-8e92-9fd90394d5c3");
                }
                else if (kvp.Key.StartsWith("antimattercapsule", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("igniter", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("processor", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("warhead_", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Key.StartsWith("singularitycontainer", StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    kvp.Value.ParentGroupName = "Complex parts";
                    kvp.Value.GroupId = new Guid("08d8a31f-5116-4472-84d8-1cf2de11b3a3");
                }
                else if (kvp.Key.StartsWith("antimattercore", StringComparison.InvariantCultureIgnoreCase))
                {
                    kvp.Value.ParentGroupName = "Exceptional parts";
                    kvp.Value.GroupId = new Guid("08d8a31f-511c-4062-8bcb-fd56f993d7ab");
                }
                else if (kvp.Key.IndexOf("container", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    kvp.Value.ParentGroupName = "Containers";
                    kvp.Value.GroupId = new Guid("08d8a31f-4ff1-4b54-8484-9d05d3885b52");
                }
                else if (
                    kvp.Value.Name.IndexOf("hover engine ", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    kvp.Value.Name.IndexOf("vertical booster ", StringComparison.InvariantCultureIgnoreCase) >= 0
                    )
                {
                    kvp.Value.ParentGroupName = "Ground Engines";
                    kvp.Value.GroupId = new Guid("08d8a31f-5034-42d4-84e8-d23f90cc84af");
                    var elSize = GetElementSize(kvp.Value.Name);
                    if (elSize != "")
                    {
                        kvp.Value.SchemaType = "CS-" + elSize;
                        kvp.Value.SchemaPrice = Schematics.FirstOrDefault(x =>
                                x.Key.Equals(kvp.Value.SchemaType)).Value.Cost;
                    }
                }
                else
                if (kvp.Key.StartsWith("industry", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (kvp.Value.Level < 1) kvp.Value.Level = 1;
                    kvp.Value.ParentGroupName = "Industry";
                    var grp = Groups.FirstOrDefault(x => x.Key == "Industry" + kvp.Value.Level);
                    if (grp.Value?.Id != null)
                    {
                        kvp.Value.GroupId = grp.Value.Id;
                    }
                }
                */

                // Determine the required industry to produce this item
                //if (string.IsNullOrEmpty(kvp.Value.Industry))
                //{
                //    DetermineIndustryFor(kvp.Value);
                //}

                // Skip further processing on some conditions...
                if (SkipProcessing ||
                    (!string.IsNullOrEmpty(kvp.Value.SchemaType) && kvp.Value.SchemaPrice > 0) ||
                    kvp.Value.ParentGroupName.EndsWith("parts", StringComparison.InvariantCultureIgnoreCase) ||
                    kvp.Value.Level < 1 ||
                    kvp.Value.Name.StartsWith("catalyst", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

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
                if (true == parentName?.Equals("core units", StringComparison.InvariantCultureIgnoreCase))
                {
                    var size = GetElementSize(kvp.Value.Name);
                    idx = "CU-" + size;
                    kvp.Value.SchemaType = idx;
                    kvp.Value.SchemaPrice = cuPriceList[Consts.SizeList.IndexOf(size)];
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
                        "Logic Operators"
                    };
                    if (parentName != null && skipGroups.Any(x => parentName.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    if (idx == "" && GetTopLevelGroup(kvp.Value.ParentGroupName) == "Elements")
                    {
                        isElement = true;
                    }

                    var elemGroups = new List<string>
                    {
                        "Atmospheric Engines",
                        "Cannons",
                        "Containers",
                        "Control Units",
                        "Electronics",
                        "Engines",
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
                        "Atmospheric Brakes",
                        "Compact Aileron",
                        "Space Brakes",
                        "Stabilizer",
                        "Wing",
                    };
                    if (csGroups.Any(x => kvp.Value.Name.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var size = GetElementSize(kvp.Value.Name);
                        idx = "CS-" + size;
                        kvp.Value.SchemaType = idx;
                        kvp.Value.SchemaPrice = csPriceList[Consts.SizeList.IndexOf(size)];
                        continue;
                    }

                    if (isElement ||
                        elemGroups.Any(x => x.Equals(parentName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        idx = "E" + GetElementSize(kvp.Value.Name);
                    }
                    else if (parentName.Equals("Product", StringComparison.InvariantCultureIgnoreCase))
                    {
                        idx = "P";
                    }
                    else if ((parentName.Equals("Ore", StringComparison.InvariantCultureIgnoreCase) ||
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
                    else if (parentName.Equals("Scraps", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (kvp.Value.Level <= 1)
                        {
                            kvp.Value.SchemaType = null;
                            kvp.Value.SchemaPrice = 0;
                            continue; // no T1
                        }

                        idx = "SC";
                    }
                    else if (parentName.StartsWith("Product Honeycomb", StringComparison.InvariantCultureIgnoreCase))
                    {
                        idx = "HP";
                    }
                    else if (parentName.StartsWith("Pure Honeycomb", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (kvp.Value.Level <= 1)
                        {
                            kvp.Value.SchemaType = null;
                            kvp.Value.SchemaPrice = 0;
                            continue; // no T1
                        }

                        idx = "HU";
                    }
                    else if (parentName == "Fuels")
                    {
                        if (kvp.Key.StartsWith("Kergon", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "SpaceFuels");
                            kvp.Value.SchemaType = schemaClass.Key;
                            kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        }
                        else if (kvp.Key.Equals("Nitron", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "AtmoFuel");
                            kvp.Value.SchemaType = schemaClass.Key;
                            kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                        }
                        else if (kvp.Key.Equals("Xeron", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var schemaClass = Schematics.FirstOrDefault(x => x.Key == "RocketFuels");
                            kvp.Value.SchemaType = schemaClass.Key;
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
                    if (schemaClass.Value == null) continue;
                    kvp.Value.SchemaType = schemaClass.Key;
                    kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                    Console.WriteLine($@"{kvp.Value.Name} = {kvp.Value.SchemaPrice} ({parentName})");
                }
                else if (kvp.Value.ParentGroupName == "Refined Materials" ||
                         (kvp.Value.Level >= 1 &&
                          kvp.Value.Name.EndsWith("product", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var schemaClass = Schematics.FirstOrDefault(x => x.Key == $"T{kvp.Value.Level}P");
                    if (schemaClass.Value == null) continue;
                    kvp.Value.SchemaType = schemaClass.Key;
                    kvp.Value.SchemaPrice = schemaClass.Value.Cost;
                    Console.WriteLine($@"{kvp.Value.Name} = {kvp.Value.SchemaPrice} ({parentName})");
                }
            }

            if (dmp)
            {
                /*
                // Ammo types output
                List<string> ammoCategories = new List<string> {
                    "326757369",  "3336558558", "399761377",  "512435856",  // Tier 2
                    "2413250793", "1705420479", "3125069948", "2913149958", // Tier 3
                    "2293088862", "3636126848", "3847207511", "2557110259", // Tier 4
                };
                foreach (var aCat in ammoCategories)
                {
                    Debug.WriteLine(_luaItems[aCat].DisplayNameWithSize);
                    var catEntries = _luaItems.Where(x => x.Value.Schematics.Count > 0 &&
                                                          x.Value.Schematics[0].Id == aCat).Select(x => x.Key).ToList();
                    var entries = Recipes.Where(x => catEntries.Contains(x.Value.NqId.ToString())).OrderBy(x => x.Key);
                    foreach (var kvp in entries)
                    {
                        Debug.WriteLine(kvp.Key);
                        //Debug.WriteLine(kvp.Value.Name);
                    }
                    Debug.WriteLine("");
                }
                var missingIds = new StringBuilder();
                var idLine = "";
                var idCount = 0;
                foreach (var kvp in _luaItems)
                {
                    // Exclusion checks
                    if (kvp.Value.DisplayNameWithSize.EndsWith("Schematic Copy")) continue; // schematics
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Model of")) continue; // reward
                    if (kvp.Value.DisplayNameWithSize.Equals("Thoramine")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.Equals("Scanner result")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Package")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Information")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Market Pod")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Gravity changer")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Alarm light")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Speaker")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Weapon")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Battery")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Smoke")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Scanner")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Deployable light orb")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Deploy construct")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Alarm speaker",
                            StringComparison.InvariantCultureIgnoreCase)) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Construct key")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Territory key")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Cryogenic pod m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Conveyor belt")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Fake Core Unit")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Teleportation Node")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Deploy ground Element")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Mission Reserved Space")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Directional detector")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.Equals("Electricity generator")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" tool", StringComparison.InvariantCultureIgnoreCase))
                        continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" Parts")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" 256m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" 512m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" 1024m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" 2048m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.EndsWith(" 4096m")) continue; // internal
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Admin ")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Alien Space Core")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Basic Light")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Artifact ")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Repulsor")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Deprecated")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Decorative Territory Unit")) continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Small depracated decorative gun"))
                        continue; // not in game
                    if (kvp.Value.DisplayNameWithSize.EndsWith("Teleportation Node")) continue; // not in game
                    if (kvp.Value.Description.StartsWith("This is a non-valuable")) continue; // world material
                    if (kvp.Value.Description.StartsWith("Deprecated")) continue; // Deprecated
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Firework")) continue; // Fireworks
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Obsidian")) continue; // Reward
                    if (kvp.Value.DisplayNameWithSize.StartsWith("Art-deco")) continue; // not in game
                    if (kvp.Value.Description.StartsWith("Admin")) continue; // internal stuff
                    if (kvp.Value.DisplayNameWithSize.Contains("Hologram")) continue; // reward
                    if (kvp.Value.DisplayNameWithSize.EndsWith("Mission Package")) continue; // missions item
                    if (kvp.Value.DisplayNameWithSize.Contains("deprecated")) continue; // Deprecated
                    if (kvp.Value.DisplayNameWithSize.Contains("Throne")) continue; // Reward
                    if (kvp.Value.DisplayNameWithSize.Contains("Blueprint")) continue; // internal

                    var itemId = kvp.Key;
                    var item = Recipes.Values.FirstOrDefault(x => x.NqId.ToString() == itemId);
                    if (item != null)
                        continue;
                    var rec = Recipes.FirstOrDefault(x =>
                        x.Value.Name.Equals(kvp.Value.DisplayNameWithSize,
                            StringComparison.InvariantCultureIgnoreCase));
                    if (rec.Key != null && rec.Value?.NqId.ToString() != kvp.Value.Id)
                    {
                        if (ulong.TryParse(kvp.Value.Id, out var uTmp))
                        {
                            rec.Value.NqId = uTmp;
                            rec.Value.id = uTmp;
                        }

                        continue;
                    }

                    idLine += itemId + ", ";
                    idCount++;
                    if (idCount > 5)
                    {
                        idCount = 0;
                        missingIds.AppendLine(idLine);
                        idLine = "";
                    }

                    Debug.WriteLine("NqId NOT IN RECIPES: " + itemId + " Key: " + kvp.Value.DisplayNameWithSize);
                }

                Debug.WriteLine(missingIds.ToString());
                */
            }

            if (dmpRcp)
            {
                /*
                var missingRec = _luaRecipes.Values.Where(x =>
                        x.Products?.Count > 0 && Recipes.Values.All(y => y.NqId != x.Products[0].Id))
                        .OrderBy(x => x.Products[0].DisplayNameWithSize);
                foreach (var missing in missingRec)
                {
                    Debug.WriteLine(missing.Products[0].Id.ToString().PadRight(30) + " " +
                                    missing.Products[0].DisplayNameWithSize);
                }

                // Check for missing Plasma in recipes' ingredients
                var plasmaIds = new List<ulong> {
                    1769135512, 1831558336, 1831557945, 1831558342,
                    1831558338, 1831558341, 1831558343, 1831558340,
                    1831558339, 1831558337
                };
                var plas = _luaRecipes.Values.Where(x => x.Ingredients.Count > 0 &&
                                                         x.Ingredients.Any(y => plasmaIds.Contains(y.Id)))
                                .OrderBy(x => x.Products[0].DisplayNameWithSize).ToList();
                foreach (var pl in plas)
                {
                    var ingName = pl.Ingredients.First(x => x.DisplayNameWithSize.StartsWith("Relic Plasma")).DisplayNameWithSize;
                    var rec = Recipes.FirstOrDefault(x => x.Value.NqId == pl.Products[0].Id);
                    if (rec.Key == null)
                    {
                        Debug.WriteLine(pl.Products[0].DisplayNameWithSize + " " + pl.Products[0].Id + " NOT FOUND!");
                        continue;
                    }
                    if (rec.Value.Ingredients.All(x => x.Name != ingName))
                    {
                        Debug.WriteLine(rec.Key + " " + rec.Value.Name + " is missing " + ingName);
                    }
                }
                */
            }

            // 1st check:
            // Is this a "part", but not an ingredient in any recipe?
            // Could also be just an API category. Unly uncomment when needed!
            //var removalEntries = (from kvp in Recipes.Values.Where(x =>
            //    x.ParentGroupName.EndsWith(" parts", StringComparison.InvariantCultureIgnoreCase))
            //    let found = Recipes.Values.Any(x => x.Ingredients?.Any(
            //        y => y.Name.Equals(kvp.Name, StringComparison.InvariantCultureIgnoreCase)) == true)
            //    where !found select kvp.Key).ToList();
            //foreach (var removalEntry in removalEntries)
            //{
            //    Recipes.Remove(removalEntry);
            //}
            // 2nd check: any ingredient has circular reference to Key?
            //var removalEntries = Recipes.Where(x => x.Value.Ingredients?.Any(y => y.Type == x.Key) == true).ToList();

            //SaveRecipes();

            RecipeNames.AddRange(Recipes.Where(x => !string.IsNullOrEmpty(x.Value.Industry) &&
                                                    x.Value.Industry.IndexOf("Honeycomb", StringComparison.InvariantCultureIgnoreCase) < 0)
                                        .Select(x => x.Value.Name).OrderBy(y => y).ToList());

            if (progressBar != null)
                progressBar.Value = 70;

            // Populate Ores
            // Check if they have an ore values file and load that
            var changed = false;
            if (File.Exists("oreValues.json"))
            {
                Ores = JsonConvert.DeserializeObject<List<Ore>>(File.ReadAllText("oreValues.json"));
            }
            else
            {
                changed = true;
                foreach (var recipe in Recipes.Values.Where(r => r.ParentGroupName == "Ore"))
                {
                    Ores.Add(new Ore()
                    {
                        Key = recipe.Key, Name = recipe.Name, Value = 25 * recipe.Level, Level = recipe.Level
                    }); // BS some values
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
            for (var i = 1; i <= 10; i++)
            {
                var plasmaKey = $"Plasma{i}";
                if (Ores.Exists(x => x.Key == plasmaKey))
                    continue;
                Ores.Add(new Ore()
                {
                    Key = plasmaKey,
                    Name = plasmas[i - 1],
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
                var multiplierTalentGroups =
                    new[] { "Pure", "Scraps", "Product" }; // Setup the names of the item-specific multiplier talents

                var genericScrapTalents =
                    new
                        List<Talent>() // Setup the scrap talents so we can add each scrap to their applicableRecipe later
                        {
                            new Talent() { Name = "Basic Scrap Refinery", Addition = -1, InputTalent = true },
                            new Talent() { Name = "Uncommon Scrap Refinery", Addition = -1, InputTalent = true },
                            new Talent() { Name = "Advanced Scrap Refinery", Addition = -1, InputTalent = true },
                            new Talent() { Name = "Rare Scrap Refinery", Addition = -1, InputTalent = true }
                        };

                foreach (var kvp in Recipes.Where(r => multiplierTalentGroups.Any(t => t == r.Value.ParentGroupName)))
                {
                    // Iterate over every recipe that is part of one of the multiplierTalentGroups, "Pure", "Scraps", or "Product"
                    var recipe = kvp.Value;
                    var talent = new Talent()
                    {
                        Name = recipe.Name + " Productivity", Addition = 0, Multiplier = 0.03
                    }; // They all have 3% multiplier
                    talent.ApplicableRecipes.Add(kvp.Key); // Each of these only applies to its one specific element
                    Talents.Add(talent);
                    if (recipe.ParentGroupName == "Pure" || recipe.ParentGroupName == "Product")
                    {
                        // Pures and products have an input reduction of 0.03 multiplier as well as the output multiplier
                        string nameString = recipe.Name;
                        if (recipe.ParentGroupName == "Pure")
                            nameString += " Ore";
                        talent = new Talent()
                            { Name = nameString + " Refining", Addition = 0, Multiplier = -0.03, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        Talents.Add(talent);
                    }
                    else if (recipe.ParentGroupName == "Scraps")
                    {
                        // And scraps get a flat -1L general, and -2 specific
                        talent = new Talent()
                            { Name = recipe.Name + " Scrap Refinery", Addition = -2, InputTalent = true };
                        talent.ApplicableRecipes.Add(kvp.Key);
                        Talents.Add(talent);
                        if (recipe.Level < 5) // Exotics don't have one
                            genericScrapTalents[recipe.Level - 1].ApplicableRecipes.Add(kvp.Key);
                    }
                }

                Talents.AddRange(genericScrapTalents);

                // Fuel talents
                // Generic gets -2% per level to inputs
                var genericRefineryTalent = new Talent()
                    { Name = "Fuel Refinery", Addition = 0, Multiplier = -0.02, InputTalent = true };

                foreach (var groupList in Recipes.Values.Where(r => r.ParentGroupName == "Fuels")
                             .GroupBy(r => r.GroupId))
                {
                    var groupName = Groups.Values.FirstOrDefault(g => g.Id == groupList.Key)?.Name;
                    var outTalent = new Talent()
                        { Name = groupName + " Productivity", Addition = 0, Multiplier = 0.05 };
                    var inTalent = new Talent()
                        { Name = groupName + " Refinery", Addition = 0, Multiplier = -0.03, InputTalent = true };
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
                Talents.Add(new Talent()
                {
                    Name = "Basic Intermediary Part Productivity", Addition = 1, Multiplier = 0,
                    ApplicableRecipes = Recipes.Values
                        .Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 1).Select(r => r.Key)
                        .ToList()
                });
                Talents.Add(new Talent()
                {
                    Name = "Uncommon Intermediary Part Productivity", Addition = 1, Multiplier = 0,
                    ApplicableRecipes = Recipes.Values
                        .Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 2).Select(r => r.Key)
                        .ToList()
                });
                Talents.Add(new Talent()
                {
                    Name = "Advanced Intermediary Part Productivity", Addition = 1, Multiplier = 0,
                    ApplicableRecipes = Recipes.Values
                        .Where(r => r.ParentGroupName == "Intermediary parts" && r.Level == 3).Select(r => r.Key)
                        .ToList()
                });

                // Ammo talents... they have uncommon, advanced of each of xs, s, m, l
                var typeList = new[] { "Uncommon", "Advanced" };
                foreach (var type in typeList)
                {
                    foreach (var size in Consts.SizeList)
                    {
                        if (size == "XL") continue;
                        Talents.Add(new Talent()
                        {
                            Name = type + " Ammo " + size + " Productivity", Addition = 1, Multiplier = 0,
                            ApplicableRecipes = Recipes.Values
                                .Where(r => r.ParentGroupName.Contains("Ammo") && r.Level == (type == "Uncommon" ? 2 : 3) &&
                                            r.Name.ToLower().EndsWith(" ammo " + size.ToLower())).Select(r => r.Key)
                                .ToList()
                        });
                    }
                }
                Talents.Sort(TalentComparer);
                SaveTalents();
            }

            if (progressBar != null)
                progressBar.Value = 100;
        }

        private static int TalentComparer(Talent x, Talent y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
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
            File.WriteAllText("RecipesGroups.json", JsonConvert.SerializeObject(Recipes));
        }

        public void SaveSchematicValues()
        {
            File.WriteAllText("schematicValues.json", JsonConvert.SerializeObject(Schematics));
        }

        private void DetermineIndustryFor(SchematicRecipe recipe)
        {
            if (recipe.ParentGroupName == "Ore" ||
                recipe.Name.StartsWith("relic plasma", StringComparison.InvariantCultureIgnoreCase))
            {
                recipe.Industry = "";
                return;
            }

            // elements can be produced by same tier as well as 1 lower tier assemblies:
            var indPrefix = recipe.Level > 1 ? Consts.TierNames[recipe.Level-1] : Consts.TierNames[recipe.Level];
            var indSuffix = "";

            var isPart    = recipe.ParentGroupName.EndsWith("Parts", StringComparison.InvariantCultureIgnoreCase);
            var isPure    = recipe.ParentGroupName.Equals("Pure");
            var isProduct = recipe.ParentGroupName.Equals("Product");
            var isScrap   = recipe.ParentGroupName.Equals("Scraps");
            var isHC      = recipe.ParentGroupName == "Product Honeycomb Materials" ||
                            recipe.ParentGroupName == "Pure Honeycomb Materials";

            var isElect   = isPart && (
                                (recipe.Name.IndexOf(" antenna ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" antimatter core", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" anti-gravity core", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" button ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" component", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" connector", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" control system ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" core system ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" electronics", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" mechanical sensor ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" motherboard ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" ore scanner ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" power system", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" power transformer ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" processor", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" quantum alignment unit", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" quantum barrier", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf("uncommon light", StringComparison.InvariantCultureIgnoreCase) >= 0));

            var isMetal   = isPart && !isElect && (
                                recipe.Name.EndsWith(" burner", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" chemical container ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" combustion chamber ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" container ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" electric engine ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" firing system ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" frame ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" gas cylinder ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" hydraulics", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" ionic chamber ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" magnet", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" magnetic rail ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" missile silo ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" mobile panel ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" pipe", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" power transformer ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" robotic arm ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                (recipe.Name.IndexOf(" screen ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" screw", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" singularity container", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" solid warhead", StringComparison.InvariantCultureIgnoreCase));

            var is3D      = !isElect && !isMetal && (
                                recipe.Name.Equals("carbon fiber product", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" casing ", StringComparison.InvariantCultureIgnoreCase) > 0) ||
                                recipe.Name.EndsWith(" fixation", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" injector", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.EndsWith(" quantum core", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" screen ", StringComparison.InvariantCultureIgnoreCase) > 0));

            var isGlass   = !isElect && !isMetal && !is3D && (
                                recipe.Key.StartsWith("led_", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Key.StartsWith("WarpCell") ||
                                recipe.Name.EndsWith(" antimatter capsule", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf(" laser chamber", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                                recipe.Name.EndsWith(" optics", StringComparison.InvariantCultureIgnoreCase) ||
                                (recipe.Name.IndexOf("glass", StringComparison.InvariantCultureIgnoreCase) >= 0));

            var isChem    = !isElect && !isMetal && !is3D && !isGlass && (
                                recipe.Name.StartsWith("Biological") ||
                                recipe.Name.StartsWith("Catalyst") ||
                                recipe.Name.EndsWith(" explosive module", StringComparison.InvariantCultureIgnoreCase) ||
                                recipe.Name.StartsWith("Fluoropolymer") ||
                                recipe.ParentGroupName.Equals("Fuels") ||
                                recipe.ParentGroupName.EndsWith("plastic product", StringComparison.InvariantCultureIgnoreCase));

            var isAssy = !isElect && !isMetal && !is3D && !isChem && !isGlass &&
                         !isPart && !isPure && !isProduct && !isScrap && !isHC;

            string getIndyName(string industry) => $"{indPrefix} {industry} {indSuffix}".Trim();

            if (recipe.Key == "OxygenPure" || recipe.Key == "HydrogenPure" || isScrap)
            {
                recipe.Industry = getIndyName("Basic Recycler M"); return;
            }
            if (isGlass)
            {
                recipe.Industry = getIndyName("Glass Furnace M"); return;
            }
            if (isChem)
            {
                recipe.Industry = getIndyName("Chemical Industry M"); return;
            }
            if (is3D)
            {
                recipe.Industry = getIndyName("3D Printer M"); return;
            }
            if (isElect)
            {
                recipe.Industry = getIndyName("Electronics Industry M"); return;
            }
            if (isMetal)
            {
                recipe.Industry = getIndyName("Metalwork Industry M"); return;
            }
            if (isPure || recipe.ParentGroupName == "Refined Materials")
            {
                recipe.Industry = getIndyName("Refiner M"); return;
            }
            if (isProduct)
            {
                recipe.Industry = getIndyName("Smelter M"); return;
            }
            if (isHC)
            {
                recipe.Industry = getIndyName("Honeycomb Refinery M"); return;
            }
            if (isAssy)
            {
                // special cases, there may be more :(
                var noLowerTier =
                    (recipe.Name.IndexOf("assembly line s", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf("3d printer", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" furnace ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" comb refinery ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" industry ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" refiner ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" recycler ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (recipe.Name.IndexOf(" smelter ", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    recipe.Name.StartsWith("ammo", StringComparison.InvariantCultureIgnoreCase)  ||
                    recipe.Name.Equals("antennalarge", StringComparison.InvariantCultureIgnoreCase)  ||
                    recipe.Name.Equals("antennamedium", StringComparison.InvariantCultureIgnoreCase)  ||
                    recipe.Name.Equals("AntennaSmall", StringComparison.InvariantCultureIgnoreCase)  ||
                    recipe.ParentGroupName.Equals("Furniture & Appliances", StringComparison.InvariantCultureIgnoreCase)  ||
                    false;
                indSuffix = GetElementSize(recipe.Name, noLowerTier);
                // and more special cases:
                if (recipe.Name.Equals("Territory Unit XL", StringComparison.InvariantCultureIgnoreCase))
                {
                    indSuffix = "M";
                }
                else
                if (recipe.Name.Equals("vertical light l", StringComparison.InvariantCultureIgnoreCase))
                {
                    indSuffix = "S";
                }
                else
                if (recipe.Name.Equals("vertical light m", StringComparison.InvariantCultureIgnoreCase) ||
                    recipe.Name.Equals("long light m", StringComparison.InvariantCultureIgnoreCase) ||
                    recipe.Name.Equals("long light l", StringComparison.InvariantCultureIgnoreCase) ||
                    recipe.Name.StartsWith("square light", StringComparison.InvariantCultureIgnoreCase) ||
                    false)
                {
                    indSuffix = "XS";
                }
                else
                switch (recipe.Key)
                {
                    case "AdjunctTipSmall":
                    case "AdjunctTipMedium": indSuffix = "XS"; break;
                    case "AdjunctTipLarge": indSuffix = "S"; break;
                    case "AileronLarge2": indSuffix = "M"; break;
                    case "AileronMedium2": indSuffix = "S"; break;
                    case "AileronShortLarge2": indSuffix = "M"; break;
                    case "AileronShortMedium2": indSuffix = "S"; break;
                    case "antenna_5_xl": indSuffix = "M"; break;
                }
                recipe.Industry = getIndyName("Assembly Line");
                return;
            }
            Debug.WriteLine(recipe.Name.PadRight(40)+" -> NO INDY!");
        }

        private string GetElementSize(string elemName, bool noLowerTier = false)
        {
            for (var idx = 0; idx < 5; idx++)
            {
                if (elemName.EndsWith(" " + Consts.SizeList[idx], StringComparison.InvariantCultureIgnoreCase))
                {
                    // most XL cannot be produced on an L assembler so "idx < 4" condition
                    return Consts.SizeList[(idx > 0 && idx < 4 && !noLowerTier) ? idx - 1 : idx];
                }
            }
            return "";
        }

        private string FindParent(Guid groupId)
        {
            if (groupId == Guid.Empty) return "";
            var grp = Groups.FirstOrDefault(x => x.Value.Id == groupId);
            if (grp.Value == null || string.IsNullOrEmpty(grp.Key)) return "";
            if (grp.Key == "ConsumableDisplay" || grp.Key == "Material" || grp.Key == "Element") return grp.Value.Name;
            return grp.Value.ParentId == Guid.Empty ? grp.Value.Name : FindParent(grp.Value.ParentId);
        }

        private string GetTopLevelGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return "";
            var grp = Groups.FirstOrDefault(x => x.Value.Name == groupName);
            if (string.IsNullOrEmpty(grp.Key)) return "";
            if (grp.Value.ParentId == Guid.Empty ||
                grp.Value.Name == "Product")
                return grp.Value.Name; // e.g. "Parts"
            var tmp = FindParent(grp.Value.ParentId);
            return tmp;
        }

        public List<IngredientRecipe> GetIngredientRecipes(string key, double quantity = 1)
        {
            var results = new List<IngredientRecipe>();
            var recipe = Recipes[key];

            // Skip catalysts entirely tho.
            if (key.StartsWith("catalyst", StringComparison.InvariantCultureIgnoreCase))
                return results;

            GetTalentsForKey(key, out var inputMultiplier, out var inputAdder, out var outputMultiplier,
                out var outputAdder);

            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                var resRecipe =
                    JsonConvert.DeserializeObject<IngredientRecipe>(
                        JsonConvert.SerializeObject(Recipes[ingredient.Type]));
                if (resRecipe == null)
                    continue;

                var entry = Groups.Values.FirstOrDefault(g => g.Id != Guid.Empty && g.Id == resRecipe.GroupId);
                if (string.IsNullOrEmpty(entry?.Name) ||
                    entry.Name.StartsWith("catalyst", StringComparison.InvariantCultureIgnoreCase) ||
                    recipe.Products?.Any() != true)
                    continue;

                resRecipe.Quantity =
                    (ingredient.Quantity * inputMultiplier /
                     (recipe.Products.First().Quantity * outputMultiplier + outputAdder)) * quantity;
                results.Add(resRecipe);
                if (resRecipe.Ingredients.Count > 0)
                    results.AddRange(GetIngredientRecipes(ingredient.Type, resRecipe.Quantity));
            }

            return results;
        }

        public double GetBaseCost(string key)
        {
            // Just like the other one, but ignore talents.
            if (!Recipes.Keys.Contains(key))
                return 0;
            var recipe = Recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.FirstOrDefault(g => g.Id == recipe.GroupId)?.Name == "Catalyst")
                return 0;

            double totalCost = 0;
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!Recipes.Keys.Contains(ingredient.Type))
                {
                    Console.WriteLine($@"Schematic {ingredient.Type} not found!");
                    continue;
                }

                if (Recipes.Keys.Contains(ingredient.Type) && Recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    totalCost += (ingredient.Quantity *
                                  Ores.First(o => o.Key == ingredient.Type).Value) /
                                 (recipe.Products.First().Quantity);
                }
                else
                {
                    totalCost += GetBaseCost(ingredient.Type) * ingredient.Quantity /
                                 (recipe.Products.First().Quantity);
                }
            }
            return totalCost;
        }

        public void GetTalentsForKey(string key, out double inputMultiplier, out double inputAdder,
            out double outputMultiplier, out double outputAdder)
        {
            inputMultiplier = 1;
            inputAdder = 0;
            outputMultiplier = 1;
            outputAdder = 0;
            foreach (var talent in Talents.Where(t => t.ApplicableRecipes.Contains(key)))
            {
                if (!ApplicableTalents.Contains(talent.Name))
                {
                    ApplicableTalents.Add(talent.Name);
                }

                if (talent.InputTalent)
                {
                    inputMultiplier +=
                        talent.Multiplier *
                        talent.Value; // Add each talent's multipler and adder so that we get values like 1.15 or 0.85, for pos/neg multipliers
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
            double quantity = 1, string title = "", bool outputDetails = true)
        {
            if (itemPrices.Count == 0) return 0;
            CostResults.AppendLine(title);
            double outSum = 0;
            foreach (var item in itemPrices)
            {
                var key = item.Key;
                if (key.StartsWith("T"))
                {
                    key = key.Substring(3); // assume tier at start, e.g. "T2 "
                }
                var recipe = Recipes.Values.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (recipe == null)
                {
                    Debug.WriteLine("NOT FOUND: " + key);
                    continue;
                }

                var isOre = recipe.ParentGroupName == "Ore";
                var isPart = recipe.ParentGroupName.EndsWith("parts", StringComparison.InvariantCultureIgnoreCase);
                var isT1Ore = listType == "U" && isOre && recipe.Level <= 1;
                var isPlasma = recipe.ParentGroupName == "Consumables" && recipe.Key.StartsWith("Plasma");
                var orePrice = 0d;
                if (listType == "U" && (isPlasma || isOre))
                {
                    orePrice = Ores.FirstOrDefault(o => o.Name == key)?.Value ?? 0;
                }

                var tmp = item.Value * quantity;
                var recName = recipe.Name.Substring(0, Math.Min(recipe.Name.Length, 29));
                var output = $"{recName}:".PadRight(30) + $"{tmp:N2}".PadLeft(10) + (isPlasma || isPart ? "  " : " L");
                if ((isPlasma || isOre) && orePrice > 0.00d)
                {
                    tmp *= orePrice;
                    output += " = " + $"{tmp:N2} q".PadLeft(15);
                }

                var idx = recipe.SchemaType;
                var lvl = recipe.Level < 2 ? 1 : recipe.Level;
                if (string.IsNullOrEmpty(idx))
                {
                    idx = $"T{lvl}{listType}";
                }

                var orePrefix = idx.Substring(0, 2) + " ";
                if (isT1Ore || isPlasma || isPart || !Schematics.ContainsKey(idx))
                {
                    if (outputDetails)
                    {
                        CostResults.AppendLine((isPlasma ? "" : orePrefix) + output);
                    }
                    continue;
                }

                // for every "started" batch we need a schematic (no talents for this in DU yet)
                var schem = Schematics[idx];
                var allQuantity = item.Value * quantity;
                int cnt = 0;
                if (recipe.SchemaType != null)
                {
                    var productQuantity = recipe.Products
                        .FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Quantity;
                    cnt = (int)Math.Ceiling(allQuantity / productQuantity);
                    tmp = schem.Cost * cnt;
                    Calculator.AddSchema(idx, cnt, tmp);
                    output += " | " + $"{cnt} sch.".PadLeft(10) + " = " + $"{tmp:N2} q".PadLeft(14);
                }

                if (outputDetails)
                {
                    CostResults.AppendLine(orePrefix + output);
                }

                outSum += tmp;
            }
            return outSum;
        }

        public List<ProductDetail> GetOreComponents(string key)
        {
            // Un-talented only
            var products = new List<ProductDetail>();
            var recipe = Recipes[key];

            // Skip catalysts entirely tho.
            if (Groups.Values.FirstOrDefault(g => g.Id == recipe.GroupId)?.Name == "Catalyst")
                return products;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (Recipes[ingredient.Type].ParentGroupName == "Ore")
                {
                    products.Add(new ProductDetail()
                    {
                        Name = ingredient.Name, Quantity = ingredient.Quantity / recipe.Products[0].Quantity,
                        Type = ingredient.Type
                    });
                }
                else
                {
                    foreach (var result in GetOreComponents(ingredient.Type))
                    {
                        products.Add(new ProductDetail()
                        {
                            Name = result.Name,
                            Quantity = (result.Quantity * ingredient.Quantity) / recipe.Products[0].Quantity,
                            Type = result.Type
                        });
                    }
                }
            }

            // Now flatten them into totals
            var results = new List<ProductDetail>();
            foreach (var group in products.GroupBy(p => p.Type))
            {
                results.Add(new ProductDetail()
                    { Name = group.First().Name, Type = group.Key, Quantity = group.Sum(p => p.Quantity) });
            }

            return results;
        }

        // This was going to be recursive but those are way too generic. We just want one parent up.
        private string GetParentGroupName(Guid id)
        {
            var group = Groups.Values.FirstOrDefault(g => g.Id == id);
            if (group == null) return null;
            return group.ParentId != Guid.Empty
                ? Groups.Values.FirstOrDefault(g => g.Id == group.ParentId)?.Name ?? "xxx"
                : group.Name;
        }

        public bool PrepareProductListRecipe()
        {
            if (ProductionBindingList.Count < 1) return false;
            var cmp = new SchematicRecipe
            {
                Key = CompoundName,
                Name = "Production List"
            };
            var cnt = 0;
            foreach (var prodItem in ProductionBindingList)
            {
                // Sanity checks
                if (string.IsNullOrEmpty(prodItem.Name) || prodItem.Quantity <= 0)
                    continue;
                var rec = Recipes.FirstOrDefault(x => x.Value.Name.Equals(prodItem.Name, StringComparison.InvariantCultureIgnoreCase));
                if (rec.Key == null || rec.Value?.Ingredients?.Any() != true || rec.Value?.Products?.Any() != true)
                    continue;

                // Accumulate crafting time over all products
                cmp.Time += (rec.Value.Time * prodItem.Quantity);

                // Sum up top-level ingredients
                foreach (var ing in rec.Value.Ingredients)
                {
                    var tmp = cmp.Ingredients.FirstOrDefault(x => x.Name.Equals(ing.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (tmp != null)
                    {
                        tmp.Quantity += (ing.Quantity * prodItem.Quantity);
                    }
                    else
                    {
                        ing.Quantity *= prodItem.Quantity;
                        cmp.Ingredients.Add(ing);
                    }
                }

                // Process products
                foreach (var prd in rec.Value.Products)
                {
                    var tmp = cmp.Products.FirstOrDefault(x => x.Name.Equals(prd.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (tmp != null)
                    {
                        tmp.Quantity += (prd.Quantity * prodItem.Quantity);
                    }
                    else
                    {
                        prd.Quantity *= prodItem.Quantity;
                        cmp.Products.Add(prd);
                    }
                }
                cnt++;
            }
            if (cnt == 0) return false;
            CompoundRecipe = cmp;

            // Add compound recipe to main recipe list, check first to remove an existing one
            Recipes.Remove(CompoundName);
            Recipes[CompoundName] = cmp;
            return true;
        }

        private string _currentRecipe;
        public double GetTotalCost(string key, double amount = 0, string level = "", int depth = 0, bool silent = false)
        {
            if (depth == 0)
            {
                _currentRecipe = key;
                amount = ProductQuantity;
                if (!ProductionListMode)
                {
                    Calculator.Initialize();
                }
                ApplicableTalents = new List<string>(160);
            }
            else
            if (depth > 10 || key == _currentRecipe) // faulty recipe?!
            {
                return 0;
            }
            if (CostResults == null) CostResults = new StringBuilder();

            double totalCost = 0;
            if (key.StartsWith("Catalyst"))
                return 0;
            if (!Recipes.Keys.Contains(key))
            {
                var err = $"*** MISSING: {key} !!!";
                Debug.WriteLine(err);
                MessageBox.Show(err);
                return 0;
            }

            var recipe = Recipes[key];
            var product = recipe.Products?.FirstOrDefault();
            if (product == null) return 0;
            if (recipe.Ingredients == null)
            {
                return 0;
            }

            GetTalentsForKey(recipe.Key, out var inputMultiplier, out var inputAdder, out var outputMultiplier,
                out var outputAdder);

            var productQty = product.Quantity;
            var curLevel = level;
            level += "     ";
            Debug.WriteLineIf(!silent, $"{curLevel}> {product.Name} ({amount:N2})");

            inputMultiplier += 1;
            outputMultiplier += 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrEmpty(ingredient.Type) || !Recipes.ContainsKey(ingredient.Type))
                {
                    var err = $"{curLevel}     MISSING: {recipe.Key}->{ingredient.Name} !!!";
                    Debug.WriteLine(err);
                    MessageBox.Show(err);
                    continue;
                }

                double cost;
                var qty = 0d;
                double factor;
                var myRecipe = Recipes[ingredient.Type];
                var isOre = myRecipe.ParentGroupName.Equals("Ore");
                var isPure = myRecipe.ParentGroupName.Equals("Pure") ||
                             myRecipe.ParentGroupName.Equals("Refined Materials");
                var isPart = myRecipe.ParentGroupName.EndsWith("parts", StringComparison.InvariantCultureIgnoreCase);
                var isProduct = myRecipe.ParentGroupName.Equals("Product");
                var isPlasma = myRecipe.ParentGroupName.Equals("Consumables") &&
                               myRecipe.Key.StartsWith("plasma", StringComparison.InvariantCultureIgnoreCase);
                var ingName = ingredient.Name;
                var ingKey = ingName;
                if (!isPlasma)
                {
                    ingKey = "T" + (myRecipe.Level < 2 ? "1" : myRecipe.Level.ToString()) + " " + ingKey;
                }

                if (isOre || isPure || isProduct)
                {
                    factor = ((productQty + outputAdder) * outputMultiplier) /
                             ((ingredient.Quantity + inputAdder) * inputMultiplier);
                }
                else
                {
                    factor = ((ingredient.Quantity + inputAdder) * inputMultiplier) /
                             ((productQty + outputAdder) * outputMultiplier);
                }

                if (isOre || isPlasma)
                {
                    // assumption: Plasma qty always 1
                    qty = isPlasma ? 1 : amount / factor;
                    cost = qty * Ores.First(o => o.Key == ingredient.Type).Value;
                    totalCost += cost;
                    Calculator.Add(SummationType.ORES, ingKey, qty);
                    Debug.WriteLineIf(!silent && qty > 0, $"{curLevel}     ({ingredient.Name}: {qty:N2} = {cost:N2}q)");
                    Calculator.Add(SummationType.INGREDIENTS, ingKey, qty);
                    continue;
                }

                if (isPure)
                {
                    qty = amount / factor;
                    cost = GetTotalCost(ingredient.Type, qty, level, depth + 1, silent);
                    totalCost += cost;
                    Calculator.Add(SummationType.PURES, ingKey, qty);
                    ingName = " " + ingKey;
                    Calculator.Add(SummationType.INGREDIENTS, ingName, qty);
                    continue;
                }

                if (depth < 1)
                {
                    qty = factor;
                }
                if (isPart || isProduct)
                {
                    if (isPart)
                    {
                        ingName = "   " + ingKey;
                        qty = amount * ingredient.Quantity;
                        Calculator.Add(SummationType.INGREDIENTS, ingName, qty);
                    }
                    else
                    if (isProduct)
                    {
                        ingName = "  " + ingKey;
                        qty = amount / factor;
                        Calculator.Add(SummationType.INGREDIENTS, ingName, qty);
                    }
                }
                else
                {
                    qty = amount * factor;
                }

                cost = GetTotalCost(ingredient.Type, qty, level, depth + 1, silent);
                totalCost += cost;
                Debug.WriteLineIf(!silent && cost > 0, $"{curLevel}     = {cost:N2}q");

                if (isPart)
                {
                    Calculator.Add(SummationType.PARTS, ingKey, qty);
                    continue;
                }

                if (!isProduct) continue;
                Calculator.Add(SummationType.PRODUCTS, ingKey, qty);
            }

            if (depth != 0) return totalCost;

            // Only on top-level we calculate total cost for all schematics for each ingredient
            CostResults.AppendLine($"Cost for {amount} {recipe.Name}");
            if (ProductionListMode && recipe.Key == CompoundName)
            {
                var prdCost = 0d;
                foreach (var prd in recipe.Products)
                {
                    var prdRec = Recipes.FirstOrDefault(x => x.Key == prd.Type);
                    if (prdRec.Key == null) continue;
                    var prdEntryCost = prdRec.Value.SchemaPrice * prd.Quantity;
                    Calculator.AddSchema(prdRec.Value.SchemaType, (int)prd.Quantity, prdEntryCost);
                    prdCost += prdEntryCost;
                }
                Calculator.AddSchematicCost(prdCost);
            }
            else
            if (!string.IsNullOrEmpty(recipe.SchemaType))
            {
                var schemaCost = amount * recipe.SchemaPrice;
                Calculator.AddSchematicCost(schemaCost);
                CostResults.AppendLine($"Item schematic(s) {recipe.SchemaType}: {schemaCost:N2}q");
            }

            CostResults.AppendLine("");
            Calculator.AddSchematicCost(CalculateItemCost(Calculator.Get(SummationType.ORES), "U", 1, "Ores"));
            Calculator.AddSchematicCost(CalculateItemCost(Calculator.Get(SummationType.PURES), "U", 1, "Pures"));
            Calculator.AddSchematicCost(CalculateItemCost(Calculator.Get(SummationType.PRODUCTS), "P", 1, "Products"));
            CalculateItemCost(Calculator.Get(SummationType.PARTS), "P", 1, "Parts");
            CostResults.AppendLine("Schematics:".PadRight(16) + $"{Calculator.SchematicsCost:N2}q".PadLeft(20));
            CostResults.AppendLine("Pures/Products:".PadRight(16) + $"{totalCost:N2}q".PadLeft(20));
            totalCost += Calculator.SchematicsCost;
            var costx1 = totalCost / amount;
            CostResults.AppendLine("Total Cost:".PadRight(16) + $"{totalCost:N2}q".PadLeft(20));
            CostResults.AppendLine("Cost x1:".PadRight(16) + $"{costx1:N2}q".PadLeft(20));
            Debug.WriteLineIf(!silent, CostResults.ToString());

            if (Calculator.SumSchemClass?.Any() == true)
            {
                CostResults.AppendLine();
                CostResults.AppendLine("----- Schematics details -----");
                CostResults.AppendLine("Schema.                                 amount              cost    copy time (1 slot)");
                foreach (var schem in Calculator.SumSchemClass)
                {
                    var s = Schematics.FirstOrDefault(x => x.Key == schem.Key);
                    var numBatches = Math.Ceiling(schem.Value.Item1 / (double)s.Value.BatchSize);
                    var batchTime = (s.Value?.BatchTime ?? 0) * numBatches;
                    var sp = TimeSpan.FromSeconds(batchTime);
                    CostResults.AppendLine(schem.Key.PadRight(35) +
                                           $"x{schem.Value.Item1:N0}".PadLeft(11) +
                                           $"{schem.Value.Item2:N2}q".PadLeft(21) +
                                           " (" + sp.ToString(@"dd\.hh\:mm\:ss") + ")");
                }
            }

            // Total Ingredients List
            var sumIng = Calculator.Get(SummationType.INGREDIENTS);
            if (sumIng?.Any() != true) return costx1;

            CostResults.AppendLine();
            CostResults.AppendLine("----- Total Ingredients List -----");
            var maxlen = 2 + sumIng.Max(x => x.Key.Length);
            foreach (var item in sumIng)
            {
                CostResults.AppendLine(item.Key.PadRight(maxlen) + $"{item.Value:N2}".PadLeft(12));
            }

            return costx1;
        }

    }
}
using System;
using System.Collections.Generic;

namespace DU_Industry_Tool
{
    public enum SummationType
    {
        ORES,
        PURES,
        PRODUCTS,
        PARTS,
        INGREDIENTS
    }

    public static class Calculator
    {
        // Container for all summations
        private static Dictionary<SummationType, SortedDictionary<string, double>> _sums;

        // Public readonly access to Sums
        public static Dictionary<SummationType, SortedDictionary<string, double>> Sums => _sums;

        public static SortedDictionary<string, Tuple<int, double>> SumSchemClass { get; private set; }
        public static double SchematicsCost { get; private set; }

        public static void Initialize()
        {
            _sums = new Dictionary<SummationType, SortedDictionary<string, double>>();
            SchematicsCost = 0;

            SumSchemClass = new SortedDictionary<string, Tuple<int, double>>();

            for (var sumType = 0; sumType <= (int)SummationType.INGREDIENTS; sumType++)
            {
                _sums.Add((SummationType)sumType, new SortedDictionary<string, double>());
            }
        }

        public static void AddSchematicCost(double quantity)
        {
            SchematicsCost += quantity;
        }

        public static void AddSchema(string schemaKey, int qty, double amount)
        {
            if (SumSchemClass == null)
            {
                SumSchemClass = new SortedDictionary<string, Tuple<int, double>>();
            }

            if (SumSchemClass.ContainsKey(schemaKey))
            {
                var item1 = SumSchemClass[schemaKey].Item1 + qty;
                var item2 = SumSchemClass[schemaKey].Item2 + amount;
                SumSchemClass.Remove(schemaKey);
                SumSchemClass.Add(schemaKey, new Tuple<int, double>(item1, item2));
            }
            else
                SumSchemClass.Add(schemaKey, new Tuple<int, double>(qty, amount));
        }

        public static void Add(SummationType sumType, string key, double quantity)
        {
            if (!_sums.ContainsKey(sumType))
            {
                _sums.Add(sumType, new SortedDictionary<string, double>());
            }

            if (_sums[sumType].ContainsKey(key))
                _sums[sumType][key] += quantity;
            else
                _sums[sumType].Add(key, quantity);
        }

        public static SortedDictionary<string, double> Get(SummationType sumType)
        {
            if (!_sums.ContainsKey(sumType))
            {
                _sums.Add(sumType, new SortedDictionary<string, double>());
            }
            return _sums[sumType];
        }
    }
}
using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Извлекает event.shaped(output, pattern, keyMap) → WorkbenchRecipeConfiguration
    /// </summary>
    public class ShapedExtractor : IRecipeExtractor
    {
        public bool CanHandle(Node? node)
        {
            if (node is not CallExpression call) return false;
            if (call.Callee is not MemberExpression member) return false;

            var obj  = AstHelper.ExtractStringValue(member.Object);
            var prop = AstHelper.ExtractStringValue(member.Property);

            return obj == "event" && prop == "shaped";
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            if (node is not CallExpression call) return null;
            if (call.Arguments.Count < 3) return null;

            // Аргумент 0: output — Item.of("id", count) или "id"
            var outputItem = ExtractOutput(call.Arguments[0]);
            if (outputItem == null) return null;

            // Аргумент 1: pattern — ["AB", "CD"] или ["ABC", "DEF", "GHI"]
            var rows = ExtractPattern(call.Arguments[1]);
            if (rows == null || rows.Count == 0 || rows.Count > 3) return null;

            // Аргумент 2: keyMap — { A: "minecraft:item", ... }
            var keyMap = ExtractKeyMap(call.Arguments[2]);
            if (keyMap == null) return null;

            WorkbenchCraftGrid grid;
            try
            {
                grid = new WorkbenchCraftGrid(rows.ToArray());
            }
            catch
            {
                return null;
            }

            RecipePattern pattern;
            try
            {
                pattern = new RecipePattern(grid, keyMap);
            }
            catch
            {
                return null;
            }

            return new WorkbenchRecipeConfiguration
            {
                itemOutput  = outputItem,
                pattern     = pattern,
                SourceStart = node!.Range.Start,
                SourceEnd   = node!.Range.End,
            };
        }

        private static ItemComponent? ExtractOutput(Node node)
        {
            // Item.of("id", count)
            if (node is CallExpression call &&
                call.Callee is MemberExpression member &&
                AstHelper.ExtractStringValue(member.Object)   == "Item" &&
                AstHelper.ExtractStringValue(member.Property) == "of" &&
                call.Arguments.Count >= 1 &&
                call.Arguments[0] is Literal idLit && idLit.Value is string id)
            {
                int count = 1;
                if (call.Arguments.Count >= 2 &&
                    call.Arguments[1] is Literal cntLit && cntLit.Value is double d)
                    count = (int)d;

                return new ItemComponent(id, count);
            }

            // "minecraft:item" или "2x minecraft:item"
            if (node is Literal strLit && strLit.Value is string str)
                return ParseItemComponent(str);

            return null;
        }

        private static List<string>? ExtractPattern(Node node)
        {
            if (node is not ArrayExpression arr) return null;

            var rows = new List<string>();
            foreach (var el in arr.Elements)
            {
                if (el is Literal lit && lit.Value is string row)
                    rows.Add(row);
                else
                    return null;
            }
            return rows;
        }

        private static Dictionary<char, string>? ExtractKeyMap(Node node)
        {
            if (node is not ObjectExpression obj) return null;

            var map = new Dictionary<char, string>();
            foreach (var prop in obj.Properties)
            {
                if (prop is not Property property) continue;

                var key = AstHelper.ExtractStringValue(property.Key);
                if (key == null || key.Length != 1) continue;

                var value = AstHelper.ExtractStringValue(property.Value);
                if (value == null) continue;

                map[key[0]] = value;
            }
            return map;
        }

        private static ItemComponent ParseItemComponent(string str)
        {
            var parts = str.Split(' ');
            if (parts.Length == 2 && parts[0].EndsWith("x") &&
                int.TryParse(parts[0].TrimEnd('x'), out int amount))
                return new ItemComponent(parts[1], amount);

            return new ItemComponent(str, 1);
        }
    }
}

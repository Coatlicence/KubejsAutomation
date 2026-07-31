using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Извлекает event.smelting/blasting/smoking/stonecutting → MinecraftStandardRecipeConfig
    /// </summary>
    public class MinecraftStandardExtractor : IRecipeExtractor
    {
        private static readonly System.Collections.Generic.Dictionary<string, MinecraftRecipeType> _methods = new()
        {
            ["smelting"]     = MinecraftRecipeType.Smelting,
            ["blasting"]     = MinecraftRecipeType.Blasting,
            ["smoking"]      = MinecraftRecipeType.Smoking,
            ["stonecutting"] = MinecraftRecipeType.Stonecutting,
        };

        public bool CanHandle(Node? node)
        {
            return TryGetRecipeType(node, out _);
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            if (!TryGetRecipeType(node, out var recipeType))
                return null;

            var call = (CallExpression)node!;

            // event.smelting('output', 'input') или event.smelting('output:2', 'input').xp(0.1)
            // Разворачиваем цепочку .xp() если есть
            var baseCall = UnwrapChain(call, out float? xp);

            if (baseCall.Arguments.Count < 2)
                return null;

            string? outputStr = ExtractString(baseCall.Arguments[0]);
            string? inputStr  = ExtractString(baseCall.Arguments[1]);

            if (outputStr == null || inputStr == null)
                return null;

            return new MinecraftStandardRecipeConfig
            {
                Type        = recipeType,
                Output      = ParseItemComponent(outputStr),
                Input       = ParseItemComponent(inputStr),
                Experience  = xp,
                SourceStart = node!.Range.Start,
                SourceEnd   = node!.Range.End,
            };
        }

        private static bool TryGetRecipeType(Node? node, out MinecraftRecipeType type)
        {
            type = default;

            // Поддерживаем как прямой вызов event.smelting(...),
            // так и цепочку event.smelting(...).xp(...)
            var call = UnwrapChainStatic(node);
            if (call is null) return false;

            if (call.Callee is not MemberExpression member) return false;

            var obj  = AstHelper.ExtractStringValue(member.Object);
            var prop = AstHelper.ExtractStringValue(member.Property);

            if (obj != "event") return false;
            if (prop == null)   return false;

            return _methods.TryGetValue(prop, out type);
        }

        /// <summary>
        /// Разворачивает цепочку вызовов и извлекает .xp() если есть.
        /// event.smoking('out','in').xp(0.5) → baseCall = event.smoking(...), xp = 0.5
        /// </summary>
        private static CallExpression UnwrapChain(CallExpression root, out float? xp)
        {
            xp = null;
            var current = root;

            while (current.Callee is MemberExpression member)
            {
                var prop = AstHelper.ExtractStringValue(member.Property);

                if (prop == "xp" && current.Arguments.Count == 1
                    && current.Arguments[0] is Literal lit && lit.Value is double d)
                {
                    xp = (float)d;
                }

                if (member.Object is CallExpression inner)
                    current = inner;
                else
                    break;
            }

            return current;
        }

        private static CallExpression? UnwrapChainStatic(Node? node)
        {
            var current = node as CallExpression;
            while (current?.Callee is MemberExpression m && m.Object is CallExpression inner)
                current = inner;
            return current;
        }

        private static string? ExtractString(Node node)
        {
            if (node is Literal lit && lit.Value is string s) return s;
            return null;
        }

        /// <summary>
        /// Парсит строку вида "minecraft:iron_ingot" или "2x minecraft:iron_ingot"
        /// </summary>
        private static ItemComponent ParseItemComponent(string str)
        {
            // KubeJS формат: "2x minecraft:item" или просто "minecraft:item"
            var parts = str.Split(' ');
            if (parts.Length == 2 && parts[0].EndsWith("x")
                && int.TryParse(parts[0].TrimEnd('x'), out int amount))
            {
                return new ItemComponent(parts[1], amount);
            }
            return new ItemComponent(str, 1);
        }
    }
}

using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.RawCode;
using System;
using System.Text.Json.Nodes;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Извлекает вызовы event.custom({...}) в CustomRecipeConfig.
    /// Поддерживает рецепты с произвольной структурой (Create, GregTech, и др.).
    /// </summary>
    public class CustomExtractor : IRecipeExtractor
    {
        public bool CanHandle(Node? node)
        {
            if (node is not CallExpression call)
                return false;

            // Инкапсулированная проверка — логика внутри экстрактора
            return IsEventCustom(call);
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            if (node is not CallExpression call)
                return null;

            if (call.Arguments.Count == 0)
                return null;

            var recipeArg = call.Arguments[0];

            if (recipeArg is not ObjectExpression objExpr)
                return null;

            var jsonObject = AstHelper.ExtractObjectExpression(objExpr);
            if (jsonObject == null)
                return null;

            var config = CustomRecipeConfig.FromJsonObject(jsonObject);

            if (jsonObject.TryGetPropertyValue("id", out var idNode) && idNode is JsonValue idVal)
            {
                config.RecipeId = idVal.ToString();
            }

            return config;
        }

        /// <summary>
        /// Проверяет, является ли вызов event.custom(...)
        /// Приватный метод — логика инкапсулирована внутри экстрактора
        /// </summary>
        private static bool IsEventCustom(CallExpression call)
        {
            if (call.Callee is MemberExpression member)
            {
                var obj = AstHelper.ExtractStringValue(member.Object);
                var prop = AstHelper.ExtractStringValue(member.Property);
                return obj == "event" && prop == "custom";
            }
            return false;
        }
    }
}
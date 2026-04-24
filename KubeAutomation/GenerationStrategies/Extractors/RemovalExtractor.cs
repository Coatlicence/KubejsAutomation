using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Извлекает event.remove({...}) → RecipeRemovalConfig
    /// </summary>
    public class RemovalExtractor : IRecipeExtractor
    {
        public bool CanHandle(Node? node)
        {
            if (node is not CallExpression call)
                return false;

            // Инкапсулированная проверка — логика внутри экстрактора
            return IsEventRemove(call);
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            if (node is not CallExpression call)
                return null;

            if (call.Arguments.Count == 0)
                return null;

            var filterArg = call.Arguments[0];

            RecipeFilter? filter = null;

            if (filterArg is ObjectExpression objExpr)
            {
                filter = AstHelper.ExtractRecipeFilter(objExpr);
            }

            if (filter == null)
                return null;

            return new RecipeRemovalConfig
            {
                Filters = new() { filter }
            };
        }

        /// <summary>
        /// Проверяет, является ли вызов event.remove(...)
        /// Приватный метод — логика инкапсулирована внутри экстрактора
        /// </summary>
        private static bool IsEventRemove(CallExpression call)
        {
            if (call.Callee is MemberExpression member)
            {
                var obj = AstHelper.ExtractStringValue(member.Object);
                var prop = AstHelper.ExtractStringValue(member.Property);
                return obj == "event" && prop == "remove";
            }
            return false;
        }
    }
}
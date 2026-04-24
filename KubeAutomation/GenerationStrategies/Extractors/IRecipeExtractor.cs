using Esprima.Ast;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Интерфейс для извлечения конфигов из AST-узлов.
    /// Комментарии не поддерживаются.
    /// </summary>
    public interface IRecipeExtractor
    {
        /// <summary>
        /// Проверяет, может ли экстрактор обработать узел
        /// </summary>
        bool CanHandle(Node? node);

        /// <summary>
        /// Извлекает конфигурацию из узла
        /// </summary>
        BaseRecipeConfiguration? Extract(Node? node);
    }
}
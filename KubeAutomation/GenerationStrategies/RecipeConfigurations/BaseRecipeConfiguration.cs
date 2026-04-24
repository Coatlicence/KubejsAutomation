using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    /// <summary>
    /// Базовый абстрактный класс для всех конфигураций рецептов.
    /// Определяет минимальный контракт, который должен реализовать каждый конкретный рецепт.
    /// </summary>
    /// <remarks>
    /// Наследуйте свои конфиги от этого класса, чтобы обеспечить:
    /// - Единый способ валидации данных
    /// - Полиморфную генерацию JS-кода
    /// - Уникальную идентификацию типа рецепта
    /// </remarks>
    public abstract class BaseRecipeConfiguration
    {
        /// <summary>
        /// Уникальный идентификатор рецепта (опционально).
        /// Используется для поиска, замены или удаления рецепта в файле.
        /// Пример: "minecraft:iron_ingot_from_smelting"
        /// </summary>
        public string? RecipeId { get; set; }

        /// <summary>
        /// Строковый идентификатор типа рецепта.
        /// Используется для классификации при парсинге файлов и выбора стратегии генерации.
        /// </summary>
        /// <example>
        /// "minecraft:crafting_shaped" для ShapedRecipeConfig
        /// "create:sequenced_assembly" для CustomRecipeConfig
        /// "gtceu:alloy_smelter" для GregTechRecipeConfig
        /// </example>
        public abstract string RecipeTypeId { get; }

        /// <summary>
        /// Позиция начала элемента в исходном файле (символы).
        /// -1 если позиция не известна.
        /// </summary>
        public int SourceStart { get; set; } = -1;

        /// <summary>
        /// Позиция конца элемента в исходном файле (символы).
        /// -1 если позиция не известна.
        /// </summary>
        public int SourceEnd { get; set; } = -1;

        /// <summary>
        /// Был ли элемент извлечён с позицией.
        /// </summary>
        public bool HasSourcePosition => SourceStart >= 0;

        /// <summary>
        /// Валидирует внутренние данные рецепта.
        /// Должен выбрасывать исключение или возвращать ошибки при некорректных данных.
        /// </summary>
        /// <exception cref="RecipeValidationException">Если данные не прошли валидацию</exception>
        public abstract void Validate();

        /// <summary>
        /// Генерирует JavaScript-код для вызова KubeJS.
        /// Возвращает готовую строку, которую можно записать в файл.
        /// </summary>
        /// <returns>JS-код, например: "event.shaped('item', ['A'], { A: 'tag' })"</returns>
        /// <remarks>
        /// Реализация должна учитывать:
        /// - Правильный синтаксис KubeJS
        /// - Экранирование строк
        /// - Отступы (если метод возвращает фрагмент для вставки)
        /// </remarks>
        public abstract string GenerateJsCode();

        /// <summary>
        /// Экранирует строку для использования в JavaScript-коде.
        /// </summary>
        /// <param name="value">Строка для экранирования</param>
        /// <returns>Экранированная строка</returns>
        protected static string EscapeJs(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }

        /// <summary>
        /// Экранирует строку и оборачивает в одинарные кавычки для JS.
        /// </summary>
        /// <param name="value">Строка для экранирования</param>
        /// <returns>Строка в кавычках: 'value'</returns>
        protected static string ToQuotedJs(string value)
        {
            return $"'{EscapeJs(value)}'";
        }
    }

    /// <summary>
    /// Исключение, выбрасываемое при ошибке валидации рецепта.
    /// Позволяет отличить ошибки данных от программных багов.
    /// </summary>
    public class RecipeValidationException : System.Exception
    {
        public RecipeValidationException(string message) : base(message) { }

        public RecipeValidationException(string message, System.Exception inner)
            : base(message, inner) { }

        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
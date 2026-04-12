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
        /// Используется для классификации при парсинге файлов и выборе стратегии генерации.
        /// </summary>
        /// <example>
        /// "minecraft:crafting_shaped" для ShapedRecipeConfig
        /// "create:sequenced_assembly" для CustomRecipeConfig
        /// "gtceu:alloy_smelter" для GregTechRecipeConfig
        /// </example>
        public abstract string RecipeTypeId { get; }

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

        public IEnumerable<string> Errors { get; set; } = [];
    }


}
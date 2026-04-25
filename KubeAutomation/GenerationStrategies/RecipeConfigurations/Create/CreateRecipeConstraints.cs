using System.Collections.Generic;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Ограничения и метаданные для UI-формы конкретного типа рецепта
    /// </summary>
    public class CreateRecipeConstraints
    {
        /// <summary>
        /// Минимальное количество входов
        /// </summary>
        public int MinInputs { get; set; } = 1;

        /// <summary>
        /// Максимальное количество входов (null = не ограничено)
        /// </summary>
        public int? MaxInputs { get; set; } = null;

        /// <summary>
        /// Минимальное количество выходов
        /// </summary>
        public int MinOutputs { get; set; } = 1;

        /// <summary>
        /// Максимальное количество выходов (null = не ограничено)
        /// </summary>
        public int? MaxOutputs { get; set; } = null;

        /// <summary>
        /// Поддерживаемые модификаторы
        /// </summary>
        public List<CreateModifierType> SupportedModifiers { get; set; } = new();

        /// <summary>
        /// Разрешены ли шансы для выходов
        /// </summary>
        public bool AllowChanceOutputs { get; set; } = true;

        /// <summary>
        /// Разрешены ли флюиды в качестве входов
        /// </summary>
        public bool AllowFluidInputs { get; set; } = false;

        /// <summary>
        /// Разрешены ли флюиды в качестве выходов
        /// </summary>
        public bool AllowFluidOutputs { get; set; } = false;

        /// <summary>
        /// Фиксированное количество входов (null = не фиксировано)
        /// </summary>
        public int? FixedInputCount { get; set; } = null;

        /// <summary>
        /// Фиксированное количество выходов (null = не фиксировано)
        /// </summary>
        public int? FixedOutputCount { get; set; } = null;
    }

    /// <summary>
    /// Поддерживаемые модификаторы
    /// </summary>
    public enum CreateModifierType
    {
        Heated,
        Superheated,
        ProcessingTime,
        KeepHeldItem
    }
}
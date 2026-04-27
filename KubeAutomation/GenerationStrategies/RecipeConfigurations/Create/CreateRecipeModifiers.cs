using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Модификаторы для Create-рецептов (heated, processingTime, и т.д.)
    /// </summary>
    public class CreateRecipeModifiers
    {
        /// <summary>
        /// Тип тепла (для compacting, mixing)
        /// </summary>
        public CreateHeatType? Heat { get; set; }

        /// <summary>
        /// Время обработки (для crushing, cutting)
        /// </summary>
        public int? ProcessingTime { get; set; }

        /// <summary>
        /// Сохранить удерживаемый предмет (для deploying)
        /// </summary>
        public bool? KeepHeldItem { get; set; }

        /// <summary>
        /// Генерирует цепочку модификаторов: .heated().processingTime(500)
        /// </summary>
        public string GenerateModifierChain()
        {
            var modifiers = new List<string>();

            if (Heat == CreateHeatType.Heated)
                modifiers.Add(".heated()");
            else if (Heat == CreateHeatType.Superheated)
                modifiers.Add(".superheated()");

            if (ProcessingTime.HasValue)
                modifiers.Add($".processingTime({ProcessingTime.Value})");

            if (KeepHeldItem == true)
                modifiers.Add(".keepHeldItem()");

            return string.Join("", modifiers);
        }

        /// <summary>
        /// Валидация модификаторов для конкретного типа рецепта
        /// </summary>
        public List<string> ValidateForRecipeType(CreateRecipeType recipeType)
        {
            var errors = new List<string>();

            // Проверяем, какие модификаторы поддерживаются для этого типа
            if (Heat.HasValue)
            {
                if (recipeType != CreateRecipeType.Compacting && recipeType != CreateRecipeType.Mixing)
                {
                    errors.Add($"Модификатор Heat не поддерживается для {recipeType}");
                }
            }

            if (ProcessingTime.HasValue)
            {
                if (recipeType != CreateRecipeType.Crushing && recipeType != CreateRecipeType.Cutting)
                {
                    errors.Add($"Модификатор ProcessingTime не поддерживается для {recipeType}");
                }
            }

            if (KeepHeldItem.HasValue)
            {
                if (recipeType != CreateRecipeType.Deploying)
                {
                    errors.Add($"Модификатор KeepHeldItem не поддерживается для {recipeType}");
                }
            }

            return errors;
        }
    }

    /// <summary>
    /// Тип тепла для рецептов
    /// </summary>
    public enum CreateHeatType
    {
        Heated,
        Superheated
    }
}
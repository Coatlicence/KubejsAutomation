using System;
using System.Collections.Generic;
using System.Text;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Базовый класс для всех Create-рецептов
    /// </summary>
    public abstract class BaseCreateRecipeConfig : BaseRecipeConfiguration
    {
        public override string RecipeTypeId => "create_base";

        /// <summary>
        /// Выходные предметы (могут быть с шансами)
        /// </summary>
        public List<CreateItemComponent> Outputs { get; set; } = new();

        /// <summary>
        /// Входные ингредиенты
        /// </summary>
        public List<CreateIngredientComponent> Inputs { get; set; } = new();

        /// <summary>
        /// Модификаторы (heated, processingTime и т.д.)
        /// </summary>
        public CreateRecipeModifiers Modifiers { get; set; } = new();

        /// <summary>
        /// Получает ограничения для конкретного типа рецепта
        /// </summary>
        public abstract CreateRecipeConstraints GetConstraints();

        public override void Validate()
        {
            var constraints = GetConstraints();
            var errors = new List<string>();

            // Проверка выходов
            if (Outputs == null || Outputs.Count == 0)
                errors.Add("Outputs не может быть пустым");

            if (constraints.FixedOutputCount.HasValue)
            {
                if (Outputs.Count != constraints.FixedOutputCount.Value)
                    errors.Add($"Количество выходов должно быть ровно {constraints.FixedOutputCount.Value}, получено: {Outputs.Count}");
            }
            else
            {
                if (constraints.MinOutputs > 0 && Outputs.Count < constraints.MinOutputs)
                    errors.Add($"Количество выходов должно быть не менее {constraints.MinOutputs}, получено: {Outputs.Count}");

                if (constraints.MaxOutputs.HasValue && Outputs.Count > constraints.MaxOutputs.Value)
                    errors.Add($"Количество выходов не должно превышать {constraints.MaxOutputs.Value}, получено: {Outputs.Count}");
            }

            // Проверка входов
            if (Inputs == null || Inputs.Count == 0)
                errors.Add("Inputs не может быть пустым");

            if (constraints.FixedInputCount.HasValue)
            {
                if (Inputs.Count != constraints.FixedInputCount.Value)
                    errors.Add($"Количество входов должно быть ровно {constraints.FixedInputCount.Value}, получено: {Inputs.Count}");
            }
            else
            {
                if (constraints.MinInputs > 0 && Inputs.Count < constraints.MinInputs)
                    errors.Add($"Количество входов должно быть не менее {constraints.MinInputs}, получено: {Inputs.Count}");

                if (constraints.MaxInputs.HasValue && Inputs.Count > constraints.MaxInputs.Value)
                    errors.Add($"Количество входов не должно превышать {constraints.MaxInputs.Value}, получено: {Inputs.Count}");
            }

            // Проверка модификаторов
            var modifierErrors = ValidateModifiers(constraints);
            errors.AddRange(modifierErrors);

            // Проверка флюидов
            var fluidErrors = ValidateFluids(constraints);
            errors.AddRange(fluidErrors);

            if (errors.Count > 0)
            {
                throw new RecipeValidationException($"Валидация {GetType().Name} не пройдена")
                {
                    Errors = errors
                };
            }
        }

        /// <summary>
        /// Валидация модификаторов
        /// </summary>
        private List<string> ValidateModifiers(CreateRecipeConstraints constraints)
        {
            var errors = new List<string>();

            if (Modifiers.Heat.HasValue && !constraints.SupportedModifiers.Contains(CreateModifierType.Heated))
                errors.Add($"Модификатор Heat не поддерживается для {GetType().Name}");

            if (Modifiers.ProcessingTime.HasValue && !constraints.SupportedModifiers.Contains(CreateModifierType.ProcessingTime))
                errors.Add($"Модификатор ProcessingTime не поддерживается для {GetType().Name}");

            if (Modifiers.KeepHeldItem.HasValue && !constraints.SupportedModifiers.Contains(CreateModifierType.KeepHeldItem))
                errors.Add($"Модификатор KeepHeldItem не поддерживается для {GetType().Name}");

            return errors;
        }

        /// <summary>
        /// Валидация использования флюидов
        /// </summary>
        private List<string> ValidateFluids(CreateRecipeConstraints constraints)
        {
            var errors = new List<string>();

            // Проверка входов
            foreach (var input in Inputs)
            {
                if (input is CreateFluidIngredientComponent && !constraints.AllowFluidInputs)
                    errors.Add($"Флюиды в качестве входов не поддерживаются для {GetType().Name}");
            }

            // Проверка выходов
            foreach (var output in Outputs)
            {
                if (output.ItemId.Contains("Fluid.of") && !constraints.AllowFluidOutputs)
                    errors.Add($"Флюиды в качестве выходов не поддерживаются для {GetType().Name}");
            }

            return errors;
        }

        /// <summary>
        /// Генерирует цепочку модификаторов
        /// </summary>
        protected string GenerateModifierChain()
        {
            return Modifiers.GenerateModifierChain();
        }
    }
}
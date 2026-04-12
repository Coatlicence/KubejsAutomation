using KubeScriptAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    public class GregTechRecipeConfiguration : BaseRecipeConfiguration
    {
        public override string RecipeTypeId => "gtceu";

        public MachineType MachineType { get; set; }
        public List<FluidComponent> InputFluids { get; set; } = new List<FluidComponent>();
        public List<FluidComponent> OutputFluids { get; set; } = new List<FluidComponent>();
        public List<ItemComponent> InputItems { get; set; } = new List<ItemComponent>();
        public List<ItemComponent> OutputItems { get; set; } = new List<ItemComponent>();
        public int? CircuitSetting { get; set; }
        public string? NotConsumableItemId { get; set; }
        public int DurationSeconds { get; set; }
        public int EUt { get; set; }
        public IRecipeType RecipeType { get; set; }

        public override void Validate()
        {
            var errors = new List<string>();

            bool hasEnter = InputItems.Count != 0 || InputFluids.Count != 0;
            bool hasExit = OutputFluids.Count != 0 || OutputItems.Count != 0;  // ✅ Исправлено

            if (!hasEnter)
                errors.Add("Рецепт должен иметь хотя бы один вход (предмет или жидкость)");

            if (!hasExit)
                errors.Add("Рецепт должен иметь хотя бы один выход (предмет или жидкость)");

            if (string.IsNullOrEmpty(RecipeId))
                errors.Add("RecipeId не может быть пустым");

            if (DurationSeconds <= 0)
                errors.Add("DurationSeconds должен быть > 0");

            if (EUt <= 0)
                errors.Add("EUt должен быть > 0");

            // Выбрасываем исключение с коллекцией ошибок
            if (errors.Count > 0)
            {
                throw new RecipeValidationException("GregTech recipe validation failed")
                {
                    Errors = errors
                };
            }
        }

        /// <summary>
        /// Генерирует JavaScript-код для GregTech рецепта.
        /// </summary>
        public override string GenerateJsCode()
        {
            Validate();

            var lines = new List<string>
            {
                "    event.recipes.gtceu",
                $"        .{MachineType.Name}(`{RecipeId}`)"
            };

            // Входные жидкости
            if (InputFluids?.Count > 0)
            {
                var fluidInputs = string.Join(", ", InputFluids.Select(f => $"`{f.FluidId} {f.Amount}`"));
                lines.Add($"        .inputFluids({fluidInputs})");
            }

            // Входные предметы
            if (InputItems?.Count > 0)
            {
                var itemInputs = string.Join(", ", InputItems.Select(i => $"`{i.Amount}x {i.ItemId}`"));
                lines.Add($"        .itemInputs({itemInputs})");
            }

            // Не потребляемый предмет
            if (!string.IsNullOrEmpty(NotConsumableItemId))
            {
                lines.Add($"        .notConsumable(`{NotConsumableItemId}`)");
            }

            // Выходные жидкости
            if (OutputFluids?.Count > 0)
            {
                var fluidOutputs = string.Join(", ", OutputFluids.Select(f => $"`{f.FluidId} {f.Amount}`"));
                lines.Add($"        .outputFluids({fluidOutputs})");
            }

            // Выходные предметы
            if (OutputItems?.Count > 0)
            {
                var itemOutputs = string.Join(", ", OutputItems.Select(i => $"`{i.Amount}x {i.ItemId}`"));
                lines.Add($"        .itemOutputs({itemOutputs})");
            }

            // Интегральный чип
            if (CircuitSetting.HasValue)
            {
                lines.Add($"        .circuit({CircuitSetting.Value})");
            }

            // Базовые параметры
            lines.Add($"        .duration({DurationSeconds}*20)");
            lines.Add($"        .EUt({EUt});");

            return string.Join(Environment.NewLine, lines);
        }


    }
}

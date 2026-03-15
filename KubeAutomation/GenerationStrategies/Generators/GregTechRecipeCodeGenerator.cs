using KubeScriptAutomation.Collectos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeScriptAutomation;

namespace KubeAutomation.GenerationStrategies.Generators
{
    public static class GregTechRecipeCodeGenerator
    {
        public static List<MachineType> Machines { get; } =
        [
            new("evaporation", 1, 1, 1, 1),
            new("brewery", 1, 1, 1, 1),
            new("chemical_reactor", 3, 2, 3, 2),
            new("large_chemical_reactor", 3, 3, 3, 3)
        ];

        public static string Generate(GregTechRecipeConfiguration config)
        {
            config.Validate();

            var lines = new List<string>
            {
            "    event.recipes.gtceu",
            $"        .{config.MachineType.Name}(`{config.RecipeId}`)"
            };

            // Добавляем входные жидкости, если они есть
            if (config.InputFluids?.Count > 0)
            {
                var fluidInputs = string.Join(", ", config.InputFluids.Select(f => $"`{f.FluidId} {f.Amount}`"));
                lines.Add($"        .inputFluids({fluidInputs})");
            }

            // Добавляем входные предметы, если они есть (максимум 3)
            if (config.InputItems?.Count > 0)
            {
                var itemInputs = string.Join(", ", config.InputItems.Select(i => $"`{i.Amount}x {i.ItemId}`"));
                lines.Add($"        .itemInputs({itemInputs})");
            }

            // Добавляем не потребляемый предмет, если он есть (только один)
            if (!string.IsNullOrEmpty(config.NotConsumableItemId))
            {
                lines.Add($"        .notConsumable(`{config.NotConsumableItemId}`)");
            }

            // Добавляем выходные жидкости, если они есть
            if (config.OutputFluids?.Count > 0)
            {
                var fluidOutputs = string.Join(", ", config.OutputFluids.Select(f => $"`{f.FluidId} {f.Amount}`"));
                lines.Add($"        .outputFluids({fluidOutputs})");
            }

            // Добавляем выходные предметы, если они есть (максимум 3)
            if (config.OutputItems?.Count > 0)
            {
                var itemOutputs = string.Join(", ", config.OutputItems.Select(i => $"`{i.Amount}x {i.ItemId}`"));
                lines.Add($"        .itemOutputs({itemOutputs})");
            }

            // Добавляем интегральный чип, если указан
            if (config.CircuitSetting.HasValue)
            {
                lines.Add($"        .circuit({config.CircuitSetting.Value})");
            }

            // Всегда добавляем базовые параметры
            lines.Add($"        .duration({config.DurationSeconds}*20)");
            lines.Add($"        .EUt({config.EUt});");


            return string.Join(Environment.NewLine, lines);
        }

        // ТОЛЬКО ДЛЯ CLI. в UI он не требуется.
        public class RecipeInputCollector : DataCollector
        {
            public override GregTechRecipeConfiguration Collect()
            {
                var config = new GregTechRecipeConfiguration { RecipeType = new ServerRecipeType() };

                Console.WriteLine("\n=== Создание нового рецепта GregTech ===");
                config.RecipeId = Prompt("Введите ID рецепта (например, salt_water_from_water):");

                // Получить машину
                while (true)
                {
                    Console.WriteLine("Существуют следующие типы машин: ");
                    foreach (var machine in Machines)
                    {
                        Console.Write($"{machine.Name} ");
                    }
                    Console.WriteLine(" ");

                    var machineNameFromPrompt = Prompt("Введите тип машины (например, evaporation):");

                    MachineType? machineType = null;
                    foreach (var machine in Machines)
                    {
                        if (machineNameFromPrompt == machine.Name)
                        {
                            machineType = machine;
                            break;
                        }
                    }

                    if (machineType != null)
                    {
                        config.MachineType = machineType.Value;
                        break;
                    }
                }

                // Сбор входных жидкостей
                Console.WriteLine("\n=== Входные жидкости ===");
                config.InputFluids = CollectFluidComponents("входную", config.MachineType.LiquidCount);

                // Сбор входных предметов
                Console.WriteLine("\n=== Входные предметы ===");
                Console.WriteLine("Хотите добавить входные предметы? (да/нет)");

                if (GetPositiveAnswer())
                {
                    config.InputItems = CollectItemComponents("входной", config.MachineType.ItemCount);
                }

                // Сбор не потребляемого предмета
                if (config.InputItems.Count > 0)
                {
                    Console.WriteLine("\nХотите добавить не потребляемый предмет? (да/нет)");
                    if (GetPositiveAnswer())
                    {
                        Console.WriteLine("Введите ID не потребляемого предмета (например, gtceu:rubber_sapling):");
                        config.NotConsumableItemId = Console.ReadLine()?.Trim();
                    }
                }

                // Сбор выходных жидкостей
                Console.WriteLine("\n=== Выходные жидкости ===");
                config.OutputFluids = CollectFluidComponents("выходной", config.MachineType.LiquidOut);

                // Сбор выходных предметов
                Console.WriteLine("\n=== Выходные предметы ===");
                Console.WriteLine("Хотите добавить выходные предметы? (да/нет)");
                if (GetPositiveAnswer())
                {
                    config.OutputItems = CollectItemComponents("выходной", config.MachineType.ItemOut);
                }

                // Сбор дополнительных параметров
                Console.WriteLine("\nХотите добавить интегральный чип? (да/нет)");
                if (GetPositiveAnswer())
                {
                    config.CircuitSetting = PromptIntInRange("Введите номер схемы (0-64):", 0, 64);
                }

                config.DurationSeconds = PromptPositiveInt("Введите длительность в секундах (должно быть >0):");
                config.EUt = PromptPositiveInt("Введите потребление энергии в EU/t (должно быть >0):");

                return config;
            }
        }

    }

}

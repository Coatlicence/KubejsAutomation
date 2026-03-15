using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeScriptAutomation.Collectos
{
    public abstract class DataCollector
    {
        public abstract GregTechRecipeConfiguration Collect();

        // Выводит сообщение в консоль и требует текстовый ввод
        protected string Prompt(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        // Запрос позитивного числа
        protected int PromptPositiveInt(string message)
        {
            while (true)
            {
                Console.WriteLine(message);
                Console.WriteLine("Диапозон: [1, 2147483647]");

                var valueStr = Console.ReadLine();

                bool parsed = int.TryParse(valueStr, out int value);
                bool valueBiggerZero = value > 0;

                if (parsed && valueBiggerZero)
                    return value;
                Console.WriteLine("Пожалуйста, введите положительное целое число без разделителей в указанном диапазоне!");
            }
        }

        // Запрос позитивного числа с включительными границами
        protected int PromptPositiveInt(string message, int minInclusive, int maxInclusive)
        {
            while (true)
            {
                Console.WriteLine(message);
                Console.WriteLine($"Диапазон включительный: [{minInclusive}, {maxInclusive}]");

                var valueStr = Console.ReadLine();

                bool parsed = int.TryParse(valueStr, out int value);
                bool valueBiggerMin    = value >= minInclusive;
                bool valueLowerMax     = value <= maxInclusive;
                bool valueCorrect   = valueBiggerMin && valueLowerMax;

                if (parsed && valueCorrect)
                    return value;
                Console.WriteLine("Пожалуйста, введите целое число в указанном диапазоне и без разделителей!");
            }
        }

        protected List<ItemComponent> CollectItemComponents(string type, uint maxItems)
        {
            var items = new List<ItemComponent>();
            Console.WriteLine($"Введите предметы (формат: '4x gtceu:fertilizer') или 'готово' для завершения (макс {maxItems}):");

            while (items.Count < maxItems)
            {
                Console.WriteLine($"Введите {type} предмет (осталось {maxItems - items.Count}):");
                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input) || input.ToLower() == "готово" || input.ToLower() == "done")
                    break;

                if (TryParseItemInput(input, out int amount, out string itemId))
                {
                    items.Add(new ItemComponent(amount, itemId));
                }
                else
                {
                    Console.WriteLine("Неверный формат. Пример: '4x gtceu:fertilizer'");
                }
            }

            return items;
        }

        protected bool TryParseItemInput(string input, out int amount, out string itemId)
        {
            amount = 0;
            itemId = string.Empty;

            // Проверяем формат "4x gtceu:fertilizer"
            var parts = input.Split('x', 2);
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0].Trim(), out amount) || amount < 1 || amount > 64)
                return false;

            itemId = parts[1].Trim();
            return !string.IsNullOrEmpty(itemId);
        }

        protected List<FluidComponent> CollectFluidComponents(string type, uint maxLiquids)
        {
            var fluids = new List<FluidComponent>();
            while (fluids.Count < maxLiquids)
            {
                Console.WriteLine($"Введите {type} жидкость или 'готово' для завершения");
                Console.WriteLine($"Осталось {maxLiquids - fluids.Count}");

                string id = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(id) || id.ToLower() == "готово" || id.ToLower() == "done" || id.ToLower() == "d")
                    break;

                int amount = PromptPositiveInt($"Введите количество жидкости {id} в mB:", 1, 4000);
                fluids.Add(new FluidComponent { FluidId = id, Amount = amount });
            }

            return fluids;
        }

        protected int PromptIntInRange(string message, int min, int max)
        {
            while (true)
            {
                Console.WriteLine($"{message} ({min}-{max})");
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Пожалуйста, введите число от {min} до {max}.");
            }
        }

        public static bool GetPositiveAnswer()
        {
            string res = Console.ReadLine()?.Trim().ToLower();

            if (res == "yes" || res == "y" || res == "да" || res == "д")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

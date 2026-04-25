using System;
using System.Text.RegularExpressions;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Компонент ингредиента (предмет, тег, флюид)
    /// </summary>
    public abstract class CreateIngredientComponent
    {
        /// <summary>
        /// Генерирует строку для KubeJS
        /// </summary>
        public abstract string ToJsString();

        /// <summary>
        /// Создаёт компонент из строки (пытается определить тип автоматически)
        /// </summary>
        public static CreateIngredientComponent Parse(string inputString)
        {
            // Проверяем: это флюид?
            var fluidMatch = Regex.Match(inputString, @"^Fluid\.of\(['""](.+?)['""],\s*(\d+)\)$", RegexOptions.IgnoreCase);
            if (fluidMatch.Success)
            {
                var fluidId = fluidMatch.Groups[1].Value;
                if (int.TryParse(fluidMatch.Groups[2].Value, out var amount))
                {
                    return new CreateFluidIngredientComponent { FluidId = fluidId, Amount = amount };
                }
            }

            // Проверяем: это тег (#tag)?
            if (inputString.StartsWith("#"))
            {
                return new CreateTagIngredientComponent { TagId = inputString };
            }

            // Проверяем: это Ingredient.of(...)?
            var ingredientMatch = Regex.Match(inputString, @"^Ingredient\.of\(['""](.+?)['""]\)$", RegexOptions.IgnoreCase);
            if (ingredientMatch.Success)
            {
                return new CreateItemIngredientComponent { ItemId = ingredientMatch.Groups[1].Value };
            }

            // Иначе — обычный предмет
            return new CreateItemIngredientComponent { ItemId = inputString };
        }
    }

    /// <summary>
    /// Простой предмет: 'minecraft:item'
    /// </summary>
    public class CreateItemIngredientComponent : CreateIngredientComponent
    {
        public string ItemId { get; set; } = "";

        public override string ToJsString()
        {
            return $"'{ItemId}'";
        }
    }

    /// <summary>
    /// Тег: '#minecraft:logs'
    /// </summary>
    public class CreateTagIngredientComponent : CreateIngredientComponent
    {
        public string TagId { get; set; } = "";

        public override string ToJsString()
        {
            return $"'{TagId}'";
        }
    }

    /// <summary>
    /// Флюид: Fluid.of('minecraft:water', 1000)
    /// </summary>
    public class CreateFluidIngredientComponent : CreateIngredientComponent
    {
        public string FluidId { get; set; } = "";
        public int Amount { get; set; } = 1000;

        public override string ToJsString()
        {
            return $"Fluid.of('{FluidId}', {Amount})";
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Generators
{
    public static class MinecraftStandartRecipesGenerator
    {
        public static readonly IReadOnlyList<string> Blocks =
        [
            "smelting",
            "blasting",
            "smoking",
            "stonecutting"
        ];

        public static string Generate(string block, ItemComponent outputItem, ItemComponent inputItem, float? experience = null)
        {
            var errors = Validate(block, outputItem, inputItem, experience);

            if (errors.Count > 0)
                throw new ArgumentException(string.Join("\n", errors));

            string indent = "    ";
            var baseCode = $"{indent}event.{block}('{outputItem.ToJsString()}', '{inputItem.ToJsString()}')";

            // Добавляем .xp() только для smoking
            if (block == "smoking" && experience.HasValue)
                baseCode += $".xp({experience.Value.ToString(CultureInfo.InvariantCulture)})";

            return baseCode;
        }

        public static List<string> Validate(string block, ItemComponent outputItem, ItemComponent inputItem, float? experience = null)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(block))
                errors.Add("block не может быть пустым");

            if (outputItem is null || string.IsNullOrEmpty(outputItem.ItemId))
                errors.Add("outputItem не может быть null");

            if (inputItem is null || string.IsNullOrEmpty(inputItem.ItemId))
                errors.Add("inputItem не может быть null");

            // Важно для работы
            if (errors.Count > 0) 
                return errors;

            if (!Blocks.Contains(block))
                errors.Add($"MinecraftStandartRecipesGenerator.Blocks не содержит {block}");

            if (outputItem!.Amount > 64 || outputItem.Amount < 1)
                errors.Add($"outputItem.Amount = {outputItem.Amount}, должен быть [1, 64]");

            if (inputItem!.Amount != 1)
                errors.Add($"inputItem.Amount = {inputItem.Amount}, должен быть 1");

            if (experience.HasValue && (experience.Value < 0 || experience.Value > 100))
                errors.Add($"experience = {experience.Value}, должен быть [0, 100]");

            if (experience.HasValue && block != "smoking")
                errors.Add($"Only smoking has experience value");

            if (inputItem.ItemId == outputItem.ItemId)
                errors.Add($"inputItem.ItemId cant be equal to outputItem.ItemId");

            return errors;
        }
    }
}
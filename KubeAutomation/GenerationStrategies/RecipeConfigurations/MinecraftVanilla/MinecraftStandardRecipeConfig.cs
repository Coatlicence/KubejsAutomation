using KubeAutomation.Instruments;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.MinecraftVanilla
{
    /// <summary>
    /// Типы стандартных рецептов Minecraft (печка, коптильня, камнерез)
    /// </summary>
    public enum MinecraftRecipeType
    {
        Smelting,     // event.smelting()
        Blasting,     // event.blasting()
        Smoking,      // event.smoking()
        Stonecutting  // event.stonecutting()
    }

    /// <summary>
    /// Конфигурация для стандартных рецептов Minecraft.
    /// Наследуется от BaseRecipeConfiguration для полиморфной генерации.
    /// </summary>
    public class MinecraftStandardRecipeConfig : BaseRecipeConfiguration
    {
        /// <summary>
        /// Идентификатор типа для парсинга
        /// </summary>
        public override string RecipeTypeId => "minecraft_standard";

        /// <summary>
        /// Тип рецепта (печка/дробитель/коптильня/камнерез)
        /// </summary>
        public MinecraftRecipeType Type { get; set; }

        /// <summary>
        /// Выходной предмет (результат рецепта)
        /// </summary>
        public ItemComponent Output { get; set; } = new();

        /// <summary>
        /// Входной предмет (ингредиент)
        /// </summary>
        public ItemComponent Input { get; set; } = new();

        /// <summary>
        /// Опыт за приготовление (только для Smoking)
        /// </summary>
        public float? Experience { get; set; }

        /// <summary>
        /// Валидация данных перед генерацией
        /// </summary>
        public override void Validate()
        {
            var errors = new List<string>();

            if (Output == null || string.IsNullOrEmpty(Output.ItemId))
                errors.Add("Output не может быть пустым");

            if (Input == null || string.IsNullOrEmpty(Input.ItemId))
                errors.Add("Input не может быть пустым");

            if (Output?.Amount is < 1 or > 64)
                errors.Add($"Output.Amount = {Output?.Amount}, должен быть [1, 64]");

            if (Input?.Amount != 1)
                errors.Add($"Input.Amount = {Input?.Amount}, должен быть 1");

            if (Experience.HasValue && (Experience.Value < 0 || Experience.Value > 100))
                errors.Add($"Experience = {Experience.Value}, должен быть [0, 100]");

            if (Experience.HasValue && Type != MinecraftRecipeType.Smoking)
                errors.Add("Experience поддерживается только для Smoking");

            if (Input?.ItemId == Output?.ItemId)
                errors.Add("Input и Output не могут быть одинаковыми");

            if (errors.Count > 0)
            {
                throw new RecipeValidationException("MinecraftStandardRecipeConfig validation failed")
                {
                    Errors = errors
                };
            }
        }

        /// <summary>
        /// Генерация JS-кода для KubeJS
        /// </summary>
        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";
            string blockName = GetBlockName();

            // Формируем базовый вызов: event.smelting('output', 'input')
            var baseCode = $"{indent}event.{blockName}('{Output.ToJsString()}', '{Input.ToJsString()}')";

            // Добавляем .xp() только для smoking
            if (Type == MinecraftRecipeType.Smoking && Experience.HasValue)
            {
                baseCode += $".xp({Experience.Value.ToString(CultureInfo.InvariantCulture)})";
            }

            return baseCode;
        }

        /// <summary>
        /// Возвращает имя метода KubeJS по типу рецепта
        /// </summary>
        private string GetBlockName()
        {
            return Type switch
            {
                MinecraftRecipeType.Smelting => "smelting",
                MinecraftRecipeType.Blasting => "blasting",
                MinecraftRecipeType.Smoking => "smoking",
                MinecraftRecipeType.Stonecutting => "stonecutting",
                _ => throw new InvalidOperationException($"Неизвестный тип рецепта: {Type}")
            };
        }

    }
}
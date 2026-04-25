using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations.Create
{
    /// <summary>
    /// Компонент предмета с поддержкой CreateItem.of(item, chance)
    /// </summary>
    public class CreateItemComponent
    {
        /// <summary>
        /// ID предмета (например, "minecraft:apple")
        /// </summary>
        public string ItemId { get; set; } = "";

        /// <summary>
        /// Количество предметов (из строки "2x minecraft:item")
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// Шанс выпадения (0.0 - 1.0), null = 100%
        /// </summary>
        public float? Chance { get; set; }

        /// <summary>
        /// Создаёт компонент из строки в формате "2x minecraft:item" или "minecraft:item"
        /// </summary>
        public static CreateItemComponent Parse(string itemString, float? chance = null)
        {
            var component = new CreateItemComponent();

            // Парсим префикс количества: "2x minecraft:item" → count = 2, itemId = "minecraft:item"
            var match = Regex.Match(itemString, @"^(\d+)x\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out var count))
                {
                    component.Count = count;
                    component.ItemId = match.Groups[2].Value.Trim();
                }
                else
                {
                    component.ItemId = itemString; // Если ошибка в парсинге числа — оставляем строку как есть
                }
            }
            else
            {
                component.ItemId = itemString;
            }

            component.Chance = chance;
            return component;
        }

        /// <summary>
        /// Генерирует строку для KubeJS: "CreateItem.of('2x minecraft:item', 0.5)" или "'minecraft:item'"
        /// </summary>
        public string ToJsString()
        {
            var itemPart = Count > 1 ? $"{Count}x {ItemId}" : ItemId;

            if (Chance.HasValue)
            {
                // Используем InvariantCulture для чисел с плавающей точкой
                return $"CreateItem.of('{itemPart}', {Chance.Value.ToString(CultureInfo.InvariantCulture)})";
            }
            else
            {
                return $"'{itemPart}'";
            }
        }
        /// <summary>
        /// Создаёт строку для обычного Item.of (без шанса)
        /// </summary>
        public string ToSimpleItemString()
        {
            var prefix = Count > 1 ? $"{Count}x " : "";
            return $"'{prefix}{ItemId}'";
        }
    }
}
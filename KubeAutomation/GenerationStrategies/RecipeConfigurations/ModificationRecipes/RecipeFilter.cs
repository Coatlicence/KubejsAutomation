using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    /// <summary>
    /// Фильтр для поиска рецептов при удалении или модификации.
    /// Поддерживает логику AND (внутри одного фильтра) и рекурсивный NOT.
    /// </summary>
    public class RecipeFilter
    {
        /// <summary>
        /// Фильтр по выходному предмету или тегу.
        /// Пример: "minecraft:stone_pickaxe" или "#minecraft:wool"
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Фильтр по входному предмету или тегу.
        /// Пример: "#forge:dusts/redstone"
        /// </summary>
        public string? Input { get; set; }

        /// <summary>
        /// Фильтр по типу рецепта.
        /// Пример: "minecraft:smelting", "minecraft:crafting_shaped"
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Фильтр по моду, создавшему рецепт.
        /// Пример: "farmersdelight", "create"
        /// </summary>
        public string? Mod { get; set; }

        /// <summary>
        /// Фильтр по уникальному ID рецепта.
        /// Пример: "minecraft:glowstone" (соответствует пути к JSON файлу рецепта)
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Рекурсивный фильтр для логики NOT.
        /// Пример: { not: { type: "minecraft:smelting" } }
        /// </summary>
        public RecipeFilter? Not { get; set; }

        /// <summary>
        /// Словарь для кастомных условий, не покрытых свойствами выше.
        /// Позволяет добавлять новые поля без изменения класса.
        /// </summary>
        public Dictionary<string, object?>? CustomConditions { get; set; }

        /// <summary>
        /// Генерирует JsonObject для этого фильтра.
        /// Используется при генерации JS-кода.
        /// </summary>
        /// <returns>JsonObject, готовый к сериализации в JSON</returns>
        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject();

            // Добавляем стандартные свойства, если они не null и не пустые
            AddIfNotEmpty(obj, "output", Output);
            AddIfNotEmpty(obj, "input", Input);
            AddIfNotEmpty(obj, "type", Type);
            AddIfNotEmpty(obj, "mod", Mod);
            AddIfNotEmpty(obj, "id", Id);

            // Рекурсивная обработка NOT
            if (Not != null)
            {
                obj["not"] = Not.ToJsonObject();
            }

            // Добавляем кастомные условия
            if (CustomConditions != null)
            {
                foreach (var kvp in CustomConditions)
                {
                    if (kvp.Value != null)
                    {
                        obj[kvp.Key] = JsonValue.Create(kvp.Value);
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Вспомогательный метод: добавляет свойство в объект, если значение не пустое.
        /// </summary>
        private static void AddIfNotEmpty(JsonObject obj, string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                obj[key] = value;
            }
        }

        /// <summary>
        /// Проверяет, является ли фильтр полностью пустым.
        /// Пустой фильтр { } удалит ВСЕ рецепты — обычно это нежелательно.
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Output)
                && string.IsNullOrEmpty(Input)
                && string.IsNullOrEmpty(Type)
                && string.IsNullOrEmpty(Mod)
                && string.IsNullOrEmpty(Id)
                && Not == null
                && (CustomConditions == null || CustomConditions.Count == 0);
        }
    }
}
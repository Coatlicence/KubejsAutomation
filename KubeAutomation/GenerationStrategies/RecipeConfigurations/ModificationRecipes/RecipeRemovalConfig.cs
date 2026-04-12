using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    /// <summary>
    /// Конфигурация для удаления рецептов через event.remove().
    /// Поддерживает множественные фильтры (логика OR между ними).
    /// </summary>
    public class RecipeRemovalConfig : BaseRecipeConfiguration
    {
        /// <summary>
        /// Идентификатор типа для парсинга и классификации.
        /// </summary>
        public override string RecipeTypeId => "removal";

        /// <summary>
        /// Список фильтров.
        /// Логика: фильтры в списке объединяются через OR.
        /// Поля внутри одного фильтра объединяются через AND.
        /// </summary>
        public List<RecipeFilter> Filters { get; set; } = [];

        /// <summary>
        /// Валидирует конфигурацию перед генерацией кода.
        /// </summary>
        /// <exception cref="RecipeValidationException">Если конфигурация невалидна</exception>
        /// <summary>
        /// Валидирует конфигурацию перед генерацией кода.
        /// </summary>
        /// <exception cref="RecipeValidationException">Если конфигурация невалидна</exception>
        public override void Validate()
        {
            var errors = new List<string>();

            // Проверка: должен быть хотя бы один фильтр
            if (Filters.Count == 0)
            {
                errors.Add("RecipeRemovalConfig должен содержать хотя бы один фильтр");
            }
            else
            {
                // Проверка каждого фильтра
                for (int i = 0; i < Filters.Count; i++)
                {
                    // ВАЖНО: Сначала проверяем на null!
                    if (Filters[i] == null)
                    {
                        errors.Add($"Фильтр #{i + 1} равен null");
                    }
                    // Только потом вызываем метод
                    else if (Filters[i].IsEmpty())
                    {
                        errors.Add($"Фильтр #{i + 1} пустой — это удалит ВСЕ рецепты! Убедитесь, что это намеренно.");
                    }
                }
            }

            // Выбрасываем исключение если есть ошибки
            if (errors.Count > 0)
            {
                throw new RecipeValidationException("RecipeRemovalConfig validation failed")
                {
                    Errors = errors
                };
            }
        }
        /// <summary>
        /// Генерирует JavaScript-код для вызова event.remove().
        /// </summary>
        /// <returns>JS-код, например: event.remove({ output: 'item' }) или event.remove([{...}, {...}])</returns>
        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";

            // Генерируем JSON для каждого фильтра
            var filterJsonStrings = Filters
                .Select(f => f!.ToJsonObject().ToJsonString())
                .ToList();

            // Если один фильтр → объект, если много → массив (логика OR)
            string filtersArg = filterJsonStrings.Count == 1
                ? filterJsonStrings[0]
                : $"[{string.Join(", ", filterJsonStrings)}]";

            return $"{indent}event.remove({filtersArg})";
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для удаления по моду.
        /// </summary>
        public static RecipeRemovalConfig ByMod(string modId)
        {
            return new RecipeRemovalConfig
            {
                Filters =
                [
                    new RecipeFilter { Mod = modId }
                ]
            };
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для удаления по выходу.
        /// </summary>
        public static RecipeRemovalConfig ByOutput(string output)
        {
            return new RecipeRemovalConfig
            {
                Filters =
                [
                    new RecipeFilter { Output = output }
                ]
            };
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для удаления по типу рецепта.
        /// </summary>
        public static RecipeRemovalConfig ByType(string recipeType)
        {
            return new RecipeRemovalConfig
            {
                Filters =
                [
                    new RecipeFilter { Type = recipeType }
                ]
            };
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для удаления по уникальному ID.
        /// </summary>
        public static RecipeRemovalConfig ById(string recipeId)
        {
            return new RecipeRemovalConfig
            {
                Filters =
                [
                    new RecipeFilter { Id = recipeId }
                ]
            };
        }
    }
}
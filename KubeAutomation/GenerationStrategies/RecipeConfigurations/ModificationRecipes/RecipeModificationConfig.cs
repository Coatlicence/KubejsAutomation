using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    /// <summary>
    /// Тип операции модификации рецепта
    /// </summary>
    public enum ModificationType
    {
        /// <summary>
        /// event.replaceInput() — замена ингредиента на входе
        /// </summary>
        ReplaceInput,

        /// <summary>
        /// event.replaceOutput() — замена ингредиента на выходе
        /// </summary>
        ReplaceOutput
    }

    /// <summary>
    /// Конфигурация для модификации рецептов через event.replaceInput() / event.replaceOutput().
    /// Позволяет массово заменять ингредиенты в существующих рецептах.
    /// 
    /// Пример KubeJS:
    /// event.replaceInput(
    ///   { input: 'minecraft:stick' },  // Фильтр: где искать
    ///   'minecraft:stick',              // Что заменяем
    ///   '#minecraft:saplings'          // На что заменяем
    /// )
    /// </summary>
    public class RecipeModificationConfig : BaseRecipeConfiguration
    {
        /// <summary>
        /// Идентификатор типа для парсинга и классификации
        /// </summary>
        public override string RecipeTypeId => "modification";

        /// <summary>
        /// Тип операции: replaceInput или replaceOutput
        /// </summary>
        public ModificationType Type { get; set; }

        /// <summary>
        /// Фильтр для поиска рецептов.
        /// Использует тот же класс RecipeFilter, что и RecipeRemovalConfig.
        /// Пример: { input: 'minecraft:stick' } или { mod: 'create', output: '#tag' }
        /// </summary>
        public RecipeFilter Filter { get; set; } = new();

        /// <summary>
        /// Предмет/тег/жидкость, который нужно заменить.
        /// Пример: 'minecraft:stick' или '#forge:ingots/iron'
        /// </summary>
        public string IngredientToReplace { get; set; } = "";

        /// <summary>
        /// Предмет/тег/жидкость, на который заменяем.
        /// Пример: '#minecraft:saplings' или 'minecraft:charcoal'
        /// </summary>
        public string ReplacementIngredient { get; set; } = "";

        /// <summary>
        /// Валидация конфигурации перед генерацией кода
        /// </summary>
        /// <exception cref="RecipeValidationException">Если конфигурация невалидна</exception>
        public override void Validate()
        {
            var errors = new List<string>();

            // Проверка фильтра
            if (Filter == null)
            {
                errors.Add("Filter не может быть null");
            }
            else if (Filter.IsEmpty())
            {
                errors.Add("Filter не может быть пустым — это применит замену ко ВСЕМ рецептам!");
            }

            // Проверка ингредиентов
            if (string.IsNullOrEmpty(IngredientToReplace))
            {
                errors.Add("IngredientToReplace не может быть пустым");
            }

            if (string.IsNullOrEmpty(ReplacementIngredient))
            {
                errors.Add("ReplacementIngredient не может быть пустым");
            }

            // Логическая проверка: если заменяем одно и то же — бессмысленно
            if (!string.IsNullOrEmpty(IngredientToReplace) &&
                !string.IsNullOrEmpty(ReplacementIngredient) &&
                IngredientToReplace == ReplacementIngredient)
            {
                errors.Add("IngredientToReplace и ReplacementIngredient не должны совпадать");
            }

            // Предупреждение (не ошибка): если фильтр и IngredientToReplace не согласованы
            // Например: фильтр по входному предмету, а заменяем выходной
            // Это не блокируем, но можно добавить логирование при необходимости

            if (errors.Count > 0)
            {
                throw new RecipeValidationException("RecipeModificationConfig validation failed")
                {
                    Errors = errors
                };
            }
        }

        /// <summary>
        /// Генерация JavaScript-кода для KubeJS
        /// </summary>
        /// <returns>JS-код, например: event.replaceInput({...}, 'old', 'new')</returns>
        public override string GenerateJsCode()
        {
            Validate();

            const string indent = "    ";

            // Выбираем имя метода
            string methodName = Type switch
            {
                ModificationType.ReplaceInput => "replaceInput",
                ModificationType.ReplaceOutput => "replaceOutput",
                _ => throw new InvalidOperationException($"Неизвестный тип модификации: {Type}")
            };

            // Генерируем JSON для фильтра
            string filterJson = Filter.ToJsonObject().ToJsonString();

            // Экранируем ингредиенты (добавляем кавычки для JS-строки)
            string oldIngredient = EscapeJsString(IngredientToReplace);
            string newIngredient = EscapeJsString(ReplacementIngredient);

            // Собираем финальный вызов
            return $"{indent}event.{methodName}({filterJson}, '{oldIngredient}', '{newIngredient}')";
        }

        /// <summary>
        /// Экранирует строку для использования в JS-коде
        /// </summary>
        private static string EscapeJsString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("\\", "\\\\")   // \ → \\
                .Replace("'", "\\'")     // ' → \'
                .Replace("\"", "\\\"")   // " → \"
                .Replace("\n", "\\n")    // перенос строки
                .Replace("\r", "\\r");   // возврат каретки
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для замены на входе
        /// </summary>
        public static RecipeModificationConfig ReplaceInput(
            RecipeFilter filter,
            string ingredientToReplace,
            string replacementIngredient)
        {
            return new RecipeModificationConfig
            {
                Type = ModificationType.ReplaceInput,
                Filter = filter,
                IngredientToReplace = ingredientToReplace,
                ReplacementIngredient = replacementIngredient
            };
        }

        /// <summary>
        /// Фабричный метод: создать конфиг для замены на выходе
        /// </summary>
        public static RecipeModificationConfig ReplaceOutput(
            RecipeFilter filter,
            string ingredientToReplace,
            string replacementIngredient)
        {
            return new RecipeModificationConfig
            {
                Type = ModificationType.ReplaceOutput,
                Filter = filter,
                IngredientToReplace = ingredientToReplace,
                ReplacementIngredient = replacementIngredient
            };
        }
    }
}
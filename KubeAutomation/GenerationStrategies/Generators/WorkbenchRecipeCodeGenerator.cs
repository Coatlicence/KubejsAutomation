using System;
using System.Collections.Generic;
using System.Linq;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Generators
{
    /// <summary>
    /// Генератор JS-кода для рецептов верстака (KubeJS shaped crafting)
    /// </summary>
    public static class WorkbenchRecipeCodeGenerator
    {
        /// <summary>
        /// Генерирует JS-фрагмент для рецепта верстака
        /// Формат: event.shaped(output, shape, keyMap)
        /// </summary>
        /// <param name="config">Конфигурация рецепта</param>
        /// <returns>JS-фрагмент (без wrapper, с отступом 4 пробела)</returns>
        public static string Generate(WorkbenchRecipeConfiguration config)
        {
            // ============================================================
            // ШАГ 1: Валидация конфигурации
            // ============================================================
            // Проверяем, что все обязательные поля заполнены
            // Если нет — выбросит ArgumentNullException
            config.Validate();

            // ============================================================
            // ШАГ 2: Извлечение данных из конфигурации
            // ============================================================
            // Получаем выходной предмет (гарантированно не null после валидации)
            var output = config.itemOutput;

            // Получаем паттерн (сетка + словарь соответствий, гарантированно не null)
            var pattern = config.pattern;

            // Получаем ID рецепта (гарантированно не null после валидации)
            var recipeId = config.RecipeId;

            // ============================================================
            // ШАГ 3: Формирование выходного предмета
            // ============================================================
            // Всегда используем Item.of(item, count) для единообразия
            // Даже если количество = 1, это упрощает парсинг в будущем
            var outputString = $"Item.of(\"{EscapeJs(output.ItemId)}\", {output.Amount})";

            // ============================================================
            // ШАГ 4: Формирование JS-кода рецепта
            // ============================================================
            // Базовый отступ: 4 пробела (для вставки в wrapper FileSaverToEnd)
            const string indent = "    ";

            // Добавляем базовый отступ ко всем строчкам, а не к самой первой
            var patternJs = pattern?.ToPatternJs();
            var indentedPatternJs = patternJs!.Replace("\n", "\n" + indent + "    ");

            var keys = pattern?.ToKeyMapJs();
            var intendedKeys = keys!.Replace("\n", "\n" + indent + "    ");

            // Собираем код построчно
            var lines = new List<string>
            {
                $"{indent}event.shaped(",
                $"{indent}    {outputString},",
                $"{indent}    {indentedPatternJs},",
                $"{indent}    {intendedKeys}",
                $"{indent})"
            };

            // ============================================================
            // ШАГ 5: Возврат результата
            // ============================================================
            var jsCode = string.Join(Environment.NewLine, lines);

            return jsCode;
        }

        /// <summary>
        /// Экранирует специальные символы для JS-строк
        /// </summary>
        private static string EscapeJs(string value)
        {
            return value
                .Replace("\\", "\\\\")   // \ → \\
                .Replace("\"", "\\\"")   // " → \"
                .Replace("\n", "\\n")    // перевод строки → \n
                .Replace("\r", "\\r")    // возврат каретки → \r
                .Replace("\t", "\\t");   // табуляция → \t
        }

        /// <summary>
        /// Создаёт пустую конфигурацию для UI (удобно для инициализации форм)
        /// </summary>
        public static WorkbenchRecipeConfiguration CreateEmptyConfig()
        {
            return new WorkbenchRecipeConfiguration();
        }
    }
}
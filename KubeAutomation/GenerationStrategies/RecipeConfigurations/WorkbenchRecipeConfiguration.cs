using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    public class WorkbenchRecipeConfiguration : BaseRecipeConfiguration
    {
        public override string RecipeTypeId => "workbench";

        /// <summary>
        /// Хранит рецепт создания предмета на верстаке (Сетка 3*3) с помощью 3 строк с 3 символами, 
        ///     прямо как в оригинальном KubeJS.
        /// Обеспечивает совместимость с KubeJS
        /// </summary>
        public readonly struct WorkbenchCraftGrid(string row1, string row2, string row3)
        {
            public string Row1 { get; } = Validate(row1);
            public string Row2 { get; } = Validate(row2);
            public string Row3 { get; } = Validate(row3);

            private static string Validate(string value)
            {
                ArgumentNullException.ThrowIfNull(value);

                if (value.Length != 3)
                    throw new ArgumentException($"String must be exactly 3 characters: '{value}'", nameof(value));
                return value;
            }

            /// <summary>
            /// Основной метод форматирования. 
            /// Используется только при создании рецептов
            /// </summary>
            /// <returns>Готовый JS-код, содержащий только паттерн</returns>
            public string ToString(string indent = "    ")
            {
                return
                    $"[\n" +
                    $"{indent}\"{Row1}\",\n" +
                    $"{indent}\"{Row2}\",\n" +
                    $"{indent}\"{Row3}\"\n" +
                    $"]";
            }

            /// <summary>
            /// Переопределение (вызывает версию с indent по умолчанию)
            /// </summary>
            public override string ToString() => ToString("    ");

            public string[] ToArray() => [Row1, Row2, Row3];
        }

        /// <summary>
        /// Покрывает каждый символ из сетки WorkbenchCraftGrid 
        ///     конкретным игровым предметом, то есть его ID.
        /// Обеспечивает совместимость с KubeJS 
        ///     и типом Dictionary из JavaScript
        /// </summary>
        public readonly struct RecipePattern
        {
            public WorkbenchCraftGrid Grid { get; }

            readonly IReadOnlyDictionary<char, string> KeyMap { get; }

            public RecipePattern(WorkbenchCraftGrid grid, Dictionary<char, string> keyMap)
            {
                Grid = grid;

                // Валидация при создании
                var errors = Validate(grid, keyMap);
                if (errors.Count > 0)
                    throw new ArgumentException(string.Join("\n", errors));

                KeyMap = keyMap;
            }

            private static List<string> Validate(WorkbenchCraftGrid grid, Dictionary<char, string> keyMap)
            {
                var errors = new List<string>();

                // 1. Получить все уникальные символы из сетки
                var gridChars = GetUniqueCharsFromGrid(grid);

                // Убираем отступ, так как он уже используется
                gridChars.Remove(' ');

                // 2. Получить все ключи из словаря
                var mapKeys = keyMap.Keys.ToHashSet();

                // 3. Проверка: все символы из сетки должны быть в словаре
                var missingInMap = gridChars.Except(mapKeys).ToList();
                if (missingInMap.Count > 0)
                    errors.Add($"Символы в сетке без покрытия: {string.Join(", ", missingInMap.Select(c => $"'{c}'"))}");

                // 4. Проверка: все ключи словаря должны использоваться в сетке
                var unusedInGrid = mapKeys.Except(gridChars).ToList();
                if (unusedInGrid.Count > 0)
                    errors.Add($"Неиспользуемые ключи в словаре: {string.Join(", ", unusedInGrid.Select(c => $"'{c}'"))}");

                // 5. Проверка: дубликаты ключей в словаре (на случай ручного добавления)
                if (keyMap.Count != mapKeys.Count)
                    errors.Add("Обнаружены дублирующиеся ключи в словаре");

                return errors;
            }

            private static HashSet<char> GetUniqueCharsFromGrid(WorkbenchCraftGrid grid)
            {
                var chars = new HashSet<char>();
                foreach (var row in grid.ToArray())
                    foreach (var c in row)
                        chars.Add(c);
                return chars;
            }

            private static string EscapeJs(string value)
            {
                return value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
            }

            private string KeyMapToJavaScript(string indent)
            {
                var lines = KeyMap
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{indent}{kvp.Key}: \"{EscapeJs(kvp.Value)}\"")
                    .ToList();

                // Закрывающая скобка с отступом на уровень меньше
                var closingIndent = indent.Length >= 4 ? indent.Substring(0, indent.Length - 4) : "";

                return "{\n" + string.Join(",\n", lines) + "\n" + closingIndent + "}";
            }

            /// <summary>
            /// Генерирует только массив паттерна (чистый JS-массив)
            /// </summary>
            public string ToPatternJs(string indent = "    ")
            {
                return Grid.ToString(indent);
            }

            /// <summary>
            /// Генерирует только объект ключей (чистый JS-объект)
            /// </summary>
            public string ToKeyMapJs(string indent = "    ")
            {
                return KeyMapToJavaScript(indent);
            }

            /// <summary>
            /// Для отладки: выводит оба аргумента друг за другом
            /// </summary>
            public override string ToString()
            {
                return $"{ToPatternJs()}\n{ToKeyMapJs()}";
            }
        }

        ///-------------------------------------------------------------------
        // ДАЛЕЕ ИДЁТ КОНФИГУРАЦИЯ РЕЦЕПТА ВЕРСТАКА


        /// <summary>
        /// Выходной предмет (или несколько), 
        /// который получается при создании рецепта на верстаке
        /// </summary>
        public ItemComponent? itemOutput;
        public RecipePattern? pattern;

        public override void Validate()
        {
            ArgumentNullException.ThrowIfNull(itemOutput);
            ArgumentNullException.ThrowIfNull(pattern);
        }
    }
}

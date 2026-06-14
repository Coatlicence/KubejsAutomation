using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KubeAutomation.GenerationStrategies.RecipeConfigurations.WorkbenchRecipeConfiguration;

namespace KubeAutomation.GenerationStrategies.RecipeConfigurations
{
    public class WorkbenchRecipeConfiguration : BaseRecipeConfiguration
    {
        public override string RecipeTypeId => "workbench";

        /// <summary>
        /// Выходной предмет (или несколько), 
        /// который получается при создании рецепта на верстаке
        /// </summary>
        public ItemComponent? itemOutput { get; set; }
        public RecipePattern? pattern { get; set; }


        [MemberNotNull(nameof(pattern), nameof(itemOutput))]
        public override void Validate()
        {
            ArgumentNullException.ThrowIfNull(itemOutput);
            ArgumentNullException.ThrowIfNull(pattern);
        }

        /// <summary>
        /// Генерирует JavaScript-код для shaped-рецепта верстака.
        /// Формат: event.shaped(output, pattern, keyMap)
        /// </summary>
        public override string GenerateJsCode()
        {
            // Валидация перед генерацией
            Validate();

            // Извлечение данных (гарантированно не null после валидации)
            var output = itemOutput;
            var pattern = this.pattern;

            // Формирование выходного предмета: всегда Item.of(id, count)
            var outputString = $"Item.of(\"{EscapeJs(output.ItemId)}\", {output.Amount})";

            // Базовый отступ для вставки в тело функции
            const string indent = "    ";

            // Получаем и форматируем паттерн (сетка)
            var patternJs = pattern?.ToPatternJs();
            var indentedPatternJs = patternJs!.Replace("\n", "\n" + indent + "    ");

            // Получаем и форматируем keyMap (словарь соответствий)
            var keysJs = pattern?.ToKeyMapJs();
            var indentedKeysJs = keysJs!.Replace("\n", "\n" + indent + "    ");

            // Собираем финальный JS-код
            var lines = new List<string>
            {
                $"{indent}event.shaped(",
                $"{indent}    {outputString},",
                $"{indent}    {indentedPatternJs},",
                $"{indent}    {indentedKeysJs}",
                $"{indent})"
            };

            return string.Join(Environment.NewLine, lines);
        }

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

        public string ToKeyMapJs()
        {
            const string indent = "    ";

            var lines = KeyMap
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{indent}{kvp.Key}: \"{EscapeJs(kvp.Value)}\"")
                .ToList();

            return "{\n" + string.Join(",\n", lines) + "\n}";
        }

        public Dictionary<char, string> ToKeyMapDict() =>
            KeyMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Генерирует только массив паттерна (чистый JS-массив)
        /// </summary>
        public string ToPatternJs()
        {
            return Grid.ToString();
        }

        /// <summary>
        /// Для отладки: выводит оба аргумента друг за другом
        /// </summary>
        public override string ToString()
        {
            return $"{ToPatternJs()},\n{ToKeyMapJs()}";
        }
    }


    /// <summary>
    /// Хранит паттерн рецепта верстака: от 1 до 3 строк, каждая от 1 до 3 символов.
    /// Совместимо с KubeJS (принимает неполные паттерны как 2x2, 1x3 и т.д.)
    /// </summary>
    public readonly struct WorkbenchCraftGrid
    {
        private readonly string[] _rows;

        public string Row1 => _rows.Length > 0 ? _rows[0] : "   ";
        public string Row2 => _rows.Length > 1 ? _rows[1] : "   ";
        public string Row3 => _rows.Length > 2 ? _rows[2] : "   ";

        public WorkbenchCraftGrid(string row1, string row2, string row3)
            : this([row1, row2, row3]) { }

        public WorkbenchCraftGrid(string[] rows)
        {
            if (rows == null || rows.Length == 0 || rows.Length > 3)
                throw new ArgumentException("Pattern must have 1 to 3 rows.", nameof(rows));

            foreach (var r in rows)
            {
                ArgumentNullException.ThrowIfNull(r);
                if (r.Length == 0 || r.Length > 3)
                    throw new ArgumentException($"Each row must be 1 to 3 characters: '{r}'", nameof(rows));
            }

            _rows = rows;
        }

        public override string ToString()
        {
            const string indent = "    ";
            var lines = string.Join(",\n", _rows.Select(r => $"{indent}\"{r}\""));
            return $"[\n{lines}\n]";
        }

        public string[] ToArray() => _rows.ToArray();
    }


}

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Generators
{
    public enum JsonType
    {
        String, Int, Float, Bool, Object, Array
    }

    public class CustomRecipeConfig : BaseRecipeConfiguration
    {
        private readonly JsonObject _root;

        public override string RecipeTypeId => "custom";

        public CustomRecipeConfig()
        {
            _root = new JsonObject();
        }

        /// <summary>
        /// 1. Создать свойство с типом
        /// Теперь корректно работает с путями типа "pattern[0]", "sequence[1].ingredients[0].item"
        /// </summary>
        public void CreateProperty(string path, JsonType type)
        {
            // Если путь содержит индекс массива — используем специальный метод
            if (path.Contains("["))
            {
                CreatePropertyWithArrayIndex(path, type);
                return;
            }

            var (parent, name) = SplitPath(path);
            var parentObj = GetOrCreateObject(parent);

            parentObj[name] = type switch
            {
                JsonType.Object => new JsonObject(),
                JsonType.Array => new JsonArray(),
                JsonType.String => "",
                JsonType.Int => 0,
                JsonType.Float => 0.0f,
                JsonType.Bool => false,
                _ => throw new ArgumentException($"Неизвестный тип: {type}")
            };
        }

        /// <summary>
        /// Новый метод: создание свойства с индексом массива
        /// </summary>
        /// <summary>
        /// Создание свойства с поддержкой индексов массивов
        /// Корректно обрабатывает: "pattern[0]", "sequence[0].ingredients", "sequence[0].ingredients[0].item"
        /// </summary>
        private void CreatePropertyWithArrayIndex(string path, JsonType type)
        {
            var segments = ParsePathSegments(path);
            JsonNode? current = _root;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                bool isLast = (i == segments.Count - 1);

                if (TryParseIndex(segment, out var name, out var index))
                {
                    // Сегмент с индексом: "sequence[0]"
                    var array = GetOrCreateArray(current, name);
                    EnsureArraySize(array, index + 1);

                    if (isLast)
                    {
                        // Последний сегмент — устанавливаем значение нужного типа
                        array[index] = CreateTypeNode(type);
                    }
                    else
                    {
                        // Промежуточный сегмент — гарантируем, что элемент существует как объект
                        if (array[index] is null)
                            array[index] = new JsonObject();
                        current = array[index];
                    }
                }
                else
                {
                    // Обычный сегмент: "ingredients"
                    if (current is JsonObject obj)
                    {
                        if (isLast)
                        {
                            // Последний сегмент — создаём свойство с нужным типом
                            obj[segment] = CreateTypeNode(type);
                        }
                        else
                        {
                            // Промежуточный сегмент — гарантируем, что свойство существует как объект
                            if (obj[segment] is null)
                                obj[segment] = new JsonObject();
                            current = obj[segment];
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Ожидается объект в {segment}, но получен {current?.GetType().Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод: создать узел нужного типа
        /// </summary>
        private JsonNode CreateTypeNode(JsonType type)
        {
            return type switch
            {
                JsonType.Object => new JsonObject(),
                JsonType.Array => new JsonArray(),
                JsonType.String => "",
                JsonType.Int => 0,
                JsonType.Float => 0.0f,
                JsonType.Bool => false,
                _ => throw new ArgumentException($"Неизвестный тип: {type}")
            };
        }


        /// <summary>
        /// 2. Назначить значение свойству. Создаёт элемент.
        /// Теперь корректно работает с путями типа "pattern[0]", "sequence[1].ingredients[0].item"
        /// </summary>
        public void SetValue(string path, object value)
        {
            // Если путь содержит индекс массива — используем специальный метод
            if (path.Contains("["))
            {
                SetValueWithArrayIndex(path, value);
                return;
            }

            var (parent, name) = SplitPath(path);
            var parentNode = GetOrCreateNode(parent);

            if (parentNode is JsonArray array)
            {
                var index = ParseIndex(name);
                EnsureArraySize(array, index + 1);
                array[index] = CreateValueNode(value);
            }
            else if (parentNode is JsonObject obj)
            {
                obj[name] = CreateValueNode(value);
            }
        }

        /// <summary>
        /// Новый метод: установка значения с индексом массива
        /// "pattern[0]" → устанавливает элемент массива pattern[0]
        /// "sequence[1].ingredients[0].item" → устанавливает глубоко вложенное значение
        /// </summary>
        private void SetValueWithArrayIndex(string path, object value)
        {
            var segments = ParsePathSegments(path);
            JsonNode? current = _root;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (TryParseIndex(segment, out var name, out var index))
                {
                    // Сегмент с индексом: "pattern[0]"
                    var array = GetOrCreateArray(current, name);
                    EnsureArraySize(array, index + 1);

                    if (i == segments.Count - 1)
                    {
                        // Последний сегмент — устанавливаем значение
                        array[index] = CreateValueNode(value);
                    }
                    else
                    {
                        // Промежуточный сегмент — идём глубже
                        if (array[index] is null)
                            array[index] = new JsonObject();
                        current = array[index];
                    }
                }
                else
                {
                    // Обычный сегмент: "ingredient"
                    if (current is JsonObject obj)
                    {
                        if (obj[segment] is null)
                            obj[segment] = new JsonObject();
                        current = obj[segment];
                    }
                    else
                    {
                        throw new InvalidOperationException($"Ожидается объект в {segment}");
                    }
                }
            }
        }

        /// <summary>
        /// 3. Перевод в JSON строку
        /// </summary>
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return _root.ToJsonString(options);
        }

        /// <summary>
        /// 4. Создать массив (если нужно явно создать пустой массив)
        /// </summary>
        public void CreateArray(string path)
        {
            CreateProperty(path, JsonType.Array);
        }

        /// <summary>
        /// 5. Добавить объект в массив → возвращает путь к новому объекту
        /// </summary>
        public string AddObjectToArray(string arrayPath)
        {
            var array = GetOrCreateArray(arrayPath);
            array.Add(new JsonObject());
            return $"{arrayPath}[{array.Count - 1}]";
        }

        /// <summary>
        /// 6. Добавить значение в массив (для примитивов: строки, числа)
        /// </summary>
        public void AddToArray(string arrayPath, object value)
        {
            var array = GetOrCreateArray(arrayPath);
            array.Add(CreateValueNode(value));
        }

        /// <summary>
        /// 7. Удалить элемент из JSON-документа
        /// </summary>
        /// <summary>
        /// 7. Удалить элемент из JSON-документа
        /// </summary>
        public void Remove(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var segments = ParsePathSegments(path);
            JsonNode? current = _root;

            // Проходим по всем сегментам, кроме последнего
            for (int i = 0; i < segments.Count - 1; i++)
            {
                var segment = segments[i];

                if (TryParseIndex(segment, out var name, out var index))
                {
                    if (current is JsonObject obj && obj[name] is JsonArray array)
                        current = (index < array.Count) ? array[index] : null;
                    else if (current is JsonArray arr)
                        current = (index < arr.Count) ? arr[index] : null;
                    else
                        return;
                }
                else
                {
                    if (current is JsonObject obj)
                        current = obj[segment];
                    else
                        return;
                }

                if (current == null) return;
            }

            // Последний сегмент — удаляем
            var lastSegment = segments[^1];

            if (TryParseIndex(lastSegment, out var lastName, out var lastIndex))
            {
                if (current is JsonObject obj && obj[lastName] is JsonArray array)
                {
                    if (lastIndex < array.Count)
                        array.RemoveAt(lastIndex);
                }
                else if (current is JsonArray arr && lastIndex < arr.Count)
                {
                    arr.RemoveAt(lastIndex);
                }
            }
            else
            {
                if (current is JsonObject obj)
                    obj.Remove(lastSegment);
            }
        }

        /// <summary>
        /// 8. Получить элемент из JSON-документа
        /// Теперь корректно работает с путями типа "pattern[0]", "sequence[1].ingredients[0].item"
        /// </summary>
        public object? GetValue(string path)
        {
            // Пустой путь → возвращаем корень
            if (string.IsNullOrEmpty(path))
                return _root;

            // Разбираем путь на сегменты: "a.b[0].c" → ["a", "b[0]", "c"]
            var segments = ParsePathSegments(path);
            JsonNode? current = _root;

            foreach (var segment in segments)
            {
                if (current == null)
                    return null;

                // Проверяем: это обращение к массиву? "results[0]"
                if (TryParseIndex(segment, out var name, out var index))
                {
                    // Случай 1: текущий узел — объект, внутри него массив
                    if (current is JsonObject obj && obj[name] is JsonArray array)
                    {
                        if (index < array.Count)
                            current = array[index];
                        else
                            return null; // Индекс за пределами массива
                    }
                    // Случай 2: текущий узел — сам массив (крайний случай)
                    else if (current is JsonArray arr)
                    {
                        if (index < arr.Count)
                            current = arr[index];
                        else
                            return null;
                    }
                    else
                    {
                        return null; // Не массив там, где ожидаем
                    }
                }
                // Обычное обращение к свойству объекта: "ingredient"
                else
                {
                    if (current is JsonObject obj)
                    {
                        current = obj[segment];
                    }
                    else
                    {
                        return null; // Не объект там, где ожидаем
                    }
                }
            }

            // Извлекаем значение из конечного узла
            if (current == null)
                return null;

            try
            {
                return current.GetValue<object>();
            }
            catch
            {
                // Если это объект/массив — GetValue() упадёт, возвращаем сам узел
                return current;
            }
        }

        // ============================================================
        // ВНУТРЕННЯЯ ЛОГИКА
        // ============================================================

        /// <summary>
        /// Разделяет путь на родительский путь и имя/индекс
        /// "a.b.c" → ("a.b", "c")
        /// "results[0].id" → ("results[0]", "id")
        /// "type" → ("", "type")
        /// </summary>
        private (string parent, string name) SplitPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return ("", "");

            // Ищем последнюю точку, но не внутри скобок
            var lastDot = -1;
            var bracketDepth = 0;

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == ']') bracketDepth++;
                if (path[i] == '[') bracketDepth--;
                if (path[i] == '.' && bracketDepth == 0)
                {
                    lastDot = i;
                    break;
                }
            }

            if (lastDot == -1)
                return ("", path);

            return (path.Substring(0, lastDot), path.Substring(lastDot + 1));
        }

        /// <summary>
        /// Получает или создаёт JsonNode по пути (объект или массив)
        /// </summary>
        private JsonNode GetOrCreateNode(string path)
        {
            if (string.IsNullOrEmpty(path))
                return _root;

            var segments = ParsePathSegments(path);
            JsonNode current = _root;

            foreach (var segment in segments)
            {
                if (TryParseIndex(segment, out var name, out var index))
                {
                    // Сегмент с индексом: "results[0]"
                    var array = GetOrCreateArray(current, name);
                    EnsureArraySize(array, index + 1);

                    if (array[index] is null)
                        array[index] = new JsonObject();

                    current = array[index];
                }
                else
                {
                    // Обычный сегмент: "transitional_item"
                    if (current is JsonObject obj)
                    {
                        if (obj[segment] is null)
                            obj[segment] = new JsonObject();
                        current = obj[segment];
                    }
                    else
                    {
                        throw new InvalidOperationException($"Ожидается объект в {segment}");
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Получает или создаёт JsonObject по пути
        /// </summary>
        private JsonObject GetOrCreateObject(string path)
        {
            var node = GetOrCreateNode(path);
            return node as JsonObject ?? throw new InvalidOperationException($"Ожидается объект в {path}");
        }

        /// <summary>
        /// Получает или создаёт JsonArray по пути
        /// </summary>
        private JsonArray GetOrCreateArray(string path)
        {
            var node = GetOrCreateNode(path);
            return node as JsonArray ?? throw new InvalidOperationException($"Ожидается массив в {path}");
        }

        /// <summary>
        /// Получает или создаёт массив в объекте
        /// </summary>
        private JsonArray GetOrCreateArray(JsonNode parent, string name)
        {
            if (parent is not JsonObject obj)
                throw new InvalidOperationException($"Ожидается объект для доступа к {name}");

            if (obj[name] is null)
                obj[name] = new JsonArray();

            return obj[name] as JsonArray ?? throw new InvalidOperationException($"Ожидается массив в {name}");
        }

        /// <summary>
        /// Разбирает путь на сегменты: "a.b[0].c" → ["a", "b[0]", "c"]
        /// </summary>
        private List<string> ParsePathSegments(string path)
        {
            var segments = new List<string>();
            var current = "";
            var bracketDepth = 0;

            foreach (var c in path)
            {
                if (c == '[') bracketDepth++;
                if (c == ']') bracketDepth--;

                if (c == '.' && bracketDepth == 0)
                {
                    segments.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
                segments.Add(current);

            return segments;
        }

        /// <summary>
        /// Проверяет: сегмент это массив с индексом?
        /// "results[0]" → true, name="results", index=0
        /// </summary>
        private bool TryParseIndex(string segment, out string name, out int index)
        {
            name = "";
            index = -1;

            var match = Regex.Match(segment, @"^([^\[]+)\[(\d+)\]$");
            if (match.Success)
            {
                name = match.Groups[1].Value;
                return int.TryParse(match.Groups[2].Value, out index);
            }

            return false;
        }

        /// <summary>
        /// Парсит индекс из имени (для SetValue с "results[0]")
        /// </summary>
        private int ParseIndex(string name)
        {
            var match = Regex.Match(name, @"^\[(\d+)\]$");
            if (match.Success)
                return int.Parse(match.Groups[1].Value);

            throw new ArgumentException($"Неверный формат индекса: {name}");
        }

        /// <summary>
        /// Гарантирует размер массива
        /// </summary>
        private void EnsureArraySize(JsonArray array, int minSize)
        {
            while (array.Count < minSize)
                array.Add(null);
        }

        /// <summary>
        /// Создаёт JsonNode из значения
        /// </summary>
        private JsonNode CreateValueNode(object value)
        {
            return value switch
            {
                string s => s,
                int i => i,
                long l => l,
                float f => f,
                double d => d,
                bool b => b,
                _ => throw new ArgumentException($"Неподдерживаемый тип: {value.GetType()}")
            };
        }





        public List<string> GetPathList()
        {
            var paths = new List<string>();
            CollectPaths(_root, "", paths);
            return paths;
        }

        private void CollectPaths(JsonNode? node, string currentPath, List<string> paths)
        {
            if (node is JsonObject obj)
            {
                foreach (var prop in obj)
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? prop.Key
                        : $"{currentPath}.{prop.Key}";

                    if (prop.Value is JsonObject or JsonArray)
                    {
                        CollectPaths(prop.Value, newPath, paths);
                    }
                    else
                    {
                        paths.Add(newPath);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var newPath = $"{currentPath}[{i}]";

                    if (arr[i] is JsonObject or JsonArray)
                    {
                        CollectPaths(arr[i], newPath, paths);
                    }
                    else
                    {
                        paths.Add(newPath);
                    }
                }
            }
        }

        /// <summary>
        /// Получить тип значения по пути
        /// </summary>
        public JsonType GetPathType(string path)
        {
            var value = GetValue(path);

            return value switch
            {
                string => JsonType.String,
                int or long => JsonType.Int,
                float or double => JsonType.Float,
                bool => JsonType.Bool,
                JsonObject => JsonType.Object,
                JsonArray => JsonType.Array,
                _ => JsonType.String
            };
        }

        /// <summary>
        /// Валидация для CustomRecipeConfig.
        /// Проверяет только базовые требования.
        /// </summary>
        public override void Validate()
        {
            var errors = new List<string>();

            // Проверка: тип рецепта должен быть указан (поле "type" в корне)
            var typeValue = GetValue("type") as string;
            if (string.IsNullOrEmpty(typeValue))
            {
                errors.Add("Поле 'type' обязательно для custom-рецепта");
            }

            // Проверка: корневой объект не должен быть пустым
            if (_root.Count == 0)
            {
                errors.Add("Рецепт не содержит свойств");
            }

            // Если есть ошибки — выбрасываем исключение
            if (errors.Count > 0)
            {
                throw new RecipeValidationException("CustomRecipeConfig validation failed")
                {
                    Errors = errors
                };
            }
        }

        /// <summary>
        /// Генерирует JavaScript-код для вызова event.custom()
        /// </summary>
        /// <returns>Строка вида: event.custom({ ... })</returns>
        public override string GenerateJsCode()
        {
            // Валидация перед генерацией (опционально, но рекомендуется)
            Validate();

            // Получаем отформатированный JSON
            string json = ToJson();

            // Оборачиваем в вызов event.custom()
            // Отступ "    " добавляется, если фрагмент вставляется в тело функции
            return $"    event.custom({json})";
        }
    }
}
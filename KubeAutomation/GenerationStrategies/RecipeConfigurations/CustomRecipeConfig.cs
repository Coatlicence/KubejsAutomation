using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace KubeAutomation.GenerationStrategies.Generators
{
    public enum JsonType
    {
        String, Int, Float, Bool, Object, Array
    }

    public class CustomRecipeConfig
    {
        private readonly JsonObject _root;

        public CustomRecipeConfig()
        {
            _root = new JsonObject();
        }

        /// <summary>
        /// 1. Создать свойство с типом
        /// </summary>
        public void CreateProperty(string path, JsonType type)
        {
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
        /// 2. Назначить значение свойству. Создаёт элемент.
        /// </summary>
        public void SetValue(string path, object value)
        {
            var (parent, name) = SplitPath(path);
            var parentNode = GetOrCreateNode(parent);

            // Если родитель — массив, name это индекс
            if (parentNode is JsonArray array)
            {
                var index = ParseIndex(name);
                EnsureArraySize(array, index + 1);
                array[index] = CreateValueNode(value);
            }
            // Если родитель — объект, name это имя свойства
            else if (parentNode is JsonObject obj)
            {
                obj[name] = CreateValueNode(value);
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
        public void Remove(string path)
        {
            var (parent, name) = SplitPath(path);
            var parentNode = GetOrCreateNode(parent);

            if (parentNode is JsonObject obj)
                obj.Remove(name);
            else if (parentNode is JsonArray arr)
                arr.Remove(arr[int.Parse(name)]);
        }

        /// <summary>
        /// 8. Получить элемент из JSON-документа
        /// </summary>
        public object GetValue(string path)
        {
            var (parent, name) = SplitPath(path);
            var parentNode = GetOrCreateNode(parent);

            if (parentNode is JsonObject obj)
                return obj[name]?.GetValue<object>();
            else if (parentNode is JsonArray arr)
                return arr[int.Parse(name)]?.GetValue<object>();

            return null;
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


    }
}
using KubeAutomation.GenerationStrategies.RecipeConfigurations.RawCode;
using KubeAutomation.Tests;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KubeAutomation.GenerationStrategies.Templates
{
    /// <summary>
    /// Тип поля в шаблоне
    /// </summary>
    public enum FieldType
    {
        Fixed,      // Фиксированное (не редактируется)
        Default,    // Значение по умолчанию (можно менять)
        Required    // Требуется заполнение (пустое в шаблоне)
    }

    /// <summary>
    /// Информация о пути в структуре
    /// </summary>
    public class PathInfo
    {
        public string Path { get; set; }
        public JsonType Type { get; set; }
        public FieldType FieldType { get; set; }
        public object? Value { get; set; }

        public override string ToString()
        {
            var val = Value?.ToString() ?? "null";
            // Если Value — JsonElement, выведем его как строку
            if (Value is System.Text.Json.JsonElement je)
                val = je.ToString();
            return $"Path='{Path}' Type={Type} Field={FieldType} Value='{val}'";
        }
    }

    /// <summary>
    /// Метаданные шаблона
    /// </summary>
    public class TemplateMetadata
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public string Category { get; set; } = "custom";
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string Version { get; set; } = "1.0";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    }

    ///------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public static class TemplateManager
    {
        // Базовая папка для шаблонов
        public static string TemplatesFolder = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Templates");

        /// <summary>
        /// Сохранить шаблон из CustomRecipeConfig
        /// </summary>
        public static bool SaveTemplate(
            string templateName,
            CustomRecipeConfig config,
            string category = "custom",
            string? description = null,
            string? author = null,
            bool overwrite = false)
        {
            // 1. Валидация имени
            if (string.IsNullOrWhiteSpace(templateName))
                return false;

            // Удалить .json если есть
            templateName = templateName.Replace(".json", "");

            // Проверка на спецсимволы
            if (templateName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                return false;

            // 2. Сформировать путь
            var categoryFolder = Path.Combine(TemplatesFolder, category);
            var filePath = Path.Combine(categoryFolder, $"{templateName}.json");

            // 3. Создать папки если не существуют
            Directory.CreateDirectory(categoryFolder);

            // 4. Проверка: файл уже существует
            if (File.Exists(filePath) && !overwrite)
                return false;

            // 5. Извлечь структуру из Config
            var paths = ExtractStructure(config);

            // 6. Создать метаданные
            var metadata = new TemplateMetadata
            {
                Name = templateName,
                Id = templateName,
                Category = category,
                Description = description,
                Author = author,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // 7. Собрать всё в один JSON
            var templateJson = CreateTemplateJson(metadata, paths);

            // 8. Сохранить в файл
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            File.WriteAllText(filePath, templateJson.ToJsonString(options));

            return true;
        }

        /// <summary>
        /// Загрузить шаблон и создать готовый CustomRecipeConfig
        /// </summary>
        public static TemplateLoadResult LoadTemplate(
            string templateName,
            string category = "custom")
        {
            // 1. Валидация имени
            if (string.IsNullOrWhiteSpace(templateName))
                return TemplateLoadResult.Fail("Имя шаблона не может быть пустым");

            templateName = templateName.Replace(".json", "");

            // 2. Сформировать путь
            var categoryFolder = Path.Combine(TemplatesFolder, category);
            var filePath = Path.Combine(categoryFolder, $"{templateName}.json");

            // 3. Проверка: файл существует
            if (!File.Exists(filePath))
                return TemplateLoadResult.Fail($"Шаблон не найден: {filePath}");

            // 4. Прочитать файл
            string jsonContent;
            try
            {
                jsonContent = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                return TemplateLoadResult.Fail($"Ошибка чтения файла: {ex.Message}");
            }

            // 5. Распарсить JSON
            JsonNode? rootNode;
            try
            {
                rootNode = JsonNode.Parse(jsonContent);
            }
            catch (Exception ex)
            {
                return TemplateLoadResult.Fail($"Ошибка парсинга JSON: {ex.Message}");
            }

            if (rootNode is not JsonObject rootObject)
                return TemplateLoadResult.Fail("Корневой элемент должен быть объектом");

            // 6. Извлечь $metadata
            TemplateMetadata? metadata;
            try
            {
                var metadataNode = rootObject["$metadata"];
                if (metadataNode == null)
                    return TemplateLoadResult.Fail("$metadata отсутствует в шаблоне");

                metadata = metadataNode.Deserialize<TemplateMetadata>();
                if (metadata == null)
                    return TemplateLoadResult.Fail("Ошибка десериализации метаданных");
            }
            catch (Exception ex)
            {
                return TemplateLoadResult.Fail($"Ошибка метаданных: {ex.Message}");
            }

            // 7. Извлечь $structure.paths
            List<PathInfo> paths;
            try
            {
                var structureNode = rootObject["$structure"];
                if (structureNode == null)
                    return TemplateLoadResult.Fail("$structure отсутствует в шаблоне");

                var pathsNode = structureNode["paths"];
                if (pathsNode == null)
                    return TemplateLoadResult.Fail("$structure.paths отсутствует");

                paths = pathsNode.Deserialize<List<PathInfo>>() ?? new List<PathInfo>();

                // test
                TestLogger.Write($"[DEBUG] Loaded paths: {paths.Count}");
                foreach (var p in paths)
                {
                    // Показываем тип Value для отладки
                    var valType = p.Value?.GetType()?.Name ?? "null";
                    TestLogger.Write($"  [PATH] Path='{p.Path}' Type={p.Type} Value='{p.Value}' (CLR: {valType})");
                }
            }
            catch (Exception ex)
            {
                return TemplateLoadResult.Fail($"Ошибка структуры: {ex.Message}");
            }

            // 8. Создать новый CustomRecipeConfig
            var config = new CustomRecipeConfig();

            // 9. Для каждого пути создать свойство и установить значение
            foreach (var pathInfo in paths)
            {
                try
                {
                    // Создать свойство с типом
                    config.CreateProperty(pathInfo.Path, pathInfo.Type);

                    // Установить значение только для Fixed и Default
                    if (pathInfo.FieldType is FieldType.Fixed or FieldType.Default)
                    {
                        // КОНВЕРТИРОВАТЬ JsonElement в правильный тип
                        var convertedValue = ConvertValue(pathInfo.Value, pathInfo.Type);

                        TestLogger.Write($"  [SET] {pathInfo.Path} = '{convertedValue}' (type: {convertedValue?.GetType()?.Name})");

                        if (convertedValue != null)
                        {
                            config.SetValue(pathInfo.Path, convertedValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestLogger.Write($"  [ERROR] Пропущен путь {pathInfo.Path}: {ex.Message}");
                }
            }

            // 10. Вернуть результат
            return TemplateLoadResult.Ok(config, metadata, paths);
        }

        /// <summary>
        /// Получить список всех доступных шаблонов
        /// </summary>
        public static List<TemplateMetadata> ListTemplates()
        {
            var templates = new List<TemplateMetadata>();

            if (!Directory.Exists(TemplatesFolder))
                return templates;

            foreach (var categoryFolder in Directory.GetDirectories(TemplatesFolder))
            {
                var category = Path.GetFileName(categoryFolder);

                foreach (var file in Directory.GetFiles(categoryFolder, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var root = JsonNode.Parse(json);
                        var metadata = root?["$metadata"]?.Deserialize<TemplateMetadata>();

                        if (metadata != null)
                            templates.Add(metadata);
                    }
                    catch
                    {
                        // Пропустить битые файлы
                    }
                }
            }

            return templates;
        }

        /// <summary>
        /// Конвертировать JsonElement в правильный тип для SetValue
        /// </summary>
        private static object? ConvertValue(object? value, JsonType expectedType)
        {
            if (value == null)
                return null;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => expectedType switch
                    {
                        JsonType.Int => jsonElement.GetInt32(),
                        JsonType.Float => jsonElement.GetDouble(),
                        _ => jsonElement.GetDouble()
                    },
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            return value;
        }

        /// <summary>
        /// Извлечь структуру из CustomRecipeConfig с автоматической классификацией
        /// </summary>
        private static List<PathInfo> ExtractStructure(CustomRecipeConfig config)
        {
            var paths = new List<PathInfo>();
            var pathList = config.GetPathList();

            foreach (var path in pathList)
            {
                var pathType = config.GetPathType(path);
                var value = config.GetValue(path);

                // Автоматическая классификация
                var fieldType = ClassifyField(path, pathType, value);
                var fieldValue = GetFieldValue(fieldType, value, pathType, config);

                paths.Add(new PathInfo
                {
                    Path = path,
                    Type = pathType,
                    FieldType = fieldType,
                    Value = fieldValue
                });
            }

            return paths;
        }

        /// <summary>
        /// Автоматическая классификация поля по правилам
        /// </summary>
        private static FieldType ClassifyField(string path, JsonType type, object? value)
        {
            // Извлечь имя поля (последний сегмент)
            var fieldName = GetFieldName(path);

            // Правило 1: type = Fixed
            if (fieldName == "type")
                return FieldType.Fixed;

            // Правило 2: loops, count, amount, chance = Default
            if (fieldName is "loops" or "count" or "amount" or "chance")
                return FieldType.Default;

            // Правило 3: active, enabled, consume = Default (bool)
            if (fieldName is "active" or "enabled" or "consume")
                return FieldType.Default;

            // Правило 4: id, item, tag, fluid = Required
            if (fieldName is "id" or "item" or "tag" or "fluid")
                return FieldType.Required;

            // Правило 5: String = Required
            if (type == JsonType.String)
                return FieldType.Required;

            // Правило 6: Int/Float = Default
            if (type is JsonType.Int or JsonType.Float)
                return FieldType.Default;

            // Правило 7: Bool = Default
            if (type == JsonType.Bool)
                return FieldType.Default;

            // Правило 8: Object = Required
            if (type == JsonType.Object)
                return FieldType.Required;

            // Правило 9: Array = Required
            if (type == JsonType.Array)
                return FieldType.Required;

            // По умолчанию
            return FieldType.Default;
        }

        /// <summary>
        /// Получить значение для поля в шаблоне
        /// </summary>
        private static object? GetFieldValue(FieldType fieldType, object? currentValue, JsonType type, CustomRecipeConfig config)
        {
            switch (fieldType)
            {
                case FieldType.Fixed:
                    // Сохранить текущее значение (например, type)
                    return currentValue;

                case FieldType.Default:
                    // Сохранить текущее или значение по умолчанию
                    if (currentValue != null)
                        return currentValue;

                    // Значения по умолчанию
                    return type switch
                    {
                        JsonType.Int => 1,
                        JsonType.Float => 1.0f,
                        JsonType.Bool => false,
                        _ => currentValue
                    };

                case FieldType.Required:
                    // Для массивов: сохранить 1 элемент с пустыми значениями
                    if (type == JsonType.Array)
                    {
                        return "[1 element]"; // Маркер для UI
                    }

                    // Для объектов: null
                    if (type == JsonType.Object)
                    {
                        return null;
                    }

                    // Для примитивов: null (требуется заполнение)
                    return null;

                default:
                    return currentValue;
            }
        }

        /// <summary>
        /// Извлечь имя поля из пути
        /// </summary>
        private static string GetFieldName(string path)
        {
            // "results[0].item" → "item"
            // "ingredient.item" → "item"
            // "type" → "type"

            var lastDot = path.LastIndexOf('.');
            var lastSegment = lastDot == -1 ? path : path.Substring(lastDot + 1);

            // Убрать индекс массива: "results[0]" → "results"
            var bracketIndex = lastSegment.IndexOf('[');
            if (bracketIndex != -1)
                return lastSegment.Substring(0, bracketIndex);

            return lastSegment;
        }

        /// <summary>
        /// Создать итоговый JSON шаблона
        /// </summary>
        private static JsonObject CreateTemplateJson(TemplateMetadata metadata, List<PathInfo> paths)
        {
            return new JsonObject
            {
                ["$metadata"] = JsonSerializer.SerializeToNode(metadata),
                ["$structure"] = new JsonObject
                {
                    ["paths"] = JsonSerializer.SerializeToNode(paths)
                }
            };
        }

        /// <summary>
        /// Сохранить РЕЦЕПТ (все данные как есть, без классификации)
        /// Для режима редактирования/ребаланса на проде
        /// </summary>
        public static bool SaveRecipe(
            string recipeName,
            CustomRecipeConfig config,
            string category = "custom",
            string? description = null,
            string? author = null,
            bool overwrite = false)
        {
            // 1. Валидация имени (как в SaveTemplate)
            if (string.IsNullOrWhiteSpace(recipeName))
                return false;

            recipeName = recipeName.Replace(".json", "");
            if (recipeName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                return false;

            // 2. Путь к файлу
            var categoryFolder = Path.Combine(TemplatesFolder, category);
            var filePath = Path.Combine(categoryFolder, $"{recipeName}.json");

            Directory.CreateDirectory(categoryFolder);
            if (File.Exists(filePath) && !overwrite)
                return false;

            // 3. КЛЮЧЕВОЕ ОТЛИЧИЕ: извлекаем ВСЕ пути со значениями, без классификации
            var paths = ExtractStructureFull(config);

            TestLogger.Write($"[DEBUG] ExtractStructureFull: {paths.Count} путей");
            foreach (var p in paths)
            {
                TestLogger.Write($"  [PATH] {p}");
            }

            // 4. Метаданные
            var metadata = new TemplateMetadata
            {
                Name = recipeName,
                Id = recipeName,
                Category = category,
                Description = description,
                Author = author,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // 5. Собираем итоговый JSON
            var recipeJson = CreateTemplateJson(metadata, paths);

            // 6. Сохраняем
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            File.WriteAllText(filePath, recipeJson.ToJsonString(options));
            return true;
        }

        /// <summary>
        /// Извлечь структуру с ВСЕМИ значениями (без классификации)
        /// Все поля помечаются как Fixed и сохраняются с текущими значениями
        /// </summary>
        private static List<PathInfo> ExtractStructureFull(CustomRecipeConfig config)
        {
            var paths = new List<PathInfo>();
            var pathList = config.GetPathList();

            foreach (var path in pathList)
            {
                var pathType = config.GetPathType(path);
                var value = config.GetValue(path);

                paths.Add(new PathInfo
                {
                    Path = path,
                    Type = pathType,
                    FieldType = FieldType.Fixed,  // Все поля как фиксированные
                    Value = value                  // Значение сохраняется как есть
                });
            }

            return paths;
        }
    }
}
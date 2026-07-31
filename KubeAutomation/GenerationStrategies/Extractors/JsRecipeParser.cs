using Esprima;
using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KubeAutomation.GenerationStrategies.Parsing
{
    /// <summary>
    /// Главный парсер JS-файлов KubeJS.
    /// Поддерживает только стандартную структуру: ServerEvents.recipes(event => { ... })
    /// </summary>
    public class JsRecipeParser
    {
        private readonly ExtractorRegistry _registry;

        public JsRecipeParser(ExtractorRegistry? registry = null)
        {
            _registry = registry ?? ExtractorRegistry.CreateDefault();
        }

        public List<BaseRecipeConfiguration> ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            var content = File.ReadAllText(filePath);
            return ParseContent(content);
        }

        public List<BaseRecipeConfiguration> ParseContent(string jsContent)
        {
            var configs = new List<BaseRecipeConfiguration>();

            try
            {
                var parserOptions = new ParserOptions
                {
                    Comments = false,
                    Tolerant = true
                };

                var parser = new JavaScriptParser(parserOptions);
                var script = parser.ParseScript(jsContent);

                // Валидация структуры файла
                ValidateKubeJsRecipeFile(script);

                // Находим ServerEvents.recipes и извлекаем лямбду
                var statement = script.Body[0];
                if (statement is ExpressionStatement exprStmt &&
                    exprStmt.Expression is CallExpression call &&
                    call.Arguments.FirstOrDefault() is ArrowFunctionExpression arrow)
                {
                    // Извлекаем рецепты из тела лямбды
                    ProcessBlock(arrow.Body, configs);
                }
            }
            catch (ParserException ex)
            {
                throw new InvalidOperationException(
                    $"Ошибка парсинга JS: {ex.Description} (строка {ex.LineNumber}, колонка {ex.Column})",
                    ex
                );
            }

            return configs;
        }

        /// <summary>
        /// Проверяет, что файл имеет поддерживаемую структуру KubeJS.
        /// Выбрасывает исключение, если структура не совпадает.
        /// </summary>
        private static void ValidateKubeJsRecipeFile(Script script)
        {
            if (script.Body.Count == 0)
                throw new InvalidOperationException("Файл пустой или не содержит утверждений.");

            if (script.Body.Count > 1)
                throw new InvalidOperationException(
                    "Файл содержит несколько утверждений. Поддерживается только один вызов ServerEvents.recipes(event => {...})."
                );

            var statement = script.Body[0];

            if (statement is not ExpressionStatement exprStmt)
                throw new InvalidOperationException(
                    "Ожидалось выражение-утверждение. Поддерживается только формат: ServerEvents.recipes(event => { ... })"
                );

            if (exprStmt.Expression is not CallExpression call)
                throw new InvalidOperationException(
                    "Ожидался вызов функции. Поддерживается только формат: ServerEvents.recipes(event => { ... })"
                );

            if (call.Callee is not MemberExpression member)
                throw new InvalidOperationException(
                    "Ожидался вызов метода. Поддерживается только формат: ServerEvents.recipes(event => { ... })"
                );

            var obj = AstHelper.ExtractStringValue(member.Object);
            var prop = AstHelper.ExtractStringValue(member.Property);

            if (obj != "ServerEvents" || prop != "recipes")
                throw new InvalidOperationException(
                    $"Ожидался вызов ServerEvents.recipes(...), получено: {obj}.{prop}(...). " +
                    "Поддерживаются только рецепты (ServerEvents.recipes)."
                );

            if (call.Arguments.Count == 0)
                throw new InvalidOperationException(
                    "ServerEvents.recipes должен содержать аргумент-лямбду: ServerEvents.recipes(event => { ... })"
                );

            if (call.Arguments[0] is not ArrowFunctionExpression)
                throw new InvalidOperationException(
                    "Аргумент ServerEvents.recipes должен быть лямбда-функцией: ServerEvents.recipes(event => { ... })"
                );
        }

        /// <summary>
        /// Обрабатывает тело функции (блок или одно выражение)
        /// </summary>
        private void ProcessBlock(Node body, List<BaseRecipeConfiguration> configs)
        {
            if (body is BlockStatement block)
            {
                foreach (var stmt in block.Body)
                {
                    ExtractFromStatement(stmt, configs);
                }
            }
            else
            {
                // Тело — одно выражение без фигурных скобок
                ExtractFromStatement(body, configs);
            }
        }

        /// <summary>
        /// Извлекает конфиги из утверждения
        /// </summary>
        private void ExtractFromStatement(Node node, List<BaseRecipeConfiguration> configs)
        {
            // Раскрываем ExpressionStatement до выражения
            var target = node is ExpressionStatement exprStmt ? exprStmt.Expression : node;

            // Спрашиваем реестр: кто может обработать этот узел?
            var extractor = _registry.GetExtractor(target);
            if (extractor != null)
            {
                var config = extractor.Extract(target);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
        }
    }
}
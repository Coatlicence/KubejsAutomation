using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Esprima.Ast;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.GenerationStrategies.Extractors
{
    /// <summary>
    /// Вспомогательные методы для работы с AST Esprima.
    /// Только действительно общие утилиты.
    /// </summary>
    public static class AstHelper
    {
        /// <summary>
        /// Извлекает строковое значение из узла (Literal, Identifier, TemplateElement)
        /// </summary>
        public static string? ExtractStringValue(Node? node)
        {
            return node switch
            {
                Literal literal => literal.Value?.ToString(),
                Identifier identifier => identifier.Name,
                TemplateElement template => template.Value?.Raw,
                _ => null
            };
        }

        /// <summary>
        /// Конвертирует ObjectExpression AST в JsonObject
        /// </summary>
        public static JsonObject? ExtractObjectExpression(ObjectExpression? objExpr)
        {
            if (objExpr == null)
                return null;

            var result = new JsonObject();

            foreach (var prop in objExpr.Properties)
            {
                if (prop is Property property)
                {
                    var key = ExtractStringValue(property.Key);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    var value = ExtractJsonValue(property.Value);
                    if (value != null)
                    {
                        result[key!] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Рекурсивно извлекает значение из узла в формат JsonNode
        /// </summary>
        private static JsonNode? ExtractJsonValue(Node? node)
        {
            return node switch
            {
                Literal literal => ExtractLiteralValue(literal),
                ObjectExpression obj => ExtractObjectExpression(obj),
                ArrayExpression arr => ExtractArrayExpression(arr),
                _ => null
            };
        }

        /// <summary>
        /// Извлекает примитивное значение из Literal
        /// </summary>
        private static JsonNode? ExtractLiteralValue(Literal literal)
        {
            if (literal.Value == null)
                return null;

            return literal.Value switch
            {
                string s => s,
                int i => i,
                long l => l,
                double d => d,
                bool b => b,
                null => null,
                _ => literal.Value.ToString()
            };
        }

        /// <summary>
        /// Извлекает массив в JsonArray
        /// </summary>
        private static JsonArray? ExtractArrayExpression(ArrayExpression? arrExpr)
        {
            if (arrExpr == null)
                return null;

            var result = new JsonArray();
            foreach (var element in arrExpr.Elements)
            {
                var value = ExtractJsonValue(element);
                result.Add(value ?? JsonValue.Create(""));
            }
            return result;
        }

        /// <summary>
        /// Конвертирует ObjectExpression в RecipeFilter (для event.remove)
        /// </summary>
        public static RecipeFilter? ExtractRecipeFilter(ObjectExpression? objExpr)
        {
            if (objExpr == null)
                return null;

            var filter = new RecipeFilter();
            var jsonObject = ExtractObjectExpression(objExpr);

            if (jsonObject == null)
                return filter;

            if (jsonObject.TryGetPropertyValue("output", out var output) && output is JsonValue outputVal)
                filter.Output = outputVal.ToString();

            if (jsonObject.TryGetPropertyValue("input", out var input) && input is JsonValue inputVal)
                filter.Input = inputVal.ToString();

            if (jsonObject.TryGetPropertyValue("type", out var type) && type is JsonValue typeVal)
                filter.Type = typeVal.ToString();

            if (jsonObject.TryGetPropertyValue("mod", out var mod) && mod is JsonValue modVal)
                filter.Mod = modVal.ToString();

            if (jsonObject.TryGetPropertyValue("id", out var id) && id is JsonValue idVal)
                filter.Id = idVal.ToString();

            if (jsonObject.TryGetPropertyValue("not", out var notNode) && notNode is JsonObject notObj)
            {
                filter.Not = ExtractRecipeFilterFromJson(notObj);
            }

            return filter;
        }

        /// <summary>
        /// Вспомогательный метод для рекурсивного извлечения "not"
        /// </summary>
        private static RecipeFilter ExtractRecipeFilterFromJson(JsonObject json)
        {
            var filter = new RecipeFilter();

            if (json.TryGetPropertyValue("output", out var output) && output is JsonValue outputVal)
                filter.Output = outputVal.ToString();
            if (json.TryGetPropertyValue("input", out var input) && input is JsonValue inputVal)
                filter.Input = inputVal.ToString();
            if (json.TryGetPropertyValue("type", out var type) && type is JsonValue typeVal)
                filter.Type = typeVal.ToString();
            if (json.TryGetPropertyValue("mod", out var mod) && mod is JsonValue modVal)
                filter.Mod = modVal.ToString();
            if (json.TryGetPropertyValue("id", out var id) && id is JsonValue idVal)
                filter.Id = idVal.ToString();

            if (json.TryGetPropertyValue("not", out var notNode) && notNode is JsonObject notObj)
            {
                filter.Not = ExtractRecipeFilterFromJson(notObj);
            }

            return filter;
        }
    }
}
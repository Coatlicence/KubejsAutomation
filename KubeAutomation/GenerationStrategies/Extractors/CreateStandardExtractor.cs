using Esprima.Ast;
using KubeAutomation.GenerationStrategies.Extractors;
using KubeAutomation.GenerationStrategies.Parsing;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.Create;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KubeAutomation.GenerationStrategies.Parsing.Create
{
    /// <summary>
    /// Универсальный экстрактор для всех Create-рецептов
    /// </summary>
    public class CreateExtractor : IRecipeExtractor
    {
        public bool CanHandle(Node? node)
        {
            if (node is not CallExpression call)
                return false;

            return IsCreateRecipeCall(call);
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            if (node is not CallExpression call)
                return null;

            var methodName = ExtractMethodName(call);
            if (string.IsNullOrEmpty(methodName))
                return null;

            // Создаём конфиг по имени метода
            var config = CreateConfigByMethodName(methodName);
            if (config == null)
                return null;

            // Извлекаем аргументы
            if (call.Arguments.Count < 2)
                return null; // Недостаточно аргументов для большинства рецептов

            // Первый аргумент: выходы
            var outputsArg = call.Arguments[0];
            config.Outputs = ExtractItems(outputsArg);

            // Второй аргумент: входы
            var inputsArg = call.Arguments[1];
            config.Inputs = ExtractIngredients(inputsArg);

            // Извлекаем модификаторы (пока не реализовано)
            // ExtractModifiers(call, config);

            return config;
        }

        /// <summary>
        /// Проверяет, является ли вызов event.recipes.create.*
        /// </summary>
        /// <summary>
        /// Проверяет, является ли вызов event.recipes.create.*
        /// </summary>
        private static bool IsCreateRecipeCall(CallExpression call)
        {
            // event.recipes.create.method (4 уровня вложенности)
            if (call.Callee is MemberExpression methodMember &&
                methodMember.Property is Identifier methodId &&
                methodMember.Object is MemberExpression createMember &&
                createMember.Property is Identifier createId &&
                createMember.Object is MemberExpression recipesMember &&
                recipesMember.Property is Identifier recipesId &&
                recipesMember.Object is Identifier eventId)
            {
                var obj = AstHelper.ExtractStringValue(eventId);
                var prop = AstHelper.ExtractStringValue(recipesId);
                var create = AstHelper.ExtractStringValue(createId);

                return obj == "event" && prop == "recipes" && create == "create";
            }
            return false;
        }

        /// <summary>
        /// Извлекает имя метода из вызова
        /// </summary>
        /// <summary>
        /// Извлекает имя метода из вызова (в нижнем регистре)
        /// </summary>
        private static string ExtractMethodName(CallExpression call)
        {
            if (call.Callee is MemberExpression member && member.Property is Identifier methodId)
            {
                return AstHelper.ExtractStringValue(methodId)?.ToLowerInvariant() ?? "";
            }
            return "";
        }

        /// <summary>
        /// Создаёт конфиг по имени метода
        /// </summary>
        private static BaseCreateRecipeConfig? CreateConfigByMethodName(string methodName)
        {
            return methodName.ToLowerInvariant() switch
            {
                "compacting" => new CreateCompactingConfig(),
                "crushing" => new CreateCrushingConfig(),
                "cutting" => new CreateCuttingConfig(),
                "deploying" => new CreateDeployingConfig(),
                "emptying" => new CreateEmptyingConfig(),
                "filling" => new CreateFillingConfig(),
                "haunting" => new CreateHauntingConfig(),
                "milling" => new CreateMillingConfig(),
                "mixing" => new CreateMixingConfig(),
                "pressing" => new CreatePressingConfig(),
                "sandpaper_polishing" => new CreateSandpaperPolishingConfig(),
                "splashing" => new CreateSplashingConfig(),
                _ => null // Неизвестный тип
            };
        }

        /// <summary>
        /// Извлекает массив предметов (выходы) из узла
        /// </summary>
        private static List<CreateItemComponent> ExtractItems(Node? node)
        {
            var items = new List<CreateItemComponent>();

            if (node is ArrayExpression arr)
            {
                foreach (var element in arr.Elements)
                {
                    var item = ExtractSingleItem(element);
                    if (item != null)
                        items.Add(item);
                }
            }
            else
            {
                var item = ExtractSingleItem(node);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Извлекает один предмет из узла (строка или CreateItem.of)
        /// </summary>
        private static CreateItemComponent? ExtractSingleItem(Node? node)
        {
            if (node is Literal literal && literal.Value is string str)
            {
                return CreateItemComponent.Parse(str);
            }
            else if (node is CallExpression call)
            {
                // Проверяем: CreateItem.of('item', chance)
                if (call.Callee is MemberExpression member &&
                    member.Object is Identifier objId &&
                    AstHelper.ExtractStringValue(objId) == "CreateItem" &&
                    AstHelper.ExtractStringValue(member.Property) == "of")
                {
                    if (call.Arguments.Count >= 2 &&
                        call.Arguments[0] is Literal itemLit && itemLit.Value is string itemStr &&
                        call.Arguments[1] is Literal chanceLit && chanceLit.Value is double chance)
                    {
                        return CreateItemComponent.Parse(itemStr, (float)chance);
                    }
                    else if (call.Arguments.Count >= 1 &&
                             call.Arguments[0] is Literal singleItemLit && singleItemLit.Value is string singleItemStr)
                    {
                        return CreateItemComponent.Parse(singleItemStr);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Извлекает массив ингредиентов (входы) из узла
        /// </summary>
        private static List<CreateIngredientComponent> ExtractIngredients(Node? node)
        {
            var ingredients = new List<CreateIngredientComponent>();

            if (node is ArrayExpression arr)
            {
                foreach (var element in arr.Elements)
                {
                    var ingredient = ExtractSingleIngredient(element);
                    if (ingredient != null)
                        ingredients.Add(ingredient);
                }
            }
            else
            {
                var ingredient = ExtractSingleIngredient(node);
                if (ingredient != null)
                    ingredients.Add(ingredient);
            }

            return ingredients;
        }

        /// <summary>
        /// Извлекает один ингредиент из узла
        /// </summary>
        private static CreateIngredientComponent? ExtractSingleIngredient(Node? node)
        {
            if (node is Literal literal && literal.Value is string str)
            {
                return CreateIngredientComponent.Parse(str);
            }
            else if (node is CallExpression call)
            {
                // Проверяем: Fluid.of, Ingredient.of
                if (call.Callee is MemberExpression member &&
                    member.Object is Identifier objId)
                {
                    var objName = AstHelper.ExtractStringValue(objId);
                    var propName = AstHelper.ExtractStringValue(member.Property);

                    if (objName == "Fluid" && propName == "of")
                    {
                        if (call.Arguments.Count >= 2 &&
                            call.Arguments[0] is Literal fluidLit && fluidLit.Value is string fluidId &&
                            call.Arguments[1] is Literal amountLit && amountLit.Value is double amount)
                        {
                            return new CreateFluidIngredientComponent
                            {
                                FluidId = fluidId,
                                Amount = (int)amount
                            };
                        }
                    }
                    else if (objName == "Ingredient" && propName == "of")
                    {
                        if (call.Arguments.Count >= 1 &&
                            call.Arguments[0] is Literal itemLit && itemLit.Value is string itemId)
                        {
                            return new CreateItemIngredientComponent { ItemId = itemId };
                        }
                    }
                }
            }

            return null;
        }
    }
}
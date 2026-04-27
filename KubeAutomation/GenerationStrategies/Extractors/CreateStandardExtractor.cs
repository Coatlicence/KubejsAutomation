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
            var baseCall = GetBaseCallExpression(node);
            if (baseCall is not CallExpression call)
                return false;

            return IsCreateRecipeCall(call);
        }

        public BaseRecipeConfiguration? Extract(Node? node)
        {
            var baseCall = GetBaseCallExpression(node);
            if (baseCall is not CallExpression call)
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

            // Извлекаем модификаторы из цепочки вызовов
            ExtractModifiers(node!, config); // node гарантированно не null, т.к. CanHandle прошёл

            return config;
        }

        /// <summary>
        /// Извлекает базовый вызов из цепочки: mixing(...).heated() → mixing(...)
        /// </summary>
        /// <summary>
        /// Извлекает базовый вызов из цепочки: mixing(...).heated() → mixing(...)
        /// </summary>
        private static CallExpression? GetBaseCallExpression(Node? node)
        {
            var current = node;

            while (current is CallExpression call &&
                   call.Callee is MemberExpression member)
            {
                // Если объект вызова — это тоже вызов, значит, цепочка продолжается
                if (member.Object is CallExpression nextCall)
                {
                    current = nextCall;
                    continue;
                }

                // Если объект — MemberExpression, проверяем, является ли это event.recipes.create.*
                if (member.Object is MemberExpression baseMember)
                {
                    // Проверим, что это действительно event.recipes.create.method
                    if (IsCreateRecipeCall(baseMember))
                    {
                        // Текущий call — это и есть базовый вызов (например, mixing(...))
                        return call;
                    }
                }

                break;
            }

            return current as CallExpression;
        }
        /// <summary>
        /// Извлекает модификаторы из цепочки вызовов: .heated().processingTime(500)
        /// </summary>
        private static void ExtractModifiers(Node node, BaseCreateRecipeConfig config)
        {
            // node = CallExpression: processingTime(500)
            var currentCallee = (node as CallExpression)?.Callee;

            while (currentCallee is MemberExpression member)
            {
                var methodName = AstHelper.ExtractStringValue(member.Property);
                if (string.IsNullOrEmpty(methodName))
                    break;

                switch (methodName.ToLowerInvariant())
                {
                    case "heated":
                        config.Modifiers.Heat = CreateHeatType.Heated;
                        break;
                    case "superheated":
                        config.Modifiers.Heat = CreateHeatType.Superheated;
                        break;
                    case "processingtime":
                        // Извлекаем аргумент: .processingTime(500)
                        if (TryExtractIntArgument(member, out var time))
                        {
                            config.Modifiers.ProcessingTime = time;
                        }
                        break;
                    case "keephelditem":
                        config.Modifiers.KeepHeldItem = true;
                        break;
                    // ... другие модификаторы ...
                    default:
                        // Неизвестный модификатор — пропускаем
                        break;
                }

                // Переходим к следующему уровню вложенности
                currentCallee = member.Object;
            }
        }

        /// <summary>
        /// Извлекает первый аргумент как int (для processingTime, loops и т.д.)
        /// </summary>
        private static bool TryExtractIntArgument(MemberExpression member, out int value)
        {
            value = 0;

            // member.Object должен быть CallExpression (в котором вызов предыдущего метода)
            if (member.Object is not CallExpression call)
                return false;

            if (call.Arguments.Count == 0)
                return false;

            var arg = call.Arguments[0];
            if (arg is Literal literal && literal.Value is double d)
            {
                value = (int)d;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, является ли вызов event.recipes.create.*
        /// </summary>
        private static bool IsCreateRecipeCall(MemberExpression member)
        {
            // event.recipes.create.method (4 уровня вложенности)
            if (member.Property is Identifier methodId &&
                member.Object is MemberExpression createMember &&
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
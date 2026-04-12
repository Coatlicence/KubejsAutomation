using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.Tests.Cases
{
    /// <summary>
    /// Тесты для стратегии удаления рецептов (event.remove)
    /// </summary>
    public class RecipeRemovalTests : TestBase
    {
        public override List<TestResult> RunAll()
        {
            var results = new List<TestResult>
            {
                // Группа 1: Базовая генерация
                TestSimpleFilter_Output(),
                TestSimpleFilter_Input(),
                TestSimpleFilter_Type(),
                TestSimpleFilter_Mod(),
                TestSimpleFilter_Id(),

                // Группа 2: Логика AND
                TestAndLogic_OutputAndType(),
                TestAndLogic_ModAndOutput(),

                // Группа 3: Логика OR
                TestOrLogic_TwoOutputs(),
                TestOrLogic_ThreeFilters(),

                // Группа 4: Логика NOT
                TestNotLogic_Simple(),
                TestNotLogic_Nested(),

                // Группа 5: Комбинированная логика
                TestComplexLogic_AndNot(),
                TestComplexLogic_OrWithNot(),

                // Группа 6: Фабричные методы
                TestFactory_ByMod(),
                TestFactory_ByOutput(),
                TestFactory_ByType(),
                TestFactory_ById(),

                // Группа 7: Валидация
                TestValidation_EmptyFilter_Throws(),
                TestValidation_NullFilter_Throws(),
                TestValidation_ValidFilter_Passes(),

                // Группа 8: Edge cases
                TestEdgeCase_TagAsOutput(),
                TestEdgeCase_CustomConditions(),
                TestEdgeCase_NullProperties_Ignored()
            };

            return results;
        }

        /// <summary>
        /// Хелпер: проверяет наличие ключа и значения в JSON (игнорирует пробелы)
        /// </summary>
        private bool JsonHasKeyValue(string json, string key, string value)
        {
            // Compact: "key":"value"
            if (json.Contains($"\"{key}\":\"{value}\""))
                return true;
            // Formatted: "key": "value"
            if (json.Contains($"\"{key}\": \"{value}\""))
                return true;
            return false;
        }

        // ============================================================
        // ГРУППА 1: Базовая генерация одного фильтра
        // ============================================================

        private TestResult TestSimpleFilter_Output()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Simple filter: output only");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter { Output = "minecraft:stone_pickaxe" } }
                };

                string js = config.GenerateJsCode();
                // ✅ Компактный JSON (без пробелов после : и ,)
                string expected = "    event.remove({\"output\":\"minecraft:stone_pickaxe\"})";

                TestLogger.Write($"[DATA] Generated: {js}");
                TestLogger.Write($"[DATA] Expected:  {expected}");

                if (js != expected)
                    throw new Exception($"JS mismatch.\nExpected: {expected}\nGot: {js}");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Simple filter: output only", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Simple filter: output only", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestSimpleFilter_Input()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Simple filter: input only");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter { Input = "#forge:dusts/redstone" } }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "input", "#forge:dusts/redstone"))
                    throw new Exception($"JS should contain input filter with tag");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Simple filter: input only", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Simple filter: input only", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestSimpleFilter_Type()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Simple filter: type only");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter { Type = "minecraft:campfire_cooking" } }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "type", "minecraft:campfire_cooking"))
                    throw new Exception($"JS should contain type filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Simple filter: type only", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Simple filter: type only", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestSimpleFilter_Mod()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Simple filter: mod only");

                var config = RecipeRemovalConfig.ByMod("farmersdelight");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "mod", "farmersdelight"))
                    throw new Exception($"JS should contain mod filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Simple filter: mod only", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Simple filter: mod only", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestSimpleFilter_Id()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Simple filter: id only");

                var config = RecipeRemovalConfig.ById("minecraft:glowstone");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "id", "minecraft:glowstone"))
                    throw new Exception($"JS should contain id filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Simple filter: id only", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Simple filter: id only", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 2: Логика AND (несколько полей в одном фильтре)
        // ============================================================

        private TestResult TestAndLogic_OutputAndType()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: AND logic: output + type in same filter");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter
                        {
                            Output = "minecraft:stone",
                            Type = "minecraft:smelting"
                        }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool hasOutput = JsonHasKeyValue(js, "output", "minecraft:stone");
                bool hasType = JsonHasKeyValue(js, "type", "minecraft:smelting");
                bool isArray = js.Trim().StartsWith("event.remove([");

                if (!hasOutput || !hasType)
                    throw new Exception($"JS should contain both output and type");

                if (isArray)
                    throw new Exception($"Single filter should NOT generate array (no OR logic)");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("AND logic: output + type in same filter", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("AND logic: output + type in same filter", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestAndLogic_ModAndOutput()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: AND logic: mod + output in same filter");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter
                        {
                            Mod = "create",
                            Output = "#create:seats"
                        }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool hasMod = JsonHasKeyValue(js, "mod", "create");
                bool hasOutput = JsonHasKeyValue(js, "output", "#create:seats");

                if (!hasMod || !hasOutput)
                    throw new Exception($"JS should contain both mod and output (AND logic)");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("AND logic: mod + output in same filter", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("AND logic: mod + output in same filter", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 3: Логика OR (несколько фильтров в списке)
        // ============================================================

        private TestResult TestOrLogic_TwoOutputs()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: OR logic: two filters = array output");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter { Output = "minecraft:stick" },
                        new RecipeFilter { Output = "minecraft:gravel" }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool isArray = js.Contains("event.remove([");
                bool hasStick = JsonHasKeyValue(js, "output", "minecraft:stick");
                bool hasGravel = JsonHasKeyValue(js, "output", "minecraft:gravel");

                if (!isArray)
                    throw new Exception($"Multiple filters should generate array (OR logic)");

                if (!hasStick || !hasGravel)
                    throw new Exception($"JS should contain both outputs");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("OR logic: two filters = array output", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("OR logic: two filters = array output", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestOrLogic_ThreeFilters()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: OR logic: three filters");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter { Mod = "mod1" },
                        new RecipeFilter { Mod = "mod2" },
                        new RecipeFilter { Mod = "mod3" }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                int openBrackets = js.Count(c => c == '[');
                int closeBrackets = js.Count(c => c == ']');

                if (openBrackets != 1)
                    throw new Exception($"Should have one opening bracket for array, got {openBrackets}");

                if (closeBrackets != 1)
                    throw new Exception($"Should have one closing bracket for array, got {closeBrackets}");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("OR logic: three filters", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("OR logic: three filters", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 4: Логика NOT (рекурсивный фильтр)
        // ============================================================

        private TestResult TestNotLogic_Simple()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: NOT logic: simple nested filter");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter
                        {
                            Output = "minecraft:stone",
                            Not = new RecipeFilter { Type = "minecraft:smelting" }
                        }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool hasNot = js.Contains("\"not\":");
                bool hasNestedType = JsonHasKeyValue(js, "type", "minecraft:smelting");

                if (!hasNot)
                    throw new Exception($"JS should contain 'not' key");

                if (!hasNestedType)
                    throw new Exception($"JS should contain nested type inside not");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("NOT logic: simple nested filter", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("NOT logic: simple nested filter", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestNotLogic_Nested()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: NOT logic: deeply nested (not inside not)");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter
                        {
                            Not = new RecipeFilter
                            {
                                Not = new RecipeFilter { Type = "minecraft:crafting_shaped" }
                            }
                        }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                int notCount = System.Text.RegularExpressions.Regex.Matches(js, "\"not\":").Count;

                if (notCount != 2)
                    throw new Exception($"Should have two 'not' keys for nested NOT, got {notCount}");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("NOT logic: deeply nested (not inside not)", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("NOT logic: deeply nested (not inside not)", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 5: Комбинированная логика
        // ============================================================

        private TestResult TestComplexLogic_AndNot()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Complex: AND + NOT in single filter");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter
                        {
                            Output = "minecraft:cooked_chicken",
                            Type = "minecraft:campfire_cooking",
                            Not = new RecipeFilter { Mod = "extra_mods" }
                        }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool hasOutput = JsonHasKeyValue(js, "output", "minecraft:cooked_chicken");
                bool hasType = JsonHasKeyValue(js, "type", "minecraft:campfire_cooking");
                bool hasNot = js.Contains("\"not\":");
                bool hasNotMod = JsonHasKeyValue(js, "mod", "extra_mods");

                if (!hasOutput || !hasType || !hasNot || !hasNotMod)
                    throw new Exception($"JS should contain output, type, and nested not+mod");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Complex: AND + NOT in single filter", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Complex: AND + NOT in single filter", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestComplexLogic_OrWithNot()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Complex: OR of filters with NOT");

                var config = new RecipeRemovalConfig
                {
                    Filters = new()
                    {
                        new RecipeFilter { Output = "stone", Not = new RecipeFilter { Type = "smelting" } },
                        new RecipeFilter { Output = "iron", Not = new RecipeFilter { Type = "blasting" } }
                    }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool isArray = js.Contains("event.remove([");
                int notCount = System.Text.RegularExpressions.Regex.Matches(js, "\"not\":").Count;

                if (!isArray)
                    throw new Exception($"Multiple filters should generate array");

                if (notCount != 2)
                    throw new Exception($"Should have two 'not' keys (one per filter), got {notCount}");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Complex: OR of filters with NOT", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Complex: OR of filters with NOT", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 6: Фабричные методы
        // ============================================================

        private TestResult TestFactory_ByMod()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Factory: ByMod");

                var config = RecipeRemovalConfig.ByMod("thermal");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!js.Contains("event.remove"))
                    throw new Exception($"Should generate event.remove call");

                if (!JsonHasKeyValue(js, "mod", "thermal"))
                    throw new Exception($"Should contain mod filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Factory: ByMod", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Factory: ByMod", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestFactory_ByOutput()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Factory: ByOutput");

                var config = RecipeRemovalConfig.ByOutput("minecraft:diamond");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "output", "minecraft:diamond"))
                    throw new Exception($"Should contain output filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Factory: ByOutput", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Factory: ByOutput", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestFactory_ByType()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Factory: ByType");

                var config = RecipeRemovalConfig.ByType("minecraft:stonecutting");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "type", "minecraft:stonecutting"))
                    throw new Exception($"Should contain type filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Factory: ByType", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Factory: ByType", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestFactory_ById()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Factory: ById");

                var config = RecipeRemovalConfig.ById("kubejs:custom_recipe");
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                if (!JsonHasKeyValue(js, "id", "kubejs:custom_recipe"))
                    throw new Exception($"Should contain id filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Factory: ById", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Factory: ById", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 7: Валидация
        // ============================================================

        private TestResult TestValidation_EmptyFilter_Throws()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Validation: empty filter throws");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter() } // Пустой фильтр
                };

                bool threw = false;
                string errorMsg = "";
                try
                {
                    config.Validate();
                }
                catch (RecipeValidationException ex)
                {
                    threw = true;
                    errorMsg = ex.Message;
                    TestLogger.Write($"[DEBUG] Caught expected exception: {errorMsg}");
                }

                if (!threw)
                    throw new Exception($"Validate() should throw RecipeValidationException for empty filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Validation: empty filter throws", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Validation: empty filter throws", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestValidation_NullFilter_Throws()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Validation: null filter in list throws");

                var config = new RecipeRemovalConfig
                {
                    Filters = new List<RecipeFilter> { null! }
                };

                bool threw = false;
                try
                {
                    config.Validate();
                }
                catch (RecipeValidationException)
                {
                    threw = true;
                }

                if (!threw)
                    throw new Exception($"Validate() should throw RecipeValidationException for null filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Validation: null filter in list throws", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Validation: null filter in list throws", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestValidation_ValidFilter_Passes()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Validation: valid filter passes");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter { Output = "test" } }
                };

                bool threw = false;
                try
                {
                    config.Validate();
                }
                catch (Exception)
                {
                    threw = true;
                }

                if (threw)
                    throw new Exception($"Validate() should NOT throw for valid filter");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Validation: valid filter passes", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Validation: valid filter passes", ex.Message, stopwatch.Elapsed);
            }
        }

        // ============================================================
        // ГРУППА 8: Edge cases
        // ============================================================

        private TestResult TestEdgeCase_TagAsOutput()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Edge case: tag (with #) as output");

                var config = new RecipeRemovalConfig
                {
                    Filters = new() { new RecipeFilter { Output = "#minecraft:wool" } }
                };

                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                // Теги должны сохраняться с #
                bool containsTag = js.Contains("\"output\":\"#minecraft:wool\"") ||
                                   js.Contains("\"output\": \"#minecraft:wool\"");

                if (!containsTag)
                    throw new Exception($"Should preserve # in tag value");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Edge case: tag (with #) as output", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Edge case: tag (with #) as output", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestEdgeCase_CustomConditions()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Edge case: custom conditions dictionary");

                var filter = new RecipeFilter
                {
                    Output = "test",
                    CustomConditions = new Dictionary<string, object?>
                    {
                        ["custom_field"] = "custom_value",
                        ["number_field"] = 42
                    }
                };

                var config = new RecipeRemovalConfig { Filters = new() { filter } };
                string js = config.GenerateJsCode();
                TestLogger.Write($"[DATA] Generated: {js}");

                bool hasCustom = JsonHasKeyValue(js, "custom_field", "custom_value");
                bool hasNumber = js.Contains("\"number_field\":42") || js.Contains("\"number_field\": 42");

                if (!hasCustom || !hasNumber)
                    throw new Exception($"Should include custom conditions in output");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Edge case: custom conditions dictionary", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Edge case: custom conditions dictionary", ex.Message, stopwatch.Elapsed);
            }
        }

        private TestResult TestEdgeCase_NullProperties_Ignored()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: Edge case: null/empty properties are not included");

                var filter = new RecipeFilter
                {
                    Output = "test",
                    Input = null,
                    Type = "",
                    Mod = null
                };

                var json = filter.ToJsonObject();
                TestLogger.Write($"[DATA] Generated JsonObject keys: {string.Join(", ", json.AsObject().Select(k => k.Key))}");

                bool hasOutput = json.ContainsKey("output");
                bool hasInput = json.ContainsKey("input");
                bool hasType = json.ContainsKey("type");
                bool hasMod = json.ContainsKey("mod");

                if (!hasOutput)
                    throw new Exception($"Should include non-empty output");

                if (hasInput)
                    throw new Exception($"Should NOT include null input");

                if (hasType)
                    throw new Exception($"Should NOT include empty type");

                if (hasMod)
                    throw new Exception($"Should NOT include null mod");

                stopwatch.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("Edge case: null/empty properties are not included", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("Edge case: null/empty properties are not included", ex.Message, stopwatch.Elapsed);
            }
        }
    }
}
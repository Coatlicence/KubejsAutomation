using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.Create;
using KubeAutomation.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KubeAutomation.Tests.Cases
{
    public class CreateRecipeTests : TestBase
    {
        public override List<TestResult> RunAll()
        {
            var results = new List<TestResult>
            {
                // Тесты валидации
                TestCompactingValidation(),
                TestDeployingValidation(),
                TestEmptyingValidation(),
                TestFillingValidation(),
                TestCrushingValidation(),
                TestCuttingValidation(),
                TestMillingValidation(),
                TestMixingValidation(),
                TestPressingValidation(),
                TestHauntingValidation(),
                TestSplashingValidation(),
                TestSandpaperPolishingValidation(),

                // Тесты генерации
                TestCompactingGeneration(),
                TestDeployingGeneration(),
                TestEmptyingGeneration(),
                TestFillingGeneration(),
                TestCrushingGeneration(),
                TestCuttingGeneration(),
                TestMillingGeneration(),
                TestMixingGeneration(),
                TestPressingGeneration(),
                TestHauntingGeneration(),
                TestSplashingGeneration(),
                TestSandpaperPolishingGeneration(),

                // Тесты модификаторов
                TestCompactingModifiers(),
                TestDeployingModifiers(),
                TestCrushingModifiers(),
                TestCuttingModifiers(),

                // Тесты CreateItem.of
                TestCreateItemParsing(),
                TestCreateItemWithChance(),
                TestCreateItemWithCount(),

                // Тесты ингредиентов
                TestFluidIngredient(),
                TestTagIngredient(),
                TestIngredientOf()
            };

            return results;
        }

        #region Validation Tests

        private TestResult TestCompactingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCompactingConfig - Валидация");

                var config = new CreateCompactingConfig();

                // Правильный рецепт: 1+ входов, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.MinInputs != 1 || constraints.MaxInputs != null ||
                    constraints.MinOutputs != 1 || constraints.MaxOutputs != null ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.Heated) ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.Superheated))
                {
                    throw new Exception("Неверные ограничения для compacting");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCompactingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCompactingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestDeployingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateDeployingConfig - Валидация");

                var config = new CreateDeployingConfig();

                // Правильный рецепт: ровно 2 входа
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:sand" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 2 ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.KeepHeldItem))
                {
                    throw new Exception("Неверные ограничения для deploying");
                }

                // Проверим ошибку при неправильном числе входов
                var wrongConfig = new CreateDeployingConfig();
                wrongConfig.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:item" });
                wrongConfig.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:out" });

                try
                {
                    wrongConfig.Validate();
                    throw new Exception("Должна быть ошибка валидации для 1 входа в deploying");
                }
                catch (RecipeValidationException)
                {
                    // OK
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateDeployingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateDeployingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestEmptyingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateEmptyingConfig - Валидация");

                var config = new CreateEmptyingConfig();

                // Правильный рецепт: 1 вход, 2 выхода
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:water_bucket" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:bucket", Count = 1 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water", Count = 1 }); // условно

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 || constraints.FixedOutputCount != 2)
                {
                    throw new Exception("Неверные ограничения для emptying");
                }

                // Проверим ошибку при неправильном числе входов/выходов
                var wrongConfig = new CreateEmptyingConfig();
                wrongConfig.Inputs.Add(new CreateItemIngredientComponent { ItemId = "item1" });
                wrongConfig.Inputs.Add(new CreateItemIngredientComponent { ItemId = "item2" });
                wrongConfig.Outputs.Add(new CreateItemComponent { ItemId = "out" });

                try
                {
                    wrongConfig.Validate();
                    throw new Exception("Должна быть ошибка валидации для 2 входов в emptying");
                }
                catch (RecipeValidationException)
                {
                    // OK
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateEmptyingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateEmptyingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestFillingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateFillingConfig - Валидация");

                var config = new CreateFillingConfig();

                // Правильный рецепт: 2 входа, 1 выход
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:bucket" });
                config.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water_bucket", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 2 || constraints.FixedOutputCount != 1)
                {
                    throw new Exception("Неверные ограничения для filling");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateFillingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateFillingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCrushingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCrushingConfig - Валидация");

                var config = new CreateCrushingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:emerald", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.ProcessingTime))
                {
                    throw new Exception("Неверные ограничения для crushing");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCrushingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCrushingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCuttingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCuttingConfig - Валидация");

                var config = new CreateCuttingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.ProcessingTime))
                {
                    throw new Exception("Неверные ограничения для cutting");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCuttingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCuttingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestMillingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateMillingConfig - Валидация");

                var config = new CreateMillingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    constraints.SupportedModifiers.Count > 0) // нет модификаторов
                {
                    throw new Exception("Неверные ограничения для milling");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateMillingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateMillingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestMixingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateMixingConfig - Валидация");

                var config = new CreateMixingConfig();

                // Правильный рецепт: 1+ входов, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal" });
                config.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.MinInputs != 1 || constraints.MinOutputs != 1 ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.Heated) ||
                    !constraints.SupportedModifiers.Contains(CreateModifierType.Superheated) ||
                    !constraints.AllowFluidInputs || !constraints.AllowFluidOutputs)
                {
                    throw new Exception("Неверные ограничения для mixing");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateMixingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateMixingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestPressingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreatePressingConfig - Валидация");

                var config = new CreatePressingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    constraints.SupportedModifiers.Count > 0) // нет модификаторов
                {
                    throw new Exception("Неверные ограничения для pressing");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreatePressingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreatePressingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestHauntingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateHauntingConfig - Валидация");

                var config = new CreateHauntingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:campfire" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:soul_campfire", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    constraints.SupportedModifiers.Count > 0) // нет модификаторов
                {
                    throw new Exception("Неверные ограничения для haunting");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateHauntingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateHauntingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestSplashingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateSplashingConfig - Валидация");

                var config = new CreateSplashingConfig();

                // Правильный рецепт: 1 вход, 1+ выходов
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:campfire" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:soul_campfire", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 ||
                    constraints.SupportedModifiers.Count > 0) // нет модификаторов
                {
                    throw new Exception("Неверные ограничения для splashing");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateSplashingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateSplashingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestSandpaperPolishingValidation()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateSandpaperPolishingConfig - Валидация");

                var config = new CreateSandpaperPolishingConfig();

                // Правильный рецепт: 1 вход, 1 выход
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                config.Validate(); // Должно пройти

                // Проверим ограничения
                var constraints = config.GetConstraints();
                if (constraints.FixedInputCount != 1 || constraints.FixedOutputCount != 1 ||
                    constraints.SupportedModifiers.Count > 0) // нет модификаторов
                {
                    throw new Exception("Неверные ограничения для sandpaper_polishing");
                }

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateSandpaperPolishingConfig - Валидация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateSandpaperPolishingConfig - Валидация", ex.Message, sw.Elapsed);
            }
        }

        #endregion

        #region Generation Tests

        private TestResult TestCompactingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCompactingConfig - Генерация");

                var config = new CreateCompactingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.compacting"))
                    throw new Exception("Код не содержит вызова compacting");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCompactingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCompactingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestDeployingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateDeployingConfig - Генерация");

                var config = new CreateDeployingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:sand" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.deploying"))
                    throw new Exception("Код не содержит вызова deploying");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateDeployingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateDeployingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestEmptyingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateEmptyingConfig - Генерация");

                var config = new CreateEmptyingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:water_bucket" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:bucket", Count = 1 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water", Count = 1000 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.emptying"))
                    throw new Exception("Код не содержит вызова emptying");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateEmptyingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateEmptyingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestFillingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateFillingConfig - Генерация");

                var config = new CreateFillingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:bucket" });
                config.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water_bucket", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.filling"))
                    throw new Exception("Код не содержит вызова filling");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateFillingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateFillingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCrushingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCrushingConfig - Генерация");

                var config = new CreateCrushingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.crushing"))
                    throw new Exception("Код не содержит вызова crushing");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCrushingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCrushingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCuttingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCuttingConfig - Генерация");

                var config = new CreateCuttingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.cutting"))
                    throw new Exception("Код не содержит вызова cutting");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCuttingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCuttingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestMillingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateMillingConfig - Генерация");

                var config = new CreateMillingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.milling"))
                    throw new Exception("Код не содержит вызова milling");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateMillingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateMillingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestMixingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateMixingConfig - Генерация");

                var config = new CreateMixingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal" });
                config.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.mixing"))
                    throw new Exception("Код не содержит вызова mixing");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateMixingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateMixingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestPressingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreatePressingConfig - Генерация");

                var config = new CreatePressingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.pressing"))
                    throw new Exception("Код не содержит вызова pressing");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreatePressingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreatePressingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestHauntingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateHauntingConfig - Генерация");

                var config = new CreateHauntingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:campfire" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:soul_campfire", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.haunting"))
                    throw new Exception("Код не содержит вызова haunting");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateHauntingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateHauntingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestSplashingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateSplashingConfig - Генерация");

                var config = new CreateSplashingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:campfire" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:soul_campfire", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.splashing"))
                    throw new Exception("Код не содержит вызова splashing");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateSplashingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateSplashingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestSandpaperPolishingGeneration()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateSandpaperPolishingConfig - Генерация");

                var config = new CreateSandpaperPolishingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains("event.recipes.create.sandpaper_polishing"))
                    throw new Exception("Код не содержит вызова sandpaper_polishing");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateSandpaperPolishingConfig - Генерация", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateSandpaperPolishingConfig - Генерация", ex.Message, sw.Elapsed);
            }
        }

        #endregion

        #region Modifier Tests

        private TestResult TestCompactingModifiers()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCompactingConfig - Модификаторы");

                var config = new CreateCompactingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
                config.Modifiers.Heat = CreateHeatType.Heated;

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains(".heated()"))
                    throw new Exception("Код не содержит модификатора heated");

                // Проверим superheated
                config.Modifiers.Heat = CreateHeatType.Superheated;
                var code2 = config.GenerateJsCode();
                if (!code2.Contains(".superheated()"))
                    throw new Exception("Код не содержит модификатора superheated");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCompactingConfig - Модификаторы", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCompactingConfig - Модификаторы", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestDeployingModifiers()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateDeployingConfig - Модификаторы");

                var config = new CreateDeployingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:sand" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
                config.Modifiers.KeepHeldItem = true;

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains(".keepHeldItem()"))
                    throw new Exception("Код не содержит модификатора keepHeldItem");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateDeployingConfig - Модификаторы", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateDeployingConfig - Модификаторы", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCrushingModifiers()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCrushingConfig - Модификаторы");

                var config = new CreateCrushingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
                config.Modifiers.ProcessingTime = 500;

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains(".processingTime(500)"))
                    throw new Exception("Код не содержит модификатора processingTime");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCrushingConfig - Модификаторы", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCrushingConfig - Модификаторы", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCuttingModifiers()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateCuttingConfig - Модификаторы");

                var config = new CreateCuttingConfig();
                config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
                config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
                config.Modifiers.ProcessingTime = 300;

                var code = config.GenerateJsCode();
                TestLogger.Write($"[GENERATED] {code}");

                if (!code.Contains(".processingTime(300)"))
                    throw new Exception("Код не содержит модификатора processingTime");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateCuttingConfig - Модификаторы", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateCuttingConfig - Модификаторы", ex.Message, sw.Elapsed);
            }
        }

        #endregion

        #region CreateItem.of Tests

        private TestResult TestCreateItemParsing()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateItemComponent - Парсинг");

                // Простой предмет
                var simple = CreateItemComponent.Parse("minecraft:diamond");
                if (simple.ItemId != "minecraft:diamond" || simple.Count != 1 || simple.Chance != null)
                    throw new Exception("Неправильный парсинг простого предмета");

                // Предмет с количеством
                var withCount = CreateItemComponent.Parse("2x minecraft:diamond");
                if (withCount.ItemId != "minecraft:diamond" || withCount.Count != 2 || withCount.Chance != null)
                    throw new Exception("Неправильный парсинг предмета с количеством");

                // Предмет с количеством и шансом
                var withChance = CreateItemComponent.Parse("2x minecraft:diamond", 0.5f);
                if (withChance.ItemId != "minecraft:diamond" || withChance.Count != 2 || withChance.Chance != 0.5f)
                    throw new Exception("Неправильный парсинг предмета с количеством и шансом");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateItemComponent - Парсинг", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateItemComponent - Парсинг", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCreateItemWithChance()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateItemComponent - Шанс");

                var item = CreateItemComponent.Parse("minecraft:diamond", 0.3f);
                var jsString = item.ToJsString();

                TestLogger.Write($"[JS STRING] {jsString}");

                if (!jsString.Contains("CreateItem.of") || !jsString.Contains("0.3"))
                    throw new Exception("Неправильная генерация CreateItem.of с шансом");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateItemComponent - Шанс", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateItemComponent - Шанс", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestCreateItemWithCount()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateItemComponent - Количество");

                var item = CreateItemComponent.Parse("3x minecraft:diamond", 0.7f);
                var jsString = item.ToJsString();

                TestLogger.Write($"[JS STRING] {jsString}");

                if (!jsString.Contains("3x") || !jsString.Contains("0.7"))
                    throw new Exception("Неправильная генерация CreateItem.of с количеством и шансом");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateItemComponent - Количество", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateItemComponent - Количество", ex.Message, sw.Elapsed);
            }
        }

        #endregion

        #region Ingredient Tests

        private TestResult TestFluidIngredient()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateFluidIngredientComponent - Парсинг");

                var fluid = new CreateFluidIngredientComponent
                {
                    FluidId = "minecraft:water",
                    Amount = 1000
                };

                var jsString = fluid.ToJsString();

                TestLogger.Write($"[JS STRING] {jsString}");

                if (!jsString.Contains("Fluid.of") || !jsString.Contains("1000"))
                    throw new Exception("Неправильная генерация Fluid.of");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateFluidIngredientComponent - Парсинг", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateFluidIngredientComponent - Парсинг", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestTagIngredient()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateTagIngredientComponent - Парсинг");

                var tag = new CreateTagIngredientComponent
                {
                    TagId = "#minecraft:logs"
                };

                var jsString = tag.ToJsString();

                TestLogger.Write($"[JS STRING] {jsString}");

                if (jsString != "'#minecraft:logs'")
                    throw new Exception("Неправильная генерация тега");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateTagIngredientComponent - Парсинг", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateTagIngredientComponent - Парсинг", ex.Message, sw.Elapsed);
            }
        }

        private TestResult TestIngredientOf()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                TestLogger.Write("[TEST] Start: CreateItemIngredientComponent - Ingredient.of");

                var ingredient = new CreateItemIngredientComponent
                {
                    ItemId = "minecraft:diamond"
                };

                var jsString = ingredient.ToJsString();

                TestLogger.Write($"[JS STRING] {jsString}");

                if (jsString != "'minecraft:diamond'")
                    throw new Exception("Неправильная генерация обычного предмета");

                sw.Stop();
                TestLogger.Write($"[TEST] Pass\n");
                return TestResult.Pass("CreateItemIngredientComponent - Ingredient.of", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                TestLogger.Write($"[TEST] Fail | {ex.Message}\n");
                return TestResult.Fail("CreateItemIngredientComponent - Ingredient.of", ex.Message, sw.Elapsed);
            }
        }

        #endregion
    }
}
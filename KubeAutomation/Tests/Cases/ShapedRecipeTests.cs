using System;
using System.Collections.Generic;
using System.Linq;
using KubeAutomation.GenerationStrategies.Parsing;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;

namespace KubeAutomation.Tests.Cases
{
    public class ShapedRecipeTests : TestBase
    {
        private readonly JsRecipeParser _parser = new();

        public override List<TestResult> RunAll()
        {
            var results = new List<TestResult>();

            TestLogger.Write("=================================================");
            TestLogger.Write("ShapedRecipeTests: Запуск тестов парсера shaped");
            TestLogger.Write("=================================================");
            TestLogger.Write("");

            TestLogger.Write("[GROUP] Парсинг: базовые случаи");
            results.Add(Test_3x3_ItemOf());
            results.Add(Test_2x2_ItemOf());
            results.Add(Test_MultipleShapedInOneFile());
            results.Add(Test_StringOutput());
            results.Add(Test_NotRawCodeBlock());

            TestLogger.Write("");
            TestLogger.Write("=================================================");
            TestLogger.Write($"ShapedRecipeTests: {results.Count} тестов завершено");
            TestLogger.Write($"   Пройдено: {results.Count(r => r.Passed)}");
            TestLogger.Write($"   Провалено: {results.Count(r => !r.Passed)}");
            TestLogger.Write("=================================================");
            TestLogger.Write("");

            return results;
        }

        private TestResult Test_3x3_ItemOf()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const string name = "Shaped 3x3 с Item.of";
            TestLogger.Write($"  [TEST] {name}");
            try
            {
                var js = Wrap(@"
                    event.shaped(Item.of('create:andesite_alloy', 2), [
                        'ABC',
                        'DEF',
                        'GHI'
                    ], {
                        A: 'minecraft:andesite',
                        B: 'minecraft:stone',
                        C: 'minecraft:cobblestone',
                        D: 'minecraft:gravel',
                        E: 'minecraft:sand',
                        F: 'minecraft:dirt',
                        G: 'minecraft:grass',
                        H: 'minecraft:oak_log',
                        I: 'minecraft:birch_log'
                    })");

                var configs = _parser.ParseContent(js);
                var shaped = configs.OfType<WorkbenchRecipeConfiguration>().FirstOrDefault();

                if (shaped == null)
                    return Fail(name, $"Не распознан как WorkbenchRecipeConfiguration. Типы: {string.Join(", ", configs.Select(c => c.GetType().Name))}", sw);

                if (shaped.itemOutput?.ItemId != "create:andesite_alloy")
                    return Fail(name, $"Неверный output: {shaped.itemOutput?.ItemId}", sw);

                if (shaped.itemOutput?.Amount != 2)
                    return Fail(name, $"Неверное количество: {shaped.itemOutput?.Amount}", sw);

                TestLogger.Write("    PASS");
                sw.Stop();
                return TestResult.Pass(name, sw.Elapsed);
            }
            catch (Exception ex) { return Fail(name, ex.Message, sw); }
        }

        private TestResult Test_2x2_ItemOf()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const string name = "Shaped 2x2 (неполный паттерн)";
            TestLogger.Write($"  [TEST] {name}");
            try
            {
                var js = Wrap(@"
                    event.shaped(Item.of('create:andesite_alloy', 2), [
                        'SS',
                        'AA'
                    ], {
                        A: 'minecraft:andesite',
                        S: 'architects_palette:algal_brick'
                    })");

                var configs = _parser.ParseContent(js);
                var shaped = configs.OfType<WorkbenchRecipeConfiguration>().FirstOrDefault();

                if (shaped == null)
                    return Fail(name, $"Не распознан. Типы: {string.Join(", ", configs.Select(c => c.GetType().Name))}", sw);

                TestLogger.Write("    PASS");
                sw.Stop();
                return TestResult.Pass(name, sw.Elapsed);
            }
            catch (Exception ex) { return Fail(name, ex.Message, sw); }
        }

        private TestResult Test_MultipleShapedInOneFile()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const string name = "Несколько shaped подряд — каждый отдельный конфиг";
            TestLogger.Write($"  [TEST] {name}");
            try
            {
                var js = Wrap(@"
                    event.shaped(Item.of('create:andesite_alloy', 2), [
                        'SS',
                        'AA'
                    ], {
                        A: 'minecraft:andesite',
                        S: 'architects_palette:algal_brick'
                    })
                    event.shaped(Item.of('create:andesite_alloy', 2), [
                        'AA',
                        'SS'
                    ], {
                        A: 'minecraft:andesite',
                        S: 'architects_palette:algal_brick'
                    })");

                var configs = _parser.ParseContent(js);
                var shaped = configs.OfType<WorkbenchRecipeConfiguration>().ToList();

                if (shaped.Count != 2)
                    return Fail(name, $"Ожидалось 2 WorkbenchRecipeConfiguration, получено {shaped.Count}. Все типы: {string.Join(", ", configs.Select(c => c.GetType().Name))}", sw);

                TestLogger.Write("    PASS");
                sw.Stop();
                return TestResult.Pass(name, sw.Elapsed);
            }
            catch (Exception ex) { return Fail(name, ex.Message, sw); }
        }

        private TestResult Test_StringOutput()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const string name = "Shaped со строковым output";
            TestLogger.Write($"  [TEST] {name}");
            try
            {
                var js = Wrap(@"
                    event.shaped('create:belt_connector', [
                        'SSS',
                        'SSS'
                    ], {
                        S: 'tfmg:rubber_sheet'
                    })");

                var configs = _parser.ParseContent(js);
                var shaped = configs.OfType<WorkbenchRecipeConfiguration>().FirstOrDefault();

                if (shaped == null)
                    return Fail(name, $"Не распознан. Типы: {string.Join(", ", configs.Select(c => c.GetType().Name))}", sw);

                if (shaped.itemOutput?.ItemId != "create:belt_connector")
                    return Fail(name, $"Неверный output: {shaped.itemOutput?.ItemId}", sw);

                TestLogger.Write("    PASS");
                sw.Stop();
                return TestResult.Pass(name, sw.Elapsed);
            }
            catch (Exception ex) { return Fail(name, ex.Message, sw); }
        }

        private TestResult Test_NotRawCodeBlock()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const string name = "Shaped не попадает в RawCodeBlock";
            TestLogger.Write($"  [TEST] {name}");
            try
            {
                var js = Wrap(@"
                    event.shaped(Item.of('minecraft:stick', 4), [
                        'W',
                        'W'
                    ], {
                        W: 'minecraft:oak_planks'
                    })");

                var configs = _parser.ParseContent(js);
                var raw = configs.OfType<RawCodeBlock>().ToList();

                if (raw.Count > 0)
                    return Fail(name, $"Обнаружен RawCodeBlock: {raw.Count} шт.", sw);

                TestLogger.Write("    PASS");
                sw.Stop();
                return TestResult.Pass(name, sw.Elapsed);
            }
            catch (Exception ex) { return Fail(name, ex.Message, sw); }
        }

        private static string Wrap(string body) =>
            $"ServerEvents.recipes(event => {{{body}}});";

        private TestResult Fail(string name, string reason, System.Diagnostics.Stopwatch sw)
        {
            TestLogger.Write($"    FAIL: {reason}");
            sw.Stop();
            return TestResult.Fail(name, reason, sw.Elapsed);
        }
    }
}

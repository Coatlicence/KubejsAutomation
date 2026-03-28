using Esprima;
using Esprima.Ast;
using KubeAutomation;
using KubeAutomation.FileSaveStrategies;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.Templates;
using KubeAutomation.Tests;
using KubeScriptAutomation;
using KubeScriptAutomation.Collectos;
using LogExtractorLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes; // Добавлено для NamedPipeClientStream
using System.Linq;
using System.Text.Json; // Добавлено для десериализации
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

class Program
{
     static readonly MyForm form = MyForm.GetInstance();

    [STAThread]
    static async Task Main()
    {
        // Запускаем форму в отдельном потоке
        Thread formThread = new(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form);
        });
        formThread.SetApartmentState(ApartmentState.STA);
        formThread.IsBackground = true;
        formThread.Start();


        Console.WriteLine("Система автоматизации KubeJS скриптов для GregTech");
        Console.WriteLine("===============================================");

        var inputCollector = new GregTechRecipeCodeGenerator.RecipeInputCollector();
        var fileSaver = new RecipeFileSaver();

        while (true)
        {
            Console.WriteLine("\nХотите создать новый рецепт? (да/нет/получить)");

            string res = Console.ReadLine()?.Trim().ToLower();

            if (res == "получить" || res == "п" || res == "g" || res == "get")
            {
                // перенесено в форму
                //_ = StartExternalAppAsync(); // Запускает задачу в фоне, не блокируя поток
            }

            if (res == "t" || res == "т")
            {
                TestRunner.RunAll();
                Console.WriteLine("\n\n\n   Введите любой символ для продолжения...");
                Console.ReadLine();
                continue;
            }

            if (res == "пр" || res == "gr")
            {
                try
                {
                    var extractor = new RecipesExtractor();
                    Console.WriteLine($"Экстрактор готов к работе: {extractor.IsValid()}.");

                    var recipes = await extractor.ExtractAsync();
                    Console.WriteLine($"Получено {recipes.Count} рецептов.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при извлечении рецептов: {ex.Message}");
                }
            }

            if (res == "н" || res == "нет" || res == "n" || res == "no")
                break;

            try
            {
                var config = inputCollector.Collect();
                string script = GregTechRecipeCodeGenerator.Generate(config);

                string? dir = null;
                while (dir == null)
                {
                    Console.WriteLine("Введите путь сохранения");
                    dir = Console.ReadLine()?.Trim();

                    if (dir == "н" || dir == "нет" || dir == "n" || dir == "no")
                    {
                        dir = "C:\\Users\\f0578\\OneDrive\\Desktop\\Предметы";
                        break;
                    }
                }
                
                fileSaver.Save(script, config.RecipeId, dir);

                Console.WriteLine("\nРецепт успешно создан!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании рецепта: {ex.Message}");
            }
        }
        ServerRecipeType s2 = new();
        Console.WriteLine(EmptyFunctionGenerator.GenerateTitle(s2));

        Console.WriteLine("Программа завершена.");
    }

    // Асинхронная функция для запуска внешнего приложения и получения данных из именного канала
    public static async Task StartExternalAppAsync()
    {
        //const string pipeName = "MyMinecraftItemsPipe"; // Не используется напрямую теперь
        //string logPath = "C:\\Users\\nojda\\AppData\\Roaming\\.tlauncher\\legacy\\Minecraft\\game\\logs\\debug.log"; // Не нужно

        await Task.Delay(1000);

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = @"C:\Users\nojda\source\repos\LogExtractor\LogExtractor\bin\Debug\net8.0\LogExtractor.exe",
            Arguments = "all",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        var process = System.Diagnostics.Process.Start(processStartInfo);
        if (process == null)
        {
            Console.WriteLine("Ошибка: не удалось запустить внешнее приложение.");
            return;
        }

        Console.WriteLine("Внешнее приложение запущено. Ожидание подключения к каналам...");

        // === Подключаемся к каналу предметов/жидкостей ===
        await ReceiveItemsAndFluidsAsync(process);

        // === Подключаемся к каналу рецептов ===
        await ReceiveRecipesAsync(process);

        Console.WriteLine("Все данные получены из каналов.");
    }

    private static async Task ReceiveItemsAndFluidsAsync(System.Diagnostics.Process process)
    {
        const string pipeName = "MyMinecraftItemsPipe";

        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
            await client.ConnectAsync(30000); // Асинхронное подключение
            Console.WriteLine("Подключено к именованному каналу предметов/жидкостей.");

            using var reader = new StreamReader(client, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: false);
            string lengthStr = await reader.ReadLineAsync();
            if (!int.TryParse(lengthStr, out int itemsDataLength))
            {
                Console.WriteLine("Ошибка: не удалось прочитать длину данных предметов из канала.");
                return;
            }

            var buffer = new byte[itemsDataLength];
            int totalRead = 0;
            while (totalRead < itemsDataLength)
            {
                int read = await client.ReadAsync(buffer, totalRead, itemsDataLength - totalRead);
                if (read == 0) break;
                totalRead += read;
            }

            if (totalRead != itemsDataLength)
            {
                Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {itemsDataLength}.");
                return;
            }

            string itemsJson = System.Text.Encoding.UTF8.GetString(buffer);
            var itemIds = JsonSerializer.Deserialize<List<string>>(itemsJson, new JsonSerializerOptions());

            // Читаем ID жидкостей
            string lengthStr2 = await reader.ReadLineAsync();
            if (!int.TryParse(lengthStr2, out int fluidsDataLength))
            {
                Console.WriteLine("Ошибка: не удалось прочитать длину данных жидкостей из канала.");
                return;
            }

            var buffer2 = new byte[fluidsDataLength];
            totalRead = 0;
            while (totalRead < fluidsDataLength)
            {
                int read = await client.ReadAsync(buffer2, totalRead, fluidsDataLength - totalRead);
                if (read == 0) break;
                totalRead += read;
            }

            if (totalRead != fluidsDataLength)
            {
                Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {fluidsDataLength}.");
                return;
            }

            string fluidsJson = System.Text.Encoding.UTF8.GetString(buffer2);
            var fluidIds = JsonSerializer.Deserialize<List<string>>(fluidsJson, new JsonSerializerOptions());

            // Читаем количество изображений
            string countItemsStr = await reader.ReadLineAsync();
            string countFluidsStr = await reader.ReadLineAsync();

            if (!int.TryParse(countItemsStr, out int totalItems) || !int.TryParse(countFluidsStr, out int totalFluids))
            {
                Console.WriteLine("Ошибка: не удалось прочитать количество изображений.");
                return;
            }

            Console.WriteLine($"Ожидается {totalItems} изображений предметов и {totalFluids} изображений жидкостей.");

            // Читаем изображения предметов
            var itemImages = new Dictionary<string, byte[]>();
            for (int i = 0; i < totalItems; i++)
            {
                string imgSizeStr = await reader.ReadLineAsync();
                if (!int.TryParse(imgSizeStr, out int imgSize))
                {
                    Console.WriteLine($"Ошибка: не удалось прочитать размер изображения предмета {i}.");
                    return;
                }

                var imgBuffer = new byte[imgSize];
                int totalImgRead = 0;
                while (totalImgRead < imgSize)
                {
                    int read = await client.ReadAsync(imgBuffer, totalImgRead, imgSize - totalImgRead);
                    if (read == 0) break;
                    totalImgRead += read;
                }

                if (totalImgRead != imgSize)
                {
                    Console.WriteLine($"Ошибка: прочитано {totalImgRead} байт изображения, ожидалось {imgSize}.");
                    return;
                }

                string id = itemIds[i];
                itemImages[id] = imgBuffer;
            }

            // Читаем изображения жидкостей
            var fluidImages = new Dictionary<string, byte[]>();
            for (int i = 0; i < totalFluids; i++)
            {
                string imgSizeStr = await reader.ReadLineAsync();
                if (!int.TryParse(imgSizeStr, out int imgSize))
                {
                    Console.WriteLine($"Ошибка: не удалось прочитать размер изображения жидкости {i}.");
                    return;
                }

                var imgBuffer = new byte[imgSize];
                int totalImgRead = 0;
                while (totalImgRead < imgSize)
                {
                    int read = await client.ReadAsync(imgBuffer, totalImgRead, imgSize - totalImgRead);
                    if (read == 0) break;
                    totalImgRead += read;
                }

                if (totalImgRead != imgSize)
                {
                    Console.WriteLine($"Ошибка: прочитано {totalImgRead} байт изображения, ожидалось {imgSize}.");
                    return;
                }

                string id = fluidIds[i];
                fluidImages[id] = imgBuffer;
            }

            Console.WriteLine($"Получено {itemIds.Count} ID предметов и {fluidIds.Count} ID жидкостей с изображениями.");


            MyForm.GetInstance().UpdateData(itemImages, fluidImages);
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Ошибка: не удалось подключиться к каналу предметов/жидкостей в течение 30 секунд.");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка именованного канала (предметы/жидкости): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка десериализации JSON (предметы/жидкости): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка (предметы/жидкости): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
    }

    private static async Task ReceiveRecipesAsync(System.Diagnostics.Process process)
    {
        const string pipeName = "MyMinecraftRecipesPipe";

        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
            await client.ConnectAsync(30000); // Асинхронное подключение
            Console.WriteLine("Подключено к именованному каналу рецептов.");

            using var reader = new StreamReader(client, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: false);
            string lengthStr = await reader.ReadLineAsync();
            if (!int.TryParse(lengthStr, out int dataLength))
            {
                Console.WriteLine("Ошибка: не удалось прочитать длину данных рецептов из канала.");
                return;
            }

            var buffer = new byte[dataLength];
            int totalRead = 0;
            while (totalRead < dataLength)
            {
                int read = await client.ReadAsync(buffer, totalRead, dataLength - totalRead);
                if (read == 0) break;
                totalRead += read;
            }

            if (totalRead != dataLength)
            {
                Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {dataLength}.");
                return;
            }

            string json = System.Text.Encoding.UTF8.GetString(buffer);
            var recipes = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions());

            if (recipes != null)
            {
                Console.WriteLine($"Получено {recipes.Count} ID рецептов.");

                MyForm.GetInstance().UpdateRecipes(recipes);
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Ошибка: не удалось подключиться к каналу рецептов в течение 30 секунд.");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка именованного канала (рецепты): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка десериализации JSON (рецепты): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка (рецепты): {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
    }

    private static void Test1()
    {
        WorkbenchRecipeConfiguration config1 = new();

        config1.itemOutput = new ItemComponent(2, "create:white_sail");

        WorkbenchCraftGrid grid = new(" DD", "NAN", "SSS");

        Dictionary<char, string> keymap = [];
        keymap.Add('S', "minecraft:stick");
        keymap.Add('N', "minecraft:iron_nugget");
        keymap.Add('A', "#forge:wool");
        keymap.Add('D', "mine");

        RecipePattern pattern = new(grid, keymap);

        Console.WriteLine(pattern.ToString());

        config1.Validate();
    }

    private static void Test2()
    {
        var conf = new GregTechRecipeConfiguration();

        conf.EUt = 10;
        conf.DurationSeconds = 1;

        conf.InputFluids.Add(new FluidComponent("test", 1));
        conf.OutputFluids.Add(new FluidComponent("test1", 1));
        conf.MachineType = GregTechRecipeCodeGenerator.Machines[0];

        conf.Validate();

        var script1 = GregTechRecipeCodeGenerator.Generate(conf);
        var saver = new FileSaverToEnd();

        saver.Save(script1, "teststamp", "C:\\Users\\f0578\\OneDrive\\Desktop\\Предметы");

    }

    private static void TestJSONGen()
    {
        var conf = new CustomRecipeConfig();

        // ============================================================
        // 1. КОРНЕВЫЕ СВОЙСТВА
        // ============================================================
        conf.CreateProperty("type", JsonType.String);
        conf.SetValue("type", "create:sequenced_assembly");

        conf.CreateProperty("loops", JsonType.Int);
        conf.SetValue("loops", 2);

        // ============================================================
        // 2. INGREDIENT (объект с одним свойством)
        // ============================================================
        conf.CreateProperty("ingredient", JsonType.Object);
        conf.CreateProperty("ingredient.tag", JsonType.String);
        conf.SetValue("ingredient.tag", "c:ingots/steel");

        // ============================================================
        // 3. RESULTS (массив с 1 объектом)
        // ============================================================
        conf.CreateArray("results");
        string result0 = conf.AddObjectToArray("results"); // results[0]
        conf.CreateProperty(result0 + ".id", JsonType.String);
        conf.SetValue(result0 + ".id", "tfmg:heavy_plate");

        // ============================================================
        // 4. SEQUENCE (массив с 3 объектами)
        // ============================================================
        conf.CreateArray("sequence");

        // --- Шаг 1 ---
        string step0 = conf.AddObjectToArray("sequence"); // sequence[0]
        conf.CreateProperty(step0 + ".type", JsonType.String);
        conf.SetValue(step0 + ".type", "create:pressing");

        // sequence[0].ingredients (массив с 1 объектом)
        conf.CreateArray(step0 + ".ingredients");
        string step0_ing0 = conf.AddObjectToArray(step0 + ".ingredients");
        conf.CreateProperty(step0_ing0 + ".item", JsonType.String);
        conf.SetValue(step0_ing0 + ".item", "tfmg:unprocessed_heavy_plate");

        // sequence[0].results (массив с 1 объектом)
        conf.CreateArray(step0 + ".results");
        string step0_res0 = conf.AddObjectToArray(step0 + ".results");
        conf.CreateProperty(step0_res0 + ".id", JsonType.String);
        conf.SetValue(step0_res0 + ".id", "tfmg:unprocessed_heavy_plate");

        // --- Шаг 2 ---
        string step1 = conf.AddObjectToArray("sequence"); // sequence[1]
        conf.CreateProperty(step1 + ".type", JsonType.String);
        conf.SetValue(step1 + ".type", "create:pressing");

        // sequence[1].ingredients
        conf.CreateArray(step1 + ".ingredients");
        string step1_ing0 = conf.AddObjectToArray(step1 + ".ingredients");
        conf.CreateProperty(step1_ing0 + ".item", JsonType.String);
        conf.SetValue(step1_ing0 + ".item", "tfmg:unprocessed_heavy_plate");

        // sequence[1].results
        conf.CreateArray(step1 + ".results");
        string step1_res0 = conf.AddObjectToArray(step1 + ".results");
        conf.CreateProperty(step1_res0 + ".id", JsonType.String);
        conf.SetValue(step1_res0 + ".id", "tfmg:unprocessed_heavy_plate");

        // --- Шаг 3 ---
        string step2 = conf.AddObjectToArray("sequence"); // sequence[2]
        conf.CreateProperty(step2 + ".type", JsonType.String);
        conf.SetValue(step2 + ".type", "create:pressing");

        // sequence[2].ingredients
        conf.CreateArray(step2 + ".ingredients");
        string step2_ing0 = conf.AddObjectToArray(step2 + ".ingredients");
        conf.CreateProperty(step2_ing0 + ".item", JsonType.String);
        conf.SetValue(step2_ing0 + ".item", "tfmg:unprocessed_heavy_plate");

        // sequence[2].results
        conf.CreateArray(step2 + ".results");
        string step2_res0 = conf.AddObjectToArray(step2 + ".results");
        conf.CreateProperty(step2_res0 + ".id", JsonType.String);
        conf.SetValue(step2_res0 + ".id", "tfmg:unprocessed_heavy_plate");

        // ============================================================
        // 5. TRANSITIONAL_ITEM (объект с одним свойством)
        // ============================================================
        conf.CreateProperty("transitional_item", JsonType.Object);
        conf.CreateProperty("transitional_item.id", JsonType.String);
        conf.SetValue("transitional_item.id", "tfmg:unprocessed_heavy_plate");

        // ============================================================
        // 6. ГЕНЕРАЦИЯ
        // ============================================================
        string json = conf.ToJson();
        Console.WriteLine(json);

        Console.WriteLine(conf.ToJson());
    }

    private static void TestJSONTemplateSave()
    {
        // 1. Создать конфиг
        var conf = new CustomRecipeConfig();
        conf.CreateProperty("type", JsonType.String);
        conf.SetValue("type", "create:sequenced_assembly");

        conf.CreateProperty("ingredient", JsonType.Object);
        conf.CreateProperty("ingredient.item", JsonType.String);
        conf.SetValue("ingredient.item", "create:copper_sheet");

        conf.CreateProperty("loops", JsonType.Int);
        conf.SetValue("loops", 1);

        conf.CreateProperty("transitionalItem", JsonType.Object);
        conf.CreateProperty("transitionalItem.item", JsonType.String);
        conf.SetValue("transitionalItem.item", "create:copper_sheet");

        conf.CreateArray("results");
        string result0 = conf.AddObjectToArray("results");
        conf.CreateProperty(result0 + ".item", JsonType.String);
        conf.SetValue(result0 + ".item", "create:transmitter");
        conf.CreateProperty(result0 + ".chance", JsonType.Int);
        conf.SetValue(result0 + ".chance", 100);

        conf.CreateArray("sequence");
        string step0 = conf.AddObjectToArray("sequence");
        conf.CreateProperty(step0 + ".type", JsonType.String);
        conf.SetValue(step0 + ".type", "create:deploying");
        // ... остальные поля sequence ...

        // 2. Сохранить как шаблон
        bool success = TemplateManager.SaveTemplate(
            templateName: "sequenced_assembly",
            config: conf,
            category: "create",
            description: "Последовательная сборка Create с deployers",
            author: "User",
            overwrite: true
        );

        Console.WriteLine(success ? "Шаблон сохранён!" : "Ошибка сохранения");
    }

    private static void TestJSONTemplateLoad()
    {
        // ============================================================
        // ПРИМЕР 1: Загрузка шаблона и заполнение значений
        // ============================================================

        Console.WriteLine("=== Загрузка шаблона ===");

        // Загрузить шаблон
        var result = TemplateManager.LoadTemplate("sequenced_assembly", "create");

        // Проверить успех
        if (!result.Success)
        {
            Console.WriteLine($"Ошибка: {result.ErrorMessage}");
            return;
        }

        Console.WriteLine($"Шаблон загружен: {result.Metadata.Name}");
        Console.WriteLine($"Описание: {result.Metadata.Description}");

        // Получить Config для заполнения
        var config11 = result.Config;

        // Заполнить Required поля (которые были null в шаблоне)
        config11.SetValue("ingredient.item", "create:copper_sheet");
        config11.SetValue("transitionalItem.item", "create:copper_sheet");
        config11.SetValue("results[0].item", "create:transmitter");

        // Сгенерировать JSON
        string json = config11.ToJson();
        Console.WriteLine("\n=== Сгенерированный JSON ===");
        Console.WriteLine(json);


        // ============================================================
        // ПРИМЕР 2: Список всех шаблонов
        // ============================================================

        Console.WriteLine("\n=== Доступные шаблоны ===");

        var templates = TemplateManager.ListTemplates();

        foreach (var template in templates)
        {
            Console.WriteLine($"   {template.Category}/{template.Name}");
            Console.WriteLine($"   {template.Description}");
            Console.WriteLine($"   Автор: {template.Author}");
            Console.WriteLine();
        }


        // ============================================================
        // ПРИМЕР 3: Полный цикл (загрузка → заполнение → сохранение)
        // ============================================================

        // 1. Загрузить шаблон
        var loadResult = TemplateManager.LoadTemplate("sequenced_assembly", "create");

        if (!loadResult.Success)
        {
            Console.WriteLine($"Ошибка загрузки: {loadResult.ErrorMessage}");
            return;
        }

        // 2. Заполнить Required поля
        var config12 = loadResult.Config;
        config12.SetValue("ingredient.item", "create:copper_sheet");
        config12.SetValue("transitionalItem.item", "create:copper_sheet");
        config12.SetValue("results[0].item", "create:transmitter");

        // 3. Сгенерировать JSON для KubeJS
        string kubejsJson = config12.ToJson();
        Console.WriteLine(kubejsJson);


    }
}

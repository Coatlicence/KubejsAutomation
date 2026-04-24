using Esprima;
using Esprima.Ast;
using KubeAutomation;
using KubeAutomation.FileSaveStrategies;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.Parsing;
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
using System.Net.Http.Json;
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

        TestJS();

        Console.WriteLine("Система автоматизации KubeJS скриптов");
        Console.WriteLine("===============================================");

        var inputCollector = new GregTechRecipeCodeGenerator.RecipeInputCollector();
        var fileSaver = new RecipeFileSaver();

        while (true)
        {
            Console.WriteLine("\nХотите создать новый рецепт? (да/нет/получить)");

            string res = Console.ReadLine()?.Trim().ToLower()!;

            if (res == "получить" || res == "п" || res == "g" || res == "get")
            {
                // перенесено в форму
                //_ = StartExternalAppAsync(); // Запускает задачу в фоне, не блокируя поток
            }

            if (res == "t" || res == "т" || res == "test" || res == "тест")
            {
                TestRunner.RunAll();
                Console.WriteLine("\n\n\n   Введите любой символ для продолжения...");
                Console.ReadKey();
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
                
                //fileSaver.Save(script, config.RecipeId, dir);

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

    public static void TestJS()
    {
        Console.WriteLine("=== Тест парсера JS ===\n");

        string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test_recipes.js");

        if (!File.Exists(testFilePath))
        {
            testFilePath = "test_recipes.js";
        }

        Console.WriteLine($"Путь к файлу: {Path.GetFullPath(testFilePath)}");
        Console.WriteLine($"Файл существует: {File.Exists(testFilePath)}\n");

        if (!File.Exists(testFilePath))
        {
            Console.WriteLine("[ERROR] Файл не найден! Создайте test_recipes.js в корне проекта.");
            Console.ReadKey();
            return;
        }


        try
        {
            var parser = new JsRecipeParser();

            Console.WriteLine("Чтение файла...");
            var configs = parser.ParseFile(testFilePath);

            // Вывод с порядком и позициями
            Console.WriteLine($"\n=== Элементы в порядке файла (всего: {configs.Count}) ===");

            int removalCount = 0;
            int customCount = 0;
            int rawCount = 0;
            int otherCount = 0;

            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                Console.WriteLine($"\n[{i + 1}]");

                if (config is RecipeRemovalConfig removal)
                {
                    removalCount++;
                    Console.WriteLine($"    [Удаление] фильтров: {removal.Filters.Count}");
                    foreach (var f in removal.Filters)
                    {
                        Console.WriteLine($"      • mod:{f.Mod ?? "-"} out:{f.Output ?? "-"} type:{f.Type ?? "-"}");
                    }
                }
                else if (config is CustomRecipeConfig custom)
                {
                    customCount++;
                    var type = custom.GetValue("type") as string ?? "-";
                    var id = custom.RecipeId ?? "-";
                    Console.WriteLine($"    [custom] type:{type} id:{id}");
                }
                else if (config is RawCodeBlock raw)
                {
                    rawCount++;
                    // ❗ RawCodeBlock не имеет свойства Content — используйте RawNode?.ToString()
                    var code = raw.RawNode?.ToString() ?? "[empty]";
                    Console.WriteLine($"    [Сырой код] {raw.Reason}: {code.Substring(0, Math.Min(40, code.Length))}...");
                }
            }

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine($"[СТАТИСТИКА]:");
            Console.WriteLine($"   Удаление: {removalCount}");
            Console.WriteLine($"   Custom: {customCount}");
            Console.WriteLine($"   Сырой код: {rawCount}");
            Console.WriteLine($"   Другие: {otherCount}");
            Console.WriteLine($"   Всего: {configs.Count}");
            Console.WriteLine("\n--------------------------------------------------");


            // Генерация обратно в JS
            Console.WriteLine($"\n=== Генерация обратно в JS ===");
            foreach (var config in configs)
            {
                Console.WriteLine(config.GenerateJsCode());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
            }
        }

        Console.WriteLine("\n=== Тест завершён ===");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();

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


}

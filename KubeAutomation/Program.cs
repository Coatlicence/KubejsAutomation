using KubeAutomation;
using KubeAutomation.FileSaveStrategies;
using KubeAutomation.GenerationStrategies.Generators;
using KubeAutomation.GenerationStrategies.Parsing;
using KubeAutomation.GenerationStrategies.RecipeConfigurations;
using KubeAutomation.GenerationStrategies.RecipeConfigurations.Create;
using KubeAutomation.Tests;
using LogExtractorLibrary;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

// --- Класс для логики ---
internal static class MainLogic
{
    public static async Task StartMainLogicAsync(WinUIWindow? mainWindow)
    {
        if (mainWindow == null) return;
        await mainWindow.RunItemDemo();
        //await Task.Run(async () =>
        //{
        //    Console.WriteLine("Система автоматизации KubeJS скриптов");
        //    Console.WriteLine("===============================================");

        //    GamePathProvider.SetGameRootPath("C:\\Users\\KOMP_2024\\AppData\\Roaming\\Create_Cog_And_Circut");

        //    await RunItemDemo();

        //    var inputCollector = new GregTechRecipeCodeGenerator.RecipeInputCollector();
        //    var fileSaver = new RecipeFileSaver();

        //    while (true)
        //    {
        //        Console.WriteLine("\nХотите создать новый рецепт? (да/нет/получить)");

        //        string? res = Console.ReadLine()?.Trim().ToLower();

        //        //if (res == "получить" || res == "п" || res == "g" || res == "get")
        //        //{
        //        //    _ = StartExternalAppAsync(mainWindow);
        //        //}

        //        if (res == "t" || res == "т" || res == "test" || res == "тест")
        //        {
        //            TestRunner.RunAll();
        //            Console.WriteLine("\n\n\n   Введите любой символ для продолжения...");
        //            Console.ReadKey();
        //            continue;
        //        }

        //        if (res == "пр" || res == "gr")
        //        {
        //            try
        //            {
        //                var extractor = new RecipesExtractor();
        //                Console.WriteLine($"Экстрактор готов к работе: {extractor.IsValid()}.");

        //                var recipes = await extractor.ExtractAsync();
        //                Console.WriteLine($"Получено {recipes.Count} рецептов.");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Ошибка при извлечении рецептов: {ex.Message}");
        //            }
        //        }

        //        if (res == "н" || res == "нет" || res == "n" || res == "no")
        //            break;

        //        try
        //        {
        //            var config = inputCollector.Collect();
        //            string script = GregTechRecipeCodeGenerator.Generate(config);

        //            string? dir = null;
        //            while (dir == null)
        //            {
        //                Console.WriteLine("Введите путь сохранения");
        //                dir = Console.ReadLine()?.Trim();

        //                if (dir == "н" || dir == "нет" || dir == "n" || dir == "no")
        //                {
        //                    dir = "C:\\Users\\f0578\\OneDrive\\Desktop\\Предметы";
        //                    break;
        //                }
        //            }

        //            Console.WriteLine("\nРецепт успешно создан!");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Ошибка при создании рецепта: {ex.Message}");
        //        }
        //    }
        //    ServerRecipeType s2 = new();
        //    Console.WriteLine(EmptyFunctionGenerator.GenerateTitle(s2));

        //    Console.WriteLine("Программа завершена.");
        //});
    }

    // --- Переносим все остальные методы из Program.cs сюда ---
    // ... (скопируйте все методы из старого Program.cs, кроме Main и объявлений классов) ...
    // Например:

    //public static async Task StartExternalAppAsync(WinUIWindow mainWindow)
    //{
    //    //const string pipeName = "MyMinecraftItemsPipe"; // Не используется напрямую теперь
    //    //string logPath = "C:\\Users\\nojda\\AppData\\Roaming\\.tlauncher\\legacy\\Minecraft\\game\\logs\\debug.log"; // Не нужно

    //    await Task.Delay(1000);

    //    var processStartInfo = new System.Diagnostics.ProcessStartInfo
    //    {
    //        FileName = @"C:\Users\nojda\source\repos\LogExtractor\LogExtractor\bin\Debug\net8.0\LogExtractor.exe",
    //        Arguments = "all",
    //        UseShellExecute = false,
    //        CreateNoWindow = true,
    //        RedirectStandardOutput = false,
    //        RedirectStandardError = false
    //    };

    //    var process = System.Diagnostics.Process.Start(processStartInfo);
    //    if (process == null)
    //    {
    //        Console.WriteLine("Ошибка: не удалось запустить внешнее приложение.");
    //        return;
    //    }

    //    Console.WriteLine("Внешнее приложение запущено. Ожидание подключения к каналам...");

    //    // === Подключаемся к каналу предметов/жидкостей ===
    //    await ReceiveItemsAndFluidsAsync(process, mainWindow); // Передаем форму

    //    // === Подключаемся к каналу рецептов ===
    //    await ReceiveRecipesAsync(process, mainWindow); // Передаем форму

    //    Console.WriteLine("Все данные получены из каналов.");
    //}

    //private static async Task ReceiveItemsAndFluidsAsync(System.Diagnostics.Process process, WinUIWindow mainWindow)
    //{
    //    const string pipeName = "MyMinecraftItemsPipe";

    //    try
    //    {
    //        using var client = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.In);
    //        await client.ConnectAsync(30000); // Асинхронное подключение
    //        Console.WriteLine("Подключено к именованному каналу предметов/жидкостей.");

    //        using var reader = new System.IO.StreamReader(client, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: false);
    //        string lengthStr = await reader.ReadLineAsync();
    //        if (!int.TryParse(lengthStr, out int itemsDataLength))
    //        {
    //            Console.WriteLine("Ошибка: не удалось прочитать длину данных предметов из канала.");
    //            return;
    //        }

    //        var buffer = new byte[itemsDataLength];
    //        int totalRead = 0;
    //        while (totalRead < itemsDataLength)
    //        {
    //            int read = await client.ReadAsync(buffer, totalRead, itemsDataLength - totalRead);
    //            if (read == 0) break;
    //            totalRead += read;
    //        }

    //        if (totalRead != itemsDataLength)
    //        {
    //            Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {itemsDataLength}.");
    //            return;
    //        }

    //        string itemsJson = System.Text.Encoding.UTF8.GetString(buffer);
    //        var itemIds = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(itemsJson, new System.Text.Json.JsonSerializerOptions());

    //        // Читаем ID жидкостей
    //        string lengthStr2 = await reader.ReadLineAsync();
    //        if (!int.TryParse(lengthStr2, out int fluidsDataLength))
    //        {
    //            Console.WriteLine("Ошибка: не удалось прочитать длину данных жидкостей из канала.");
    //            return;
    //        }

    //        var buffer2 = new byte[fluidsDataLength];
    //        totalRead = 0;
    //        while (totalRead < fluidsDataLength)
    //        {
    //            int read = await client.ReadAsync(buffer2, totalRead, fluidsDataLength - totalRead);
    //            if (read == 0) break;
    //            totalRead += read;
    //        }

    //        if (totalRead != fluidsDataLength)
    //        {
    //            Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {fluidsDataLength}.");
    //            return;
    //        }

    //        string fluidsJson = System.Text.Encoding.UTF8.GetString(buffer2);
    //        var fluidIds = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(fluidsJson, new System.Text.Json.JsonSerializerOptions());

    //        // Читаем количество изображений
    //        string countItemsStr = await reader.ReadLineAsync();
    //        string countFluidsStr = await reader.ReadLineAsync();

    //        if (!int.TryParse(countItemsStr, out int totalItems) || !int.TryParse(countFluidsStr, out int totalFluids))
    //        {
    //            Console.WriteLine("Ошибка: не удалось прочитать количество изображений.");
    //            return;
    //        }

    //        Console.WriteLine($"Ожидается {totalItems} изображений предметов и {totalFluids} изображений жидкостей.");

    //        // Читаем изображения предметов
    //        var itemImages = new System.Collections.Generic.Dictionary<string, byte[]>();
    //        for (int i = 0; i < totalItems; i++)
    //        {
    //            string imgSizeStr = await reader.ReadLineAsync();
    //            if (!int.TryParse(imgSizeStr, out int imgSize))
    //            {
    //                Console.WriteLine($"Ошибка: не удалось прочитать размер изображения предмета {i}.");
    //                return;
    //            }

    //            var imgBuffer = new byte[imgSize];
    //            int totalImgRead = 0;
    //            while (totalImgRead < imgSize)
    //            {
    //                int read = await client.ReadAsync(imgBuffer, totalImgRead, imgSize - totalImgRead);
    //                if (read == 0) break;
    //                totalImgRead += read;
    //            }

    //            if (totalImgRead != imgSize)
    //            {
    //                Console.WriteLine($"Ошибка: прочитано {totalImgRead} байт изображения, ожидалось {imgSize}.");
    //                return;
    //            }

    //            string id = itemIds[i];
    //            itemImages[id] = imgBuffer;
    //        }

    //        // Читаем изображения жидкостей
    //        var fluidImages = new System.Collections.Generic.Dictionary<string, byte[]>();
    //        for (int i = 0; i < totalFluids; i++)
    //        {
    //            string imgSizeStr = await reader.ReadLineAsync();
    //            if (!int.TryParse(imgSizeStr, out int imgSize))
    //            {
    //                Console.WriteLine($"Ошибка: не удалось прочитать размер изображения жидкости {i}.");
    //                return;
    //            }

    //            var imgBuffer = new byte[imgSize];
    //            int totalImgRead = 0;
    //            while (totalImgRead < imgSize)
    //            {
    //                int read = await client.ReadAsync(imgBuffer, totalImgRead, imgSize - totalImgRead);
    //                if (read == 0) break;
    //                totalImgRead += read;
    //            }

    //            if (totalImgRead != imgSize)
    //            {
    //                Console.WriteLine($"Ошибка: прочитано {totalImgRead} байт изображения, ожидалось {imgSize}.");
    //                return;
    //            }

    //            string id = fluidIds[i];
    //            fluidImages[id] = imgBuffer;
    //        }

    //        Console.WriteLine($"Получено {itemIds.Count} ID предметов и {fluidIds.Count} ID жидкостей с изображениями.");


    //        // Вызов метода формы для обновления данных
    //        // Важно: вызывать через Invoke, если поток клиента отличается от потока формы
    //        //var formInstance = MyForm.GetInstance();
    //        //if (formInstance.InvokeRequired)
    //        //{
    //        //    formInstance.Invoke(new Action(() => formInstance.UpdateData(itemImages, fluidImages)));
    //        //}
    //        //else
    //        //{
    //        //    formInstance.UpdateData(itemImages, fluidImages);
    //        //}

    //        // *** КРИТИЧЕСКОЕ ИЗМЕНЕНИЕ ***
    //        // Вместо вызова formInstance.UpdateData(itemImages, fluidImages)
    //        // Создаем список MinecraftItem из полученных данных
    //        var allItems = new List<MinecraftItem>();

    //        // Предположим, у вас есть метод, который создает MinecraftItem из id и imageBytes
    //        // Используем простой пример, где ModName и Type известны или могут быть извлечены
    //        // В реальности, вам нужно будет расширить ваш Extractor, чтобы он возвращал
    //        // полные объекты MinecraftItem, а не только ID и изображения.
    //        // Но для примера, создадим их с заглушками:

    //        foreach (var id in itemIds)
    //        {
    //            if (itemImages.TryGetValue(id, out var imageData))
    //            {
    //                // Здесь вы можете вызвать метод извлекающий LocalizedName, ModName, Type, Tags из других источников
    //                // или расширить ваш LogExtractor, чтобы он возвращал полные MinecraftItem объекты.
    //                // Пока что используем заглушки:
    //                var item = new MinecraftItem
    //                {
    //                    Id = id,
    //                    LocalizedName = id, // Заглушка
    //                    ModName = "Unknown Mod", // Заглушка
    //                    Type = "item", // Заглушка
    //                    Tags = new List<string>(), // Заглушка
    //                    ImageData = imageData
    //                };
    //                allItems.Add(item);
    //            }
    //        }

    //        foreach (var id in fluidIds)
    //        {
    //            if (fluidImages.TryGetValue(id, out var imageData))
    //            {
    //                var fluid = new MinecraftItem
    //                {
    //                    Id = id,
    //                    LocalizedName = id, // Заглушка
    //                    ModName = "Unknown Mod", // Заглушка
    //                    Type = "fluid", // Заглушка
    //                    Tags = new List<string>(), // Заглушка
    //                    ImageData = imageData
    //                };
    //                allItems.Add(fluid);
    //            }
    //        }

    //        // Вызываем метод WinUI окна для обновления предметов
    //        // Это безопасно вызывать из любого потока, так как он использует DispatcherQueue
    //        mainWindow.UpdateItems(allItems); // Вызов метода из WinUIWindow.xaml.cs
    //    }
    //    catch (System.TimeoutException)
    //    {
    //        Console.WriteLine("Ошибка: не удалось подключиться к каналу предметов/жидкостей в течение 30 секунд.");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (System.IO.IOException ex)
    //    {
    //        Console.WriteLine($"Ошибка именованного канала (предметы/жидкости): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (System.Text.Json.JsonException ex)
    //    {
    //        Console.WriteLine($"Ошибка десериализации JSON (предметы/жидкости): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Произошла ошибка (предметы/жидкости): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //}

    //private static async Task ReceiveRecipesAsync(System.Diagnostics.Process process, WinUIWindow mainwindow)
    //{
    //    const string pipeName = "MyMinecraftRecipesPipe";

    //    try
    //    {
    //        using var client = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.In);
    //        await client.ConnectAsync(30000); // Асинхронное подключение
    //        Console.WriteLine("Подключено к именованному каналу рецептов.");

    //        using var reader = new System.IO.StreamReader(client, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: false);
    //        string lengthStr = await reader.ReadLineAsync();
    //        if (!int.TryParse(lengthStr, out int dataLength))
    //        {
    //            Console.WriteLine("Ошибка: не удалось прочитать длину данных рецептов из канала.");
    //            return;
    //        }

    //        var buffer = new byte[dataLength];
    //        int totalRead = 0;
    //        while (totalRead < dataLength)
    //        {
    //            int read = await client.ReadAsync(buffer, totalRead, dataLength - totalRead);
    //            if (read == 0) break;
    //            totalRead += read;
    //        }

    //        if (totalRead != dataLength)
    //        {
    //            Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {dataLength}.");
    //            return;
    //        }

    //        string json = System.Text.Encoding.UTF8.GetString(buffer);
    //        var recipes = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json, new System.Text.Json.JsonSerializerOptions());

    //        if (recipes != null)
    //        {
    //            Console.WriteLine($"Получено {recipes.Count} ID рецептов.");

    //            // Вызов метода формы для обновления данных
    //            //var formInstance = MyForm.GetInstance();
    //            //if (formInstance.InvokeRequired)
    //            //{
    //            //    formInstance.Invoke(new Action(() => formInstance.UpdateRecipes(recipes)));
    //            //}
    //            //else
    //            //{
    //            //    formInstance.UpdateRecipes(recipes);
    //            //}
    //            mainwindow.UpdateRecipes(recipes);
    //        }
    //    }
    //    catch (System.TimeoutException)
    //    {
    //        Console.WriteLine("Ошибка: не удалось подключиться к каналу рецептов в течение 30 секунд.");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (System.IO.IOException ex)
    //    {
    //        Console.WriteLine($"Ошибка именованного канала (рецепты): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (System.Text.Json.JsonException ex)
    //    {
    //        Console.WriteLine($"Ошибка десериализации JSON (рецепты): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Произошла ошибка (рецепты): {ex.Message}");
    //        if (!process.HasExited)
    //        {
    //            try { process.Kill(); }
    //            catch { /* Игнорируем ошибки при завершении */ }
    //        }
    //    }
    //}

    //private static async Task RunItemDemo()
    //{
    //    var extractor = new ItemFluidWithIconExtractor();
    //    if (!extractor.IsValid())
    //    {
    //        Console.WriteLine("Путь к иконкам недействителен.");
    //        return;
    //    }

    //    var data = await extractor.ExtractAsync();
    //    var _itemDatabase = new System.Collections.Generic.Dictionary<string, MinecraftItem>();

    //    // Объединяем Items и Fluids в один словарь
    //    foreach (var item in data.Items)
    //    {
    //        _itemDatabase[item.Id] = item;
    //    }
    //    foreach (var fluid in data.Fluids)
    //    {
    //        _itemDatabase[fluid.Id] = fluid;
    //    }

    //    Console.WriteLine($"Загружено {_itemDatabase.Count} элементов.\n");

    //    Console.WriteLine($"=== Первые 10 элементов из базы данных ===");
    //    int shown = 0;
    //    foreach (var kvp in _itemDatabase)
    //    {
    //        if (shown >= 10) break;

    //        Console.WriteLine($"ID: {kvp.Key}");
    //        Console.WriteLine($"  Название: {kvp.Value.LocalizedName}");
    //        Console.WriteLine($"  Мод: {kvp.Value.ModName}");
    //        Console.WriteLine($"  Тип: {kvp.Value.Type}");
    //        Console.WriteLine($"  Теги: {string.Join(", ", kvp.Value.Tags)}");
    //        Console.WriteLine($"  Размер изображения: {kvp.Value.ImageData.Length} байт");
    //        Console.WriteLine();

    //        shown++;
    //    }

    //    Console.WriteLine("=== Демонстрация поиска ===");
    //    Console.WriteLine("Введите часть названия или ID предмета/жидкости для поиска (или 'exit' для выхода):");

    //    while (true)
    //    {
    //        Console.Write("> ");
    //        string? input = Console.ReadLine()?.Trim();

    //        if (string.IsNullOrEmpty(input) || input!.Equals("exit", StringComparison.OrdinalIgnoreCase))
    //        {
    //            break;
    //        }

    //        var results = FindItemsByName(input, _itemDatabase);

    //        Console.WriteLine($"\n=== Результаты поиска по запросу '{input}' ===");
    //        if (results.Count == 0)
    //        {
    //            Console.WriteLine("Ничего не найдено.");
    //            continue;
    //        }

    //        foreach (var item in results)
    //        {
    //            Console.WriteLine($"ID: {item.Id}");
    //            Console.WriteLine($"  Название: {item.LocalizedName}");
    //            Console.WriteLine($"  Мод: {item.ModName}");
    //            Console.WriteLine($"  Тип: {item.Type}");
    //            Console.WriteLine($"  Теги: {string.Join(", ", item.Tags)}");
    //            Console.WriteLine();
    //        }
    //    }
    //}

    private static System.Collections.Generic.List<MinecraftItem> FindItemsByName(string nameQuery, System.Collections.Generic.Dictionary<string, MinecraftItem> database)
    {
        var results = new System.Collections.Generic.List<MinecraftItem>();
        // Поиск без учёта регистра
        string lowerQuery = nameQuery.ToLowerInvariant();

        foreach (var kvp in database)
        {
            // Проверяем как локализованное название, так и ID
            if (kvp.Value.LocalizedName.ToLowerInvariant().Contains(lowerQuery) ||
                kvp.Key.ToLowerInvariant().Contains(lowerQuery))
            {
                results.Add(kvp.Value);
            }
        }

        return results;
    }

    public static void TestJS()
    {
        Console.WriteLine("=== Тест парсера JS ===\n");

        string testFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test_recipes.js");

        if (!System.IO.File.Exists(testFilePath))
        {
            testFilePath = "test_recipes.js";
        }

        Console.WriteLine($"Путь к файлу: {System.IO.Path.GetFullPath(testFilePath)}");
        Console.WriteLine($"Файл существует: {System.IO.File.Exists(testFilePath)}\n");

        if (!System.IO.File.Exists(testFilePath))
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
                else if (config is CreateFillingConfig filling)
                {
                    Console.WriteLine($"    [Create:Filling] out:{filling.Outputs[0].ItemId} in:{filling.Inputs.Count}");
                }
                else if (config is CreateCrushingConfig crushing)
                {
                    Console.WriteLine($"    [Create:Crushing] out:{crushing.Outputs.Count} in:{crushing.Inputs}");
                }
                else if (config is CreateDeployingConfig deploying)
                {
                    Console.WriteLine($"    [Create:Deploying] out:{deploying.Outputs[0].ItemId} in:{deploying.Inputs.Count}");
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

    private static void TestCreateChains()
    {
        Console.WriteLine("=== Тест: Цепочки модификаторов Create ===\n");

        // 1. Создаём конфиг с модификаторами
        var config = new CreateMixingConfig();
        config.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal" });
        config.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
        config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1, Chance = 0.5f });
        config.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:stick", Count = 1, Chance = 0.5f });

        config.Modifiers.Heat = CreateHeatType.Superheated;

        Console.WriteLine("1. Исходный конфиг:");
        Console.WriteLine($"   Inputs: {config.Inputs.Count}, Outputs: {config.Outputs.Count}");
        Console.WriteLine($"   Heat: {config.Modifiers.Heat}, Chance: {config.Outputs[0].Chance}");
        Console.WriteLine();

        // 2. Генерация в JS
        string generatedCode = config.GenerateJsCode();
        Console.WriteLine("2. Сгенерированный код:");
        Console.WriteLine(generatedCode);
        Console.WriteLine();

        // 3. Сохраняем во временный файл
        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "test_create_chain.js");
        string fullJs = $"ServerEvents.recipes(event => {{\n    {generatedCode}\n}});";
        System.IO.File.WriteAllText(tempFile, fullJs);
        Console.WriteLine($"3. Временный файл: {tempFile}");
        Console.WriteLine();

        // 4. Парсим обратно
        var parser = new JsRecipeParser();
        var configs = parser.ParseFile(tempFile);
        Console.WriteLine($"4. Распознано элементов: {configs.Count}");

        if (configs.Count > 0 && configs[0] is CreateMixingConfig parsedConfig)
        {
            Console.WriteLine("5. Распознанный конфиг:");
            Console.WriteLine($"   Inputs: {parsedConfig.Inputs.Count}, Outputs: {parsedConfig.Outputs.Count}");
            Console.WriteLine($"   Heat: {parsedConfig.Modifiers.Heat}, Chance: {parsedConfig.Outputs[0].Chance}");

            // Проверка соответствия
            bool matches = config.Inputs.Count == parsedConfig.Inputs.Count &&
                          config.Outputs.Count == parsedConfig.Outputs.Count &&
                          config.Modifiers.Heat == parsedConfig.Modifiers.Heat &&
                          config.Outputs[0].Chance == parsedConfig.Outputs[0].Chance;

            Console.WriteLine($"   Соответствие: {(matches ? "✅ OK" : "❌ FAIL")}");
        }
        else
        {
            Console.WriteLine("5. ❌ Не удалось распознать CreateMixingConfig");
            if (configs.Count > 0)
            {
                Console.WriteLine($"   Фактический тип: {configs[0].GetType().Name}");
            }
        }

        // 6. Очистка
        try { System.IO.File.Delete(tempFile); } catch { }
        Console.WriteLine("\n=== Тест завершён ===");
    }

    private static void GenCreateRecipes()
    {
        Console.WriteLine("=== Примеры Create-рецептов ===\n");

        // 1. Compacting
        var compacting = new CreateCompactingConfig();
        compacting.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
        compacting.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
        compacting.Modifiers.Heat = CreateHeatType.Heated;
        Console.WriteLine("// Compacting (с нагревом)");
        Console.WriteLine(compacting.GenerateJsCode());
        Console.WriteLine();

        // 2. Deploying
        var deploying = new CreateDeployingConfig();
        deploying.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
        deploying.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:sand" });
        deploying.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
        deploying.Modifiers.KeepHeldItem = true;
        Console.WriteLine("// Deploying (с сохранением предмета)");
        Console.WriteLine(deploying.GenerateJsCode());
        Console.WriteLine();

        // 3. Emptying
        var emptying = new CreateEmptyingConfig();
        emptying.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:water_bucket" });
        emptying.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:bucket", Count = 1 });
        emptying.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water", Count = 1000 });
        Console.WriteLine("// Emptying (опустошение)");
        Console.WriteLine(emptying.GenerateJsCode());
        Console.WriteLine();

        // 4. Filling
        var filling = new CreateFillingConfig();
        filling.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:bucket" });
        filling.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
        filling.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:water_bucket", Count = 1 });
        Console.WriteLine("// Filling (наполнение)");
        Console.WriteLine(filling.GenerateJsCode());
        Console.WriteLine();

        // 5. Crushing
        var crushing = new CreateCrushingConfig();
        crushing.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
        crushing.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
        crushing.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:emerald", Count = 1, Chance = 0.5f });
        crushing.Modifiers.ProcessingTime = 500;
        Console.WriteLine("// Crushing (дробление с шансами и временем)");
        Console.WriteLine(crushing.GenerateJsCode());
        Console.WriteLine();

        // 6. Mixing
        var mixing = new CreateMixingConfig();
        mixing.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal" });
        mixing.Inputs.Add(new CreateFluidIngredientComponent { FluidId = "minecraft:water", Amount = 1000 });
        mixing.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1 });
        mixing.Modifiers.Heat = CreateHeatType.Superheated;
        Console.WriteLine("// Mixing (смешивание с флюидами и нагревом)");
        Console.WriteLine(mixing.GenerateJsCode());
        Console.WriteLine();

        // 7. Sandpaper Polishing
        var sandpaper = new CreateSandpaperPolishingConfig();
        sandpaper.Inputs.Add(new CreateItemIngredientComponent { ItemId = "minecraft:coal_block" });
        sandpaper.Outputs.Add(new CreateItemComponent { ItemId = "minecraft:diamond", Count = 1, Chance = 0.7f });
        Console.WriteLine("// Sandpaper Polishing (с шансом)");
        Console.WriteLine(sandpaper.GenerateJsCode());
        Console.WriteLine();

        Console.WriteLine("=== Конец примеров ===");
    }
}

// --- Основной метод запуска WinUI ---
public partial class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Инициализация WinUI ComWrappers (обязательно до первого вызова WinUI)
        WinRT.ComWrappersSupport.InitializeComWrappers();

        // Запуск WinUI Application
        Microsoft.UI.Xaml.Application.Start((p) => new App());
    }
}
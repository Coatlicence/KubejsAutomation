using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes; // Добавлено для NamedPipeClientStream
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json; // Добавлено для десериализации
using KubeScriptAutomation.CodeGeneratorStrategies;
using KubeScriptAutomation;
using KubeScriptAutomation.Collectos;
using KubeAutomation;
using System.Windows.Forms;

class Program
{
    static List<string> Items = new List<string>();
    static List<string> Recipes = new List<string>();
    static Dictionary<string, byte[]> ItemImages = new Dictionary<string, byte[]>();
    static Dictionary<string, byte[]> FluidImages = new Dictionary<string, byte[]>();

    static MyForm form = new MyForm();

    [STAThread]
    static async Task Main(string[] args)
    {
        // Запускаем форму в отдельном потоке
        Thread formThread = new Thread(() =>
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
        var codeGenerator = new GregTechRecipeCodeGenerator();
        var fileSaver = new RecipeFileSaver();

        while (true)
        {
            Console.WriteLine("\nХотите создать новый рецепт? (да/нет/получить)");

            string res = Console.ReadLine()?.Trim().ToLower();

            if (res == "получить" || res == "п" || res == "g" || res == "get")
            {
                //_ = StartExternalAppAsync(); // Запускает задачу в фоне, не блокируя поток
            }

            if (res == "н" || res == "нет" || res == "n" || res == "no")
                break;

            try
            {
                var config = inputCollector.Collect();
                string script = codeGenerator.Generate(config);
                fileSaver.Save(script, config);

                Console.WriteLine("\nРецепт успешно создан!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании рецепта: {ex.Message}");
            }
        }

        Console.WriteLine("Программа завершена.");
    }

    // Асинхронная функция для запуска внешнего приложения и получения данных из именного канала
    public static async Task StartExternalAppAsync(MyForm form)
    {
        //const string pipeName = "MyMinecraftItemsPipe"; // Не используется напрямую теперь
        //string logPath = "C:\\Users\\nojda\\AppData\\Roaming\\.tlauncher\\legacy\\Minecraft\\game\\logs\\debug.log"; // Не нужно

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = @"C:\Users\nojda\source\repos\LogExtractor\LogExtractor\bin\Debug\net8.0\LogExtractor.exe",
            Arguments = "all", // Теперь передаём "all", чтобы получить всё
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

            // Объединяем ID и передаём в UI
            var allIds = new List<string>();
            allIds.AddRange(itemIds);
            allIds.AddRange(fluidIds);

            // Сохраняем изображения (если нужно использовать в UI)
            ItemImages = itemImages;
            FluidImages = fluidImages;

            Items = allIds;
            form?.UpdateItemsAndImages(Items, ItemImages, FluidImages);
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
                Recipes = recipes;
                Console.WriteLine($"Получено {recipes.Count} ID рецептов.");

                form?.UpdateRecipes(Recipes);
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

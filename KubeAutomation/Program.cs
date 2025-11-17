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
    public static List<string> Items = [];

    [STAThread]
    static async Task Main(string[] args)
    {
        // Запускаем форму в отдельном потоке
        Thread formThread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyForm());
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
        const string pipeName = "MyMinecraftItemsPipe";

        string logPath = "C:\\Users\\nojda\\AppData\\Roaming\\.tlauncher\\legacy\\Minecraft\\game\\logs\\debug.log";

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = @"C:\Users\nojda\source\repos\LogExtractor\LogExtractor\bin\Debug\net8.0\LogExtractor.exe",
            Arguments = $"\"{logPath}\"",
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

        Console.WriteLine("Внешнее приложение запущено. Ожидание подключения к каналу...");

        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
            // Устанавливаем таймаут подключения (например, 30 секунд)
            await client.ConnectAsync(30000); // Асинхронное подключение
            Console.WriteLine("Подключено к именованному каналу.");

            using var reader = new StreamReader(client, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: false);
            string lengthStr = await reader.ReadLineAsync();
            if (!int.TryParse(lengthStr, out int dataLength))
            {
                Console.WriteLine("Ошибка: не удалось прочитать длину данных из канала.");
                return;
            }

            Console.WriteLine($"Ожидаемый размер данных: {dataLength} байт");

            var buffer = new byte[dataLength];
            int totalRead = 0;
            while (totalRead < dataLength)
            {
                int read = await client.ReadAsync(buffer, totalRead, dataLength - totalRead);

                //await Task.Delay(5000); // симулируем долгую работу.

                if (read == 0)
                    break; // Конец потока
                totalRead += read;
            }

            if (totalRead != dataLength)
            {
                Console.WriteLine($"Ошибка: прочитано {totalRead} байт, ожидалось {dataLength}.");
                return;
            }

            string json = System.Text.Encoding.UTF8.GetString(buffer);
            Console.WriteLine("Данные получены. Десериализация...");

            var items = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions());

            if (items != null)
            {
                Items = items;

                form?.UpdateAutoCompleteSource(items);
            }

            
            //ProcessItems(Items);

            Console.WriteLine($"\n\nВсе данные получены из именного канала. Найдено {Items.Count} элементов типа minecraft:item.\n\n");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Ошибка: не удалось подключиться к именованному каналу в течение 30 секунд.");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка именованного канала: {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка десериализации JSON: {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
            if (!process.HasExited)
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки при завершении */ }
            }
        }
    }

    static async void ProcessItems(List<string> items)
    {
        // Делайте что-то с items
        foreach (var item in items)
        {
            await Task.Delay(1);
            Console.WriteLine($"Элемент: {item}");
        }
    }
}

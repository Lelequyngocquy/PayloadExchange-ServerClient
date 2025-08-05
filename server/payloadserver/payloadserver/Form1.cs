using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace payloadserver
{
    public partial class Form1 : Form
    {
        private TcpListener server;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            server = new TcpListener(IPAddress.Any, 12345);
            server.Start();
            Log("✅ Server is listening on port 12345...");
            Task.Run(() => ListenForClients());
        }

        private async Task ListenForClients()
        {
            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                Task.Run(() => HandleClient(client));
            }
        }
        private void Log(string message)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string>(Log), message);
                    return;
                }

                textBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Log($"❗ Error while handling client: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client)
        {
            IPEndPoint remoteIp = (IPEndPoint)client.Client.RemoteEndPoint;
            Log($"🔗 Client connected from {remoteIp.Address}:{remoteIp.Port}");
            AddClientToTable(remoteIp);
            NetworkStream stream = null;
            BinaryWriter writer = null;

            try
            {
                stream = client.GetStream();
                writer = new BinaryWriter(stream);

                string payloadPath = "PayloadApp.zip";

                if (!File.Exists(payloadPath))
                {
                    Log("❌ Payload is not exist, send 0");
                    writer.Write(0); // Gửi payload size = 0
                    writer.Flush();
                    return;
                }

                byte[] payloadBytes = File.ReadAllBytes(payloadPath);
                int payloadSize = payloadBytes.Length;

                Log($"📦 Prepare to send PayloadApp ({payloadSize} bytes)...");
                writer.Write(payloadSize);
                writer.Flush();
                Log("✅ Sent length of payload");

                writer.Write(payloadBytes);
                writer.Flush();
                Log("✅ Sent payload");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi mà không gọi lại Log(...) trong Log()
                try { Log($"❗ Error while sending PayloadApp: {ex.Message}"); } catch { }
            }
            finally
            {
                // Sau khi gửi payload
                Log("✅ Sent payload");

                // Giữ kết nối mở để đợi tín hiệu từ client
                StreamReader reader = new StreamReader(stream);
                string signal = reader.ReadLine();  // client sẽ gửi "done\n"
                Log($"📨 Received signal from client: {signal}");

                try { 
                    Log($"❌ Close connection with {remoteIp.Address}:{remoteIp.Port}");
                    Log($".");
                    Log($"===========================");
                    RemoveClientFromTable(remoteIp);
                }
                catch { }

            }
        }

        private void AddClientToTable(IPEndPoint clientEndPoint)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddClientToTable(clientEndPoint)));
                return;
            }

            dataGridView1.Rows.Add(
                clientEndPoint.Address.ToString(),
                clientEndPoint.Port.ToString(),
                DateTime.Now.ToString("HH:mm:ss")
            );
        }

        private void RemoveClientFromTable(IPEndPoint clientEndPoint)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RemoveClientFromTable(clientEndPoint)));
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["dataGridViewTextBoxColumn1"].Value.ToString() == clientEndPoint.Address.ToString()
                    && row.Cells["dataGridViewTextBoxColumn2"].Value.ToString() == clientEndPoint.Port.ToString())
                {
                    dataGridView1.Rows.Remove(row);
                    break;
                }
            }
        }

    }
}

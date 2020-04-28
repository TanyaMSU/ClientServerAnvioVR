using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WindowsFormsClient
{
    public partial class Form1 : Form
    {
        int port = 8005; // server port
        string address = "127.0.0.1"; // server address
        List<int> IDs;
        string filename;

        public Form1()
        {
            InitializeComponent();
            IDs = new List<int>();

            try
            {
                // Путь .\\Log
                string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                filename = Path.Combine(pathToLog, string.Format("{0}_{1:dd.MM.yyy}.log",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Log("GET PROCESSES button was clicked\n");

                //IP point = ip address + port
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                //create socket instance
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // connect to remote host
                socket.Connect(ipPoint);

                string message = "processes";
                byte[] data = Encoding.Unicode.GetBytes(message);

                //send message as byte array
                socket.Send(data);

                data = new byte[256]; // буфер для ответа
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                do //get response
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);

                string[] processes = builder.ToString().Split('\n');

                textBox1.Clear();
                foreach (string oneProcess in processes)
                {
                    if (oneProcess.Length > 0)
                    {
                        textBox1.AppendText(oneProcess);
                        textBox1.AppendText(Environment.NewLine);
                        string[] parts = oneProcess.Split(' ');
                        IDs.Add(Int32.Parse(parts[parts.Length - 1]));
                    }

                }
                Log("Response was obtained\n");

                // закрываем сокет
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                //textBox1.Clear();
                textBox1.AppendText("something went wrong");
                Log($"Exception: {ex.Message}\n");

            }
            Console.Read();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Log("KILL button was clicked\n");
                if (!Int32.TryParse(textBox2.Text, out int processToKill))
                {
                    processToKill = -1;
                }
                if (IDs.Contains(processToKill))
                {
                    //IP point = ip address + port
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                    //create socket instance
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    // connect to remote host
                    socket.Connect(ipPoint);

                    string message = "kill " + processToKill;
                    byte[] data = Encoding.Unicode.GetBytes(message);

                    //send message as byte array
                    socket.Send(data);

                    data = new byte[256]; // буфер для ответа
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байт

                    do
                    {
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);

                    textBox2.Clear();
                    textBox2.AppendText(builder.ToString());

                    Log("Response was obtained\n");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                }
                else
                {
                    textBox2.Clear();
                    textBox2.AppendText("Wrong ID");
                    Log("Wrong ID was given\n");
                }
            }
            catch
            {

            }
        }

        public void Log(string message)
        {
            File.AppendAllText(filename, message);
        }
    }
}

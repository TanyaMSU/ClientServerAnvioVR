using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        TcpListener myServer;
        Thread myServerThread;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int port = 8005;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            myServer = new TcpListener(ipPoint);
            myServer.Start();

            myServerThread = new Thread(new ThreadStart(ServerThreadStart));
            myServerThread.Start();
        }

        protected override void OnStop()
        {

            if (myServer != null)
            {
                myServer.Stop();

                // Wait for one second for the the thread to stop.
                myServerThread.Join(1000);

                // If still alive; Get rid of the thread.
                if (myServerThread.IsAlive)
                {
                    myServerThread.Abort();
                }
                myServerThread = null;

                //free server object
                myServer = null;
            }

        }


        private void ServerThreadStart()
        {
            Socket clientSocket;
            bool stopServer = false;

            while (!stopServer)
            {
                try
                {
                    // Wait for any client requests and if there is any 
                    // request from any client accept it
                    clientSocket = myServer.AcceptSocket();

                    StringBuilder builder = new StringBuilder(); //for message received
                    int bytes = 0; // number of bytes obtained
                    byte[] data = new byte[256]; // receive buffer

                    do
                    {
                        bytes = clientSocket.Receive(data); // receive data from socket
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));//put data array to builder
                    }
                    while (clientSocket.Available > 0); //determine whether data is queued for reading

                    StringBuilder messageBuilder = new StringBuilder();
                    string message;

                    //obtained message should be
                    //"processes" - to get names and IDs of all processes
                    //"kill *process ID*" - to kill the process with the given ID
                    if (builder.ToString().ToLower() == "processes")
                    {
                        Process[] localAll = Process.GetProcesses();
                        foreach (Process oneProcess in localAll)
                        {
                            messageBuilder.Append(oneProcess.ProcessName);
                            messageBuilder.Append(" ");
                            messageBuilder.Append(oneProcess.Id);
                            messageBuilder.Append("\n");
                        }
                        message = messageBuilder.ToString();
                    }
                    else if (builder.ToString().ToLower().StartsWith("kill "))
                    {
                        try
                        {
                            Process processToKill = Process.GetProcessById(Int32.Parse(builder.ToString().Substring(4)));
                            processToKill.Kill();
                            message = "Process killed";
                        }
                        catch
                        {
                            message = "Message not identified";
                        }
                    }
                    else
                    {
                        message = "Message not identified";
                    }

                    //convert respond message into byte array
                    data = Encoding.Unicode.GetBytes(message);

                    //send to client
                    clientSocket.Send(data);

                    // close socket
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch (SocketException)
                {
                    stopServer = true;
                }

            }
        }


    }
}

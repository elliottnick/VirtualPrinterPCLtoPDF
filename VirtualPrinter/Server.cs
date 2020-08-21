using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VirtualPrinter
{
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private int i = 0; // this is for naming files later

        public Server(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                // Waits for client to connect to server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // Thread to handle communication
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (File.Exists(i + ".pcl")) i += 1;
            while (true)
            {


                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (Exception e)
                {
                    //a socket error has occured
                    Debug.WriteLine(e.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Debug.WriteLine("disconnected");
                    break;
                }

                // message segment has successfully been received, do what you need to with it
                AppendAllBytes(i + ".pcl", message);

            }


            // All the data is finished sending, conversion/saving/printing should take place here

            tcpClient.Close();
            PCLtoFile(i + ".pcl", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "-" + i + ".pdf");
        }
        public static void AppendAllBytes(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void PCLtoFile(string inputFile, string outputFile)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Users\nelliott\source\repos\VirtualPrinter\VirtualPrinter\gspcl\gpcl6win32.exe"; // StaticProperties is the name of my class with a string to the location of the GhostPCL executable
            startInfo.Arguments = "-dNOPAUSE -sDEVICE=pdfwrite -r300 -sOutputFile=" + outputFile + " " + inputFile;
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
        }

    }
}

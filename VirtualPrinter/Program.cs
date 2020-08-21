using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VirtualPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            new Thread(() => { Server server = new Server(9100); }).Start();

        }
    }
}

using Server.Model;
using System;
using System.Threading;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServerModel server = new ServerModel();

            Thread thread = new Thread(server.Listen);
            thread.Start();

            Console.Read();
        }
    }
}

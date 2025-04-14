using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

namespace Client
{
    class Program
    {

        static TcpClient? Cliente;
        static string HostName = "127.0.0.1";

        static void Main(string[] args)
        {
            // Creamos el cliente
            Cliente = new TcpClient();

            try
            {
                // Establecemos la conexión con el server
                Cliente.Connect(HostName, 10001);
                if(Cliente.Connected) Console.WriteLine("Cliente: Conectado");
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            }
        }

    }
}
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using Conexion;

namespace servidor
{

    class Program
    {

        static TcpListener? Servidor;
        static string HostName = "127.0.0.1";

        static void Main(string[] args)
        {
            // Creamos e iniciamos el server
            Servidor = new TcpListener(IPAddress.Parse(HostName), 10001);
            Servidor.Start();
            Console.WriteLine("Server: Servidor iniciado");

            // Acceptamos nuevas conexiones
            while (true)
            {
                TcpClient Cliente = Servidor.AcceptTcpClient();

                Thread TCliente = new Thread(GestionarCliente!);
                TCliente.Start(Cliente);
            }
        }

        static void GestionarCliente (object c) 
        {
            TcpClient Cliente = (TcpClient)c;

            if (Cliente.Connected) Console.WriteLine("Servidor: Gestionando nuevo vehículo…");
        }

    }
}
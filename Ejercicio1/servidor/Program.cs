using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using Conexion;
using NetworkStreamNS;

namespace servidor
{

    class Program
    {

        static TcpListener? Servidor;
        static string HostName = "127.0.0.1";
        static int Id = 1;
        static readonly object locker = new object();
        static Random rnd = new Random();
        static readonly string[] Direcciones = ["norte", "sur"];

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

            if (Cliente.Connected)
            {
                int IdCliente;
                lock (locker)
                {
                    IdCliente = Id++;
                }
                string DirCliente = Direcciones[rnd.Next(0,Direcciones.Length)];

                Console.WriteLine("Servidor: Gestionando vehículo ID - {0}, Direccion - {1}", IdCliente, DirCliente);

                // Obtenemos NetworkStream
                NetworkStream NS = Cliente.GetStream();
            }
        }

    }
}
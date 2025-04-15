using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using NetworkStreamNS;

namespace Client
{
    class Program
    {

        static TcpClient? Cliente;
        static readonly string HostName = "127.0.0.1";
        static NetworkStream? NS;
        static int Id;

        static void Main(string[] args)
        {
            // Creamos el cliente
            Cliente = new TcpClient();

            try
            {
                // Establecemos la conexión con el server
                Cliente.Connect(HostName, 10001);
                if(Cliente.Connected) 
                {
                    // Obtenemos el NetworkStream
                    NS = Cliente.GetStream();

                    // Establecer el Handshake
                    NetworkStreamClass.EscribirMensajeNetworkStream(NS, "INICIO");
                    Id = int.Parse(NetworkStreamClass.LeerMensajeNetworkStream(NS));
                    NetworkStreamClass.EscribirMensajeNetworkStream(NS, Id.ToString());

                    Console.WriteLine("Cliente: Conectado");
                }
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            }
            Console.ReadLine();
        }

    }
}
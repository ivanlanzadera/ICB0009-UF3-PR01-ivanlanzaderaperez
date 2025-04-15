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
            Console.WriteLine("Servidor iniciado. Escuchando nuevas conexiones...");

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

            try
            {
                if (Cliente.Connected)
                {
                    // Obtenemos NetworkStream
                    NetworkStream NS = Cliente.GetStream();
                    int IdCliente;
                    string DirCliente;

                    // Establecemos Handshake
                    if (NetworkStreamClass.LeerMensajeNetworkStream(NS) == "INICIO")
                    {
                        lock (locker)
                        {
                            IdCliente = Id++;
                        }
                        DirCliente = Direcciones[rnd.Next(0,Direcciones.Length)];

                        NetworkStreamClass.EscribirMensajeNetworkStream(NS, IdCliente.ToString());
                        string RespuestaHandShake = NetworkStreamClass.LeerMensajeNetworkStream(NS);
                        if (RespuestaHandShake != IdCliente.ToString()) 
                            throw new Exception("El cliente "+IdCliente+" no ha completado el handshake como se esperaba. Cerrando conexión...");
                        
                        Console.WriteLine("El handshake del cliente {0} ha sido exitoso!", IdCliente);
                    } else
                    {
                        throw new Exception("El cliente ha iniciado el handshake erróneamente. Cerrando conexión...");
                    }
                    
                }
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
                Cliente.Close();
            }
        }

    }
}
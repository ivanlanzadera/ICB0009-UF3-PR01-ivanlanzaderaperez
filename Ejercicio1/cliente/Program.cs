using System.Net.Sockets;
using NetworkStreamNS;

namespace cliente
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

                    string msg;
                    do 
                    {
                        msg = Console.ReadLine()!;
                        NetworkStreamClass.EscribirMensajeNetworkStream(NS, msg);
                        string respuesta = NetworkStreamClass.LeerMensajeNetworkStream(NS);
                        if (msg != "quit") Console.WriteLine("{0}", respuesta);
                    } while(msg != "quit");
                }
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            }
        }

    }
}
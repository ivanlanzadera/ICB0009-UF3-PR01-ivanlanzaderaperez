using System.Net.Sockets;
using CarreteraClass;
using NetworkStreamNS;
using VehiculoClass;

namespace cliente
{
    class Program
    {

        static TcpClient? Cliente;
        static readonly string HostName = "127.0.0.1";
        static NetworkStream? NS;
        static int Id;
        static Vehiculo? vehiculo;

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

                    vehiculo = new Vehiculo();
                    vehiculo.Id = Id;

                    // Pasamos datos del vehiculo al server
                    NetworkStreamClass.EscribirDatosVehiculoNS(NS, vehiculo);

                    // Recogemos los datos de la carretera
                    Carretera c = NetworkStreamClass.LeerDatosCarreteraNS(NS);

                    foreach (Vehiculo v in c.VehiculosEnCarretera)
                    {
                        Console.WriteLine("Vehiculo circulando - ID {0}", v.Id);
                    }

                    // string msg;
                    // do 
                    // {
                    //     msg = Console.ReadLine()!;
                    //     NetworkStreamClass.EscribirMensajeNetworkStream(NS, msg);
                    //     string respuesta = NetworkStreamClass.LeerMensajeNetworkStream(NS);
                    //     if (msg != "quit") Console.WriteLine("{0}", respuesta);
                    // } while(msg != "quit");
                }
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            }
        }

    }
}
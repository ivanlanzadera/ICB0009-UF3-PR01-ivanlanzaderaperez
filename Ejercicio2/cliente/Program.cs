using System.Data.Common;
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
        static Vehiculo? v;

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
                    int Id = Handshake(NS);

                    // Recogemos la dirección que nos indica el servidor
                    string Dir = NetworkStreamClass.LeerMensajeNetworkStream(NS);

                    Task TareaCarretera = new(EscuchasCarretera);
                    TareaCarretera.Start();

                    OperarVehiculo(NS, Id, Dir);
                }

            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            }
            Console.ReadLine();
        }

        private static int Handshake(NetworkStream NS)
        {
            NetworkStreamClass.EscribirMensajeNetworkStream(NS, "INICIO");
            Id = int.Parse(NetworkStreamClass.LeerMensajeNetworkStream(NS));
            NetworkStreamClass.EscribirMensajeNetworkStream(NS, Id.ToString());

            Console.WriteLine("Cliente: Conectado");
            return Id;
        }

        private static void OperarVehiculo(NetworkStream NS, int Id, string Dir)
        {
            v = new Vehiculo();
            v.Id = Id;
            v.Direccion = Dir;
            if (v.Direccion == "norte") 
            {
                v.Pos = 100;
                for (int i = 100; i > 0; i--)
                {
                    Thread.Sleep(v.Velocidad);
                    v.Pos--;
                    NetworkStreamClass.EscribirDatosVehiculoNS(NS, v);
                }
            } else 
            {
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(v.Velocidad);
                    v.Pos++;
                    NetworkStreamClass.EscribirDatosVehiculoNS(NS, v);
                }
            }

            v.Acabado = true;
        }

        private static void EscuchasCarretera()
        {
            Carretera c;

            do
            {
                c = NetworkStreamClass.LeerDatosCarreteraNS(NS);
                Console.WriteLine("\n\n\n### MOSTRANDO ESTADO DE LOS VEHÍCULOS ###\n");
                foreach (Vehiculo v in c.VehiculosEnCarretera)
                {
                    Console.Write("[{0}]\t Vehículo #{1}: ", v.Direccion, v.Id);
                    for (int i = 0; i<100; i += 2)
                    {
                        if (v.Direccion == "sur")
                        {
                            if (i<v.Pos) Console.Write("+");
                            else Console.Write("-");
                        } else 
                        {
                            if (i<v.Pos) Console.Write("-");
                            else Console.Write("+");
                        }
                    }
                    Console.Write(" (Km {0} - ", v.Pos);
                    if (v.Parado) Console.WriteLine("Esperando)");
                    else Console.WriteLine("Cruzando)");
                }

            } while (true);
        }

    }
}
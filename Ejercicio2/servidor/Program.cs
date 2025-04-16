using System.Net.Sockets;
using System.Net;
using Conexion;
using NetworkStreamNS;
using VehiculoClass;
using CarreteraClass;

namespace servidor
{

    class Program
    {

        static TcpListener? Servidor;
        static readonly string HostName = "127.0.0.1";
        static int Id = 1;
        static readonly object locker = new ();
        static readonly Random rnd = new ();
        static readonly string[] Direcciones = ["norte", "sur"];
        static List<Cliente> clientes = new();

        #pragma warning disable CS8618
        static Carretera carretera;
        #pragma warning restore CS8618

        static void Main(string[] args)
        {
            // Creamos e iniciamos el server
            Servidor = new TcpListener(IPAddress.Parse(HostName), 10001);
            Servidor.Start();
            Console.WriteLine("Servidor iniciado. Escuchando nuevas conexiones...");
            carretera = new Carretera();

            // Acceptamos nuevas conexiones
            while (true)
            {
                TcpClient Cliente = Servidor.AcceptTcpClient();

                Thread TCliente = new (GestionarCliente!);
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

                    // Establecemos Handshake
                    Cliente conexion = Handshake(NS, Cliente);

                    // Recibimos el vehículo generado por el cliente
                    Vehiculo v = RecibirVehiculo(NS, conexion);
                    MostrarVehiculos();

                    // Gestionamos el movimiento del vehículo y la comunicación con los clientes
                    ActualizarVehiculo(NS, conexion, v);
                }

            } catch (DesconexionClienteException e)
            { // Manejo de desconexión tras haber realizado el handshake
                Console.WriteLine(e.Message);
                if (e.Vehiculo != null)
                {
                    lock(locker)
                    {
                        carretera.VehiculosEnCarretera.Remove(e.Vehiculo);
                    }
                }
                Cliente.Close();
                lock (locker)
                {
                    clientes.Remove(e.Cliente);
                }
                Console.WriteLine("Conexión eliminada - Total conexiones: {0}", clientes.Count);
            } catch (Exception e)
            { // Manejo de errores generales y de handshake erróneo
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
                Cliente.Close();
            }
        }

        private static Cliente Handshake (NetworkStream NS, TcpClient Cliente)
        {

            if (NetworkStreamClass.LeerMensajeNetworkStream(NS) == "INICIO")
            {
                int IdCliente;
                lock (locker)
                {
                    IdCliente = Id++;
                }

                NetworkStreamClass.EscribirMensajeNetworkStream(NS, IdCliente.ToString());
                string RespuestaHandShake = NetworkStreamClass.LeerMensajeNetworkStream(NS);
                if (RespuestaHandShake != IdCliente.ToString()) 
                    throw new Exception("El cliente "+IdCliente+" no ha completado el handshake como se esperaba. Cerrando conexión...");
                
                // Handshake exitoso, conservamos la conexion
                var conexion = new Cliente(IdCliente, NS);
                
                lock (locker)
                {
                    clientes.Add(conexion);
                }
                return conexion;
            } else
            {
                throw new Exception("El cliente ha iniciado el handshake erróneamente. Cerrando conexión...");
            }

        }

        private static Vehiculo RecibirVehiculo (NetworkStream NS, Cliente conexion)
        {
            string DirCliente = Direcciones[rnd.Next(0,Direcciones.Length)];
            NetworkStreamClass.EscribirMensajeNetworkStream(NS, DirCliente);

            // Recibimos el vehículo que ha creado el cliente
            Vehiculo v = NetworkStreamClass.LeerDatosVehiculoNS(NS) ?? throw new DesconexionClienteException(conexion, null, 
                "El cliente se ha desconectado de forma abrupta. Liberando recursos...");
            lock (locker)
            {
                carretera.AñadirVehiculo(v);
            }
            return v;
        }

        private static void ActualizarVehiculo(NetworkStream NS, Cliente conexion, Vehiculo v)
        {
            Vehiculo VActual;
            do
            {   // Recibimos la actualización del vehículo y lo registramos en la carretera
                VActual = NetworkStreamClass.LeerDatosVehiculoNS(NS) ?? throw new DesconexionClienteException(conexion, v, 
                    "El cliente se ha desconectado de forma abrupta. Liberando recursos...");
                
                lock(locker)
                {
                    carretera.ActualizarVehiculo( VActual );
                }
                MostrarVehiculos();

                // Enviamos los datos de la carretera a todos los clientes
                foreach (Cliente cli in clientes)
                {
                    NetworkStreamClass.EscribirDatosCarreteraNS(cli.NS, carretera);
                }
            } while (!VActual.Acabado);
        }

        private static void MostrarVehiculos()
        {
            Console.WriteLine("\n\n\n### MOSTRANDO ESTADO DE LOS VEHÍCULOS ###\n");
            foreach (Vehiculo v in carretera.VehiculosEnCarretera)
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
        }

    }
}
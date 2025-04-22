using System.Net.Sockets;
using System.Net;
using Conexion;
using NetworkStreamNS;
using VehiculoClass;
using CarreteraClass;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
        static Vehiculo? VehiculoPuente = null;
        static ConcurrentQueue<Vehiculo> ColaPuente = new();
        static SemaphoreSlim SemPuente = new (1);

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
            NetworkStream? NS = null;
            Cliente? conexion = null;

            try
            {

                if (Cliente.Connected)
                {
                    // Obtenemos NetworkStream
                    NS = Cliente.GetStream();

                    // Establecemos Handshake
                    conexion = Handshake(NS, Cliente);

                    // Recibimos el vehículo generado por el cliente
                    Vehiculo v = RecibirVehiculo(NS, conexion);
                    // MostrarVehiculos();

                    // Gestionamos el movimiento del vehículo y la comunicación con los clientes
                    ActualizarVehiculo(NS, conexion, v);
                }

            } catch (DesconexionClienteException e)
            { // Manejo de desconexión tras haber realizado el handshake
                Console.WriteLine(e.Message);
                if (e.Vehiculo != null && !e.Vehiculo.Acabado)
                {
                    lock(locker)
                    {
                        for (int i = carretera.VehiculosEnCarretera.Count - 1; i >= 0; i--)
                        {
                            if (carretera.VehiculosEnCarretera[i].Id == e.Vehiculo.Id)
                            {
                                carretera.VehiculosEnCarretera.RemoveAt(i);
                            }
                        }
                        carretera.NumVehiculosEnCarrera--;

                        if (VehiculoPuente?.Id == e.Vehiculo.Id) VehiculoPuente = null;
                    }
                }
                lock (locker)
                {
                    clientes.Remove(e.Cliente);
                }
                Console.WriteLine("Conexión eliminada - Total conexiones: {0}", clientes.Count);
            } catch (Exception e)
            { // Manejo de errores generales y de handshake erróneo
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
            } finally
            {
                try { NS?.Close(); } catch {}
                try { Cliente.Close(); } catch {}
                lock (locker)
                {
                    if (conexion != null) clientes.Remove(conexion);
                }
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
            Vehiculo v = NetworkStreamClass.LeerDatosVehiculoNS(conexion) ?? throw new DesconexionClienteException(conexion, null, 
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
                VActual = NetworkStreamClass.LeerDatosVehiculoNS(conexion) ?? throw new DesconexionClienteException(conexion, v,
                    "El cliente se ha desconectado de forma abrupta. Liberando recursos...");
                
                lock(locker)
                {
                    carretera.ActualizarVehiculo( VActual! );
                }
                // MostrarVehiculos();

                // Evaluar si el vehículo debe pasar
                if ((VActual!.Direccion == "sur" && VActual.Pos == 30) || (VActual.Direccion == "norte" && VActual.Pos == 50))
                {
                    // Añadir el vehiculo en la cola de espera
                    ColaPuente.Enqueue(VActual);
                    bool avanza = false;
                    do
                    {
                        lock (locker)
                        {
                            ColaPuente.TryPeek(out Vehiculo? VEspera);
                            if (carretera.VehiculoPuente == null && VEspera?.Id == VActual.Id ) 
                            {
                                ColaPuente.TryDequeue(out _);
                                avanza = true;
                                VActual.Parado = false;
                                carretera.VehiculoPuente = VActual;
                                Console.WriteLine($"El vehiculo #{VActual.Id} ha accedido al tunel...");
                            } else
                            {
                                Console.WriteLine($"El vehiculo #{VActual.Id} está solicitando entrar al tunel...");
                                Thread.Sleep(500);
                            }
                        }
                    } while (!avanza);
                }


                // Salimos del puente
                if ((VActual!.Direccion == "sur" && VActual.Pos == 50) || (VActual.Direccion == "norte" && VActual.Pos == 30))
                {
                    lock (locker) { carretera.VehiculoPuente = null; }
                    Console.WriteLine($"El vehiculo #{VActual.Id} ha liberado el tunel...");
                }


                Task TareaEnvioClientes = new Task(() =>
                {
                    Parallel.ForEach(clientes, cli =>
                    {
                        try
                        {
                            lock (locker)
                            {
                                if (cli.NS.CanWrite) NetworkStreamClass.EscribirDatosCarreteraNS(cli.NS, carretera);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Cliente #{cli.Id} desconectado al enviar datos. Eliminando de la lista. Error: {e.Message}");
                            lock (locker)
                            {
                                clientes.Remove(cli);
                            }
                        }
                    });
                });
                TareaEnvioClientes.Start();

            } while (!VActual!.Acabado);

            // Al terminar el vehículo cerramos la conexión
            lock (locker)
            {
                clientes.Remove(conexion);
            }

            //Console.WriteLine($"Vehiculo #{v.Id} ha finalizado. Conexión cerrada.");
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
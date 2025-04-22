using System.ComponentModel.DataAnnotations;
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
        static readonly object locker = new();
        static SemaphoreSlim SemAcceso = new (0);

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
                    Id = Handshake(NS);

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
            NS?.Close();
            Cliente.Close();
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
            lock (locker)
            {
                v = new Vehiculo();
                v.Id = Id;
                v.Direccion = Dir;
            }

            if (v.Direccion == "norte") 
            {
                v.Pos = 100;
                for (int i = 100; i > 0; i--)
                {
                    Thread.Sleep(v.Velocidad);
                    v.Pos--;

                    if (v.Pos % 5 == 0 && v.Pos != 50) { NetworkStreamClass.EscribirDatosVehiculoNS(NS, v); }

                    // Si está a punto de entrar al puente, escuchar permiso del servidor
                    if (v.Pos == 50)
                    {
                        lock (locker) { v.Parado = true; }
                        NetworkStreamClass.EscribirDatosVehiculoNS(NS, v);
                        // Console.WriteLine($"Enviando Vehiculo -> Id: {v.Id}, Direccion: {v.Direccion}, Pos: {v.Pos}, Parado: {v.Parado}, Acabado: {v.Acabado}");
                        Console.WriteLine("El vehículo ha llegado al puente. Esperando acceso...");
                        SemAcceso.Wait();
                        Console.WriteLine("¡Acceso concedido!");
                    }
                }
            } else 
            {
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(v.Velocidad);
                    v.Pos++;

                    if (v.Pos % 5 == 0 && v.Pos != 30) { NetworkStreamClass.EscribirDatosVehiculoNS(NS, v); }

                    // Si está a punto de entrar al puente, escuchar permiso del servidor
                    if (v.Pos == 30)
                    {
                        lock (locker) { v.Parado = true; }
                        NetworkStreamClass.EscribirDatosVehiculoNS(NS, v);
                        // Console.WriteLine($"Enviando Vehiculo -> Id: {v.Id}, Direccion: {v.Direccion}, Pos: {v.Pos}, Parado: {v.Parado}, Acabado: {v.Acabado}");
                        Console.WriteLine("El vehículo ha llegado al puente. Esperando acceso...");
                        SemAcceso.Wait();
                        Console.WriteLine("¡Acceso concedido!");
                    }
                }
            }
            lock (locker)
            {
                v.Acabado = true;
                NetworkStreamClass.EscribirDatosVehiculoNS(NS, v);
            }
        }

        private static void EscuchasCarretera()
        {
            Carretera c = new();
            Console.WriteLine("Ejecutando escuchas carretera");
            string BufferPendienteCarretera ="";

            do
            {
                try 
                {
                    c = NetworkStreamClass.LeerDatosCarreteraNS(NS, ref BufferPendienteCarretera);
                    int? pos = null;
                    bool? parado = null;
                    foreach (Vehiculo vc in c.VehiculosEnCarretera)
                    {
                        if (vc.Id == v!.Id) 
                        { 
                            pos = vc.Pos; 
                            parado = vc.Parado;
                        }
                    }
                } catch (Exception e) { Console.WriteLine($"Algo ha ocurrido mal durante la lectura: {e.Message}"); }

                if (v!.Parado && c.VehiculoPuente?.Id == v!.Id)
                {
                    lock (locker) { v.Parado = false; }
                    SemAcceso.Release();
                }

                Console.WriteLine($"\n\n\n### RESUMEN CARRETERA - VEHICULO {v.Id} ###\n");
                foreach (Vehiculo vehiculo in c.VehiculosEnCarretera)
                {

                    Console.Write("[{0}]\t Vehículo #{1}: ", vehiculo.Direccion, vehiculo.Id);
                    for (int i = 0; i<100; i += 2)
                    {
                        if (vehiculo.Direccion == "sur")
                        {
                            if (i<vehiculo.Pos) Console.Write("+");
                            else Console.Write("-");
                        } else 
                        {
                            if (i<vehiculo.Pos) Console.Write("-");
                            else Console.Write("+");
                        }
                    }
                    Console.Write(" (Km {0} - ", vehiculo.Pos);
                    if (vehiculo.Parado) Console.WriteLine("Esperando)");
                    else Console.WriteLine("Cruzando)");
                }

                if (v != null && v.Acabado)
                {
                    Console.WriteLine("El vehiculo ha llegado al final de la carretera");
                    break;
                } 

            } while (true);
        }

    }
}
using System.Net.Sockets;
using System.Net;
using Conexion;
using NetworkStreamNS;

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
                        
                        // Handshake exitoso, conservamos la conexion
                        var conexion = new Cliente(IdCliente, NS);
                        
                        lock (locker)
                        {
                            clientes.Add(conexion);
                        }
                        Console.WriteLine("Conexión creada - Total conexiones: {0}", clientes.Count);
                        string instruccion;
                        do
                        {
                            instruccion = NetworkStreamClass.LeerMensajeNetworkStream(NS);
                            if (instruccion == null) 
                                throw new DesconexionClienteException(conexion, "El cliente ha cerrado la conexión de forma inesperada. Liberando recursos...");
                            if (instruccion == "quit") 
                                throw new DesconexionClienteException(conexion, "El cliente ha cerrado la conexión de forma controlada. Liberando recursos...");
                            NetworkStreamClass.EscribirMensajeNetworkStream(NS, "Servidor: Petición realizada ("+instruccion+")");
                            Console.WriteLine("Se ha procesado la solicitud: {0}", instruccion);
                        } while (true);
                    } else
                    {
                        throw new Exception("El cliente ha iniciado el handshake erróneamente. Cerrando conexión...");
                    }
                    
                }
            } catch (DesconexionClienteException e)
            {
                Console.WriteLine(e.Message);
                Cliente.Close();
                lock (locker)
                {
                    clientes.Remove(e.Cliente);
                }
                Console.WriteLine("Conexión eliminada - Total conexiones: {0}", clientes.Count);
            } catch (Exception e)
            {
                Console.WriteLine("Ha ocurrido un error: {0}", e.Message);
                Cliente.Close();
            }
        }

    }
}
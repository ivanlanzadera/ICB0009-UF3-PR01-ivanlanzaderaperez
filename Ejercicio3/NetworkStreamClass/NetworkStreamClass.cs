using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using VehiculoClass;
using CarreteraClass;
using System.Threading;
using Conexion;

namespace NetworkStreamNS
{
    public class NetworkStreamClass
    {

        //Método para escribir en un NetworkStream los datos de tipo Carretera
        public static void  EscribirDatosCarreteraNS(NetworkStream NS, Carretera C)
        {
            // Pasar los datos a bytes
            byte[] DatosCarretera = C.CarreteraABytes();
            string mensaje = Encoding.UTF8.GetString(DatosCarretera) + "<EOF>";

            byte[] DatosDelimitados = Encoding.UTF8.GetBytes(mensaje);
            NS.Write(DatosDelimitados, 0, DatosDelimitados.Length);
        }

        //Metódo para leer de un NetworkStream los datos que de un objeto Carretera
        public static Carretera LeerDatosCarreteraNS (NetworkStream NS, ref string BufferPendienteCarretera)
        {
            StringBuilder MensajeCompleto = new();
            byte[] buffer = new byte[1024];
            MensajeCompleto.Append(BufferPendienteCarretera);
            BufferPendienteCarretera = "";

            // Leemos datos del stream
            do
            {
                int BytesLeidos = NS.Read(buffer, 0, buffer.Length);
                MensajeCompleto.Append(Encoding.UTF8.GetString(buffer, 0, BytesLeidos));

                // Procesamos el mensaje si contiene el delimitador
                if (MensajeCompleto.ToString().Contains("<EOF>"))
                {
                    string MensajeDelimitado = MensajeCompleto.ToString();
                    string ObjetoObtenido = MensajeDelimitado.Substring(0, MensajeDelimitado.IndexOf("<EOF>"));
                    BufferPendienteCarretera = MensajeDelimitado.Substring(MensajeDelimitado.IndexOf("<EOF>") + "<EOF>".Length);

                    // Convertimos el objeto a bytes
                    byte[] InstanciaBytes = Encoding.UTF8.GetBytes(ObjetoObtenido);
                    return Carretera.BytesACarretera(InstanciaBytes);
                }
            } while (true);
        }

        //Método para enviar datos de tipo Vehiculo en un NetworkStream
        public static void  EscribirDatosVehiculoNS(NetworkStream NS, Vehiculo V)
        {
            // Pasar vehiculo a bytes
            byte[] DatosVehiculo = V.VehiculoaBytes();
            // Añadimos delimitador de una instancia del vehiculo
            string instancia = Encoding.UTF8.GetString(DatosVehiculo) + "<EOF>";

            // Convertimos el objeto completo a bytes y lo escribimos en el NS
            byte[] DatosDelimitados = Encoding.UTF8.GetBytes(instancia);
            NS.Write(DatosDelimitados, 0, DatosDelimitados.Length);
        }

        //Metódo para leer de un NetworkStream los datos que de un objeto Vehiculo
        public static Vehiculo LeerDatosVehiculoNS (Cliente cliente)
        {

            StringBuilder MensajeCompleto = new();
            byte[] buffer = new byte[1024];
            MensajeCompleto.Append(cliente.BufferPendienteVehiculo);
            cliente.BufferPendienteVehiculo = "";

            // Leemos del NS hasta encontrar un delimitador
            do
            {
                int BytesLeidos = cliente.NS.Read(buffer, 0, buffer.Length);
                MensajeCompleto.Append(Encoding.UTF8.GetString(buffer, 0, BytesLeidos));

                // Si encontramos delimitador salimos del bucle
                if (MensajeCompleto.ToString().Contains("<EOF>"))
                {
                    string MensajeDelimitado = MensajeCompleto.ToString();
                    string MensajeInstancia = MensajeDelimitado.Substring(0, MensajeDelimitado.IndexOf("<EOF>"));
                    cliente.BufferPendienteVehiculo = MensajeDelimitado.Substring(MensajeDelimitado.IndexOf("<EOF>") + "<EOF>".Length);

                    // Convertimos el mensaje sin delimitador a bytes
                    byte[] instancia = Encoding.UTF8.GetBytes(MensajeInstancia);

                    // Deserializamos y devolvemos el objeto Vehiculo
                    return Vehiculo.BytesAVehiculo(instancia);
                }
            } while (true);
        }

        //Método que permite leer un mensaje de tipo texto (string) de un NetworkStream
        public static string LeerMensajeNetworkStream (NetworkStream NS)
        {
            // Definimos el buffer de recepción
            byte[] bufferLectura = new byte[1024];

            //Lectura del mensaje
            int bytesLeidos = 0;
            var tmpStream = new MemoryStream();
            byte[] bytesTotales; 
            do
            {
                // Leemos del NS, si la conexión se cierra devolvemos null para manejar excepciones en server
                int bytesLectura;
                try {bytesLectura = NS.Read(bufferLectura,0,bufferLectura.Length);}
                catch (Exception)
                {
                    return null;
                }
                // Escribimos en un MemoryStream temporal
                tmpStream.Write(bufferLectura, 0, bytesLectura);
                // Incrementamos los Bytes leídos
                bytesLeidos = bytesLeidos + bytesLectura;
            }while (NS.DataAvailable);

            bytesTotales = tmpStream.ToArray();            

            return Encoding.UTF8.GetString(bytesTotales, 0, bytesLeidos);
        }

        //Método que permite escribir un mensaje de tipo texto (string) al NetworkStream
        public static void  EscribirMensajeNetworkStream(NetworkStream NS, string Str)
        {
            // Pasamos el String a Btytes
            byte[] MensajeBytes = Encoding.UTF8.GetBytes(Str);
            // Realizamos el envío de los Bytes a través del NS
            NS.Write(MensajeBytes,0,MensajeBytes.Length);                        
        }                          
    }
}

using System;
using System.Net.Sockets;
using System.Text;
using System.IO;


namespace NetworkStreamNS
{
    public class NetworkStreamClass
    {
        //Método para escribir en un NetworkStream los datos de tipo Carretera
        public static void  EscribirDatosCarreteraNS(NetworkStream NS, Carretera C)
        {            
                            
        }

        //Metódo para leer de un NetworkStream los datos que de un objeto Carretera
        /*public static Carretera LeerDatosCarreteraNS (NetworkStream NS)
        {
            

        }*/

        //Método para enviar datos de tipo Vehiculo en un NetworkStream
        public static void  EscribirDatosVehiculoNS(NetworkStream NS, Vehiculo V)
        {            
                              
        }

        //Metódo para leer de un NetworkStream los datos que de un objeto Vehiculo
        /*public static Vehiculo LeerDatosVehiculoNS (NetworkStream NS)
        {

        }*/

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
                catch (Exception e)
                {
                    return null;
                }
                // Escribimos en un MemoryStream temporal
                tmpStream.Write(bufferLectura, 0, bytesLectura);
                // Incrementamos los Bytes leídos
                bytesLeidos = bytesLeidos + bytesLectura;
            }while (NS.DataAvailable);

            bytesTotales = tmpStream.ToArray();            

            return Encoding.Unicode.GetString(bytesTotales, 0, bytesLeidos);
        }

        //Método que permite escribir un mensaje de tipo texto (string) al NetworkStream
        public static void  EscribirMensajeNetworkStream(NetworkStream NS, string Str)
        {
            // Pasamos el String a Btytes
            byte[] MensajeBytes = Encoding.Unicode.GetBytes(Str);
            // Realizamos el envío de los Bytes a través del NS
            NS.Write(MensajeBytes,0,MensajeBytes.Length);                        
        }                          
    }
}

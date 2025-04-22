using System.Xml.Serialization;
using System.Text;

namespace VehiculoClass;

[Serializable]
public class Vehiculo
{
    public int Id {get; set;}
    public int Pos {get;set;}
    public int Velocidad {get; set;}
    public string Direccion {get; set;} // "Norte" o "Sur" 
    public bool Acabado {get;set;}
    public bool Parado {get; set;}
    
    public Vehiculo()
    {
        var randVelocidad = new Random();

        this.Velocidad = randVelocidad.Next(100,500);
        this.Pos = 0;
        this.Acabado = false;
    }

    //Permite serializar Vehiculo a array de bytes mediant formato XML
    public byte[] VehiculoaBytes()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Vehiculo));
            
        MemoryStream MS = new MemoryStream();
  
        serializer.Serialize(MS, this);
       
        return MS.ToArray();
    }

    //Permite desserializar una cadena de bytes a un objeto de tipo Vehiculo
    public static Vehiculo BytesAVehiculo(byte[] bytesVehiculo)
    {
        Vehiculo tmpVehiculo; 

        string datos = Encoding.UTF8.GetString(bytesVehiculo);
        

        try 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vehiculo));

            MemoryStream MS = new MemoryStream(bytesVehiculo);

            tmpVehiculo = (Vehiculo)serializer.Deserialize(MS);

            return tmpVehiculo;
        } catch (Exception) { Console.WriteLine($"Error al leer el mensaje: {datos}"); }

        return null;
    }
}

using Conexion;
using VehiculoClass;

namespace servidor;

public class DesconexionClienteException : Exception
{
    public Cliente Cliente { get; }
    public Vehiculo? Vehiculo { get; }

    public DesconexionClienteException (Cliente c, Vehiculo? v, string msg) : base(msg)
    {
        Cliente = c;
        Vehiculo = v;
    }
}

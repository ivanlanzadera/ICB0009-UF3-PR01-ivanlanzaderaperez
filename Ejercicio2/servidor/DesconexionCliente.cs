using Conexion;

namespace servidor;

public class DesconexionClienteException : Exception
{
    public Cliente Cliente { get; }

    public DesconexionClienteException (Cliente c, string msg) : base(msg)
    {
        Cliente = c;
    }
}

using System.Net.Sockets;

namespace Conexion;

public class Cliente
{
    public int Id { get; set; }
    public NetworkStream NS { get; set; }

    public Cliente (int id, NetworkStream ns)
    {
        Id = id;
        NS = ns;
    }
}

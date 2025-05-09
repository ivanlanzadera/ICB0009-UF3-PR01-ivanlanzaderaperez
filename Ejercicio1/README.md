# 🧪 Ejercicio 1 - Conexión de clientes en C#

## 📌 Descripción del ejercicio

Este proyecto consiste en el desarrollo de una aplicación cliente-servidor en C# que permite múltiples conexiones simultáneas a través de `TcpClient` y `TcpListener`, utilizando `NetworkStream` para la comunicación.

Los principales logros del ejercicio son:

- Establecer una conexión entre cliente y servidor.
- Implementar un **handshake** inicial que valide la conexión.
- Permitir la comunicación bidireccional entre cliente y servidor.
- Gestionar múltiples clientes concurrentes mediante hilos.
- Controlar adecuadamente las desconexiones, tanto controladas como inesperadas, mediante una **excepción personalizada**.

---


---

## ⚙️ Funcionamiento general

### 🔁 Handshake

1. El cliente se conecta al servidor e inicia la comunicación enviando el mensaje `"INICIO"`.
2. El servidor asigna un `Id` al cliente y le responde con ese valor.
3. El cliente debe devolver el mismo `Id` como confirmación.
4. Si el handshake es exitoso, la conexión se mantiene activa y se almacena en la lista de conexiones activas.

### 💬 Comunicación

- El cliente puede enviar cualquier texto.
- El servidor responde con una confirmación del tipo:  
  `"Servidor: Petición realizada (<mensaje>)"`

### ❌ Desconexión

- Si el cliente envía `"quit"` o se desconecta inesperadamente:
  - Se lanza una **excepción personalizada** `DesconexionClienteException`.
  - Esta excepción incluye una referencia al objeto `Cliente` que ha causado la desconexión.
  - El servidor elimina dicha conexión de la lista y libera los recursos.

---

## 🧠 Aspectos técnicos destacados

### 📦 `NetworkStreamClass`

Contiene dos métodos estáticos reutilizables:

- `LeerMensajeNetworkStream(NetworkStream)`: lee un mensaje del stream y devuelve `null` si el cliente se ha desconectado.
- `EscribirMensajeNetworkStream(NetworkStream, string)`: envía un mensaje al cliente.

### ⚙️ Clase `Cliente`

Ubicada en la carpeta `Conexion`, encapsula los datos del cliente:

```csharp
public class Cliente
{
    public int Id { get; set; }
    public NetworkStream NS { get; set; }

    public Cliente(int id, NetworkStream ns)
    {
        Id = id;
        NS = ns;
    }
}
```

### ❗ Excepción personalizada: DesconexionClienteException

Permite manejar desconexiones de forma clara y estructurada. Almacena una referencia directa al cliente desconectado:
```csharp
public class DesconexionClienteException : Exception
{
    public Cliente Cliente { get; }

    public DesconexionClienteException (Cliente c, string msg) : base(msg)
    {
        Cliente = c;
    }
}
```

### 🧵 Concurrencia

- El servidor gestiona cada cliente en un hilo distinto.
- Se utiliza lock para proteger el acceso a la lista compartida de clientes (List < Cliente >).

#### ¿Por qué usar `List<Clientes>`?
Para mantener un registro de todas las conexiones activas, se optó por una lista global compartida entre los distintos hilos que gestionan los clientes. Aunque inicialmente se consideró el uso de estructuras concurrentes como `ConcurrentQueue`, finalmente se decidió emplear una lista secuencial (List<Cliente>) junto con mecanismos explícitos de sincronización mediante bloqueos (lock).
Esta elección se basa en la simplicidad de gestión que ofrece esta estructura, especialmente a la hora de eliminar conexiones inactivas o cerradas, lo cual facilita una administración más directa y controlada del ciclo de vida de cada cliente.

---

# 🖼️ Capturas de pantalla
### ▶️ Cliente conectado con éxito
![](imgs/conexion-cliente.png)
![](imgs/conexion-server.png)
> En esta simulación se han conectado dos clientes al servidor

### ✉️ Comunicación entre cliente y servidor
![](imgs/comunicacion-cliente.png)
![](imgs/comunicacion-server.png)

### ❌ Cliente desconectado de forma inesperada
![](imgs/desconexion-server.png)


---

# 🚀 Cómo ejecutar el proyecto

1. Abre la solución en *Visual Studio* o *Visual Studio Code*.
2. Ejecuta primero el proyecto **servidor**.
3. Luego ejecuta una o varias instancias del proyecto **cliente**.
4. Prueba enviando mensajes desde el cliente, incluyendo "quit" para desconectar.

---

# ✅ Conclusiones

- Se ha implementado un sistema cliente-servidor robusto, que maneja correctamente la concurrencia y las desconexiones.
- Se ha separado la lógica de comunicación (`NetworkStreamClass`) del control de flujos y gestión de excepciones.
- La implementación es extensible, y permite añadir nuevas funcionalidades fácilmente, como autenticación o mensajes más complejos.
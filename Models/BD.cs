namespace TP05_Martinez_Loufer.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string _connectionString =
        @"Server=localhost; DataBase=MiPagina ; Integrated Security=True; TrustServerCertificate=True;";

    public int ValidarUsuario(string nombreUsuario, string password)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            int id = 0;
            string query = "SELECT Id FROM Usuarios WHERE NombreUsuario = @nombreUsuario AND Password = @password";
            id = connection.QueryFirstOrDefault<int>(query, new { nombreUsuario, password });
            return id;
        }
    }

    public string BuscarNombreUsuario(string nombreUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT NombreUsuario FROM Usuarios WHERE NombreUsuario = @nombreUsuario";
            string encontrado = connection.QueryFirstOrDefault<string>(query, new { nombreUsuario });
            return encontrado;
        }
    }
    public bool RegistrarUsuario(string nombreUsuario, string password, string nombre, string apellido, string tipoUsuario, string email)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            if (BuscarNombreUsuario(nombreUsuario) == null)
            {
                string query = "INSERT INTO Usuarios (NombreUsuario, Password, Nombre, Apellido, TipoUsuario, Email) VALUES (@nombreUsuario, @password, @nombre, @apellido, @tipoUsuario, @email)";
                connection.Execute(query, new { nombreUsuario, password, nombre, apellido, tipoUsuario, email });
                return true;
            }
            return false;
        }
    }

    public Usuario ObtenerUsuarioPorId(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuarios WHERE Id = @id";
            Usuario usuario = connection.QueryFirstOrDefault<Usuario>(query, new { id });
            return usuario;
        }
    }

    public Usuario BuscarPorEmail(string email)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuarios WHERE Email = @email";
            Usuario usuario = connection.QueryFirstOrDefault<Usuario>(query, new { email });
            return usuario;
        }
    }

    public string GenerarTokenRecuperacion(int usuarioId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string token = Guid.NewGuid().ToString("N");
            DateTime fechaExpiracion = DateTime.Now.AddMinutes(30);
            
            string query = "INSERT INTO TokensRecuperacion (UsuarioId, Token, FechaExpiracion) VALUES (@usuarioId, @token, @fechaExpiracion)";
            connection.Execute(query, new { usuarioId, token, fechaExpiracion });
            
            return token;
        }
    }

    public int ValidarTokenRecuperacion(string token)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT UsuarioId FROM TokensRecuperacion WHERE Token = @token AND FechaExpiracion > GETDATE()";
            int usuarioId = connection.QueryFirstOrDefault<int>(query, new { token });
            
            if (usuarioId > 0)
            {
                // Eliminar token después de validarlo (se usa una sola vez)
                connection.Execute("DELETE FROM TokensRecuperacion WHERE Token = @token", new { token });
            }
            
            return usuarioId;
        }
    }

    public bool ActualizarPassword(int usuarioId, string nuevoPassword)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE Usuarios SET Password = @nuevoPassword WHERE Id = @usuarioId";
            int filasAfectadas = connection.Execute(query, new { usuarioId, nuevoPassword });
            return filasAfectadas > 0;
        }
    }
}
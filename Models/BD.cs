namespace TP05_Martinez_Loufer.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string _connectionString =
        @"Server=localhost; DataBase= ??? ; Integrated Security=True; TrustServerCertificate=True;";

    public int ValidarUsuario(string nombreUsuario, string password)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT Id FROM Usuarios WHERE NombreUsuario = @nombreUsuario AND Password = @password";
            int id = connection.QueryFirstOrDefault<int>(query, new { nombreUsuario, password });
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
    public bool RegistrarUsuario(string nombreUsuario, string password, string nombre, string apellido, string tipoUsuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            if (BuscarNombreUsuario(nombreUsuario) == null)
            {
                string query = "INSERT INTO Usuarios (NombreUsuario, Password, Nombre, Apellido, TipoUsuario) VALUES (@nombreUsuario, @password, @nombre, @apellido, @tipoUsuario)";
                connection.Execute(query, new { nombreUsuario, password, nombre, apellido, tipoUsuario });
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
}
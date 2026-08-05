namespace TP05_Martinez_Loufer.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string _connectionString =
        @"Server=localhost; DataBase= ??? ; Integrated Security=True; TrustServerCertificate=True;";
}
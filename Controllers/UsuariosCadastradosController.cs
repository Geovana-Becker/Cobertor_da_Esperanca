using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosCadastradosController : ControllerBase
    {
        private readonly ILogger<UsuariosCadastradosController> _logger;
        public UsuariosCadastradosController(ILogger<UsuariosCadastradosController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        [HttpGet(Name = "GetUsuarios")]
        public IEnumerable<Usuarios> Get()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string querry = "SELECT * FROM Usuarios";
                SqlCommand command = new SqlCommand(querry, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Usuarios usuario = new Usuarios()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        NOME = reader["NOME"].ToString(),
                        CPF = reader["CPF"].ToString(),
                        TELEFONE = reader["TELEFONE"].ToString(),
                        EMAIL = reader["EMAIL"].ToString(),
                        SENHA = reader["SENHA"].ToString(),
                        PONTUACAO = Convert.ToInt32(reader["PONTUACAO"])
                    };

                    usuarios.Add(usuario);
                }

                reader.Close();
            }

            return usuarios;
        }


        [HttpPut("{id}")]

        public ActionResult Updateusuario(int id, [FromBody] Usuarios usuario)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "UPDATE Usuarios SET PONTUACAO = @PONTUACAO WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PONTUACAO", usuario.PONTUACAO);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok();
                }

            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUsuario(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Abrir conexão
                connection.Open();

                // Iniciar uma transação para garantir que ambas as exclusões ocorram de forma atômica
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Excluir as recompensas resgatadas do usuário
                        string deleteRecompensasQuery = "DELETE FROM RecompensasResgatadas WHERE UsuarioId = @Id";
                        SqlCommand deleteRecompensasCommand = new SqlCommand(deleteRecompensasQuery, connection, transaction);
                        deleteRecompensasCommand.Parameters.AddWithValue("@Id", id);
                        deleteRecompensasCommand.ExecuteNonQuery();

                        // Excluir o usuário
                        string deleteUsuarioQuery = "DELETE FROM Usuarios WHERE Id = @Id";
                        SqlCommand deleteUsuarioCommand = new SqlCommand(deleteUsuarioQuery, connection, transaction);
                        deleteUsuarioCommand.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = deleteUsuarioCommand.ExecuteNonQuery();

                        // Verificar se o usuário foi excluído
                        if (rowsAffected > 0)
                        {
                            // Se tudo deu certo, commit a transação
                            transaction.Commit();
                            return Ok();
                        }
                        else
                        {
                            // Se não houver registros afetados, dá rollback na transação
                            transaction.Rollback();
                            return NotFound();
                        }
                    }
                    catch (Exception)
                    {
                        // Em caso de erro, faz rollback
                        transaction.Rollback();
                        return StatusCode(500, "Erro ao excluir o usuário e as recompensas resgatadas.");
                    }
                }
            }
        }
    }
}

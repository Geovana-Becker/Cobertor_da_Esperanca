using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class RecompensasController : Controller
    {
        private readonly ILogger<RecompensasController> _logger;
        public RecompensasController(ILogger<RecompensasController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        // Método para gerar código aleatório
        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public IActionResult CreateRecompensa([FromBody] Recompensas recompensa)
        {

            _logger.LogInformation($"Imagem recebida: {(string.IsNullOrEmpty(recompensa.IMAGEM) ? "Vazia" : "Preenchida")}");

            // Gerar código aleatório
            string codigoAleatorio = GenerateRandomCode(10);

            using SqlConnection connection = new SqlConnection(ConnectionString);
            {
                string query = "INSERT INTO Recompensas (IMAGEM, NOME, DESCRICAO, VALOR, CODIGO) VALUES (@IMAGEM, @NOME, @DESCRICAO, @VALOR, @CODIGO)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IMAGEM", recompensa.IMAGEM ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NOME", recompensa.NOME);
                command.Parameters.AddWithValue("@DESCRICAO", recompensa.DESCRICAO);
                command.Parameters.AddWithValue("@VALOR", recompensa.VALOR);
                command.Parameters.AddWithValue("@CODIGO", codigoAleatorio);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok("Recompensa criada com sucesso!");
                }
                else
                {
                    return BadRequest("Erro ao inserir a recompensa no banco de dados.");
                }
            }
        }

        [HttpGet(Name = "Getrecompensas")]
        public IEnumerable<Recompensas> Get()
        {
            List<Recompensas> produto = new List<Recompensas>();

            using (SqlConnection conection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * from  Recompensas";
                SqlCommand command = new SqlCommand(query, conection);
                conection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Recompensas recompensa = new Recompensas
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        NOME = reader["NOME"].ToString(),
                        DESCRICAO = reader["DESCRICAO"].ToString(),
                        VALOR = Convert.ToInt32(reader["VALOR"]),
                        CODIGO = reader["CODIGO"].ToString()
                    };

                    recompensa.IMAGEM = reader["IMAGEM"].ToString();

                    produto.Add(recompensa);
                }

                reader.Close();
            }

            return produto;
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteEmployee(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open(); // Certifica-se de abrir a conexão antes de iniciar a transação

                    // Começa uma transação para garantir que ambas as exclusões ocorram de forma atômica
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Deletar da tabela RecompensasResgatadas onde o Id da recompensa é igual ao id fornecido
                            string deleteResgatadasQuery = "DELETE FROM RecompensasResgatadas WHERE RecompensaId = @Id";
                            SqlCommand deleteResgatadasCommand = new SqlCommand(deleteResgatadasQuery, connection, transaction);
                            deleteResgatadasCommand.Parameters.AddWithValue("@Id", id);
                            deleteResgatadasCommand.ExecuteNonQuery();

                            // Agora, deletar da tabela Recompensas
                            string deleteRecompensasQuery = "DELETE FROM Recompensas WHERE Id = @Id";
                            SqlCommand deleteRecompensasCommand = new SqlCommand(deleteRecompensasQuery, connection, transaction);
                            deleteRecompensasCommand.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = deleteRecompensasCommand.ExecuteNonQuery();

                            // Se a recompensa foi excluída com sucesso, confirma a transação
                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return Ok();
                            }
                            else
                            {
                                // Se a recompensa não foi encontrada para excluir, faz o rollback da transação
                                transaction.Rollback();
                                return NotFound();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Se ocorrer um erro dentro da transação, faz o rollback e retorna detalhes do erro
                            transaction.Rollback();
                            return StatusCode(500, $"Erro ao tentar excluir a recompensa: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Se a conexão não puder ser aberta ou outro erro ocorrer, retorna o erro
                    return StatusCode(500, $"Erro ao conectar ao banco de dados: {ex.Message}");
                }
            }
        }
    }
}

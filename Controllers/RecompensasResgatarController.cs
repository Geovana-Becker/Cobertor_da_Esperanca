using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    // Controlador para Resgatar Recompensas
    [ApiController]
    [Route("[controller]")]
    public class RecompensasResgatarController : ControllerBase
    {
        private readonly ILogger<RecompensasResgatarController> _logger;
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        public RecompensasResgatarController(ILogger<RecompensasResgatarController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Resgatar")]
        public async Task<IActionResult> ResgatarRecompensa([FromBody] ResgateRequest request)
        {
            if (request == null || request.UsuarioId <= 0 || request.RecompensaId <= 0)
            {
                return BadRequest(new { message = "Dados inválidos na requisição." });
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Verificar se a recompensa já foi resgatada
                    string queryVerificarResgate = "SELECT COUNT(1) FROM RecompensasResgatadas WHERE RecompensaId = @RecompensaId";
                    using (SqlCommand cmdVerificarResgate = new SqlCommand(queryVerificarResgate, connection))
                    {
                        cmdVerificarResgate.Parameters.AddWithValue("@RecompensaId", request.RecompensaId);
                        int jaResgatada = (int)await cmdVerificarResgate.ExecuteScalarAsync();

                        if (jaResgatada > 0)
                        {
                            return BadRequest(new { message = "Esta recompensa já foi resgatada por outra pessoa." });
                        }
                    }

                    // Verificar se o usuário existe e obter a pontuação
                    string queryUsuario = "SELECT PONTUACAO FROM Usuarios WHERE Id = @UsuarioId";
                    int pontuacaoAtual;

                    using (SqlCommand cmdUsuario = new SqlCommand(queryUsuario, connection))
                    {
                        cmdUsuario.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
                        var result = await cmdUsuario.ExecuteScalarAsync();

                        if (result == null)
                        {
                            return NotFound(new { message = "Usuário não encontrado." });
                        }

                        pontuacaoAtual = Convert.ToInt32(result);
                    }

                    // Verificar se a recompensa existe e obter dados
                    string queryRecompensa = "SELECT VALOR, CODIGO, NOME FROM Recompensas WHERE Id = @RecompensaId";
                    int valorRecompensa;
                    string codigoRecompensa;
                    string nomeRecompensa;

                    using (SqlCommand cmdRecompensa = new SqlCommand(queryRecompensa, connection))
                    {
                        cmdRecompensa.Parameters.AddWithValue("@RecompensaId", request.RecompensaId);
                        using (var reader = await cmdRecompensa.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return NotFound(new { message = "Recompensa não encontrada." });
                            }

                            await reader.ReadAsync();
                            valorRecompensa = reader.GetInt32(0);
                            codigoRecompensa = reader.GetString(1);
                            nomeRecompensa = reader.GetString(2);
                        }
                    }

                    // Verificar se o usuário possui pontuação suficiente
                    if (pontuacaoAtual < valorRecompensa)
                    {
                        return BadRequest(new { message = "Pontos insuficientes para resgatar a recompensa." });
                    }

                    // Atualizar a pontuação do usuário
                    string queryAtualizarPontuacao = "UPDATE Usuarios SET PONTUACAO = PONTUACAO - @Valor WHERE Id = @UsuarioId";
                    using (SqlCommand cmdAtualizar = new SqlCommand(queryAtualizarPontuacao, connection))
                    {
                        cmdAtualizar.Parameters.AddWithValue("@Valor", valorRecompensa);
                        cmdAtualizar.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);

                        int rowsAffected = await cmdAtualizar.ExecuteNonQueryAsync();

                        if (rowsAffected <= 0)
                        {
                            return StatusCode(500, new { message = "Erro ao atualizar pontuação do usuário." });
                        }
                    }

                    // Registrar o resgate
                    string queryInserirResgate = "INSERT INTO RecompensasResgatadas (UsuarioId, RecompensaId, Codigo, DataResgate) " +
                                                 "VALUES (@UsuarioId, @RecompensaId, @Codigo, @DataResgate)";
                    using (SqlCommand cmdInserirResgate = new SqlCommand(queryInserirResgate, connection))
                    {
                        cmdInserirResgate.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
                        cmdInserirResgate.Parameters.AddWithValue("@RecompensaId", request.RecompensaId);
                        cmdInserirResgate.Parameters.AddWithValue("@Codigo", codigoRecompensa);
                        cmdInserirResgate.Parameters.AddWithValue("@DataResgate", DateTime.Now);

                        int rowsAffectedResgate = await cmdInserirResgate.ExecuteNonQueryAsync();

                        if (rowsAffectedResgate <= 0)
                        {
                            return StatusCode(500, new { message = "Erro ao registrar o resgate da recompensa." });
                        }
                    }

                    // Retornar sucesso
                    return Ok(new
                    {
                        message = "Recompensa resgatada com sucesso!",
                        codigo = codigoRecompensa,
                        nome = nomeRecompensa
                    });
                }
                catch (SqlException sqlEx)
                {
                    _logger.LogError($"Erro de SQL: {sqlEx.Message}");
                    return StatusCode(500, new { message = "Erro ao acessar o banco de dados." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro inesperado: {ex.Message}");
                    return StatusCode(500, new { message = "Erro interno no servidor." });
                }
            }
        }
    }

        // Controlador para Recompensas Resgatadas
        [ApiController]
    [Route("api/[controller]")]
    public class RecompensasResgatadasController : ControllerBase
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetRecompensasResgatadas(int usuarioId)
        {
            List<RecompensasResgatadas> recompensasResgatadas = new List<RecompensasResgatadas>();

            // Usando Ado.NET para acessar o banco de dados
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Realizando JOIN para incluir o nome da recompensa
                string query = @"
                    SELECT r.Codigo, r.DataResgate, rp.Nome 
                    FROM RecompensasResgatadas r
                    JOIN Recompensas rp ON r.RecompensaId = rp.Id
                    WHERE r.UsuarioId = @UsuarioId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                await connection.OpenAsync();

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        RecompensasResgatadas recompensa = new RecompensasResgatadas
                        {
                            Codigo = reader["Codigo"].ToString(),
                            DataResgate = Convert.ToDateTime(reader["DataResgate"]),
                            Nome = reader["Nome"].ToString()  // Recuperando o nome da recompensa
                        };

                        recompensasResgatadas.Add(recompensa);
                    }
                }
            }

            if (recompensasResgatadas.Count == 0)
            {
                return NotFound(new { message = "Nenhuma recompensa resgatada encontrada." });
            }

            return Ok(recompensasResgatadas);
        }
    }

    // Classe de Requisição para Resgatar Recompensa
    public class ResgateRequest
    {
        public int UsuarioId { get; set; }
        public int RecompensaId { get; set; }
    }

    // Classe representando uma recompensa resgatada
    public class RecompensasResgatadas
    {
        public string Codigo { get; set; }
        public DateTime DataResgate { get; set; }
        public string Nome { get; set; }  // Nova propriedade para o nome da recompensa
    }
}

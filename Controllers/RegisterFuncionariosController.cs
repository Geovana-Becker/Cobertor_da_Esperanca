using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class RegisterFuncionariosController : ControllerBase
    {
        private readonly ILogger<RegisterFuncionariosController> _logger;
        public RegisterFuncionariosController(ILogger<RegisterFuncionariosController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        [HttpPost]
        public async Task<ActionResult> CreateFuncionario([FromBody] Funcionarios funcionario)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Verificar se o CPF já existe no banco
                    string checkQuery = "SELECT COUNT(1) FROM Funcionarios WHERE CPF = @CPF";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@CPF", funcionario.CPF);

                    int count = (int)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        // Se o CPF já existe, retorne BadRequest com mensagem
                        return BadRequest(new { message = "CPF já cadastrado." });
                    }

                    // Caso o CPF não exista, faça o INSERT no banco
                    string insertQuery = "INSERT INTO Funcionarios (NOME, CPF, CARGO, TELEFONE, EMAIL, SENHA) VALUES (@NOME, @CPF, @CARGO, @TELEFONE, @EMAIL, @SENHA)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@NOME", funcionario.NOME);
                    insertCommand.Parameters.AddWithValue("@CPF", funcionario.CPF);
                    insertCommand.Parameters.AddWithValue("@CARGO", funcionario.CARGO);
                    insertCommand.Parameters.AddWithValue("@TELEFONE", funcionario.TELEFONE);
                    insertCommand.Parameters.AddWithValue("@EMAIL", funcionario.EMAIL);
                    insertCommand.Parameters.AddWithValue("@SENHA", funcionario.SENHA);

                    int rowsAffected = await insertCommand.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Funcionário cadastrado com sucesso!" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Erro ao cadastrar funcionário." });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (you can replace this with your logging mechanism)
                    _logger.LogError("Erro ao criar funcionário: " + ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno no servidor." });
                }
            }

        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;
        public RegisterController(ILogger<RegisterController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";



        [HttpPost]
        public async Task<ActionResult> CreateUsuario([FromBody] Usuarios usuario)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Verificar se o CPF já existe no banco
                    string checkQuery = "SELECT COUNT(1) FROM Usuarios WHERE CPF = @CPF";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@CPF", usuario.CPF);

                    int count = (int)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        // Se o CPF já existe, retorne BadRequest com mensagem
                        return BadRequest(new { message = "CPF já cadastrado." });
                    }

                    // Caso o CPF não exista, faça o INSERT no banco
                    string insertQuery = "INSERT INTO Usuarios (NOME, CPF, TELEFONE, EMAIL, SENHA) VALUES (@NOME, @CPF, @TELEFONE, @EMAIL, @SENHA)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@NOME", usuario.NOME);
                    insertCommand.Parameters.AddWithValue("@CPF", usuario.CPF);
                    insertCommand.Parameters.AddWithValue("@TELEFONE", usuario.TELEFONE);
                    insertCommand.Parameters.AddWithValue("@EMAIL", usuario.EMAIL);
                    insertCommand.Parameters.AddWithValue("@SENHA", usuario.SENHA);

                    int rowsAffected = await insertCommand.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Usuário cadastrado com sucesso!" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Erro ao cadastrar usuário." });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (you can replace this with your logging mechanism)
                    _logger.LogError("Erro ao criar usuário: " + ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno no servidor." });
                }
            }
        }

    }
}

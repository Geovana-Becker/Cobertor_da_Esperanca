using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginFuncionariosController : ControllerBase
    {
        private readonly ILogger<LoginFuncionariosController> _logger;
        public LoginFuncionariosController(ILogger<LoginFuncionariosController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        [HttpGet(Name = "GetFuncionarios")]
        public IEnumerable<Funcionarios> Get()
        {
            List<Funcionarios> funcionarios = new List<Funcionarios>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string querry = "SELECT * FROM Funcionarios";
                SqlCommand command = new SqlCommand(querry, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Funcionarios funcionario = new Funcionarios()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        NOME = reader["NOME"].ToString(),
                        CPF = reader["CPF"].ToString(),
                        CARGO = reader["CARGO"].ToString(),
                        TELEFONE = reader["TELEFONE"].ToString(),
                        EMAIL = reader["EMAIL"].ToString(),
                        SENHA = reader["SENHA"].ToString(),
                    };

                    funcionarios.Add(funcionario);
                }

                reader.Close();
            }

            return funcionarios;
        }



        [HttpPost]
        public ActionResult<bool> ValidarLogin([FromBody] Funcionarios funcionario)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Funcionarios WHERE CPF = @CPF AND SENHA = @SENHA";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CPF", funcionario.CPF);
                command.Parameters.AddWithValue("@SENHA", funcionario.SENHA);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    return Ok(true);
                }
            }
            return BadRequest(false);
        }
    }
}
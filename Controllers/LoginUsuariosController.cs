using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCC.Controllers
{
    [ApiController]
    [Route("LoginUsuarios")]
    public class LoginUsuariosController : ControllerBase
    {
        private readonly ILogger<LoginUsuariosController> _logger;
        public LoginUsuariosController(ILogger<LoginUsuariosController> logger)
        {
            _logger = logger;
        }

        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Banco_tcc;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        [HttpGet("{id}", Name = "GetUsuarioById")]
        public ActionResult<Usuarios> GetUsuarioById(int id)
        {
           
            if (id <= 0)
            {
                return BadRequest(new { message = "ID inválido." });
            }

            Usuarios usuario = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                
                string query = "SELECT * FROM Usuarios WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    usuario = new Usuarios()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        NOME = reader["NOME"].ToString(),
                        CPF = reader["CPF"].ToString(),
                        TELEFONE = reader["TELEFONE"].ToString(),
                        EMAIL = reader["EMAIL"].ToString(),
                        SENHA = reader["SENHA"].ToString(),
                        PONTUACAO = Convert.ToInt32(reader["PONTUACAO"]),
                    };
                }

                reader.Close();
            }

            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(usuario);  
        }


        [HttpPost]
        public ActionResult<Usuarios> ValidarLogin([FromBody] Usuarios usuario)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Usuarios WHERE CPF = @CPF AND SENHA = @SENHA";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CPF", usuario.CPF);
                command.Parameters.AddWithValue("@SENHA", usuario.SENHA);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    
                    var usuarioAutenticado = new Usuarios
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        NOME = reader["NOME"].ToString(),
                        CPF = reader["CPF"].ToString(),
                        TELEFONE = reader["TELEFONE"].ToString(),
                        EMAIL = reader["EMAIL"].ToString(),
                        SENHA = reader["SENHA"].ToString(),
                        PONTUACAO = Convert.ToInt32(reader["PONTUACAO"]),
                    };

                    return Ok(usuarioAutenticado); 
                }

                return BadRequest("Usuário ou senha incorretos.");
            }
        }


    

    }
}
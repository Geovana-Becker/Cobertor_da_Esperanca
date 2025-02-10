// window.onload = async function () {
//     console.log(localStorage);

//     const isAuthenticatedFuncionario = localStorage.getItem('funcionario_authenticated') === 'true';

//     const botaoLogin = document.getElementById("botao_login");
//     const botaoSair = document.getElementById("botao_sair");
//     const botaoEntrar2 = document.getElementById("botao-entrar2");
//     const pontuacaoElemento = document.getElementById("pontuacao");
//     const textopontuacaoElemento = document.getElementById("texto-pontuacao");
//     const recompensas = document.getElementById("cadastro-recompensas");
//     const userId = localStorage.getItem('userId'); // Obtém o ID do usuário
//     const gerenciamento = document.getElementById("gerenciamento-usuarios");
//     const recompensasResgatadas = document.getElementById("recompensas_resgatadas");

//     if (isAuthenticatedFuncionario) {
//         // Funcionário autenticado
//         botaoLogin.style.display = "none";
//         botaoSair.style.display = "block";
//         botaoEntrar2.style.display = "none";
//         recompensas.style.display = "block"; // Mostra cadastro de recompensas
//         gerenciamento.style.display = "block";
//          recompensasResgatadas.style.display = "none";

//     } else if (userId) {
//         try {
//             // Verifica a existência do usuário com o ID armazenado
//             const response = await fetch(`https://localhost:7177/LoginUsuarios/${userId}`);
//             console.log(response);

//             if (response.ok) {
//                 const data = await response.json();
//                 console.log(data);

//                 // Atualiza o estado da interface
//                 botaoLogin.style.display = "none";
//                 botaoSair.style.display = "block";
//                 botaoEntrar2.style.display = "none";
//                 textopontuacaoElemento.style.display = "block";
//                  recompensasResgatadas.style.display = "block";

//                 pontuacaoElemento.style.display = "block";


//                 pontuacaoElemento.textContent = data.pontuacao || '0'; // Exibe a pontuação (caso não tenha, mostra 0)

//                 if (isAuthenticated) {
//                     // Usuário comum autenticado
//                     botaoLogin.style.display = "none";
//                     botaoSair.style.display = "block";
//                     botaoEntrar2.style.display = "none";
//                     recompensas.style.display = "none";
//                     gerenciamento.style.display = "none";
//                      recompensasResgatadas.style.display = "block";

//                     pontuacaoElemento.style.display = "block";
//                 }
//             } else {
//                 throw new Error('Usuário não encontrado ou inválido');
//             }
//         } catch (error) {
//             console.error('Erro ao verificar usuário:', error);

//             // Redirecionar para a página de login em caso de erro
//             //window.location.href = 'login.html';
//         }
//     } else {
//         // Usuário não autenticado
//         botaoLogin.style.display = "block";
//         botaoSair.style.display = "none";
//         botaoEntrar2.style.display = "block";
//         textopontuacaoElemento.style.display = "none";

//         pontuacaoElemento.textContent = '';
//     }
// };
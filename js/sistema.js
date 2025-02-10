
function verificarAutenticacao2(event) {
    const isAuthenticated = localStorage.getItem('authenticated') === 'true';
    const isAuthenticatedFuncionario = localStorage.getItem('funcionario_authenticated') === 'true';

    if (!isAuthenticated && !isAuthenticatedFuncionario) {
        // Usuário não autenticado: exibe alerta e cancela a navegação
        alert('Você precisa estar logado para acessar a página de recompensas.');
        event.preventDefault(); // Evita que o link seja acessado
        return false;
    }

    // Usuário autenticado: permite a navegação
    return true;
}

function logout() {
    localStorage.removeItem('userId'); // Remove o userId ao deslogar
    window.location.href = 'login.html';
    localStorage.setItem('funcionario_authenticated', 'false');
    localStorage.setItem('authenticated', 'false');
}

window.onload = async function () {
    console.log(localStorage);

    const isAuthenticatedFuncionario = localStorage.getItem('funcionario_authenticated') === 'true';

    const botaoLogin = document.getElementById("botao_login");
    const botaoSair = document.getElementById("botao_sair");
    const botaoEntrar2 = document.getElementById("botao-entrar2");
    const pontuacaoElemento = document.getElementById("pontuacao");
    const textopontuacaoElemento = document.getElementById("texto-pontuacao");
    const recompensas = document.getElementById("cadastro-recompensas");
    const userId = localStorage.getItem('userId'); 
    const gerenciamento = document.getElementById("gerenciamento-usuarios");
    

    if (isAuthenticatedFuncionario) {
        // Funcionário autenticado
        botaoLogin.style.display = "none";
        botaoSair.style.display = "block";
        botaoEntrar2.style.display = "none";
        recompensas.style.display = "block"; 
        gerenciamento.style.display = "block";
        
    } else if (userId) {
        try {
            // Verifica a existência do usuário com o ID armazenado
            const response = await fetch(`https://localhost:7177/LoginUsuarios/${userId}`);
            console.log(response);

            if (response.ok) {
                const data = await response.json();
                console.log(data);

                // Atualiza o estado da interface
                botaoLogin.style.display = "none";
                botaoSair.style.display = "block";
                botaoEntrar2.style.display = "none";
                textopontuacaoElemento.style.display = "block";
                
               
                pontuacaoElemento.style.display = "block";


                pontuacaoElemento.textContent = data.pontuacao || '0'; // Exibe a pontuação (caso não tenha, mostra 0)

                
            } else {
                throw new Error('Usuário não encontrado ou inválido');
            }
        } catch (error) {
            console.error('Erro ao verificar usuário:', error);
            
        }
    } else {
        // Usuário não autenticado
        botaoLogin.style.display = "block";
        botaoSair.style.display = "none";
        botaoEntrar2.style.display = "block";
        textopontuacaoElemento.style.display = "none";

        pontuacaoElemento.textContent = '';
    }
};
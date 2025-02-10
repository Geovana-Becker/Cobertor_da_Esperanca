 document.addEventListener('DOMContentLoaded', function () {
            const userId = localStorage.getItem('userId'); // Recupera o ID do usuário do localStorage
            const isAuthenticatedFuncionario = localStorage.getItem('funcionario_authenticated') === 'true';

            // Verifica se o usuário está logado como funcionário
            if (isAuthenticatedFuncionario) {
                // Se o usuário for um funcionário, esconde os links de Home, Doações e Contato
                document.getElementById('home_menu').style.display = 'none';
                document.getElementById('doacao-menu').style.display = 'none';
                document.getElementById('contato-menu').style.display = 'none';
                document.getElementById('botao-contato1').style.display = 'none';
                document.getElementById('doacao12').style.display = 'none';
                document.getElementById('contato12').style.display = 'none';
            } else {
                // Se o usuário não for funcionário, mostra os links de Home, Doações e Contato
                document.getElementById('home_menu').style.display = 'block';
                document.getElementById('doacao-menu').style.display = 'block';
                document.getElementById('contato-menu').style.display = 'block';
                document.getElementById('botao-contato1').style.display = 'block';
                document.getElementById('doacao12').style.display = 'block';
                document.getElementById('contato12').style.display = 'block';
            }

            // Chama a função para carregar as recompensas
        });
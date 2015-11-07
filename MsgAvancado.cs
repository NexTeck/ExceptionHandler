using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExceptionHandler
{
    /*
     * MsgAvancado:
     * Serve para facilitar a exibição de mensagens para o usuário, sem que tenha 
     * que criar MessageBox gigantes para exibir mensagens simples, tornando o código mais limpo,
     * mas sua principal função é a exibição de erros. Onde pode filtrar os erros que serão exibidos
     * para o programador e o usuário com o campo exibeDetalhesDeErros, se for = false já não exibirá mais
     * os erros de programação para os usuários. Além de ajudar na exibição de erros, se o campo
     * salvarRelatoriosDeErros for true, então ele gera automaticamente um log que informa os erros 
     * acontecidos no programa para que possam ser analisados depois.
     * 
     * Essa classe só funciona com Windows Forms.
     * 
     * 
     * Andrei 07/11/2015
     * 
     * */

    /// <summary>
    /// Objeto estático usado para exibir mensagens de erro para os usuários
    /// Os erros exibidos irão ser exibidos se estiver configurado para ser exibido para
    /// programador ou usuário padrão. Erros graves serão armazenados em arquivos de log
    /// Andrei  27/10/2015
    /// </summary>
    public static class MsgAvancado
    {

        #region Atributos
        private static bool salvarRelatoriosDeErros = true;
        private static bool exibeDetalhesDeErros = true;
        // só pra saber se precisa reiniciar após salvar relatórios
        private static IntensidadeErro intensity;
        // fila de exceções 
        private static Queue<Exception> fila = new Queue<Exception>();

        /// <summary>
        /// Objeto que cria um thread pra tentar salvar o estado atual do sistema
        /// Leonardo 28/10/2015
        /// </summary>
        private static BackgroundWorker recover = new BackgroundWorker();
        /// <summary>
        /// O nome do programa para exibir as mensagens de erro
        /// </summary>
        public static string NomePrograma { get; private set; }

        /// <summary>
        /// Variável que indica se exibe erros de programação
        /// </summary>
        public static bool ExibeErrosProgramador { get; set; }

        /// <summary>
        /// Só pra não ficar salvando relatórios enquanto o sistema é desenvolvido
        /// Leonardo 28/10/2015
        /// </summary>
        public static bool SalvarRelatoriosDeErros
        {
            get { return salvarRelatoriosDeErros; }
            set { salvarRelatoriosDeErros = value; }
        }
        /// <summary>
        /// Define se devem ser exibidas as InnerExceptions da Exception
        /// Leonardo 28/10/2015
        /// </summary>
        public static bool ExibeDetalhesDeErros
        {
            get { return exibeDetalhesDeErros; }
            set
            {
                if (ExibeErrosProgramador && value)
                    exibeDetalhesDeErros = value;
                else
                    exibeDetalhesDeErros = false;
            }
        }
        #endregion

        /// <summary>
        /// Exibe uma mensagem de erro ao usuário e gera relatórios de erros, salvos num arquivo
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="ex">Exceção a ser relatada</param>
        /// <param name="mensagemUsuario">Mensagem a ser exibida ao usuário. Se o parâmetro não for passado, se for vazio, nulo,
        /// e a intensidade do erro for diferente de gravissimo, não será exibida a mensagem</param>
        /// <param name="intensidade">Intensidade do erro</param>
        public static void ExibirErro(Exception ex, string mensagemUsuario = "", IntensidadeErro intensidade = IntensidadeErro.Grave)
        {
            if (intensidade > intensity)
                intensity = intensidade;

            if (intensidade > 0 && salvarRelatoriosDeErros)
            {
                fila.Enqueue(ex);
                if (!recover.IsBusy)
                {
                    recover.DoWork += recover_DoWork;
                    recover.RunWorkerAsync();
                    recover.RunWorkerCompleted += recover_RunWorkerCompleted;
                }
            }

            string mensagem = mensagemUsuario;
            int numEx = 1;
            if (ExibeErrosProgramador)
            {
                mensagem += Environment.NewLine + "Exceção principal: " + ex.Message;
                if (ExibeDetalhesDeErros)
                {
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        mensagem += Environment.NewLine + "Exceção interna " + numEx++ + ": " + inner.Message;
                        inner = inner.InnerException;
                    }
                }
            }
            else
                if (intensidade == IntensidadeErro.Gravissimo)
                mensagem += Environment.NewLine + "Informe este código ao desenvolvedor do sistema: " + ((ex.HResult - 5) * 7);
            if (!string.IsNullOrEmpty(mensagem))
                MessageBox.Show(mensagem, NomePrograma, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (intensidade == IntensidadeErro.Gravissimo && !salvarRelatoriosDeErros)
                Application.Restart();
        }

        /// <summary>
        /// Exibe uma informação
        /// </summary>
        public static void ExibirInformacao(string mensagem)
        {
            MessageBox.Show(mensagem, NomePrograma, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Exibe um erro validação
        /// </summary>
        public static void ExibirErroValidacao(string mensagem)
        {
            MessageBox.Show(mensagem, NomePrograma, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Exibe um erro pergunta
        /// </summary>
        public static DialogResult ExibirPergunta(string mensagem)
        {
            return MessageBox.Show(mensagem, NomePrograma, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Evento executado de forma assíncrona que tenta salvar o relatório
        /// Leonardo Costa 30/10/2015
        /// </summary>
        private static void recover_DoWork(object sender, DoWorkEventArgs e)
        {
            ErroConfig erro = (ErroConfig)ControllerConfig.CarregarOuCriar(typeof(ErroConfig));
            while (fila.Count > 0)
                erro.AddError(fila.Dequeue());
            erro.Salvar();
        }

        /// <summary>
        /// Reinicia o sistema se a intensidade do erro é gravissima, assim que termina de salvar o relatório
        /// Leonardo Costa 30/10/2015
        /// </summary>
        private static void recover_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (fila.Count > 0)
                recover.RunWorkerAsync();
            else if (intensity == IntensidadeErro.Gravissimo)
                Application.Restart();
        }

        /// <summary>
        /// Configura o MsgAvancado e os relatórios de erros.
        /// Deve ser chamado no inicio do programa
        /// Andrei 07/11/2015
        /// </summary>
        /// <param name="nomePrograma"></param>
        public static void ConfigurarMsgAvancado(string nomePrograma, bool exibirErros = true, bool salvarErros= true, bool exibirDetalhes = true)
        {
            // Tratamento de exceções não tratadas e de exceções dos Threads dos Forms
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            Application.ThreadException += GlobalThreadExceptionHandler;
            //Define as configurações básicas
            NomePrograma = nomePrograma;
            ExibeErrosProgramador = exibirErros;
            SalvarRelatoriosDeErros = salvarErros;
            exibeDetalhesDeErros = exibirDetalhes;
        }

        private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            // Gera o relatório do erro e reinicia o sistema
            MsgAvancado.ExibirErro(e.Exception, "Ops, ocorreu um erro", MsgAvancado.IntensidadeErro.Gravissimo);
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // Tem um cast, pode dar erro, e se der erro, pode entrar num laço infinito de erros quando der cast de novo, então precisa de try..catch
                MsgAvancado.ExibirErro((Exception)e.ExceptionObject, "Ops, ocorreu um erro", MsgAvancado.IntensidadeErro.Gravissimo);
            }
            catch (Exception ex)
            {
                MsgAvancado.ExibirErro(ex, "Falha ao iniciar sistema. Contate um desenvolvedor.");
                // Finaliza o sistema da forma mais dramática possível, pra impedir qualquer outro erro
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Enumera a intensidade do erro, para definir o que deve ser feito
        /// Leonardo 28/10/2015
        /// </summary>
        public enum IntensidadeErro
        {
            /// <summary>
            /// Erro banal, não é necessário interromper a atividade do sistema
            /// Leonardo 28/10/2015
            /// </summary>
            Simples = 0,
            /// <summary>
            /// Salva tudo que puder
            /// Leonardo 28/10/2015
            /// </summary>
            Grave,
            /// <summary>
            /// Erro fatal, toda atividade do sistema deve ser interrompida imediatamente e o programador deve ser contatado
            /// Leonardo 28/10/2015
            /// </summary>
            Gravissimo
        }
    }
}
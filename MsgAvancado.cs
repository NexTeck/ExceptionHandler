using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExceptionHandler
{
    /// <summary>
    /// Objeto estático usado para exibir mensagens de erro para os usuários
    /// Os erros exibidos irão ser exibidos se estiver configurado para ser exibido para
    /// programador ou usuário padrão. Erros graves serão armazenados em arquivos de log
    /// Andrei  27/10/2015
    /// </summary>
    public static class MsgAvancado
    {

        #region Atributos

        /// <summary>
        /// O nome do programa para exibir as mensagens de erro
        /// </summary>
        public static string NomePrograma { get; set; }

        /// <summary>
        /// Variável que indica se exibe erros de programação
        /// </summary>
        public static bool ExibeErrosProgramador { get; set; }

        private static bool salvarRelatoriosDeErros = true;
        /// <summary>
        /// Só pra não ficar salvando relatórios enquanto o sistema é desenvolvido
        /// Leonardo 28/10/2015
        /// </summary>
        public static bool SalvarRelatoriosDeErros
        {
            get { return salvarRelatoriosDeErros; }
            set { salvarRelatoriosDeErros = value; }
        }

        private static bool exibeDetalhesDeErros = true;
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

        /// <summary>
        /// Enumera níveis de alertas em controles
        /// Leonardo Costa 30/10/2015
        /// </summary>
        public enum ControleBackgroundColor
        {
            /// <summary>
            /// Verde claro #80FF80, indica que tudo está OK
            /// </summary>
            Verde = 0,
            /// <summary>
            /// Amarelo #FFFF80, indica uma recomendação
            /// </summary>
            Amarelo,
            /// <summary>
            /// Vermelho #FF8080, indica uma obrigação
            /// </summary>
            Vermelho
        }

        // só pra saber se precisa reiniciar após salvar relatórios
        private static IntensidadeErro intensity;
        // fila de exceções 
        private static Queue<Exception> fila = new Queue<Exception>();

        /// <summary>
        /// Objeto que cria um thread pra tentar salvar o estado atual do sistema
        /// Leonardo 28/10/2015
        /// </summary>
        private static BackgroundWorker recover = new BackgroundWorker();

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
                System.Windows.Forms.MessageBox.Show(mensagem, NomePrograma, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            if (intensidade == IntensidadeErro.Gravissimo && !salvarRelatoriosDeErros)
                Application.Restart();
        }

        /// <summary>
        /// Altera a cor e o texto de um controle para exibir um alerta
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="ctrl">Controle a ser alterado</param>
        /// <param name="texto">Texto do controle</param>
        /// <param name="cbc">Nível do alerta</param>
        /// <returns>Retorna o controle alterado</returns>
        public static Control AlterarControle(Control ctrl, string texto = "", ControleBackgroundColor cbc = ControleBackgroundColor.Amarelo)
        {
            switch (cbc)
            {
                case ControleBackgroundColor.Verde:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(128, 255, 128);
                    break;
                case ControleBackgroundColor.Amarelo:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
                    break;
                case ControleBackgroundColor.Vermelho:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
                    break;
            }
            if (!string.IsNullOrEmpty(texto))
                ctrl.Text = texto;
            return ctrl;
        }

        /// <summary>
        /// Extensão que altera a cor e o texto de um controle para exibir um alerta
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="texto">Texto do controle</param>
        /// <param name="cbc">Nível do alerta</param>
        public static Control AlterarControleExt(this Control ctrl, string texto = "", ControleBackgroundColor cbc = ControleBackgroundColor.Amarelo)
        {
            switch (cbc)
            {
                case ControleBackgroundColor.Verde:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(128, 255, 128);
                    break;
                case ControleBackgroundColor.Amarelo:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
                    break;
                case ControleBackgroundColor.Vermelho:
                    ctrl.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
                    break;
            }
            if (!string.IsNullOrEmpty(texto))
                ctrl.Text = texto;
            return ctrl;
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
    }
}
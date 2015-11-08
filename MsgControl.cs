using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExceptionHandler
{
    /// <summary>
    /// Classe responsável por alterar controles (um por instância desta classe)
    /// e exibições de status de carregamento
    /// Andrei 07/11/2015
    /// </summary>
    public class MsgSingleControl
    {
        #region Campos e propriedades
        string textoExibido;
        bool textoMudou;
        bool tipoMudou;
        bool rodandoCarregamento;
        Timer timer;
        uint contador;
        Control control;
        TipoMensagem tipo;

        /// <summary>
        /// As cores padrão que são exibidas
        /// </summary>
        public static Color[] CoresTexto = new Color[]
        {
            Color.Black,
            Color.Blue,
            Color.Red,
            Color.Green
        };

        /// <summary>
        /// O controle vinculado a esse objeto
        /// </summary>
        public Control Control
        {
            get { return control; }
        }

        /// <summary>
        /// Get or Set se está exibindo uma mensagem de carregamento,
        /// é importante para não ocorrer erro de thread
        /// se setar como false mesmo com um carregamento, ele cancela
        /// a atualização quando o timer atualizar novamente
        /// </summary>
        public bool RodandoCarregamento
        {
            get { return rodandoCarregamento; }
            set
            {
                if (!rodandoCarregamento && value)
                {
                    IniciarExibicaoCarregamento();
                }
                else if (rodandoCarregamento && !value)
                {
                    rodandoCarregamento = false;
                }
            }
        }

        /// <summary>
        /// Propriedade que muda diretamente o texto que está sendo exibido pelo controle
        /// o tipo do texto continua sendo o mesmo do anterior
        /// </summary>
        public string TextoExibido
        {
            get { return textoExibido; }
            set
            {
                if (textoExibido != value)
                {
                    //Define o novo TextoExibido
                    if (value == null)
                        textoExibido = "";
                    else
                        textoExibido = value;
                    //Se estiver rodando um carregamento só define que o texto mudou
                    //Se não, muda o texto do controle
                    if (rodandoCarregamento)
                        textoMudou = true;
                    else
                        control.Text = textoExibido;
                }
            }
        }

        /// <summary>
        /// O tipo da mensagem que é exibido
        /// </summary>
        public TipoMensagem Tipo
        {
            get { return tipo; }
            set
            {
                if (tipo != value)
                {
                    tipo = value;
                    //Se estiver rodando um carregamento só define que o tipo mudou
                    //Se não, muda a cor do controle
                    if (rodandoCarregamento)
                        tipoMudou = true;
                    else
                        DefinirCor();
                }
            }
        }
        #endregion

        /// <summary>
        /// Declara um MsgControl
        /// </summary>
        /// <param name="control_">Controle vinculado a esse MsgControl</param>
        public MsgSingleControl(Control control_)
        {
            control = control_;
            CoresTexto[0] = control.ForeColor;
        }

        /// <summary>
        /// Define a mensagem que será exibida e o tipo dela
        /// </summary>
        public void DefinirMensagem(string texto, TipoMensagem tipo_)
        {
            Tipo = tipo_;
            TextoExibido = texto;
        }

        /// <summary>
        /// Inicia a exibição de carregamento do controle
        /// </summary>
        private void IniciarExibicaoCarregamento()
        {
            rodandoCarregamento = true;
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            timer.Enabled = true;
            timer.Start();
        }

        /// <summary>
        /// Finaliza a exibição de carregamento do controle
        /// </summary>
        private void CancelarExibicaoCarregamento()
        {
            textoMudou = false;
            RodandoCarregamento = false;
            contador = 0;
            timer.Tick -= timer_Tick;
            timer.Enabled = false;
            timer = null;
            control.Text = textoExibido;
            DefinirCor();
        }

        /// <summary>
        /// Define a cor do controle exibida
        /// </summary>
        private void DefinirCor()
        {
            control.ForeColor = CoresTexto[(int)Tipo];
        }

        /// <summary>
        /// Cada atualização do timer
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            if (!RodandoCarregamento)
            {
                CancelarExibicaoCarregamento();
                return;
            }
            if (textoMudou)
            {
                textoMudou = false;
                contador = 0;
                DefinirCor();
            }
            if (tipoMudou)
            {
                tipoMudou = false;
                contador = 0;
                DefinirCor();
            }
            contador++;
            uint a = contador / 5;
            if (a >= 4)
            {
                contador = 0;
            }
            string pontos = "";
            for (int i = 0; i < a; i++)
            {
                pontos += ".";
            }
            control.Text = textoExibido + pontos;
        }

        /// <summary>
        /// Define o tipo de mensagem que será exibido
        /// </summary>
        public enum TipoMensagem
        {
            /// <summary>
            /// Texto comum, o padrão é definido pela cor do texto do controle quando a instancia
            /// desta classe é construída
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Texto de informação que algum dado foi modificado ou que o processo foi concluido
            /// o padrão é azul
            /// </summary>
            Informacao,
            /// <summary>
            /// Texto que indica que ocorreu um erro, o padrão é vermelho
            /// </summary>
            Erro,
            /// <summary>
            /// Texo que indica um comentário, o padrão é verde
            /// </summary>
            Comentario
        }
    }

    /// <summary>
    /// Alerta: classe em processo de construção!
    /// Classe que agiliza a alteração de controles (vários por instância desta
    /// classe) para exibição de alertas e mensagens, com reversão automática
    /// da alteração durante um evento específico
    /// </summary>
    public class MsgMultControl
    {
        #region propriedades
        
        private bool toolTipAtivo = true;
        /// <summary>
        /// Propriedade que define quando um ToolTip é exibido nos controles associados
        /// </summary>
        public bool ToolTipAtivo
        {
            get { return toolTipAtivo; }
            set { toolTipAtivo = value; }
        }
        
        private ToolTip tooltip = new ToolTip();

        /// <summary>
        /// Define para qual cor o fundo do controle deve ser alterado
        /// Leonardo Costa 30/10/2015
        /// </summary>
        public enum BackColor
        {
            NaoMudar = 0,
            /// <summary>
            /// #80FF80
            /// </summary>
            Verde,
            /// <summary>
            /// #FFFF80
            /// </summary>
            Amarelo,
            /// <summary>
            /// #FF8080
            /// </summary>
            Vermelho
        }

        /// <summary>
        /// Define para qual cor o texto do controle deve ser alterado
        /// </summary>
        public enum ForeColor
        {
            NaoMudar = 0,
            Verde,
            Azul,
            Vermelho
        }

        /// <summary>
        /// Define em qual evento as cores de fundo e do texto do
        /// controle devem voltar ao normal
        /// </summary>
        public enum ReverterQuando
        {
            /// <summary>
            /// Não voltará ao normal automaticamente
            /// </summary>
            NaoReverter = 0,
            /// <summary>
            /// Volta ao normal quando chamado o método ReverterAgora()
            /// </summary>
            EuMandar,
            /// <summary>
            /// Volta ao normal quando o controle ganha foco
            /// </summary>
            EntrarNoFoco,
            /// <summary>
            /// Volta ao normal quando o texto do controle muda
            /// </summary>
            TextoMudar,
            /// <summary>
            /// Volta ao normal quando o controle é clicado pelo cursor
            /// </summary>
            Clicar
        }

        // Cores predefinidas para o texto e para o fundo
        private Color[] ForeColors = new Color[] { Color.Black, Color.Green, Color.Blue, Color.Red };
        private Color[] BackColors = new Color[]
        {
            Color.FromArgb(255, 255, 255),
            Color.FromArgb(128, 255, 128),
            Color.FromArgb(255, 255, 128),
            Color.FromArgb(255, 128, 128)
        };
        
        /// <summary>
        /// Armazena as propriedades originais do controle
        /// e quando estas foram alteradas
        /// </summary>
        private struct Reversor
        {
            public Color BackColor;
            public Color ForeColor;
            public string Texto;
            public ReverterQuando Quando;

            public Reversor(Color back, Color fore, string texto, ReverterQuando quando)
            {
                BackColor = back;
                ForeColor = fore;
                Texto = texto;
                Quando = quando;
            }
        }

        /// <summary>
        /// Armazena as alterações de todos os controles alterados, com opção de
        /// reversão das alterações
        /// </summary>
        /// <param type="int">HashCode do controle</param>
        /// <param type="Reversor">struct com as propriedades originais do controle</param>
        private Dictionary<int, Reversor> aReverter = new Dictionary<int, Reversor>();

        #endregion

        /// <summary>
        /// Configura as seguintes propriedades do ToolTip usado pelo MsgMultControl:
        /// AutomaticDelay, AutoPopDelay, BackColor, ForeColor, InitialDelay, IsBalloon,
        /// ReshowDelay, ShowAlways, ToolTipIcon, ToolTipTitle, UseAnimation e UseFading.
        /// </summary>
        /// <param name="tip">Exemplo de uso: ConfigurarToolTip(new ToolTip { ShowAlways = true, IsBalloon = true });</param>
        public void ConfigurarToolTip(ToolTip tip)
        {
            tooltip.AutomaticDelay = tip.AutomaticDelay;
            tooltip.AutoPopDelay = tip.AutoPopDelay;
            tooltip.BackColor = tip.BackColor;
            tooltip.ForeColor = tip.ForeColor;
            tooltip.InitialDelay = tip.InitialDelay;
            tooltip.IsBalloon = tip.IsBalloon;
            tooltip.ReshowDelay = tip.ReshowDelay;
            tooltip.ShowAlways = tip.ShowAlways;
            tooltip.ToolTipIcon = tip.ToolTipIcon;
            tooltip.ToolTipTitle = tip.ToolTipTitle;
            tooltip.UseAnimation = tip.UseAnimation;
            tooltip.UseFading = tip.UseFading;
        }

        /// <summary>
        /// Muda o texto, a cor de fundo e do texto do controle para o texto e as cores de
        /// antes de ser chamado o método AlterarControle()
        /// </summary>
        /// <param name="ctrl">Controle a ser restaurado. Se o método não foi chamado com este
        /// controle ou se o controle é nulo, ele não é modificado</param>
        /// <returns>Retorna o controle com as propriedades originais</returns>
        public Control ReverterAgora(Control ctrl)
        {
            int hash = ctrl.GetHashCode();

            if (!aReverter.ContainsKey(hash) || ctrl == null) return ctrl;

            if (aReverter[hash].Quando > 0)
            {
                ctrl.BackColor = aReverter[hash].BackColor;
                ctrl.ForeColor = aReverter[hash].ForeColor;
                ctrl.Text = aReverter[hash].Texto;
                switch (aReverter[hash].Quando)
                {
                    case ReverterQuando.EntrarNoFoco:
                        ctrl.GotFocus -= Evento;
                        break;
                    case ReverterQuando.TextoMudar:
                        ctrl.TextChanged -= Evento;
                        break;
                    case ReverterQuando.Clicar:
                        ctrl.Click -= Evento;
                        break;
                }
            }
            aReverter.Remove(hash);
            return ctrl;
        }

        /// <summary>
        /// Altera as propriedades de um controle para exibir algo ao usuário. Todos
        /// os parâmetros são de passagem opcional.
        /// </summary>
        /// <param name="ctrl">O controle a ser alterado</param>
        /// <param name="tip">Uma mensagem para ser exibida em um ToolTip. Se for
        /// vazio ou nulo não será exibido o ToolTip. Configure-o com o método
        /// ConfigurarToolTip()</param>
        /// <param name="texto">Texto para alterar o controle</param>
        /// <param name="corDoTexto">ForeColor do controle</param>
        /// <param name="corDeFundo">BackColor do controle</param>
        /// <param name="quando">Quando o controle volta ao normal</param>
        /// <returns>Retorna o controle com as novas propriedades</returns>
        public Control AlterarControle(Control ctrl, string tip = "", string texto = "", ForeColor corDoTexto = ForeColor.NaoMudar,
            BackColor corDeFundo = BackColor.NaoMudar, ReverterQuando quando = ReverterQuando.NaoReverter)
        {
            if (ctrl == null) return null;

            int hash = ctrl.GetHashCode();

            if (aReverter.ContainsKey(hash))
                ReverterAgora(ctrl);

            if (quando > 0)
            {
                aReverter.Add(ctrl.GetHashCode(), new Reversor(ctrl.BackColor, ctrl.ForeColor, ctrl.Text, quando));
                switch (quando)
                {
                    case ReverterQuando.Clicar:
                        ctrl.Click += Evento;
                        break;
                    case ReverterQuando.EntrarNoFoco:
                        ctrl.GotFocus += Evento;
                        break;
                    case ReverterQuando.TextoMudar:
                        ctrl.TextChanged += Evento;
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(texto))
                ctrl.Text = texto;

            if (!string.IsNullOrWhiteSpace(tip) && ToolTipAtivo)
                tooltip.SetToolTip(ctrl, tip);

            if (corDoTexto > 0)
                ctrl.ForeColor = ForeColors[(int)corDoTexto];
            if (corDeFundo > 0)
                ctrl.BackColor = BackColors[(int)corDeFundo];

            return ctrl;
        }
        
        /// <summary>
        /// Evento chamado para alterar o controle
        /// </summary>
        private void Evento(object sender, EventArgs e)
        {
            ReverterAgora((Control)sender);
        }
        
        public MsgMultControl()
        {
            
        }
    }
}

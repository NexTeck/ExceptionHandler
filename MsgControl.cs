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
    /// Classe responsável por mostrar mensagens em controles e exibições de 
    /// status de carregamento
    /// Andrei 07/11/2015
    /// </summary>
    public class MsgControl
    {
        #region Campos e propriedades
        string textoExibido;
        bool textoMudou;
        bool tipoMudou;
        bool rodandoCarregamento;
        Timer timer;
        int contador;
        Control control;
        TipoMensagem tipo;

        /// <summary>
        /// As cores padrão que são exibidas
        /// </summary>
        public static Color[] Cores = new Color[] 
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
                    //Se estiver rodando um carregamento só define que o texto mudou
                    //Se não, muda o texto do controle
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
        public MsgControl(Control control_)
        {
            control = control_;
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
            control.ForeColor = Cores[(int)Tipo];
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
            contador++;
            int a = contador / 5;
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
            /// Texto comum, o padrão é preto
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
}


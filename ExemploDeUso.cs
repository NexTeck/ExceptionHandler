using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExceptionHandler;

namespace ExceptionHandler
{
    public partial class ExemploDeUso : Form
    {
        MsgSingleControl msg;

        public ExemploDeUso()
        {
            InitializeComponent();

            MsgAvancado.ExibirErroValidacao("Você errou em alguma parte");
            MsgAvancado.ExibirInformacao("Pare de errar");
            var r = MsgAvancado.ExibirPergunta("Você vai parar de errar?");
            if (r == System.Windows.Forms.DialogResult.Yes)
                MsgAvancado.ExibirInformacao("Que bom cara");
            else
                MsgAvancado.ExibirErroValidacao("Você deve parar de errar!");


            //Define a MsgSingleControl com a atualização de um label
            msg = new MsgSingleControl(label1);
            msg.TextoExibido = "Texto que não terá tempo suficiente para ser exibido";
            //Define que ele está rodando async
            //Não pode setar durante a thread async
            msg.RodandoCarregamento = true;


            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //O texto pode ser mudado diretamente pela propriedade quando roda async
            //Des de que a propriedade RodandoCarregamento esteja setado true
            msg.TextoExibido = "Inicio da espera";
            System.Threading.Thread.Sleep(1000);
            msg.DefinirMensagem("Esperando", MsgSingleControl.TipoMensagem.Normal);
            System.Threading.Thread.Sleep(10000);
            msg.DefinirMensagem("Espera finalizada", MsgSingleControl.TipoMensagem.Informacao);
            //Pode definir a propriedade RodandoCarregamento como false diretamente pela 
            //Thread async, porém uma vez mudado não se pode mudar os textos por causa
            //que há a possibilidade de ocorrer um erro
            msg.RodandoCarregamento = false;
        }
    }
}

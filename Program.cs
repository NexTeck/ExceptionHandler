using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExceptionHandler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Tratamento de exceções não tratadas e de exceções dos Threads dos Forms
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            Application.ThreadException += GlobalThreadExceptionHandler;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form());
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
    }
}

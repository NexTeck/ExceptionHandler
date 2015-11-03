using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHandler
{
    /// <summary>
    /// Classe que trata e gera relatório de exceções não tratadas em Tasks
    /// Leonardo Costa 30/10/2015
    /// </summary>
    static class TaskExHandler
    {
        /// <summary>
        /// Inicia uma nova Task e adiciona um handler para exceções não tratadas.
        /// Se erros de programador não são exibidos, não será exibida mensag
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="act">O delegate a ser executado de forma assíncrona</param>
        /// <param name="mensagem">Mensagem a ser exibida caso ocorra uma exceção na Task.
        /// Se o parâmetro não for passado, se for vazio, nulo, e não são exibidos erros de programador
        /// não será exibida mensagem ao usuário.</param>
        /// <returns>Retorna a Task iniciada com o handler</returns>
        public static Task StarNew(Action act, string mensagem = "")
        {
            var task = Task.Factory.StartNew(act);
            task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        /// <summary>
        /// Extensão que adiciona um Handler para gerar relatórios de exceções não tratadas
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <returns>Retorna a Task com o Handler</returns>
        public static Task AddExceptionHandlerExt(this Task t)
        {
            t.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            return t;
        }

        /// <summary>
        /// Método que adiciona um Handler para gerar relatórios de exceções não tratadas
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="t">Task que receberá o Handler</param>
        /// <returns>Retorna a Task com o Handler</returns>
        public static Task AddExceptionHandler(Task t)
        {
            t.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            return t;
        }

        /// <summary>
        /// Gera o relatório da exceção da Task
        /// Leonardo Costa 30/10/2015
        /// </summary>
        /// <param name="task">Task que foi interrompida por uma exceção não tratada</param>
        private static void ExceptionHandler(Task task)
        {
            MsgAvancado.ExibirErro(task.Exception);
        }
    }
}

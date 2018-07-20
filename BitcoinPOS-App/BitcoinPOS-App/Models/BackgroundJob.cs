using System;
using System.Diagnostics;
using System.Threading;

namespace BitcoinPOS_App.Models
{
    public class BackgroundJob
    {
        private Thread _thread;

        public BackgroundJob(Thread thread)
        {
            _thread = thread;
        }

        public void Cancel()
        {
            if (_thread != null)
            {
                try
                {
                    _thread.Abort();
                    _thread = null;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERRO: Falha ao cancelar thread: " + e);
                }
            }
        }
    }
}
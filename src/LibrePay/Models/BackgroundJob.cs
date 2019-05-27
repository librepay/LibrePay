using System;
using System.Diagnostics;
using System.Threading;

namespace LibrePay.Models
{
    public class BackgroundJob
    {
        private Thread _thread;

        public BackgroundJob(Thread thread)
        {
            Debug.WriteLine("[INFO] Creating new background job.");
            _thread = thread;
        }

        public void Cancel()
        {
            Debug.WriteLine("[INFO] Ending background job.");

            if (_thread != null)
            {
                try
                {
                    _thread.Abort();
                    _thread = null;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR: Failure to cancel thread: " + e);
                }
            }
        }
    }
}

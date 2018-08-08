using System.Threading;
using BitcoinPOS_App.Models;
using Xunit;

namespace BitcoinPOS_App.UnitTests.Models
{
    public class BackgroundJobTests
    {
        [Fact]
        public void CanCancelAThreadMultipleTimes()
        {
            var th = new Thread(() => { });
            var bj = new BackgroundJob(th);

            bj.Cancel();
            bj.Cancel();
        }
    }
}

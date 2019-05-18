using System.Threading;
using LibrePay.Models;
using Xunit;

namespace LibrePay.UnitTests.Models
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

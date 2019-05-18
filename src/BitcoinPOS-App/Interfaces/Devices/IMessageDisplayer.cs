using System.Threading.Tasks;

namespace BitcoinPOS_App.Interfaces.Devices
{
    public interface IMessageDisplayer
    {
        Task ShowMessageAsync(string text);
    }
}

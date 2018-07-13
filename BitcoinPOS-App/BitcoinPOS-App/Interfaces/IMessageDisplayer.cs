using System.Threading.Tasks;

namespace BitcoinPOS_App.Interfaces
{
    public interface IMessageDisplayer
    {
        Task ShowMessageAsync(string text);
    }
}

using System.Threading.Tasks;

namespace LibrePay.Interfaces.Devices
{
    public interface IMessageDisplayer
    {
        Task ShowMessageAsync(string text);
    }
}

using Quantum.ViewModels;
using System;
using System.Threading.Tasks;
using WebAPI.Models.DTO;

namespace Quantum.Service
{
    public interface IDialogService
    {
        Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel, HubConnectionManager hubConnectionManager, ChatDTO chatDTO);
    }
}

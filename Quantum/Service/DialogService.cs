using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Quantum.ViewModels;
using Quantum.Views;
using WebAPI.Models.DTO;

namespace Quantum.Service
{
    public class DialogService : IDialogService
    {
        public async Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel, HubConnectionManager hubConnectionManager, ChatDTO chatDTO)
        {
            var dialogWindow = new DialogWindow();
            dialogWindow.DataContext = new DialogWindowViewModel(dialogWindow, mainWindowViewModel, homeViewModel,hubConnectionManager,  chatDTO);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return await dialogWindow.ShowDialog<string>(desktop.MainWindow);
            }

            return null;
        }
    }
}
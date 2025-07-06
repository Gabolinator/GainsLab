using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace GainsLab.Models.Core.LifeCycle;

public interface IAppLifeCycle
{
   
    public event Action onAppStart;
    public event Func<Task>? onAppStartAsync;
    public event Action onAppExit;
    public event Func<Task>? onAppExitAsync;
    public Task InitializeAsync( IServiceProvider serviceProvider, IApplicationLifetime? lifetime);
    
    public Task OnStartAppAsync();


    public Task OnExitAppAsync();
}
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PollingEngine
{
    [RunInstaller(true)]
    public class WinServiceInstaller : Installer
    {
        private readonly ServiceProcessInstaller serviceProcessInstaller;
        private readonly ServiceInstaller serviceInstaller;

        public WinServiceInstaller()
        {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            //serviceProcessInstaller.Username = "PETER-PC\\Peter";
            //serviceProcessInstaller.Password = null;

            serviceInstaller.Description = WinService.Description;
            serviceInstaller.DisplayName = WinService.DisplayName;
            serviceInstaller.ServiceName = WinService.ServiceName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.AddRange(new Installer[]
            {
                serviceProcessInstaller,
                serviceInstaller,
            });
        }
    }
}

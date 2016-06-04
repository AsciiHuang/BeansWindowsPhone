using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Xna.Framework;

namespace Beans
{
    public class XNAFrameworkDispatcherService : IApplicationService
    {
        private DispatcherTimer m_FrameworkDispatcherTimer;

        public XNAFrameworkDispatcherService()
        {
            m_FrameworkDispatcherTimer = new DispatcherTimer();
            m_FrameworkDispatcherTimer.Tick += new EventHandler(OnFrameworkDispatcherTimerTick);
            m_FrameworkDispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
        }

        void IApplicationService.StartService(ApplicationServiceContext context) { m_FrameworkDispatcherTimer.Start(); }
        void IApplicationService.StopService() { m_FrameworkDispatcherTimer.Stop(); }
        void OnFrameworkDispatcherTimerTick(object sender, EventArgs e) { FrameworkDispatcher.Update(); }
    }
}

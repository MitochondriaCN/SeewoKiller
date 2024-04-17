using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SeewoKiller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly NotifyIcon nicon;
        readonly Timer timer;

        public MainWindow()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;

            Settings.LoadSettings();

            //通知栏图标
            nicon = new NotifyIcon();
            nicon.Text = "析龌管家";
            nicon.Icon = new System.Drawing.Icon("icon.ico");
            nicon.DoubleClick += Nicon_DoubleClick;
            nicon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem("退出",(x,y)=>{System.Windows.Application.Current.Shutdown(); })
            });
            nicon.Visible = true;

            //开机队列
            RefreshQueueListBox();
            if (Settings.IsStartupQueueExecutable)
            {
                foreach (var v in Settings.StartupQueue)
                {
                    try
                    {
                        v.Execute();
                    }
                    catch (Exception e)
                    {
                        nicon.ShowBalloonTip(1000, "析龌管家开机队列执行失败", "执行“" + v.ToString() + "”时失败，错误日志已输出到软件目录。", ToolTipIcon.Error);
                        string logname = "errlog-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        File.WriteAllText(logname, e.Message + "\n" + e.StackTrace);
                        nicon.BalloonTipClicked += new EventHandler((x, y) => { Process.Start(logname); });
                        //注意，每catch到一个Action的异常，就往里放一个打开日志的匿名方法，也就是说，点击nicon后会打开所有错误日志
                    }
                }
            }

            //定时器
            //定时器功能写死，不给用户定制的空间
            timer = new Timer(60000);
            timer.Elapsed += Timer_Elapsed;
            if (Settings.IsTimerEnabled)
                timer.Start();
        }

        /// <summary>
        /// 计时器方法。该方法在另一线程。
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //非周一8:40, 9:30, 10:25, 11:30
            if (((e.SignalTime.Hour == 8 && e.SignalTime.Minute == 40) ||
                (e.SignalTime.Hour == 9 && e.SignalTime.Minute == 30) ||
                (e.SignalTime.Hour == 10 && e.SignalTime.Minute == 25) ||
                (e.SignalTime.Hour == 11 && e.SignalTime.Minute == 30))
                && e.SignalTime.DayOfWeek != DayOfWeek.Monday)
            {
                Input.Keyboard.Press(System.Windows.Input.Key.LWin);
                Input.Keyboard.Press(System.Windows.Input.Key.D);
                Input.Keyboard.Release(System.Windows.Input.Key.LWin);
                Input.Keyboard.Release(System.Windows.Input.Key.D);
            }

            //周一8:25, 9:15, 10:05, 11:15
            if (((e.SignalTime.Hour == 8 && e.SignalTime.Minute == 25) ||
                (e.SignalTime.Hour == 9 && e.SignalTime.Minute == 15) ||
                (e.SignalTime.Hour == 10 && e.SignalTime.Minute == 05) ||
                (e.SignalTime.Hour == 11 && e.SignalTime.Minute == 15))
                && e.SignalTime.DayOfWeek == DayOfWeek.Monday)
            {
                Input.Keyboard.Press(System.Windows.Input.Key.LWin);
                Input.Keyboard.Press(System.Windows.Input.Key.D);
                Input.Keyboard.Release(System.Windows.Input.Key.LWin);
                Input.Keyboard.Release(System.Windows.Input.Key.D);
            }

            //15:45, 16:35, 17:45
            if ((e.SignalTime.Hour == 15 && e.SignalTime.Minute == 45) ||
                (e.SignalTime.Hour == 16 && e.SignalTime.Minute == 35) ||
                (e.SignalTime.Hour == 17 && e.SignalTime.Minute == 45))
            {
                Input.Keyboard.Press(System.Windows.Input.Key.LWin);
                Input.Keyboard.Press(System.Windows.Input.Key.D);
                Input.Keyboard.Release(System.Windows.Input.Key.LWin);
                Input.Keyboard.Release(System.Windows.Input.Key.D);
            }
        }

        private void Nicon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
            
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Settings.StartupQueue.Remove(lstStartupQueue.SelectedItem as Action);
            Settings.SaveStartupQueue();
            RefreshQueueListBox();
        }

        private void btnOpenHotspot_Click(object sender, RoutedEventArgs e)
        {
            pupHotspot.IsOpen = true;
        }

        private void btnHotspotConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbHotspotSSID.Text) || string.IsNullOrWhiteSpace(txbHotspotPasswd.Text))
            {
                System.Windows.MessageBox.Show("参数未全部填写。", "错误");
                return;
            }

            Settings.StartupQueue.Add(new HotspotAction(
                cmbHotspotAction.SelectedIndex == 0 ? HotspotAction.HotspotStatus.On : HotspotAction.HotspotStatus.Off,
                txbHotspotSSID.Text, txbHotspotPasswd.Text));
            Settings.SaveStartupQueue();

            pupHotspot.IsOpen = false;
            txbHotspotPasswd.Text = "";
            txbHotspotSSID.Text = "";
            RefreshQueueListBox();
        }

        private void RefreshQueueListBox()
        {
            lstStartupQueue.ItemsSource = new List<Action>();
            lstStartupQueue.ItemsSource = Settings.StartupQueue;
            //大道至简
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(this.WindowState==WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void btnResolutionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbDPI.Text))
            {
                System.Windows.MessageBox.Show("参数未全部填写。", "错误");
                return;
            }

            Settings.StartupQueue.Add(new ResolutionAction(
                uint.Parse(txbDPI.Text),
                uint.Parse(cmbResolution.Text.Split('×')[0]),
                uint.Parse(cmbResolution.Text.Split('×')[1])));
            Settings.SaveStartupQueue();
            RefreshQueueListBox();
            pupResolution.IsOpen = false;
        }

        private void btnChangeResolution_Click(object sender, RoutedEventArgs e)
        {
            pupResolution.IsOpen = true;
        }

        private void btnSmartDesktop_Click(object sender, RoutedEventArgs e)
        {
            pupSmartDesktop.IsOpen = true;
        }

        private void btnSmartDesktopConfirm_Click(object sender, RoutedEventArgs e)
        {
            Settings.StartupQueue.Add(new SmartDesktopAction(ckbIsNewsEnabled.IsChecked.Value, ckbCustomWallpaper.IsChecked.Value));
            Settings.SaveStartupQueue();
            RefreshQueueListBox();
            pupSmartDesktop.IsOpen = false;
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private void btnNotification_Click(object sender, RoutedEventArgs e)
        {
            pupNotification.IsOpen = true;
        }

        private void btnNotificationConfirm_Click(object sender, RoutedEventArgs e)
        {
            new Notification(txbNotification.Text).Show();
            pupNotification.IsOpen = false;
        }
    }
}

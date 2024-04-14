using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeewoKiller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon nicon;

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
            Settings.StartupQueue.Add(new SmartDesktopAction(ckbIsNewsEnabled.IsChecked.Value));
            Settings.SaveStartupQueue();
            RefreshQueueListBox();
            pupSmartDesktop.IsOpen = false;
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }
    }
}

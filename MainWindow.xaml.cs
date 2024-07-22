using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;


namespace Keep_Asleep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
            Int32 Flags);

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int WM_POWERBROADCAST = 0x0218;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        private bool isLaptopLidOpen = true;
        private uint num_sleep = 0;

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    isLaptopLidOpen = ps.Data != 0;
                    if (isLaptopLidOpen)
                    {
                        UpdateTrayText();
                    }
                }
            }
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            RegisterForPowerNotifications();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void RegisterForPowerNotifications()
        {
            IntPtr handle = new WindowInteropHelper(Application.Current.Windows[0]).Handle;
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
                 ref GUID_LIDSWITCH_STATE_CHANGE,
                 DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_POWERBROADCAST:
                    OnPowerBroadcast(wParam, lParam);
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void UpdateTrayText()
        {
            TBIconTBox.Text = "Keep Asleep V"
                + Assembly.GetExecutingAssembly().GetName().Version.ToString()
                + "\r\nRe-Sleep Count: "
                + num_sleep.ToString();
        }
        [STAThread]
        private void ReSleep_Thread()
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                if (isLaptopLidOpen)
                {
                    Thread.Sleep(3000);
                }
                else
                {
                    Thread.Sleep(2000);
                    // Wait a bit and recheck if user just opened the lid
                    // Recovery mode--if user holds down insert key
                    if (!isLaptopLidOpen)
                    {
                        num_sleep++;
                        // Turn off display -> retrigger modern standby
                        SendMessage(0xFFFF, 0x112, 0xF170, 2);
                    }
                }
            }
        }
        public MainWindow()
        {
            WindowState = WindowState.Minimized;
            InitializeComponent();
            
            this.SourceInitialized += MainWindow_SourceInitialized;
            UpdateTrayText();
            
            new Thread(() => ReSleep_Thread()).Start();
        }
        private void ExitBtn_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}

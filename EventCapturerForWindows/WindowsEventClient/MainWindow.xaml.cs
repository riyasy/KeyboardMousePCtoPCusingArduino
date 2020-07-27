using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GlobalLowLevelHooks;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Timers;

namespace WindowsEventClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MouseHook mouseHook;
        KeyboardHook keyboardHook;

        private static System.Timers.Timer mouseMoveRefreshTimer;
        private static SerialPort serialPort;
        private int lastMouseX = -1000;
        private int lastMouseY = -1000;
        private bool mouseMoved = false;
        private int latestMouseX = 0;
        private int latestMouseY = 0;
        private bool isPortOpened = false;

        public MainWindow()
        {
            InitializeComponent();
            

            Application.Current.Exit += ApplicationExit;
            
            mouseHook = new MouseHook();
            keyboardHook = new KeyboardHook();

            mouseHook.MouseMove += MouseHook_MouseMove;
            mouseHook.LeftButtonDown += MouseHook_LeftButtonDown;
            mouseHook.LeftButtonUp += MouseHook_LeftButtonUp;
            mouseHook.RightButtonDown += MouseHook_RightButtonDown;
            mouseHook.RightButtonUp += MouseHook_RightButtonUp;
            mouseHook.Install();

            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.KeyUp += KeyboardHook_KeyUp;
            keyboardHook.Install();            

            ((INotifyCollectionChanged)eventListBox.Items).CollectionChanged += ListView_CollectionChanged;

            mouseMoveRefreshTimer = new System.Timers.Timer(20);
            mouseMoveRefreshTimer.Elapsed += SendMouseMove;
            mouseMoveRefreshTimer.AutoReset = true;
            mouseMoveRefreshTimer.Enabled = true;
        }

        int f10Count = 0;

        private void KeyboardHook_KeyUp(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.F10)
            {
                f10Count++;
                if (f10Count == 5)
                {
                    keyboardHook.DisablePassThrough = !keyboardHook.DisablePassThrough;
                    mouseHook.DisablePassThrough = !mouseHook.DisablePassThrough;
                }
            }
            else
            {
                f10Count = 0;
            }

            eventListBox.Items.Add("KeyboardHook_KeyUp " + key.ToString());
        }

        private void KeyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            eventListBox.Items.Add("KeyboardHook_KeyDown " + key.ToString());
        }

        private void SendMouseMove(object sender, ElapsedEventArgs e)
        {
            if (mouseMoved)
            {
                if (lastMouseX != -1000)
                {
                    int deltaX = latestMouseX - lastMouseX;
                    int deltaY = latestMouseY - lastMouseY;
                    string mouseMoveData = string.Format("e{0:00000;-0000;00000}{1:00000;-0000;00000}z", deltaX, deltaY);

                    Dispatcher.Invoke((Action)delegate () {
                        eventListBox.Items.Add("MouseHook_MouseMove :" + mouseMoveData);
                        SerialWrite(mouseMoveData);
                    });                    
                }
                lastMouseX = latestMouseX;
                lastMouseY = latestMouseY;
                mouseMoved = false;
            }
        }

        private void MouseHook_MouseMove(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            mouseMoved = true;
            latestMouseX = mouseStruct.pt.x;
            latestMouseY = mouseStruct.pt.y;
        }

        private void MouseHook_LeftButtonDown(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            SerialWrite("az");
            eventListBox.Items.Add("MouseHook_LeftButtonDown " + ToString(mouseStruct));
        }



        private void MouseHook_LeftButtonUp(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            SerialWrite("bz");
            eventListBox.Items.Add("MouseHook_LeftButtonUp " + ToString(mouseStruct));
        }

        private void MouseHook_RightButtonDown(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            eventListBox.Items.Add("MouseHook_RightButtonDown " + ToString(mouseStruct));
        }

        private void MouseHook_RightButtonUp(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            eventListBox.Items.Add("MouseHook_RightButtonUp " + ToString(mouseStruct));
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            mouseHook.MouseMove -= MouseHook_MouseMove;
            mouseHook.LeftButtonDown -= MouseHook_LeftButtonDown;
            mouseHook.LeftButtonUp -= MouseHook_LeftButtonUp;
            mouseHook.RightButtonDown -= MouseHook_RightButtonDown;
            mouseHook.RightButtonUp -= MouseHook_RightButtonUp;
            mouseHook.Uninstall();

            keyboardHook.KeyDown -= KeyboardHook_KeyDown;
            keyboardHook.KeyUp -= KeyboardHook_KeyUp;
            keyboardHook.Uninstall();

            if (isPortOpened)
            {
                serialPort.Close();
            }

            mouseMoveRefreshTimer.Stop();
            mouseMoveRefreshTimer.Dispose();
        }

        private string ToString(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            return String.Format("x {0} y {1}", mouseStruct.pt.x, mouseStruct.pt.y);
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                eventListBox.ScrollIntoView(e.NewItems[0]);
            }
        }

        private void SerialWrite(string data)
        {
            if (isPortOpened)
            {
                serialPort.Write(data);
            }
            
        }
        private void btnPortOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPort = new SerialPort(textBoxPort.Text, 115200, Parity.None, 8, StopBits.One);
                serialPort.Open();
                isPortOpened = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LetsPlayTimer_Single
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _lpTimer;
        private Stopwatch _lpStopwatch;
        private volatile bool _timing;
        private TimeSpan _setTime;
        private DateTime _timerStart;
        private bool _timerYellow;
        private bool _timerRed;        
        private Functions.KeyboardHook _keyHook;
        private Functions.KeyboardHook.VKeys _hotKey;
        private Properties.Settings appSets;
        private int counter;

        public MainWindow()
        {
            InitializeComponent();
            appSets = new Properties.Settings();
            initTimer();
            _keyHook = new Functions.KeyboardHook();
            _hotKey = Functions.KeyboardHook.VKeys.NUMPAD0;
            foreach (var item in (Functions.KeyboardHook.VKeys[]) Enum.GetValues(typeof(Functions.KeyboardHook.VKeys)))
            {
                keySelector.Items.Add(item);
            }

            if ((keySelector.SelectedIndex = appSets.key) >= 0)
                {
                    keyHookCheck.IsChecked = appSets.keyHooked;
                }
        }

        private void initTimer()
        {
            _lpTimer = new DispatcherTimer();
            _lpTimer.Interval = new TimeSpan(1000000);
            _lpStopwatch = new Stopwatch();
            _lpTimer.Tick += lpTimer_Tick;
            setTime(appSets._setTime);
            _timing = false;
        }

        void lpTimer_Tick(object sender, EventArgs e)
        {
            
            updateUI(_lpStopwatch.Elapsed);
        }

        private void updateUI(TimeSpan time)
        {
            timerTextBlock.Text = time.ToString(@"hh\:mm\:ss\.f");
            //if (time.TotalSeconds < timeProgess.Maximum)
            //{
            //    timeProgess.Value = time.TotalSeconds;
            //}
            if ((_setTime - time) < (new TimeSpan(0,3,0)))
            {
                if ((_setTime - time) < (new TimeSpan(0)))
                {
                    setTimerRed();
                }
                else
                {
                    setTimerYellow();
                }
            }
        }

        private void setTimerYellow()
        {
            if (!_timerYellow)
            {
                timerTextBlock.Foreground = Brushes.Yellow;
                _timerYellow = true;
                _timerRed = false;
            }
        }

        private void setTimerGreen()
        {
                timerTextBlock.Foreground = Brushes.LightGreen;
                _timerRed = false;
                _timerYellow = false;
        }

        private void setTimerRed()
        {
            if (!_timerRed)
            {
                timerTextBlock.Foreground = Brushes.Red;
                _timerRed = true;
                _timerYellow = false;
            }
        }

        private void timerStart()
        {
            _lpTimer.Start();
            _lpStopwatch.Start();
            _timerStart = DateTime.Now;
            _timing = true;
            setTimerGreen();
            startButton.Content = "Stop";
        }

        private void timerStop()
        {
            _lpTimer.Stop();
            _lpStopwatch.Stop();
            _lpStopwatch.Reset();
            _timing = false;
            startButton.Content = "Start";
        }

        private void switchTiming()
        {
            if (_timing)
            {
                timerStop();
            }
            else
            {
                timerStart();
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            switchTiming();
        }

        private void minGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            minElp.Stroke = Brushes.White;
            minLbl.Foreground = Brushes.White;
        }

        private void minGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            minElp.Stroke = Brushes.DimGray;
            minLbl.Foreground = Brushes.DimGray;
        }

        private void exitGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            exitElp.Stroke = Brushes.White;
            exitLbl.Foreground = Brushes.White;
        }

        private void exitGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            exitElp.Stroke = Brushes.DimGray;
            exitLbl.Foreground = Brushes.DimGray;
        }

        private void minGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void exitGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void setTime(TimeSpan time)
        {
            if (time < new TimeSpan(0))
            {
                _setTime = new TimeSpan(0);
            }
            else
            {
                _setTime = time;
            }
            setTimerGreen();
            timerTextBlock.Text = _setTime.ToString(@"hh\:mm\:ss\.f");
        }

        private void timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setTime(new TimeSpan(0, (int)e.NewValue, 0));
        }

        private void keyHookCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (keyHookCheck.IsChecked.Value)
            {
                if (!_keyHook.IsInstalled)
                {
                    _keyHook.Install();
                    _keyHook.KeyDown += _keyHook_KeyDown;
                }
            }
            else
            {
                if (_keyHook.IsInstalled)
                {
                    _keyHook.Uninstall();
                    _keyHook.KeyDown -= _keyHook_KeyDown;
                }
            }
        }

        void _keyHook_KeyDown(Functions.KeyboardHook.VKeys key)
        {
            if (key == _hotKey)
            {
                switchTiming();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _hotKey = (Functions.KeyboardHook.VKeys)e.AddedItems[0];
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_timing)
            {
                _lpTimer.Stop();
            }
            timerTextBlock.Text = _setTime.ToString(@"hh\:mm\:ss\.f");
            ((Rectangle)sender).Opacity = 0.2;
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Rectangle)sender).Opacity = 0;
            if (_timing)
            {
                _lpTimer.Start();
            }
        }

        private void hRect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_timing)
            {
                if (_setTime.Hours != 0)
                {
                    setTime(_setTime + (new TimeSpan(e.Delta / 120, 0, 0)));
                }
                else if (e.Delta > 0)
                {
                    setTime(_setTime + (new TimeSpan(e.Delta / 120, 0, 0)));
                }
            }
        }

        private void mRect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_timing)
            {
                setTime(_setTime + (new TimeSpan(0, e.Delta / 120, 0)));
            }
        }

        private void sRect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_timing)
            {
                setTime(_setTime + (new TimeSpan(0, 0, e.Delta / 120)));
            }
        }

        private void timeSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_timing)
            {
                _lpTimer.Stop();
            }
            timerTextBlock.Text = _setTime.ToString(@"hh\:mm\:ss\.f");
        }

        private void timeSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_timing)
            {
                _lpTimer.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            appSets._setTime = _setTime;
            appSets.keyHooked = keyHookCheck.IsChecked.Value;
            appSets.key = keySelector.SelectedIndex;
            appSets.Save();
        }
    }
}

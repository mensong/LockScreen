using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Models;
using Notifications.Wpf;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace LockScreen
{
    // logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        DataBase<tbl_Setting> m_tblSetting;
        DataBase<tbl_QuestionBank> m_tblQuestionBank;
        DispatcherTimer m_timer;
        IEnumerable<tbl_QuestionBank> m_questions;
        tbl_QuestionBank m_curQuestion;
        List<tbl_Setting> m_edittingSetting;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

#if DEBUG
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                this.AllowsTransparency = false;
#else
                // 满屏
                int width = 0, height = 0;
                foreach (var screen in Screen.AllScreens)
                {
                    width += screen.WorkingArea.Width;
                    height += screen.WorkingArea.Height;
                }
                this.Width = width;
                this.Height = height;

                // for responsive in multiple Monitors
                if (Screen.AllScreens.Length > 1)
                {
                    Grid.SetColumn(mainBox, 2);
                    Screen screen = Screen.AllScreens[0];
                    int primaryWidth = screen.WorkingArea.Width;
                    int primaryHeight = screen.WorkingArea.Height;
                    mainBox.Margin = new Thickness(0, 0, ((primaryWidth / 2) - (mainBox.Width / 2)) - 120, 0);
                }
#endif

                //生成题库
                initQuestionBank();
                //生成配置
                initSetting();
                
                m_questions = m_tblQuestionBank.SelectAll();                     
                pickQuestion();

                //从配置中更新内容
                updateSetting();
                
                mainWindow = this;
                // hook keyboard
                IntPtr hModule = GetModuleHandle(IntPtr.Zero);
                m_hookProc = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
                m_hHook = SetWindowsHookEx(WH_KEYBOARD_LL, m_hookProc, hModule, 0);
                DisableTaskManager();//禁用任务管理器
                if (m_hHook == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to set hook, error = " + Marshal.GetLastWin32Error());
                }

                //dy.Children.Add(XamlReader.Parse(@"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Content='Click Me'/>") as System.Windows.Controls.Button);
            }
            catch (Exception ex) { Debug.Text(ex, "MainWindow()"); }
        }

        void initSetting()
        {
            m_tblSetting = new DataBase<tbl_Setting>();

            Func<string, string, bool> initSettingItem = delegate (string name, string defaultName)
            {
                if (!m_tblSetting.Exist(item => item.name == name))
                {
                    int id = 1;                    
                    try
                    {
                        id = (int)m_tblSetting.Max(a => a.id);
                        ++id;
                    }
                    catch 
                    { }
                    
                    return m_tblSetting.Insert(new tbl_Setting()
                    {
                        id = id,
                        name = name,
                        value = defaultName,
                    });
                }
                return true;
            };

            if (!initSettingItem("StartUp", "1"))
            {
                Console.WriteLine("初始化配置StartUp失败");
            }
            if (!initSettingItem("ExamInterval", "120"))
            {
                Console.WriteLine("初始化配置ExamInterval失败");
            }
            if (!initSettingItem("Password", "admin123"))
            {
                Console.WriteLine("初始化配置password失败");
            }
            if (!initSettingItem("Title", "好好学习，天天向上"))
            {
                Console.WriteLine("初始化配置title失败");
            }
        }

        void initQuestionBank()
        {
            m_tblQuestionBank = new DataBase<tbl_QuestionBank>();

            if (m_tblQuestionBank.Count() == 0)
            {
                tbl_QuestionBank question = new tbl_QuestionBank()
                {
                    level = 1,
                    caseSensitive = false,
                    passCount = 0,
                    errorCount = 0,
                };

                int id = 1;
                //加法题
                for (int i = 0; i <= 20; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        question.id = id++;
                        question.question = i.ToString() + "+" + j.ToString() + "=";
                        question.answer = (i + j).ToString();
                        m_tblQuestionBank.Insert(question);
                    }
                }

                //减法题
                for (int i = 1; i <= 20; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        if (i < j)
                            continue;

                        question.id = id++;
                        question.question = i.ToString() + "-" + j.ToString() + "=";
                        question.answer = (i - j).ToString();
                        m_tblQuestionBank.Insert(question);
                    }
                }
            }
        }

        void pickQuestion()
        {
            int count = m_questions.Count();
            Random rd = new Random();
            //0-count的随机整数
            int idx = rd.Next(0, count);
            var qq = m_questions.Skip(idx).Take(1);
            if (qq != null)
            {
                m_curQuestion = qq.FirstOrDefault();
                if (m_curQuestion != null)
                {
                    Question.Text = m_curQuestion.question;
                }
            }
        }

        void startToExam(object sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Visible)
            {
                pickQuestion();
                this.Visibility = Visibility.Visible;
                editAnswer.Focus();
            }
        }

        private void editAnswer_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool sucess = false;

            if (m_curQuestion != null)
            {
                if (m_curQuestion.caseSensitive)
                {
                    if (editAnswer.Text == m_curQuestion.answer)
                    {
                        sucess = true;
                    }
                }
                else if (editAnswer.Text.Equals(m_curQuestion.answer, StringComparison.OrdinalIgnoreCase))
                {
                    sucess = true;
                }
            }

            if (sucess)
            {
                //Message("成功", "考试成功", NotificationType.Success);
                this.Visibility = Visibility.Hidden;
                editAnswer.Text = "";
            }
            else
            {
                //Message("错误", "答案错误", NotificationType.Error);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            bool sucess = false;

            if (m_curQuestion != null)
            {
                if (m_curQuestion.caseSensitive)
                {
                    if (editAnswer.Text == m_curQuestion.answer)
                    {
                        sucess = true;
                    }
                }
                else if (editAnswer.Text.Equals(m_curQuestion.answer, StringComparison.OrdinalIgnoreCase))
                {
                    sucess = true;
                }
            }

            if (sucess)
            {
                Message("成功", "考试成功", NotificationType.Success);
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                Message("错误", "答案错误", NotificationType.Error);
            }

            editAnswer.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                UnhookWindowsHookEx(m_hHook); // release keyboard hook
                EnableCTRLALTDEL();
            }
            catch (Exception ex) { Debug.Text(ex, "Window_Closed()"); }
        }

        private double settingBoxActualHeight = 450;
        // hide/show setting box 
        void show_hideSettingBox()
        {
            if (settingBox.Visibility == Visibility.Visible)
            {
                settingBoxActualHeight = settingBox.ActualHeight;
                DoubleAnimation animation0 = new DoubleAnimation();
                animation0.From = settingBox.ActualHeight;
                animation0.To = 0;
                animation0.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                animation0.Completed += (object sender, EventArgs e) =>
                {
                    settingBox.Visibility = Visibility.Collapsed;
                };
                settingBox.BeginAnimation(HeightProperty, animation0);
            }
            else
            {
                settingBox.Visibility = Visibility.Visible;

                DoubleAnimation animation1 = new DoubleAnimation();
                animation1.From = 0;
                animation1.To = settingBoxActualHeight;
                animation1.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                settingBox.BeginAnimation(HeightProperty, animation1);
            }
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            m_edittingSetting = m_tblSetting.SelectAll().ToList();
            ListSetting.ItemsSource = m_edittingSetting;

            SettingPassword.Visibility = Visibility.Visible;
            SettingContext.Visibility = Visibility.Collapsed;

            show_hideSettingBox();
        }

        private void btnCheckPassword_Click(object sender, RoutedEventArgs e)
        {
            tbl_Setting setingPassword = m_tblSetting.SelectOne(a => a.name == "Password");
            if (setingPassword == null || setingPassword.value == myPasswordBox.Password)
            {
                SettingPassword.Visibility = Visibility.Collapsed;
                SettingContext.Visibility = Visibility.Visible;

                myPasswordBox.Password = "";
            }
            else
            {
                Message("错误", "密码错误", NotificationType.Error);
            }
        }
        
        private void btnSettingOK_Click(object sender, RoutedEventArgs e)
        {
            if (!m_tblSetting.Update(m_edittingSetting))
            {
                Message("错误", "修改设置失败", NotificationType.Error);
                return;
            }
            updateSetting();
            show_hideSettingBox();

            Message("成功", "修改设置成功", NotificationType.Success);
        }

        private void btnSettingClose_Click(object sender, RoutedEventArgs e)
        {
            show_hideSettingBox();
        }

        private void updateSetting()
        {
            //重新设置定时器
            if (m_timer != null)
            {
                m_timer.Stop();
                m_timer = null;
            }
            tbl_Setting settingExamInterval = m_tblSetting.SelectOne(a => a.name == "ExamInterval");
            int ExamInterval = 120;
            if (settingExamInterval != null)
            {
                int.TryParse(settingExamInterval.value, out ExamInterval);                
            }
            if (ExamInterval > 0)
            {
                m_timer = new DispatcherTimer();
                m_timer.Interval = TimeSpan.FromSeconds(ExamInterval);
                m_timer.Tick += new System.EventHandler(startToExam);
                m_timer.Start();
            }

            //重新设置开机启动
            tbl_Setting settingStartUp = m_tblSetting.SelectOne(a => a.name == "StartUp");
            if (settingStartUp != null)
            {
                int StartUp = 1;
                int.TryParse(settingStartUp.value, out StartUp);
                InstallMeOnStartUp(StartUp == 1);
            }

            //设置标题
            tbl_Setting settingTitle = m_tblSetting.SelectOne(a => a.name == "Title");
            if (settingTitle != null)
            {
                txtTitle.Text = settingTitle.value;
            }
        }

        // show notification message 
        // <param name="type">blue-green-red-yellow</param>
        public void Message(string title, string message, NotificationType type)
        {
            try
            {
                WindowArea.Show(new NotificationContent
                {
                    Type = type,
                    Message = message,
                    Title = title
                }
                , TimeSpan.FromSeconds(3), null, null);
                //, TimeSpan.FromSeconds(3), onClick: () => Console.WriteLine("Click"), onClose: () => Console.WriteLine("Closed!"));
            }
            catch (Exception ex) { Debug.Text(ex, "Message()"); }
        }

        void InstallMeOnStartUp(bool setInStartUp = true)
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                if (setInStartUp)
                {
                    if (key.GetValue(curAssembly.GetName().Name) == null)
                        key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                }
                else
                {
                    if (key.GetValue(curAssembly.GetName().Name) != null)
                        key.SetValue(curAssembly.GetName().Name, null);
                }
            }
            catch (Exception ex) { Debug.Text(ex, "InstallMeOnStartUp()"); }
        }

        private void DisableTaskManager()
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "1";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex) { Debug.Text(ex, "DisableTaskManager()"); }
        }

        public static void EnableCTRLALTDEL()
        {
            try
            {
                string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);
            }
            catch (Exception ex) { Debug.Text(ex, "EnableCTRLALTDEL()"); }
        }

        // Disable Real All Key out of Program (Except ctrl+alt+delete)
        #region disable keys

        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }

        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(IntPtr path);

        private IntPtr m_hHook;
        LowLevelKeyboardProcDelegate m_hookProc; // prevent gc
        const int WH_KEYBOARD_LL = 13;
        private static MainWindow mainWindow;

        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (mainWindow != null && mainWindow.Visibility == Visibility.Visible && nCode >= 0)
            {
                switch (wParam)
                {
                    case 256: // WM_KEYDOWN
                    case 257: // WM_KEYUP
                    case 260: // WM_SYSKEYDOWN
                    case 261: // M_SYSKEYUP
                        if (
                            (lParam.vkCode == 0x09 && lParam.flags == 32) || // Alt+Tab
                            (lParam.vkCode == 0x1b && lParam.flags == 32) || // Alt+Esc
                            (lParam.vkCode == 0x73 && lParam.flags == 32) || // Alt+F4
                            (lParam.vkCode == 0x1b && lParam.flags == 0) || // Ctrl+Esc
                            (lParam.vkCode == 0x5b && lParam.flags == 1) || // Left Windows Key 
                            (lParam.vkCode == 0x5c && lParam.flags == 1))    // Right Windows Key 
                        {
                            return 1; //Do not handle key events
                        }
                        break;
                }
            }

            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        #endregion

        private void ListSetting_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0 || e.Column.DisplayIndex == 1)
                e.Cancel = true;
        }

        private void ListSetting_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Keyboard.Focus(editAnswer);
        }
    }
}

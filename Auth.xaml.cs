using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Reflection.Emit;

namespace lab
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string password, captcha;
        private bool capneed = true;
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer checksucces = new DispatcherTimer();
        private Random random = new Random();

        labEntities2 db = new labEntities2();
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;

            var userstatus = db.История_входа.OrderByDescending(x => x.Код_входа).First();

            DateTime neededdatetime = userstatus.Дата_входа.GetValueOrDefault();

            if (userstatus.Состояние.TrimEnd() == "Окончил")
            {
                if(DateTime.Now <= neededdatetime.AddHours(3))
                {
                    authbtn.IsEnabled = false;

                    checksucces.Interval = TimeSpan.FromMinutes(40);
                    timer.Tick += timer_TickCheck;
                    timer.Start();
                }
                else
                {
                    authbtn.IsEnabled = true;
                }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            authbtn.IsEnabled = true;
            capchan.IsEnabled = true;
            timer.Stop();
        }

        void timer_TickCheck(object sender, EventArgs e)
        {
            var userstatus = db.История_входа.OrderByDescending(x => x.Код_входа).First();

            DateTime neededdatetime = userstatus.Дата_входа.GetValueOrDefault();

            if (userstatus.Состояние.TrimEnd() == "Окончил")
            {
                if (DateTime.Now > neededdatetime.AddHours(3))
                {
                    authbtn.IsEnabled = true;
                    timer.Stop();
                }
            }
        }

        private void authbtn_Click(object sender, RoutedEventArgs e)
        {
            if (passwordnovis.Visibility == Visibility.Visible)
                password = passwordnovis.Password;
            else
                password = passwordvis.Text;

            var user = db.users_.FirstOrDefault(x => x.login == login.Text && x.password == password);
            if (capneed)
            {
                //Без капчи
                if (user != null)
                {
                    using (var context = new labEntities2())
                    {
                        var ИсторияВхода = new История_входа
                        {
                            Код_пользователя = user.id,
                            Состояние = "Успешно",
                            Дата_входа = DateTime.Now
                        };
                        context.История_входа.Add(ИсторияВхода);
                        context.SaveChanges();
                    }
                    General general = new General(user.id);
                    general.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неправильно ввели логин или пароль");
                    capneed = false;
                    cap.Visibility = Visibility.Visible;
                    capin.Visibility = Visibility.Visible;
                    caplabel.Visibility = Visibility.Visible;
                    capchan.Visibility = Visibility.Visible;
                    captcha = GenerateRandomText(6);
                    cap.Content = captcha;
                }
            }
            else
            {
                //С капчей
                if (user != null && capin.Text == captcha)
                {
                    using (var context = new labEntities2())
                    {
                        var ИсторияВхода = new История_входа
                        {
                            Код_пользователя = user.id,
                            Состояние = "Успешно",
                            Дата_входа = DateTime.Now
                        };
                        context.История_входа.Add(ИсторияВхода);
                        context.SaveChanges();
                    }
                    General general = new General(user.id);
                    general.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неправильно ввели логин, пароль или капчу");
                    authbtn.IsEnabled = false;
                    capchan.IsEnabled = false;
                    
                    timer.Interval = TimeSpan.FromSeconds(10);
                    timer.Tick += timer_Tick;
                    timer.Start();
                    captcha = GenerateRandomText(6);
                    cap.Content = captcha;
                }
            }

        }

        private void showpass_Click(object sender, RoutedEventArgs e)
        {
            if(passwordnovis.Visibility == Visibility.Visible)
            {
                passwordnovis.Visibility = Visibility.Hidden;
                passwordvis.Visibility = Visibility.Visible;
                passwordvis.Text = passwordnovis.Password;
            }
            else
            {
                passwordnovis.Visibility = Visibility.Visible;
                passwordvis.Visibility = Visibility.Hidden;
                passwordnovis.Password = passwordvis.Text;
            }
        }

        private void capchan_Click(object sender, RoutedEventArgs e)
        {
            captcha = GenerateRandomText(6);
            cap.Content = captcha;
            capchan.IsEnabled = false;
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] captchaChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                captchaChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(captchaChars);
        }
    }
}

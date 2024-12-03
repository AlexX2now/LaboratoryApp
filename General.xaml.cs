using System;
using System.Collections.Generic;
using System.Globalization;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
using Mail_LIB;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using BarcodeStandard;
using System.IO;
using System.Diagnostics;

namespace lab
{
    /// <summary>
    /// Логика взаимодействия для General.xaml
    /// </summary>
    public partial class General : Window
    {
        private int iduser;
        private DispatcherTimer timer;
        private BitmapImage avatar;

        private TimeSpan remainingTime = TimeSpan.FromHours(2.5);
        
        private List<int> idservices = new List<int>();

        private readonly string chars = "0123456789";
        private Random randomgen = new Random();

        public static readonly DependencyProperty TickCounterProperty = DependencyProperty.Register(
            "TickCounter", typeof(TimeSpan), typeof(General), new PropertyMetadata(TimeSpan.Zero));


        labEntities2 db = new labEntities2();
        public General(int id)
        {
            InitializeComponent();
            this.iduser = id;
            var user = db.users_.FirstOrDefault(x => x.id == id);
            idlab.Content = user.name;

            if (user.Профессии.Наименование_должности.TrimEnd() == "Лаборант")
            {
                avatar = new BitmapImage(new Uri("pack://application:,,,/images/avatars/laborant_1.jpeg"));
                avatarshow.Source = avatar;
                LabAssist.Visibility = Visibility.Visible;
                timerset.Visibility = Visibility.Visible;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else if (user.Профессии.Наименование_должности.TrimEnd() == "Лаборант-исследователь")
            {
                avatar = new BitmapImage(new Uri("pack://application:,,,/images/avatars/laborant_2.png"));
                avatarshow.Source = avatar;
                LabAssistResearcher.Visibility = Visibility.Visible;
                timerset.Visibility = Visibility.Visible;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else if (user.Профессии.Наименование_должности.TrimEnd() == "Бухгалтер")
            {
                avatar = new BitmapImage(new Uri("pack://application:,,,/images/avatars/Бухгалтер.jpeg"));
                avatarshow.Source = avatar;
                timerset.Visibility = Visibility.Hidden;
                Accountant.Visibility = Visibility.Visible;
            }
            else
            {
                avatar = new BitmapImage(new Uri("pack://application:,,,/images/avatars/Администратор.png"));
                avatarshow.Source = avatar;
                timerset.Visibility = Visibility.Hidden;
                Admin.Visibility = Visibility.Visible;
            }
        }

        public TimeSpan TickCounter
        {
            get { return (TimeSpan)GetValue(TickCounterProperty); }
            set { SetValue(TickCounterProperty, value); }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            TickCounter = remainingTime;
            if (remainingTime == TimeSpan.FromMinutes(15))
            {
                MessageBox.Show("Внимание! Вам осталось 15 минут до окончания сеанса, когда время закончится, сеанс сам остановится.");
            }
            else if (remainingTime == TimeSpan.Zero)
            {
                MessageBox.Show("Ваш сеанс закончился!");

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();

                var userstatus = db.История_входа.Last(x => x.Код_пользователя == iduser);
                userstatus.Состояние = "Окончил";
                db.SaveChanges();
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void AdmGenARep_Click(object sender, RoutedEventArgs e)
        {
            AdmLabel.Visibility = Visibility.Hidden;

            CheckHisGrid.Visibility = Visibility.Hidden;
            WorkMaterialsGrid.Visibility = Visibility.Hidden;
            AdmGenARepGrid.Visibility = Visibility.Visible;
            //Добавить пользователя

            ProfessionNewUser.Items.Clear();

            List<Профессии> Profession = db.Профессии.ToList();

            for(int i = 0; i < Profession.Count; i++)
            {
                ProfessionNewUser.Items.Add(Profession[i].Наименование_должности.TrimEnd());
            }


        }

        private void AdmCheckHis_Click(object sender, RoutedEventArgs e)
        {
            AdmLabel.Visibility = Visibility.Hidden;

            CheckHisGrid.Visibility = Visibility.Visible;
            WorkMaterialsGrid.Visibility = Visibility.Hidden;
            AdmGenARepGrid.Visibility = Visibility.Hidden;

            List<История_входа> allhistories = db.История_входа.ToList();

            for (int i = 0; i < allhistories.Count; i++)
            {
                CheckHisList.Items.Add(allhistories[i].users_.name + " в " + allhistories[i].Дата_входа + ", состояние: " + allhistories[i].Состояние);
            }
        }

        private void AdmWorkMaterials_Click(object sender, RoutedEventArgs e)
        {
            AdmLabel.Visibility = Visibility.Hidden;

            CheckHisGrid.Visibility = Visibility.Hidden;
            WorkMaterialsGrid.Visibility = Visibility.Visible;
            AdmGenARepGrid.Visibility = Visibility.Hidden;
            //Работать с материалами
        }

        private void LAAddPatientBtn(object sender, RoutedEventArgs e)
        {
            LAlabel.Visibility = Visibility.Hidden;
            LARlabel.Visibility = Visibility.Hidden;

            AddPatientGrid.Visibility = Visibility.Visible;
            ShowPatientGrid.Visibility = Visibility.Hidden;
            WorkGrid.Visibility = Visibility.Hidden;
            AddMaterialGrid.Visibility = Visibility.Hidden;
        }

        private void LAShowPatientBtn(object sender, RoutedEventArgs e)
        {
            LAlabel.Visibility = Visibility.Hidden;
            LARlabel.Visibility = Visibility.Hidden;

            AddPatientGrid.Visibility = Visibility.Hidden;
            ShowPatientGrid.Visibility = Visibility.Visible;
            WorkGrid.Visibility = Visibility.Hidden;
            AddMaterial.Visibility = Visibility.Hidden;


            List<Данные_пациентов> allpatients = db.Данные_пациентов.ToList();
            ShowPatients.Items.Clear();

            for (int i = 0; i < allpatients.Count; i++)
            {
                DateTime neededdate = allpatients[i].Дата_рождения.Value;

                ShowPatients.Items.Add(allpatients[i].Фамилия.TrimEnd() + " " + allpatients[i].Имя.TrimEnd() + " " + allpatients[i].Отчество.TrimEnd() + " " + allpatients[i].E_mail.TrimEnd() + " " + neededdate.ToShortDateString() + ". Паспорт, номер: " + allpatients[i].Номер_паспорта + " , серия: " + allpatients[i].Серия_паспорта);
            }
        }

        private void AddPatientToDB(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(surname.Text) ||
            string.IsNullOrEmpty(name.Text) ||
            string.IsNullOrEmpty(patr.Text) ||
            string.IsNullOrEmpty(dateofborn.Text) ||
            string.IsNullOrEmpty(passnum.Text) ||
            string.IsNullOrEmpty(passser.Text) ||
            string.IsNullOrEmpty(telephone.Text) ||
            string.IsNullOrEmpty(email.Text) ||
            string.IsNullOrEmpty(login.Text) ||
            string.IsNullOrEmpty(password.Text))
            {
                MessageBox.Show("Не все поля заполнены");
            }
            else
            {
                if (!Check.check_login(login.Text) ||
                    !Check.check_mail(email.Text) ||
                    !Check.check_password(password.Text))
                {
                    MessageBox.Show("Логин, пароль или e-mail введены некорректно");
                }
                else
                {
                    if (telephone.Text.Length != 11 ||
                        passser.Text.Length != 4 ||
                        passnum.Text.Length != 6)
                    {
                        MessageBox.Show("Телефон, серия или номер паспорта введены некорректно");
                    }
                    else
                    {
                        using (var context = new labEntities2())
                        {
                            var Пациент = new Данные_пациентов
                            {
                                Фамилия = surname.Text,
                                Имя = name.Text,
                                Отчество = patr.Text,
                                Дата_рождения = DateTime.Parse(dateofborn.ToString()),
                                Логин = login.Text,
                                Пароль = password.Text,
                                Номер_паспорта = passnum.Text,
                                Серия_паспорта = passser.Text,
                                Телефон = telephone.Text,
                                E_mail = email.Text
                            };
                            context.Данные_пациентов.Add(Пациент);
                            context.SaveChanges();
                        }
                        
                        MessageBox.Show("Пациент добавлен!");

                        surname.Text = "";
                        name.Text = "";
                        patr.Text = "";
                        dateofborn.Text = "";
                        login.Text = "";
                        password.Text = "";
                        passnum.Text = "";
                        passser.Text = "";
                        telephone.Text = "";
                        email.Text = "";
                    }
                }
            }
        }

        private void WorkBtn_Click(object sender, RoutedEventArgs e)
        {
            LAlabel.Visibility = Visibility.Hidden;
            LARlabel.Visibility = Visibility.Hidden;

            AddPatientGrid.Visibility = Visibility.Hidden;
            ShowPatientGrid.Visibility = Visibility.Hidden;
            WorkGrid.Visibility = Visibility.Visible;

            ShowReports.Items.Clear();
            selectPatient.Items.Clear();
            selectService.Items.Clear();
            selectBiomaterial.Items.Clear();

            List<Данные_пациентов> allpatients = db.Данные_пациентов.ToList();
            
            List<services_> allservices = db.services_.ToList();
            List<services_> needservice = allservices.FindAll(x => x.user_id == iduser);

            List<Биоматериалы> allbiomaterials = db.Биоматериалы.ToList();

            for (int i = 0; i < allpatients.Count; i++)
            {
                selectPatient.Items.Add(allpatients[i].Фамилия.TrimEnd() + " " + allpatients[i].Имя[0] +  ". " + allpatients[i].Отчество[0] + ".");
            }

            for (int i = 0; i < needservice.Count; i++)
            {
                selectService.Items.Add(needservice[i].Услуги_.Service);
            }

            for (int i = 0; i < allbiomaterials.Count; i++)
            {
                selectBiomaterial.Items.Add(allbiomaterials[i].Название.TrimEnd());
            }

            List<Заказ> reports = db.Заказ.ToList();
            List<Услуги_> service = db.Услуги_.ToList();
            for(int i = 0; i < reports.Count; i++)
            {
                var needed = service.Find(x => x.Code == reports[i].Код_услуги);

                ShowReports.Items.Add(reports[i].Данные_пациентов.Фамилия.TrimEnd() + " " + reports[i].Данные_пациентов.Имя[0] + ". " + reports[i].Данные_пациентов.Отчество[0] + "., дата: " + reports[i].Дата_создания + ", услуга: " + needed.Service + ", требуемое время в днях: " + reports[i].Время_выполнения_заказа__в_днях_.TrimEnd() + ", состояние: " + reports[i].Статус_заказа.TrimEnd() + ", биоматериал: " + reports[i].Биоматериалы.Название.TrimEnd() + ", цена: " + service[i].Price);
            }
        }

        private void AddReport(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(neededDays.Text) || selectPatient.SelectedIndex == -1 || selectService.SelectedIndex == -1 || selectBiomaterial.SelectedIndex == -1) 
            {
                MessageBox.Show("Пациент либо услуга не выбрана или не написано сколько потребуется времени.");
            }
            else
            {
                if (!char.IsDigit(Convert.ToChar(neededDays.Text)))
                {
                    MessageBox.Show("Вы написали не числовое значение в поле \"Сколько времени потребуется\"");
                }
                else
                {
                    var servicesselected = db.Услуги_.FirstOrDefault(x => x.Service == selectService.Text);

                    using (var context = new labEntities2())
                    {
                        var Заказ = new Заказ {
                            Дата_создания = DateTime.Now,
                            Код_услуги = servicesselected.Code,
                            Статус_заказа = "Ждёт подтверждения",
                            Время_выполнения_заказа__в_днях_ = neededDays.Text,
                            Код_пациента = selectPatient.SelectedIndex + 1,
                            Код_биоматериалов = selectBiomaterial.SelectedIndex + 1
                        };
                        context.Заказ.Add(Заказ);
                        context.SaveChanges();
                    }

                    var lastreport = db.Заказ.OrderByDescending(x => x.Код_заказа).FirstOrDefault();

                    DateTime now = DateTime.Now;

                    string result = $"{lastreport.Код_заказа}{now.Day}{now.Month}{now.Hour}{now.Year}";

                    for (int i = 0; i < 6; i++)
                    {
                        result += chars[randomgen.Next(chars.Length)];
                    }
                    Code.Text = result;

                    System.Drawing.Image image = null;
                    //переменная для штрих-кода
                    BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                    //Фон штрих-кода
                    b.BackColor = System.Drawing.Color.White;
                    //Цвет шрих-кода
                    b.ForeColor = System.Drawing.Color.Black;
                    //наличие текста на штрих-коде
                    b.IncludeLabel = true;
                    //Положение кода на картинке (слева, по центру, справа)
                    b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                    //Положение текста на картинке
                    //Формат изображения
                    b.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    //Настройки шрифта
                    System.Drawing.Font font = new System.Drawing.Font("verdana", 10f);
                    b.LabelFont = font;
                    //Настройка высоты изображения (в пикселях)
                    b.Height = 100;
                    //Настройка ширины изображения (в пикселях)
                    b.Width = 200;
                    //Генерация изображения
                    image = b.Encode(BarcodeLib.TYPE.CODE128C, result);
                    //Сохранение изображения
                    image.Save(@"C:\Users\usersql\Desktop\code.jpg");

                    Document doc = new Document();

                    // Создаем экземпляр PdfWriter для записи в PDF файл
                    PdfWriter.GetInstance(doc, new FileStream(@"C:\Users\usersql\Desktop\code.pdf", FileMode.Create));

                    // Открываем документ для записи
                    doc.Open();

                    // Создаем изображение iTextSharp
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(@"C:\Users\usersql\Desktop\code.jpg");

                    // Устанавливаем размеры изображения в документе PDF
                    img.ScaleToFit(doc.PageSize);

                    // Добавляем изображение в документ PDF
                    doc.Add(img);

                    // Закрываем документ
                    doc.Close();
                    Process.Start(@"C:\Users\usersql\Desktop\code.pdf");
                    MessageBox.Show("Заказ добавлен!");

                    ShowReports.Items.Add(selectPatient.Text + ", дата: " + DateTime.Now + ", услуга: " + servicesselected.Code + ", требуемое время в днях: " + neededDays.Text + ", состояние: Ждёт подтверждения, биоматериал: " + selectBiomaterial.Text + ", цена:" + servicesselected.Price);
                }
            }
        }

        private void CheckInsuranceBtn_Click(object sender, RoutedEventArgs e)
        {
            Accountantlabel.Visibility = Visibility.Hidden;

            AddInsuranceGrid.Visibility = Visibility.Hidden;
            CheckInsuranceGrid.Visibility = Visibility.Visible;

            InsuranceListBox.Items.Clear();
            CompaniesList.Items.Clear();

            List<Данные_пациентов> allpatients = db.Данные_пациентов.ToList();
            List<Данные_пациентов> datepatients = allpatients.FindAll(x => x.Код_страхового_полиса != null);

            for (int i = 0; i < datepatients.Count; i++)
            {

                InsuranceListBox.Items.Add(datepatients[i].Фамилия.TrimEnd() + " " + datepatients[i].Имя[0] + ". " + datepatients[i].Отчество[0] + ". Номер страхового полиса: " + datepatients[i].Страховой_полис.Номер_страхового_полиса + " тип страхового полиса: " + datepatients[i].Страховой_полис.Тип_страхового_полиса.TrimEnd() + ". Страховая компания: " + datepatients[i].Данные_о_страховых_компаниях.Название_страховой_компании.TrimEnd());


                CompaniesList.Items.Add(datepatients[i].Данные_о_страховых_компаниях.Название_страховой_компании.TrimEnd() + ". Адрес: " + datepatients[i].Данные_о_страховых_компаниях.Адрес.TrimEnd() + ", ИНН:" + datepatients[i].Данные_о_страховых_компаниях.ИНН.TrimEnd() + ", р/с: " + datepatients[i].Данные_о_страховых_компаниях.р_с.TrimEnd() + ", БИК:" + datepatients[i].Данные_о_страховых_компаниях.БИК.TrimEnd());
            }
        }

        private void AddInsuranceBtn_Click(object sender, RoutedEventArgs e)
        {
            Accountantlabel.Visibility = Visibility.Hidden;

            AddInsuranceGrid.Visibility = Visibility.Visible;
            CheckInsuranceGrid.Visibility = Visibility.Hidden;

            AccselectPatient.Items.Clear();

            List<Данные_пациентов> allpatients = db.Данные_пациентов.ToList();

            for (int i = 0; i < allpatients.Count; i++)
            {
                AccselectPatient.Items.Add(allpatients[i].Фамилия.TrimEnd() + " " + allpatients[i].Имя[0] + ". " + allpatients[i].Отчество[0] + ".");
            }
        }

        private void AddCompany_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NumberPolicy.Text) || TypeOfPolicy.SelectedIndex == -1 || string.IsNullOrEmpty(NameOfCompany.Text) || string.IsNullOrEmpty(INNOfCompany.Text) || string.IsNullOrEmpty(BIKOfCompany.Text) || string.IsNullOrEmpty(AdressOfCompany.Text) || string.IsNullOrEmpty(r_sOfCompany.Text) || AccselectPatient.SelectedIndex == -1)
            {
                MessageBox.Show("Не оставляйте поля пустыми");
            }
            else
            {
                if (!Mail_LIB.Check.check_num(NumberPolicy.Text)
                    || !Mail_LIB.Check.check_num(INNOfCompany.Text)
                    || !Mail_LIB.Check.check_num(r_sOfCompany.Text) 
                    || !Mail_LIB.Check.check_num(BIKOfCompany.Text))
                {
                    MessageBox.Show("В поле \"Номер страхового полиса\", \"ИНН\", \"р/с\", \"БИК\" напишите число");
                }
                else
                {
                    if (NumberPolicy.Text.Length != 16 || INNOfCompany.Text.Length != 10 || r_sOfCompany.Text.Length != 20 || BIKOfCompany.Text.Length != 9)
                    {
                        MessageBox.Show("Номер полиса должен быть длинной в 16 единиц, ИНН в 10, р/с в 20 и БИК в 9.");
                    }
                    else
                    {

                            int idcompany;

                            using (var context = new labEntities2())
                            {
                                var Policy = new Страховой_полис
                                {
                                    Номер_страхового_полиса = NumberPolicy.Text,
                                    Тип_страхового_полиса = TypeOfPolicy.Text
                                };
                                context.Страховой_полис.Add(Policy);
                                idcompany = Policy.Код_страхового_полиса;

                                var Company = new Данные_о_страховых_компаниях
                                {
                                    Название_страховой_компании = NameOfCompany.Text,
                                    Адрес = AdressOfCompany.Text,
                                    ИНН = INNOfCompany.Text,
                                    р_с = r_sOfCompany.Text,
                                    БИК = BIKOfCompany.Text
                                };
                                context.Данные_о_страховых_компаниях.Add(Company);

                                context.SaveChanges();
                            }

                            var lastuser = db.Данные_пациентов.FirstOrDefault(x => x.Код_пациента == AccselectPatient.SelectedIndex + 1);
                            MessageBox.Show(lastuser.Фамилия);
                            var lastreport = db.Данные_о_страховых_компаниях.OrderByDescending(x => x.Код_страховой_компании).FirstOrDefault();

                            lastuser.Код_страхового_полиса = lastreport.Код_страховой_компании;
                            lastuser.Код_страховой_компании = lastreport.Код_страховой_компании;

                            db.SaveChanges();
                            MessageBox.Show("Данные о страховом полисе и страховых компаниях пациента добавлены!");
                        }


                }
            }
        }

        private void AccselectPatient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccselectPatient.SelectedIndex != -1)
            {
                var usercheck = db.Данные_пациентов.FirstOrDefault(x => x.Код_пациента == AccselectPatient.SelectedIndex + 1);

                if (usercheck.Код_страхового_полиса != null)
                {
                    MessageBox.Show("У человека уже есть страховая компания.");
                    AddCompany.IsEnabled = false;
                }
                else
                {
                    AddCompany.IsEnabled = true;
                }
            }            
        }

        private void ProfessionNewUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NeededService.Items.Clear();
            if (ProfessionNewUser.SelectedIndex == 0 || ProfessionNewUser.SelectedIndex == 1)
            {

                forlaborants.Visibility = Visibility.Visible;

                List<Услуги_> allservice = db.Услуги_.ToList();

                for (int i = 0; i < allservice.Count; i++)
                {
                    NeededService.Items.Add(allservice[i].Service.TrimEnd());
                }
            }
            else
            {
                forlaborants.Visibility = Visibility.Hidden;
            }
        }

        private void AddNewUser_Click(object sender, RoutedEventArgs e)
        {
            var lastuser = db.users_.OrderByDescending(x => x.id).FirstOrDefault();
            int neededuserid = lastuser.id + 1;
            if (ProfessionNewUser.Text.TrimEnd() == "Лаборант" || ProfessionNewUser.Text.TrimEnd() == "Лаборант-исследователь")
            {
                //Добавление лаборанта и лаборанта исследователя
                if (string.IsNullOrEmpty(NameNewUser.Text) ||
                    string.IsNullOrEmpty(EmailNewUser.Text) ||
                    string.IsNullOrEmpty(TelephoneNewUser.Text) ||
                    string.IsNullOrEmpty(LoginNewUser.Text) ||
                    string.IsNullOrEmpty(PasswordNewUser.Text) ||
                    string.IsNullOrEmpty(ProfessionNewUser.Text))
                {
                    MessageBox.Show("Не оставляйте поля пустыми.");
                }
                else
                {
                    if (!Check.check_mail(EmailNewUser.Text) ||
                        !Check.check_login(LoginNewUser.Text) ||
                        !Check.check_password(PasswordNewUser.Text) ||
                        TelephoneNewUser.Text.Length != 11 ||
                        !Check.check_num(TelephoneNewUser.Text))
                    {
                        MessageBox.Show("Пароль, логин, электронная почта или номер телефона написаны некорректно");
                    }
                    else
                    {
                        if(idservices.Count == 0)
                        {
                            MessageBox.Show("Выберите услуги, которые лаборант или лаборант исследователь могут делать");
                        }
                        else
                        {
                            var neededidprof = db.Профессии.FirstOrDefault(x => x.Наименование_должности.TrimEnd() == ProfessionNewUser.Text);

                            
                            using (var context = new labEntities2())
                            {
                                var user = new users_
                                {
                                    name = NameNewUser.Text,
                                    login = LoginNewUser.Text,
                                    password = PasswordNewUser.Text,
                                    email = EmailNewUser.Text,
                                    Номер_телефона = TelephoneNewUser.Text,
                                    Код_профессии = neededidprof.Код
                                };
                                context.users_.Add(user);
                                context.SaveChanges();

                                

                                for (int i = 0; i < idservices.Count; i++)
                                {
                                    var services = new services_
                                    {
                                        user_id = neededuserid,
                                        services = idservices[i]
                                    };
                                    context.services_.Add(services);
                                }

                                context.SaveChanges();
                            }
                            MessageBox.Show("Пользователь успешно добавлен!");
                        }      
                    }
                }
                }
            else
            {
                if (string.IsNullOrEmpty(NameNewUser.Text) || 
                    string.IsNullOrEmpty(EmailNewUser.Text) ||
                    string.IsNullOrEmpty(TelephoneNewUser.Text) ||
                    string.IsNullOrEmpty(LoginNewUser.Text) || 
                    string.IsNullOrEmpty(PasswordNewUser.Text) ||
                    string.IsNullOrEmpty(ProfessionNewUser.Text))
                {
                    MessageBox.Show("Не оставляйте поля пустыми.");
                }
                else
                {
                    if (!Check.check_mail(EmailNewUser.Text) || 
                        !Check.check_login(LoginNewUser.Text) ||
                        !Check.check_password(PasswordNewUser.Text) ||
                        TelephoneNewUser.Text.Length != 11 ||
                        !Check.check_num(TelephoneNewUser.Text))
                    {
                        MessageBox.Show("Пароль, логин, электронная почта или номер телефона написаны некорректно");
                    }
                    else
                    {
                        var neededidprof = db.Профессии.FirstOrDefault(x => x.Наименование_должности.TrimEnd() == ProfessionNewUser.Text);
                        
                        using (var context = new labEntities2())
                        {
                            var user = new users_ { 
                            name = NameNewUser.Text,
                            login = LoginNewUser.Text,
                            password = PasswordNewUser.Text,
                            email = EmailNewUser.Text,
                            Номер_телефона = TelephoneNewUser.Text,
                            Код_профессии = neededidprof.Код
                            };
                            context.users_.Add(user);
                            context.SaveChanges();
                        }
                        MessageBox.Show("Пользователь успешно добавлен!");
                    }
                }
            }
        }

        
        private void delService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NeededService.Text))
            {
                MessageBox.Show("Вы не выбрали услугу");
            }
            else
            {
                var Услуги = db.Услуги_.FirstOrDefault(x => x.Service.TrimEnd() == NeededService.Text);

                if (idservices.Contains(Услуги.Code))
                {
                    selectedService.Items.Remove(NeededService.Text);

                    idservices.Remove(Услуги.Code);
                }
            }
        }

        private void addService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NeededService.Text))
            {
                MessageBox.Show("Вы не выбрали услугу");
            }
            else
            {
                var Услуги = db.Услуги_.FirstOrDefault(x => x.Service.TrimEnd() == NeededService.Text);

                if (idservices.Contains(Услуги.Code))
                {
                    MessageBox.Show("Вы уже ввели это значение");
                }
                else
                {
                    selectedService.Items.Add(NeededService.Text);

                    idservices.Add(Услуги.Code);
                }
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            LAlabel.Visibility = Visibility.Hidden;
            LARlabel.Visibility = Visibility.Hidden;

            AddPatientGrid.Visibility = Visibility.Hidden;
            ShowPatientGrid.Visibility = Visibility.Hidden;
            WorkGrid.Visibility = Visibility.Hidden;
            AddMaterialGrid.Visibility = Visibility.Visible;
        }

        private void Addmaterial_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(codeofmaterial.Text) || string.IsNullOrEmpty(nameofmaterial.Text))
            {
                MessageBox.Show("Не оставляйте поля пустыми");
            }
            else
            {
                if (!Check.check_num(codeofmaterial.Text))
                {
                    MessageBox.Show("Код написан некорректно.");
                }
                else
                {
                    using (var context = new labEntities2())
                    {
                        var Material = new Биоматериалы
                        {
                            Код_биоматериалов = codeofmaterial.Text,
                            Название = nameofmaterial.Text
                        };

                        context.Биоматериалы.Add(Material);

                        context.SaveChanges();
                    }
                    MessageBox.Show("Биоматериал успешно добавлен!");
                    codeofmaterial.Text = "";
                    nameofmaterial.Text = "";
                }
            }
        }
    }
}

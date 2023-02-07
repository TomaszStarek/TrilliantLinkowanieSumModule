using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Net.Mime.MediaTypeNames;

namespace SkanowanieTrillaintSumModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SerialNumber { get; set; } = "";
        private static UInt32 _indexOfFirstBarcode;
        public static List<string> ListOfSerialNumbers { get; set; } = new List<string>();


        public MainWindow()
        {
            InitializeComponent();

            Dispatcher.Invoke(new Action(() => textBoxBarcode.Focus()));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            var buttonName = button.Name;

            var indexOfButton = buttonName.Substring(6, buttonName.Length - 6);
            Int16 indexOfButtonParsed;

            bool isParsable = Int16.TryParse(indexOfButton, out indexOfButtonParsed);


            if (ListOfSerialNumbers.Count > 1)
            {
                if (!button.Content.Equals($"{ListOfSerialNumbers[indexOfButtonParsed - 1]}\nSCRAP"))
                {

                    Dispatcher.Invoke(new Action(() => button.Content = $"{ListOfSerialNumbers[indexOfButtonParsed - 1]}\nSCRAP"));
                    Dispatcher.Invoke(new Action(() => button.Background = Brushes.Red));


                }
                else
                {
                    Dispatcher.Invoke(new Action(() => button.Content = string.Empty));
                    Dispatcher.Invoke(new Action(() => button.Background = Brushes.Transparent));
                }
            }

        }

        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            string buttonContent = button.Content.ToString();

            var buttonName = button.Name;

            var indexOfButton = buttonName.Substring(6, buttonName.Length - 6);
            Int16 indexOfButtonParsed;

            bool isParsable = Int16.TryParse(indexOfButton, out indexOfButtonParsed);


            if (!buttonContent.Contains("SCRAP"))
            {
                if (ListOfSerialNumbers.Count > 1)
                {
                    Dispatcher.Invoke(new Action(() => button.Content = $"Zaznacz płytę\n{ListOfSerialNumbers[indexOfButtonParsed - 1]}\n(bez loga)"));
                }
                // Dispatcher.Invoke(new Action(() => button.Background = Brushes.MistyRose));
            }
            else
            {
                //  Dispatcher.Invoke(new Action(() => button.Content = $"Odznacz SCRAPA\n (Wyślij loga)"));
            }
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            string buttonContent = button.Content.ToString();

            if (!buttonContent.Contains("SCRAP"))
            {
                Dispatcher.Invoke(new Action(() => button.Content = string.Empty));
                Dispatcher.Invoke(new Action(() => button.Background = Brushes.Transparent));
            }

        }

        private void textBoxBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.Key == Key.Return)
            {
                if (textBox.Text.Length > 5)
                {

                    textBox.Text = Regex.Replace(textBox.Text, @"\s+", string.Empty);
                    SerialNumber = textBox.Text;

                    StartSetLabels();

                    if (!CountIndexFormMes())
                        return;
                    Dispatcher.Invoke(new Action(() => textBox.IsEnabled = false));

                    Dispatcher.Invoke(new Action(() => labelZaznaczPlyty.Visibility = Visibility.Visible));
                    Dispatcher.Invoke(new Action(() => buttonNadaj.Visibility = Visibility.Visible));
                    Dispatcher.Invoke(new Action(() => buttonReset.Visibility = Visibility.Visible));
                  
                    if (RadioTop.IsChecked == true)
                        Dispatcher.Invoke(new Action(() => image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "top.jpg", UriKind.Absolute))));
                }
                else
                {
                    ErrorSnReset();
                }
            }
        }
        private void Reset()
        {

        }
        private void ErrorSnReset()
        {
            Dispatcher.Invoke(new Action(() => labelPodajBarcode.Content = "Zeskanuj numer seryjny z kropką!"));
            SerialNumber = string.Empty;
            ListOfSerialNumbers.Clear();
            //  Dispatcher.Invoke(new Action(() => textBoxBarcode.Background = System.Windows.Media.Brushes.IndianRed));
            Dispatcher.Invoke(new Action(() => textBoxBarcode.Text = String.Empty));
            Dispatcher.Invoke(new Action(() => labelZaznaczPlyty.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new Action(() => buttonNadaj.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new Action(() => buttonReset.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new Action(() => textBoxBarcode.IsEnabled = true));
            Dispatcher.Invoke(new Action(() => textBoxBarcode.Focus()));
        }

        private bool CountIndexFormMes()
        {
            CheckHistoryMes.GetPanelSnList(@SerialNumber);

            if (ListOfSerialNumbers.Count == 16)
            {
                if (RadioTop.IsChecked == true)
                {
                    SwapList();
                    CheckHistoryMes.CheckSetupSheetList("11379");
                    CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_TOP");
                }
                else
                {
                    CheckHistoryMes.CheckSetupSheetList("12962");
                    CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_BTM");
                }
                CheckLabelForErrorSn();
                return true;
            }             
            else
            {
                Dispatcher.Invoke(new Action(() => textBoxBarcode.Text = String.Empty));
                MessageBox.Show($"Błąd połączenia MES: {SerialNumber}\n Zeskanuj jeszcze raz numer seryjny!", "Błąd MES - TIS", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Dispatcher.Invoke(new Action(() => textBoxBarcode.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => textBoxBarcode.Focus()));

            }
            return false;

        }

        private void CheckLabelForErrorSn()
        {

            for (int i = 1; i <= 16; i++)
            {
                if (ListOfSerialNumbers[i - 1].Contains("FAIL"))
                {
                    var buttonName = string.Format("button{0}", i);
                    var button = (Button)this.FindName(buttonName);
                    Dispatcher.Invoke(new Action(() => button.Visibility = Visibility.Visible));
                    Dispatcher.Invoke(new Action(() => button.Content = $"Nie można nadać loga:\n {ListOfSerialNumbers[i - 1]}"));
                    
                    Dispatcher.Invoke(new Action(() => button.IsEnabled = false));

 
                }

            }

        }   //0423A262236005233


        private bool CountIndexFromSerialNumber()
        {

            var substringSerial = SerialNumber.Substring(7, SerialNumber.Length - 7);  //0423A262237001665

            bool isParsable = UInt32.TryParse(substringSerial, out _indexOfFirstBarcode);

            if (!isParsable)
            {
                ErrorSnReset();
                MessageBox.Show($"Błąd podczas konwertowania numeru seryjnego: {SerialNumber}\n Zeskanuj jeszcze raz numer!", "Błąd numeru seryjnego", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            for (int i = 0; i < 16; i++)
            {
                var addedIndexBarcode = _indexOfFirstBarcode + i;
                ListOfSerialNumbers.Add(SerialNumber.Remove(7) + addedIndexBarcode.ToString() );
            }
            if (RadioTop.IsChecked == true)
            {
                SwapList();
                CheckHistoryMes.CheckSetupSheetList("11379");
                CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_TOP");
            }
            else
            {
                CheckHistoryMes.CheckSetupSheetList("12962");
                CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_BTM");
            }

            return true;                
        }

        private void SwapList()
        {
            Swap<string>(ListOfSerialNumbers, 0, 12);
            Swap<string>(ListOfSerialNumbers, 1, 13);
            Swap<string>(ListOfSerialNumbers, 2, 14);
            Swap<string>(ListOfSerialNumbers, 3, 15);

            Swap<string>(ListOfSerialNumbers, 4, 8);
            Swap<string>(ListOfSerialNumbers, 5, 9);
            Swap<string>(ListOfSerialNumbers, 6, 10);
            Swap<string>(ListOfSerialNumbers, 7, 11);
        }

        public static IList<T> Swap<T>(IList<T> list, int indexA, int indexB)
        {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
            return list;
        }

        private void StartSetLabels()
        {

            for (int i = 1; i <= 16; i++)
            {
                var buttonName = string.Format("button{0}", i);
                var button = (Button)this.FindName(buttonName);
                Dispatcher.Invoke(new Action(() => button.Visibility = Visibility.Visible));
                Dispatcher.Invoke(new Action(() => button.Content = string.Empty));
                Dispatcher.Invoke(new Action(() => button.Background = Brushes.Transparent));
                Dispatcher.Invoke(new Action(() => button.IsEnabled = true));
                //   Dispatcher.Invoke(new Action(() => textBoxBarcode.Focus()));
            }

        }

        private void RadioButtonTop_Checked(object sender, RoutedEventArgs e)
        {
            var uriSource = new Uri(@"/Resorces/polaryzacja.jpg", UriKind.Relative);
            Dispatcher.Invoke(new Action(() => imagePolarisation.Source = new BitmapImage(uriSource)));
            StartSetLabels();

            Dispatcher.Invoke(new Action(() => image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "top.jpg", UriKind.Absolute))));
            Dispatcher.Invoke(new Action(() => this.Title = "Nadawanie kroku PACE_TOP"));
         //   Dispatcher.Invoke(new Action(() => button13.BorderThickness = new Thickness(5)));
         //   Dispatcher.Invoke(new Action(() => button1.BorderThickness = new Thickness(0)));

            if (SerialNumber.Length > 0 && ListOfSerialNumbers.Count == 16)
            {
                CheckHistoryMes.GetPanelSnList(@SerialNumber);
                CheckHistoryMes.CheckSetupSheetList("11379");
                CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_TOP");
                SwapList();
                CheckLabelForErrorSn();
            }

        }

        private void RadioButtonBot_Checked(object sender, RoutedEventArgs e)
        {
            // @"/WpfApplication1;component/Images/Untitled.png",
            if(this.Title.Equals("Nadawanie kroku PACE_TOP"))
            {
                var uriSource = new Uri(@"/Resorces/polaryzacja_bot.jpg", UriKind.Relative);
                Dispatcher.Invoke(new Action(() => imagePolarisation.Source = new BitmapImage(uriSource)));
            }

        //    var uriSource = new Uri(@"/polaryzacja_bot.jpg", UriKind.RelativeOrAbsolute);
            
   //         Dispatcher.Invoke(new Action(() => imagePolarisation.Source = new BitmapImage(uriSource)));
            StartSetLabels();
            Dispatcher.Invoke(new Action(() => image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "bottom.jpg", UriKind.Absolute))));
            Dispatcher.Invoke(new Action(() => this.Title = "Nadawanie kroku PACE_BOT"));
        //    Dispatcher.Invoke(new Action(() => button13.BorderThickness = new Thickness(0)));
        //    Dispatcher.Invoke(new Action(() => button1.BorderThickness = new Thickness(5)));

            if (SerialNumber.Length > 0 && ListOfSerialNumbers.Count == 16)
            {
                CheckHistoryMes.GetPanelSnList(@SerialNumber);
                CheckHistoryMes.CheckSetupSheetList("12962");
                CheckHistoryMes.CheckSerialNumberByCheckpointEPSList("PACE_BTM");
                CheckLabelForErrorSn();
            }
                

        }

        private void textBoxBarcode_GotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "kropka.jpg", UriKind.Absolute))));
        }

        private void buttonNadaj_Click(object sender, RoutedEventArgs e)
        {
            SendSerialToMes();
        }

        private void SendSerialToMes()
        {
            for (int i = 1; i <= 16; i++)
            {
                var labelName = string.Format("button{0}", i);
                var button = (Button)this.FindName(labelName);

                if (button.Background != Brushes.Red && !ListOfSerialNumbers[i - 1].Contains("FAIL"))
                {
                    if(RadioTop.IsChecked == true)
                        Save.SendLogMesTisAsync(ListOfSerialNumbers[i - 1], "TOP");
                    else if(RadioBot.IsChecked == true)
                        Save.SendLogMesTisAsync(ListOfSerialNumbers[i - 1], "BOT");
                            //    Save.SaveLog(ListOfSerialNumbers[i - 1], "BOT");
                }

            }
            ErrorSnReset();
            StartSetLabels();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            StartSetLabels();
            ErrorSnReset();           
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          //  MessageBox.Show("Nie zmieniaju");
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
           var popop = CheckHistoryMes.CheckSerialNumberByCheckpointEPS(@"0423A262236005233", @"TRILLIANT", @"PACE_BTM"); //23523          
        }
    }
}

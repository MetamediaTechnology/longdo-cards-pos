using LongdoCardsPOS.Controller;
using LongdoCardsPOS.Model;
using LongdoCardsPOS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LongdoCardsPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            BarcodeBox.Focus();
            LoadCard();
        }

        private void BarcodeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadCustomer();
            }
        }

        private void PhoneBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadCustomer();
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCustomer();
        }

        private void LoadCard()
        {
            if (string.IsNullOrEmpty(Settings.Default.CardName))
            {
                Service.GetCards((error, data) =>
                {
                    try
                    {
                        if (error == null)
                        {
                            var card = data.ToArray().First().ToDict();
                            Settings.Default.CardId = card.String("card_id");
                            Settings.Default.CardName = card.String("name");
                            Settings.Default.Save();
                        }
                        LoadCard();
                    }
                    catch (Exception)
                    {
                        CardTextBlock.Text = "You have no card";
                    }
                });
            } else
            {
                CardTextBlock.Text = Settings.Default.CardName;
                LoadRewards();
            }
        }

        private void LoadRewards()
        {
            if (Settings.Default.Rewards == null)
            {
                Service.GetRewards((error, data) =>
                {
                    if (error == null)
                    {
                        if (data == null) return;

                        Settings.Default.Rewards = data.ToArray().Select(x => Reward.FromDict(x)).ToArray();
                        Settings.Default.Save();
                    }
                    LoadRewards();
                });
            }
            else
            {
                RewardListView.ItemsSource = Settings.Default.Rewards;
            }
        }

        private void LoadCustomer()
        {
            Service.GetCustomer(BarcodeBox.Text, PhoneBox.Text, (error, data) =>
            {
                if (error == null)
                {
                    //
                } else
                {
                    MessageBox.Show(error);
                }
            });
        }
    }
}

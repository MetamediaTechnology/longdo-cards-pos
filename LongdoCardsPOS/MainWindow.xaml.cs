using LongdoCardsPOS.Controller;
using LongdoCardsPOS.Model;
using LongdoCardsPOS.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private User user;
        private Reward reward;

        public MainWindow()
        {
            InitializeComponent();

            BarcodeBox.Focus();
            LoadCard();

            Title += FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).ProductVersion;
#if DEBUG
            Title += " DEV";
#endif
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            CardTextBlock.Text = "Loading...";
            var cardId = Settings.Default.CardId;
            Settings.Default.CardId = null;
            LoadCard(cardId);
        }

        private void TicketButton_Click(object sender, RoutedEventArgs e)
        {
            new TicketWindow().Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Service.Logout();
                Util.Logout();
                new LoginWindow().Show();
                Close();
            }
        }

        private void BarcodeBox_KeyUp(object sender, KeyEventArgs e)
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

        private void PointBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddPointButton_Click(null, null);
            }
        }

        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            Service.SubscribeCustomer(user, (error, data) =>
            {
                if (error == null)
                {
                    Status("Subscribe completed", Brushes.Green);
                    ShowExpire(data);               
                }
                else
                {
                    Status(error);
                }
            });
        }

        private void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCustomer()) return;

            int point;
            if (int.TryParse(PointBox.Text, out point))
            {
                Service.AddPoint(user, PointBox.Text, (error, data) =>
                {
                    if (error == null)
                    {
                        PointBox.Text = string.Empty;
                        PointTextBlock.Text = data.ToDict().Dict("point").String("now");

                        Status("Added " + point + " point", Brushes.Green);
                    }
                    else
                    {
                        Status(error);
                    }
                });
            }
            else
            {
                Status("Invalid point");
            }
        }

        private void UsePointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCustomer()) return;
            if (reward == null)
            {
                Status("No reward selected");
                return;
            }

            Service.UsePoint(user, reward, (error, data) =>
            {
                if (error == null)
                {
                    int point;
                    int.TryParse(PointTextBlock.Text, out point);
                    int amount;
                    int.TryParse(reward.Amount, out amount);

                    PointBox.Text = string.Empty;
                    PointTextBlock.Text = (point + amount).ToString();
                    RewardListView.SelectedItem = null;

                    Status("Used " + -amount + " point", Brushes.Green);
                }
                else
                {
                    Status(error);
                }
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var edit = new EditWindow(user);
            edit.Closed += (sender2, e2) =>
            {
                if (user == null) return;

                NameTextBlock.Text = user.Fname + " " + user.Lname;
            };
            edit.ShowDialog();
        }

        private void RewardListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            reward = (Reward)RewardListView.SelectedItem;
            PointBox.Text = reward == null ? string.Empty : reward.Name;
        }

        private void LoadCard(string currentCardId = null)
        {
            if (string.IsNullOrEmpty(Settings.Default.CardId))
            {
                Service.GetCards((error, data) =>
                {
                    try
                    {
                        if (error == null)
                        {
                            IEnumerable<object> array = data.ToArray();
                            if (currentCardId != null)
                            {
                                var next = array.SkipWhile(x => x.ToDict().String("card_id") != currentCardId).Skip(1);
                                array = next.Count() == 0 ? array : next;
                            }

                            var card = array.First().ToDict();
                            Settings.Default.CardId = card.String("card_id");
                            Settings.Default.CardName = card.String("name");
                            Settings.Default.Rewards = null;
                            Settings.Default.Save();
                        }
                        LoadCard(currentCardId);
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
                RewardListView.ItemsSource = new Reward[0];
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
            user = null;

            NameTextBlock.Text = "...";
            ExpireTextBlock.Text = "...";
            ExpireTextBlock.Foreground = Brushes.Black;
            RenewButton.Visibility = Visibility.Collapsed;
            PointTextBlock.Text = "...";
            PointBox.Text = string.Empty;
            EditButton.Content = "New customer";
            EditButton.IsEnabled = true;
            Status("Loading...", Brushes.Gray);
            RewardListView.SelectedItem = null;

            if (string.IsNullOrEmpty(BarcodeBox.Text))
            {
                Status("No customer");
                return;
            }

            Service.GetCustomer(BarcodeBox.Text, (error, data) =>
            {
                if (error == null)
                {
                    BarcodeBox.Text = string.Empty;
                    StatusTextBlock.Text = string.Empty;

                    var dict = data.ToDict();
                    var isPlastic = dict.String("card_type") == "plastic";
                    user = User.FromDict(dict["user_info"], isPlastic);
                    NameTextBlock.Text = user.Fname + " " + user.Lname;

                    ShowExpire(dict["card"]);
                    PointTextBlock.Text = dict.Dict("point").String("now");
                    if (isPlastic)
                    {
                        EditButton.Content = "Edit customer";
                    }
                    else
                    {
                        EditButton.Content = "Edit in app";
                        EditButton.IsEnabled = false;
                    }
                    PointBox.Focus();
                }
                else
                {
                    Status(error);
                }
            });
        }

        private bool CheckCustomer()
        {
            if (user == null)
            {
                Status("No customer");
                return false;
            }
            else
            {
                Status("Updating...", Brushes.Gray);
                return true;
            }
        }

        private void ShowExpire(object card)
        {
            try
            {
                int timestamp;
                int.TryParse(card.ToDict().String("expired"), out timestamp);
                var time = Util.DateTimeFromTimestamp(timestamp);
                ExpireTextBlock.Text = time.ToShortDateString();
                var expire = (time - DateTime.Now).TotalDays;
                if (expire < 30)
                {
                    ExpireTextBlock.Foreground = expire < 0 ? Brushes.Red : Brushes.Orange;
                    RenewButton.Content = "Renew";
                    RenewButton.Visibility = Visibility.Visible;
                } else
                {
                    RenewButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                ExpireTextBlock.Text = "N/A";
                RenewButton.Content = "Subscribe";
                RenewButton.Visibility = Visibility.Visible;
            }
        }

        private void Status(string message, Brush color = null)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = color ?? Brushes.Red;
        }
    }
}

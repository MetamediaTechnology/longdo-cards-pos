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
        private User user;
        private Reward reward;

        public MainWindow()
        {
            InitializeComponent();

            BarcodeBox.Focus();
            LoadCard();
#if DEBUG
            Title += " DEV";
            BarcodeBox.Text = "9999999999999999";
#endif
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            CardTextBlock.Text = "Loading...";
            var cardId = Settings.Default.CardId;
            Settings.Default.CardId = null;
            LoadCard(cardId);
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

        private void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCustomer()) return;

            int point;
            if (int.TryParse(PointBox.Text, out point))
            {
                Service.AddPoint(user.Id, PointBox.Text, (error, data) =>
                {
                    if (error == null)
                    {
                        PointTextBlock.Text = data.ToDict().Dict("point").String("now");

                        StatusTextBlock.Text = "Completed!";
                        StatusTextBlock.Foreground = Brushes.Green;
                    }
                    else
                    {
                        StatusTextBlock.Text = error;
                        StatusTextBlock.Foreground = Brushes.Red;
                    }
                });
            }
            else
            {
                StatusTextBlock.Text = "Invalid point";
                StatusTextBlock.Foreground = Brushes.Red;
            }
        }

        private void UsePointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCustomer()) return;
            if (reward == null)
            {
                StatusTextBlock.Text = "No reward selected";
                StatusTextBlock.Foreground = Brushes.Red;
                return;
            }

            Service.UsePoint(user.Id, reward, (error, data) =>
            {
                if (error == null)
                {
                    int point;
                    int.TryParse(PointTextBlock.Text, out point);
                    int amount;
                    int.TryParse(reward.Amount, out amount);

                    PointTextBlock.Text = (point + amount).ToString();

                    StatusTextBlock.Text = "Completed!";
                    StatusTextBlock.Foreground = Brushes.Green;
                }
                else
                {
                    StatusTextBlock.Text = error;
                    StatusTextBlock.Foreground = Brushes.Red;
                }
            });
        }

        private void SubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            var edit = new EditWindow(user);
            edit.Closed += (sender2, e2) =>
            {
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

            CustomerTextBlock.Text = "Loading...";
            CustomerTextBlock.Foreground = Brushes.Gray;
            CustomerTextBlock.FontSize = 16;

            NameTextBlock.Text = "...";
            ExpireTextBlock.Text = "...";
            PointTextBlock.Text = "...";
            PointBox.Text = string.Empty;
            SubscribeButton.Content = "Subscribe";
            StatusTextBlock.Text = string.Empty;
            RewardListView.SelectedItem = null;

            Service.GetCustomer(BarcodeBox.Text, (error, data) =>
            {
                if (error == null)
                {
                    BarcodeBox.Text = string.Empty;
                    CustomerTextBlock.Text = "Customer Info ---------------";
                    CustomerTextBlock.Foreground = Brushes.Black;
                    CustomerTextBlock.FontSize = FontSize;

                    var dict = data.ToDict();
                    user = User.FromDict(dict["user_info"]);
                    NameTextBlock.Text = user.Fname + " " + user.Lname;

                    int timestamp;
                    int.TryParse(dict.Dict("card").String("expired"), out timestamp);
                    var time = Util.DateTimeFromTimestamp(timestamp);
                    ExpireTextBlock.Text = time.ToShortDateString();
                    var expire = (time - DateTime.Now).TotalDays;
                    ExpireTextBlock.Foreground = expire < 0 ? Brushes.Red : expire < 30 ? Brushes.Orange : Brushes.Black;
                    PointTextBlock.Text = dict.Dict("point").String("now");
                    SubscribeButton.Content = "Edit";
                }
                else
                {
                    CustomerTextBlock.Text = error;
                    CustomerTextBlock.Foreground = Brushes.Red;
                    CustomerTextBlock.FontSize = 16;
                }
            });
        }

        private bool CheckCustomer()
        {
            if (user == null)
            {
                StatusTextBlock.Text = "No customer";
                StatusTextBlock.Foreground = Brushes.Red;
                return false;
            }
            else
            {
                StatusTextBlock.Text = "Updating...";
                StatusTextBlock.Foreground = Brushes.Gray;
                return true;
            }
        }
    }
}

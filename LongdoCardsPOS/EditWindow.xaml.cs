using LongdoCardsPOS.Controller;
using LongdoCardsPOS.Model;
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
using System.Windows.Shapes;

namespace LongdoCardsPOS
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public User User { get; set; }

        public EditWindow(User user)
        {
            InitializeComponent();

            if (user == null)
            {
                user = new User();
                Title = "Subcribe Customer";
            }
            else
            {
                CodeGrid.Visibility = Visibility.Collapsed;
            }
            DataContext = this;
            User = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(User.Id))
            {
                if (string.IsNullOrEmpty(SerialBox.Text))
                {
                    MessageBox.Show("Serial is required");
                    return;
                }
                if (string.IsNullOrEmpty(BarcodeBox.Text))
                {
                    MessageBox.Show("Barcode is required");
                    return;
                }
                if (string.IsNullOrEmpty(User.Mobile))
                {
                    MessageBox.Show("Mobile No. is required");
                    return;
                }
                Service.SubscribeCustomer(SerialBox.Text, BarcodeBox.Text, User, (error, data) =>
                {
                    if (error == null)
                    {
                        MessageBox.Show("Subscribe completed");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(error);
                    }
                });
            } else
            {
                Service.SetCustomer(User, (error, data) =>
                {
                    if (error == null)
                    {
                        MessageBox.Show("Info Updated");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(error);
                    }
                });
            }
        }
    }
}

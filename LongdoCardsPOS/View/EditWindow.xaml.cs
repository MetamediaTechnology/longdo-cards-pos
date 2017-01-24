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
                Title = "New Customer";
            }
            else if (string.IsNullOrEmpty(user.Key))
            {
                Title = "Assign Customer";
                CodeGrid.Visibility = Visibility.Collapsed;
                Height -= 90;
            }
            else
            {
                CodeGrid.Visibility = Visibility.Collapsed;
                Height -= 90;
            }
            DataContext = this;
            User = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var validate = Validate();
            if (string.IsNullOrEmpty(validate))
            {
                Status("Loading...", Brushes.Gray);
            }
            else
            {
                Status(validate);
                return;
            }

            if (string.IsNullOrEmpty(User.Id))
            {
                Service.NewCustomer(SerialBox.Text, BarcodeBox.Text, User, (error, data) =>
                {
                    if (error == null)
                    {
                        MessageBox.Show("Subscribe completed");
                        Close();
                    }
                    else
                    {
                        Status(error);
                    }
                });
            }
            else if(string.IsNullOrEmpty(User.Key))
            {
                User.Key = "pcard_id";
                Service.SubscribeCustomer(User, (error, data) =>
                {
                    if (error == null)
                    {
                        MessageBox.Show("Assign completed");
                        Close();
                    }
                    else
                    {
                        User.Key = null;
                        Status(error);
                    }
                });
            }
            else
            {
                Service.SetCustomer(User, (error, data) =>
                {
                    if (error == null)
                    {
                        MessageBox.Show("Info updated");
                        Close();
                    }
                    else
                    {
                        Status(error);
                    }
                });
            }
        }

        private string Validate()
        {
            if (string.IsNullOrEmpty(User.Id))
            {
                if (string.IsNullOrEmpty(SerialBox.Text)) return "Serial is required";
                if (string.IsNullOrEmpty(BarcodeBox.Text)) return "Barcode is required";
            }

            if (string.IsNullOrEmpty(User.Mobile)) return "Mobile no. is required";

            return null;
        }

        private void Status(string message, Brush color = null)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = color ?? Brushes.Red;
        }
    }
}

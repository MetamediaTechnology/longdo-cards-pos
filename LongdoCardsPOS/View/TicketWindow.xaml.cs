using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    /// Interaction logic for TicketWindow.xaml
    /// </summary>
    public partial class TicketWindow : Window
    {
        public bool IsPoint { get; set; } = true;
        public bool IsMember
        {
            get
            {
                return !IsPoint;
            }
            set
            {
                IsPoint = !value;
            }
        }

        public TicketWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPoint)
            {
                int point;
                if (int.TryParse(AmountBox.Text, out point))
                {
                    Status("Loading...", Brushes.Gray);
                    Service.CreateTicket(AmountBox.Text, RemarkBox.Text, (error, data) =>
                    {
                        if (error == null)
                        {
                            PrintTicket("TK:", data.ToDict().String("serial"), "Scan to get " + point + " points");
                            StatusTextBlock.Text = string.Empty;
                        }
                        else
                        {
                            Status(error);
                        }
                    });
                }
                else
                {
                    Status("Invalid Point");
                }
            }
            else
            {
                Status("Loading...", Brushes.Gray);
                Service.CreateMemberTicket(AmountBox.Text, RemarkBox.Text, (error, data) =>
                {
                    if (error == null)
                    {
                        PrintTicket("MT:", data.ToDict().String("serial"), "Scan to become a member");
                        StatusTextBlock.Text = string.Empty;
                    }
                    else
                    {
                        Status(error);
                    }
                });
            }
        }

        private void PrintTicket(string prefix, string serial, string text)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(prefix + serial, QRCodeGenerator.ECCLevel.M);
            var code = new QRCode(data).GetGraphic(5);

            var font1 = new System.Drawing.Font("Courier New", 8);
            var font2 = new System.Drawing.Font("Tahoma", 12);
            var color = System.Drawing.Brushes.Black;

            var doc = new PrintDocument();
            doc.PrintPage += (sender, e) => {
                e.Graphics.DrawImage(code, 0, 0);
                e.Graphics.DrawString(serial, font1, color, 18, 130);
                text = "Longdo cards" + Environment.NewLine + text;
                if (!String.IsNullOrEmpty(RemarkBox.Text))
                {
                    text += Environment.NewLine + RemarkBox.Text;
                }
                e.Graphics.DrawString(text, font2, color, 15, 145);
            };
            doc.DefaultPageSettings.PaperSize = new PaperSize("Roll", 200, 200);
            doc.Print();
        }

        private void Status(string message, Brush color = null)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = color ?? Brushes.Red;
        }
    }
}

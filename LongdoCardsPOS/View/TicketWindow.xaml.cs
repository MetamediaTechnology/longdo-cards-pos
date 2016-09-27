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
        public TicketWindow()
        {
            InitializeComponent();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            int point;
            if (int.TryParse(AmountBox.Text, out point))
            {
                Status("Loading...", Brushes.Gray);
                Service.CreateTicket(AmountBox.Text, RemarkBox.Text, (error, data) =>
                {
                    if (error == null)
                    {
                        PrintTicket(data.ToDict().String("serial"));
                        StatusTextBlock.Text = string.Empty;
                    }
                    else
                    {
                        Status(error);
                    }
                });
            } else
            {
                Status("Invalid Point");
            }
        }

        private void PrintTicket(string serial)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode("TK:" + serial, QRCodeGenerator.ECCLevel.M);
            var code = new QRCode(data).GetGraphic(5);

            var font1 = new System.Drawing.Font("Courier New", 8);
            var font2 = new System.Drawing.Font("Tahoma", 12);
            var color = System.Drawing.Brushes.Black;

            var doc = new PrintDocument();
            doc.PrintPage += (sender, e) => {
                e.Graphics.DrawImage(code, 0, 0);
                e.Graphics.DrawString(serial, font1, color, 18, 130);
                var text = "Longdo cards" + Environment.NewLine + "Scan to get " + AmountBox.Text + " points";
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

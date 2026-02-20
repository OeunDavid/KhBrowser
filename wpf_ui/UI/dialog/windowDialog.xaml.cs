using System.Linq;
using System.Windows;

namespace ToolKHBrowser.UI.dialog
{
    public partial class windowDialog : Window
    {
        public string Code { get; private set; } = "";

        // default constructor (required for XAML designer)
        public windowDialog()
        {
            InitializeComponent();
            Loaded += (_, __) => txtCode.Focus();
        }

        // constructor with accountId
        public windowDialog(string accountId) : this()
        {
            if (!string.IsNullOrWhiteSpace(accountId))
                Title = $"2FA Required - {accountId}";
            else
                Title = "2FA Required";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var raw = txtCode.Text ?? "";
            var digits = new string(raw.Where(char.IsDigit).ToArray());

            if (digits.Length < 6)
            {
                MessageBox.Show("Please enter a valid 6-digit code.",
                                "Invalid Code",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            Code = digits;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
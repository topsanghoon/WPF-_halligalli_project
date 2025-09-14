using System.Windows;

namespace halligalli_client
{
    public partial class NameInputWindow : Window
    {
        public string UserName { get; private set; } = "";

        public NameInputWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                UserName = NameTextBox.Text.Trim();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("이름을 입력하세요.");
            }
        }
    }
}


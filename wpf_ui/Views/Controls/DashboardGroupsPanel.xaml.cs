using System.Windows.Controls;

namespace ToolKHBrowser.Views.Controls
{
    public partial class DashboardGroupsPanel : UserControl
    {
        public bool IsInviteFriendsToGroupChecked => tglInviteFriends?.IsChecked == true;

        public DashboardGroupsPanel()
        {
            InitializeComponent();
        }
    }
}

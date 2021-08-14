using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FileManager.Models;


namespace FileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void GroupsCillection_OnFilter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is WorkMenuList group)) return;
            if (group.Name is null) return;

            var filter_text = GroupNameFilterText.Text;
            if (filter_text.Length == 0) return;

            if (group.Name.Contains(filter_text, StringComparison.OrdinalIgnoreCase)) return;
            if (group.Descriptions != null) return;


            e.Accepted = false;
        }

        private void OnGroupsFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            var text_box = (TextBox)sender;
            var collection = (CollectionViewSource)text_box.FindResource("GroupsCollection");
            collection.View.Refresh();
        }

        private void MenuItem_TargetUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void MenuItem_TargetUpdated_1(object sender, DataTransferEventArgs e)
        {

        }
    }
}

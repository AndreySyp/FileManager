using System.Windows;

namespace FileManager.Views
{
    /// <summary>
    /// Логика взаимодействия для MenuListEditorWindow.xaml
    /// </summary>
    public partial class MenuListEditorWindow : Window
    {
        #region Name

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(
                nameof(Name),
                typeof(string),
                typeof(MenuListEditorWindow),
                new PropertyMetadata(null));

        public string Name { get => (string)GetValue(NameProperty); set => SetValue(NameProperty, value); }

        #endregion

        #region Descriptions

        public static readonly DependencyProperty DescriptionsProperty =
            DependencyProperty.Register(
                nameof(Descriptions),
                typeof(string),
                typeof(MenuListEditorWindow),
                new PropertyMetadata(null));

        public string Descriptions { get => (string)GetValue(DescriptionsProperty); set => SetValue(DescriptionsProperty, value); }

        #endregion

        public MenuListEditorWindow()
        {
            InitializeComponent();
        }
    }
}

using FileManager.Infrastructure.Commands;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace FileManager.Views
{
    /// <summary>
    /// Логика взаимодействия для FileEditorWindow.xaml
    /// </summary>
    public partial class FileEditorWindow : Window
    {

        #region Name

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(
                nameof(Name),
                typeof(string),
                typeof(FileEditorWindow),
                new PropertyMetadata(null));

        public string Name { get => (string)GetValue(NameProperty); set => SetValue(NameProperty, value); }

        #endregion

        #region Directory

        public static readonly DependencyProperty DirectoryProperty =
            DependencyProperty.Register(
                nameof(Directory),
                typeof(string),
                typeof(FileEditorWindow),
                new PropertyMetadata(null));

        public string Directory { get => (string)GetValue(DirectoryProperty); set => SetValue(DirectoryProperty, value); }

        #endregion

        #region Tag

        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register(
                nameof(Tag),
                typeof(string),
                typeof(FileEditorWindow),
                new PropertyMetadata(null));

        public string Tag { get => (string)GetValue(TagProperty); set => SetValue(TagProperty, value); }

        #endregion

        #region  WindowSelectCommand

        private ICommand _WindowSelectCommand;

        public ICommand WindowSelectCommand => _WindowSelectCommand ??= new LambdaCommand(OnWindowSelectCommandExecuted, CanWindowSelectCommandExecute);
        private static bool CanWindowSelectCommandExecute(object p) => true;

        private void OnWindowSelectCommandExecuted(object p)
        {
            var dlg = new OpenFileDialog();


            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                Directory = dlg.FileName;
            }
        }

        #endregion

        public FileEditorWindow()
        {
            InitializeComponent();
        }
    }
}

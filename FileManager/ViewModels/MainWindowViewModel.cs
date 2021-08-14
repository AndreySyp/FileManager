using FileManager.Infrastructure.Commands;
using FileManager.Models;
using FileManager.ViewModels.Base;
using FileManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FileManager.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        public ObservableCollection<WorkMenuList> SpaceData { get; }

        #region Select

        #region SelectMenuList

        private WorkMenuList _SelectSpaceData;
        public WorkMenuList SelectedSpaceData
        {
            get => _SelectSpaceData;
            set
            {
                if (!Set(ref _SelectSpaceData, value)) return;
                _SelectedMenuList.Source = value?.File;
                OnPropertyChanged(nameof(SelectedMenuList));
            }
        }

        #endregion

        #region SelectFile

        private WorkSpaceData _SelectFile;
        public WorkSpaceData SelectFile
        {
            get => _SelectFile;
            set => Set(ref _SelectFile, value);

            //set
            //{
            //    if (!Set(ref _SelectFile, value)) return;
            //    OnPropertyChanged(nameof(SelectedMenuList));
            //}
        }

        #endregion

        #region SelectedMenuListFilter

        private readonly CollectionViewSource _SelectedMenuList = new CollectionViewSource();

        public ICollectionView SelectedMenuList => _SelectedMenuList?.View;

        private void OnWorkSpaceFiltred(object sender, FilterEventArgs e)
        {
            if (!(e.Item is WorkSpaceData student))
            {
                e.Accepted = false;
                return;
            }

            var filter_text = _SpaceDataFilterText;
            if (string.IsNullOrWhiteSpace(filter_text)) return;

            if (student.Name is null || student.Tag is null)
            {
                e.Accepted = false;
                return;
            }

            if (student.Name.Contains(filter_text, StringComparison.OrdinalIgnoreCase)) return;
            if (student.Tag.Contains(filter_text, StringComparison.OrdinalIgnoreCase)) return;

            e.Accepted = false;

        }

        #endregion

        #endregion

        #region Commands

        #region CloseApplicationCommand

        public ICommand _CloseApplicationComand;
        public ICommand CloseApplicationComand => _CloseApplicationComand ??= new LambdaCommand(OnCloseApplicationComandExecuted, CanCloseApplicationComandExecuted);
        
        private bool CanCloseApplicationComandExecuted(object p) => true;
        
        private void OnCloseApplicationComandExecuted(object p)
        {
            SaveFile();
            Application.Current.Shutdown();
        }

        #endregion

        #region CreateMenuListCommand
        public ICommand _CreateMenuListCommand;
        public ICommand CreateMenuListCommand => _CreateMenuListCommand ??= new LambdaCommand(OnCreateMenuListCommandExecute, CanCreateMenuListCommandExecute);

        private bool CanCreateMenuListCommandExecute(object p) => true;

        private void OnCreateMenuListCommandExecute(object p)
        {
            var dlg = new MenuListEditorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() != true)
            {
                ShowInformation("WorkSpace not create", "File manager");
                return;
            }

            if (dlg.Name == null)
            {
                var list_index = SpaceData.Count + 1;
                dlg.Name = $"Пространство {list_index}";
            }
            else
            {
                dlg.Name = DeleteVoidSymbol(dlg.Name);

                if (dlg.Name == "")
                {
                    var list_index = SpaceData.Count + 1;
                    dlg.Name = $"Пространство {list_index}";
                }
            }


            var new_list = new WorkMenuList
            {
                Name = dlg.Name,
                Descriptions = dlg.Descriptions,
                File = new ObservableCollection<WorkSpaceData>()
            };

            SpaceData.Add(new_list);
            ShowInformation("List create", "File manager");
        }

        #endregion

        #region EditMenuListCommand
        public ICommand _EditMenuListCommand;
        public ICommand EditMenuListCommand => _EditMenuListCommand ??= new LambdaCommand(OnEditMenuListCommandExecute, CanEditMenuListCommandExecute);

        private bool CanEditMenuListCommandExecute(object p) => p is WorkMenuList;

        private void OnEditMenuListCommandExecute(object p)
        {
            var list = (WorkMenuList)p;

            var dlg = new MenuListEditorWindow
            {
                Name = list.Name,
                Descriptions = list.Descriptions,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (dlg.ShowDialog() != true)
            {
                ShowInformation("WorkSpace not edit", "File manager");
                return;
            }

            
            if (dlg.Name == null)
            {
                var list_index = SpaceData.Count + 1;
                dlg.Name = $"Пространство {list_index}";
            }
            else
            {
                dlg.Name = DeleteVoidSymbol(dlg.Name);

                if (dlg.Name == "")
                {
                    var list_index = SpaceData.Count + 1;
                    dlg.Name = $"Пространство {list_index}";
                }
                else list.Name = dlg.Name;
            }
            list.Descriptions = dlg.Descriptions;

        }

        #endregion

        #region DeleteMenuListCommand
        public ICommand _DeleteMenuListCommand;
        public ICommand DeleteMenuListCommand => _DeleteMenuListCommand ??= new LambdaCommand(OnDeleteMenuListCommandExecute, CanDeleteMenuListCommandExecute);

        private bool CanDeleteMenuListCommandExecute(object p) => p is WorkMenuList group && SpaceData.Contains(group);

        private void OnDeleteMenuListCommandExecute(object p)
        {
            if (!(p is WorkMenuList list)) return;

            var list_index = SpaceData.IndexOf(list);
            SpaceData.Remove(list);

            if (list_index < SpaceData.Count) SelectedSpaceData = SpaceData[list_index];
        }

        #endregion


        #region OpenFileCommand

        public ICommand _OpenFileCommand;
        public ICommand OpenFileCommand => _OpenFileCommand ??= new LambdaCommand(OnOpenFileCommandExecute, CanOpenFileCommandExecute);

        private bool CanOpenFileCommandExecute(object p) => true;

        private void OnOpenFileCommandExecute(object p)
        {
            var path = SelectFile.Directory.ToString();
            if (path == null) return;
            if (!File.Exists(path))
            {
                ShowError("File not found", "File manager");
                return;
            }

            var ProcessOpen = new Process
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                }
            };
            ProcessOpen.Start();
        }

        #endregion

        #region EditFileCommand

        private ICommand _EditFileCommand;

        public ICommand EditFileCommand => _EditFileCommand ??= new LambdaCommand(OnEditFileCommandExecuted, CanEditFileCommandExecute);

        private static bool CanEditFileCommandExecute(object p) => p is WorkSpaceData;

        private void OnEditFileCommandExecuted(object p)
        {
            WorkSpaceData file = (WorkSpaceData)p;

            var dlg = new FileEditorWindow
            {
                Name = file.Name,
                Directory = file.Directory,
                Tag = file.Tag,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() != true)
            {
                ShowInformation("File not edited", "File manager");
                return;
            }
            if (!File.Exists(dlg.Directory))
            {
                ShowError("File not found", "File manager");
                return;
            }


            if (file.Directory == dlg.Directory)
            {
                file.Name = dlg.Name;
                file.Tag = dlg.Tag;
                ShowInformation("File edited", "File manager");
                return;
            }
            else
            {
                var FileInfo = new FileInfo(dlg.Directory);

                if (dlg.Name == null)
                    file.Name = FileInfo.Name;
                else
                {
                    dlg.Name = DeleteVoidSymbol(dlg.Name);


                    if (dlg.Name == "") dlg.Name = FileInfo.Name;
                    else dlg.Name = dlg.Name;
                }

                file.Tag = dlg.Tag;
                file.Directory = dlg.Directory;
                file.DateChanged = FileInfo.LastWriteTimeUtc;
                file.Size = FileInfo.Length;
            }
        }

        #endregion

        #region DeleteFileCommand

        public ICommand _DeleteFileCommand;
        public ICommand DeleteFileCommand => _DeleteFileCommand ??= new LambdaCommand(OnDeleteFileCommandExecute, CanDeleteFileCommandExecute);

        private bool CanDeleteFileCommandExecute(object p) => p is WorkSpaceData;

        private void OnDeleteFileCommandExecute(object p)
        {
            WorkSpaceData file = (WorkSpaceData)p;
            SelectedSpaceData.File.Remove(file);
            ShowInformation("File delete", "File manager");
        }

        #endregion

        #region AddNewFileCommand

        private ICommand _AddNewFileCommand;

        public ICommand AddNewFileCommand => _AddNewFileCommand ??= new LambdaCommand(OnAddNewFileCommandExecuted, CanAddNewFileCommandExecute);
        private static bool CanAddNewFileCommandExecute(object p) => p is WorkMenuList;

        private void OnAddNewFileCommandExecuted(object p)
        {
            var list = (WorkMenuList)p;
            var data = new WorkSpaceData();

            var dlg = new FileEditorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() != true)
            {
                ShowInformation("File not create", "File manager");
                return;
            }
            if (!File.Exists(dlg.Directory))
            {
                ShowError("File not found", "File manager");
                return;
            }

            var FileInfo = new FileInfo(dlg.Directory);

            if (dlg.Name == null)
                data.Name = FileInfo.Name;
            else
            {
                dlg.Name = DeleteVoidSymbol(dlg.Name);


                if (dlg.Name == "") data.Name = FileInfo.Name;
                else data.Name = dlg.Name;
            }

            data.Directory = dlg.Directory;
            data.Tag = dlg.Tag;
            data.DateChanged = FileInfo.LastWriteTimeUtc;
            data.Size = FileInfo.Length;

            list.File.Add(data);
            ShowInformation("File create", "File manager");
        }

        #endregion  

        #region CreateFileCommandCommand

        private ICommand _CreateNewFileCommand;

        public ICommand CreateNewFileCommand => _CreateNewFileCommand ??= new LambdaCommand(OnCreateNewFileCommandExecuted, CanCreateNewFileCommandExecute);
        private static bool CanCreateNewFileCommandExecute(object p) => p is WorkMenuList;

        private void OnCreateNewFileCommandExecuted(object p)
        {
            var list = (WorkMenuList)p;
            var data = new WorkSpaceData();

            var dlg = new FileEditorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() != true)
            {
                ShowInformation("File not create", "File manager");
                return;
            }

            File.Create(dlg.Directory);
            var FileInfo = new FileInfo(dlg.Directory);

            if (dlg.Name == null)
                data.Name = FileInfo.Name;
            else
            { 
                dlg.Name = DeleteVoidSymbol(dlg.Name);


                if (dlg.Name == "") data.Name = FileInfo.Name;
                else data.Name = dlg.Name;
            }

            data.Tag = dlg.Tag;
            data.Directory = dlg.Directory;
            data.DateChanged = FileInfo.LastWriteTimeUtc;
            data.Size = FileInfo.Length;

            list.File.Add(data);
            ShowInformation("File create", "File manager");
        }

        #endregion


        #region SaveCommand

        public ICommand _SaveCommand;
        public ICommand SaveCommand => _SaveCommand ??= new LambdaCommand(OnSaveCommandExecute, CanSaveCommandExecute);

        private bool CanSaveCommandExecute(object p) => true;

        private void OnSaveCommandExecute(object p)
        {
            SaveFile();
        }

        #endregion


        #region HelperOpenCommand

        public ICommand _HelperOpenCommand;
        public ICommand HelperOpenCommand => _HelperOpenCommand ??= new LambdaCommand(OnHelperOpenCommandExecute, CanHelperOpenCommandExecute);

        private bool CanHelperOpenCommandExecute(object p) => true;

        private void OnHelperOpenCommandExecute(object p)
        {
            var dlg = new Helper
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            dlg.Show();
        }

        #endregion
        #endregion

        #region SpaceDataFilterText

        private string _SpaceDataFilterText;

        public string FileFilterText
        {
            get => _SpaceDataFilterText;
            set
            {
                if (!Set(ref _SpaceDataFilterText, value)) return;
                _SelectedMenuList.View.Refresh();
            }
        }

        #endregion

        #region Title

        private string _Title = "File Manager";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);

        }

        #endregion


        #region Function
        const string SaveName = "Saves.txt";
        const string VoidSymbol = ".,.";
        const string Delimiter = " :: ";

        void SaveFile()
        {
            using StreamWriter file = new StreamWriter(SaveName);
            foreach (var itemList in SpaceData)
            {
                file.Write(itemList.Name + Delimiter);
                if (itemList.Descriptions != null) file.WriteLine(itemList.Descriptions);
                else file.WriteLine(VoidSymbol);


                foreach (var itemFile in itemList.File)
                {
                    file.Write(itemFile.Name + Delimiter + itemFile.Size + Delimiter
                        + itemFile.DateChanged + Delimiter + itemFile.Directory);

                    if (itemFile.Tag != null && itemFile.Tag != "") file.WriteLine(Delimiter + itemFile.Tag);
                    else file.WriteLine(Delimiter + VoidSymbol);
                }
                file.WriteLine("}");
            }
            file.WriteLine("end");
        }
        void LoadFile()
        {
            try
            {
                using var file = new StreamReader(SaveName);
                string str;

                while ((str = file.ReadLine()) != "end")
                {
                    #region Read
                    if (str == "" || str == null) break;

                    string[] wordList = str.Split(Delimiter);
                    string NameSp = wordList[0];
                    string Dis = wordList[1];
                    if (Dis == VoidSymbol) Dis = null;


                    List<string> nameFile = new List<string>();
                    List<int> size = new List<int>();
                    List<DateTime> dateChange = new List<DateTime>();
                    List<string> directory = new List<string>();
                    List<string> tag = new List<string>();


                    int j;
                    for (j = 0; ; j++)
                    {
                        str = file.ReadLine();
                        if (str == "}") break;

                        string[] wordFiles = str.Split(Delimiter);

                        nameFile.Add(wordFiles[0]);
                        size.Add(int.Parse(wordFiles[1]));
                        dateChange.Add(DateTime.Parse(wordFiles[2]));
                        directory.Add(wordFiles[3]);

                        tag.Add(wordFiles[4]);
                        if (tag[j] == VoidSymbol) tag[j] = null;
                    }

                    #endregion

                    #region CreateAndAdd

                    var spaces = Enumerable.Range(0, j).Select(i => new WorkSpaceData
                    {
                        Name = nameFile[i],
                        Size = size[i],
                        DateChanged = dateChange[i],
                        Directory = directory[i],
                        Tag = tag[i]
                    });
                    var list = new WorkMenuList
                    {
                        Name = NameSp,
                        Descriptions = Dis,
                        File = new ObservableCollection<WorkSpaceData>(spaces)
                    };

                    SpaceData.Add(list);

                    #endregion
                }
            }
            catch
            {
                File.Create(SaveName);
            }
        }
        string DeleteVoidSymbol(string str)
        {
            char[] charArray = str.ToCharArray();
            int countVoid;

            for (countVoid = 0; countVoid < str.Length; countVoid++)
                if (charArray[countVoid] != ' ') break;
            
            str = str.Remove(0, countVoid);
            return str;
        }

        #endregion

        public MainWindowViewModel()
        {
            SpaceData = new ObservableCollection<WorkMenuList>();
            LoadFile();
            

            _SelectedMenuList.Filter += OnWorkSpaceFiltred;

            //_SelectedGroupStudents.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            //_SelectedGroupStudents.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
        }


    }



}

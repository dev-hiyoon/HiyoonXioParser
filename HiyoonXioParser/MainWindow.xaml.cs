using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HiyoonXioParser
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private String path;
        private String isoFileName = "hiyoon.txt";
        private ObservableCollection<XIOFile> XIOsAll = new ObservableCollection<XIOFile>();
        private ObservableCollection<XIOFile> XIOs = new ObservableCollection<XIOFile>();
        private ObservableCollection<XIOData> XIODatas = new ObservableCollection<XIOData>();

        public MainWindow()
        {
            InitializeComponent();

            this.lbXio.ItemsSource = XIOs;
            this.lvXio.ItemsSource = XIODatas;

            this.btnParsing.Click += BtnParsing_Click;
            this.btnSearch.Click += BtnSearch_Click;
            this.tbFilter.TextChanged += TbFilter_TextChanged;
            CommandBinding lbCb = new CommandBinding(ApplicationCommands.Copy, LbCopyCmdExecuted, LbCopyCmdCanExecute);
            this.lbXio.CommandBindings.Add(lbCb);
            CommandBinding lvCb = new CommandBinding(ApplicationCommands.Copy, LvCopyCmdExecuted, LvCopyCmdCanExecute);
            this.lvXio.CommandBindings.Add(lvCb);

            this.tbPath.Text = initFilePath();
            this.BtnSearch_Click(null, null);
        }

        void LbCopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;
            string copyContent = String.Empty; 
            foreach (XIOFile item in lb.SelectedItems)
            {
                copyContent = item.Name;
                copyContent += Environment.NewLine;
            }

            Clipboard.SetText(copyContent);
        }

        void LbCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;
            // CanExecute only if there is one or more selected Item.   
            if (lb.SelectedItems.Count > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        void LvCopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ListView lv = e.OriginalSource as ListView;
            string copyContent = String.Empty;
            foreach (XIOData item in lv.SelectedItems)
            {
                copyContent += String.Format("{0} | {1} | {2} | {3}", item.Id, item.Name, item.Length, item.Value);
                copyContent += Environment.NewLine;
            }

            Clipboard.SetText(copyContent);
        }

        void LvCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListView lv = e.OriginalSource as ListView; 
            if (lv.SelectedItems.Count > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void TbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            XIOs.Clear();
            XIOsAll.Where(x => String.IsNullOrEmpty(this.tbFilter.Text) || x.Name.Contains(this.tbFilter.Text)).ToList().ForEach(XIOs.Add);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            XIOs.Clear();
            XIOsAll.Clear();
            path = this.tbPath.Text;
            this.writeFilePath(path);
            string[] filePaths = Directory.GetFiles(@path, "*.xio");
            if (filePaths == null || filePaths.Length < 1)
            {
                MessageBox.Show("해당 경로에 xio파일이 없네요");
                return;
            }

            foreach (string filePath in filePaths)
            {
                XIOsAll.Add(new XIOFile(filePath));
            }

            XIOsAll.Where(x => String.IsNullOrEmpty(this.tbFilter.Text) || x.Name.Contains(this.tbFilter.Text)).ToList().ForEach(XIOs.Add);
        }

        private void BtnParsing_Click(object sender, RoutedEventArgs e)
        {
            if (this.lbXio.SelectedItem == null)
            {
                MessageBox.Show("XIO 전문을 선택해주세요");
                return;
            }

            this.XIODatas.Clear();
            XIOFile xIOFile = this.lbXio.SelectedItem as XIOFile;
            List<XIOData> xioDatas = XIOParser.parse(xIOFile.FilePath, this.tbXIO.Text);
            xioDatas.ToList().ForEach(this.XIODatas.Add);
        }

        private String initFilePath()
        {
            String path = @"C:\";
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(this.isoFileName))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(this.isoFileName, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        path = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(this.isoFileName, FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.WriteLine(path);
                    }
                }
            }

            return path;
        }

        private void writeFilePath(String newPath)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(this.isoFileName, FileMode.Open, isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(newPath);
                }
            }
        }
    }
}

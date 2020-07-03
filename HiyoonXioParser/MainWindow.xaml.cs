using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HiyoonXioParser
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private String path;
        private String isoFileName = "hiyoon.txt";
        private String isoXioFileName = "hiyoonxio.txt";
        private static int preIdx = 1109;
        private static int postIdx = 2;
        private BackgroundWorker xioFileSearchWorker;
        private BackgroundWorker tbPasteWorker;
        private ObservableCollection<XIOFile> XIOsAll = new ObservableCollection<XIOFile>();
        private ObservableCollection<XIOFile> XIOs = new ObservableCollection<XIOFile>();
        private ObservableCollection<XIOData> XIODatas = new ObservableCollection<XIOData>();

        public MainWindow()
        {
            InitializeComponent();

            this.lbXio.ItemsSource = XIOs;
            this.lvXio.ItemsSource = XIODatas;

            this.btnXioParse.Click += BtnXioParse_Click;
            this.btnContentsParse.Click += BtnContentsParse_Click;
            this.btnMerge.Click += BtnMerge_Click;
            this.btnSearch.Click += BtnSearch_Click;
            this.tbFilter.TextChanged += TbFilter_TextChanged;
            DataObject.AddPastingHandler(this.tbXIO, OnPaste);
            xioFileSearchWorker = new BackgroundWorker();
            xioFileSearchWorker.DoWork += new DoWorkEventHandler(xioFileSearchWorker_DoWork);
            xioFileSearchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(xioFileSearchWorker_RunWorkerCompleted);
            tbPasteWorker = new BackgroundWorker();
            tbPasteWorker.DoWork += new DoWorkEventHandler(tbPasteWorker_DoWork);
            tbPasteWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(tbPasteWorker_RunWorkerCompleted);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CommandBinding lbCb = new CommandBinding(ApplicationCommands.Copy, LbCopyCmdExecuted, LbCopyCmdCanExecute);
            this.lbXio.CommandBindings.Add(lbCb);
            CommandBinding lvCb = new CommandBinding(ApplicationCommands.Copy, LvCopyCmdExecuted, LvCopyCmdCanExecute);
            this.lvXio.CommandBindings.Add(lvCb);

            this.tbPath.Text = initIsoFile(this.isoFileName);
            this.initIsoFile(this.isoXioFileName);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            tbPasteWorker.RunWorkerAsync();
        }

        private void BtnContentsParse_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.tbXIO.Text))
            {
                MessageBox.Show("분석할 내용이 없습니다.");
                return;
            }

            tbPasteWorker.RunWorkerAsync();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
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
            this.btnSearch.IsEnabled = false;
            XIOs.Clear();
            XIOsAll.Clear();
            path = this.tbPath.Text;
            writeIsoFile(this.isoFileName);
            xioFileSearchWorker.RunWorkerAsync();
        }

        void xioFileSearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<XIOFile> xioFileListTemp = new List<XIOFile>();
            string[] filePaths = Directory.GetFiles(@path.Replace(System.Environment.NewLine, ""), "*.xio");
            if (filePaths != null && filePaths.Length > 0)
            {
                foreach (string filePath in filePaths)
                {
                    xioFileListTemp.Add(new XIOFile(filePath));
                }
            }

            string[] directories = Directory.GetDirectories(@path.Replace(System.Environment.NewLine, ""), "xio", SearchOption.AllDirectories);
            if (directories != null && directories.Length > 0)
            {
                foreach (String directory in directories)
                {
                    if (directory.Contains("target"))
                    {
                        continue;
                    }

                    string[] subFilePaths = Directory.GetFiles(@directory.Replace(System.Environment.NewLine, ""), "*.xio");
                    if (subFilePaths != null && subFilePaths.Length > 0)
                    {
                        foreach (string filePath in subFilePaths)
                        {
                            xioFileListTemp.Add(new XIOFile(filePath));
                        }
                    }
                }
            }

            XIOsAll.Clear();
            xioFileListTemp.GroupBy(x => x.Name).Select(x => x.First()).ToList().ForEach(XIOsAll.Add);
            writeIsoFile(this.isoXioFileName);
        }

        void xioFileSearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btnSearch.IsEnabled = true;

            // 에러가 있는지 체크
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
                return;
            }

            if (XIOsAll.Count < 1)
            {
                MessageBox.Show("해당 경로에 xio파일이 없네요");
                return;
            }

            XIOs.Clear();
            XIOsAll.Where(x => String.IsNullOrEmpty(this.tbFilter.Text) || x.Name.Contains(this.tbFilter.Text)).ToList().ForEach(XIOs.Add);
        }
        void tbPasteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        void tbPasteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            byte[] xioByte = XIOParser.getByte(this.tbXIO.Text);
            if (XIOsAll.Where(x => x.TotalLength == xioByte.Length || (x.TotalLength == xioByte.Length - preIdx - postIdx)).Count() > 0)
            {
                XIOs.Clear();
                XIOsAll.Where(x => x.TotalLength == xioByte.Length || (x.TotalLength == xioByte.Length - preIdx - postIdx)).ToList().ForEach(XIOs.Add);
            }
        }

        private void BtnXioParse_Click(object sender, RoutedEventArgs e)
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

        private void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (this.XIODatas == null || this.XIODatas.Count < 1)
            {
                MessageBox.Show("XIO Data가 없습니다.");
                return;
            }

            String result = XIOParser.merge(this.XIODatas.ToList());
            this.tbXIO.Text = result;
        }
        
        private String initIsoFile(String fileName)
        {
            String path = @"C:\";
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.FileExists(fileName))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(fileName, FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        if (fileName == this.isoFileName)
                        {
                            path = reader.ReadToEnd().Replace(System.Environment.NewLine, "");
                        }
                        else if (fileName == this.isoXioFileName)
                        {
                            String xioList = path = reader.ReadToEnd();
                            if (!String.IsNullOrEmpty(xioList))
                            {
                                String[] arrXio = xioList.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (String xioData in arrXio)
                                {
                                    if (!String.IsNullOrEmpty(xioData))
                                    {
                                        try
                                        {
                                            this.XIOsAll.Add(new XIOFile(xioData));
                                        } catch (Exception ex)
                                        {
                                        }
                                    }
                                }

                                XIOs.Clear();
                                XIOsAll.Where(x => String.IsNullOrEmpty(this.tbFilter.Text) || x.Name.Contains(this.tbFilter.Text)).ToList().ForEach(XIOs.Add);
                            }
                        }
                    }
                }
            }
            else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(fileName, FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.Write(path);
                    }
                }
            }

            return path;
        }

        private void writeIsoFile(String fileName)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(fileName, FileMode.Create, isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    if (fileName == this.isoFileName)
                    {
                        writer.Write(path);
                    }
                    else if (fileName == this.isoXioFileName)
                    {
                        this.XIOsAll.ToList().ForEach(x => writer.WriteLine(x.FilePath));
                    }
                }
            }
        }

        private void lbXio_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                DependencyObject mouseItem = e.OriginalSource as DependencyObject;
                if (mouseItem != null)
                {
                    // Get the container based on the element
                    var container = listBox.ContainerFromElement(mouseItem);
                    if (container != null)
                    {
                        var index = listBox.ItemContainerGenerator.IndexFromContainer(container);
                        listBox.SelectedIndex = index;
                    }
                }
            }
        }
    }
}

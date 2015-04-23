using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using Peter;
using ShellTestApp;

namespace DocumentIconDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class ListBoxData
        {
            public BitmapSource ItemIcon { get; set; }
            public string ItemText { get; set; }
        }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textField.Text = "";
        }

          private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = new List<ListBoxData>();
            string filePath = "c:\\temp\\test.xml";
         //   foreach (var filePath in dir)
            {
                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            sysicon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();

                var itm = new ListBoxData() { ItemIcon = bmpSrc, ItemText = filePath };

                data.Add(itm);
            }

            lstBox.ItemsSource = data;

          //  test();
          }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textField.Text = dialog.SelectedPath;

                if (textField.Text == "")
                    return;

                string[] dir = Directory.GetFiles(textField.Text);

                var data = new List<ListBoxData>();

                foreach (var filePath in dir)
                {
                    var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    sysicon.Dispose();

                    var itm = new ListBoxData() { ItemIcon = bmpSrc, ItemText = filePath };

                    data.Add(itm);
                }

                lstBox.ItemsSource = data;
            }
            else
                textField.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        void test()
        {

            ShellContextMenu ctxMnu = new ShellContextMenu();
            FileInfo[] arrFI = new FileInfo[1];
            arrFI[0] = new FileInfo("c:\\temp\\test.xml"); // (this.treeMain.SelectedNode.Tag.ToString());
            ctxMnu.ShowContextMenu(arrFI, new System.Drawing.Point(20,20)); //(e.X, e.Y)));
        }

        private void lstBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
             Point clickPoint = e.GetPosition(this);
         var vv= this.PointToScreen(new Point(clickPoint.X, clickPoint.Y));
            

            ShellContextMenu ctxMnu = new ShellContextMenu();
            FileInfo[] arrFI = new FileInfo[1];
            arrFI[0] = new FileInfo("c:\\temp\\test.xml"); // (this.treeMain.SelectedNode.Tag.ToString());
            //ctxMnu.ShowContextMenu(arrFI, new System.Drawing.Point((int)clickPoint.X, (int)clickPoint.Y));
            ctxMnu.ShowContextMenu(arrFI, new System.Drawing.Point((int)vv.X, (int)vv.Y));

        }
    }
}

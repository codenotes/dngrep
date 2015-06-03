

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using dnGREP.Common;
using dnGREP.Engines;
using NLog;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using dnGREP.Common.UI;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Blue.Windows;
using Jam.Shell;
using Everything;


namespace dnGREP.WPF
{
    /// <summary>
	/// Interaction logic for MainForm.xaml
    /// </summary>
    public partial class MainFormEx : Window
    {
        public static MainFormEx gMainForm = null;
		private static Logger logger = LogManager.GetCurrentClassLogger();
        private MainViewModel inputData;
        private bool isVisible = true;

     

        public MainFormEx()
            : this (true)
        {
            gMainForm = this;
        }

        public MainFormEx(bool isVisible)
        {
            InitializeComponent();
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            if (!UiUtils.IsOnScreen(this))
                UiUtils.CenterWindow(this);
            this.isVisible = isVisible;
            inputData = new MainViewModel();
            this.DataContext = inputData;

            changeTab(1);
            SHFileList.Scrollable = true;


        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hwnd, IntPtr hwndNewParent);

        private const int HWND_MESSAGE = -3;

        private IntPtr hwnd;
        private IntPtr oldParent;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (!isVisible)
            {
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

                if (hwndSource != null)
                {
                    hwnd = hwndSource.Handle;
                    oldParent = SetParent(hwnd, (IntPtr)HWND_MESSAGE);
                    Visibility = Visibility.Hidden;
                }
            }
        }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            inputData.StickyWindow = new StickyWindow(this);
            inputData.StickyWindow.StickToScreen = true;
            inputData.StickyWindow.StickToOther = true;
            inputData.StickyWindow.StickOnResize = true;
            inputData.StickyWindow.StickOnMove = true;
            DataObject.AddPastingHandler(tbSearchFor, new DataObjectPastingEventHandler(onPaste));
            DataObject.AddPastingHandler(tbReplaceWith, new DataObjectPastingEventHandler(onPaste));

         //   System.Windows.Forms.Integration.WindowsFormsHost host =
         //       new System.Windows.Forms.Integration.WindowsFormsHost();

         //   // Create the MaskedTextBox control.
         ////   TextBox mtbDate = new TextBox();
         //  var j=new Jam.Shell.FileList();
         //  // mtbDate.Text = "hitherebob";
         //   // Assign the MaskedTextBox control as the host control's child.
         //   host.Child = j;

         //   // Add the interop host control to the Grid 
         //   // control's collection of child controls. 
         //   this.grid1.Children.Add(host);

		}

        /// <summary>
        /// Workaround to enable pasting tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text, true);
            if (!isText) return;
            var senderControl = (Control)sender;
            var textBox = (TextBox)senderControl.Template.FindName("PART_EditableTextBox", senderControl);
            textBox.AcceptsTab = true;
            var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                textBox.AcceptsTab = false;
            }), null);
        }		

		private void MainForm_Closing(object sender, CancelEventArgs e)
		{
			Properties.Settings.Default.Width = (int)this.ActualWidth;
            Properties.Settings.Default.Height = (int)this.ActualHeight;
            Properties.Settings.Default.Top = (int)this.Top;
            Properties.Settings.Default.Left = (int)this.Left;
            Properties.Settings.Default.Save();
            inputData.CloseCommand.Execute(null);
		}
        
        #region UI fixes
        private void TextBoxFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source is TextBox)
            {
                ((TextBox)e.Source).SelectAll();
            }
        }

        //private void btnSearchFastBookmarks_Click(object sender, RoutedEventArgs e)
        //{
        //    cbSearchFastBookmark.IsDropDownOpen = true;
        //    cbSearchFastBookmark.Focus();
        //}

        //private void btnReplaceFastBookmarks_Click(object sender, RoutedEventArgs e)
        //{
        //    cbReplaceFastBookmark.IsDropDownOpen = true;
        //    cbReplaceFastBookmark.Focus();
        //    cbReplaceFastBookmark.SelectAll();
        //}

        private void Window_Activated(object sender, EventArgs e)
        {
            inputData.ActivatePreviewWindow();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            inputData.ChangePreviewWindowState(this.WindowState);
        }

        //private void tbPreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Up || e.Key == Key.Down)
        //    {
        //        if (!inputData.Multiline)
        //        {
        //            if (sender != null && sender == tbSearchFor)
        //            {
        //                if (e.Key == Key.Down)
        //                    cbSearchFastBookmark.SelectedIndex++;
        //                else
        //                {
        //                    if (cbSearchFastBookmark.SelectedIndex > 0)
        //                        cbSearchFastBookmark.SelectedIndex--;
        //                }
        //            }
        //            else if (sender != null && sender == tbReplaceWith)
        //            {
        //                if (e.Key == Key.Down)
        //                    cbReplaceFastBookmark.SelectedIndex++;
        //                else
        //                {
        //                    if (cbReplaceFastBookmark.SelectedIndex > 0)
        //                        cbReplaceFastBookmark.SelectedIndex--;
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion
        
		private void cbMultiline_Unchecked(object sender, RoutedEventArgs e)
        {
            //gridMain.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
            //gridMain.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
        }

        private void FilesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)e.Source;
            var items = new List<FormattedGrepResult>();
            foreach (FormattedGrepResult item in listView.SelectedItems)
            {
                items.Add(item);
            }
            inputData.SetCodeSnippets(items);
        }

        private void btnOtherActions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            advanceContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            advanceContextMenu.PlacementTarget = (UIElement)sender;
            advanceContextMenu.IsOpen = true;
        }

        private void btnOtherActions_Click(object sender, RoutedEventArgs e)
        {
            advanceContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            advanceContextMenu.PlacementTarget = (UIElement)sender;
            advanceContextMenu.IsOpen = true;
        }

        private void SHFileList_Click(object sender, EventArgs e)
        {
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            
        
        }

        private void SendSelectedToGrep()
        {
            var p = SHFileList.SelectedItems;
            string s = "";
            CustomFileListItem c;
            //            Console.WriteLine("{0}", p.ToString());



            foreach (CustomFileListItem x in p)
            {


                s = s + x.Path + ";";

                Console.WriteLine("{0}", x.Path);


            }

            tbFolderName.tbFolderName.Text = s;

            changeTab(0);


        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            
 


        }

        private void btnFileNameSearch_Click(object sender, RoutedEventArgs e)
        {
       

        }

        private void SHFileList_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        //bottom left button for testing code
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            SendSelectedToGrep();
            //SHFileList.Add("c:\\temp\\test.xml");
            //SHFileList.Add("c:\\temp\\test2.xml");
            
          //  tbFolderName.

         //   SHFileList.CheckBoxes = true;

            //FolderSelectDropdown
            //.IsSelectedProperty = true;
            

        
        }

        public void changeTab(int i)
        {
            MainTab.SelectedIndex = i;//this works and flips tab

            if(i==1)
            {
                txtSearch.Focus();
            }


        }

        private void tbFolderName_KeyDown(object sender, KeyEventArgs e)
        {
         //   Console.WriteLine("kydown");
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        //    Console.WriteLine("{0}", e.Key);

        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {

        }

        private void TabControl_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void MainTab_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Initialized(object sender, EventArgs e)
        {

            SHFileList.StartMessagePump = true;
            SHFileList.ShellControlConnector = new Jam.Shell.ShellControlConnector();
         
        }

        private void EvSearchClick(object sender, RoutedEventArgs e)
        {


            SHFileList.SearchEverything(txtSearch.Text);



            return;

            //below is old legacy
            int i;
            const int bufsize = 260;
            StringBuilder buf = new StringBuilder(bufsize);

            SHFileList.Clear();

            // set the search
            EverythingSearch.Everything_SetSearchW(txtSearch.Text);

            // use our own custom scrollbar... 			
            // Everything_SetMax(listBox1.ClientRectangle.Height / listBox1.ItemHeight);
            // Everything_SetOffset(VerticalScrollBarPosition...);

            // execute the query
            EverythingSearch.Everything_QueryW(true);

            // sort by path
            // Everything_SortResultsByPath();

            // clear the old list of results			
            //listBox1.Items.Clear();

            // set the window title
      //      Text = textBox1.Text + " - " + EverythingSearch.Everything_GetNumResults() + " Results";


            // loop through the results, adding each result to the listbox.
            for (i = 0; i < EverythingSearch.Everything_GetNumResults(); i++)
            {
                // get the result's full path and file name.
                EverythingSearch.Everything_GetResultFullPathNameW(i, buf, bufsize);
                SHFileList.Add(buf.ToString());

                // add it to the list box				
                //    listBox1.Items.Insert(i, buf);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EvSearchClick(sender, null);

            }
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Focus();
        }

        
       
        public void turnOnChecks()
        {
            Console.WriteLine("checks on");
            SHFileList.CheckBoxes = !SHFileList.CheckBoxes;


        }

        public void copyAll()
        {
            Console.WriteLine("copy all");
            SendSelectedToGrep();

        }
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FileBrowser;
using ShellDll;
using System.Runtime.InteropServices;
using System.IO;

namespace TestApp
{
    public partial class FileBrowserTest : Form
    {
        public FileBrowserTest()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void fileBrowser_ContextMenuMouseHover(object sender, ContextMenuMouseHoverEventArgs e)
        {
            helpInfoLabel.Text = e.ContextMenuItemInfo;
        }

        private void fileBrowser_SelectedFolderChanged(object sender, SelectedFolderChangedEventArgs e)
        {
            Icon icon = ShellImageList.GetIcon(e.Node.ImageIndex, true);

            if (icon != null)
            {
                currentDirInfo.Image = icon.ToBitmap();
                this.Icon = icon;
            }
            else
            {
                currentDirInfo.Image = null;
                this.Icon = null;
            }

            currentDirInfo.Text = e.Node.Text;
            this.Text = e.Node.Text;
        }

        private void showBrowserButton_Click(object sender, EventArgs e)
        {
            dualPane.SplitterDistance = Math.Max(0, Math.Min(dualPane.SplitterDistance, this.Width - dualPane.SplitterWidth - 50));
            dualPane.Panel2Collapsed = false;
            showBrowserButton.Visible = false;
        }

        private void hideBrowserButton_Click(object sender, EventArgs e)
        {
            dualPane.Panel2Collapsed = true;
            showBrowserButton.Visible = true;
        }

        #region Splitter Focus Fix

        // These methods will fix a problem with the focus of the SplitContainer. 

        private void splitter_MouseDown(object sender, MouseEventArgs e)
        {
            /* This disables the normal move behavior */
            ((SplitContainer)sender).IsSplitterFixed = true;
        }

        private void splitter_MouseMove(object sender, MouseEventArgs e)
        {
            /* Check to make sure the splitter won't be updated by the 
               normal move behavior also */
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                /* Make sure that the button used to move the splitter 
                   is the left mouse button */
                if (e.Button.Equals(MouseButtons.Left))
                {
                    /* Checks to see if the splitter is aligned Vertically */
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        /* Only move the splitter if the mouse is within 
                           the appropriate bounds */
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            /* Move the splitter */
                            ((SplitContainer)sender).SplitterDistance = e.X;
                        }
                    }
                    /* If it isn't aligned vertically then it must be 
                       horizontal */
                    else
                    {
                        /* Only move the splitter if the mouse is within 
                           the appropriate bounds */
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            /* Move the splitter */
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                        }
                    }

                    ((SplitContainer)sender).Refresh();
                }
                /* If a button other than left is pressed or no button 
                   at all */
                else
                {
                    /* This allows the splitter to be moved normally again */
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }

        private void splitter_MouseUp(object sender, MouseEventArgs e)
        {
            /* This allows the splitter to be moved normally again */
            ((SplitContainer)sender).IsSplitterFixed = false;
        }

        #endregion        
    }
}
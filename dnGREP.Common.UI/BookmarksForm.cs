using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using dnGREP.Common;

namespace dnGREP.Common.UI
{
	public partial class BookmarksForm : Form, INotifyPropertyChanged
	{
		DataTable copyOfBookmarks = new DataTable();
		public event PropertyChangedEventHandler PropertyChanged;
		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private string filePattern = "";

		public string FilePattern
		{
			get { return filePattern; }
			set { filePattern = value; OnPropertyChanged("FilePattern"); }
		}
		private string searchFor = "";

		public string SearchFor
		{
			get { return searchFor; }
			set { searchFor = value; OnPropertyChanged("SearchFor"); }
		}
		private string replaceWith = "";

		public string ReplaceWith
		{
			get { return replaceWith; }
			set { replaceWith = value; OnPropertyChanged("ReplaceWith"); }
		}

		private void changeState()
		{
			if (gridBookmarks.SelectedRows.Count == 1)
			{
				btnEdit.Enabled = true;
				btnDelete.Enabled = true;
			}
			else
			{
				btnEdit.Enabled = false;
				btnDelete.Enabled = false;
			}
		}

		private void refreshGrid()
		{
			copyOfBookmarks = BookmarkLibrary.Instance.GetDataTable();
			gridBookmarks.DataSource = copyOfBookmarks;
			gridBookmarks.Refresh();
		}

		public BookmarksForm()
		{
			InitializeComponent();			
		}

		private void BookmarksForm_Load(object sender, EventArgs e)
		{
			refreshGrid();
			changeState();
		}

		private void BookmarksForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			BookmarkLibrary.Save();
		}

		private void textSearch_TextChanged(object sender, EventArgs e)
		{
			typeTimer.Stop();
			typeTimer.Start();
		}

		private void gridBookmarks_SelectionChanged(object sender, EventArgs e)
		{
			changeState();
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (gridBookmarks.SelectedRows.Count != 1)
				return;
			DataRowView bookmarkRow = (DataRowView)gridBookmarks.SelectedRows[0].DataBoundItem;
			Bookmark oldBookmark = new Bookmark(bookmarkRow["SearchPattern"].ToString(), bookmarkRow["ReplacePattern"].ToString(),
				bookmarkRow["FileNames"].ToString(), bookmarkRow["Description"].ToString());
			Bookmark newBookmark = new Bookmark(bookmarkRow["SearchPattern"].ToString(), bookmarkRow["ReplacePattern"].ToString(),
				bookmarkRow["FileNames"].ToString(), bookmarkRow["Description"].ToString());
			BookmarkDetails editForm = new BookmarkDetails(CreateOrEdit.Edit);
			editForm.Bookmark = newBookmark;
			if (editForm.ShowDialog() == DialogResult.OK)
			{
				BookmarkLibrary.Instance.Bookmarks.Remove(oldBookmark);
				BookmarkLibrary.Instance.Bookmarks.Add(newBookmark);
				BookmarkLibrary.Save();
				refreshGrid();
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (gridBookmarks.SelectedRows.Count != 1)
				return;
			DataRowView bookmarkRow = (DataRowView)gridBookmarks.SelectedRows[0].DataBoundItem;
			Bookmark oldBookmark = new Bookmark(bookmarkRow["SearchPattern"].ToString(), bookmarkRow["ReplacePattern"].ToString(),
				bookmarkRow["FileNames"].ToString(), bookmarkRow["Description"].ToString());
			BookmarkLibrary.Instance.Bookmarks.Remove(oldBookmark);
			refreshGrid();
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			BookmarkDetails editForm = new BookmarkDetails(CreateOrEdit.Create);
			if (editForm.ShowDialog() == DialogResult.OK)
			{
				Bookmark bookmark = editForm.Bookmark;
				if (!BookmarkLibrary.Instance.Bookmarks.Contains(bookmark))
				{
					BookmarkLibrary.Instance.Bookmarks.Add(bookmark);
					BookmarkLibrary.Save();
					refreshGrid();
				}
			}
		}

		private void gridBookmarks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			btnUse_Click(this, null);
		}

		private void formKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				Close();
		}

		private void btnUse_Click(object sender, EventArgs e)
		{
			if (gridBookmarks.SelectedRows.Count != 1)
				return;
			DataRowView bookmarkRow = (DataRowView)gridBookmarks.SelectedRows[0].DataBoundItem;
			FilePattern = bookmarkRow["FileNames"].ToString();
			SearchFor = bookmarkRow["SearchPattern"].ToString();
			ReplaceWith = bookmarkRow["ReplacePattern"].ToString();
		}

		private void doSearch(object sender, EventArgs e)
		{
			typeTimer.Stop();
			
			if (string.IsNullOrEmpty(textSearch.Text.Trim()))
			{
				refreshGrid();
				return;
			}

			copyOfBookmarks = BookmarkLibrary.Instance.GetDataTable();
			
			for (int i = copyOfBookmarks.Rows.Count - 1; i >= 0; i--)
			{
				DataRow row = copyOfBookmarks.Rows[i];
				bool found = row["FileNames"].ToString().ToLower().Contains(textSearch.Text.ToLower()) ||
							row["SearchPattern"].ToString().ToLower().Contains(textSearch.Text.ToLower()) ||
							row["ReplacePattern"].ToString().ToLower().Contains(textSearch.Text.ToLower()) ||
							row["Description"].ToString().ToLower().Contains(textSearch.Text.ToLower());
				if (!found)
					copyOfBookmarks.Rows.RemoveAt(i);
			}
			gridBookmarks.DataSource = copyOfBookmarks;
			gridBookmarks.Refresh();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
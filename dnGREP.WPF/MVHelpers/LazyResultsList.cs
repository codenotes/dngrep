﻿using dnGREP.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dnGREP.WPF.MVHelpers
{
    public class LazyResultsList : ObservableCollection<FormattedGrepLine>, INotifyPropertyChanged
    {
        private GrepSearchResult result;
        private FormattedGrepResult formattedResult;
        private bool isLoaded;
        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
        }

        public LazyResultsList(GrepSearchResult result, FormattedGrepResult formattedResult)
        {
            this.result = result;
            this.formattedResult = formattedResult;
            if ((result.Matches != null && result.Matches.Count > 0) || !result.IsSuccess)
            {
                GrepSearchResult.GrepLine emptyLine = new GrepSearchResult.GrepLine(-1, "", true, null);
                var dummyLine = new FormattedGrepLine(emptyLine, formattedResult, 30);
                this.Add(dummyLine);
                isLoaded = false;
            }
        }

        private int lineNumberColumnWidth = 30;
        public int LineNumberColumnWidth
        {
            get { return lineNumberColumnWidth; }
            set { lineNumberColumnWidth = value; OnPropertyChanged("LineNumberColumnWidth"); }
        }

        public void Load(bool isAsync)
        {
            if (isLoaded || isLoading)
                return;

            isLoading = true;
            if (!isAsync)
            {
                int currentLine = -1;
                List<GrepSearchResult.GrepLine> linesWithContext = new List<GrepSearchResult.GrepLine>();
                if (GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext))
                    linesWithContext = result.GetLinesWithContext(GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesBefore),
                    GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesAfter));
                else
                    linesWithContext = result.GetLinesWithContext(0, 0);

                if (this.Count == 1 && this[0].GrepLine.LineNumber == -1)
                {
                    this.Clear();
                }

                for (int i = 0; i < linesWithContext.Count; i++)
                {
                    GrepSearchResult.GrepLine line = linesWithContext[i];

                    // Adding separator
                    if (this.Count > 0 && GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext) &&
                        (currentLine != line.LineNumber && currentLine + 1 != line.LineNumber))
                    {
                        GrepSearchResult.GrepLine emptyLine = new GrepSearchResult.GrepLine(-1, "", true, null);
                        this.Add(new FormattedGrepLine(emptyLine, formattedResult, 30));
                    }

                    currentLine = line.LineNumber;
                    if (currentLine <= 999 && LineNumberColumnWidth < 30)
                        LineNumberColumnWidth = 30;
                    else if (currentLine > 999 && LineNumberColumnWidth < 35)
                        LineNumberColumnWidth = 35;
                    else if (currentLine > 9999 && LineNumberColumnWidth < 47)
                        LineNumberColumnWidth = 47;
                    else if (currentLine > 99999 && LineNumberColumnWidth < 50)
                        LineNumberColumnWidth = 50;

                    this.Add(new FormattedGrepLine(line, formattedResult, LineNumberColumnWidth));                  
                }
                isLoaded = true;
                isLoading = false;
            }
            else
            {
                int currentLine = -1;
                var asyncTask = Task.Factory.StartNew<List<GrepSearchResult.GrepLine>>(() =>
                {
                    List<GrepSearchResult.GrepLine> linesWithContext = new List<GrepSearchResult.GrepLine>();
                    if (GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext))
                        linesWithContext = result.GetLinesWithContext(GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesBefore),
                        GrepSettings.Instance.Get<int>(GrepSettings.Key.ContextLinesAfter));
                    else
                        linesWithContext = result.GetLinesWithContext(0, 0);
                    //System.Threading.Thread.Sleep(5000);
                    return linesWithContext;
                }).ContinueWith(task =>
                {
                    if (this.Count == 1 && this[0].GrepLine.LineNumber == -1)
                    {
                        if (Application.Current != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                                this.Clear()
                            ));
                    }

                    List<GrepSearchResult.GrepLine> linesWithContext = task.Result;
                    List<FormattedGrepLine> tempList = new List<FormattedGrepLine>();
                    for (int i = 0; i < linesWithContext.Count; i++)
                    {
                        GrepSearchResult.GrepLine line = linesWithContext[i];

                        // Adding separator
                        if (this.Count > 0 && GrepSettings.Instance.Get<bool>(GrepSettings.Key.ShowLinesInContext) &&
                            (currentLine != line.LineNumber && currentLine + 1 != line.LineNumber))
                        {
                            GrepSearchResult.GrepLine emptyLine = new GrepSearchResult.GrepLine(-1, "", true, null);
                            if (Application.Current != null)
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                    this.Add(new FormattedGrepLine(emptyLine, formattedResult, 30))
                                ));
                        }

                        currentLine = line.LineNumber;
                        if (currentLine <= 999 && LineNumberColumnWidth < 30)
                            LineNumberColumnWidth = 30;
                        else if (currentLine > 999 && LineNumberColumnWidth < 35)
                            LineNumberColumnWidth = 35;
                        else if (currentLine > 9999 && LineNumberColumnWidth < 47)
                            LineNumberColumnWidth = 47;
                        else if (currentLine > 99999 && LineNumberColumnWidth < 50)
                            LineNumberColumnWidth = 50;
                        tempList.Add(new FormattedGrepLine(line, formattedResult, LineNumberColumnWidth));
                    }

                    if (Application.Current != null)
                        Application.Current.Dispatcher.Invoke(new Action(() => 
                        {
                            foreach (var l in tempList) this.Add(l);
                        }
                    ));
                    isLoaded = true;
                    isLoading = false;
                    LoadFinished(this, null);
                });
            }
        }

        public event EventHandler<PropertyChangedEventArgs> LineNumberColumnWidthChanged;
        public event EventHandler LoadFinished;

        protected void OnPropertyChanged(string name)
        {
            LineNumberColumnWidthChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}

﻿using dnGREP.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Tests
{
	public class UtilsTest : TestBase, IDisposable
	{
        static string sourceFolder;
        static string destinationFolder;

        public UtilsTest()
		{
			sourceFolder = GetDllPath() + "\\Files";
            destinationFolder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(destinationFolder);
		}

        public void Dispose()
        {
            if (Directory.Exists(destinationFolder))
                Utils.DeleteFolder(destinationFolder);
        }

		[InlineData("Hello world", "Hello world", 2, 1)]
		[InlineData("Hi", "Hi", 2, 1)]
		[InlineData("Hi\r\n\r\nWorld", "", 4, 2)]
		[InlineData("Hi\r\n\r\nWorld", "World", 6, 3)]
		[InlineData(null, null, 6, -1)]
        [Obsolete]
		public void TestGetLine(string body, string line, int index, int lineNumber)
		{
			int returnedLineNumber = -1;
			string returnedLine = Utils.GetLine(body, index, out returnedLineNumber);
			Assert.Equal(returnedLine, line);
			Assert.Equal(returnedLineNumber, lineNumber);
		}

        [Fact]
        public void GetIntArray()
        {
            int[] r = Utils.GetIntArray(3, 3);
            Assert.Equal(r, new int[] { 3, 4, 5 });

            r = Utils.GetIntArray(0, 3);
            Assert.Equal(r, new int[] { 0, 1, 2 });

            r = Utils.GetIntArray(0, 0);
            Assert.Equal(r, new int[] { });
        }

		[Fact]
		public void TestGetContextLines()
		{
			string test = "Hi\r\nmy\r\nWorld\r\nMy name is Denis\r\nfor\r\nloop";
            
            List<GrepSearchResult.GrepMatch> bodyMatches = new List<GrepSearchResult.GrepMatch>();
            List<GrepSearchResult.GrepLine> lines = new List<GrepSearchResult.GrepLine>();
            using (StringReader reader = new StringReader(test))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 9, 2));
                lines = Utils.GetLinesEx(reader, bodyMatches, 2, 2);
            }
            Assert.Equal(lines.Count, 5);
			Assert.Equal(lines[0].LineNumber, 1);
			Assert.Equal(lines[0].LineText, "Hi");
			Assert.Equal(lines[0].IsContext, true);
            Assert.Equal(lines[1].IsContext, true);
            Assert.Equal(lines[2].IsContext, false);
			Assert.Equal(lines[3].LineNumber, 4);
			Assert.Equal(lines[3].LineText, "My name is Denis");
			Assert.Equal(lines[3].IsContext, true);
			Assert.Equal(lines[4].LineNumber, 5);
			Assert.Equal(lines[4].LineText, "for");
			Assert.Equal(lines[4].IsContext, true);


            bodyMatches = new List<GrepSearchResult.GrepMatch>();
            using (StringReader reader = new StringReader(test))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 9, 2));
                lines = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }
			Assert.Equal(lines.Count, 1);

            bodyMatches = new List<GrepSearchResult.GrepMatch>();
            using (StringReader reader = new StringReader(test))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 4, 1));
                lines = Utils.GetLinesEx(reader, bodyMatches, 10, 0);
            }
            Assert.Equal(lines.Count, 2);
			Assert.Equal(lines[0].LineNumber, 1);
			Assert.Equal(lines[0].LineText, "Hi");
			Assert.Equal(lines[0].IsContext, true);
            Assert.Equal(lines[1].LineText, "my");
            Assert.Equal(lines[1].IsContext, false);

            bodyMatches = new List<GrepSearchResult.GrepMatch>();
            using (StringReader reader = new StringReader(test))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 34, 1));
                lines = Utils.GetLinesEx(reader, bodyMatches, 1, 10);
            }

            Assert.Equal(lines.Count, 3);
            Assert.Equal(lines[0].LineNumber, 4);
			Assert.Equal(lines[1].LineNumber, 5);
			Assert.Equal(lines[1].LineText, "for");
            Assert.Equal(lines[1].IsContext, false);
			Assert.Equal(lines[2].LineNumber, 6);
			Assert.Equal(lines[2].LineText, "loop");
			Assert.Equal(lines[2].IsContext, true);			
		}

        [Fact]
        public void TestDefaultSettings()
        {
            var type = GrepSettings.Instance.Get<SearchType>(GrepSettings.Key.TypeOfSearch);
            Assert.Equal<SearchType>(type, SearchType.Regex);
        }

		[Theory]
		[InlineData("hello\rworld", "hello\r\nworld")]
		[InlineData("hello\nworld", "hello\r\nworld")]
		[InlineData("hello\rworld\r", "hello\r\nworld\r")]
		public void TestCleanLineBreaks(string input, string output)
		{
			string result = Utils.CleanLineBreaks(input);
			Assert.Equal(result, output);
		}

        [Theory]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs", "\\Files\\TestCase1")]
		[InlineData("\\Files\\TestCase1", "\\Files\\TestCase1")]
		[InlineData("\\Files\\TestCas\\", null)]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs;\\Files\\TestCase1\\test-file-plain.txt", "\\Files\\TestCase1")]
		public void TestGetBaseFolder(string relativePath, string result)
		{
			StringBuilder sb = new StringBuilder();
			string pathToDll = GetDllPath();
			string[] rPaths = relativePath.Split(';');
			foreach (string rPath in rPaths)
				sb.Append(pathToDll + rPath + ";");

			if (result == null)
				Assert.Equal(Utils.GetBaseFolder(sb.ToString()), null);
			else
				Assert.Equal(Utils.GetBaseFolder(sb.ToString()), pathToDll + result);
		}

		[Theory]
		[InlineData("\\Files\\TestCase7\\Test;Folder", "\\Files\\TestCase7\\Test;Folder")]
		[InlineData("\\Files\\TestCase7\\Test,Folder", "\\Files\\TestCase7\\Test,Folder")]
		public void TestGetBaseFolderWithColons(string relativePath, string result)
		{
			string pathToDll = GetDllPath();
			relativePath = pathToDll + relativePath;

			if (result == null)
				Assert.Equal(Utils.GetBaseFolder(relativePath), null);
			else
				Assert.Equal(Utils.GetBaseFolder(relativePath), pathToDll + result);
		}

        [Theory]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs", true)]
		[InlineData("\\Files\\TestCase1\\test-file-code2.cs", false)]
		[InlineData("\\Files\\TestCase1\\", true)]
		[InlineData("\\Files\\TestCase1", true)]
		[InlineData("\\Files\\TestCas\\", false)]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs;\\Files\\TestCase1\\test-file-plain.txt", true)]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs;\\Files\\TestCase1\\test-file-plain.txt;\\Files\\TestCase1", true)]
		[InlineData("\\Files\\TestCase1\\test11-file-code.cs;\\Files\\TestCase1\\test-file-plain.txt;\\Files\\TestCase1", false)]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs;\\Files\\TestCase1\\test-file-plain.txt;\\Files1\\TestCase1", false)]
		public void TestIsPathValied(string relativePath, bool result)
		{
			StringBuilder sb = new StringBuilder();
			string pathToDll = GetDllPath();
			string[] rPaths = relativePath.Split(';');
			foreach (string rPath in rPaths)
				sb.Append(pathToDll + rPath + ";");

			Assert.Equal(result, Utils.IsPathValid(sb.ToString()));

			foreach (string rPath in rPaths)
				sb.Append(pathToDll + rPath + ",");

			Assert.Equal(result, Utils.IsPathValid(sb.ToString()));
		}

		[Theory]
		[InlineData("\\Files\\TestCase7\\Test;Folder\\issue-10.txt", true)]
		[InlineData("\\Files\\TestCase7\\Test,Folder\\issue-10.txt", true)]
		[InlineData("\\Files\\TestCase1\\test-file-code.cs", true)]
		public void TestIsPathValiedWithColon(string relativePath, bool result)
		{
			string pathToDll = GetDllPath();
			relativePath = pathToDll + relativePath;

			Assert.Equal(result, Utils.IsPathValid(relativePath));
		}

		[Fact]
		public void TestIsPathValidWithoutCollon()
		{
			StringBuilder sb = new StringBuilder();
			string pathToDll = GetDllPath();
			sb.Append(pathToDll + "\\Files\\TestCase1");

			Assert.Equal(Utils.IsPathValid(sb.ToString()), true);
		}		      

		[Fact]
		public void TestCleanResults()
		{
			List<GrepSearchResult.GrepLine> results =  new List<GrepSearchResult.GrepLine>();
            results.Add(new GrepSearchResult.GrepLine(1, "test", true, null));
            results.Add(new GrepSearchResult.GrepLine(3, "test3", false, null));
            results.Add(new GrepSearchResult.GrepLine(2, "test2", false, null));
            results.Add(new GrepSearchResult.GrepLine(1, "test1", false, null));
			Utils.CleanResults(ref results);

			Assert.Equal(results.Count, 3);
			Assert.Equal(results[0].IsContext, false);
			Assert.Equal(results[0].LineNumber, 1);
			Assert.Equal(results[2].IsContext, false);
			Assert.Equal(results[2].LineNumber, 3);

			results = null;
			Utils.CleanResults(ref results);
			results = new List<GrepSearchResult.GrepLine>();
			Utils.CleanResults(ref results);
		}


        [Theory]
		[InlineData("0.9.1", "0.9.2", true)]
		[InlineData("0.9.1", "0.9.2.5556", true)]
		[InlineData("0.9.1.5554", "0.9.1.5556", true)]
		[InlineData("0.9.0.5557", "0.9.1.5550", true)]
		[InlineData("0.9.1", "0.9.0.5556", false)]
		[InlineData("0.9.5.5000", "0.9.0.5556", false)]
		[InlineData(null, "0.9.0.5556", false)]
		[InlineData("0.9.5.5000", "", false)]
		[InlineData("0.9.5.5000", null, false)]
		[InlineData("xyz", "abc", false)]
		public void CompareVersions(string v1, string v2, bool result)
		{
			Assert.True(PublishedVersionExtractor.IsUpdateNeeded(v1, v2) == result);
		}

		[Obsolete]
        public void GetLines_Returns_Correct_Line()
		{
			string text = "Hello world" + Environment.NewLine + "My tests are good" + Environment.NewLine + "How about yours?";
			List<int> lineNumbers = new List<int>();
            List<GrepSearchResult.GrepMatch> matches = new List<GrepSearchResult.GrepMatch>();
			List<string> lines = Utils.GetLines(text, 3, 2, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 1);
			Assert.Equal(lines[0], "Hello world");
			Assert.Equal(lineNumbers.Count, 1);
			Assert.Equal(lineNumbers[0], 1);

            lines = Utils.GetLines(text, 14, 2, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 1);
			Assert.Equal(lines[0], "My tests are good");
			Assert.Equal(lineNumbers.Count, 1);
			Assert.Equal(lineNumbers[0], 2);

            lines = Utils.GetLines(text, 3, 11, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 2);
			Assert.Equal(lines[0], "Hello world");
			Assert.Equal(lines[1], "My tests are good");
			Assert.Equal(lineNumbers.Count, 2);
			Assert.Equal(lineNumbers[0], 1);
			Assert.Equal(lineNumbers[1], 2);

            lines = Utils.GetLines(text, 3, 30, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 3);
			Assert.Equal(lines[0], "Hello world");
			Assert.Equal(lines[1], "My tests are good");
			Assert.Equal(lines[2], "How about yours?");
			Assert.Equal(lineNumbers.Count, 3);
			Assert.Equal(lineNumbers[0], 1);
			Assert.Equal(lineNumbers[1], 2);
			Assert.Equal(lineNumbers[2], 3);

            lines = Utils.GetLines("test", 1, 2, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 1);
			Assert.Equal(lines[0], "test");
			Assert.Equal(lineNumbers.Count, 1);
			Assert.Equal(lineNumbers[0], 1);

            lines = Utils.GetLines("test", 0, 2, out matches, out lineNumbers);
			Assert.Equal(lines.Count, 1);
			Assert.Equal(lines[0], "test");
			Assert.Equal(lineNumbers.Count, 1);
			Assert.Equal(lineNumbers[0], 1);

            lines = Utils.GetLines("test", 10, 2, out matches, out lineNumbers);
			Assert.Null(lines);
			Assert.Null(lineNumbers);

            lines = Utils.GetLines("test", 2, 10, out matches, out lineNumbers);
			Assert.Null(lines);
			Assert.Null(lineNumbers);
		}

        [Fact]
        public void GetLinesEx_Returns_Correct_Line()
        {
            string text = "Hello world" + Environment.NewLine + "My tests are good" + Environment.NewLine + "How about yours?";
            List<int> lineNumbers = new List<int>();
            List<GrepSearchResult.GrepMatch> bodyMatches = new List<GrepSearchResult.GrepMatch>();
            List<GrepSearchResult.GrepLine> results = new List<GrepSearchResult.GrepLine>(); 
            using (StringReader reader = new StringReader(text))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 3, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count(l=>l.IsContext == false), 1);
            Assert.Equal(results[0].LineText, "Hello world");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);

            using (StringReader reader = new StringReader(text))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 14, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }


            Assert.Equal(results.Count, 1);
            Assert.Equal(results[0].LineText, "My tests are good");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 2);

            using (StringReader reader = new StringReader(text))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 3, 11));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count, 2);
            Assert.Equal(results[0].LineText, "Hello world");
            Assert.Equal(results[1].LineText, "My tests are good");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[1].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);
            Assert.Equal(results[1].LineNumber, 2);

            using (StringReader reader = new StringReader(text))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 3, 30));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count, 3);
            Assert.Equal(results[0].LineText, "Hello world");
            Assert.Equal(results[1].LineText, "My tests are good");
            Assert.Equal(results[2].LineText, "How about yours?");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[1].Matches.Count, 1);
            Assert.Equal(results[2].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);
            Assert.Equal(results[1].LineNumber, 2);
            Assert.Equal(results[2].LineNumber, 3);

            using (StringReader reader = new StringReader("test"))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 2, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count, 1);
            Assert.Equal(results[0].LineText, "test");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);

            using (StringReader reader = new StringReader("test"))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 0, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count, 1);
            Assert.Equal(results[0].LineText, "test");
            Assert.Equal(results[0].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);

            using (StringReader reader = new StringReader("test"))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 10, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Empty(results);

            using (StringReader reader = new StringReader("test"))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 2, 10));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Empty(results);

            using (StringReader reader = new StringReader(text))
            {
                bodyMatches.Clear();
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 3, 2));
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 6, 2));
                bodyMatches.Add(new GrepSearchResult.GrepMatch(0, 14, 2));
                results = Utils.GetLinesEx(reader, bodyMatches, 0, 0);
            }

            Assert.Equal(results.Count, 2);
            Assert.Equal(results[0].LineText, "Hello world");
            Assert.Equal(results[1].LineText, "My tests are good");
            Assert.Equal(results[0].Matches.Count, 2);
            Assert.Equal(results[1].Matches.Count, 1);
            Assert.Equal(results[0].LineNumber, 1);
            Assert.Equal(results[1].LineNumber, 2);            
        }

        [Fact]
        public void TestMergeResultsHappyPath()
        {
            List<GrepSearchResult.GrepLine> results = new List<GrepSearchResult.GrepLine>();
            List<GrepSearchResult.GrepLine> context = new List<GrepSearchResult.GrepLine>();
            results.Add(new GrepSearchResult.GrepLine(3, "text3", false, null));
            results.Add(new GrepSearchResult.GrepLine(5, "text5", false, null));
            results.Add(new GrepSearchResult.GrepLine(6, "text6", false, new List<GrepSearchResult.GrepMatch>()));
            context.Add(new GrepSearchResult.GrepLine(1, "text1", true, null));
            context.Add(new GrepSearchResult.GrepLine(2, "text2", true, null));
            context.Add(new GrepSearchResult.GrepLine(3, "text3", true, null));
            Utils.MergeResults(ref results, context);
            Assert.Equal(5, results.Count);
            Assert.Equal("text1", results[0].LineText);
            Assert.Equal("text2", results[1].LineText);
            Assert.Equal(true, results[1].IsContext);
            Assert.Equal("text3", results[2].LineText);
            Assert.Equal(false, results[2].IsContext);
        }

        [Fact]
        public void TestMergeResultsBorderCases()
        {
            List<GrepSearchResult.GrepLine> results = new List<GrepSearchResult.GrepLine>();
            Utils.MergeResults(ref results, null);
            Assert.Equal(0, results.Count);

            List<GrepSearchResult.GrepLine> context = new List<GrepSearchResult.GrepLine>();
            context.Add(new GrepSearchResult.GrepLine(1, "text1", true, null));
            context.Add(new GrepSearchResult.GrepLine(2, "text2", true, null));
            context.Add(new GrepSearchResult.GrepLine(3, "text3", true, null));

            Utils.MergeResults(ref results, context);
            Assert.Equal(3, results.Count);
            
            results.Add(new GrepSearchResult.GrepLine(3, "text3", false, null));
            results.Add(new GrepSearchResult.GrepLine(5, "text5", false, null));
            results.Add(new GrepSearchResult.GrepLine(6, "text6", false, new List<GrepSearchResult.GrepMatch>()));

            results = new List<GrepSearchResult.GrepLine>();
            results.Add(new GrepSearchResult.GrepLine(3, "text3", false, null));
            results.Add(new GrepSearchResult.GrepLine(5, "text5", false, null));
            results.Add(new GrepSearchResult.GrepLine(6, "text6", false, new List<GrepSearchResult.GrepMatch>()));
            Utils.MergeResults(ref results, null);
            Assert.Equal(3, results.Count);

            results = new List<GrepSearchResult.GrepLine>();
            results.Add(new GrepSearchResult.GrepLine(3, "text3", false, null));
            results.Add(new GrepSearchResult.GrepLine(5, "text5", false, null));
            context.Add(new GrepSearchResult.GrepLine(20, "text20", true, null));
            context.Add(new GrepSearchResult.GrepLine(30, "text30", true, null));

            Utils.MergeResults(ref results, context);
            Assert.Equal(6, results.Count);
            Assert.Equal("text30", results[5].LineText);
        }

        [Fact]
        public void TestTextReaderReadLine()
        {
            string text = "Hello world" + Environment.NewLine + "My tests are good\nHow about \ryours?\n";
            int lineNumber = 0;
            using (StringReader reader = new StringReader(text))
            {
                while (reader.Peek() > 0)
                {
                    lineNumber++;
                    var line = reader.ReadLine(true);
                    if (lineNumber == 1)
                        Assert.Equal("Hello world" + Environment.NewLine, line);
                    if (lineNumber == 2)
                        Assert.Equal("My tests are good\n", line);
                    if (lineNumber == 3)
                        Assert.Equal("How about \r", line);
                    if (lineNumber == 4)
                        Assert.Equal("yours?\n", line);
                }
            }
            Assert.Equal(lineNumber, 4);
            text = "Hello world";
            lineNumber = 0;
            using (StringReader reader = new StringReader(text))
            {
                while (reader.Peek() > 0)
                {
                    lineNumber++;
                    var line = reader.ReadLine(true);
                    Assert.Equal("Hello world", line);
                }
            }
            Assert.Equal(lineNumber, 1);
        }

        [Theory]
		[InlineData(null,null,2)]
		[InlineData("", "", 2)]
		[InlineData(null, ".*\\.cs", 1)]
		[InlineData(".*\\.txt", null, 1)]
		public void TestCopyFiles(string includePattern, string excludePattern, int numberOfFiles)
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase1", destinationFolder, includePattern, excludePattern);
			Assert.Equal(Directory.GetFiles(destinationFolder).Length, numberOfFiles);
		}

        [Theory]
		[InlineData(null, null, 2)]
		public void TestCopyFilesToNonExistingFolder(string includePattern, string excludePattern, int numberOfFiles)
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase1", destinationFolder + "\\123", includePattern, excludePattern);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\123").Length, numberOfFiles);
		}

		[Fact]
		public void TestCopyFilesWithSubFolders()
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase3", destinationFolder + "\\TestCase3", ".*", null);
			Assert.Equal(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 4);
			Assert.True(Directory.Exists(destinationFolder + "\\TestCase3\\SubFolder"));
			Utils.DeleteFolder(destinationFolder + "\\TestCase3");
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase3\\SubFolder\\test-file-plain-hidden.txt", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase3\\test-file-code.cs", "", null));
			Utils.CopyFiles(source, sourceFolder + "\\TestCase3", destinationFolder + "\\TestCase3", true);
			Assert.Equal(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 2);
			Assert.True(Directory.Exists(destinationFolder + "\\TestCase3\\SubFolder"));			
		}

		[Fact]
		public void TestCopyResults()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", "", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1").Length, 2);
            source.Add(new GrepSearchResult(sourceFolder + "\\issue-10.txt", "", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, true);
			Assert.Equal(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 3);
			try
			{
				Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
				Assert.True(false, "Not supposed to get here");
			}
			catch
			{
				//OK
			}
			Assert.Equal(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 3);
			Utils.CopyFiles(source, sourceFolder, destinationFolder + "\\123", false);
			Assert.Equal(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 6);
		}

		[Fact]
		public void TestCanCopy()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\TestCase1\\test-file-plain2.txt", "", null));
			Assert.False(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1"));
			Assert.False(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1\\"));
			Assert.True(Utils.CanCopyFiles(source, sourceFolder));
			Assert.False(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1\\TestCase1"));
			Assert.False(Utils.CanCopyFiles(null, null));
			Assert.False(Utils.CanCopyFiles(source, null));
			Assert.False(Utils.CanCopyFiles(null, sourceFolder));
		}

		[Fact]
		public void WriteToCsvTest()
		{
            Utils.CopyFiles(sourceFolder + "\\TestCase3", destinationFolder + "\\TestCase3", null, null);
			File.WriteAllText(destinationFolder + "\\test.csv", "hello");
            var core = new GrepCore();
            var results = core.Search(Directory.GetFiles(destinationFolder + "\\TestCase3", "*.*"), SearchType.PlainText, "string", GrepSearchOption.None, -1);
            Assert.Equal(results.Count, 2);
            Assert.Equal(results[0].Matches.Count, 3);
            Assert.Equal(results[1].Matches.Count, 282);
            Utils.SaveResultsAsCSV(results, destinationFolder + "\\test.csv");
			string[] stringLines = File.ReadAllLines(destinationFolder + "\\test.csv");
			Assert.Equal(stringLines.Length, 177);
			Assert.Equal(stringLines[0].Split(',')[0].Trim(), "File Name");
			Assert.Equal(stringLines[1].Split(',')[1].Trim(), "1");
            Assert.Equal(stringLines[2].Split(',')[2].Trim(), "\"\tstring returnedLine = Utils.GetLine(body");
		}

		[Fact]
		public void DeleteFilesTest()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", "", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 2);
			List<GrepSearchResult> source2 = new List<GrepSearchResult>();
            source2.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-code.cs", "", null));
			Utils.DeleteFiles(source2);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 1);
            source2.Add(new GrepSearchResult(destinationFolder + "\\test-file-code.cs", "", null));
			Utils.DeleteFiles(source2);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 1);
            source2.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-plain.txt", "", null));
			Utils.DeleteFiles(source2);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 0);
		}

		[Fact]
		public void TestCopyFileInNonExistingFolder()
		{
			Utils.CopyFile(sourceFolder + "\\TestCase1\\test-file-code.cs", destinationFolder + "\\Test\\test-file-code2.cs", false);
			Assert.True(File.Exists(destinationFolder + "\\Test\\test-file-code2.cs"));
		}

		[Fact]
		public void DeleteFolderTest()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", "", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.Equal(Directory.GetFiles(destinationFolder + "\\TestCase1").Length, 2);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-code.cs", FileAttributes.ReadOnly);
			Utils.DeleteFolder(destinationFolder);
			Assert.False(Directory.Exists(destinationFolder));
		}

        [Theory]
		[InlineData("*.*", false, true, true, 0, 0, 5)]
		[InlineData("*.*", false, true, false, 0, 0, 4)]
		[InlineData("*.*", false, true, false, 0, 40, 3)]
		[InlineData("*.*", false, true, false, 1, 40, 1)]
		[InlineData(".*\\.txt", true, true, true, 0, 0, 3)]
		[InlineData(".*\\.txt", true, false, true, 0, 0, 2)]
		[InlineData(null, true, false, true, 0, 0, 0)]
		[InlineData("", true, true, true, 0, 0, 0)]
		public void GetFileListTest(string namePattern, bool isRegex, bool includeSubfolders, bool includeHidden, int sizeFrom, int sizeTo, int result)
		{
			DirectoryInfo di = new DirectoryInfo(sourceFolder + "\\TestCase2\\HiddenFolder");
            if (!di.Exists)
            {
                di.Create();
                File.WriteAllText(di.FullName + "\\test-file-plain-hidden.txt", "Hello world");
            }
			di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            Assert.Equal(result, Utils.GetFileList(sourceFolder + "\\TestCase2", namePattern, null, isRegex, includeSubfolders, includeHidden, true, sizeFrom, sizeTo).Length);
		}

		[Fact]
		public void GetFileListTestWithMultiplePaths()
		{
			string dllPath = GetDllPath();
			string path = sourceFolder + "\\TestCase2;" + sourceFolder + "\\TestCase2\\excel-file.xls";
			Assert.Equal(Utils.GetFileList(path, "*.*", "", false, false, false, true, 0, 0).Length, 4);

			path = sourceFolder + "\\TestCase2;" + sourceFolder + "\\TestCase3\\test-file-code.cs";
			Assert.Equal(Utils.GetFileList(path, "*.*", "", false, false, false, true, 0, 0).Length, 5);

			path = sourceFolder + "\\TestCase3\\test-file-code.cs;" + sourceFolder + "\\TestCase2";
			Assert.Equal(Utils.GetFileList(path, "*.*", "", false, false, false, true, 0, 0).Length, 5);

			path = sourceFolder + "\\TestCase2;" + sourceFolder + "\\TestCase3\\test-file-code.cs;" + sourceFolder + "\\TestCase3\\test-file-plain.txt";
			Assert.Equal(Utils.GetFileList(path, "*.*", null, false, false, false, true, 0, 0).Length, 6);

			path = sourceFolder + "\\TestCase3\\test-file-code.cs;" + sourceFolder + "\\TestCase3\\test-file-plain.txt";
			Assert.Equal(Utils.GetFileList(path, "*.*", null, false, false, false, true, 0, 0).Length, 2);

			path = sourceFolder + "\\TestCase3\\test-file-code.cs;" + sourceFolder + "\\TestCase3\\test-file-plain.txt;";
			Assert.Equal(Utils.GetFileList(path, "*.*", null, false, false, false, true, 0, 0).Length, 2);

			path = sourceFolder + "\\TestCase3\\test-file-code.cs," + sourceFolder + "\\TestCase3\\test-file-plain.txt,";
			Assert.Equal(Utils.GetFileList(path, "*.*", null, false, false, false, true, 0, 0).Length, 2);

			path = sourceFolder + "\\TestCase3\\test-file-code.cs," + sourceFolder + "\\TestCase3\\test-file-plain.txt";
			Assert.Equal(Utils.GetFileList(path, "*.*", null, false, false, false, true, 0, 0).Length, 2);
		}

		[Fact]
		public void GetFileListWithExcludes()
		{
			string dllPath = GetDllPath();
			string path = sourceFolder + "\\TestCase2";
			Assert.Equal(Utils.GetFileList(path, "*.*", "*.xls", false, false, false, true, 0, 0).Length, 3);
			Assert.Equal(Utils.GetFileList(path, "excel*.*", "*.xls", false, false, false, true, 0, 0).Length, 0);
			Assert.Equal(Utils.GetFileList(path, "excel*.*", "*.xs", false, false, false, true, 0, 0).Length, 1);
			Assert.Equal(Utils.GetFileList(path, "t[a-z]st-file-*.*", "*.cs", false, false, false, true, 0, 0).Length, 2);
			Assert.Equal(Utils.GetFileList(path, "t[ea]st-file-*.*", "*.cs", false, false, false, true, 0, 0).Length, 2);
		}

		[Fact]
		public void GetFileListFromNonExistingFolderReturnsEmptyString()
		{
			Assert.Equal(Utils.GetFileList(sourceFolder + "\\NonExisting", "*.*", null, false, true, true, true, 0, 0).Length, 0);
		}
        
        [Theory]
		[InlineData("", 1, 1)]
		[InlineData("5", 0, 5)]
		[InlineData(" 12", 1, 12)]
		[InlineData("", int.MinValue, int.MinValue)]
		[InlineData(null, int.MinValue, int.MinValue)]
		[InlineData(" 22 ", int.MinValue, 22)]
		public void ParseIntTest(string text, int defaultValue, int result)
		{
			if (defaultValue != int.MinValue)
				Assert.Equal(Utils.ParseInt(text, defaultValue), result);
			else
				Assert.Equal(Utils.ParseInt(text), result);
		}

		[Fact]
		public void GetReadOnlyFilesTest()
		{			
			List<GrepSearchResult> source = new List<GrepSearchResult>();
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", "", null));
            source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", "", null));

			List<GrepSearchResult> destination = new List<GrepSearchResult>();
            destination.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-code.cs", "", null));
            destination.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-plain.txt", "", null));

			Utils.CopyFiles(source, sourceFolder + "\\TestCase1", destinationFolder + "\\TestCase1", true);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-code.cs", FileAttributes.ReadOnly);
			Assert.Equal(Utils.GetReadOnlyFiles(destination).Count, 1);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-plain.txt", FileAttributes.ReadOnly);
			Assert.Equal(Utils.GetReadOnlyFiles(destination).Count, 2);

			Assert.Equal(Utils.GetReadOnlyFiles(null).Count, 0);
			Assert.Equal(Utils.GetReadOnlyFiles(new List<GrepSearchResult>()).Count, 0);
		}

		[Theory]
		[InlineData("\\TestCase6\\test.rar", true)]
		[InlineData("\\TestCase6\\test_file.txt", false)]
		[InlineData("\\TestCase5\\big-word-document.doc", true)]
		public void TestIsBinaryFile(string file, bool isBinary)
		{
			Assert.Equal<bool>(Utils.IsBinary(sourceFolder + file), isBinary);
		}

        public static IEnumerable<object[]> TestGetPaths_Source
        {
            get {
                yield return new object[] { sourceFolder + "\\TestCase5\\big-word-document.doc", 1 };
                yield return new object[] { sourceFolder + "\\TestCase7;" + sourceFolder + "\\TestCase7", 2 };
                yield return new object[] { sourceFolder + "\\TestCase5;" + sourceFolder + "\\TestCase7", 2 };
                yield return new object[] { sourceFolder + "\\TestCase7\\Test,Folder\\;" + sourceFolder + "\\TestCase7", 2 };
                yield return new object[] { sourceFolder + "\\TestCase7\\Test;Folder\\;" + sourceFolder + "\\TestCase7", 2 };
                yield return new object[] { sourceFolder + "\\TestCase7\\Test;Folder\\;" + sourceFolder + "\\TestCase7;" + sourceFolder + "\\TestCase7\\Test;Folder\\", 3 };
                yield return new object[] { null, 0 };
                yield return new object[] { "", 0 };
            }
        }

        [Theory]
        [PropertyData("TestGetPaths_Source")]
        public void TestGetPathsCount(string source, int? count)
        {
            string[] result = Utils.SplitPath(source);
            if (result == null)
                Assert.Null(count);
            else
                Assert.Equal(count, result.Length);
        }

        [Fact]
        public void TestGetPathsContent()
        {
            string[] result = Utils.SplitPath(sourceFolder + "\\TestCase7\\Test;Folder\\;" + sourceFolder + "\\TestXXXX;" + sourceFolder + "\\TestCase7\\Test;Fo;lder\\;" + sourceFolder + "\\TestCase7\\Test,Folder\\;");
            Assert.Equal(sourceFolder + "\\TestCase7\\Test;Folder\\", result[0]);
            Assert.Equal(sourceFolder + "\\TestXXXX", result[1]);
            Assert.Equal(sourceFolder + "\\TestCase7\\Test;Fo;lder\\", result[2]);
            Assert.Equal(sourceFolder + "\\TestCase7\\Test,Folder\\", result[3]);
        }

        [Fact]
        public void TestTrimEndOfString()
        {
            string text = "test\r\n";
            Assert.Equal("test", text.TrimEndOfLine());
            text = "test\r";
            Assert.Equal("test", text.TrimEndOfLine());
            text = "test\n";
            Assert.Equal("test", text.TrimEndOfLine());
            text = "test";
            Assert.Equal("test", text.TrimEndOfLine());
            text = "";
            Assert.Equal("", text.TrimEndOfLine());
        }

        [Theory]
        [InlineData("\\TestCase1", "*.cs", 1)]
        [InlineData("\\TestCase2", "*.txt", 2)]
        [InlineData("\\TestCase2", "*.txt;*.xls", 3)]
        [InlineData("\\TestCase2", null, 0)]
        [InlineData("\\TestCase11", "#!*python", 2)]
        [InlineData("\\TestCase11", "#!*python;#!*sh", 3)]
        public void TestAsteriskGetFilesWithoutExclude(string folder, string pattern, int expectedCount)
        {
            var result = Utils.GetFileListEx(sourceFolder + folder, pattern, null, false, false, true, true, 0, 0);
            int counter = 0;
            foreach (var f in result)
                counter++;
            Assert.Equal(expectedCount, counter);
        }

        [Theory]
        [InlineData("\\TestCase13", "*.*", "Obj\\*", 10)]
        [InlineData("\\TestCase13", "*.*", ".svn\\*", 11)]
        [InlineData("\\TestCase13", "*.*", ".svn\\*;obj\\*", 6)]
        [InlineData("\\TestCase13", "*.*", "*.*", 0)]
        [InlineData("\\TestCase13", "*.*", "", 15)]
        public void TestAsteriskGetFilesWithExclude(string folder, string pattern, String excludePattern, int expectedCount)
        {
            // This recurses to subfolders
            var result = Utils.GetFileListEx(sourceFolder + folder, pattern, excludePattern, false, true, true, true, 0, 0);
            int counter = 0;
            foreach (var f in result)
                counter++;
            Assert.Equal(expectedCount, counter);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, "0.0s")]
        [InlineData(1, 30, 15, 7, 123, "54h 15m 7.123s")]
        [InlineData(0, 10, 0, 1, 234, "10h 0m 1.234s")]
        [InlineData(0, 0, 13, 1, 234, "13m 1.234s")]
        [InlineData(0, 0, 0, 1, 234, "1.234s")]
        [InlineData(0, 0, 0, 0, 123456789, "34h 17m 36.789s")]
        public void TestDurationGetPrettyString(int days, int hours, int minutes, int seconds, int milliseconds, String expectedString)
        {
            var duration = new TimeSpan(days, hours, minutes, seconds, milliseconds);
            Assert.Equal<String>(expectedString, duration.GetPrettyString());
        }
	}
}

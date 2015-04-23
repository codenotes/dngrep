using System;
using System.Collections.Generic;
using System.Text;

namespace dnGREP.Common
{
	public class SearchDelegates
	{
        public delegate List<GrepSearchResult.GrepMatch> DoSearch(string text, string searchPattern, GrepSearchOption searchOptions, bool includeContext);
        public delegate string DoReplace(string text, string searchPattern, string replacePattern, GrepSearchOption searchOptions);
	}
}

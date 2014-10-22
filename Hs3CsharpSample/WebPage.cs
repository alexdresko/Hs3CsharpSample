using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Microsoft.VisualBasic;
using Scheduler;

namespace Hs3CsharpSample
{
	public class WebPage : PageBuilderAndMenu.clsPageBuilder
	{
		private int get_RefreshIntervalMilliSeconds;

		public WebPage(string pagename) : base(pagename)
		{
		}

		public override string postBackProc(string page, string data, string user, int userRights)
		{
			NameValueCollection parts = null;
			parts = HttpUtility.ParseQueryString(data);
			// handle postbacks here
			// update DIV'S with:
			//Me.divToUpdate.Add(DIV_ID, HTML_FOR_DIV)
			// refresh a page (this page or other page):
			//Me.pageCommands.Add("newpage", url)
			// open a dialog
			//Me.pageCommands.Add("opendialog", "dyndialog")
			if (parts["id"] == "b1")
			{
				divToUpdate.Add("current_time", "This div was just updated with this");
			}
			if (parts["action"] == "updatetime")
			{
				// ajax timer has expired and posted back to us, update the time
				divToUpdate.Add("current_time", DateTime.Now.ToString());
				if (DateTime.Now.Second == 0)
				{
					divToUpdate.Add("updatediv", "job complete");
				}
				else if (DateTime.Now.Second == 30)
				{
					divToUpdate.Add("updatediv", "working...");
				}
			}

			return base.postBackProc(page, data, user, userRights);
		}


		// build and return the actual page
		public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
		{
			var stb = new StringBuilder();

			try
			{
				reset();

				// handle any queries like mode=something
				NameValueCollection parts = null;
				if ((!string.IsNullOrEmpty(queryString)))
				{
					parts = HttpUtility.ParseQueryString(queryString);
				}

				// add any custom menu items

				// add any special header items
				//page.AddHeader(header_string)

				// add the normal title
				AddHeader(Util.hs.GetPageHeader(pageName, "Sample Plugin", "", "", false, false));

				stb.Append(DivStart("pluginpage", ""));

				// a message area for error messages from jquery ajax postback (optional, only needed if using AJAX calls to get data)
				stb.Append(DivStart("errormessage", "class='errormessage'"));
				stb.Append(DivEnd());

				// specific page starts here

				stb.Append("<div id='current_time'>" + DateTime.Now + "</div>" + Constants.vbCrLf);

				var b = new clsJQuery.jqButton("b1", "Button", Util.IFACE_NAME, false);
				stb.Append(b.Build());
				stb.Append(DivEnd());
				stb.Append("<br>pagename: " + pageName + "<br>");
				stb.Append("<br>This is instance: " + Util.Instance + "<br>");

				get_RefreshIntervalMilliSeconds = 2000;
				stb.Append(AddAjaxHandlerPost("action=updatetime", Util.IFACE_NAME));

				// add the body html to the page
				AddBody(stb.ToString());

				AddFooter(Util.hs.GetPageFooter());
				suppressDefaultFooter = true;

				// return the full page
				return BuildPage();
			}
			catch (Exception ex)
			{
				//WriteMon("Error", "Building page: " & ex.Message)
				return "error";
			}
		}
	}
}
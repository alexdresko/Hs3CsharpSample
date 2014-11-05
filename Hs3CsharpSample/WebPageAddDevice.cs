using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Scheduler;

namespace HSPI_HS3CSHARPSAMPLE
{
	public class WebPageAddDevice : PageBuilderAndMenu.clsPageBuilder
	{
		public WebPageAddDevice(string pagename) : base(pagename)
		{
		}

		public override string postBackProc(string page, string data, string user, int userRights)
		{
			NameValueCollection parts = null;
			parts = HttpUtility.ParseQueryString(data);

			return base.postBackProc(page, data, user, userRights);
		}


		// build and return the actual page
		public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
		{
			var stb = new StringBuilder();

			try
			{
				reset();

				stb.Append("This is the add device config");

				return stb.ToString();
			}
			catch (Exception ex)
			{
				//WriteMon("Error", "Building page: " & ex.Message)
				return "error";
			}
		}
	}
}
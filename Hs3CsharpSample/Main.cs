using System;
using System.Threading;
using HomeSeerAPI;
using HSCF.Communication.Scs.Communication;
using HSCF.Communication.Scs.Communication.EndPoints.Tcp;
using HSCF.Communication.ScsServices.Client;

namespace ConsoleApplication1
{
	internal static class Program
	{
		private static IScsServiceClient<IHSApplication> withEventsField_client;

		private static IScsServiceClient<IAppCallbackAPI> clientCallback;
		private static IHSApplication host;
		// real plugin functions, user supplied
		public static HSPI plugin = new HSPI();

		public static IScsServiceClient<IHSApplication> client
		{
			get { return withEventsField_client; }
			set
			{
				if (withEventsField_client != null)
				{
					withEventsField_client.Disconnected -= client_Disconnected;
				}
				withEventsField_client = value;
				if (withEventsField_client != null)
				{
					withEventsField_client.Disconnected += client_Disconnected;
				}
			}
		}


		public static void Main(string[] argv)
		{
			var sIp = "127.0.0.1";

			string sCmd = null;
			foreach (var sCmd_loopVariable in argv)
			{
				sCmd = sCmd_loopVariable;
				var ch = new string[1];
				ch[0] = "=";
				var parts = sCmd.Split(ch, StringSplitOptions.None);
				switch (parts[0].ToLower())
				{
					case "server":
						sIp = parts[1];
						break;
					case "instance":
						try
						{
							Util.Instance = parts[1];
						}
						catch (Exception ex)
						{
							Util.Instance = "";
						}
						break;
				}
			}

			Console.WriteLine("Plugin: " + Util.IFACE_NAME + " Instance: " + Util.Instance + " starting...");
			Console.WriteLine("Connecting to server at " + sIp + "...");
			client = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(sIp, 10400), plugin);
			clientCallback = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(sIp, 10400), plugin);
			var Attempts = 1;
			TryAgain:

			try
			{
				client.Connect();
				clientCallback.Connect();

				double APIVersion = 0;

				try
				{
					host = client.ServiceProxy;
					APIVersion = host.APIVersion;
					// will cause an error if not really connected
					Console.WriteLine("Host API Version: " + APIVersion);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error getting API version from host object: " + ex.Message + "->" + ex.StackTrace);
					//Return
				}

				try
				{
					Util.callback = clientCallback.ServiceProxy;
					APIVersion = Util.callback.APIVersion;
					// will cause an error if not really connected
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error getting API version from callback object: " + ex.Message + "->" + ex.StackTrace);
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Cannot connect attempt " + Attempts + ": " + ex.Message);
				if (ex.Message.ToLower().Contains("timeout occurred."))
				{
					Attempts += 1;
					if (Attempts < 6)
						goto TryAgain;
				}

				if (client != null)
				{
					client.Dispose();
					client = null;
				}
				if (clientCallback != null)
				{
					clientCallback.Dispose();
					clientCallback = null;
				}
				wait(4);
				return;
			}

			try
			{
				// create the user object that is the real plugin, accessed from the pluginAPI wrapper
				Util.callback = Util.callback;
				Util.hs = host;
				plugin.OurInstanceFriendlyName = Util.Instance;
				// connect to HS so it can register a callback to us
				host.Connect(Util.IFACE_NAME, Util.Instance);
				Console.WriteLine("Connected, waiting to be initialized...");
				do
				{
					Thread.Sleep(10);
				} while (client.CommunicationState == CommunicationStates.Connected & !HSPI.bShutDown);
				Console.WriteLine("Connection lost, exiting");
				// disconnect from server for good here
				client.Disconnect();
				clientCallback.Disconnect();
				wait(2);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Cannot connect(2): " + ex.Message);
				wait(2);
				Environment.Exit(0);
			}
		}

		private static void client_Disconnected(object sender, EventArgs e)
		{
			Console.WriteLine("Disconnected from server - client");
		}


		private static void wait(int secs)
		{
			Thread.Sleep(secs*1000);
		}
	}
}
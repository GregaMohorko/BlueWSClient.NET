using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueWSClient.NET.Net;

namespace BlueWSClient.NET
{
	public class WebService
	{
		/// <summary>
		/// The address of the .php script on which to call the actions.
		/// </summary>
		public readonly string ServerAddress;

		/// <summary>
		/// The http method used to send data to the server. Allowed methods are POST and GET.
		/// </summary>
		public readonly HttpMethod HttpMethod;
		
		/// <param name="serverAddress">The address of the .php script on which to call the actions.</param>
		/// <param name="httpMethod">The http method used to send data to the server. Allowed methods are POST and GET.</param>
		public WebService(string serverAddress,HttpMethod httpMethod)
		{
			ServerAddress = serverAddress;
			HttpMethod = httpMethod;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlueWSClient.NET.Net
{
	public class BlueWebClient : IDisposable
	{
		public const int DEFAULT_TIMEOUT = 10000;

		private readonly BlueWebClientInternal webClient;

		public BlueWebClient() : this(DEFAULT_TIMEOUT) { }

		public BlueWebClient(int timeout)
		{
			webClient = new BlueWebClientInternal();
			Timeout = timeout;
		}

		public void Dispose()
		{
			webClient.Dispose();
		}

		/// <summary>
		/// Gets or sets the length of time, in milliseconds, before the request times out.
		/// </summary>
		public int Timeout
		{
			get { return webClient.Timeout; }
			set { webClient.Timeout = value; }
		}

		/// <summary>
		/// Uploads the specified name/value collection to the resource identified by the specified URI.
		/// </summary>
		/// <param name="address">The URI of the resource to receive the collection.</param>
		/// <param name="data">The NameValueCollection to send to the resource.</param>
		/// <param name="method">The http method used to send data to the resource. Allowed methods are POST and GET.</param>
		public string UploadValues(string address,NameValueCollection data,HttpMethod method)
		{
			switch(method) {
				case HttpMethod.Get:
					return UploadValuesGet(address, data);
				case HttpMethod.Post:
					return UploadValuesPost(address, data);
			}
			throw new NotImplementedException("Unsupported http method.");
		}

		public Task<string> UploadValuesAsyncTask(string address,NameValueCollection data,HttpMethod method)
		{
			switch(method) {
				case HttpMethod.Get:
					return UploadValuesGetAsyncTask(address, data);
				case HttpMethod.Post:
					return UploadValuesPostAsyncTask(address, data);
			}
			throw new NotImplementedException("Unsupported http method.");
		}

		private string UploadValuesGet(string address, NameValueCollection data)
		{
			AddGetParameters(ref address, data);
			return webClient.DownloadString(address);
		}

		private async Task<string> UploadValuesGetAsyncTask(string address,NameValueCollection data)
		{
			AddGetParameters(ref address, data);
			return await webClient.DownloadStringTaskAsync(address);
		}

		private void AddGetParameters(ref string address,NameValueCollection data)
		{
			if(data.Count == 0) {
				return;
			}
			
			List<string> collection = new List<string>(data.Count);
			foreach(string name in data.AllKeys) {
				string value = data.Get(name);
				collection.Add($"{name}={value}");
			}
			address += "?" + string.Join("&", collection);
		}

		private string UploadValuesPost(string address, NameValueCollection data)
		{
			byte[] byteArray = webClient.UploadValues(address, data);
			return BytesToString(byteArray);
		}

		private async Task<string> UploadValuesPostAsyncTask(string address,NameValueCollection data)
		{
			byte[] byteArray = await webClient.UploadValuesTaskAsync(address, data);
			return BytesToString(byteArray);
		}

		private string BytesToString(byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>
		/// Solution for timeout came from here: http://w3ka.blogspot.si/2009/12/how-to-fix-webclient-timeout-issue.html
		/// </summary>
		private class BlueWebClientInternal:WebClient
		{
			/// <summary>
			/// Gets or sets the length of time, in milliseconds, before the request times out.
			/// </summary>
			public int Timeout { get; set; }

			/// <summary>
			/// Overriden just for added functionality of timeout.
			/// </summary>
			protected override WebRequest GetWebRequest(Uri address)
			{
				WebRequest request = base.GetWebRequest(address);

				if(request!=null)
					request.Timeout = Timeout;

				return request;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BlueWSClient.NET.Net;
using Newtonsoft.Json;

namespace BlueWSClient.NET.Requests
{
	/// <summary>
	/// Represents a request to the web service server.
	/// </summary>
	/// <typeparam name="T">The type of the server response.</typeparam>
	public class Request<T>
	{
		/// <summary>
		/// Gets the WebService associated with this request.
		/// </summary>
		public readonly WebService WebService;

		/// <summary>
		/// Parameters to be sent to the server.
		/// </summary>
		public List<object> Parameters;

		/// <summary>
		/// True if everything was successful.
		/// </summary>
		public bool Success { get; protected set; }
		/// <summary>
		/// Decoded server response.
		/// </summary>
		public T Response { get; protected set; }

		/// <summary>
		/// Raw server response.
		/// </summary>
		public string RawResponse { get; private set; }
		/// <summary>
		/// True if the web service call was unsuccessful due to no internet connection.
		/// </summary>
		public bool NoNetwork { get; private set; }
		/// <summary>
		/// The action that was called.
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// Creates a new request for the provided web service.
		/// </summary>
		/// <param name="webService">The web service on which to make the request.</param>
		public Request(WebService webService)
		{
			WebService = webService;
			Parameters = new List<object>();
		}

		/// <summary>
		/// Calls the associated server with the specified action as an asynchronous action.
		/// </summary>
		/// <param name="action">The name of the action to call.</param>
		public async Task<T> CallAsyncTask(string action)
		{
			NameValueCollection data=BeforeCalling(action);

			try {
				string address = WebService.ServerAddress;
				HttpMethod httpMethod = WebService.HttpMethod;
				using(BlueWebClient webClient = new BlueWebClient()) {
					RawResponse = await webClient.UploadValuesAsyncTask(address, data, httpMethod);
				}
			} catch(WebException) {
				NoNetwork = true;
				return default(T);
			}

			AfterCalling();

			return Response;
		}

		/// <summary>
		/// Calls the associated server with the specified action.
		/// </summary>
		/// <param name="action">The name of the action to call.</param>
		public T Call(string action)
		{
			NameValueCollection data = BeforeCalling(action);
			
			try {
				string address = WebService.ServerAddress;
				HttpMethod httpMethod = WebService.HttpMethod;
				using(BlueWebClient webClient=new BlueWebClient()) {
					RawResponse = webClient.UploadValues(address, data, httpMethod);
				}
			} catch(WebException) {
				NoNetwork = true;
				return default(T);
			}

			AfterCalling();

			return Response;
		}

		private NameValueCollection BeforeCalling(string action)
		{
			Action = action;
			// set it to false, and only set it to true after, when everything is ok
			Success = false;

			NameValueCollection data = new NameValueCollection();
			data.Add("action", action);
			if(Parameters.Count > 0) {
				object value = Parameters.Count == 1 ? Parameters[0] : Parameters;
				string json = JsonConvert.SerializeObject(value);
				data.Add("data", json);
			}

			return data;
		}

		private void AfterCalling()
		{
			try {
				Response = ParseServerResponse(RawResponse);
				Success = true;
				OnParseSuccess();
			} catch(Exception e) {
				OnParseFailure(e);
			}
		}

		/// <summary>
		/// Override this method if you want to parse the server response yourself. The default implementation uses Newtonsoft.Json.JsonConvert.DeserializeObject method.
		/// </summary>
		/// <param name="rawResponse">Raw server response to parse.</param>
		protected virtual T ParseServerResponse(string rawResponse)
		{
			return JsonConvert.DeserializeObject<T>(rawResponse);
		}

		/// <summary>
		/// Override this method if you want to do something when the raw server response is parsed.
		/// </summary>
		protected virtual void OnParseSuccess() { }

		/// <summary>
		/// Override this method if you want to do something when the raw server response could not be parsed.
		/// </summary>
		/// <param name="e">The exception thrown while parsing.</param>
		protected virtual void OnParseFailure(Exception e) { }
	}
}

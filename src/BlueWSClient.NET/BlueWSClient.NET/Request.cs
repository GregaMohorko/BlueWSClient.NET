/*
   Copyright 2018 Grega Mohorko

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

Project: BlueWSClient.NET
Created: 2018-1-7
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BlueWS.Net;
using Newtonsoft.Json;

namespace BlueWS
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
		public readonly Dictionary<string,object> Parameters;

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
			Parameters = new Dictionary<string, object>();
		}

		/// <summary>
		/// Calls the associated server with the specified action as an asynchronous action.
		/// <para>If the action is not specified, it will be set to the name of the calling method.</para>
		/// </summary>
		/// <param name="action">The name of the action to call.</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public async Task<T> CallAsyncTask(string action=null)
		{
			if(action == null) {
				action = GetCallingMethod().Name;
			}

			NameValueCollection data=BeforeCalling(action);

			try {
				string address = WebService.ServerAddress;
				HttpMethod httpMethod = WebService.HttpMethod;
				using(var webClient = new BlueWebClient()) {
					RawResponse = await webClient.UploadValuesAsyncTask(address, data, httpMethod);
				}
				AfterCalling();
			} catch(WebException e) {
				OnWebException(e);
			}
			
			return Response;
		}

		/// <summary>
		/// Calls the associated server with the specified action.
		/// <para>If the action is not specified, it will be set to the name of the calling method.</para>
		/// </summary>
		/// <param name="action">The name of the action to call.</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public T Call(string action=null)
		{
			if(action == null) {
				action = GetCallingMethod().Name;
			}

			NameValueCollection data = BeforeCalling(action);
			
			try {
				string address = WebService.ServerAddress;
				HttpMethod httpMethod = WebService.HttpMethod;
				using(var webClient=new BlueWebClient()) {
					RawResponse = webClient.UploadValues(address, data, httpMethod);
				}
				AfterCalling();
			} catch(WebException e) {
				OnWebException(e);
			}
			
			return Response;
		}

		private void OnWebException(WebException e)
		{
			NoNetwork = true;
			if(WebService.IsThrowable) {
				throw e;
			}
		}

		/// <summary>
		/// Resets all the request info and serializes the parameters to prepare the data to be sent to the server.
		/// </summary>
		/// <param name="action">The action to be called.</param>
		private NameValueCollection BeforeCalling(string action)
		{
			Action = action;
			// set it to false, and only set it to true after, when everything is ok
			Success = false;
			RawResponse = null;
			Response = default(T);
			NoNetwork = false;

			var data = new NameValueCollection
			{
				{ "action", action }
			};
			if(Parameters.Count > 0) {
				string serializedParameters = SerializeParameters();
				data.Add("data", serializedParameters);
			}

			return data;
		}

		/// <summary>
		/// Tries to parse the raw response.
		/// </summary>
		private void AfterCalling()
		{
			try {
				Response = ParseServerResponse(RawResponse);
				Success = true;
			} catch(Exception e) {
				FailedParseAction result = OnParseFailure(e);
				if(WebService.IsThrowable && result==FailedParseAction.THROW) {
					throw e;
				}
				if(result != FailedParseAction.REPARSED) {
					return;
				}
			}

			OnParseSuccess();
		}

		/// <summary>
		/// Override this method if you want to serialize the <see cref="Parameters"/> to be sent to the server yourself. The default implementation uses <see cref="JsonConvert.SerializeObject(object)"/> method.
		/// </summary>
		protected virtual string SerializeParameters()
		{
			return JsonConvert.SerializeObject(Parameters);
		}

		/// <summary>
		/// Override this method if you want to parse the server response yourself. The default implementation uses <see cref="JsonConvert.DeserializeObject{T}(string)"/> method.
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
		protected virtual FailedParseAction OnParseFailure(Exception e)
		{
			return FailedParseAction.THROW;
		}

		/// <summary>
		/// Determines the action after a parse attempt has failed.
		/// </summary>
		public enum FailedParseAction
		{
			/// <summary>
			/// Means that the exception was not handled and that it should be thrown.
			/// </summary>
			THROW,
			/// <summary>
			/// Means that the exception was handled, but the parsing was still not done, so the method <see cref="OnParseSuccess"/> will not be called.
			/// </summary>
			HANDLED,
			/// <summary>
			/// Means that you have manually re-parsed the <see cref="RawResponse"/> successfully and that <see cref="OnParseSuccess"/> should be called normally.
			/// </summary>
			REPARSED
		}
	}
}

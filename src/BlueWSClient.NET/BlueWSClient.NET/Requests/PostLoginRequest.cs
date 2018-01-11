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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlueWS.Requests
{
	/// <summary>
	/// An example of a request type for post login actions.
	/// </summary>
	/// <typeparam name="T">The type of the server response.</typeparam>
	public class PostLoginRequest<T> : Request<T>
	{
		private const string MESSAGE_USERDENIED = "User denied: ";

		/// <summary>
		/// If true, indicates that the user access was denied.
		/// </summary>
		public bool UserDenied => UserDeniedReason != null;

		/// <summary>
		/// The reason why the user access was denied.
		/// </summary>
		public string UserDeniedReason { get; private set; }

		/// <summary>
		/// Creates a new instance of <see cref="PostLoginRequest{T}"/>.
		/// </summary>
		/// <param name="webService">The web service of the request.</param>
		public PostLoginRequest(WebService webService) : base(webService) { }

		/// <summary>
		/// Checks if the user was denied.
		/// </summary>
		protected override void OnParseSuccess()
		{
			if(typeof(T) == typeof(JObject)) {
				CheckIfUserDenied(Response as JObject);
			}
		}

		/// <summary>
		/// Tries to parse as <see cref="JObject"/> and then checks if the user was denied.
		/// </summary>
		protected override FailedParseAction OnParseFailure(Exception e)
		{
			JObject response;
			try {
				response = JsonConvert.DeserializeObject<JObject>(RawResponse);
			} catch(Exception) {
				return FailedParseAction.THROW;
			}

			CheckIfUserDenied(response);

			return FailedParseAction.HANDLED;
		}

		private void CheckIfUserDenied(JObject response)
		{
			string error = response.Value<string>("Error");
			if(error == null || !error.StartsWith(MESSAGE_USERDENIED)) {
				return;
			}

			UserDeniedReason = error.Substring(MESSAGE_USERDENIED.Length);
			Success = false;
			Response = default(T);
		}
	}
}

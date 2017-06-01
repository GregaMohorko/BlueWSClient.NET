using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlueWSClient.NET.Requests
{
	/// <summary>
	/// The request type for post login actions.
	/// </summary>
	/// <typeparam name="T">The type of the server response.</typeparam>
	public class PostLoginRequest<T> : Request<T>
	{
		private const string MESSAGE_USERDENIED = "User denied: ";

		/// <summary>
		/// If true, indicates that the user access was denied.
		/// </summary>
		public bool UserDenied { get { return UserDeniedReason != null; } }

		/// <summary>
		/// The reason why the user access was denied.
		/// </summary>
		public string UserDeniedReason { get; private set; }

		public PostLoginRequest(WebService webService) : base(webService) { }

		protected override void OnParseSuccess()
		{
			if(typeof(T) == typeof(JObject)) {
				CheckIfUserDenied(Response as JObject);
			}
		}

		protected void OnParseFailure()
		{
			JObject response;
			try {
				response = JsonConvert.DeserializeObject<JObject>(RawResponse);
			} catch(Exception) {
				return;
			}

			CheckIfUserDenied(response);
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

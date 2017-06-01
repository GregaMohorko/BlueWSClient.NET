using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueWSClient.NET.Net;
using BlueWSClient.NET.Requests;
using Newtonsoft.Json.Linq;

namespace BlueWSClient.NET.Test
{
	class Program
	{
		// please set your own server address
		const string serverAddress = "http://psis2.mohorko.info/test/";

		static void Main(string[] args)
		{
			WebService webService = new WebService(serverAddress,HttpMethod.Get);

			// examples below are testing actions in the BlueWS repository in the /test folder
			// you can test your own actions in the same manner
			TestAction1(webService).Wait();
			TestAction2(webService);
			TestAction3(webService);
			TestAction4(webService);
			TestAction5(webService);
			TestAction6(webService);
			TestAction7(webService);
			TestAction8(webService);
			TestAction0(webService).Wait();

			Console.Write("TESTING FINISHED");
			Console.ReadKey(true);
		}

		private static async Task TestAction1(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 1 ...");
			Console.WriteLine();
			
			// the sync version
			{
				Request<JObject> request = new Request<JObject>(webService);
				JObject response = request.Call("TestAction1");
				CheckResult(request, response);
				string message = response.Value<string>("Message");
				Debug.Assert(message == "Hello world!");
			}

			// the async version
			{
				Request<JObject> request = new Request<JObject>(webService);
				JObject response = await request.CallAsyncTask("TestAction1");
				CheckResult(request, response);
				string message = response.Value<string>("Message");
				Debug.Assert(message == "Hello world!");
			}
		}

		private static void TestAction2(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 2 ...");
			Console.WriteLine();

			Request<JObject> request = new Request<JObject>(webService);
			JObject response = request.Call("TestAction2");
			Debug.Assert(!request.Success);
			Debug.Assert(!request.NoNetwork);
			Debug.Assert(request.RawResponse != null);
			Debug.Assert(ReferenceEquals(request.Response, response));
			Debug.Assert(request.Response == null);
		}

		private static void TestAction3(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 3 ...");
			Console.WriteLine();

			Request<JObject> request = new Request<JObject>(webService);
			JObject response = request.Call("TestAction3");
			CheckResult(request, response);
			string explanation = response.Value<string>("Explanation");
			Debug.Assert(explanation == "You did something wrong.");
		}

		private static void TestAction4(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 4 ...");
			Console.WriteLine();

			PostLoginRequest<JObject> request = new PostLoginRequest<JObject>(webService);
			JObject response = request.Call("TestAction4");
			CheckResult(request, response);
			string message = response.Value<string>("Message");
			Debug.Assert(message == "Success");
			Debug.Assert(!request.UserDenied);
			Debug.Assert(request.UserDeniedReason == null);
		}

		private static void TestAction5(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 5 ...");
			Console.WriteLine();

			PostLoginRequest<JObject> request = new PostLoginRequest<JObject>(webService);
			JObject response = request.Call("TestAction5");
			Debug.Assert(!request.Success);
			Debug.Assert(!request.NoNetwork);
			Debug.Assert(request.RawResponse != null);
			Debug.Assert(ReferenceEquals(request.Response, response));
			Debug.Assert(request.UserDenied);
			Debug.Assert(request.UserDeniedReason == "The user must be admin.");
			Debug.Assert(request.Response == null);
		}

		private static void TestAction6(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 6 ...");
			Console.WriteLine();

			PostLoginRequest<JObject> request = new PostLoginRequest<JObject>(webService);
			JObject response = request.Call("TestAction6");
			CheckResult(request, response);
			string message = response.Value<string>("Message");
			Debug.Assert(message == "Success");
			Debug.Assert(!request.UserDenied);
			Debug.Assert(request.UserDeniedReason == null);
		}

		private static void TestAction7(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 7 ...");
			Console.WriteLine();

			Request<JObject> request = new Request<JObject>(webService);
			JObject response = request.Call("TestAction7");
			CheckResult(request, response);
			int actionParameter = response.Value<int>("ActionParameter");
			Debug.Assert(actionParameter == 42);
		}

		private static void TestAction8(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 8 ...");
			Console.WriteLine();

			// sending no data
			Request<JObject> request = new Request<JObject>(webService);
			JObject response = request.Call("TestAction8");
			CheckResult(request, response);
			Debug.Assert(response.Count == 1);
			JProperty dataProperty = response.AsJEnumerable().Single() as JProperty;
			Debug.Assert(dataProperty != null);
			Debug.Assert(dataProperty.Name == "data");
			JValue dataValue = dataProperty.Value as JValue;
			Debug.Assert(dataValue.Value == null);

			// sending an integer
			request = new Request<JObject>(webService);
			int sentInteger = 42;
			request.Parameters.Add(sentInteger);
			response = request.Call("TestAction8");
			CheckResult(request, response);
			int returnedInteger = response.Value<int>("data");
			Debug.Assert(returnedInteger == sentInteger);

			// sending three integers
			request = new Request<JObject>(webService);
			List<int> sentIntegers = new List<int>(){ 27, 42, 67 };
			sentIntegers.ForEach(i => request.Parameters.Add(i));
			response = request.Call("TestAction8");
			CheckResult(request, response);
			List<int> returnedIntegers = response.Value<JArray>("data").Select(jt => jt.Value<int>()).ToList();
			Debug.Assert(returnedIntegers.SequenceEqual(sentIntegers));
		}

		private static async Task TestAction0(WebService webService)
		{
			Console.WriteLine();
			Console.WriteLine("Testing Action 0 ...");
			Console.WriteLine();

			// the sync version
			Request<JObject> request = new Request<JObject>(webService);
			JObject response = request.Call("TestAction0");
			Debug.Assert(!request.Success);
			Debug.Assert(!request.NoNetwork);
			Debug.Assert(request.RawResponse != null);
			Debug.Assert(request.Response == null);

			// the async version
			request = new Request<JObject>(webService);
			response = await request.CallAsyncTask("TestAction0");
			Debug.Assert(!request.Success);
			Debug.Assert(!request.NoNetwork);
			Debug.Assert(request.RawResponse != null);
			Debug.Assert(request.Response == null);
		}

		private static void CheckResult<T>(Request<T> request,T response)
		{
			Debug.Assert(request != null);
			Debug.Assert(request.Success);
			Debug.Assert(!request.NoNetwork);
			Debug.Assert(request.RawResponse != null);
			Debug.Assert(ReferenceEquals(request.Response, response));
		}
	}
}

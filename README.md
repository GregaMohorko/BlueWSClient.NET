# BlueWSClient.NET
A client library in .NET for [BlueWS](https://github.com/GregaMohorko/BlueWS) library. Includes logic and utility for calling BlueWS web service actions.

## Documentation & Tutorials
You can read the documentation and tutorials under the [Wiki](https://github.com/GregaMohorko/BlueWSClient.NET/wiki).

## Short examples
Creating a web service instance:
```C#
WebService webService = new WebService("https://www.myserver.com/", HttpMethod.Get);
```

Getting the response for an action *TestAction*:
```C#
Request<JObject> request = new Request<JObject>(webService);
JObject response = request.Call("TestAction");
```

The above example was synchronous and blocked the calling thread.

The asynchronous call looks like this:
```C#
JObject response = await request.CallAsyncTask("TestAction");
```

To send parameters to the server, add them to the ```Parameter``` property of the request before calling:
```C#
request.Parameters.Add(42);
JObject response = request.Call("TestAction");
```

To get the response in your own type:
```C#
Request<YourClass> request = new Request<YourClass>(webService);
YourClass response = request.Call("TestAction");
```

## Requirements
.NET Framework 4.6.1

## License
[Apache License 2.0](./LICENSE)

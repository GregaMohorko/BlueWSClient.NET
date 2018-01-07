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

namespace BlueWS.Net
{
	/// <summary>
	/// A HTTP method.
	/// </summary>
	public enum HttpMethod
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		UNKNOWN=0,
		/// <summary>
		/// Get method.
		/// </summary>
		GET=1,
		/// <summary>
		/// Post method.
		/// </summary>
		POST=2
	}
}

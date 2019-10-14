using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.web
{

	public sealed class HttpMethod
	{
		 public static readonly HttpMethod Options = new HttpMethod( "Options", InnerEnum.Options );
		 public static readonly HttpMethod Get = new HttpMethod( "Get", InnerEnum.Get );
		 public static readonly HttpMethod Head = new HttpMethod( "Head", InnerEnum.Head );
		 public static readonly HttpMethod Post = new HttpMethod( "Post", InnerEnum.Post );
		 public static readonly HttpMethod Put = new HttpMethod( "Put", InnerEnum.Put );
		 public static readonly HttpMethod Patch = new HttpMethod( "Patch", InnerEnum.Patch );
		 public static readonly HttpMethod Delete = new HttpMethod( "Delete", InnerEnum.Delete );
		 public static readonly HttpMethod Trace = new HttpMethod( "Trace", InnerEnum.Trace );
		 public static readonly HttpMethod Connect = new HttpMethod( "Connect", InnerEnum.Connect );

		 private static readonly IList<HttpMethod> valueList = new List<HttpMethod>();

		 static HttpMethod()
		 {
			 valueList.Add( Options );
			 valueList.Add( Get );
			 valueList.Add( Head );
			 valueList.Add( Post );
			 valueList.Add( Put );
			 valueList.Add( Patch );
			 valueList.Add( Delete );
			 valueList.Add( Trace );
			 valueList.Add( Connect );
		 }

		 public enum InnerEnum
		 {
			 Options,
			 Get,
			 Head,
			 Post,
			 Put,
			 Patch,
			 Delete,
			 Trace,
			 Connect
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private HttpMethod( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal Private const;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable public static HttpMethod valueOfOrNull(String name)
		 public static HttpMethod ValueOfOrNull( string name )
		 {
			  return _methodsByName.get( name );
		 }

		 private static IDictionary<string, HttpMethod> IndexMethodsByName()
		 {
			  HttpMethod[] methods = HttpMethod.values();
			  IDictionary<string, HttpMethod> result = new Dictionary<string, HttpMethod>( methods.Length * 2 );
			  foreach ( HttpMethod method in methods )
			  {
					result[method.name()] = method;
			  }
			  return unmodifiableMap( result );
		 }

		public static IList<HttpMethod> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static HttpMethod valueOf( string name )
		{
			foreach ( HttpMethod enumInstance in HttpMethod.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}
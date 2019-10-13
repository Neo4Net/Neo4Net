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
namespace Neo4Net.Server.plugins
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Description = org.hamcrest.Description;
	using Factory = org.hamcrest.Factory;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class PluginFunctionalTestHelper
	{
		 private PluginFunctionalTestHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, Object> makeGet(String url) throws org.neo4j.server.rest.domain.JsonParseException
		 public static IDictionary<string, object> MakeGet( string url )
		 {
			  JaxRsResponse response = ( new RestRequest() ).get(url);

			  string body = GetResponseText( response );
			  response.Close();

			  return DeserializeMap( body );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.Map<String, Object> deserializeMap(final String body) throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 protected internal static IDictionary<string, object> DeserializeMap( string body )
		 {
			  IDictionary<string, object> result = JsonHelper.jsonToMap( body );
			  assertThat( result, CoreMatchers.@is( not( nullValue() ) ) );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<java.util.Map<String, Object>> deserializeList(final String body) throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static IList<IDictionary<string, object>> DeserializeList( string body )
		 {
			  IList<IDictionary<string, object>> result = JsonHelper.jsonToList( body );
			  assertThat( result, CoreMatchers.@is( not( nullValue() ) ) );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected static String getResponseText(final org.neo4j.server.rest.JaxRsResponse response)
		 protected internal static string GetResponseText( JaxRsResponse response )
		 {
			  string body = response.Entity;

			  assertEquals( body, 200, response.Status );
			  return body;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.Map<String, Object> makePostMap(String url) throws org.neo4j.server.rest.domain.JsonParseException
		 protected internal static IDictionary<string, object> MakePostMap( string url )
		 {
			  JaxRsResponse response = ( new RestRequest() ).post(url,null);

			  string body = GetResponseText( response );
			  response.Close();

			  return DeserializeMap( body );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.Map<String, Object> makePostMap(String url, java.util.Map<String, Object> params) throws org.neo4j.server.rest.domain.JsonParseException
		 protected internal static IDictionary<string, object> MakePostMap( string url, IDictionary<string, object> @params )
		 {
			  string json = JsonHelper.createJsonFrom( @params );
			  JaxRsResponse response = ( new RestRequest() ).post(url, json, MediaType.APPLICATION_JSON_TYPE);

			  string body = GetResponseText( response );
			  response.Close();

			  return DeserializeMap( body );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.List<java.util.Map<String, Object>> makePostList(String url) throws org.neo4j.server.rest.domain.JsonParseException
		 protected internal static IList<IDictionary<string, object>> MakePostList( string url )
		 {
			  JaxRsResponse response = ( new RestRequest() ).post(url, null);

			  string body = GetResponseText( response );
			  response.Close();

			  return DeserializeList( body );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.List<java.util.Map<String, Object>> makePostList(String url, java.util.Map<String, Object> params) throws org.neo4j.server.rest.domain.JsonParseException
		 protected internal static IList<IDictionary<string, object>> MakePostList( string url, IDictionary<string, object> @params )
		 {
			  string json = JsonHelper.createJsonFrom( @params );
			  JaxRsResponse response = ( new RestRequest() ).post(url, json);

			  string body = GetResponseText( response );
			  response.Close();

			  return DeserializeList( body );
		 }

		 public class RegExp : TypeSafeMatcher<string>
		 {
			  internal abstract class MatchType
			  {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               end("ends with") { boolean match(String pattern, String string) { return string.endsWith(pattern); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               matches() { boolean match(String pattern, String string) { return string.matches(pattern); } };

					private static readonly IList<MatchType> valueList = new List<MatchType>();

					static MatchType()
					{
						valueList.Add( end );
						valueList.Add( matches );
					}

					public enum InnerEnum
					{
						end,
						matches
					}

					public readonly InnerEnum innerEnumValue;
					private readonly string nameValue;
					private readonly int ordinalValue;
					private static int nextOrdinal = 0;

					private MatchType( string name, InnerEnum innerEnum )
					{
						nameValue = name;
						ordinalValue = nextOrdinal++;
						innerEnumValue = innerEnum;
					}
					internal readonly string description;

					internal abstract bool match( string pattern, string @string );

					internal MatchType( string name, InnerEnum innerEnum )
					{
						 this.Description = name();

						nameValue = name;
						ordinalValue = nextOrdinal++;
						innerEnumValue = innerEnum;
					}

					internal MatchType( string name, InnerEnum innerEnum, string description )
					{
						 this.Description = description;

						nameValue = name;
						ordinalValue = nextOrdinal++;
						innerEnumValue = innerEnum;
					}

				  public static IList<MatchType> values()
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

				  public static MatchType valueOf( string name )
				  {
					  foreach ( MatchType enumInstance in MatchType.valueList )
					  {
						  if ( enumInstance.nameValue == name )
						  {
							  return enumInstance;
						  }
					  }
					  throw new System.ArgumentException( name );
				  }
			  }

			  internal readonly string Pattern;
			  internal string String;
			  internal readonly MatchType Type;

			  internal RegExp( string regexp, MatchType type )
			  {
					this.Pattern = regexp;
					this.Type = type;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Factory public static org.hamcrest.Matcher<String> endsWith(String pattern)
			  public static Matcher<string> EndsWith( string pattern )
			  {
					return new RegExp( pattern, MatchType.End );
			  }

			  public override bool MatchesSafely( string @string )
			  {
					this.String = @string;
					return Type.match( Pattern, @string );
			  }

			  public override void DescribeTo( Description descr )
			  {
					descr.appendText( "expected something that " ).appendText( Type.description ).appendText( " [" ).appendText( Pattern ).appendText( "] but got [" ).appendText( String ).appendText( "]" );
			  }
		 }

	}

}
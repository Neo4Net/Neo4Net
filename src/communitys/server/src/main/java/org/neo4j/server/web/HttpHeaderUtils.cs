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

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Log = Neo4Net.Logging.Log;

	public class HttpHeaderUtils
	{

		 public const string MAX_EXECUTION_TIME_HEADER = "max-execution-time";

		 public static readonly IDictionary<string, string> Charset = Collections.singletonMap( "charset", StandardCharsets.UTF_8.name() );

		 private HttpHeaderUtils()
		 {
		 }

		 public static MediaType MediaTypeWithCharsetUtf8( string mediaType )
		 {
			  return new MediaType( mediaType, null, Charset );
		 }

		 public static MediaType MediaTypeWithCharsetUtf8( MediaType mediaType )
		 {
			  IDictionary<string, string> parameters = mediaType.Parameters;
			  if ( parameters.Count == 0 )
			  {
					return new MediaType( mediaType.Type, mediaType.Subtype, Charset );
			  }
			  if ( parameters.ContainsKey( "charset" ) )
			  {
					return mediaType;
			  }
			  IDictionary<string, string> paramsWithCharset = new Dictionary<string, string>( parameters );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  paramsWithCharset.putAll( Charset );
			  return new MediaType( mediaType.Type, mediaType.Subtype, paramsWithCharset );
		 }

		 /// <summary>
		 /// Retrieve custom transaction timeout in milliseconds from numeric <seealso cref="MAX_EXECUTION_TIME_HEADER"/> request
		 /// header.
		 /// If header is not set returns -1. </summary>
		 /// <param name="request"> http request </param>
		 /// <param name="errorLog"> errors log for header parsing errors </param>
		 /// <returns> custom timeout if header set, -1 otherwise or when value is not a valid number. </returns>
		 public static long GetTransactionTimeout( HttpServletRequest request, Log errorLog )
		 {
			  string headerValue = request.getHeader( MAX_EXECUTION_TIME_HEADER );
			  if ( !string.ReferenceEquals( headerValue, null ) )
			  {
					try
					{
						 return long.Parse( headerValue );
					}
					catch ( System.FormatException e )
					{
						 errorLog.Error( string.Format( "Fail to parse `{0}` header with value: '{1}'. Should be a positive number.", MAX_EXECUTION_TIME_HEADER, headerValue ), e );
					}
			  }
			  return GraphDatabaseSettings.UNSPECIFIED_TIMEOUT;
		 }

		 /// <summary>
		 /// Validates given HTTP header name. Does not allow blank names and names with control characters, like '\n' (LF) and '\r' (CR).
		 /// Can be used to detect and neutralize CRLF in HTTP headers.
		 /// </summary>
		 /// <param name="name"> the HTTP header name, like 'Accept' or 'Content-Type'. </param>
		 /// <returns> {@code true} when given name represents a valid HTTP header, {@code false} otherwise. </returns>
		 public static bool IsValidHttpHeaderName( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) || name.Length == 0 )
			  {
					return false;
			  }
			  bool isBlank = true;
			  for ( int i = 0; i < name.Length; i++ )
			  {
					char c = name[i];
					if ( char.IsControl( c ) )
					{
						 return false;
					}
					if ( !char.IsWhiteSpace( c ) )
					{
						 isBlank = false;
					}
			  }
			  return !isBlank;
		 }
	}

}
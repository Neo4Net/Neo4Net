using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.rest.batch
{
	using JsonFactory = org.codehaus.jackson.JsonFactory;
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using JsonNode = org.codehaus.jackson.JsonNode;
	using JsonParser = org.codehaus.jackson.JsonParser;
	using JsonToken = org.codehaus.jackson.JsonToken;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;


	using InternalJettyServletRequest = Neo4Net.Server.rest.web.InternalJettyServletRequest;
	using RequestData = Neo4Net.Server.rest.web.InternalJettyServletRequest.RequestData;
	using InternalJettyServletResponse = Neo4Net.Server.rest.web.InternalJettyServletResponse;
	using WebServer = Neo4Net.Server.web.WebServer;

	public abstract class BatchOperations
	{
		 private static readonly Pattern _placeholderPattern = Pattern.compile( "\\{(\\d{1,10})}" );

		 protected internal const string ID_KEY = "id";
		 protected internal const string METHOD_KEY = "method";
		 protected internal const string BODY_KEY = "body";
		 protected internal const string TO_KEY = "to";
		 protected internal static readonly JsonFactory JsonFactory = new JsonFactory().disable(JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM);
		 protected internal readonly WebServer WebServer;
		 protected internal readonly ObjectMapper Mapper;

		 public BatchOperations( WebServer webServer )
		 {
			  this.WebServer = webServer;
			  Mapper = new ObjectMapper();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void addHeaders(final org.neo4j.server.rest.web.InternalJettyServletRequest res, final javax.ws.rs.core.HttpHeaders httpHeaders)
		 protected internal virtual void AddHeaders( InternalJettyServletRequest res, HttpHeaders httpHeaders )
		 {
			  foreach ( KeyValuePair<string, IList<string>> header in httpHeaders.RequestHeaders.entrySet() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String key = header.getKey();
					string key = header.Key;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> value = header.getValue();
					IList<string> value = header.Value;
					if ( value == null )
					{
						 continue;
					}
					if ( value.Count != 1 )
					{
						 throw new System.ArgumentException( "expecting one value per header" );
					}
					if ( !key.Equals( "Accept" ) && !key.Equals( "Content-Type" ) )
					{
						 res.AddHeader( key, value[0] );
					}
			  }
			  // Make sure they are there and always json
			  // Taking advantage of Map semantics here
			  res.AddHeader( "Accept", "application/json" );
			  res.AddHeader( "Content-Type", "application/json" );
		 }

		 protected internal virtual URI CalculateTargetUri( UriInfo serverUriInfo, string requestedPath )
		 {
			  URI baseUri = serverUriInfo.BaseUri;

			  if ( requestedPath.StartsWith( baseUri.ToString(), StringComparison.Ordinal ) )
			  {
					requestedPath = requestedPath.Substring( baseUri.ToString().Length );
			  }

			  if ( !requestedPath.StartsWith( "/", StringComparison.Ordinal ) )
			  {
					requestedPath = "/" + requestedPath;
			  }

			  return baseUri.resolve( "." + requestedPath );
		 }

		 protected internal virtual string ReplaceLocationPlaceholders( string str, IDictionary<int, string> locations )
		 {
			  if ( !str.Contains( "{" ) )
			  {
					return str;
			  }
			  Matcher matcher = _placeholderPattern.matcher( str );
			  StringBuilder sb = new StringBuilder();
			  string replacement = null;
			  while ( matcher.find() )
			  {
					string id = matcher.group( 1 );
					try
					{
						 replacement = locations[Convert.ToInt32( id )];
					}
					catch ( System.FormatException )
					{
						 // The body contained a value that happened to match our regex, but is not a valid integer.
						 // Specifically, the digits inside the brackets must have been > 2^31-1.
						 // Simply ignore this, since we don't support non-integer placeholders, this is not a valid placeholder
					}
					if ( !string.ReferenceEquals( replacement, null ) )
					{
						 matcher.appendReplacement( sb, replacement );
					}
					else
					{
						 matcher.appendReplacement( sb, matcher.group() );
					}
			  }
			  matcher.appendTail( sb );
			  return sb.ToString();
		 }

		 protected internal virtual bool Is2XXStatusCode( int statusCode )
		 {
			  return statusCode >= 200 && statusCode < 300;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void parseAndPerform(javax.ws.rs.core.UriInfo uriInfo, javax.ws.rs.core.HttpHeaders httpHeaders, javax.servlet.http.HttpServletRequest req, java.io.InputStream body, java.util.Map<int, String> locations) throws java.io.IOException, javax.servlet.ServletException
		 protected internal virtual void ParseAndPerform( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body, IDictionary<int, string> locations )
		 {
			  JsonParser jp = JsonFactory.createJsonParser( body );
			  JsonToken token;
			  InternalJettyServletRequest.RequestData requestData = InternalJettyServletRequest.RequestData.from( req );

			  while ( ( token = jp.nextToken() ) != null )
			  {
					if ( token == JsonToken.START_OBJECT )
					{
						 string jobMethod = "";
						 string jobPath = "";
						 string jobBody = "";
						 int? jobId = null;
						 while ( ( token = jp.nextToken() ) != JsonToken.END_OBJECT && token != null )
						 {
							  string field = jp.Text;
							  jp.nextToken();
							  switch ( field )
							  {
							  case METHOD_KEY:
									jobMethod = jp.Text.ToUpper();
									break;
							  case TO_KEY:
									jobPath = jp.Text;
									break;
							  case ID_KEY:
									jobId = jp.IntValue;
									break;
							  case BODY_KEY:
									jobBody = ReadBody( jp );
									break;
							  default:
									break;
							  }
						 }
						 // Read one job description. Execute it.
						 PerformRequest( uriInfo, jobMethod, jobPath, jobBody, jobId, httpHeaders, locations, requestData );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String readBody(org.codehaus.jackson.JsonParser jp) throws java.io.IOException
		 private string ReadBody( JsonParser jp )
		 {
			  JsonNode node = Mapper.readTree( jp );
			  StringWriter @out = new StringWriter();
			  JsonGenerator gen = JsonFactory.createJsonGenerator( @out );
			  Mapper.writeTree( gen, node );
			  gen.flush();
			  gen.close();
			  return @out.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void performRequest(javax.ws.rs.core.UriInfo uriInfo, String method, String path, String body, System.Nullable<int> id, javax.ws.rs.core.HttpHeaders httpHeaders, java.util.Map<int, String> locations, org.neo4j.server.rest.web.InternalJettyServletRequest.RequestData requestData) throws java.io.IOException, javax.servlet.ServletException
		 protected internal virtual void PerformRequest( UriInfo uriInfo, string method, string path, string body, int? id, HttpHeaders httpHeaders, IDictionary<int, string> locations, InternalJettyServletRequest.RequestData requestData )
		 {
			  path = ReplaceLocationPlaceholders( path, locations );
			  body = ReplaceLocationPlaceholders( body, locations );
			  URI targetUri = CalculateTargetUri( uriInfo, path );

			  InternalJettyServletResponse res = new InternalJettyServletResponse();
			  InternalJettyServletRequest req = new InternalJettyServletRequest( method, targetUri.ToString(), body, res, requestData );
			  AddHeaders( req, httpHeaders );

			  Invoke( method, path, body, id, targetUri, req, res );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void invoke(String method, String path, String body, System.Nullable<int> id, java.net.URI targetUri, org.neo4j.server.rest.web.InternalJettyServletRequest req, org.neo4j.server.rest.web.InternalJettyServletResponse res) throws java.io.IOException, javax.servlet.ServletException;
		 protected internal abstract void Invoke( string method, string path, string body, int? id, URI targetUri, InternalJettyServletRequest req, InternalJettyServletResponse res );
	}

}
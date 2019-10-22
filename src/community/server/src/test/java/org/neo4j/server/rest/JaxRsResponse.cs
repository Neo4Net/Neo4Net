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
namespace Neo4Net.Server.rest
{
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using StringKeyObjectValueIgnoreCaseMultivaluedMap = com.sun.jersey.core.util.StringKeyObjectValueIgnoreCaseMultivaluedMap;
	using Test = org.junit.Test;


	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Database = Neo4Net.Server.database.Database;
	using ConsoleService = Neo4Net.Server.rest.management.console.ConsoleService;
	using ServerRootRepresentation = Neo4Net.Server.rest.management.repr.ServerRootRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class JaxRsResponse : Response
	{

		 private readonly int _status;
		 private readonly MultivaluedMap<string, object> _metaData;
		 private readonly MultivaluedMap<string, string> _headers;
		 private readonly URI _location;
		 private string _data;
		 private MediaType _type;

		 public JaxRsResponse( ClientResponse response ) : this( response, ExtractContent( response ) )
		 {
		 }

		 public JaxRsResponse( ClientResponse response, string IEntity )
		 {
			  _status = response.Status;
			  _metaData = ExtractMetaData( response );
			  _headers = ExtractHeaders( response );
			  _location = response.Location;
			  _type = response.Type;
			  _data = IEntity;
			  response.close();
		 }

		 private static string ExtractContent( ClientResponse response )
		 {
			  if ( response.Status == Status.NO_CONTENT.StatusCode )
			  {
					return null;
			  }
			  return response.getEntity( typeof( string ) );
		 }

		 public static JaxRsResponse ExtractFrom( ClientResponse clientResponse )
		 {
			  return new JaxRsResponse( clientResponse );
		 }

		 public override string IEntity
		 {
			 get
			 {
				  return _data;
			 }
		 }

		 public override int Status
		 {
			 get
			 {
				  return _status;
			 }
		 }

		 public override MultivaluedMap<string, object> Metadata
		 {
			 get
			 {
				  return _metaData;
			 }
		 }

		 private MultivaluedMap<string, object> ExtractMetaData( ClientResponse jettyResponse )
		 {
			  MultivaluedMap<string, object> metadata = new StringKeyObjectValueIgnoreCaseMultivaluedMap();
			  foreach ( KeyValuePair<string, IList<string>> header in jettyResponse.Headers.entrySet() )
			  {
					foreach ( object value in header.Value )
					{
						 metadata.putSingle( header.Key, value );
					}
			  }
			  return metadata;
		 }

		 public virtual MultivaluedMap<string, string> Headers
		 {
			 get
			 {
				  return _headers;
			 }
		 }

		 private MultivaluedMap<string, string> ExtractHeaders( ClientResponse jettyResponse )
		 {
			  return jettyResponse.Headers;
		 }

		 // new URI( getHeaders().get( HttpHeaders.LOCATION ).get(0));
		 public virtual URI Location
		 {
			 get
			 {
				  return _location;
			 }
		 }

		 public virtual void Close()
		 {

		 }

		 public virtual MediaType Type
		 {
			 get
			 {
				  return _type;
			 }
		 }

		 public class ServerRootRepresentationTest
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideAListOfServiceUris() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldProvideAListOfServiceUris()
			  {
					ConsoleService consoleService = new ConsoleService( null, mock( typeof( Database ) ), NullLogProvider.Instance, null );
					ServerRootRepresentation srr = new ServerRootRepresentation( new URI( "http://example.org:9999" ), Collections.singletonList( consoleService ) );
					IDictionary<string, IDictionary<string, string>> map = srr.Serialize();

					assertNotNull( map["services"] );

					assertThat( map["services"][consoleService.Name], containsString( consoleService.ServerPath ) );
			  }
		 }
	}

}
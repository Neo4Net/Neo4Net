using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Server.rest.security
{
	using JsonNode = org.codehaus.jackson.JsonNode;


	using Org.Neo4j.Graphdb;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	internal class RESTInteraction : AbstractRESTInteraction
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RESTInteraction(java.util.Map<String,String> config) throws java.io.IOException
		 internal RESTInteraction( IDictionary<string, string> config ) : base( config )
		 {
		 }

		 internal override string CommitPath()
		 {
			  return "db/data/transaction/commit";
		 }

		 internal override HTTP.RawPayload ConstructQuery( string query )
		 {
			  return quotedJson( "{'statements':[{'statement':'" + query.Replace( "'", "\\'" ).Replace( "\"", "\\\"" ) + "'}]}" );
		 }

		 internal override void Consume( System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer, JsonNode data )
		 {
			  if ( data.has( "results" ) && data.get( "results" ).has( 0 ) )
			  {
					resultConsumer( new RESTResult( this, data.get( "results" ).get( 0 ) ) );
			  }
		 }

		 protected internal override HTTP.Response Authenticate( string principalCredentials )
		 {
			  return HTTP.withHeaders( HttpHeaders.AUTHORIZATION, principalCredentials ).request( POST, CommitURL() );
		 }

		 private class RESTResult : AbstractRESTResult
		 {
			 private readonly RESTInteraction _outerInstance;

			  internal RESTResult( RESTInteraction outerInstance, JsonNode fullResult ) : base( outerInstance, fullResult )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override JsonNode GetRow( JsonNode data, int i )
			  {
					return data.get( i ).get( "row" );
			  }
		 }
	}

}
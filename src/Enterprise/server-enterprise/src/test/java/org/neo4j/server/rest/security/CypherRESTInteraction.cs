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
namespace Neo4Net.Server.rest.security
{
	using JsonNode = org.codehaus.jackson.JsonNode;


	using Neo4Net.Graphdb;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	internal class CypherRESTInteraction : AbstractRESTInteraction
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CypherRESTInteraction(java.util.Map<String,String> config) throws java.io.IOException
		 internal CypherRESTInteraction( IDictionary<string, string> config ) : base( config )
		 {
		 }

		 internal override string CommitPath()
		 {
			  return "db/data/cypher";
		 }

		 internal override HTTP.RawPayload ConstructQuery( string query )
		 {
			  return quotedJson( " { 'query': '" + query.Replace( "'", "\\'" ).Replace( "\"", "\\\"" ) + "' }" );
		 }

		 internal override void Consume( System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer, JsonNode data )
		 {
			  if ( data.has( "data" ) && data.get( "data" ).has( 0 ) )
			  {
					resultConsumer( new CypherRESTResult( this, data ) );
			  }
		 }

		 protected internal override HTTP.Response Authenticate( string principalCredentials )
		 {
			  return HTTP.withHeaders( HttpHeaders.AUTHORIZATION, principalCredentials ).request( POST, CommitURL(), ConstructQuery("RETURN 1") );
		 }

		 private class CypherRESTResult : AbstractRESTResult
		 {
			 private readonly CypherRESTInteraction _outerInstance;

			  internal CypherRESTResult( CypherRESTInteraction outerInstance, JsonNode fullResult ) : base( outerInstance, fullResult )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override JsonNode GetRow( JsonNode data, int i )
			  {
					return data.get( i );
			  }
		 }
	}

}
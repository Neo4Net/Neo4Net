using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server.rest
{
	using Test = org.junit.Test;

	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using ResponseEntity = Org.Neo4j.Server.rest.RESTRequestGenerator.ResponseEntity;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using StreamingFormat = Org.Neo4j.Server.rest.repr.StreamingFormat;
	using Graph = Org.Neo4j.Test.GraphDescription.Graph;
	using Title = Org.Neo4j.Test.TestData.Title;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class GetOnRootIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Title("Get service root") @Documented("The service root is your starting point to discover the REST API. It contains the basic starting " + "points for the database, and some version and extension information.") @Test @Graph("I know you") public void assert200OkFromGet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Get service root"), Documented("The service root is your starting point to discover the REST API. It contains the basic starting " + "points for the database, and some version and extension information."), Graph("I know you")]
		 public virtual void Assert200OkFromGet()
		 {
			  string body = GenConflict.get().expectedStatus(200).get(DataUri).entity();
			  IDictionary<string, object> map = JsonHelper.jsonToMap( body );
			  assertEquals( DataUri + "node", map["node"] );
			  assertNotNull( map["node_index"] );
			  assertNotNull( map["relationship_index"] );
			  assertNotNull( map["extensions_info"] );
			  assertNotNull( map["batch"] );
			  assertNotNull( map["cypher"] );
			  assertNotNull( map["indexes"] );
			  assertNotNull( map["constraints"] );
			  assertNotNull( map["node_labels"] );
			  assertEquals( Version.Neo4jVersion, map["neo4j_version"] );

			  // Make sure advertised urls work
			  JaxRsResponse response;
			  if ( map["reference_node"] != null )
			  {
					response = RestRequest.Req().get((string) map["reference_node"]);
					assertEquals( 200, response.Status );
					response.Close();
			  }
			  response = RestRequest.Req().get((string) map["node_index"]);
			  assertTrue( response.Status == 200 || response.Status == 204 );
			  response.Close();

			  response = RestRequest.Req().get((string) map["relationship_index"]);
			  assertTrue( response.Status == 200 || response.Status == 204 );
			  response.Close();

			  response = RestRequest.Req().get((string) map["extensions_info"]);
			  assertEquals( 200, response.Status );
			  response.Close();

			  response = RestRequest.Req().post((string) map["batch"], "[]");
			  assertEquals( 200, response.Status );
			  response.Close();

			  response = RestRequest.Req().post((string) map["cypher"], "{\"query\":\"CREATE (n) RETURN n\"}");
			  assertEquals( 200, response.Status );
			  response.Close();

			  response = RestRequest.Req().get((string) map["indexes"]);
			  assertEquals( 200, response.Status );
			  response.Close();

			  response = RestRequest.Req().get((string) map["constraints"]);
			  assertEquals( 200, response.Status );
			  response.Close();

			  response = RestRequest.Req().get((string) map["node_labels"]);
			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("All responses from the REST API can be transmitted as JSON streams, resulting in\n" + "better performance and lower memory overhead on the server side. To use\n" + "streaming, supply the header `X-Stream: true` with each request.") @Test public void streaming() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("All responses from the REST API can be transmitted as JSON streams, resulting in\n" + "better performance and lower memory overhead on the server side. To use\n" + "streaming, supply the header `X-Stream: true` with each request.")]
		 public virtual void Streaming()
		 {
			  Data.get();
			  ResponseEntity responseEntity = Gen().withHeader(Org.Neo4j.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").expectedType(APPLICATION_JSON_TYPE).expectedStatus(200).get(DataUri);
			  JaxRsResponse response = responseEntity.Response();
			  // this gets the full media type, including things like
			  // ; stream=true at the end
			  string foundMediaType = response.Type.ToString();
			  string expectedMediaType = Org.Neo4j.Server.rest.repr.StreamingFormat_Fields.MediaType.ToString();
			  assertEquals( expectedMediaType, foundMediaType );

			  string body = responseEntity.Entity();
			  IDictionary<string, object> map = JsonHelper.jsonToMap( body );
			  assertEquals( DataUri + "node", map["node"] );
		 }
	}

}
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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using GeometryType = Neo4Net.Kernel.impl.store.GeometryType;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;
	using Response = Neo4Net.Test.server.HTTP.Response;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.graphContainsDeletedNodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.graphContainsDeletedRelationships;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.graphContainsNoDeletedEntities;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.restContainsDeletedEntities;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.restContainsNoDeletedEntities;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsDeletedEntities;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsDeletedEntitiesInPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsNoDeletedEntities;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.RawPayload.quotedJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.pointValue;

	public class QueryResultsSerializationTest : AbstractRestFunctionalTestBase
	{
		 private readonly HTTP.Builder _http = HTTP.withBaseUri( Server().baseUri() );

		 private string _commitResource;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );
			  try
			  {
					_commitResource = begin.StringFromContent( "commit" );
			  }
			  catch ( JsonParseException e )
			  {
					fail( "Exception caught when setting up test: " + e.Message );
			  }
			  assertThat( _commitResource, equalTo( begin.Location() + "/commit" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  // empty the database
			  Graphdb().execute("MATCH (n) DETACH DELETE n");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedEntitiesGraph()
		 public virtual void ShouldBeAbleToReturnDeletedEntitiesGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (s:Start)-[r:R]->(e:End) DELETE s, r, e RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsDeletedRelationships( 1 ) );
			  assertThat( commit, graphContainsDeletedNodes( 2 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedEntitiesRest()
		 public virtual void ShouldBeAbleToReturnDeletedEntitiesRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (s:Start)-[r:R]->(e:End) DELETE s, r, e RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, restContainsDeletedEntities( 3 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedEntitiesRow()
		 public virtual void ShouldBeAbleToReturnDeletedEntitiesRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (s:Start)-[r:R]->(e:End) DELETE s, r, e RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsDeletedEntities( 2, 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMarkNormalEntitiesAsDeletedGraph()
		 public virtual void ShouldNotMarkNormalEntitiesAsDeletedGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (s:Start)-[r:R]->(e:End) RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsNoDeletedEntities() );
			  assertThat( commit.Status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMarkNormalEntitiesAsDeletedRow()
		 public virtual void ShouldNotMarkNormalEntitiesAsDeletedRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (s:Start)-[r:R]->(e:End) RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsNoDeletedEntities() );
			  assertThat( commit.Status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMarkNormalEntitiesAsDeletedRest()
		 public virtual void ShouldNotMarkNormalEntitiesAsDeletedRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (s:Start)-[r:R]->(e:End) RETURN *" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, restContainsNoDeletedEntities() );
			  assertThat( commit.Status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedNodesGraph()
		 public virtual void ShouldBeAbleToReturnDeletedNodesGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (n:NodeToDelete) DELETE n RETURN n" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsDeletedNodes( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedNodesRow()
		 public virtual void ShouldBeAbleToReturnDeletedNodesRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (n:NodeToDelete) DELETE n RETURN n" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsDeletedEntities( 1, 0 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedNodesRest()
		 public virtual void ShouldBeAbleToReturnDeletedNodesRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (n:NodeToDelete) DELETE n RETURN n" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, restContainsDeletedEntities( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedRelationshipsGraph()
		 public virtual void ShouldBeAbleToReturnDeletedRelationshipsGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (s)-[r:R]->(e) DELETE r RETURN r" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsDeletedRelationships( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedRelationshipsRow()
		 public virtual void ShouldBeAbleToReturnDeletedRelationshipsRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (s)-[r:R]->(e) DELETE r RETURN r" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsDeletedEntities( 0, 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnDeletedRelationshipsRest()
		 public virtual void ShouldBeAbleToReturnDeletedRelationshipsRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (s)-[r:R]->(e) DELETE r RETURN r" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, restContainsDeletedEntities( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedNodeGraph()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedNodeGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (n:NodeToDelete) DELETE n RETURN n.p" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedNodeRow()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedNodeRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (n:NodeToDelete) DELETE n RETURN n.p" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedNodeRest()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedNodeRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete {p: 'a property'})");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (n:NodeToDelete) DELETE n RETURN n.p" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnLabelsOfDeletedNodeGraph()
		 public virtual void ShouldFailIfTryingToReturnLabelsOfDeletedNodeGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (n:NodeToDelete) DELETE n RETURN labels(n)" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnLabelsOfDeletedNodeRow()
		 public virtual void ShouldFailIfTryingToReturnLabelsOfDeletedNodeRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (n:NodeToDelete) DELETE n RETURN labels(n)" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnLabelsOfDeletedNodeRest()
		 public virtual void ShouldFailIfTryingToReturnLabelsOfDeletedNodeRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:NodeToDelete)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (n:NodeToDelete) DELETE n RETURN labels(n)" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedRelationshipGraph()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedRelationshipGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (s)-[r:R]->(e) DELETE r RETURN r.p" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedRelationshipRow()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedRelationshipRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (s)-[r:R]->(e) DELETE r RETURN r.p" ) );

			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTryingToReturnPropsOfDeletedRelationshipRest()
		 public virtual void ShouldFailIfTryingToReturnPropsOfDeletedRelationshipRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:MARKER {p: 'a property'}]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (s)-[r:MARKER]->(e) DELETE r RETURN r.p" ) );

			  assertThat( "Error raw response: " + commit.RawContent(), commit, hasErrors(Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningADeletedPathGraph()
		 public virtual void ReturningADeletedPathGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH p=(s)-[r:R]->(e) DELETE p RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsDeletedNodes( 2 ) );
			  assertThat( commit, graphContainsDeletedRelationships( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningAPartiallyDeletedPathGraph()
		 public virtual void ReturningAPartiallyDeletedPathGraph()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH p=(s)-[r:R]->(e) DELETE s,r RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, graphContainsDeletedNodes( 1 ) );
			  assertThat( commit, graphContainsDeletedRelationships( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningADeletedPathRow()
		 public virtual void ReturningADeletedPathRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH p=(s)-[r:R]->(e) DELETE p RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsDeletedEntitiesInPath( 2, 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningAPartiallyDeletedPathRow()
		 public virtual void ReturningAPartiallyDeletedPathRow()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH p=(s)-[r:R]->(e) DELETE s,r RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsDeletedEntitiesInPath( 1, 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningADeletedPathRest()
		 public virtual void ReturningADeletedPathRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH p=(s)-[r:R]->(e) DELETE p RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returningAPartiallyDeletedPathRest()
		 public virtual void ReturningAPartiallyDeletedPathRest()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH p=(s)-[r:R]->(e) DELETE s,r RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( NodesInDatabase(), equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nestedShouldWorkGraph()
		 public virtual void NestedShouldWorkGraph()
		 {
			  // given
			  Graphdb().execute("CREATE ()");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonGraph( "MATCH (n) DELETE (n) RETURN [n, {someKey: n}]" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( commit, graphContainsDeletedNodes( 1 ) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nestedShouldWorkRest()
		 public virtual void NestedShouldWorkRest()
		 {
			  // given
			  Graphdb().execute("CREATE ()");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRest( "MATCH (n) DELETE (n) RETURN [n, {someKey: n}]" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( commit, RestContainsNestedDeleted() );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nestedShouldWorkRow()
		 public virtual void NestedShouldWorkRow()
		 {
			  // given
			  Graphdb().execute("CREATE ()");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (n) DELETE (n) RETURN [n, {someKey: n}]" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( commit, rowContainsDeletedEntities( 2, 0 ) );
			  assertThat( NodesInDatabase(), equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTemporalArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTemporalArrays()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  ZonedDateTime date = ZonedDateTime.of( 1980, 3, 11, 0, 0, 0, 0, ZoneId.of( "Europe/Stockholm" ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "date", new ZonedDateTime[]{ date } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "row" ).get( 0 ).get( "date" ).get( 0 );

			  assertEquals( "\"1980-03-11T00:00+01:00[Europe/Stockholm]\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurationArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDurationArrays()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  Duration duration = Duration.ofSeconds( 73 );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "duration", new Duration[]{ duration } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "row" ).get( 0 ).get( "duration" ).get( 0 );

			  assertEquals( "\"PT1M13S\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTemporalUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTemporalUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  ZonedDateTime date = ZonedDateTime.of( 1980, 3, 11, 0, 0, 0, 0, ZoneId.of( "Europe/Stockholm" ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "date", date );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "date" );
			  assertEquals( "\"1980-03-11T00:00+01:00[Europe/Stockholm]\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurationUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDurationUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  Duration duration = Duration.ofSeconds( 73 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "duration", duration );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "duration" );
			  assertEquals( "\"PT1M13S\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTemporalArraysUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTemporalArraysUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  ZonedDateTime date = ZonedDateTime.of( 1980, 3, 11, 0, 0, 0, 0, ZoneId.of( "Europe/Stockholm" ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "dates", new ZonedDateTime[]{ date } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "dates" ).get( 0 );
			  assertEquals( "\"1980-03-11T00:00+01:00[Europe/Stockholm]\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurationArraysUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDurationArraysUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  Duration duration = Duration.ofSeconds( 73 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "durations", new Duration[]{ duration } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "durations" ).get( 0 );
			  assertEquals( "\"PT1M13S\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTemporalUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTemporalUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  ZonedDateTime date = ZonedDateTime.of( 1980, 3, 11, 0, 0, 0, 0, ZoneId.of( "Europe/Stockholm" ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "date", date );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "date" );
			  assertEquals( "\"1980-03-11T00:00+01:00[Europe/Stockholm]\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurationUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDurationUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  Duration duration = Duration.ofSeconds( 73 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "duration", duration );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "duration" );
			  assertEquals( "\"PT1M13S\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTemporalArraysUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTemporalArraysUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  ZonedDateTime date = ZonedDateTime.of( 1980, 3, 11, 0, 0, 0, 0, ZoneId.of( "Europe/Stockholm" ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "dates", new ZonedDateTime[]{ date } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "dates" ).get( 0 );
			  assertEquals( "\"1980-03-11T00:00+01:00[Europe/Stockholm]\"", row.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurationArraysUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDurationArraysUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  Duration duration = Duration.ofSeconds( 73 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "durations", new Duration[]{ duration } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  //Then
			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "durations" ).get( 0 );
			  assertEquals( "\"PT1M13S\"", row.ToString() );
		 }

		 private HTTP.RawPayload QueryAsJsonGraph( string query )
		 {
			  return quotedJson( "{ 'statements': [ { 'statement': '" + query + "', 'resultDataContents': [ 'graph' ] } ] }" );
		 }

		 private HTTP.RawPayload QueryAsJsonRest( string query )
		 {
			  return quotedJson( "{ 'statements': [ { 'statement': '" + query + "', 'resultDataContents': [ 'rest' ] } ] }" );
		 }

		 private HTTP.RawPayload QueryAsJsonRow( string query )
		 {
			  return quotedJson( "{ 'statements': [ { 'statement': '" + query + "', 'resultDataContents': [ 'row' ] } ] }" );
		 }

		 private long NodesInDatabase()
		 {
			  Result r = Graphdb().execute("MATCH (n) RETURN count(n) AS c");
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  long? nodes = ( long? ) r.ColumnAs( "c" ).next();
			  r.Close();
			  return nodes.Value;
		 }

		 private void AssertHasTxLocation( HTTP.Response begin )
		 {
			  assertThat( begin.Location(), matches("http://localhost:\\d+/db/data/transaction/\\d+") );
		 }

		 /// <summary>
		 /// This matcher is hardcoded to check for a list containing one deleted node and one map with a
		 /// deleted node mapped to the key `someKey`.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private static org.hamcrest.Matcher<? super Neo4Net.test.server.HTTP.Response> restContainsNestedDeleted()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static Matcher<object> RestContainsNestedDeleted()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<HTTP.Response>
		 {
			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						JsonNode list = TransactionMatchers.GetJsonNodeWithName( response, "rest" ).GetEnumerator().next();

						assertThat( list.get( 0 ).get( "metadata" ).get( "deleted" ).asBoolean(), equalTo(true) );
						assertThat( list.get( 1 ).get( "someKey" ).get( "metadata" ).get( "deleted" ).asBoolean(), equalTo(true) );

						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }
	}

}
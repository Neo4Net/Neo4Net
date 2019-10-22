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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;
	using Response = Neo4Net.Test.server.HTTP.Response;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.integration.TransactionMatchers.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsAMetaListAtIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsMetaNodesAtIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.integration.TransactionMatchers.rowContainsMetaRelsAtIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class RowFormatMetaFieldTestIT : AbstractRestFunctionalTestBase
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
//ORIGINAL LINE: @Test public void metaFieldShouldGetCorrectIndex()
		 public virtual void MetaFieldShouldGetCorrectIndex()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH (s:Start)-[r:R]->(e:End) RETURN s, r, 1, e" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsMetaNodesAtIndex( 0, 3 ) );
			  assertThat( commit, rowContainsMetaRelsAtIndex( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void metaFieldShouldGivePathInfoInList()
		 public virtual void MetaFieldShouldGivePathInfoInList()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH p=(s)-[r:R]->(e) RETURN p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsAMetaListAtIndex( 0 ) );
			  assertThat( commit.Status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void metaFieldShouldPutPathListAtCorrectIndex()
		 public virtual void MetaFieldShouldPutPathListAtCorrectIndex()
		 {
			  // given
			  Graphdb().execute("CREATE (:Start)-[:R]->(:End)");

			  // execute and commit
			  HTTP.Response commit = _http.POST( _commitResource, QueryAsJsonRow( "MATCH p=(s)-[r:R]->(e) RETURN 10, p" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit, rowContainsAMetaListAtIndex( 1 ) );
			  assertThat( commit.Status(), equalTo(200) );
		 }

		 private HTTP.RawPayload QueryAsJsonRow( string query )
		 {
			  return quotedJson( "{ 'statements': [ { 'statement': '" + query + "', 'resultDataContents': [ 'row' ] } ] }" );
		 }

		 private void AssertHasTxLocation( HTTP.Response begin )
		 {
			  assertThat( begin.Location(), matches("http://localhost:\\d+/db/data/transaction/\\d+") );
		 }
	}

}
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
namespace Org.Neo4j.Server.rest.transactional.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Org.Neo4j.Server.database.Database;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class SnapshotQueryExecutionIT : ExclusiveServerTestBase
	{

		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _server = serverOnRandomPorts().withProperty(GraphDatabaseSettings.snapshot_query.name(), Settings.TRUE).build();
			  _server.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithSnapshotEngine()
		 public virtual void ExecuteQueryWithSnapshotEngine()
		 {
			  Database database = _server.Database;
			  GraphDatabaseFacade graph = database.Graph;
			  using ( Transaction transaction = graph.BeginTx() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 Node node = graph.CreateNode();
						 node.SetProperty( "a", "b" );
					}
					transaction.Success();
			  }

			  HTTP.Builder httpClientBuilder = HTTP.withBaseUri( _server.baseUri() );
			  HTTP.Response transactionStart = httpClientBuilder.Post( TransactionURI() );
			  assertThat( transactionStart.Status(), equalTo(201) );
			  HTTP.Response response = httpClientBuilder.POST( transactionStart.Location(), quotedJson("{ 'statements': [ { 'statement': 'MATCH (n) RETURN n' } ] }") );
			  assertThat( response.Status(), equalTo(200) );
		 }

		 private string TransactionURI()
		 {
			  return "db/data/transaction";
		 }
	}

}
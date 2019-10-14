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
namespace Neo4Net.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Neo4Net.Graphdb.index;
	using WriteOperationsNotAllowedException = Neo4Net.Graphdb.security.WriteOperationsNotAllowedException;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static true;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class AccessExplicitIndexReadOnlyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAndReadExplicitIndexesForReadOnlyDb() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAndReadExplicitIndexesForReadOnlyDb()
		 {
			  // given a db with some nodes and populated explicit indexes
			  string key = "key";
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes("NODE");
					Index<Relationship> relationshipIndex = Db.index().forRelationships("RELATIONSHIP");

					for ( int i = 0; i < 10; i++ )
					{
						 Node node = Db.createNode();
						 Relationship relationship = node.CreateRelationshipTo( node, MyRelTypes.TEST );
						 nodeIndex.Add( node, key, i.ToString() );
						 relationshipIndex.Add( relationship, key, i.ToString() );
					}
					tx.Success();
			  }

			  // when restarted as read-only
			  Db.restartDatabase( GraphDatabaseSettings.read_only.name(), TRUE.ToString() );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(Db.index().nodeIndexNames()[0]);
					Index<Relationship> relationshipIndex = Db.index().forRelationships(Db.index().relationshipIndexNames()[0]);

					// then try and read the indexes
					for ( int i = 0; i < 10; i++ )
					{
						 assertNotNull( nodeIndex.get( key, i.ToString() ).Single );
						 assertNotNull( relationshipIndex.get( key, i.ToString() ).Single );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateIndexesForReadOnlyDb()
		 public virtual void ShouldNotCreateIndexesForReadOnlyDb()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.layout.DatabaseLayout databaseLayout = db.databaseLayout();
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  // Make sure we have database to start on
			  Db.shutdown();
			  GraphDatabaseService db = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseLayout.DatabaseDirectory()).setConfig(GraphDatabaseSettings.read_only, TRUE.ToString()).newGraphDatabase();
			  try
			  {
					// when
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 Db.index().forNodes("NODE");
							 fail( "Should've failed" );
							}
					}
					catch ( WriteOperationsNotAllowedException )
					{
						 // then good
					}
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 Db.index().forRelationships("RELATIONSHIP");
							 fail( "Should've failed" );
							}
					}
					catch ( WriteOperationsNotAllowedException )
					{
						 // then good
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}
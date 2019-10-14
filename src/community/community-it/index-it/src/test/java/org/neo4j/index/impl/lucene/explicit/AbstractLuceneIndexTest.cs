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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using TestName = org.junit.rules.TestName;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using RelationshipIndex = Neo4Net.Graphdb.index.RelationshipIndex;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public abstract class AbstractLuceneIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testname = new org.junit.rules.TestName();
		 public readonly TestName Testname = new TestName();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory(AbstractLuceneIndexTest.class);
		 public static TestDirectory TestDirectory = TestDirectory.testDirectory( typeof( AbstractLuceneIndexTest ) );
		 protected internal static GraphDatabaseService GraphDb;
		 protected internal Transaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpStuff()
		 public static void SetUpStuff()
		 {
			  GraphDb = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownStuff()
		 public static void TearDownStuff()
		 {
			  GraphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void commitTx()
		 public virtual void CommitTx()
		 {
			  FinishTx( true );
		 }

		 public virtual void RollbackTx()
		 {
			  FinishTx( false );
		 }

		 public virtual void FinishTx( bool success )
		 {
			  if ( Tx != null )
			  {
					if ( success )
					{
						 Tx.success();
					}
					Tx.close();
					Tx = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void beginTx()
		 public virtual void BeginTx()
		 {
			  if ( Tx == null )
			  {
					Tx = GraphDb.beginTx();
			  }
		 }

		 internal virtual void RestartTx()
		 {
			  CommitTx();
			  BeginTx();
		 }

		 protected internal interface EntityCreator<T> where T : Neo4Net.Graphdb.PropertyContainer
		 {
			  T Create( params object[] properties );

			  void Delete( T entity );
		 }

		 private static readonly RelationshipType _testType = RelationshipType.withName( "TEST_TYPE" );

		 protected internal static readonly EntityCreator<Node> NODE_CREATOR = new EntityCreatorAnonymousInnerClass();

		 private class EntityCreatorAnonymousInnerClass : EntityCreator<Node>
		 {
			 public Node create( params object[] properties )
			 {
				  Node node = GraphDb.createNode();
				  SetProperties( node, properties );
				  return node;
			 }

			 public void delete( Node entity )
			 {
				  entity.Delete();
			 }
		 }
		 protected internal static readonly EntityCreator<Relationship> RELATIONSHIP_CREATOR = new EntityCreatorAnonymousInnerClass2();

		 private class EntityCreatorAnonymousInnerClass2 : EntityCreator<Relationship>
		 {
			 public Relationship create( params object[] properties )
			 {
				  Relationship rel = GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), _testType);
				  SetProperties( rel, properties );
				  return rel;
			 }

			 public void delete( Relationship entity )
			 {
				  entity.Delete();
			 }
		 }

		 private static void SetProperties( PropertyContainer entity, params object[] properties )
		 {
			  foreach ( KeyValuePair<string, object> entry in MapUtil.map( properties ).SetOfKeyValuePairs() )
			  {
					entity.SetProperty( entry.Key, entry.Value );
			  }
		 }

		 protected internal virtual Index<Node> NodeIndex()
		 {
			  return NodeIndex( CurrentIndexName(), stringMap() );
		 }

		 protected internal virtual Index<Node> NodeIndex( IDictionary<string, string> config )
		 {
			  return NodeIndex( CurrentIndexName(), config );
		 }

		 protected internal virtual Index<Node> NodeIndex( string name, IDictionary<string, string> config )
		 {
			  return GraphDb.index().forNodes(name, config);
		 }

		 protected internal virtual RelationshipIndex RelationshipIndex( IDictionary<string, string> config )
		 {
			  return RelationshipIndex( CurrentIndexName(), config );
		 }

		 protected internal virtual RelationshipIndex RelationshipIndex( string name, IDictionary<string, string> config )
		 {
			  return GraphDb.index().forRelationships(name, config);
		 }

		 protected internal virtual string CurrentIndexName()
		 {
			  return Testname.MethodName;
		 }
	}

}
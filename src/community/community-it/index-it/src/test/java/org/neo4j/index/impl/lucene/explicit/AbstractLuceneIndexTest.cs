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

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public abstract class AbstractLuceneIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testname = new org.junit.rules.TestName();
		 public readonly TestName Testname = new TestName();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory(AbstractLuceneIndexTest.class);
		 public static TestDirectory TestDirectory = TestDirectory.testDirectory( typeof( AbstractLuceneIndexTest ) );
		 protected internal static IGraphDatabaseService GraphDb;
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

		 protected internal interface IEntityCreator<T> where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  T Create( params object[] properties );

			  void Delete( T IEntity );
		 }

		 private static readonly RelationshipType _testType = RelationshipType.withName( "TEST_TYPE" );

		 protected internal static readonly IEntityCreator<Node> NODE_CREATOR = new IEntityCreatorAnonymousInnerClass();

		 private class IEntityCreatorAnonymousInnerClass : IEntityCreator<Node>
		 {
			 public Node create( params object[] properties )
			 {
				  Node node = GraphDb.createNode();
				  SetProperties( node, properties );
				  return node;
			 }

			 public void delete( Node IEntity )
			 {
				  IEntity.Delete();
			 }
		 }
		 protected internal static readonly IEntityCreator<Relationship> RELATIONSHIP_CREATOR = new IEntityCreatorAnonymousInnerClass2();

		 private class IEntityCreatorAnonymousInnerClass2 : IEntityCreator<Relationship>
		 {
			 public Relationship create( params object[] properties )
			 {
				  Relationship rel = GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), _testType);
				  SetProperties( rel, properties );
				  return rel;
			 }

			 public void delete( Relationship IEntity )
			 {
				  IEntity.Delete();
			 }
		 }

		 private static void SetProperties( IPropertyContainer IEntity, params object[] properties )
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
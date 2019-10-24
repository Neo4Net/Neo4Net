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
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.GraphDb.Index;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Neo4NetTestCase.assertContains;

	public class TestIndexDeletion
	{
		 private const string INDEX_NAME = "index";
		 private static IGraphDatabaseService _graphDb;
		 private Index<Node> _index;
		 private Transaction _tx;
		 private string _key;
		 private Node _node;
		 private string _value;
		 private IList<WorkThread> _workers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpStuff()
		 public static void SetUpStuff()
		 {
			  _graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownStuff()
		 public static void TearDownStuff()
		 {
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void commitTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommitTx()
		 {
			  FinishTx( true );
			  foreach ( WorkThread worker in _workers )
			  {
					worker.Rollback();
					worker.Die();
					worker.Dispose();
			  }
		 }

		 public virtual void RollbackTx()
		 {
			  FinishTx( false );
		 }

		 public virtual void FinishTx( bool success )
		 {
			  if ( _tx != null )
			  {
					if ( success )
					{
						 _tx.success();
					}
					_tx.close();
					_tx = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createInitialData()
		 public virtual void CreateInitialData()
		 {
			  BeginTx();
			  _index = _graphDb.index().forNodes(INDEX_NAME);
			  _index.delete();
			  RestartTx();

			  _index = _graphDb.index().forNodes(INDEX_NAME);
			  _key = "key";

			  _value = "my own value";
			  _node = _graphDb.createNode();
			  _index.add( _node, _key, _value );
			  _workers = new List<WorkThread>();
		 }

		 public virtual void BeginTx()
		 {
			  if ( _tx == null )
			  {
					_tx = _graphDb.beginTx();
			  }
		 }

		 internal virtual void RestartTx()
		 {
			  FinishTx( true );
			  BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDeleteAndRecreateIndex()
		 public virtual void ShouldBeAbleToDeleteAndRecreateIndex()
		 {
			  RestartTx();
			  assertContains( _index.query( _key, "own" ) );
			  _index.delete();
			  RestartTx();

			  Index<Node> recreatedIndex = _graphDb.index().forNodes(INDEX_NAME, LuceneIndexImplementation.FulltextConfig);
			  assertNull( recreatedIndex.get( _key, _value ).Single );
			  recreatedIndex.Add( _node, _key, _value );
			  RestartTx();
			  assertContains( recreatedIndex.query( _key, "own" ), _node );
			  recreatedIndex.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeDeletedWhenDeletionRolledBack()
		 public virtual void ShouldNotBeDeletedWhenDeletionRolledBack()
		 {
			  RestartTx();
			  _index.delete();
			  RollbackTx();
			  BeginTx();
			  using ( IndexHits<Node> indexHits = _index.get( _key, _value ) )
			  {
					//empty
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateForActionsAfterDeletedOnIndex()
		 public virtual void ShouldThrowIllegalStateForActionsAfterDeletedOnIndex()
		 {
			  RestartTx();
			  _index.delete();
			  RestartTx();
			  try
			  {
					_index.query( _key, "own" );
					fail( "Should fail" );
			  }
			  catch ( NotFoundException e )
			  {
					assertThat( e.Message, containsString( "doesn't exist" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateForActionsAfterDeletedOnIndex2()
		 public virtual void ShouldThrowIllegalStateForActionsAfterDeletedOnIndex2()
		 {
			  RestartTx();
			  _index.delete();
			  RestartTx();
			  try
			  {
					_index.add( _node, _key, _value );
					fail();
			  }
			  catch ( NotFoundException e )
			  {
					assertThat( e.Message, containsString( "doesn't exist" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIllegalStateForActionsAfterDeletedOnIndex3()
		 public virtual void ShouldThrowIllegalStateForActionsAfterDeletedOnIndex3()
		 {
			  RestartTx();
			  _index.delete();
			  _index.query( _key, "own" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIllegalStateForActionsAfterDeletedOnIndex4()
		 public virtual void ShouldThrowIllegalStateForActionsAfterDeletedOnIndex4()
		 {
			  RestartTx();
			  _index.delete();
			  Index<Node> newIndex = _graphDb.index().forNodes(INDEX_NAME);
			  newIndex.query( _key, "own" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteInOneTxShouldNotAffectTheOther() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteInOneTxShouldNotAffectTheOther()
		 {
			  _index.delete();

			  WorkThread firstTx = CreateWorker( "Single" );
			  firstTx.BeginTransaction();
			  firstTx.CreateNodeAndIndexBy( _key, "another value" );
			  firstTx.Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteAndCommitShouldBePublishedToOtherTransaction2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteAndCommitShouldBePublishedToOtherTransaction2()
		 {
			  WorkThread firstTx = CreateWorker( "First" );
			  WorkThread secondTx = CreateWorker( "Second" );

			  firstTx.BeginTransaction();
			  secondTx.BeginTransaction();

			  firstTx.CreateNodeAndIndexBy( _key, "some value" );
			  secondTx.CreateNodeAndIndexBy( _key, "some other value" );

			  firstTx.DeleteIndex();
			  firstTx.Commit();

			  try
			  {
					secondTx.QueryIndex( _key, "some other value" );
					fail( "Should throw exception" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( NotFoundException ) ) );
					assertThat( e.InnerException.Message.ToLower(), containsString("index 'index' doesn't exist") );
			  }

			  secondTx.Rollback();

			  // Since $Before will start a tx, add a value and keep tx open and
			  // workers will delete the index so this test will fail in @After
			  // if we don't rollback this tx
			  RollbackTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexDeletesShouldNotByVisibleUntilCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexDeletesShouldNotByVisibleUntilCommit()
		 {
			  CommitTx();

			  WorkThread firstTx = CreateWorker( "First" );

			  firstTx.BeginTransaction();
			  firstTx.RemoveFromIndex( _key, _value );

			  using ( Transaction transaction = _graphDb.beginTx() )
			  {
					IndexHits<Node> indexHits = _index.get( _key, _value );
					assertThat( indexHits, Contains.ContainsConflict( _node ) );
			  }

			  firstTx.Rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canDeleteIndexEvenIfEntitiesAreFoundToBeAbandonedInTheSameTx()
		 public virtual void CanDeleteIndexEvenIfEntitiesAreFoundToBeAbandonedInTheSameTx()
		 {
			  // create and index a node
			  Index<Node> nodeIndex = _graphDb.index().forNodes("index");
			  Node node = _graphDb.createNode();
			  nodeIndex.Add( node, "key", "value" );
			  // make sure to commit the creation of the entry
			  RestartTx();

			  // delete the node to abandon the index entry
			  node.Delete();
			  RestartTx();

			  // iterate over all nodes indexed with the key to discover abandoned
			  foreach ( Node ignore in nodeIndex.get( "key", "value" ) )
			  {
			  }

			  nodeIndex.Delete();
			  RestartTx();
		 }

		 private WorkThread CreateWorker( string name )
		 {
			  WorkThread workThread = new WorkThread( name, _index, _graphDb, _node );
			  _workers.Add( workThread );
			  return workThread;
		 }
	}

}
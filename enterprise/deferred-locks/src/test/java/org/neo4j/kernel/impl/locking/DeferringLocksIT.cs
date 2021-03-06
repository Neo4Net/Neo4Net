﻿/*
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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using InvalidRecordException = Org.Neo4j.Kernel.impl.store.InvalidRecordException;
	using Barrier = Org.Neo4j.Test.Barrier;
	using Org.Neo4j.Test.OtherThreadExecutor;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Org.Neo4j.Test.rule.EnterpriseDatabaseRule;
	using Org.Neo4j.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	public class DeferringLocksIT
	{
		 private const long TEST_TIMEOUT = 30_000;

		 private static readonly Label _label = Label.label( "label" );
		 private const string PROPERTY_KEY = "key";
		 private const string VALUE_1 = "value1";
		 private const string VALUE_2 = "value2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.EnterpriseDatabaseRule().startLazily();
		 public readonly DatabaseRule DbRule = new EnterpriseDatabaseRule().startLazily();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t3 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> T3 = new OtherThreadRule<Void>();

		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void initDb()
		 public virtual void InitDb()
		 {
			  DbRule.withSetting( DeferringStatementLocksFactory.DeferredLocksEnabled, Settings.TRUE );
			  _db = DbRule.GraphDatabaseAPI;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void shouldNotFreakOutIfTwoTransactionsDecideToEachAddTheSameProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFreakOutIfTwoTransactionsDecideToEachAddTheSameProperty()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
			  Org.Neo4j.Test.Barrier_Control barrier = new Org.Neo4j.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node;
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					tx.Success();
			  }

			  // WHEN
			  T2.execute((WorkerCommand<Void, Void>) state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 node.SetProperty( PROPERTY_KEY, VALUE_1 );
					 tx.Success();
					 barrier.Reached();
				}
				return null;
			  });
			  using ( Transaction tx = _db.beginTx() )
			  {
					barrier.Await();
					node.SetProperty( PROPERTY_KEY, VALUE_2 );
					tx.Success();
					barrier.Release();
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( 1, count( node.PropertyKeys ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void firstRemoveSecondChangeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FirstRemoveSecondChangeProperty()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
			  Org.Neo4j.Test.Barrier_Control barrier = new Org.Neo4j.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node;
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					node.SetProperty( PROPERTY_KEY, VALUE_1 );
					tx.Success();
			  }

			  // WHEN
			  Future<Void> future = T2.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 node.RemoveProperty( PROPERTY_KEY );
					 tx.Success();
					 barrier.Reached();
				}
				return null;
			  });
			  using ( Transaction tx = _db.beginTx() )
			  {
					barrier.Await();
					node.SetProperty( PROPERTY_KEY, VALUE_2 );
					tx.Success();
					barrier.Release();
			  }

			  future.get();
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( VALUE_2, node.GetProperty( PROPERTY_KEY, VALUE_2 ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void removeNodeChangeNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RemoveNodeChangeNodeProperty()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
			  Org.Neo4j.Test.Barrier_Control barrier = new Org.Neo4j.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId;
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					nodeId = node.Id;
					node.SetProperty( PROPERTY_KEY, VALUE_1 );
					tx.Success();
			  }

			  // WHEN
			  Future<Void> future = T2.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 _db.getNodeById( nodeId ).delete();
					 tx.Success();
					 barrier.Reached();
				}
				return null;
			  });
			  try
			  {
					using ( Transaction tx = _db.beginTx() )
					{
						 barrier.Await();
						 _db.getNodeById( nodeId ).setProperty( PROPERTY_KEY, VALUE_2 );
						 tx.Success();
						 barrier.Release();
					}
			  }
			  catch ( TransactionFailureException e )
			  {
					// Node was already deleted, fine.
					assertThat( e.InnerException, instanceOf( typeof( InvalidRecordException ) ) );
			  }

			  future.get();
			  using ( Transaction tx = _db.beginTx() )
			  {
					try
					{
						 _db.getNodeById( nodeId );
						 assertEquals( VALUE_2, _db.getNodeById( nodeId ).getProperty( PROPERTY_KEY, VALUE_2 ) );
					}
					catch ( NotFoundException )
					{
						 // Fine, its gone
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void readOwnChangesFromRacingIndexNoBlock() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadOwnChangesFromRacingIndexNoBlock()
		 {
			  Future<Void> t2Future = T2.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 CreateNodeWithProperty( _label, PROPERTY_KEY, VALUE_1 );
					 AssertNodeWith( _label, PROPERTY_KEY, VALUE_1 );

					 tx.success();
				}
				return null;
			  });

			  Future<Void> t3Future = T3.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 CreateAndAwaitIndex( _label, PROPERTY_KEY );
					 tx.success();
				}
				return null;
			  });

			  t3Future.get();
			  t2Future.get();

			  AssertInTxNodeWith( _label, PROPERTY_KEY, VALUE_1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void readOwnChangesWithoutIndex()
		 public virtual void ReadOwnChangesWithoutIndex()
		 {
			  // WHEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( _label );
					node.SetProperty( PROPERTY_KEY, VALUE_1 );

					AssertNodeWith( _label, PROPERTY_KEY, VALUE_1 );

					tx.Success();
			  }

			  AssertInTxNodeWith( _label, PROPERTY_KEY, VALUE_1 );
		 }

		 private void AssertInTxNodeWith( Label label, string key, object value )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					AssertNodeWith( label, key, value );
					tx.Success();
			  }
		 }

		 private void AssertNodeWith( Label label, string key, object value )
		 {
			  using ( ResourceIterator<Node> nodes = _db.findNodes( label, key, value ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( nodes.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node foundNode = nodes.next();
					assertTrue( foundNode.HasLabel( label ) );
					assertEquals( value, foundNode.GetProperty( key ) );
			  }
		 }

		 private Node CreateNodeWithProperty( Label label, string key, object value )
		 {
			  Node node = _db.createNode( label );
			  node.SetProperty( key, value );
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void,Void> createAndAwaitIndex(final org.neo4j.graphdb.Label label, final String key)
		 private WorkerCommand<Void, Void> CreateAndAwaitIndex( Label label, string key )
		 {
			  return state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 _db.schema().indexFor(label).on(key).create();
					 tx.success();
				}
				using ( Transaction ignore = _db.beginTx() )
				{
					 _db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
				}
				return null;
			  };
		 }
	}

}
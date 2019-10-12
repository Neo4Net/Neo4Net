using System;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Neo4Net.Graphdb.index;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using DummyIndexExtensionFactory = Neo4Net.Kernel.impl.index.DummyIndexExtensionFactory;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class TransactionRepresentationCommitProcessIT
	{
		 private const string INDEX_NAME = "index";
		 private const int TOTAL_ACTIVE_THREADS = 6;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.check_point_interval_time, "10ms");
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.check_point_interval_time, "10ms");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 15000) public void commitDuringContinuousCheckpointing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommitDuringContinuousCheckpointing()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.index.Index<org.neo4j.graphdb.Node> index;
			  Index<Node> index;
			  using ( Transaction tx = Db.beginTx() )
			  {
					index = Db.index().forNodes(INDEX_NAME, stringMap(Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, DummyIndexExtensionFactory.IDENTIFIER));
					tx.Success();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean done = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean done = new AtomicBoolean();
			  Workers<ThreadStart> workers = new Workers<ThreadStart>( this.GetType().Name );
			  for ( int i = 0; i < TOTAL_ACTIVE_THREADS; i++ )
			  {
					workers.Start( new RunnableAnonymousInnerClass( this, index, tx, done ) );
			  }

			  Thread.Sleep( SECONDS.toMillis( 2 ) );
			  done.set( true );
			  workers.AwaitAndThrowOnError();

			  NeoStores neoStores = GetDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  assertThat( "Count store should be rotated once at least", neoStores.Counts.txId(), greaterThan(0L) );

			  long lastRotationTx = GetDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "test" ) );
			  TransactionIdStore txIdStore = GetDependency( typeof( TransactionIdStore ) );
			  assertEquals( "NeoStore last closed transaction id should be equal last count store rotation transaction id.", txIdStore.LastClosedTransactionId, lastRotationTx );
			  assertEquals( "Last closed transaction should be last rotated tx in count store", txIdStore.LastClosedTransactionId, neoStores.Counts.txId() );
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly TransactionRepresentationCommitProcessIT _outerInstance;

			 private Index<Node> _index;
			 private Transaction _tx;
			 private AtomicBoolean _done;

			 public RunnableAnonymousInnerClass( TransactionRepresentationCommitProcessIT outerInstance, Index<Node> index, Transaction tx, AtomicBoolean done )
			 {
				 this.outerInstance = outerInstance;
				 this._index = index;
				 this._tx = tx;
				 this._done = done;
				 random = ThreadLocalRandom.current();
			 }

			 private readonly ThreadLocalRandom random;

			 public void run()
			 {
				  while ( !_done.get() )
				  {
						using ( Transaction tx = _outerInstance.db.beginTx() )
						{
							 Node node = _outerInstance.db.createNode();
							 _index.add( node, "key", node.Id );
							 tx.Success();
						}
						randomSleep();
				  }
			 }

			 private void randomSleep()
			 {
				  try
				  {
						Thread.Sleep( random.Next( 50 ) );
				  }
				  catch ( InterruptedException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 private T GetDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return Db.DependencyResolver.resolveDependency( clazz );
		 }
	}

}
using System;
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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.schema.ConstraintDefinition;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using LockWrapper = Neo4Net.Kernel.impl.store.kvstore.LockWrapper;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Barrier = Neo4Net.Test.Barrier;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestLabels = Neo4Net.Test.TestLabels;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;
	using Neo4Net.Test.rule.concurrent;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.storageengine.api.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// The scenario, which takes place on database instance applying constraint
	/// creation as an external transaction, looks like this:
	/// 
	/// <ol>
	/// <li>
	/// Transaction T1 creates the constraint index and population P starts
	/// </li>
	/// <li>
	/// Transaction T2 which activates the constraint starts applying and now has a read lock on the counts store
	/// </li>
	/// <li>
	/// Check point triggers, wants to rotate counts store and so acquires its write lock.
	/// It will have to block, but doing so will also blocks further read lock requests
	/// </li>
	/// <li>
	/// T2 moves on to activate the constraint. Doing so means first waiting for the index to come online
	/// </li>
	/// <li>
	/// P moves on to flip after population, something which includes initializing some sample data in counts store
	/// for this index. Will block on the counts store read lock, completing the deadlock
	/// </li>
	/// </ol>
	/// </summary>
	public class CheckPointerConstraintCreationDeadlockIT
	{
		 private const Label LABEL = TestLabels.LABEL_ONE;
		 private const string KEY = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.VerboseTimeout timeout = org.Neo4Net.test.rule.VerboseTimeout.builder().withTimeout(30, SECONDS).build();
		 public readonly VerboseTimeout Timeout = VerboseTimeout.builder().withTimeout(30, SECONDS).build();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>("T2");
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2" );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t3 = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>("T3");
		 public readonly OtherThreadRule<Void> T3 = new OtherThreadRule<Void>( "T3" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeadlock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeadlock()
		 {
			  IList<TransactionRepresentation> transactions = CreateConstraintCreatingTransactions();
			  Monitors monitors = new Monitors();
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setMonitors(monitors).newImpermanentDatabase();
			  Neo4Net.Test.Barrier_Control controller = new Neo4Net.Test.Barrier_Control();
			  bool success = false;
			  try
			  {
					IndexingService.Monitor monitor = new MonitorAdapterAnonymousInnerClass( this, controller );
					monitors.AddMonitorListener( monitor );
					Future<object> applier = ApplyInT2( db, transactions );

					controller.Await();

					// At this point the index population has completed and the population thread is ready to
					// acquire the counts store read lock for initializing some samples there. We're starting the
					// check pointer, which will eventually put itself in queue for acquiring the write lock

					Future<object> checkPointer = T3.execute( state => Db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "MANUAL" ) ) );
					try
					{
						 T3.get().waitUntilWaiting(details => details.isAt(typeof(LockWrapper), "writeLock"));
					}
					catch ( System.InvalidOperationException )
					{
						 // Thrown when the fix is in, basically it's thrown if the check pointer didn't get blocked
						 checkPointer.get(); // to assert that no exception was thrown during in check point thread
					}

					// Alright the trap is set. Let the population thread move on and seal the deal
					controller.Release();

					// THEN these should complete
					applier.get( 10, SECONDS );
					checkPointer.get( 10, SECONDS );
					success = true;

					using ( Transaction tx = Db.beginTx() )
					{
						 ConstraintDefinition constraint = single( Db.schema().getConstraints(LABEL) );
						 assertEquals( KEY, single( constraint.PropertyKeys ) );
						 tx.Success();
					}

					CreateNode( db, "A" );
					try
					{
						 CreateNode( db, "A" );
						 fail( "Should have failed" );
					}
					catch ( ConstraintViolationException )
					{
						 // THEN good
					}
			  }
			  finally
			  {
					if ( !success )
					{
						 T2.interrupt();
						 T3.interrupt();
						 // so that shutdown won't hang too
					}
					Db.shutdown();
			  }
		 }

		 private class MonitorAdapterAnonymousInnerClass : IndexingService.MonitorAdapter
		 {
			 private readonly CheckPointerConstraintCreationDeadlockIT _outerInstance;

			 private Neo4Net.Test.Barrier_Control _controller;

			 public MonitorAdapterAnonymousInnerClass( CheckPointerConstraintCreationDeadlockIT outerInstance, Neo4Net.Test.Barrier_Control controller )
			 {
				 this.outerInstance = outerInstance;
				 this._controller = controller;
			 }

			 public override void indexPopulationScanComplete()
			 {
				  _controller.reached();
			 }
		 }

		 private void CreateNode( GraphDatabaseAPI db, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( LABEL ).setProperty( KEY, name );
					tx.Success();
			  }
		 }

		 private Future<object> ApplyInT2( GraphDatabaseAPI db, IList<TransactionRepresentation> transactions )
		 {
			  TransactionCommitProcess commitProcess = Db.DependencyResolver.resolveDependency( typeof( TransactionCommitProcess ) );
			  return T2.execute(state =>
			  {
				transactions.ForEach(tx =>
				{
					 try
					 {
						  // It will matter if the transactions are supplied all in the same batch or one by one
						  // since the CountsTracker#apply lock is held and released per transaction
						  commitProcess.Commit( new TransactionToApply( tx ), NULL, EXTERNAL );
					 }
					 catch ( TransactionFailureException e )
					 {
						  throw new Exception( e );
					 }
				});
				return null;
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<org.Neo4Net.kernel.impl.transaction.TransactionRepresentation> createConstraintCreatingTransactions() throws Exception
		 private static IList<TransactionRepresentation> CreateConstraintCreatingTransactions()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(LABEL).assertPropertyIsUnique(KEY).create();
						 tx.Success();
					}

					LogicalTransactionStore txStore = Db.DependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );
					IList<TransactionRepresentation> result = new List<TransactionRepresentation>();
					using ( TransactionCursor cursor = txStore.GetTransactions( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 1 ) )
					{
						 while ( cursor.next() )
						 {
							  result.Add( cursor.get().TransactionRepresentation );
						 }
					}
					return result;
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}
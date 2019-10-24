using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Label = Neo4Net.GraphDb.Label;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionCursor = Neo4Net.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Barrier = Neo4Net.Test.Barrier;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestLabels = Neo4Net.Test.TestLabels;
	using Neo4Net.Test.rule.concurrent;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// It's master creating a constraint. There are two mini transactions in creating a constraint:
	/// <ol>
	/// <li>Create the backing index and activating the constraint (index population follows).</li>
	/// <li>Activating the constraint after successful index population.</li>
	/// </ol>
	/// 
	/// If slave pulls the first mini transaction, but crashes or otherwise does a nonclean shutdown before it gets
	/// the other mini transaction (and that index record happens to have been evicted to disk in between)
	/// then the next start of that slave would set that constraint index as failed and even delete it
	/// and refuse to activate it when it eventually would pull the other mini transaction which wanted to
	/// activate the constraint.
	/// 
	/// This issue is tested in single db mode because it's way easier to reliably test in this environment.
	/// </summary>
	public class HalfAppliedConstraintRecoveryIT
	{
		 private const Label LABEL = TestLabels.LABEL_ONE;
		 private const string KEY = "key";
		 private const string KEY2 = "key2";
		 private static readonly System.Action<GraphDatabaseAPI> _uniqueConstraintCreator = db => Db.schema().constraintFor(LABEL).assertPropertyIsUnique(KEY).create();

		 private static readonly System.Action<GraphDatabaseAPI> _nodeKeyConstraintCreator = db => Db.execute( "CREATE CONSTRAINT ON (n:" + LABEL.Name() + ") ASSERT (n." + KEY + ") IS NODE KEY" );

		 private static readonly System.Action<GraphDatabaseAPI> _compositeNodeKeyConstraintCreator = db => Db.execute( "CREATE CONSTRAINT ON (n:" + LABEL.Name() + ") ASSERT (n." + KEY + ", n." + KEY2 + ") IS NODE KEY" );
		 private static readonly System.Action<GraphDatabaseAPI, IList<TransactionRepresentation>> _reapply = ( db, txs ) => apply( db, txs.subList( txs.size() - 1, txs.size() ) );

		 private static System.Action<GraphDatabaseAPI, IList<TransactionRepresentation>> Recreate( System.Action<GraphDatabaseAPI> constraintCreator )
		 {
			  return ( db, txs ) => createConstraint( db, constraintCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>("T2");
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2" );
		 private readonly Monitors _monitors = new Monitors();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndContinueApplyHalfConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndContinueApplyHalfConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( _reapply, _uniqueConstraintCreator, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndRecreateHalfConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndRecreateHalfConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( Recreate( _uniqueConstraintCreator ), _uniqueConstraintCreator, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndContinueApplyHalfNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndContinueApplyHalfNodeKeyConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( _reapply, _nodeKeyConstraintCreator, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndRecreateHalfNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndRecreateHalfNodeKeyConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( Recreate( _nodeKeyConstraintCreator ), _nodeKeyConstraintCreator, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndContinueApplyHalfCompositeNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndContinueApplyHalfCompositeNodeKeyConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( _reapply, _compositeNodeKeyConstraintCreator, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromAndRecreateHalfCompositeNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromAndRecreateHalfCompositeNodeKeyConstraintAppliedBeforeCrash()
		 {
			  RecoverFromHalfConstraintAppliedBeforeCrash( Recreate( _compositeNodeKeyConstraintCreator ), _compositeNodeKeyConstraintCreator, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void recoverFromHalfConstraintAppliedBeforeCrash(System.Action<org.Neo4Net.kernel.internal.GraphDatabaseAPI,java.util.List<org.Neo4Net.kernel.impl.transaction.TransactionRepresentation>> applier, System.Action<org.Neo4Net.kernel.internal.GraphDatabaseAPI> constraintCreator, boolean composite) throws Exception
		 private void RecoverFromHalfConstraintAppliedBeforeCrash( System.Action<GraphDatabaseAPI, IList<TransactionRepresentation>> applier, System.Action<GraphDatabaseAPI> constraintCreator, bool composite )
		 {
			  // GIVEN
			  IList<TransactionRepresentation> transactions = CreateTransactionsForCreatingConstraint( constraintCreator );
			  GraphDatabaseAPI db = NewDb();
			  EphemeralFileSystemAbstraction crashSnapshot;
			  try
			  {
					Apply( db, transactions.subList( 0, transactions.Count - 1 ) );
					FlushStores( db );
					crashSnapshot = Fs.snapshot();
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // WHEN
			  db = ( GraphDatabaseAPI ) ( new TestEnterpriseGraphDatabaseFactory() ).setFileSystem(crashSnapshot).newImpermanentDatabase();
			  try
			  {
					applier( db, transactions );

					// THEN
					using ( Transaction tx = Db.beginTx() )
					{
						 ConstraintDefinition constraint = single( Db.schema().getConstraints(LABEL) );
						 assertEquals( LABEL.Name(), constraint.Label.name() );
						 if ( composite )
						 {
							  assertEquals( Arrays.asList( KEY, KEY2 ), Iterables.asList( constraint.PropertyKeys ) );
						 }
						 else
						 {
							  assertEquals( KEY, single( constraint.PropertyKeys ) );
						 }
						 IndexDefinition index = single( Db.schema().getIndexes(LABEL) );
						 assertEquals( LABEL.Name(), single(index.Labels).name() );
						 if ( composite )
						 {
							  assertEquals( Arrays.asList( KEY, KEY2 ), Iterables.asList( index.PropertyKeys ) );
						 }
						 else
						 {
							  assertEquals( KEY, single( index.PropertyKeys ) );
						 }
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromNonUniqueHalfConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromNonUniqueHalfConstraintAppliedBeforeCrash()
		 {
			  // GIVEN
			  RecoverFromConstraintAppliedBeforeCrash( _uniqueConstraintCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromNonUniqueHalfNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromNonUniqueHalfNodeKeyConstraintAppliedBeforeCrash()
		 {
			  // GIVEN
			  RecoverFromConstraintAppliedBeforeCrash( _nodeKeyConstraintCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFromNonUniqueHalfCompositeNodeKeyConstraintAppliedBeforeCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFromNonUniqueHalfCompositeNodeKeyConstraintAppliedBeforeCrash()
		 {
			  // GIVEN
			  RecoverFromConstraintAppliedBeforeCrash( _compositeNodeKeyConstraintCreator );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void recoverFromConstraintAppliedBeforeCrash(System.Action<org.Neo4Net.kernel.internal.GraphDatabaseAPI> constraintCreator) throws Exception
		 private void RecoverFromConstraintAppliedBeforeCrash( System.Action<GraphDatabaseAPI> constraintCreator )
		 {
			  IList<TransactionRepresentation> transactions = CreateTransactionsForCreatingConstraint( constraintCreator );
			  EphemeralFileSystemAbstraction crashSnapshot;
			  {
					GraphDatabaseAPI db = NewDb();
					Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
					_monitors.addMonitorListener( new MonitorAdapterAnonymousInnerClass( this, barrier ) );
					try
					{
						 // Create two nodes that have duplicate property values
						 string value = "v";
						 using ( Transaction tx = Db.beginTx() )
						 {
							  for ( int i = 0; i < 2; i++ )
							  {
									Db.createNode( LABEL ).setProperty( KEY, value );
							  }
							  tx.Success();
						 }
						 T2.execute(state =>
						 {
						  Apply( db, transactions.subList( 0, transactions.Count - 1 ) );
						  return null;
						 });
						 barrier.Await();
						 FlushStores( db );
						 // Crash before index population have discovered that there are duplicates
						 // (nowadays happens in between index population and creating the constraint)
						 crashSnapshot = Fs.snapshot();
						 barrier.Release();
					}
					finally
					{
						 Db.shutdown();
					}
			  }

			  {
			  // WHEN
					GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestEnterpriseGraphDatabaseFactory() ).setFileSystem(crashSnapshot).newImpermanentDatabase();
					try
					{
						 Recreate( constraintCreator ).accept( db, transactions );
						 fail( "Should not be able to create constraint on non-unique data" );
					}
					catch ( Exception e ) when ( e is ConstraintViolationException || e is QueryExecutionException )
					{
						 // THEN good
					}
					finally
					{
						 Db.shutdown();
					}
			  }
		 }

		 private class MonitorAdapterAnonymousInnerClass : IndexingService.MonitorAdapter
		 {
			 private readonly HalfAppliedConstraintRecoveryIT _outerInstance;

			 private Neo4Net.Test.Barrier_Control _barrier;

			 public MonitorAdapterAnonymousInnerClass( HalfAppliedConstraintRecoveryIT outerInstance, Neo4Net.Test.Barrier_Control barrier )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
			 }

			 public override void indexPopulationScanComplete()
			 {
				  _barrier.reached();
			 }
		 }

		 private GraphDatabaseAPI NewDb()
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(Fs).setMonitors(_monitors).newImpermanentDatabase();
		 }

		 private static void FlushStores( GraphDatabaseAPI db )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().flush(Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited);
		 }

		 private static void Apply( GraphDatabaseAPI db, IList<TransactionRepresentation> transactions )
		 {
			  TransactionCommitProcess committer = Db.DependencyResolver.resolveDependency( typeof( TransactionCommitProcess ) );
			  transactions.ForEach(tx =>
			  {
				try
				{
					 committer.Commit( new TransactionToApply( tx ), CommitEvent.NULL, EXTERNAL );
				}
				catch ( TransactionFailureException e )
				{
					 throw new Exception( e );
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<org.Neo4Net.kernel.impl.transaction.TransactionRepresentation> createTransactionsForCreatingConstraint(System.Action<org.Neo4Net.kernel.internal.GraphDatabaseAPI> uniqueConstraintCreator) throws Exception
		 private static IList<TransactionRepresentation> CreateTransactionsForCreatingConstraint( System.Action<GraphDatabaseAPI> uniqueConstraintCreator )
		 {
			  // A separate db altogether
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestEnterpriseGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					CreateConstraint( db, uniqueConstraintCreator );
					LogicalTransactionStore txStore = Db.DependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );
					IList<TransactionRepresentation> transactions = new List<TransactionRepresentation>();
					using ( TransactionCursor cursor = txStore.GetTransactions( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 1 ) )
					{
						 cursor.forAll( tx => transactions.Add( tx.TransactionRepresentation ) );
					}
					return transactions;
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static void CreateConstraint( GraphDatabaseAPI db, System.Action<GraphDatabaseAPI> constraintCreator )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					constraintCreator( db );
					tx.Success();
			  }
		 }
	}

}
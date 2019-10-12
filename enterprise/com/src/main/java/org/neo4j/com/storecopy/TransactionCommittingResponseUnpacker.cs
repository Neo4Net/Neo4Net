/*
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
namespace Org.Neo4j.com.storecopy
{
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

	/// <summary>
	/// Receives and unpacks <seealso cref="Response responses"/>.
	/// Transaction obligations are handled by <seealso cref="TransactionObligationFulfiller"/> and
	/// <seealso cref="TransactionStream transaction streams"/> are {@link TransactionCommitProcess committed to the
	/// store}, in batches.
	/// <para>
	/// It is assumed that any <seealso cref="TransactionStreamResponse response carrying transaction data"/> comes from the one
	/// and same thread.
	/// </para>
	/// <para>
	/// SAFE ZONE EXPLAINED
	/// </para>
	/// <para>
	/// PROBLEM
	/// A slave can read inconsistent or corrupted data (mixed state records) because of reuse of property ids.
	/// This happens when a record that has been read gets reused and then read again or possibly reused in
	/// middle of reading a property chain or dynamic record chain.
	/// This is guarded for in single instance with the delaying of id reuse. This does not cover the Slave
	/// case because the transactions that SET, REMOVE and REUSE the record are applied in batch and thus a
	/// slave transaction can see states from all of the transactions that touches the reused record during its
	/// lifetime.
	/// </para>
	/// <para>
	/// SOLUTION
	/// Master and Slave are configured with the same safeZone time.
	/// Let S = safeZone time (more about safeZone time further down)
	/// </para>
	/// <para>
	/// -> Master promise to hold all deleted ids in quarantine before reusing them, (S duration).
	/// He thereby creates a safe zone of transactions that among themselves are guaranteed to be free of
	/// id reuse contamination.
	/// -> Slave promise to not let any transactions cross the safe zone boundary.
	/// Meaning all transactions that falls out of the safe zone, as updates gets applied,
	/// will need to be terminated, with a hint that they can simply be restarted
	/// </para>
	/// <para>
	/// Safe zone is a time frame in Masters domain. All transactions that started and finished within this
	/// time frame are guarantied to not have read any mixed state records.
	/// </para>
	/// <para>
	/// Example of a transaction running on slave that starts reading a dynamic property, then a batch is pulled from master
	/// that deletes the property and and reuses the record in the chain, making the transaction read inconsistent data.
	/// </para>
	/// <para>
	/// TX starts reading
	/// tx here
	/// <pre>
	/// v
	/// |aaaa|->|aaaa|->|aaaa|->|aaaa|
	/// 1       2       3       4
	/// </pre>
	/// "a" string is deleted and replaced with "bbbbbbbbbbbbbbbb"
	/// <pre>
	/// tx here
	/// v
	/// |bbbb|->|bbbb|->|bbbb|->|bbbb|
	/// 1       2       3       4
	/// </pre>
	/// TX continues reading and does not know anything is wrong,
	/// returning the inconsistent string "aaaaaaaabbbbbbbb".
	/// <pre>
	/// tx here
	/// v
	/// |bbbb|->|bbbb|->|bbbb|->|bbbb|
	/// 1       2       3       4
	/// </pre>
	/// Example of how the safe zone window moves while applying a batch
	/// <pre>
	/// x---------------------------------------------------------------------------------->| TIME
	/// |MASTER STATE
	/// |---------------------------------------------------------------------------------->|
	/// |                                                          Batch to apply to slave
	/// |                                  safeZone with size S  |<------------------------>|
	/// |                                                  |
	/// |                                                  v     A
	/// |SLAVE STATE 1 (before applying batch)         |<---S--->|
	/// |----------------------------------------------+-------->|
	/// |                                                        |
	/// |                                                        |
	/// |                                                        |      B
	/// |SLAVE STATE 2 (mid apply)                            |<-+-S--->|
	/// |-----------------------------------------------------+--+----->|
	/// |                                                        |      |
	/// |                                                        |      |
	/// |                                                        |      |  C
	/// |SLAVE STATE 3 (mid apply / after apply)                 |<---S-+->|
	/// |--------------------------------------------------------+------+->|
	/// |                                                        |      |  |
	/// |                                                        |      |  |
	/// |                                                        |      |  |                D
	/// |SLAVE STATE 4 (after apply)                             |      |  |      |<---S--->|
	/// |--------------------------------------------------------+------+--+------+-------->|
	/// </pre>
	/// </para>
	/// <para>
	/// What we see in this diagram is a slave pulling updates from the master.
	/// While doing so, the safe zone window |<---S--->| is pushed forward. NOTE that we do not see any explicit transaction
	/// running on slave. Only the times (A, B, C, D) that we discuss.
	/// </para>
	/// <para>
	/// slaveTx start on slave when slave is in SLAVE STATE 1
	/// - Latest applied transaction on slave has timestamp A and safe zone is A-S.
	/// - slaveTx.startTime = A
	/// </para>
	/// <para>
	/// Scenario 1 - slaveTx finish when slave is in SLAVE STATE 2
	/// Latest applied transaction in store has timestamp B and safe zone is B-S.
	/// slaveTx did not cross the safe zone boundary as slaveTx.startTime = A > B-S
	/// We can safely assume that slaveTx did not read any mixed state records.
	/// </para>
	/// <para>
	/// Scenario 2 - slaveTx has not yet finished in SLAVE STATE 3
	/// Latest applied transaction in store has timestamp C and safe zone is C-S.
	/// We are just about to apply the next part of the batch and push the safe zone window forward.
	/// This will make slaveTx.startTime = A < C-S. This means Tx is now in risk of reading mixed state records.
	/// We will terminate slaveTx and let the user try again.
	/// </para>
	/// <para>
	/// <b>NOTE ABOUT TX_COMMIT_TIMESTAMP</b>
	/// commitTimestamp is used by <seealso cref="MetaDataStore"/> to keep track of the commit timestamp of the last committed
	/// transaction. When starting up a db we can not always know what the the latest commit timestamp is but slave need it
	/// to know when a transaction needs to be terminated during batch application.
	/// The latest commit timestamp is an important part of "safeZone" that is explained in
	/// TransactionCommittingResponseUnpacker.
	/// </para>
	/// <para>
	/// Here are the different scenarios, what timestamp that is used and what it means for execution.
	/// </para>
	/// <para>
	/// Empty store <br>
	/// TIMESTAMP: <seealso cref="TransactionIdStore.BASE_TX_COMMIT_TIMESTAMP"/> <br>
	/// ==> FINE. NO KILL because no previous state can have been observed anyway <br>
	/// </para>
	/// <para>
	/// Upgraded store w/ tx logs <br>
	/// TIMESTAMP CARRIED OVER FROM LOG <br>
	/// ==> FINE <br>
	/// </para>
	/// <para>
	/// Upgraded store w/o tx logs <br>
	/// TIMESTAMP <seealso cref="TransactionIdStore.UNKNOWN_TX_COMMIT_TIMESTAMP"/> (1) <br>
	/// ==> SLAVE TRANSACTIONS WILL TERMINATE WHEN FIRST PULL UPDATES HAPPENS <br>
	/// </para>
	/// <para>
	/// Store on 2.3.prev, w/ tx logs (no upgrade) <br>
	/// TIMESTAMP CARRIED OVER FROM LOG <br>
	/// ==> FINE <br>
	/// </para>
	/// <para>
	/// Store on 2.3.prev w/o tx logs (no upgrade) <br>
	/// TIMESTAMP <seealso cref="TransactionIdStore.UNKNOWN_TX_COMMIT_TIMESTAMP"/> (1) <br>
	/// ==> SLAVE TRANSACTIONS WILL TERMINATE WHEN FIRST PULL UPDATES HAPPENS <br>
	/// </para>
	/// <para>
	/// Store already on 2.3.next, w/ or w/o tx logs <br>
	/// TIMESTAMP CORRECT <br>
	/// ==> FINE
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= TransactionBatchCommitter </seealso>
	public class TransactionCommittingResponseUnpacker : LifecycleAdapter, ResponseUnpacker
	{
		 /// <summary>
		 /// Dependencies that this <seealso cref="TransactionCommittingResponseUnpacker"/> has. These are called upon
		 /// in <seealso cref="TransactionCommittingResponseUnpacker.start()"/>.
		 /// </summary>
		 public interface Dependencies
		 {
			  /// <summary>
			  /// Responsible for committing batches of transactions received from transaction stream responses.
			  /// </summary>
			  TransactionCommitProcess CommitProcess();

			  /// <summary>
			  /// Responsible for fulfilling transaction obligations received from transaction obligation responses.
			  /// </summary>
			  TransactionObligationFulfiller ObligationFulfiller();

			  /// <summary>
			  /// Log provider
			  /// </summary>
			  LogService LogService();

			  KernelTransactions KernelTransactions();

			  /// <summary>
			  /// Version context supplier
			  /// </summary>
			  VersionContextSupplier VersionContextSupplier();
		 }

		 /// <summary>
		 /// Common implementation which pulls out dependencies from a <seealso cref="DependencyResolver"/> and constructs
		 /// whatever components it needs from that.
		 /// </summary>
		 private class ResolvableDependencies : Dependencies
		 {
			  internal readonly DependencyResolver GlobalResolver;

			  internal ResolvableDependencies( DependencyResolver globalResolver )
			  {
					this.GlobalResolver = globalResolver;
			  }

			  public override TransactionCommitProcess CommitProcess()
			  {
					// We simply can't resolve the commit process here, since the commit process of a slave
					// is one that sends transactions to the master. We here, however would like to actually
					// commit transactions in this db.
					DependencyResolver databaseResolver = DatabaseResolver;
					return new TransactionRepresentationCommitProcess( databaseResolver.ResolveDependency( typeof( TransactionAppender ) ), databaseResolver.ResolveDependency( typeof( StorageEngine ) ) );
			  }

			  public override TransactionObligationFulfiller ObligationFulfiller()
			  {
					try
					{
						 return GlobalResolver.resolveDependency( typeof( TransactionObligationFulfiller ) );
					}
					catch ( UnsatisfiedDependencyException )
					{
						 return toTxId =>
						 {
						  throw new System.NotSupportedException( "Should not be called" );
						 };
					}
			  }

			  public override LogService LogService()
			  {
					return GlobalResolver.resolveDependency( typeof( LogService ) );
			  }

			  public override KernelTransactions KernelTransactions()
			  {
					return DatabaseResolver.resolveDependency( typeof( KernelTransactions ) );
			  }

			  internal virtual DependencyResolver DatabaseResolver
			  {
				  get
				  {
						DatabaseManager databaseManager = GlobalResolver.resolveDependency( typeof( DatabaseManager ) );
						Config config = GlobalResolver.resolveDependency( typeof( Config ) );
						GraphDatabaseFacade facade = databaseManager.GetDatabaseFacade( config.Get( GraphDatabaseSettings.active_database ) ).get();
						return facade.DependencyResolver;
				  }
			  }

			  public override VersionContextSupplier VersionContextSupplier()
			  {
					return GlobalResolver.resolveDependency( typeof( VersionContextSupplier ) );
			  }
		 }

		 public const int DEFAULT_BATCH_SIZE = 100;

		 // Assigned in constructor
		 private readonly Dependencies _dependencies;
		 private readonly int _maxBatchSize;
		 private readonly long _idReuseSafeZoneTime;

		 // Assigned in start()
		 private TransactionObligationFulfiller _obligationFulfiller;
		 private TransactionBatchCommitter _batchCommitter;
		 private VersionContextSupplier _versionContextSupplier;
		 private Log _log;
		 // Assigned in stop()
		 private volatile bool _stopped;

		 public TransactionCommittingResponseUnpacker( DependencyResolver dependencies, int maxBatchSize, long idReuseSafeZoneTime ) : this( new ResolvableDependencies( dependencies ), maxBatchSize, idReuseSafeZoneTime )
		 {
		 }

		 public TransactionCommittingResponseUnpacker( Dependencies dependencies, int maxBatchSize, long idReuseSafeZoneTime )
		 {
			  this._dependencies = dependencies;
			  this._maxBatchSize = maxBatchSize;
			  this._idReuseSafeZoneTime = idReuseSafeZoneTime;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void unpackResponse(org.neo4j.com.Response<?> response, ResponseUnpacker_TxHandler txHandler) throws Exception
		 public override void UnpackResponse<T1>( Response<T1> response, ResponseUnpacker_TxHandler txHandler )
		 {
			  if ( _stopped )
			  {
					throw new System.InvalidOperationException( "Component is currently stopped" );
			  }

			  BatchingResponseHandler responseHandler = new BatchingResponseHandler( _maxBatchSize, _batchCommitter, _obligationFulfiller, txHandler, _versionContextSupplier, _log );
			  try
			  {
					response.Accept( responseHandler );
			  }
			  finally
			  {
					responseHandler.ApplyQueuedTransactions();
			  }
		 }

		 public override void Start()
		 {
			  this._obligationFulfiller = _dependencies.obligationFulfiller();
			  this._log = _dependencies.logService().getInternalLog(typeof(BatchingResponseHandler));
			  this._versionContextSupplier = _dependencies.versionContextSupplier();
			  this._batchCommitter = new TransactionBatchCommitter( _dependencies.kernelTransactions(), _idReuseSafeZoneTime, _dependencies.commitProcess(), _log );
			  this._stopped = false;
		 }

		 public override void Stop()
		 {
			  this._stopped = true;
		 }
	}

}
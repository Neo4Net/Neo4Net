using System;

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
namespace Org.Neo4j.causalclustering.core.state.snapshot
{

	using CatchUpClient = Org.Neo4j.causalclustering.catchup.CatchUpClient;
	using CatchUpClientException = Org.Neo4j.causalclustering.catchup.CatchUpClientException;
	using Org.Neo4j.causalclustering.catchup;
	using CatchupAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider;
	using CatchupAddressResolutionException = Org.Neo4j.causalclustering.catchup.CatchupAddressResolutionException;
	using CatchupResult = Org.Neo4j.causalclustering.catchup.CatchupResult;
	using CommitStateHelper = Org.Neo4j.causalclustering.catchup.storecopy.CommitStateHelper;
	using DatabaseShutdownException = Org.Neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Org.Neo4j.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreCopyProcess = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyProcess;
	using StoreIdDownloadFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using CoreStateMachines = Org.Neo4j.causalclustering.core.state.machines.CoreStateMachines;
	using Suspendable = Org.Neo4j.causalclustering.helper.Suspendable;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	public class CoreStateDownloader
	{
		 private readonly LocalDatabase _localDatabase;
		 private readonly Suspendable _suspendOnStoreCopy;
		 private readonly RemoteStore _remoteStore;
		 private readonly CatchUpClient _catchUpClient;
		 private readonly Log _log;
		 private readonly StoreCopyProcess _storeCopyProcess;
		 private readonly CoreStateMachines _coreStateMachines;
		 private readonly CoreSnapshotService _snapshotService;
		 private CommitStateHelper _commitStateHelper;

		 public CoreStateDownloader( LocalDatabase localDatabase, Suspendable suspendOnStoreCopy, RemoteStore remoteStore, CatchUpClient catchUpClient, LogProvider logProvider, StoreCopyProcess storeCopyProcess, CoreStateMachines coreStateMachines, CoreSnapshotService snapshotService, CommitStateHelper commitStateHelper )
		 {
			  this._localDatabase = localDatabase;
			  this._suspendOnStoreCopy = suspendOnStoreCopy;
			  this._remoteStore = remoteStore;
			  this._catchUpClient = catchUpClient;
			  this._log = logProvider.getLog( this.GetType() );
			  this._storeCopyProcess = storeCopyProcess;
			  this._coreStateMachines = coreStateMachines;
			  this._snapshotService = snapshotService;
			  this._commitStateHelper = commitStateHelper;
		 }

		 /// <summary>
		 /// Tries to catchup this instance by downloading a snapshot. A snapshot consists of both the
		 /// comparatively small state of the cluster state machines as well as the database store. The
		 /// store is however caught up using two different approach. If it is possible to catchup by
		 /// pulling transactions, then this will be sufficient, but if the store is lagging too far
		 /// behind then a complete store copy will be attempted.
		 /// </summary>
		 /// <param name="addressProvider"> Provider of addresses to catchup from. </param>
		 /// <returns> True if the operation succeeded, and false otherwise. </returns>
		 /// <exception cref="LifecycleException"> A major database component failed to start or stop. </exception>
		 /// <exception cref="IOException"> An issue with I/O. </exception>
		 /// <exception cref="DatabaseShutdownException"> The database is shutting down. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean downloadSnapshot(org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider) throws org.neo4j.kernel.lifecycle.LifecycleException, java.io.IOException, org.neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException
		 internal virtual bool DownloadSnapshot( CatchupAddressProvider addressProvider )
		 {
			  /* Extract some key properties before shutting it down. */
			  bool isEmptyStore = _localDatabase.Empty;

			  /*
			   *  There is no reason to try to recover if there are no transaction logs and in fact it is
			   *  also problematic for the initial transaction pull during the snapshot download because the
			   *  kernel will create a transaction log with a header where previous index points to the same
			   *  index as that written down into the metadata store. This is problematic because we have no
			   *  guarantee that there are later transactions and we need at least one transaction in
			   *  the log to figure out the Raft log index (see {@link RecoverConsensusLogIndex}).
			   */
			  if ( _commitStateHelper.hasTxLogs( _localDatabase.databaseLayout() ) )
			  {
					_log.info( "Recovering local database" );
					Ensure( _localDatabase.start, "start local database" );
					Ensure( _localDatabase.stop, "stop local database" );
			  }

			  AdvertisedSocketAddress primary;
			  StoreId remoteStoreId;
			  try
			  {
					primary = addressProvider.Primary();
					remoteStoreId = _remoteStore.getStoreId( primary );
			  }
			  catch ( Exception e ) when ( e is CatchupAddressResolutionException || e is StoreIdDownloadFailedException )
			  {
					_log.warn( "Store copy failed", e );
					return false;
			  }

			  if ( !isEmptyStore && !remoteStoreId.Equals( _localDatabase.storeId() ) )
			  {
					_log.error( "Store copy failed due to store ID mismatch" );
					return false;
			  }

			  Ensure( _suspendOnStoreCopy.disable, "disable auxiliary services before store copy" );
			  Ensure( _localDatabase.stopForStoreCopy, "stop local database for store copy" );

			  _log.info( "Downloading snapshot from core server at %s", primary );

			  /* The core snapshot must be copied before the store, because the store has a dependency on
			   * the state of the state machines. The store will thus be at or ahead of the state machines,
			   * in consensus log index, and application of commands will bring them in sync. Any such commands
			   * that carry transactions will thus be ignored by the transaction/token state machines, since they
			   * are ahead, and the correct decisions for their applicability have already been taken as encapsulated
			   * in the copied store. */

			  CoreSnapshot coreSnapshot;
			  try
			  {
					coreSnapshot = _catchUpClient.makeBlockingRequest( primary, new CoreSnapshotRequest(), new CatchUpResponseAdaptorAnonymousInnerClass(this) );
			  }
			  catch ( CatchUpClientException e )
			  {
					_log.warn( "Store copy failed", e );
					return false;
			  }

			  if ( !isEmptyStore )
			  {
					StoreId localStoreId = _localDatabase.storeId();
					CatchupResult catchupResult;
					try
					{
						 catchupResult = _remoteStore.tryCatchingUp( primary, localStoreId, _localDatabase.databaseLayout(), false, false );
					}
					catch ( StoreCopyFailedException e )
					{
						 _log.warn( "Failed to catch up", e );
						 return false;
					}

					if ( catchupResult == E_TRANSACTION_PRUNED )
					{
						 _log.warn( format( "Failed to pull transactions from (%s). They may have been pruned away", primary ) );
						 _localDatabase.delete();
						 isEmptyStore = true;
					}
					else if ( catchupResult != SUCCESS_END_OF_STREAM )
					{
						 _log.warn( format( "Unexpected catchup operation result %s from %s", catchupResult, primary ) );
						 return false;
					}
			  }

			  if ( isEmptyStore )
			  {
					try
					{
						 _storeCopyProcess.replaceWithStoreFrom( addressProvider, remoteStoreId );
					}
					catch ( StoreCopyFailedException e )
					{
						 _log.warn( "Failed to copy and replace store", e );
						 return false;
					}
			  }

			  /* We install the snapshot after the store has been downloaded,
			   * so that we are not left with a state ahead of the store. */
			  _snapshotService.installSnapshot( coreSnapshot );
			  _log.info( "Core snapshot installed: " + coreSnapshot );

			  /* Starting the database will invoke the commit process factory in
			   * the EnterpriseCoreEditionModule, which has important side-effects. */
			  _log.info( "Starting local database" );
			  Ensure( _localDatabase.start, "start local database after store copy" );

			  _coreStateMachines.installCommitProcess( _localDatabase.CommitProcess );
			  Ensure( _suspendOnStoreCopy.enable, "enable auxiliary services after store copy" );

			  return true;
		 }

		 private class CatchUpResponseAdaptorAnonymousInnerClass : CatchUpResponseAdaptor<CoreSnapshot>
		 {
			 private readonly CoreStateDownloader _outerInstance;

			 public CatchUpResponseAdaptorAnonymousInnerClass( CoreStateDownloader outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void onCoreSnapshot( CompletableFuture<CoreSnapshot> signal, CoreSnapshot response )
			 {
				  signal.complete( response );
			 }
		 }

		 public interface LifecycleAction
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void perform() throws Throwable;
			  void Perform();
		 }

		 private static void Ensure( LifecycleAction action, string operation )
		 {
			  try
			  {
					action.Perform();
			  }
			  catch ( Exception cause )
			  {
					throw new LifecycleException( "Failed to " + operation, cause );
			  }
		 }
	}

}
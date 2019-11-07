using System;
using System.Threading;

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
namespace Neo4Net.causalclustering.readreplica
{

	using CatchupAddressProvider_SingleAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider_SingleAddressProvider;
	using DatabaseShutdownException = Neo4Net.causalclustering.catchup.storecopy.DatabaseShutdownException;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreCopyProcess = Neo4Net.causalclustering.catchup.storecopy.StoreCopyProcess;
	using StoreIdDownloadFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using TopologyLookupException = Neo4Net.causalclustering.core.state.snapshot.TopologyLookupException;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using UpstreamDatabaseSelectionException = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	internal class ReadReplicaStartupProcess : Lifecycle
	{
		 private readonly RemoteStore _remoteStore;
		 private readonly LocalDatabase _localDatabase;
		 private readonly Lifecycle _txPulling;
		 private readonly Log _debugLog;
		 private readonly Log _userLog;

		 private readonly TimeoutStrategy _timeoutStrategy;
		 private readonly UpstreamDatabaseStrategySelector _selectionStrategy;
		 private readonly TopologyService _topologyService;

		 private string _lastIssue;
		 private readonly StoreCopyProcess _storeCopyProcess;

		 internal ReadReplicaStartupProcess( RemoteStore remoteStore, LocalDatabase localDatabase, Lifecycle txPulling, UpstreamDatabaseStrategySelector selectionStrategy, TimeoutStrategy timeoutStrategy, LogProvider debugLogProvider, LogProvider userLogProvider, StoreCopyProcess storeCopyProcess, TopologyService topologyService )
		 {
			  this._remoteStore = remoteStore;
			  this._localDatabase = localDatabase;
			  this._txPulling = txPulling;
			  this._selectionStrategy = selectionStrategy;
			  this._timeoutStrategy = timeoutStrategy;
			  this._debugLog = debugLogProvider.getLog( this.GetType() );
			  this._userLog = userLogProvider.getLog( this.GetType() );
			  this._storeCopyProcess = storeCopyProcess;
			  this._topologyService = topologyService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  _localDatabase.init();
			  _txPulling.init();
		 }

		 private string IssueOf( string operation, int attempt )
		 {
			  return format( "Failed attempt %d of %s", attempt, operation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException, Neo4Net.causalclustering.catchup.storecopy.DatabaseShutdownException
		 public override void Start()
		 {
			  bool syncedWithUpstream = false;
			  Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout timeout = _timeoutStrategy.newTimeout();
			  int attempt = 0;
			  while ( !syncedWithUpstream )
			  {
					attempt++;
					MemberId source = null;
					try
					{
						 source = _selectionStrategy.bestUpstreamDatabase();
						 SyncStoreWithUpstream( source );
						 syncedWithUpstream = true;
					}
					catch ( UpstreamDatabaseSelectionException )
					{
						 _lastIssue = IssueOf( "finding upstream member", attempt );
						 _debugLog.warn( _lastIssue );
					}
					catch ( StoreCopyFailedException )
					{
						 _lastIssue = IssueOf( format( "copying store files from %s", source ), attempt );
						 _debugLog.warn( _lastIssue );
					}
					catch ( StoreIdDownloadFailedException )
					{
						 _lastIssue = IssueOf( format( "getting store id from %s", source ), attempt );
						 _debugLog.warn( _lastIssue );
					}
					catch ( TopologyLookupException )
					{
						 _lastIssue = IssueOf( format( "getting address of %s", source ), attempt );
						 _debugLog.warn( _lastIssue );
					}

					try
					{
						 Thread.Sleep( timeout.Millis );
						 timeout.Increment();
					}
					catch ( InterruptedException )
					{
						 Thread.interrupted();
						 _lastIssue = "Interrupted while trying to start read replica";
						 _debugLog.warn( _lastIssue );
						 break;
					}
			  }

			  if ( !syncedWithUpstream )
			  {
					_userLog.error( _lastIssue );
					throw new Exception( _lastIssue );
			  }

			  try
			  {
					_localDatabase.start();
					_txPulling.start();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void syncStoreWithUpstream(Neo4Net.causalclustering.identity.MemberId source) throws java.io.IOException, Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException, Neo4Net.causalclustering.core.state.snapshot.TopologyLookupException, Neo4Net.causalclustering.catchup.storecopy.DatabaseShutdownException
		 private void SyncStoreWithUpstream( MemberId source )
		 {
			  if ( _localDatabase.Empty )
			  {
					_debugLog.info( "Local database is empty, attempting to replace with copy from upstream server %s", source );

					_debugLog.info( "Finding store id of upstream server %s", source );
					AdvertisedSocketAddress fromAddress = _topologyService.findCatchupAddress( source ).orElseThrow( () => new TopologyLookupException(source) );
					StoreId storeId = _remoteStore.getStoreId( fromAddress );

					_debugLog.info( "Copying store from upstream server %s", source );
					_localDatabase.delete();
					_storeCopyProcess.replaceWithStoreFrom( new CatchupAddressProvider_SingleAddressProvider( fromAddress ), storeId );

					_debugLog.info( "Restarting local database after copy.", source );
			  }
			  else
			  {
					EnsureSameStoreIdAs( source );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureSameStoreIdAs(Neo4Net.causalclustering.identity.MemberId upstream) throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, Neo4Net.causalclustering.core.state.snapshot.TopologyLookupException
		 private void EnsureSameStoreIdAs( MemberId upstream )
		 {
			  StoreId localStoreId = _localDatabase.storeId();
			  AdvertisedSocketAddress advertisedSocketAddress = _topologyService.findCatchupAddress( upstream ).orElseThrow( () => new TopologyLookupException(upstream) );
			  StoreId remoteStoreId = _remoteStore.getStoreId( advertisedSocketAddress );
			  if ( !localStoreId.Equals( remoteStoreId ) )
			  {
					throw new System.InvalidOperationException( format( "This read replica cannot join the cluster. " + "The local database is not empty and has a mismatching storeId: expected %s actual %s.", remoteStoreId, localStoreId ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _txPulling.stop();
			  _localDatabase.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _txPulling.shutdown();
			  _localDatabase.shutdown();
		 }
	}

}
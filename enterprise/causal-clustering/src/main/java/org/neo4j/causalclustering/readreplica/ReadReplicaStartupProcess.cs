using System;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.readreplica
{

	using CatchupAddressProvider_SingleAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_SingleAddressProvider;
	using DatabaseShutdownException = Org.Neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Org.Neo4j.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreCopyProcess = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyProcess;
	using StoreIdDownloadFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using TopologyLookupException = Org.Neo4j.causalclustering.core.state.snapshot.TopologyLookupException;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using TimeoutStrategy = Org.Neo4j.causalclustering.helper.TimeoutStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using UpstreamDatabaseSelectionException = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseSelectionException;
	using UpstreamDatabaseStrategySelector = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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
//ORIGINAL LINE: public void start() throws java.io.IOException, org.neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException
		 public override void Start()
		 {
			  bool syncedWithUpstream = false;
			  Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout timeout = _timeoutStrategy.newTimeout();
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
//ORIGINAL LINE: private void syncStoreWithUpstream(org.neo4j.causalclustering.identity.MemberId source) throws java.io.IOException, org.neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, org.neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException, org.neo4j.causalclustering.core.state.snapshot.TopologyLookupException, org.neo4j.causalclustering.catchup.storecopy.DatabaseShutdownException
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
//ORIGINAL LINE: private void ensureSameStoreIdAs(org.neo4j.causalclustering.identity.MemberId upstream) throws org.neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, org.neo4j.causalclustering.core.state.snapshot.TopologyLookupException
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
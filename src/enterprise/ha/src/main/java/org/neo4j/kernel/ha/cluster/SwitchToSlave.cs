using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.cluster
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using Neo4Net.com;
	using ServerUtil = Neo4Net.com.ServerUtil;
	using MoveAfterCopy = Neo4Net.com.storecopy.MoveAfterCopy;
	using StoreCopyClient = Neo4Net.com.storecopy.StoreCopyClient;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using TransactionCommittingResponseUnpacker = Neo4Net.com.storecopy.TransactionCommittingResponseUnpacker;
	using TransactionObligationFulfiller = Neo4Net.com.storecopy.TransactionObligationFulfiller;
	using CancellationRequest = Neo4Net.Helpers.CancellationRequest;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using DatabaseAvailability = Neo4Net.Kernel.availability.DatabaseAvailability;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.ha;
	using ClusterMember = Neo4Net.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using HandshakeResult = Neo4Net.Kernel.ha.com.master.HandshakeResult;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using MasterClient = Neo4Net.Kernel.ha.com.slave.MasterClient;
	using MasterClientResolver = Neo4Net.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveImpl = Neo4Net.Kernel.ha.com.slave.SlaveImpl;
	using SlaveServer = Neo4Net.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Neo4Net.Kernel.ha.id.HaIdGeneratorFactory;
	using UnableToCopyStoreFromOldMasterException = Neo4Net.Kernel.ha.store.UnableToCopyStoreFromOldMasterException;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using MissingLogDataException = Neo4Net.Kernel.impl.transaction.log.MissingLogDataException;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using StoreLockerLifecycleAdapter = Neo4Net.Kernel.Internal.locker.StoreLockerLifecycleAdapter;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.member.ClusterMembers.hasInstanceId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.getServerId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NeoStores.isStorePresent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public abstract class SwitchToSlave
	{
		 // TODO solve this with lifecycle instance grouping or something
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static final Class[] SERVICES_TO_RESTART_FOR_STORE_COPY = new Class[]{ org.neo4j.kernel.internal.locker.StoreLockerLifecycleAdapter.class, org.neo4j.kernel.impl.transaction.state.DataSourceManager.class, org.neo4j.kernel.ha.com.RequestContextFactory.class, org.neo4j.com.storecopy.TransactionCommittingResponseUnpacker.class, org.neo4j.kernel.impl.index.IndexConfigStore.class};
		 private static readonly Type[] _servicesToRestartForStoreCopy = new Type[]{ typeof( StoreLockerLifecycleAdapter ), typeof( DataSourceManager ), typeof( RequestContextFactory ), typeof( TransactionCommittingResponseUnpacker ), typeof( IndexConfigStore ) };
		 private readonly StoreCopyClient _storeCopyClient;
		 private readonly System.Func<Slave, SlaveServer> _slaveServerFactory;
		 protected internal readonly UpdatePuller UpdatePuller;
		 protected internal readonly Monitors Monitors;
		 internal readonly Log UserLog;
		 internal readonly Log MsgLog;
		 protected internal readonly Config Config;
		 private readonly HaIdGeneratorFactory _idGeneratorFactory;
		 private readonly DelegateInvocationHandler<Master> _masterDelegateHandler;
		 private readonly ClusterMemberAvailability _clusterMemberAvailability;
		 protected internal readonly RequestContextFactory RequestContextFactory;
		 private readonly MasterClientResolver _masterClientResolver;
		 private readonly PullerFactory _updatePullerFactory;
		 protected internal readonly Monitor Monitor;
		 protected internal readonly DatabaseLayout DatabaseLayout;
		 protected internal readonly PageCache PageCache;

		 private readonly System.Func<NeoStoreDataSource> _neoDataSourceSupplier;
		 private readonly System.Func<TransactionIdStore> _transactionIdStoreSupplier;
		 private readonly System.Func<DatabaseTransactionStats> _transactionStatsSupplier;

		 internal SwitchToSlave( HaIdGeneratorFactory idGeneratorFactory, Monitors monitors, RequestContextFactory requestContextFactory, DelegateInvocationHandler<Master> masterDelegateHandler, ClusterMemberAvailability clusterMemberAvailability, MasterClientResolver masterClientResolver, Monitor monitor, PullerFactory pullerFactory, UpdatePuller updatePuller, System.Func<Slave, SlaveServer> slaveServerFactory, Config config, LogService logService, PageCache pageCache, DatabaseLayout databaseLayout, System.Func<TransactionIdStore> transactionIdStoreSupplier, System.Func<DatabaseTransactionStats> transactionStatsSupplier, System.Func<NeoStoreDataSource> neoDataSourceSupplier, StoreCopyClient storeCopyClient )
		 {
			  this._idGeneratorFactory = idGeneratorFactory;
			  this.Monitors = monitors;
			  this.RequestContextFactory = requestContextFactory;
			  this._masterDelegateHandler = masterDelegateHandler;
			  this._clusterMemberAvailability = clusterMemberAvailability;
			  this._masterClientResolver = masterClientResolver;
			  this.UserLog = logService.GetUserLog( this.GetType() );
			  this.MsgLog = logService.GetInternalLog( this.GetType() );
			  this.Monitor = monitor;
			  this._updatePullerFactory = pullerFactory;
			  this.UpdatePuller = updatePuller;
			  this._slaveServerFactory = slaveServerFactory;
			  this.Config = config;
			  this.PageCache = pageCache;
			  this.DatabaseLayout = databaseLayout;
			  this._transactionIdStoreSupplier = transactionIdStoreSupplier;
			  this._transactionStatsSupplier = transactionStatsSupplier;
			  this._neoDataSourceSupplier = neoDataSourceSupplier;
			  this._storeCopyClient = storeCopyClient;
		 }

		 /// <summary>
		 /// Performs a switch to the slave state. Starts the communication endpoints, switches components to the slave state
		 /// and ensures that the current database is appropriate for this cluster. It also broadcasts the appropriate
		 /// Slave Is Available event
		 /// </summary>
		 /// <param name="haCommunicationLife"> The LifeSupport instance to register the network facilities required for
		 ///                            communication with the rest of the cluster </param>
		 /// <param name="me"> The URI this instance must bind to </param>
		 /// <param name="masterUri"> The URI of the master for which this instance must become slave to </param>
		 /// <param name="cancellationRequest"> A handle for gracefully aborting the switch </param>
		 /// <returns> The URI that was broadcasted as the slave endpoint or null if the task was cancelled </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI switchToSlave(org.neo4j.kernel.lifecycle.LifeSupport haCommunicationLife, java.net.URI me, java.net.URI masterUri, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual URI SwitchToSlaveConflict( LifeSupport haCommunicationLife, URI me, URI masterUri, CancellationRequest cancellationRequest )
		 {
			  URI slaveUri;
			  bool success = false;

			  Monitor.switchToSlaveStarted();

			  // Wait a short while for current transactions to stop first, just to be nice.
			  // We can't wait forever since switching to our designated role is quite important.
			  Clock clock = Clocks.systemClock();
			  long deadline = clock.millis() + Config.get(HaSettings.internal_state_switch_timeout).toMillis();
			  DatabaseTransactionStats transactionStats = _transactionStatsSupplier.get();
			  while ( transactionStats.NumberOfActiveTransactions > 0 && clock.millis() < deadline )
			  {
					parkNanos( MILLISECONDS.toNanos( 10 ) );
			  }

			  try
			  {
					InstanceId myId = Config.get( ClusterSettings.server_id );

					UserLog.info( "ServerId %s, moving to slave for master %s", myId, masterUri );

					Debug.Assert( masterUri != null ); // since we are here it must already have been set from outside

					_idGeneratorFactory.switchToSlave();

					CopyStoreFromMasterIfNeeded( masterUri, me, cancellationRequest );

					/*
					 * The following check is mandatory, since the store copy can be cancelled and if it was actually
					 * happening then we can't continue, as there is no store in place
					 */
					if ( cancellationRequest.CancellationRequested() )
					{
						 MsgLog.info( "Switch to slave cancelled during store copy if no local store is present." );
						 return null;
					}

					/*
					 * We get here either with a fresh store from the master copy above so we need to
					 * start the ds or we already had a store, so we have already started the ds. Either way,
					 * make sure it's there.
					 */
					NeoStoreDataSource neoDataSource = _neoDataSourceSupplier.get();
					neoDataSource.AfterModeSwitch();
					StoreId myStoreId = neoDataSource.StoreId;

					bool consistencyChecksExecutedSuccessfully = ExecuteConsistencyChecks( _transactionIdStoreSupplier.get(), masterUri, me, myStoreId, cancellationRequest );

					if ( !consistencyChecksExecutedSuccessfully )
					{
						 MsgLog.info( "Switch to slave cancelled due to consistency check failure." );
						 return null;
					}

					if ( cancellationRequest.CancellationRequested() )
					{
						 MsgLog.info( "Switch to slave cancelled after consistency checks." );
						 return null;
					}

					// no exception were thrown and we can proceed
					slaveUri = StartHaCommunication( haCommunicationLife, neoDataSource, me, masterUri, myStoreId, cancellationRequest );
					if ( slaveUri == null )
					{
						 MsgLog.info( "Switch to slave unable to connect." );
						 return null;
					}

					success = true;
					UserLog.info( "ServerId %s, successfully moved to slave for master %s", myId, masterUri );
			  }
			  finally
			  {
					Monitor.switchToSlaveCompleted( success );
			  }

			  return slaveUri;
		 }

		 internal virtual void CheckMyStoreIdAndMastersStoreId( StoreId myStoreId, URI masterUri )
		 {
			  ClusterMembers clusterMembers = ResolveDatabaseDependency( typeof( ClusterMembers ) );
			  InstanceId serverId = HighAvailabilityModeSwitcher.getServerId( masterUri );
			  IEnumerable<ClusterMember> members = clusterMembers.Members;
			  ClusterMember master = firstOrNull( filter( hasInstanceId( serverId ), members ) );
			  if ( master == null )
			  {
					throw new System.InvalidOperationException( "Cannot find the master among " + members + " with master serverId=" + serverId + " and uri=" + masterUri );
			  }

			  StoreId masterStoreId = master.StoreId;

			  if ( !myStoreId.Equals( masterStoreId ) )
			  {
					throw new MismatchingStoreIdException( myStoreId, master.StoreId );
			  }
			  else if ( !myStoreId.EqualsByUpgradeId( master.StoreId ) )
			  {
					throw new BranchedDataException( "My store with " + myStoreId + " was updated independently from " + "master's store " + masterStoreId );
			  }
		 }

		 protected internal virtual T ResolveDatabaseDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return _neoDataSourceSupplier.get().DependencyResolver.resolveDependency(clazz);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URI startHaCommunication(org.neo4j.kernel.lifecycle.LifeSupport haCommunicationLife, org.neo4j.kernel.NeoStoreDataSource neoDataSource, java.net.URI me, java.net.URI masterUri, org.neo4j.storageengine.api.StoreId storeId, org.neo4j.helpers.CancellationRequest cancellationRequest) throws IllegalArgumentException, InterruptedException
		 private URI StartHaCommunication( LifeSupport haCommunicationLife, NeoStoreDataSource neoDataSource, URI me, URI masterUri, StoreId storeId, CancellationRequest cancellationRequest )
		 {
			  MasterClient master = NewMasterClient( masterUri, me, neoDataSource.StoreId, haCommunicationLife );

			  TransactionObligationFulfiller obligationFulfiller = ResolveDatabaseDependency( typeof( TransactionObligationFulfiller ) );
			  UpdatePullerScheduler updatePullerScheduler = _updatePullerFactory.createUpdatePullerScheduler( UpdatePuller );

			  Slave slaveImpl = new SlaveImpl( obligationFulfiller );

			  SlaveServer server = _slaveServerFactory.apply( slaveImpl );

			  if ( cancellationRequest.CancellationRequested() )
			  {
					MsgLog.info( "Switch to slave cancelled, unable to start HA-communication" );
					return null;
			  }

			  _masterDelegateHandler.Delegate = master;

			  haCommunicationLife.Add( updatePullerScheduler );
			  haCommunicationLife.Add( server );
			  haCommunicationLife.Start();

			  /*
			   * Take the opportunity to catch up with master, now that we're alone here, right before we
			   * drop the availability guard, so that other transactions might start.
			   */
			  if ( !CatchUpWithMaster( UpdatePuller ) )
			  {
					return null;
			  }

			  URI slaveHaURI = CreateHaURI( me, server );
			  _clusterMemberAvailability.memberIsAvailable( HighAvailabilityModeSwitcher.SLAVE, slaveHaURI, storeId );

			  return slaveHaURI;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean catchUpWithMaster(org.neo4j.kernel.ha.UpdatePuller updatePuller) throws IllegalArgumentException, InterruptedException
		 private bool CatchUpWithMaster( UpdatePuller updatePuller )
		 {
			  Monitor.catchupStarted();
			  RequestContext catchUpRequestContext = RequestContextFactory.newRequestContext();
			  UserLog.info( "Catching up with master. I'm at %s", catchUpRequestContext );

			  if ( !updatePuller.TryPullUpdates() )
			  {
					return false;
			  }

			  UserLog.info( "Now caught up with master" );
			  Monitor.catchupCompleted();
			  return true;
		 }

		 private URI CreateHaURI<T1>( URI me, Server<T1> server )
		 {
			  InetSocketAddress serverSocketAddress = server.SocketAddress;
			  string hostString = ServerUtil.getHostString( serverSocketAddress );

			  string host = IsWildcard( hostString ) ? me.Host : hostString;
			  host = EnsureWrapForIpv6Uri( host );

			  InstanceId serverId = Config.get( ClusterSettings.server_id );
			  return URI.create( "ha://" + host + ":" + serverSocketAddress.Port + "?serverId=" + serverId );
		 }

		 private static string EnsureWrapForIpv6Uri( string host )
		 {
			  if ( host.Contains( ":" ) && !host.Contains( "[" ) )
			  {
					host = "[" + host + "]";
			  }
			  return host;
		 }

		 private static bool IsWildcard( string hostString )
		 {
			  return hostString.Contains( "0.0.0.0" ) || hostString.Contains( "::" ) || hostString.Contains( "0:0:0:0:0:0:0:0" );
		 }

		 internal virtual MasterClient NewMasterClient( URI masterUri, URI me, StoreId storeId, LifeSupport life )
		 {
			  return _masterClientResolver.instantiate( masterUri.Host, masterUri.Port, me.Host, Monitors, storeId, life );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServicesAgain() throws Throwable
		 private void StartServicesAgain()
		 {
			  MsgLog.debug( "Starting services again" );
			  foreach ( Type serviceClass in SwitchToSlave._servicesToRestartForStoreCopy )
			  {
					ResolveDatabaseDependency( serviceClass ).start();
			  }
		 }

		 internal virtual void CheckDataConsistencyWithMaster( URI availableMasterId, Master master, StoreId storeId, TransactionIdStore transactionIdStore )
		 {
			  TransactionId myLastCommittedTxData = transactionIdStore.LastCommittedTransaction;
			  long myLastCommittedTx = myLastCommittedTxData.TransactionIdConflict();
			  HandshakeResult handshake;
			  try
			  {
					  using ( Response<HandshakeResult> response = master.Handshake( myLastCommittedTx, storeId ) )
					  {
						handshake = response.ResponseConflict();
						RequestContextFactory.Epoch = handshake.Epoch();
					  }
			  }
			  catch ( BranchedDataException e )
			  {
					// Rethrow wrapped in a branched data exception on our side, to clarify where the problem originates.
					throw new BranchedDataException( "The database stored on this machine has diverged from that " + "of the master. This will be automatically resolved.", e );
			  }
			  catch ( Exception e )
			  {
					// Checked exceptions will be wrapped as the cause if this was a serialized
					// server-side exception
					if ( e.InnerException is MissingLogDataException )
					{
						 /*
						  * This means the master was unable to find a log entry for the txid we just asked. This
						  * probably means the thing we asked for is too old or too new. Anyway, since it doesn't
						  * have the tx it is better if we just throw our store away and ask for a new copy. Next
						  * time around it shouldn't have to even pass from here.
						  */
						 throw new StoreOutOfDateException( "The master is missing the log required to complete the " + "consistency check", e.InnerException );
					}
					throw e;
			  }

			  long myChecksum = myLastCommittedTxData.Checksum();
			  if ( myChecksum != handshake.TxChecksum() )
			  {
					string msg = "The cluster contains two logically different versions of the database.. This will be " +
							  "automatically resolved. Details: I (server_id:" + Config.get( ClusterSettings.server_id ) +
							  ") think checksum for txId (" + myLastCommittedTx + ") is " + myChecksum +
							  ", but master (server_id:" + getServerId( availableMasterId ) + ") says that it's " +
							  handshake.TxChecksum() + ", where handshake is " + handshake;
					throw new BranchedDataException( msg );
			  }
			  MsgLog.info( "Checksum for last committed tx ok with lastTxId=" + myLastCommittedTx + " with checksum=" + myChecksum );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyStoreFromMasterIfNeeded(java.net.URI masterUri, java.net.URI me, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable
		 private void CopyStoreFromMasterIfNeeded( URI masterUri, URI me, CancellationRequest cancellationRequest )
		 {
			  if ( !isStorePresent( PageCache, DatabaseLayout ) )
			  {
					bool success = false;
					Monitor.storeCopyStarted();
					LifeSupport copyLife = new LifeSupport();
					try
					{
						 MasterClient masterClient = NewMasterClient( masterUri, me, null, copyLife );
						 copyLife.Start();

						 bool masterIsOld = Neo4Net.Kernel.ha.com.slave.MasterClient_Fields.Current.CompareTo( masterClient.ProtocolVersion ) > 0;
						 if ( masterIsOld )
						 {
							  throw new UnableToCopyStoreFromOldMasterException( Neo4Net.Kernel.ha.com.slave.MasterClient_Fields.Current.ApplicationProtocol, masterClient.ProtocolVersion.ApplicationProtocol );
						 }
						 else
						 {
							  CopyStoreFromMaster( masterClient, cancellationRequest, MoveAfterCopy.moveReplaceExisting() );
							  success = true;
						 }
					}
					finally
					{
						 Monitor.storeCopyCompleted( success );
						 copyLife.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean executeConsistencyChecks(org.neo4j.kernel.impl.transaction.log.TransactionIdStore txIdStore, java.net.URI masterUri, java.net.URI me, org.neo4j.storageengine.api.StoreId storeId, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable
		 private bool ExecuteConsistencyChecks( TransactionIdStore txIdStore, URI masterUri, URI me, StoreId storeId, CancellationRequest cancellationRequest )
		 {
			  LifeSupport consistencyCheckLife = new LifeSupport();
			  try
			  {
					MasterClient masterClient = NewMasterClient( masterUri, me, storeId, consistencyCheckLife );
					consistencyCheckLife.Start();

					if ( cancellationRequest.CancellationRequested() )
					{
						 return false;
					}

					CheckDataConsistency( masterClient, txIdStore, storeId, masterUri, me, cancellationRequest );
			  }
			  finally
			  {
					consistencyCheckLife.Shutdown();
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void checkDataConsistency(org.neo4j.kernel.ha.com.slave.MasterClient masterClient, org.neo4j.kernel.impl.transaction.log.TransactionIdStore txIdStore, org.neo4j.storageengine.api.StoreId storeId, java.net.URI masterUri, java.net.URI me, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable;
		 internal abstract void CheckDataConsistency( MasterClient masterClient, TransactionIdStore txIdStore, StoreId storeId, URI masterUri, URI me, CancellationRequest cancellationRequest );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void cleanStoreDir() throws java.io.IOException
		 internal virtual void CleanStoreDir()
		 {
			  // Tests verify that this method is called
			  StoreUtil.cleanStoreDir( DatabaseLayout.databaseDirectory() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopServices() throws Exception
		 internal virtual void StopServices()
		 {
			  MsgLog.debug( "Stopping services to handle branched store" );
			  AwaitDatabaseStart();
			  for ( int i = _servicesToRestartForStoreCopy.Length - 1; i >= 0; i-- )
			  {
					Type serviceClass = _servicesToRestartForStoreCopy[i];
					try
					{
						 ResolveDatabaseDependency( serviceClass ).stop();
					}
					catch ( Exception exception )
					{
						 throw exception;
					}
					catch ( Exception throwable )
					{
						 throw new Exception( "Unexpected error while stopping services to handle branched data", throwable );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitDatabaseStart() throws InterruptedException
		 private void AwaitDatabaseStart()
		 {
			  DatabaseAvailability databaseAvailability = ResolveDatabaseDependency( typeof( DatabaseAvailability ) );
			  while ( !databaseAvailability.Started )
			  {
					TimeUnit.MILLISECONDS.sleep( 10 );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copyStoreFromMaster(org.neo4j.kernel.ha.com.slave.MasterClient masterClient, org.neo4j.helpers.CancellationRequest cancellationRequest, org.neo4j.com.storecopy.MoveAfterCopy moveAfterCopy) throws Throwable
		 internal virtual void CopyStoreFromMaster( MasterClient masterClient, CancellationRequest cancellationRequest, MoveAfterCopy moveAfterCopy )
		 {
			  try
			  {
					UserLog.info( "Copying store from master" );
					StoreCopyClient.StoreCopyRequester requester = new StoreCopyRequesterAnonymousInnerClass( this, masterClient );
					MoveAfterCopy moveAfterCopyWithLogging = ( moves, fromDirectory, toDirectory ) =>
					{
					 UserLog.info( "Copied store from master to " + fromDirectory );
					 MsgLog.info( "Starting post copy operation to move store from " + fromDirectory + " to " + toDirectory );
					 moveAfterCopy.Move( moves, fromDirectory, toDirectory );
					};
					_storeCopyClient.copyStore( requester, cancellationRequest, moveAfterCopyWithLogging );

					StartServicesAgain();
					UserLog.info( "Finished copying store from master" );
			  }
			  catch ( Exception t )
			  {
					// Delete store so that we can copy from master without conflicts next time
					CleanStoreDir();
					throw t;
			  }
		 }

		 private class StoreCopyRequesterAnonymousInnerClass : StoreCopyClient.StoreCopyRequester
		 {
			 private readonly SwitchToSlave _outerInstance;

			 private MasterClient _masterClient;

			 public StoreCopyRequesterAnonymousInnerClass( SwitchToSlave outerInstance, MasterClient masterClient )
			 {
				 this.outerInstance = outerInstance;
				 this._masterClient = masterClient;
			 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.com.Response<?> copyStore(org.neo4j.com.storecopy.StoreWriter writer)
			 public Response<object> copyStore( StoreWriter writer )
			 {
				  return _masterClient.copyStore( new RequestContext( 0, _outerInstance.config.get( ClusterSettings.server_id ).toIntegerIndex(), 0, BASE_TX_ID, 0 ), writer );
			 }

			 public void done()
			 { // Nothing to clean up here
			 }
		 }

		 /// <summary>
		 /// Monitors events in <seealso cref="SwitchToSlave"/>
		 /// </summary>
		 public interface Monitor
		 {
			  /// <summary>
			  /// Called before any other slave-switching code is executed.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void switchToSlaveStarted()
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// Called after all slave-switching code has been executed, regardless of whether it was successful or not.
			  /// </summary>
			  /// <param name="wasSuccessful"> whether or not the slave switch was successful. Depending on the type of failure
			  /// other failure handling outside this class kicks in and there may be a switch retry later. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void switchToSlaveCompleted(boolean wasSuccessful)
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// A full store-copy is required, either if this is the first time this db starts up or if this
			  /// store has branched and needs to fetch a new copy from master.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void storeCopyStarted()
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// A full store-copy has completed.
			  /// </summary>
			  /// <param name="wasSuccessful"> whether or not this store-copy was successful. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void storeCopyCompleted(boolean wasSuccessful)
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// After a successful handshake with master an optimized catch-up is performed.
			  /// This call marks the start of that.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void catchupStarted()
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// This db is now caught up with the master.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void catchupCompleted()
	//		  { // no-op by default
	//		  }
		 }
	}

}
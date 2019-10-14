using System.Collections.Generic;

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

	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using MoveAfterCopy = Neo4Net.com.storecopy.MoveAfterCopy;
	using StoreCopyClient = Neo4Net.com.storecopy.StoreCopyClient;
	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using CancellationRequest = Neo4Net.Helpers.CancellationRequest;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using MasterClient = Neo4Net.Kernel.ha.com.slave.MasterClient;
	using MasterClientResolver = Neo4Net.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveServer = Neo4Net.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Neo4Net.Kernel.ha.id.HaIdGeneratorFactory;
	using ForeignStoreException = Neo4Net.Kernel.ha.store.ForeignStoreException;
	using UnableToCopyStoreFromOldMasterException = Neo4Net.Kernel.ha.store.UnableToCopyStoreFromOldMasterException;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class SwitchToSlaveCopyThenBranch : SwitchToSlave
	{
		 private readonly LogService _logService;

		 public SwitchToSlaveCopyThenBranch<T1>( DatabaseLayout databaseLayout, LogService logService, FileSystemAbstraction fileSystemAbstraction, Config config, HaIdGeneratorFactory idGeneratorFactory, DelegateInvocationHandler<Master> masterDelegateHandler, ClusterMemberAvailability clusterMemberAvailability, RequestContextFactory requestContextFactory, PullerFactory pullerFactory, IEnumerable<T1> kernelExtensions, MasterClientResolver masterClientResolver, Monitor monitor, StoreCopyClientMonitor storeCopyMonitor, System.Func<NeoStoreDataSource> neoDataSourceSupplier, System.Func<TransactionIdStore> transactionIdStoreSupplier, System.Func<Slave, SlaveServer> slaveServerFactory, UpdatePuller updatePuller, PageCache pageCache, Monitors monitors, System.Func<DatabaseTransactionStats> transactionStatsSupplier ) : this( databaseLayout, logService, config, idGeneratorFactory, masterDelegateHandler, clusterMemberAvailability, requestContextFactory, pullerFactory, masterClientResolver, monitor, new StoreCopyClient( databaseLayout, config, kernelExtensions, logService.UserLogProvider, fileSystemAbstraction, pageCache, storeCopyMonitor, false ), neoDataSourceSupplier, transactionIdStoreSupplier, slaveServerFactory, updatePuller, pageCache, monitors, transactionStatsSupplier )
		 {
		 }

		 internal SwitchToSlaveCopyThenBranch( DatabaseLayout databaseLayout, LogService logService, Config config, HaIdGeneratorFactory idGeneratorFactory, DelegateInvocationHandler<Master> masterDelegateHandler, ClusterMemberAvailability clusterMemberAvailability, RequestContextFactory requestContextFactory, PullerFactory pullerFactory, MasterClientResolver masterClientResolver, SwitchToSlave.Monitor monitor, StoreCopyClient storeCopyClient, System.Func<NeoStoreDataSource> neoDataSourceSupplier, System.Func<TransactionIdStore> transactionIdStoreSupplier, System.Func<Slave, SlaveServer> slaveServerFactory, UpdatePuller updatePuller, PageCache pageCache, Monitors monitors, System.Func<DatabaseTransactionStats> transactionStatsSupplier ) : base( idGeneratorFactory, monitors, requestContextFactory, masterDelegateHandler, clusterMemberAvailability, masterClientResolver, monitor, pullerFactory, updatePuller, slaveServerFactory, config, logService, pageCache, databaseLayout, transactionIdStoreSupplier, transactionStatsSupplier, neoDataSourceSupplier, storeCopyClient )
		 {
			  this._logService = logService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkDataConsistency(org.neo4j.kernel.ha.com.slave.MasterClient masterClient, org.neo4j.kernel.impl.transaction.log.TransactionIdStore txIdStore, org.neo4j.storageengine.api.StoreId storeId, java.net.URI masterUri, java.net.URI me, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable
		 internal override void CheckDataConsistency( MasterClient masterClient, TransactionIdStore txIdStore, StoreId storeId, URI masterUri, URI me, CancellationRequest cancellationRequest )
		 {
			  try
			  {
					UserLog.info( "Checking store consistency with master" );
					CheckMyStoreIdAndMastersStoreId( storeId, masterUri );
					CheckDataConsistencyWithMaster( masterUri, masterClient, storeId, txIdStore );
					UserLog.info( "Store is consistent" );
			  }
			  catch ( StoreUnableToParticipateInClusterException upe )
			  {
					UserLog.info( "The store is inconsistent. Will treat it as branched and fetch a new one from the master" );
					MsgLog.warn( "Current store is unable to participate in the cluster; fetching new store from master", upe );
					try
					{
						 StopServicesAndHandleBranchedStore( Config.get( HaSettings.branched_data_policy ), masterUri, me, cancellationRequest );
					}
					catch ( IOException e )
					{
						 MsgLog.warn( "Failed while trying to handle branched data", e );
					}

					throw upe;
			  }
			  catch ( MismatchingStoreIdException e )
			  {
					UserLog.info( "The store does not represent the same database as master. Will remove and fetch a new one from " + "master" );
					if ( txIdStore.LastCommittedTransactionId == BASE_TX_ID )
					{
						 MsgLog.warn( "Found and deleting empty store with mismatching store id", e );
						 StopServicesAndHandleBranchedStore( BranchedDataPolicy.keep_none, masterUri, me, cancellationRequest );
						 throw e;
					}

					MsgLog.error( "Store cannot participate in cluster due to mismatching store IDs", e );
					throw new ForeignStoreException( e.Expected, e.Encountered );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopServicesAndHandleBranchedStore(org.neo4j.kernel.ha.BranchedDataPolicy branchPolicy, java.net.URI masterUri, java.net.URI me, org.neo4j.helpers.CancellationRequest cancellationRequest) throws Throwable
		 internal virtual void StopServicesAndHandleBranchedStore( BranchedDataPolicy branchPolicy, URI masterUri, URI me, CancellationRequest cancellationRequest )
		 {
			  MoveAfterCopy moveWithCopyThenBranch = ( moves, fromDirectory, toDirectory ) =>
			  {
				StopServices();

				MsgLog.debug( "Branching store: " + DatabaseLayout.databaseDirectory() );
				branchPolicy.handle( DatabaseLayout.databaseDirectory(), PageCache, _logService );

				MsgLog.debug( "Moving downloaded store from " + fromDirectory + " to " + toDirectory );
				MoveAfterCopy.moveReplaceExisting().move(moves, fromDirectory, toDirectory);
				MsgLog.debug( "Moved downloaded store from " + fromDirectory + " to " + toDirectory );
			  };
			  CopyStore( masterUri, me, cancellationRequest, moveWithCopyThenBranch );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyStore(java.net.URI masterUri, java.net.URI me, org.neo4j.helpers.CancellationRequest cancellationRequest, org.neo4j.com.storecopy.MoveAfterCopy moveAfterCopy) throws Throwable
		 private void CopyStore( URI masterUri, URI me, CancellationRequest cancellationRequest, MoveAfterCopy moveAfterCopy )
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
						 CopyStoreFromMaster( masterClient, cancellationRequest, moveAfterCopy );
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

}
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
namespace Neo4Net.Kernel.ha.cluster
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using ServerUtil = Neo4Net.com.ServerUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.ha;
	using ConversationManager = Neo4Net.Kernel.ha.com.master.ConversationManager;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using MasterServer = Neo4Net.Kernel.ha.com.master.MasterServer;
	using SlaveFactory = Neo4Net.Kernel.ha.com.master.SlaveFactory;
	using HaIdGeneratorFactory = Neo4Net.Kernel.ha.id.HaIdGeneratorFactory;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;

	public class SwitchToMaster : IDisposable
	{
		 internal System.Func<Locks, ConversationManager> ConversationManagerFactory;
		 internal System.Func<ConversationManager, LifeSupport, Master> MasterFactory;
		 internal System.Func<Master, ConversationManager, MasterServer> MasterServerFactory;
		 private Log _userLog;
		 private HaIdGeneratorFactory _idGeneratorFactory;
		 private Config _config;
		 private System.Func<SlaveFactory> _slaveFactorySupplier;
		 private DelegateInvocationHandler<Master> _masterDelegateHandler;
		 private ClusterMemberAvailability _clusterMemberAvailability;
		 private System.Func<NeoStoreDataSource> _dataSourceSupplier;

		 public SwitchToMaster( LogService logService, HaIdGeneratorFactory idGeneratorFactory, Config config, System.Func<SlaveFactory> slaveFactorySupplier, System.Func<Locks, ConversationManager> conversationManagerFactory, System.Func<ConversationManager, LifeSupport, Master> masterFactory, System.Func<Master, ConversationManager, MasterServer> masterServerFactory, DelegateInvocationHandler<Master> masterDelegateHandler, ClusterMemberAvailability clusterMemberAvailability, System.Func<NeoStoreDataSource> dataSourceSupplier )
		 {
			  this.ConversationManagerFactory = conversationManagerFactory;
			  this.MasterFactory = masterFactory;
			  this.MasterServerFactory = masterServerFactory;
			  this._userLog = logService.GetUserLog( this.GetType() );
			  this._idGeneratorFactory = idGeneratorFactory;
			  this._config = config;
			  this._slaveFactorySupplier = slaveFactorySupplier;
			  this._masterDelegateHandler = masterDelegateHandler;
			  this._clusterMemberAvailability = clusterMemberAvailability;
			  this._dataSourceSupplier = dataSourceSupplier;
		 }

		 /// <summary>
		 /// Performs a switch to the master state. Starts communication endpoints, switches components to the master state
		 /// and broadcasts the appropriate Master Is Available event. </summary>
		 /// <param name="haCommunicationLife"> The LifeSupport instance to register communication endpoints. </param>
		 /// <param name="me"> The URI that the communication endpoints should bind to </param>
		 /// <returns> The URI at which the master communication was bound. </returns>
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual URI SwitchToMasterConflict( LifeSupport haCommunicationLife, URI me )
		 {
			  _userLog.info( "I am %s, moving to master", MyId( _config ) );

			  // Do not wait for currently active transactions to complete before continuing switching.
			  // - A master in a cluster is very important, without it the cluster cannot process any write requests
			  // - Awaiting open transactions to complete assumes that this instance just now was a slave that is
			  //   switching to master, which means the previous master where these active transactions were hosted
			  //   is no longer available so these open transactions cannot continue and complete anyway,
			  //   so what's the point waiting for them?
			  // - Read transactions may still be able to complete, but the correct response to failures in those
			  //   is to have them throw transient error exceptions hinting that they should be retried,
			  //   at which point they may get redirected to another instance, or to this instance if it has completed
			  //   the switch until then.

			  _idGeneratorFactory.switchToMaster();
			  NeoStoreDataSource dataSource = _dataSourceSupplier.get();
			  dataSource.AfterModeSwitch();

			  Locks locks = dataSource.DependencyResolver.resolveDependency( typeof( Locks ) );
			  ConversationManager conversationManager = ConversationManagerFactory.apply( locks );
			  Master master = MasterFactory.apply( conversationManager, haCommunicationLife );

			  MasterServer masterServer = MasterServerFactory.apply( master, conversationManager );

			  haCommunicationLife.Add( masterServer );
			  _masterDelegateHandler.Delegate = master;

			  haCommunicationLife.Start();

			  URI masterHaURI = GetMasterUri( me, masterServer, _config );
			  _clusterMemberAvailability.memberIsAvailable( MASTER, masterHaURI, dataSource.StoreId );
			  _userLog.info( "I am %s, successfully moved to master", MyId( _config ) );

			  _slaveFactorySupplier.get().StoreId = dataSource.StoreId;

			  return masterHaURI;
		 }

		 internal static URI GetMasterUri( URI me, MasterServer masterServer, Config config )
		 {
			  string hostname = config.Get( HaSettings.ha_server ).Host;
			  InetSocketAddress masterSocketAddress = masterServer.SocketAddress;

			  if ( string.ReferenceEquals( hostname, null ) || IsWildcard( hostname ) )
			  {
					InetAddress masterAddress = masterSocketAddress.Address;
					hostname = masterAddress.AnyLocalAddress ? me.Host : ServerUtil.getHostString( masterSocketAddress );
					hostname = EnsureWrapForIPv6Uri( hostname );
			  }

			  return URI.create( "ha://" + hostname + ":" + masterSocketAddress.Port + "?serverId=" + MyId( config ) );
		 }

		 private static string EnsureWrapForIPv6Uri( string hostname )
		 {
			  if ( hostname.Contains( ":" ) && !hostname.Contains( "[" ) )
			  {
					hostname = "[" + hostname + "]";
			  }
			  return hostname;
		 }

		 private static bool IsWildcard( string hostname )
		 {
			  return hostname.Contains( "0.0.0.0" ) || hostname.Contains( "[::]" ) || hostname.Contains( "[0:0:0:0:0:0:0:0]" );
		 }

		 private static InstanceId MyId( Config config )
		 {
			  return config.Get( ClusterSettings.server_id );
		 }

		 public override void Close()
		 {
			  _userLog = null;
			  ConversationManagerFactory = null;
			  MasterFactory = null;
			  MasterServerFactory = null;
			  _idGeneratorFactory = null;
			  _config = null;
			  _slaveFactorySupplier = null;
			  _masterDelegateHandler = null;
			  _clusterMemberAvailability = null;
			  _dataSourceSupplier = null;
		 }
	}

}
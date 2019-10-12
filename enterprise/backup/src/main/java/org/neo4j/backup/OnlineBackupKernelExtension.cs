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
namespace Org.Neo4j.backup
{

	using BackupImpl = Org.Neo4j.backup.impl.BackupImpl;
	using BackupServer = Org.Neo4j.backup.impl.BackupServer;
	using BindingListener = Org.Neo4j.cluster.BindingListener;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using BindingNotifier = Org.Neo4j.cluster.com.BindingNotifier;
	using ClusterMemberAvailability = Org.Neo4j.cluster.member.ClusterMemberAvailability;
	using ClusterMemberEvents = Org.Neo4j.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Org.Neo4j.cluster.member.ClusterMemberListener;
	using ServerUtil = Org.Neo4j.com.ServerUtil;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using StoreCopyServer = Org.Neo4j.com.storecopy.StoreCopyServer;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using LogFileInformation = Org.Neo4j.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_server;

	/// @deprecated This will be moved to an internal package in the future. 
	[Obsolete("This will be moved to an internal package in the future.")]
	public class OnlineBackupKernelExtension : LifecycleAdapter
	{
		 private object _startBindingListener;
		 private object _bindingListener;

		 [Obsolete]
		 public interface BackupProvider
		 {
			  TheBackupInterface NewBackup();
		 }

		 // This is the role used to announce that a cluster member can handle backups
		 public const string BACKUP = "backup";
		 // In this context, the IPv4 zero-address is understood as "any address on this host."
		 public const string INADDR_ANY = "0.0.0.0";

		 private readonly Config _config;
		 private readonly GraphDatabaseAPI _graphDatabaseAPI;
		 private readonly LogProvider _logProvider;
		 private readonly Monitors _monitors;
		 private BackupServer _server;
		 private readonly BackupProvider _backupProvider;
		 private volatile URI _me;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public OnlineBackupKernelExtension(org.neo4j.kernel.configuration.Config config, final org.neo4j.kernel.internal.GraphDatabaseAPI graphDatabaseAPI, final org.neo4j.logging.LogProvider logProvider, final org.neo4j.kernel.monitoring.Monitors monitors, final org.neo4j.kernel.NeoStoreDataSource neoStoreDataSource, final org.neo4j.io.fs.FileSystemAbstraction fileSystemAbstraction)
		 public OnlineBackupKernelExtension( Config config, GraphDatabaseAPI graphDatabaseAPI, LogProvider logProvider, Monitors monitors, NeoStoreDataSource neoStoreDataSource, FileSystemAbstraction fileSystemAbstraction ) : this(config, graphDatabaseAPI, () ->
		 {
		  {
				DependencyResolver dependencyResolver = graphDatabaseAPI.DependencyResolver;
				TransactionIdStore transactionIdStore = dependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
				StoreCopyServer copier = new StoreCopyServer( neoStoreDataSource, dependencyResolver.resolveDependency( typeof( CheckPointer ) ), fileSystemAbstraction, graphDatabaseAPI.DatabaseLayout().databaseDirectory(), monitors.NewMonitor(typeof(StoreCopyServer.Monitor)) );
				LogicalTransactionStore logicalTransactionStore = dependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );
				LogFileInformation logFileInformation = dependencyResolver.resolveDependency( typeof( LogFileInformation ) );
				return new BackupImpl( copier, logicalTransactionStore, transactionIdStore, logFileInformation, graphDatabaseAPI.storeId, logProvider );
		  }, monitors, logProvider);
		 }

		 private OnlineBackupKernelExtension( Config config, GraphDatabaseAPI graphDatabaseAPI, BackupProvider provider, Monitors monitors, LogProvider logProvider )
		 {
			  this._config = config;
			  this._graphDatabaseAPI = graphDatabaseAPI;
			  this._backupProvider = provider;
			  this._monitors = monitors;
			  this._logProvider = logProvider;
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  if ( _config.get( OnlineBackupSettings.online_backup_enabled ) )
				  {
						try
						{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							 _server = new BackupServer( _backupProvider.newBackup(), _config.get(online_backup_server), _logProvider, _monitors.newMonitor(typeof(ByteCounterMonitor), typeof(BackupServer).FullName), _monitors.newMonitor(typeof(RequestMonitor), typeof(BackupServer).FullName) );
							 _server.init();
							 _server.start();
      
							 try
							 {
								  _startBindingListener = new StartBindingListener( this );
								  _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ClusterMemberEvents ) ).addClusterMemberListener( ( ClusterMemberListener ) _startBindingListener );
      
								  _bindingListener = ( BindingListener ) myUri => _me = myUri;
								  _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( BindingNotifier ) ).addBindingListener( ( BindingListener ) _bindingListener );
							 }
							 catch ( Exception e ) when ( e is NoClassDefFoundError || e is UnsatisfiedDependencyException )
							 {
								  // Not running HA
							 }
						}
						catch ( Exception t )
						{
							 throw new Exception( t );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  if ( _server != null )
				  {
						_server.stop();
						_server.shutdown();
						_server = null;
      
						try
						{
							 _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ClusterMemberEvents ) ).removeClusterMemberListener( ( ClusterMemberListener ) _startBindingListener );
							 _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( BindingNotifier ) ).removeBindingListener( ( BindingListener ) _bindingListener );
      
							 ClusterMemberAvailability client = ClusterMemberAvailability;
							 client.MemberIsUnavailable( BACKUP );
						}
						catch ( Exception e ) when ( e is NoClassDefFoundError || e is UnsatisfiedDependencyException )
						{
							 // Not running HA
						}
				  }
			 }
		 }

		 private class StartBindingListener : Org.Neo4j.cluster.member.ClusterMemberListener_Adapter
		 {
			 private readonly OnlineBackupKernelExtension _outerInstance;

			 public StartBindingListener( OnlineBackupKernelExtension outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  public override void MemberIsAvailable( string role, InstanceId available, URI availableAtUri, StoreId storeId )
			  {
					if ( outerInstance.graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).ServerId.Equals( available ) && "master".Equals( role ) )
					{
						 {
						 // It was me and i am master - yey!
							  try
							  {
									URI backupUri = outerInstance.createBackupURI();
									ClusterMemberAvailability ha = outerInstance.ClusterMemberAvailability;
									ha.MemberIsAvailable( BACKUP, backupUri, storeId );
							  }
							  catch ( Exception t )
							  {
									throw new Exception( t );
							  }
						 }
					}
			  }

			  public override void MemberIsUnavailable( string role, InstanceId unavailableId )
			  {
					if ( outerInstance.graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).ServerId.Equals( unavailableId ) && "master".Equals( role ) )
					{
						 {
						 // It was me and i am master - yey!
							  try
							  {
									ClusterMemberAvailability ha = outerInstance.ClusterMemberAvailability;
									ha.MemberIsUnavailable( BACKUP );
							  }
							  catch ( Exception t )
							  {
									throw new Exception( t );
							  }
						 }
					}
			  }
		 }

		 private ClusterMemberAvailability ClusterMemberAvailability
		 {
			 get
			 {
				  return _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ClusterMemberAvailability ) );
			 }
		 }

		 private URI CreateBackupURI()
		 {
			  string hostString = ServerUtil.getHostString( _server.SocketAddress );
			  string host = hostString.Contains( INADDR_ANY ) ? _me.Host : hostString;
			  int port = _server.SocketAddress.Port;
			  return URI.create( "backup://" + host + ":" + port );
		 }
	}

}
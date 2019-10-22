using System;
using System.Collections.Generic;
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
namespace Neo4Net.cluster.client
{

	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// <summary>
	/// This service starts quite late, and is available for the instance to join as a member in the cluster.
	/// <para>
	/// On start it will try to join the cluster specified by the initial hosts. After start finishes it will have
	/// either joined an existing cluster or created a new one. On stop it will leave the cluster, but will fail
	/// and continue the stop after one minute.
	/// </para>
	/// </summary>
	public class ClusterJoin : LifecycleAdapter
	{
		 public interface Configuration
		 {
			  IList<HostnamePort> InitialHosts { get; }

			  string ClusterName { get; }

			  bool AllowedToCreateCluster { get; }

			  long ClusterJoinTimeout { get; }
		 }

		 private readonly Configuration _config;
		 private readonly ProtocolServer _protocolServer;
		 private readonly Log _userLog;
		 private readonly Log _messagesLog;
		 private Cluster _cluster;

		 public ClusterJoin( Configuration config, ProtocolServer protocolServer, LogService logService )
		 {
			  this._config = config;
			  this._protocolServer = protocolServer;
			  this._userLog = logService.GetUserLog( this.GetType() );
			  this._messagesLog = logService.GetInternalLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  _cluster = _protocolServer.newClient( typeof( Cluster ) );

			  JoinByConfig();
		 }

		 public override void Stop()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore semaphore = new java.util.concurrent.Semaphore(0);
			  Semaphore semaphore = new Semaphore( 0 );

			  _cluster.addClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, semaphore ) );

			  _cluster.leave();

			  try
			  {
					if ( !semaphore.tryAcquire( 60, TimeUnit.SECONDS ) )
					{
						 _messagesLog.info( "Unable to leave cluster, timeout" );
					}
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted();
					_messagesLog.warn( "Unable to leave cluster, interrupted", e );
			  }
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly ClusterJoin _outerInstance;

			 private Semaphore _semaphore;

			 public ClusterListener_AdapterAnonymousInnerClass( ClusterJoin outerInstance, Semaphore semaphore )
			 {
				 this.outerInstance = outerInstance;
				 this._semaphore = semaphore;
			 }

			 public override void leftCluster()
			 {
				  _outerInstance.cluster.removeClusterListener( this );
				  _semaphore.release();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void joinByConfig() throws java.util.concurrent.TimeoutException
		 private void JoinByConfig()
		 {
			  IList<HostnamePort> hosts = _config.InitialHosts;

			  _cluster.addClusterListener( new UnknownJoiningMemberWarning( this, hosts ) );

			  if ( hosts == null || hosts.Count == 0 )
			  {
					_userLog.info( "No cluster hosts specified. Creating cluster %s", _config.ClusterName );
					_cluster.create( _config.ClusterName );
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					URI[] memberURIs = hosts.Select( member => URI.create( "cluster://" + ResolvePortOnlyHost( member ) ) ).ToArray( URI[]::new );

					while ( true )
					{
						 _userLog.info( "Attempting to join cluster of %s", hosts.ToString() );
						 Future<ClusterConfiguration> clusterConfig = _cluster.join( this._config.ClusterName, memberURIs );

						 try
						 {
							  ClusterConfiguration clusterConf = _config.ClusterJoinTimeout > 0 ? clusterConfig.get( _config.ClusterJoinTimeout, TimeUnit.MILLISECONDS ) : clusterConfig.get();
							  _userLog.info( "Joined cluster: %s", clusterConf );
							  return;
						 }
						 catch ( InterruptedException )
						 {
							  _userLog.warn( "Could not join cluster, interrupted. Retrying..." );
						 }
						 catch ( ExecutionException e )
						 {
							  _messagesLog.debug( "Could not join cluster " + this._config.ClusterName );
							  if ( e.InnerException is System.InvalidOperationException )
							  {
									throw ( System.InvalidOperationException ) e.InnerException;
							  }

							  if ( _config.AllowedToCreateCluster )
							  {
									// Failed to join cluster, create new one
									_userLog.info( "Could not join cluster of %s", hosts.ToString() );
									_userLog.info( "Creating new cluster with name [%s]...", _config.ClusterName );
									_cluster.create( _config.ClusterName );
									break;
							  }

							  _userLog.warn( "Could not join cluster, timed out. Retrying..." );
						 }
					}
			  }
		 }

		 private string ResolvePortOnlyHost( HostnamePort host )
		 {
			  try
			  {
					return host.ToString( InetAddress.LocalHost.HostAddress );
			  }
			  catch ( UnknownHostException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private class UnknownJoiningMemberWarning : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly ClusterJoin _outerInstance;

			  internal readonly IList<HostnamePort> InitialHosts;

			  internal UnknownJoiningMemberWarning( ClusterJoin outerInstance, IList<HostnamePort> initialHosts )
			  {
				  this._outerInstance = outerInstance;
					this.InitialHosts = initialHosts;
			  }

			  public override void JoinedCluster( InstanceId member, URI uri )
			  {
					foreach ( HostnamePort host in InitialHosts )
					{
						 if ( host.Matches( uri ) )
						 {
							  return;
						 }
					}
					outerInstance.messagesLog.Info( "Member " + member + "(" + uri + ") joined cluster but was not part of initial hosts (" + InitialHosts + ")" );
			  }

			  public override void LeftCluster()
			  {
					outerInstance.cluster.RemoveClusterListener( this );
			  }
		 }
	}

}
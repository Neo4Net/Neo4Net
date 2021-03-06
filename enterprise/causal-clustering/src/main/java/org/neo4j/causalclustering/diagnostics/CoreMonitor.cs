﻿using System.Threading;

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
namespace Org.Neo4j.causalclustering.diagnostics
{

	using MembershipWaiter = Org.Neo4j.causalclustering.core.consensus.membership.MembershipWaiter;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;
	using PersistentSnapshotDownloader = Org.Neo4j.causalclustering.core.state.snapshot.PersistentSnapshotDownloader;
	using HazelcastCoreTopologyService = Org.Neo4j.causalclustering.discovery.HazelcastCoreTopologyService;
	using Limiters = Org.Neo4j.causalclustering.helper.Limiters;
	using ClusterBinder = Org.Neo4j.causalclustering.identity.ClusterBinder;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using SocketAddress = Org.Neo4j.Helpers.SocketAddress;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Monitors major clustering events and logs them appropriately. The main intention
	/// is for this class to make sure that the neo4j.log gets the most important events
	/// logged in a way that is useful for end users and aligned across components.
	/// <para>
	/// In particular the startup should be logged in a way as to aid in debugging
	/// common issues; e.g. around network connectivity.
	/// </para>
	/// <para>
	/// This pattern also de-clutters implementing classes from specifics of logging (e.g.
	/// formatting, dual-logging, rate limiting, ...) and encourages a structured interface.
	/// </para>
	/// </summary>
	public class CoreMonitor : ClusterBinder.Monitor, HazelcastCoreTopologyService.Monitor, PersistentSnapshotDownloader.Monitor, MembershipWaiter.Monitor
	{
		 private readonly Log _debug;
		 private readonly Log _user;

		 private readonly System.Action<ThreadStart> _binderLimit = Limiters.rateLimiter( Duration.ofSeconds( 10 ) );
		 private readonly System.Action<ThreadStart> _waiterLimit = Limiters.rateLimiter( Duration.ofSeconds( 10 ) );

		 public static void Register( LogProvider debugLogProvider, LogProvider userLogProvider, Monitors monitors )
		 {
			  new CoreMonitor( debugLogProvider, userLogProvider, monitors );
		 }

		 private CoreMonitor( LogProvider debugLogProvider, LogProvider userLogProvider, Monitors monitors )
		 {
			  this._debug = debugLogProvider.getLog( this.GetType() );
			  this._user = userLogProvider.getLog( this.GetType() );

			  monitors.AddMonitorListener( this );
		 }

		 public override void WaitingForCoreMembers( int minimumCount )
		 {
			  _binderLimit.accept(() =>
			  {
			  string message = "Waiting for a total of %d core members...";
			  _user.info( format( message, minimumCount ) );
			  });
		 }

		 public override void WaitingForBootstrap()
		 {
			  _binderLimit.accept( () => _user.info("Waiting for bootstrap by other instance...") );
		 }

		 public override void Bootstrapped( CoreSnapshot snapshot, ClusterId clusterId )
		 {
			  _user.info( "This instance bootstrapped the cluster." );
			  _debug.info( format( "Bootstrapped with snapshot: %s and clusterId: %s", snapshot, clusterId ) );
		 }

		 public override void BoundToCluster( ClusterId clusterId )
		 {
			  _user.info( "Bound to cluster with id " + clusterId.Uuid() );
		 }

		 public override void DiscoveredMember( SocketAddress socketAddress )
		 {
			  _user.info( "Discovered core member at " + socketAddress );
		 }

		 public override void LostMember( SocketAddress socketAddress )
		 {
			  _user.warn( "Lost core member at " + socketAddress );
		 }

		 public override void StartedDownloadingSnapshot()
		 {
			  _user.info( "Started downloading snapshot..." );
		 }

		 public override void DownloadSnapshotComplete()
		 {
			  _user.info( "Download of snapshot complete." );
		 }

		 public override void WaitingToHearFromLeader()
		 {
			  _waiterLimit.accept( () => _user.info("Waiting to hear from leader...") );
		 }

		 public override void WaitingToCatchupWithLeader( long localCommitIndex, long leaderCommitIndex )
		 {
			  _waiterLimit.accept(() =>
			  {
			  long gap = leaderCommitIndex - localCommitIndex;
			  _user.info( "Waiting to catchup with leader... we are %d entries behind leader at %d.", gap, leaderCommitIndex );
			  });
		 }

		 public override void JoinedRaftGroup()
		 {
			  _user.info( "Successfully joined the Raft group." );
		 }
	}

}
﻿using System;
using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using Node = Org.Neo4j.Graphdb.Node;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_frequency;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_rotation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.state_machine_flush_window_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataOnMemberEventuallyLooksLike;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.SampleData.createData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class CoreToCoreCopySnapshotIT
	{
		 protected internal const int NR_CORE_MEMBERS = 3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(NR_CORE_MEMBERS).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(NR_CORE_MEMBERS).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDownloadLargerFreshSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDownloadLargerFreshSnapshot()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  CoreClusterMember source = cluster.CoreTx((db, tx) =>
			  {
				createData( db, 1000 );
				tx.success();
			  });

			  // when
			  CoreClusterMember follower = cluster.AwaitCoreMemberWithRole( Role.FOLLOWER, 5, TimeUnit.SECONDS );

			  // shutdown the follower, remove the store, restart
			  follower.Shutdown();
			  DeleteDirectoryRecursively( follower.DatabaseDirectory(), follower.ServerId() );
			  DeleteDirectoryRecursively( follower.ClusterStateDirectory(), follower.ServerId() );
			  follower.Start();

			  // then
			  assertEquals( DbRepresentation.of( source.Database() ), DbRepresentation.of(follower.Database()) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void deleteDirectoryRecursively(java.io.File directory, int id) throws java.io.IOException
		 protected internal virtual void DeleteDirectoryRecursively( File directory, int id )
		 {
			  FileUtils.deleteRecursively( directory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDownloadToNewInstanceAfterPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDownloadToNewInstanceAfterPruning()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap( CausalClusteringSettings.state_machine_flush_window_size.name(), "1", CausalClusteringSettings.raft_log_pruning_strategy.name(), "3 entries", CausalClusteringSettings.raft_log_rotation_size.name(), "1K" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(params).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( @params ).startCluster();

			  CoreClusterMember leader = cluster.CoreTx((db, tx) =>
			  {
				createData( db, 10000 );
				tx.success();
			  });

			  // when
			  foreach ( CoreClusterMember coreDb in cluster.CoreMembers() )
			  {
					coreDb.RaftLogPruner().prune();
			  }

			  cluster.RemoveCoreMember( leader ); // to force a change of leader
			  leader = cluster.AwaitLeader();

			  int newDbId = 3;
			  cluster.AddCoreMemberWithId( newDbId ).start();
			  CoreGraphDatabase newDb = cluster.GetCoreMemberById( newDbId ).database();

			  // then
			  assertEquals( DbRepresentation.of( leader.Database() ), DbRepresentation.of(newDb) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDownloadToRejoinedInstanceAfterPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDownloadToRejoinedInstanceAfterPruning()
		 {
			  // given
			  IDictionary<string, string> coreParams = stringMap();
			  coreParams[raft_log_rotation_size.name()] = "1K";
			  coreParams[raft_log_pruning_strategy.name()] = "keep_none";
			  coreParams[raft_log_pruning_frequency.name()] = "100ms";
			  coreParams[state_machine_flush_window_size.name()] = "64";
			  int numberOfTransactions = 100;

			  // start the cluster
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(coreParams).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( coreParams ).startCluster();
			  Timeout timeout = new Timeout( this, Clocks.systemClock(), 120, SECONDS );

			  // accumulate some log files
			  int firstServerLogFileCount;
			  CoreClusterMember firstServer;
			  do
			  {
					timeout.AssertNotTimedOut();
					firstServer = DoSomeTransactions( cluster, numberOfTransactions );
					firstServerLogFileCount = GetMostRecentLogIdOn( firstServer );
			  } while ( firstServerLogFileCount < 5 );
			  firstServer.Shutdown();

			  /* After shutdown we wait until we accumulate enough logs, and so that enough of the old ones
			   * have been pruned, so that the rejoined instance won't be able to catch up to without a snapshot. */
			  int oldestLogOnSecondServer;
			  CoreClusterMember secondServer;
			  do
			  {
					timeout.AssertNotTimedOut();
					secondServer = DoSomeTransactions( cluster, numberOfTransactions );
					oldestLogOnSecondServer = GetOldestLogIdOn( secondServer );
			  } while ( oldestLogOnSecondServer < firstServerLogFileCount + 5 );

			  // when
			  firstServer.Start();

			  // then
			  dataOnMemberEventuallyLooksLike( firstServer, secondServer );
		 }

		 private class Timeout
		 {
			 private readonly CoreToCoreCopySnapshotIT _outerInstance;

			  internal readonly Clock Clock;
			  internal readonly long AbsoluteTimeoutMillis;

			  internal Timeout( CoreToCoreCopySnapshotIT outerInstance, Clock clock, long time, TimeUnit unit )
			  {
				  this._outerInstance = outerInstance;
					this.Clock = clock;
					this.AbsoluteTimeoutMillis = clock.millis() + unit.toMillis(time);
			  }

			  internal virtual void AssertNotTimedOut()
			  {
					if ( Clock.millis() > AbsoluteTimeoutMillis )
					{
						 throw new AssertionError( "Timed out" );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getOldestLogIdOn(org.neo4j.causalclustering.discovery.CoreClusterMember clusterMember) throws java.io.IOException
		 private int GetOldestLogIdOn( CoreClusterMember clusterMember )
		 {
			  return clusterMember.LogFileNames.firstKey().intValue();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getMostRecentLogIdOn(org.neo4j.causalclustering.discovery.CoreClusterMember clusterMember) throws java.io.IOException
		 private int GetMostRecentLogIdOn( CoreClusterMember clusterMember )
		 {
			  return clusterMember.LogFileNames.lastKey().intValue();
		 }

		 private CoreClusterMember DoSomeTransactions<T1>( Cluster<T1> cluster, int count )
		 {
			  try
			  {
					CoreClusterMember last = null;
					for ( int i = 0; i < count; i++ )
					{
						 last = cluster.CoreTx((db, tx) =>
						 {
						  Node node = Db.createNode();
						  node.setProperty( "that's a bam", String( 1024 ) );
						  tx.success();
						 });
					}
					return last;
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 private string String( int numberOfCharacters )
		 {
			  StringBuilder s = new StringBuilder();
			  for ( int i = 0; i < numberOfCharacters; i++ )
			  {
					s.Append( i.ToString() );
			  }
			  return s.ToString();
		 }

	}

}
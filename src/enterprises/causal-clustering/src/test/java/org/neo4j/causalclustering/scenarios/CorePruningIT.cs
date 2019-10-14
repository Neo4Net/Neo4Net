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
namespace Neo4Net.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.SampleData.createData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class CorePruningIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.state_machine_flush_window_size, "1").withSharedCoreParam(raft_log_pruning_strategy, "keep_none").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_rotation_size, "1K").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_frequency, "100ms");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.state_machine_flush_window_size, "1").withSharedCoreParam(raft_log_pruning_strategy, "keep_none").withSharedCoreParam(CausalClusteringSettings.raft_log_rotation_size, "1K").withSharedCoreParam(CausalClusteringSettings.raft_log_pruning_frequency, "100ms");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void actuallyDeletesTheFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ActuallyDeletesTheFiles()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  CoreClusterMember coreGraphDatabase = null;
			  int txs = 10;
			  for ( int i = 0; i < txs; i++ )
			  {
					coreGraphDatabase = cluster.CoreTx((db, tx) =>
					{
					 createData( db, 1 );
					 tx.success();
					});
			  }

			  // when pruning kicks in then some files are actually deleted
			  File raftLogDir = coreGraphDatabase.RaftLogDirectory();
			  int expectedNumberOfLogFilesAfterPruning = 2;
			  assertEventually( "raft logs eventually pruned", () => NumberOfFiles(raftLogDir), equalTo(expectedNumberOfLogFilesAfterPruning), 5, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPruneUncommittedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPruneUncommittedEntries()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  CoreClusterMember coreGraphDatabase = null;
			  int txs = 1000;
			  for ( int i = 0; i < txs; i++ )
			  {
					coreGraphDatabase = cluster.CoreTx( ( db, tx ) => createData( db, 1 ) );
			  }

			  // when pruning kicks in then some files are actually deleted
			  int expectedNumberOfLogFilesAfterPruning = 2;
			  File raftLogDir = coreGraphDatabase.RaftLogDirectory();
			  assertEventually( "raft logs eventually pruned", () => NumberOfFiles(raftLogDir), equalTo(expectedNumberOfLogFilesAfterPruning), 5, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int numberOfFiles(java.io.File raftLogDir) throws RuntimeException
		 private int NumberOfFiles( File raftLogDir )
		 {
			  return raftLogDir.list().length;
		 }
	}

}
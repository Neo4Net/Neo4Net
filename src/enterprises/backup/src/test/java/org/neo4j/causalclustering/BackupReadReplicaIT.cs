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
namespace Neo4Net.causalclustering
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Neo4Net.causalclustering.discovery;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using ConfigFileBuilder = Neo4Net.Server.configuration.ConfigFileBuilder;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupCoreIT.backupArguments;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupCoreIT.createSomeData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupCoreIT.getConfig;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.CausalClusteringTestHelpers.transactionAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.admin.AdminTool.STATUS_SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.awaitEx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupReadReplicaIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppress = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput Suppress = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withSharedCoreParam(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE).withNumberOfReadReplicas(1).withSharedReadReplicaParam(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.TRUE);
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withSharedCoreParam(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).withNumberOfReadReplicas(1).withSharedReadReplicaParam(OnlineBackupSettings.online_backup_enabled, Settings.TRUE);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private File _backupPath;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _backupPath = ClusterRule.testDirectory().cleanDirectory("backup-db");
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBackupCanBePerformed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureBackupCanBePerformed()
		 {
			  // Run backup
			  CoreGraphDatabase leader = createSomeData( _cluster );

			  ReadReplicaGraphDatabase readReplica = _cluster.findAnyReadReplica().database();

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(leader, readReplica), 1, TimeUnit.MINUTES );

			  DbRepresentation beforeChange = DbRepresentation.of( readReplica );
			  string backupAddress = transactionAddress( readReplica );

			  string[] args = backupArguments( backupAddress, _backupPath, "readreplica" );

			  File configFile = ConfigFileBuilder.builder( ClusterRule.clusterDirectory() ).build();
			  assertEquals( STATUS_SUCCESS, runBackupToolFromOtherJvmToGetExitCode( ClusterRule.clusterDirectory(), args ) );

			  // Add some new data
			  DbRepresentation afterChange = DbRepresentation.of( createSomeData( _cluster ) );

			  // Verify that backed up database can be started and compare representation
			  DbRepresentation backupRepresentation = DbRepresentation.of( DatabaseLayout.of( _backupPath, "readreplica" ).databaseDirectory(), Config );
			  assertEquals( beforeChange, backupRepresentation );
			  assertNotEquals( backupRepresentation, afterChange );
		 }

		 private static bool ReadReplicasUpToDateAsTheLeader( CoreGraphDatabase leader, ReadReplicaGraphDatabase readReplica )
		 {
			  long leaderTxId = leader.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;
			  long lastClosedTxId = readReplica.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;
			  return lastClosedTxId == leaderTxId;
		 }
	}

}
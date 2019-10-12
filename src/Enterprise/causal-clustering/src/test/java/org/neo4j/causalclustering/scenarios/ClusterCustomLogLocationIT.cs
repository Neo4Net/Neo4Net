using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.scenarios
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.logFilesBasedOnlyBuilder;

	public class ClusterCustomLogLocationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(2);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(2);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clusterWithCustomTransactionLogLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClusterWithCustomTransactionLogLocation()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  for ( int i = 0; i < 10; i++ )
			  {
					cluster.CoreTx((db, tx) =>
					{
					 Db.createNode();
					 tx.success();
					});
			  }

			  ICollection<CoreClusterMember> coreClusterMembers = cluster.CoreMembers();
			  foreach ( CoreClusterMember coreClusterMember in coreClusterMembers )
			  {
					DependencyResolver dependencyResolver = coreClusterMember.Database().DependencyResolver;
					LogFiles logFiles = dependencyResolver.ResolveDependency( typeof( LogFiles ) );
					assertEquals( logFiles.LogFilesDirectory().Name, "core-tx-logs-" + coreClusterMember.ServerId() );
					assertTrue( logFiles.HasAnyEntries( 0 ) );
					File[] coreLogDirectories = coreClusterMember.DatabaseDirectory().listFiles(file => file.Name.StartsWith("core"));
					assertThat( coreLogDirectories, Matchers.arrayWithSize( 1 ) );

					LogFileInStoreDirectoryDoesNotExist( coreClusterMember.DatabaseDirectory(), dependencyResolver );
			  }

			  ICollection<ReadReplica> readReplicas = cluster.ReadReplicas();
			  foreach ( ReadReplica readReplica in readReplicas )
			  {
					readReplica.TxPollingClient().upToDateFuture().get();
					DependencyResolver dependencyResolver = readReplica.Database().DependencyResolver;
					LogFiles logFiles = dependencyResolver.ResolveDependency( typeof( LogFiles ) );
					assertEquals( logFiles.LogFilesDirectory().Name, "replica-tx-logs-" + readReplica.ServerId() );
					assertTrue( logFiles.HasAnyEntries( 0 ) );
					File[] replicaLogDirectories = readReplica.DatabaseDirectory().listFiles(file => file.Name.StartsWith("replica"));
					assertThat( replicaLogDirectories, Matchers.arrayWithSize( 1 ) );

					LogFileInStoreDirectoryDoesNotExist( readReplica.DatabaseDirectory(), dependencyResolver );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void logFileInStoreDirectoryDoesNotExist(java.io.File storeDir, org.neo4j.graphdb.DependencyResolver dependencyResolver) throws java.io.IOException
		 private static void LogFileInStoreDirectoryDoesNotExist( File storeDir, DependencyResolver dependencyResolver )
		 {
			  FileSystemAbstraction fileSystem = dependencyResolver.ResolveDependency( typeof( FileSystemAbstraction ) );
			  LogFiles storeLogFiles = logFilesBasedOnlyBuilder( storeDir, fileSystem ).build();
			  assertFalse( storeLogFiles.VersionExists( 0 ) );
		 }
	}

}
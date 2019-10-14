using System;
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
namespace Neo4Net.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using Node = Neo4Net.Graphdb.Node;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class RecoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeConsistentAfterShutdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeConsistentAfterShutdown()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  FireSomeLoadAtTheCluster( cluster );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<File> storeDirs = cluster.CoreMembers().Select(CoreClusterMember::databaseDirectory).collect(toSet());

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  assertEventually( "All cores have the same data", () => cluster.CoreMembers().Select(RecoveryIT.dbRepresentation).collect(toSet()).size(), equalTo(1), 10, TimeUnit.SECONDS );

			  // when
			  cluster.Shutdown();

			  // then
			  storeDirs.forEach( RecoveryIT.assertConsistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void singleServerWithinClusterShouldBeConsistentAfterRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SingleServerWithinClusterShouldBeConsistentAfterRestart()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  int clusterSize = cluster.NumberOfCoreMembersReportedByTopology();

			  FireSomeLoadAtTheCluster( cluster );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<File> storeDirs = cluster.CoreMembers().Select(CoreClusterMember::databaseDirectory).collect(toSet());

			  // when
			  for ( int i = 0; i < clusterSize; i++ )
			  {
					cluster.RemoveCoreMemberWithServerId( i );
					FireSomeLoadAtTheCluster( cluster );
					cluster.AddCoreMemberWithId( i ).start();
			  }

			  // then
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  assertEventually( "All cores have the same data", () => cluster.CoreMembers().Select(RecoveryIT.dbRepresentation).collect(toSet()).size(), equalTo(1), 10, TimeUnit.SECONDS );

			  cluster.Shutdown();

			  storeDirs.forEach( RecoveryIT.assertConsistent );
		 }

		 private static DbRepresentation DbRepresentation( CoreClusterMember member )
		 {
			  return DbRepresentation.of( member.Database() );
		 }

		 private static void AssertConsistent( File storeDir )
		 {
			  ConsistencyCheckService.Result result;
			  try
			  {
					result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(DatabaseLayout.of(storeDir), Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, true);
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }

			  assertTrue( result.Successful );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void fireSomeLoadAtTheCluster(org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 private static void FireSomeLoadAtTheCluster<T1>( Cluster<T1> cluster )
		 {
			  for ( int i = 0; i < cluster.NumberOfCoreMembersReportedByTopology(); i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String prop = "val" + i;
					string prop = "val" + i;
					cluster.CoreTx((db, tx) =>
					{
					 Node node = Db.createNode( label( "demo" ) );
					 node.setProperty( "server", prop );
					 tx.success();
					});
			  }
		 }
	}

}
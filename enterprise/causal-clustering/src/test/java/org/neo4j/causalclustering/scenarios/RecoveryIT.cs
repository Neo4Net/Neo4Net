using System;
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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using Node = Org.Neo4j.Graphdb.Node;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

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
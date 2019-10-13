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
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ClassicNeo4jStore = Neo4Net.causalclustering.helpers.ClassicNeo4jStore;
	using Neo4Net.Functions;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using RestoreDatabaseCommand = Neo4Net.restore.RestoreDatabaseCommand;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_advertised_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ConvertNonCausalClusteringStoreIT
	public class ConvertNonCausalClusteringStoreIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public String recordFormat;
		 public string RecordFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "Record format {0}") public static java.util.Collection<Object> data()
		 public static ICollection<object> Data()
		 {
			  return Arrays.asList( new object[]{ Standard.LATEST_NAME, HighLimit.NAME } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransactionToCoreMembers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransactionToCoreMembers()
		 {
			  // given
			  TestDirectory testDirectory = ClusterRule.testDirectory();
			  File dbDir = testDirectory.CleanDirectory( "classic-db-" + RecordFormat );
			  int classicNodeCount = 1024;
			  File classicNeo4jStore = CreateNeoStore( dbDir, classicNodeCount );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = this.clusterRule.withRecordFormat(recordFormat).createCluster();
			  Cluster<object> cluster = this.ClusterRule.withRecordFormat( RecordFormat ).createCluster();

			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					foreach ( CoreClusterMember core in cluster.CoreMembers() )
					{
						 ( new RestoreDatabaseCommand( fileSystem, classicNeo4jStore, core.Config(), core.SettingValue(GraphDatabaseSettings.active_database.name()), true ) ).execute();
					}
			  }

			  cluster.Start();

			  // when
			  cluster.CoreTx((coreDB, tx) =>
			  {
				Node node = coreDB.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  cluster.AddReadReplicaWithIdAndRecordFormat( 4, RecordFormat ).start();

			  // then
			  foreach ( CoreClusterMember server in cluster.CoreMembers() )
			  {
					CoreGraphDatabase db = server.database();

					using ( Transaction tx = Db.beginTx() )
					{
						 ThrowingSupplier<long, Exception> nodeCount = () => count(Db.AllNodes);

						 Config config = Db.DependencyResolver.resolveDependency( typeof( Config ) );

						 assertEventually( "node to appear on core server " + config.Get( raft_advertised_address ), nodeCount, greaterThan( ( long ) classicNodeCount ), 15, SECONDS );

						 assertEquals( classicNodeCount + 1, count( Db.AllNodes ) );

						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNeoStore(java.io.File dbDir, int classicNodeCount) throws java.io.IOException
		 private File CreateNeoStore( File dbDir, int classicNodeCount )
		 {
			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					return ClassicNeo4jStore.builder( dbDir, fileSystem ).amountOfNodes( classicNodeCount ).recordFormats( RecordFormat ).build().StoreDir;
			  }
		 }
	}

}
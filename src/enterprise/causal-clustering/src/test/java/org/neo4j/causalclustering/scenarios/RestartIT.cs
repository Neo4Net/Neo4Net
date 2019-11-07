using System;
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
namespace Neo4Net.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using WriteOperationsNotAllowedException = Neo4Net.GraphDb.security.WriteOperationsNotAllowedException;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using AcquireLockTimeoutException = Neo4Net.Kernel.Api.StorageEngine.@lock.AcquireLockTimeoutException;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;

	public class RestartIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.causalclustering.ClusterRule clusterRule = new Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restartFirstServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestartFirstServer()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
			  cluster.RemoveCoreMemberWithServerId( 0 );
			  cluster.AddCoreMemberWithId( 0 ).start();

			  // then
			  cluster.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restartSecondServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestartSecondServer()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
			  cluster.RemoveCoreMemberWithServerId( 1 );
			  cluster.AddCoreMemberWithId( 1 ).start();

			  // then
			  cluster.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restartWhileDoingTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestartWhileDoingTransactions()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService coreDB = cluster.getCoreMemberById(0).database();
			  IGraphDatabaseService coreDB = cluster.GetCoreMemberById( 0 ).database();

			  ExecutorService executor = Executors.newCachedThreadPool();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean done = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean done = new AtomicBoolean( false );
			  executor.execute(() =>
			  {
				while ( !done.get() )
				{
					 try
					 {
						 using ( Transaction tx = coreDB.BeginTx() )
						 {
							  Node node = coreDB.CreateNode( label( "boo" ) );
							  node.setProperty( "foobar", "baz_bat" );
							  tx.success();
						 }
					 }
					 catch ( Exception e ) when ( e is AcquireLockTimeoutException || e is WriteOperationsNotAllowedException )
					 {
						  // expected sometimes
					 }
				}
			  });
			  Thread.Sleep( 500 );

			  cluster.RemoveCoreMemberWithServerId( 1 );
			  cluster.AddCoreMemberWithId( 1 ).start();
			  Thread.Sleep( 500 );

			  // then
			  done.set( true );
			  executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveWritableClusterAfterCompleteRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveWritableClusterAfterCompleteRestart()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  cluster.Shutdown();

			  // when
			  cluster.Start();

			  CoreClusterMember last = cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then
			  dataMatchesEventually( last, cluster.CoreMembers() );
			  cluster.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readReplicaTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadReplicaTest()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfCoreMembers(2).withNumberOfReadReplicas(1).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfCoreMembers( 2 ).withNumberOfReadReplicas( 1 ).startCluster();

			  // when
			  CoreClusterMember last = cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  cluster.AddCoreMemberWithId( 2 ).start();
			  dataMatchesEventually( last, cluster.CoreMembers() );
			  dataMatchesEventually( last, cluster.ReadReplicas() );

			  cluster.Shutdown();

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					foreach ( CoreClusterMember core in cluster.CoreMembers() )
					{
						 ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(DatabaseLayout.of(core.DatabaseDirectory()), Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, fileSystem, false, new ConsistencyFlags(true, true, true, true, false));
						 assertTrue( "Inconsistent: " + core, result.Successful );
					}

					foreach ( ReadReplica readReplica in cluster.ReadReplicas() )
					{
						 ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(DatabaseLayout.of(readReplica.DatabaseDirectory()), Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, fileSystem, false, new ConsistencyFlags(true, true, true, true, false));
						 assertTrue( "Inconsistent: " + readReplica, result.Successful );
					}
			  }
		 }
	}

}
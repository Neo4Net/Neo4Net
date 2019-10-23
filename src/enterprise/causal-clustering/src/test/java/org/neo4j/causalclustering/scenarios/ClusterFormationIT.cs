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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ClusterFormationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBuiltInProcedures()
		 public virtual void ShouldSupportBuiltInProcedures()
		 {
			  _cluster.addReadReplicaWithId( 0 ).start();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Stream.concat( _cluster.readReplicas().Select(ReadReplica::database), _cluster.coreMembers().Select(CoreClusterMember::database) ).forEach(gdb =>
			  {
				{
				// (1) BuiltInProcedures from community
					 Result result = gdb.execute( "CALL dbms.procedures()" );
					 assertTrue( result.hasNext() );
					 result.close();
				}

				// (2) BuiltInProcedures from enterprise
				using ( InternalTransaction tx = gdb.BeginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ) )
				{
					 Result result = gdb.execute( tx, "CALL dbms.listQueries()", EMPTY_MAP );
					 assertTrue( result.hasNext() );
					 result.close();

					 tx.success();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAddAndRemoveCoreMembers()
		 public virtual void ShouldBeAbleToAddAndRemoveCoreMembers()
		 {
			  // when
			  _cluster.getCoreMemberById( 0 ).shutdown();
			  _cluster.getCoreMemberById( 0 ).start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.removeCoreMemberWithServerId( 1 );

			  // then
			  assertEquals( 2, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.addCoreMemberWithId( 4 ).start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAddAndRemoveCoreMembersUnderModestLoad()
		 public virtual void ShouldBeAbleToAddAndRemoveCoreMembersUnderModestLoad()
		 {
			  // given
			  ExecutorService executorService = Executors.newSingleThreadExecutor();
			  executorService.submit(() =>
			  {
				CoreGraphDatabase leader = _cluster.getMemberWithRole( Role.LEADER ).database();
				using ( Transaction tx = leader.beginTx() )
				{
					 leader.createNode();
					 tx.success();
				}
			  });

			  // when
			  _cluster.getCoreMemberById( 0 ).shutdown();
			  _cluster.getCoreMemberById( 0 ).start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.removeCoreMemberWithServerId( 0 );

			  // then
			  assertEquals( 2, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.addCoreMemberWithId( 4 ).start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );

			  executorService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRestartTheCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRestartTheCluster()
		 {
			  // when started then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.shutdown();
			  _cluster.start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );

			  // when
			  _cluster.removeCoreMemberWithServerId( 1 );

			  _cluster.addCoreMemberWithId( 3 ).start();
			  _cluster.shutdown();

			  _cluster.start();

			  // then
			  assertEquals( 3, _cluster.numberOfCoreMembersReportedByTopology() );
		 }
	}

}
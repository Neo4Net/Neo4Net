﻿/*
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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ClusterFormationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
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
				using ( InternalTransaction tx = gdb.beginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ) )
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
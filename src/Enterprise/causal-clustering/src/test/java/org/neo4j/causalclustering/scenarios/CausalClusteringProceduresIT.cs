using System.Diagnostics;

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
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using Result = Neo4Net.Graphdb.Result;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.api.security.EnterpriseLoginContext.AUTH_DISABLED;

	public class CausalClusteringProceduresIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(2).withNumberOfReadReplicas(1);
		 public static readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(2).withNumberOfReadReplicas(1);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private static Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void coreProceduresShouldBeAvailable()
		 public virtual void CoreProceduresShouldBeAvailable()
		 {
			  string[] coreProcs = new string[]{ "dbms.cluster.role", "dbms.cluster.routing.getServers", "dbms.cluster.overview", "dbms.procedures", "dbms.listQueries" };

			  foreach ( string procedure in coreProcs )
			  {
					Optional<CoreClusterMember> firstCore = _cluster.coreMembers().First();
					Debug.Assert( firstCore.Present );
					CoreGraphDatabase database = firstCore.get().database();
					InternalTransaction tx = database.BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED );
					Result coreResult = database.Execute( "CALL " + procedure + "()" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "core with procedure " + procedure, coreResult.HasNext() );
					coreResult.Close();
					tx.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readReplicaProceduresShouldBeAvailable()
		 public virtual void ReadReplicaProceduresShouldBeAvailable()
		 {
			  // given
			  string[] readReplicaProcs = new string[]{ "dbms.cluster.role", "dbms.procedures", "dbms.listQueries" };

			  // when
			  foreach ( string procedure in readReplicaProcs )
			  {
					Optional<ReadReplica> firstReadReplica = _cluster.readReplicas().First();
					Debug.Assert( firstReadReplica.Present );
					ReadReplicaGraphDatabase database = firstReadReplica.get().database();
					InternalTransaction tx = database.BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED );
					Result readReplicaResult = database.Execute( "CALL " + procedure + "()" );

					// then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "read replica with procedure " + procedure, readReplicaResult.HasNext() );
					readReplicaResult.Close();
					tx.Close();
			  }
		 }
	}


}
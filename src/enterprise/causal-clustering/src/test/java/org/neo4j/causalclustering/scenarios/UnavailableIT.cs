using System;

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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status.statusCodeOf;

	public class UnavailableIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3);

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
//ORIGINAL LINE: @Test public void shouldReturnUnavailableStatusWhenDoingLongOperation()
		 public virtual void ShouldReturnUnavailableStatusWhenDoingLongOperation()
		 {
			  // given
			  ClusterMember member = _cluster.getCoreMemberById( 1 );

			  // when
			  member.database().DependencyResolver.resolveDependency(typeof(DatabaseAvailabilityGuard)).require(() => "Not doing long operation");

			  // then
			  try
			  {
					  using ( Transaction tx = member.database().BeginTx() )
					  {
						tx.Success();
						fail();
					  }
			  }
			  catch ( Exception e )
			  {
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, statusCodeOf( e ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUnavailableStatusWhenShutdown()
		 public virtual void ShouldReturnUnavailableStatusWhenShutdown()
		 {
			  // given
			  ClusterMember member = _cluster.getCoreMemberById( 1 );

			  // when
			  GraphDatabaseAPI db = member.database();
			  member.shutdown();

			  // then
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						tx.Success();
						fail();
					  }
			  }
			  catch ( Exception e )
			  {
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, statusCodeOf( e ) );
			  }
		 }
	}

}
using System.Collections.Generic;

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
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.causalclustering.discovery;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using Transaction_Type = Neo4Net.Kernel.Api.Internal.Transaction_Type;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.cluster_allow_reads_on_followers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ProcedureNames.GET_SERVERS_V1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClusterDiscoveryIT
	public class ClusterDiscoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String ignored;
		 public string Ignored; // <- JUnit is happy only if this is here!
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public java.util.Map<String,String> config;
		 public IDictionary<string, string> Config;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public boolean expectFollowersAsReadEndPoints;
		 public bool ExpectFollowersAsReadEndPoints;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> params()
		 public static ICollection<object[]> Params()
		 {
			  return Arrays.asList( new object[]{ "with followers as read end points", singletonMap( cluster_allow_reads_on_followers.name(), Settings.TRUE ), true }, new object[]{ "no followers as read end points", singletonMap(cluster_allow_reads_on_followers.name(), Settings.FALSE), false } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindReadWriteAndRouteServers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindReadWriteAndRouteServers()
		 {
			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(config).withNumberOfReadReplicas(1).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( Config ).withNumberOfReadReplicas( 1 ).startCluster();

			  // then
			  int cores = cluster.CoreMembers().Count;
			  int readReplicas = cluster.ReadReplicas().Count;
			  int readEndPoints = ExpectFollowersAsReadEndPoints ? ( cores - 1 + readReplicas ) : readReplicas;
			  for ( int i = 0; i < 3; i++ )
			  {
					IList<IDictionary<string, object>> members = GetMembers( cluster.GetCoreMemberById( i ).database() );

					assertEquals( 1, members.Where( x => x.get( "role" ).Equals( "WRITE" ) ).flatMap( x => Arrays.stream( ( object[] ) x.get( "addresses" ) ) ).Count() );

					assertEquals( readEndPoints, members.Where( x => x.get( "role" ).Equals( "READ" ) ).flatMap( x => Arrays.stream( ( object[] ) x.get( "addresses" ) ) ).Count() );

					assertEquals( cores, members.Where( x => x.get( "role" ).Equals( "ROUTE" ) ).flatMap( x => Arrays.stream( ( object[] ) x.get( "addresses" ) ) ).Count() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToDiscoverFromReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToDiscoverFromReadReplicas()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(config).withNumberOfReadReplicas(2).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( Config ).withNumberOfReadReplicas( 2 ).startCluster();

			  try
			  {
					// when
					GetMembers( cluster.GetReadReplicaById( 0 ).database() );
					fail( "Should not be able to discover members from read replicas" );
			  }
			  catch ( ProcedureException ex )
			  {
					// then
					assertThat( ex.Message, containsString( "There is no procedure with the name" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.List<java.util.Map<String,Object>> getMembers(org.Neo4Net.kernel.impl.factory.GraphDatabaseFacade db) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException, org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private IList<IDictionary<string, object>> GetMembers( GraphDatabaseFacade db )
		 {
			  Kernel kernel = Db.DependencyResolver.resolveDependency( typeof( Kernel ) );
			  using ( Transaction tx = kernel.BeginTransaction( Transaction_Type.@implicit, AnonymousContext.read() ) )
			  {
					// when
					IList<object[]> currentMembers = new IList<object[]> { tx.Procedures().procedureCallRead(procedureName(GET_SERVERS_V1.fullyQualifiedProcedureName()), new object[0], ProcedureCallContext.EMPTY) };

					return ( IList<IDictionary<string, object>> ) currentMembers[0][1];
			  }
		 }
	}

}
﻿using System.Collections.Generic;

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
	using Description = org.hamcrest.Description;
	using FeatureMatcher = org.hamcrest.FeatureMatcher;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using RoleInfo = Org.Neo4j.causalclustering.discovery.RoleInfo;
	using ClusterOverviewProcedure = Org.Neo4j.causalclustering.discovery.procedures.ClusterOverviewProcedure;
	using Org.Neo4j.Collection;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using Transaction_Type = Org.Neo4j.@internal.Kernel.Api.Transaction_Type;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using AnonymousContext = Org.Neo4j.Kernel.api.security.AnonymousContext;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.RoleInfo.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.RoleInfo.LEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.RoleInfo.READ_REPLICA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class BaseClusterOverviewIT
	public abstract class BaseClusterOverviewIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.disable_middleware_logging, "false").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.disable_middleware_logging, "false").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.middleware_logging_level, "0").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.middleware_logging_level, "0");
		 public ClusterRule ClusterRule = new ClusterRule().withSharedCoreParam(CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedReadReplicaParam(CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(CausalClusteringSettings.disable_middleware_logging, "false").withSharedReadReplicaParam(CausalClusteringSettings.disable_middleware_logging, "false").withSharedCoreParam(CausalClusteringSettings.middleware_logging_level, "0").withSharedReadReplicaParam(CausalClusteringSettings.middleware_logging_level, "0");

		 protected internal BaseClusterOverviewIT( DiscoveryServiceType discoveryServiceType )
		 {
			  ClusterRule.withDiscoveryServiceType( discoveryServiceType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverCoreMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverCoreMembers()
		 {
			  // given
			  int coreMembers = 3;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  ClusterRule.withNumberOfReadReplicas( 0 );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  Matcher<IList<MemberInfo>> expected = allOf( ContainsMemberAddresses( cluster.CoreMembers() ), ContainsRole(LEADER, 1), ContainsRole(FOLLOWER, coreMembers - 1), DoesNotContainRole(READ_REPLICA) );

			  // then
			  AssertAllEventualOverviews( cluster, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverCoreMembersAndReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverCoreMembersAndReadReplicas()
		 {
			  // given
			  int coreMembers = 3;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  int replicaCount = 3;
			  ClusterRule.withNumberOfReadReplicas( replicaCount );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  Matcher<IList<MemberInfo>> expected = allOf( ContainsAllMemberAddresses( cluster.CoreMembers(), cluster.ReadReplicas() ), ContainsRole(LEADER, 1), ContainsRole(FOLLOWER, 2), ContainsRole(READ_REPLICA, replicaCount) );

			  // then
			  AssertAllEventualOverviews( cluster, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverReadReplicasAfterRestartingCores() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverReadReplicasAfterRestartingCores()
		 {
			  // given
			  int coreMembers = 3;
			  int readReplicas = 3;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  ClusterRule.withNumberOfReadReplicas( readReplicas );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  cluster.ShutdownCoreMembers();
			  cluster.StartCoreMembers();

			  Matcher<IList<MemberInfo>> expected = allOf( ContainsAllMemberAddresses( cluster.CoreMembers(), cluster.ReadReplicas() ), ContainsRole(LEADER, 1), ContainsRole(FOLLOWER, coreMembers - 1), ContainsRole(READ_REPLICA, readReplicas) );

			  // then
			  AssertAllEventualOverviews( cluster, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverNewCoreMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverNewCoreMembers()
		 {
			  // given
			  int initialCoreMembers = 3;
			  ClusterRule.withNumberOfCoreMembers( initialCoreMembers );
			  ClusterRule.withNumberOfReadReplicas( 0 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
			  int extraCoreMembers = 2;
			  int finalCoreMembers = initialCoreMembers + extraCoreMembers;
			  IntStream.range( 0, extraCoreMembers ).forEach( idx => cluster.AddCoreMemberWithId( initialCoreMembers + idx ).start() );

			  Matcher<IList<MemberInfo>> expected = allOf( ContainsMemberAddresses( cluster.CoreMembers() ), ContainsRole(LEADER, 1), ContainsRole(FOLLOWER, finalCoreMembers - 1) );

			  // then
			  AssertAllEventualOverviews( cluster, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverNewReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverNewReadReplicas()
		 {
			  // given
			  int coreMembers = 3;
			  int initialReadReplicas = 2;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  ClusterRule.withNumberOfReadReplicas( initialReadReplicas );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
			  cluster.AddReadReplicaWithId( initialReadReplicas ).start();
			  cluster.AddReadReplicaWithId( initialReadReplicas + 1 ).start();

			  Matcher<IList<MemberInfo>> expected = allOf( ContainsAllMemberAddresses( cluster.CoreMembers(), cluster.ReadReplicas() ), ContainsRole(LEADER, 1), ContainsRole(FOLLOWER, coreMembers - 1), ContainsRole(READ_REPLICA, initialReadReplicas + 2) );

			  // then
			  AssertAllEventualOverviews( cluster, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverRemovalOfReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverRemovalOfReadReplicas()
		 {
			  // given
			  int coreMembers = 3;
			  int initialReadReplicas = 3;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  ClusterRule.withNumberOfReadReplicas( initialReadReplicas );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  AssertAllEventualOverviews( cluster, ContainsRole( READ_REPLICA, initialReadReplicas ) );

			  // when
			  cluster.RemoveReadReplicaWithMemberId( 0 );
			  cluster.RemoveReadReplicaWithMemberId( 1 );

			  // then
			  AssertAllEventualOverviews( cluster, ContainsRole( READ_REPLICA, initialReadReplicas - 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverRemovalOfCoreMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverRemovalOfCoreMembers()
		 {
			  // given
			  int coreMembers = 5;
			  ClusterRule.withNumberOfCoreMembers( coreMembers );
			  ClusterRule.withNumberOfReadReplicas( 0 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  AssertAllEventualOverviews( cluster, allOf( ContainsRole( LEADER, 1 ), ContainsRole( FOLLOWER, coreMembers - 1 ) ) );

			  // when
			  cluster.RemoveCoreMemberWithServerId( 0 );
			  cluster.RemoveCoreMemberWithServerId( 1 );

			  // then
			  AssertAllEventualOverviews( cluster, allOf( ContainsRole( LEADER, 1 ), ContainsRole( FOLLOWER, coreMembers - 1 - 2 ) ), asSet( 0, 1 ), Collections.emptySet() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverTimeoutBasedLeaderStepdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverTimeoutBasedLeaderStepdown()
		 {
			  ClusterRule.withNumberOfCoreMembers( 3 );
			  ClusterRule.withNumberOfReadReplicas( 0 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  IList<CoreClusterMember> followers = cluster.GetAllMembersWithRole( Role.FOLLOWER );
			  CoreClusterMember leader = cluster.GetMemberWithRole( Role.LEADER );
			  followers.ForEach( CoreClusterMember.shutdown );

			  AssertEventualOverview( ContainsRole( LEADER, 0 ), leader, "core" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverGreaterTermBasedLeaderStepdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverGreaterTermBasedLeaderStepdown()
		 {
			  int originalCoreMembers = 3;
			  ClusterRule.withNumberOfCoreMembers( originalCoreMembers ).withNumberOfReadReplicas( 0 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  CoreClusterMember leader = cluster.AwaitLeader();
			  leader.Config().augment(CausalClusteringSettings.refuse_to_be_leader, Settings.TRUE);

			  IList<MemberInfo> preElectionOverview = ClusterOverview( leader.Database() );

			  CoreClusterMember follower = cluster.GetMemberWithRole( Role.FOLLOWER );
			  follower.Raft().triggerElection(Clock.systemUTC());

			  AssertEventualOverview( allOf( ContainsRole( LEADER, 1 ), ContainsRole( FOLLOWER, originalCoreMembers - 1 ), not( equalTo( preElectionOverview ) ) ), leader, "core" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAllEventualOverviews(org.neo4j.causalclustering.discovery.Cluster<?> cluster, org.hamcrest.Matcher<java.util.List<MemberInfo>> expected) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private void AssertAllEventualOverviews<T1>( Cluster<T1> cluster, Matcher<IList<MemberInfo>> expected )
		 {
			  AssertAllEventualOverviews( cluster, expected, Collections.emptySet(), Collections.emptySet() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAllEventualOverviews(org.neo4j.causalclustering.discovery.Cluster<?> cluster, org.hamcrest.Matcher<java.util.List<MemberInfo>> expected, java.util.Set<int> excludedCores, java.util.Set<int> excludedRRs) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private void AssertAllEventualOverviews<T1>( Cluster<T1> cluster, Matcher<IList<MemberInfo>> expected, ISet<int> excludedCores, ISet<int> excludedRRs )
		 {
			  foreach ( CoreClusterMember core in cluster.CoreMembers() )
			  {
					if ( !excludedCores.Contains( core.ServerId() ) )
					{
						 AssertEventualOverview( expected, core, "core" );
					}

			  }

			  foreach ( ReadReplica rr in cluster.ReadReplicas() )
			  {
					if ( !excludedRRs.Contains( rr.ServerId() ) )
					{
						 AssertEventualOverview( expected, rr, "rr" );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertEventualOverview(org.hamcrest.Matcher<java.util.List<MemberInfo>> expected, org.neo4j.causalclustering.discovery.ClusterMember<? extends org.neo4j.kernel.impl.factory.GraphDatabaseFacade> member, String role) throws org.neo4j.internal.kernel.api.exceptions.KernelException, InterruptedException
		 private void AssertEventualOverview<T1>( Matcher<IList<MemberInfo>> expected, ClusterMember<T1> member, string role ) where T1 : Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  System.Func<IList<MemberInfo>, string> printableMemberInfos = memberInfos => memberInfos.Select( MemberInfo::toString ).collect( Collectors.joining( ", " ) );

			  string message = string.Format( "should have overview from {0} {1}, but view was ", role, member.ServerId() );
			  assertEventually( memberInfos => message + printableMemberInfos( memberInfos ), () => ClusterOverview(member.Database()), expected, 180, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final org.hamcrest.Matcher<Iterable<? extends MemberInfo>> containsAllMemberAddresses(java.util.Collection<? extends org.neo4j.causalclustering.discovery.ClusterMember>... members)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Matcher<IEnumerable<MemberInfo>> ContainsAllMemberAddresses( params ICollection<ClusterMember>[] members )
		 {
			  return ContainsMemberAddresses( Stream.of( members ).flatMap( System.Collections.ICollection.stream ).collect( toList() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.hamcrest.Matcher<Iterable<? extends MemberInfo>> containsMemberAddresses(java.util.Collection<? extends org.neo4j.causalclustering.discovery.ClusterMember> members)
		 private Matcher<IEnumerable<MemberInfo>> ContainsMemberAddresses<T1>( ICollection<T1> members ) where T1 : Org.Neo4j.causalclustering.discovery.ClusterMember
		 {
			  return containsInAnyOrder( members.Select( coreClusterMember => new TypeSafeMatcherAnonymousInnerClass( this ) ).ToList() );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<MemberInfo>
		 {
			 private readonly BaseClusterOverviewIT _outerInstance;

			 public TypeSafeMatcherAnonymousInnerClass( BaseClusterOverviewIT outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override bool matchesSafely( MemberInfo item )
			 {
				  ISet<string> addresses = asSet( item.Addresses );
				  foreach ( URI uri in coreClusterMember.clientConnectorAddresses().uriList() )
				  {
						if ( !addresses.Contains( uri.ToString() ) )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "MemberInfo with addresses: " ).appendValue( coreClusterMember.clientConnectorAddresses().boltAddress() );
			 }
		 }

		 private Matcher<IList<MemberInfo>> ContainsRole( RoleInfo expectedRole, long expectedCount )
		 {
			  return new FeatureMatcherAnonymousInnerClass( this, equalTo( expectedCount ), expectedRole.name(), expectedRole );
		 }

		 private class FeatureMatcherAnonymousInnerClass : FeatureMatcher<IList<MemberInfo>, long>
		 {
			 private readonly BaseClusterOverviewIT _outerInstance;

			 private RoleInfo _expectedRole;

			 public FeatureMatcherAnonymousInnerClass( BaseClusterOverviewIT outerInstance, UnknownType equalTo, UnknownType name, RoleInfo expectedRole ) : base( equalTo, name, "count" )
			 {
				 this.outerInstance = outerInstance;
				 this._expectedRole = expectedRole;
			 }

			 protected internal override long? featureValueOf( IList<MemberInfo> overview )
			 {
				  return overview.Where( info => info.role == _expectedRole ).Count();
			 }
		 }

		 private Matcher<IList<MemberInfo>> DoesNotContainRole( RoleInfo unexpectedRole )
		 {
			 return ContainsRole( unexpectedRole, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.List<MemberInfo> clusterOverview(org.neo4j.kernel.impl.factory.GraphDatabaseFacade db) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private IList<MemberInfo> ClusterOverview( GraphDatabaseFacade db )
		 {
			  Kernel kernel = Db.DependencyResolver.resolveDependency( typeof( Kernel ) );

			  IList<MemberInfo> infos = new List<MemberInfo>();
			  using ( Transaction tx = kernel.BeginTransaction( Transaction_Type.@implicit, AnonymousContext.read() ) )
			  {
					RawIterator<object[], ProcedureException> itr = tx.Procedures().procedureCallRead(procedureName("dbms", "cluster", ClusterOverviewProcedure.PROCEDURE_NAME), null, ProcedureCallContext.EMPTY);

					while ( itr.HasNext() )
					{
						 object[] row = itr.Next();
						 IList<string> addresses = ( IList<string> ) row[1];
						 infos.Add( new MemberInfo( addresses.ToArray(), Enum.Parse(typeof(RoleInfo), (string) row[2]) ) );
					}

					return infos;
			  }
		 }

		 private class MemberInfo
		 {
			  internal readonly string[] Addresses;
			  internal readonly RoleInfo Role;

			  internal MemberInfo( string[] addresses, RoleInfo role )
			  {
					this.Addresses = addresses;
					this.Role = role;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					MemberInfo that = ( MemberInfo ) o;
					return Arrays.Equals( Addresses, that.Addresses ) && Role == that.Role;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( Arrays.GetHashCode( Addresses ), Role );
			  }

			  public override string ToString()
			  {
					return string.Format( "MemberInfo{{addresses='{0}', role={1}}}", Arrays.ToString( Addresses ), Role );
			  }
		 }
	}

}
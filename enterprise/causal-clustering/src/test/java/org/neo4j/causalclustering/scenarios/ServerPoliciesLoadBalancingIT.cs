﻿using System;
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
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using EnterpriseCluster = Org.Neo4j.causalclustering.discovery.EnterpriseCluster;
	using HazelcastDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.HazelcastDiscoveryServiceFactory;
	using IpFamily = Org.Neo4j.causalclustering.discovery.IpFamily;
	using Endpoint = Org.Neo4j.causalclustering.routing.Endpoint;
	using LoadBalancingResult = Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingResult;
	using Policies = Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies.Policies;
	using ParameterNames = Org.Neo4j.causalclustering.routing.load_balancing.procedure.ParameterNames;
	using ResultFormatV1 = Org.Neo4j.causalclustering.routing.load_balancing.procedure.ResultFormatV1;
	using Org.Neo4j.Function;
	using Result = Org.Neo4j.Graphdb.Result;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.load_balancing.procedure.ProcedureNames.GET_SERVERS_V2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class ServerPoliciesLoadBalancingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultBehaviour() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DefaultBehaviour()
		 {
			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), 3, 3, new HazelcastDiscoveryServiceFactory(), emptyMap(), emptyMap(), emptyMap(), emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _cluster.start();

			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 2, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultBehaviourWithAllowReadsOnFollowers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DefaultBehaviourWithAllowReadsOnFollowers()
		 {
			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), 3, 3, new HazelcastDiscoveryServiceFactory(), stringMap(CausalClusteringSettings.cluster_allow_reads_on_followers.name(), "true"), emptyMap(), emptyMap(), emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _cluster.start();

			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 2, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFallOverBetweenRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFallOverBetweenRules()
		 {
			  IDictionary<string, System.Func<int, string>> instanceCoreParams = new Dictionary<string, System.Func<int, string>>();
			  instanceCoreParams[CausalClusteringSettings.server_groups.name()] = id => "core" + id + ",core";
			  IDictionary<string, System.Func<int, string>> instanceReplicaParams = new Dictionary<string, System.Func<int, string>>();
			  instanceReplicaParams[CausalClusteringSettings.server_groups.name()] = id => "replica" + id + ",replica";

			  string defaultPolicy = "groups(core) -> min(3); groups(replica1,replica2) -> min(2);";

			  IDictionary<string, string> coreParams = stringMap( CausalClusteringSettings.cluster_allow_reads_on_followers.name(), "true", CausalClusteringSettings.load_balancing_config.name() + ".server_policies.default", defaultPolicy, CausalClusteringSettings.multi_dc_license.name(), "true" );

			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), 5, 5, new HazelcastDiscoveryServiceFactory(), coreParams, instanceCoreParams, emptyMap(), instanceReplicaParams, Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _cluster.start();
			  // should use the first rule: only cores for reading
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 5, 1, 4, 0 ) );

			  _cluster.getCoreMemberById( 3 ).shutdown();
			  // one core reader is gone, but we are still fulfilling min(3)
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 4, 1, 3, 0 ) );

			  _cluster.getCoreMemberById( 0 ).shutdown();
			  // should now fall over to the second rule: use replica1 and replica2
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 0, 2 ) );

			  _cluster.getReadReplicaById( 0 ).shutdown();
			  // this does not affect replica1 and replica2
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 0, 2 ) );

			  _cluster.getReadReplicaById( 1 ).shutdown();
			  // should now fall over to use the last rule: all
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 2, 3 ) );

			  _cluster.addCoreMemberWithId( 3 ).start();
			  // should now go back to first rule
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 4, 1, 3, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportSeveralPolicies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportSeveralPolicies()
		 {
			  IDictionary<string, System.Func<int, string>> instanceCoreParams = new Dictionary<string, System.Func<int, string>>();
			  instanceCoreParams[CausalClusteringSettings.server_groups.name()] = id => "core" + id + ",core";
			  IDictionary<string, System.Func<int, string>> instanceReplicaParams = new Dictionary<string, System.Func<int, string>>();
			  instanceReplicaParams[CausalClusteringSettings.server_groups.name()] = id => "replica" + id + ",replica";

			  string defaultPolicySpec = "groups(replica0,replica1)";
			  string policyOneTwoSpec = "groups(replica1,replica2)";
			  string policyZeroTwoSpec = "groups(replica0,replica2)";
			  string policyAllReplicasSpec = "groups(replica); halt()";
			  string allPolicySpec = "all()";

			  IDictionary<string, string> coreParams = stringMap( CausalClusteringSettings.cluster_allow_reads_on_followers.name(), "true", CausalClusteringSettings.load_balancing_config.name() + ".server_policies.all", allPolicySpec, CausalClusteringSettings.load_balancing_config.name() + ".server_policies.default", defaultPolicySpec, CausalClusteringSettings.load_balancing_config.name() + ".server_policies.policy_one_two", policyOneTwoSpec, CausalClusteringSettings.load_balancing_config.name() + ".server_policies.policy_zero_two", policyZeroTwoSpec, CausalClusteringSettings.load_balancing_config.name() + ".server_policies.policy_all_replicas", policyAllReplicasSpec, CausalClusteringSettings.multi_dc_license.name(), "true" );

			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), 3, 3, new HazelcastDiscoveryServiceFactory(), coreParams, instanceCoreParams, emptyMap(), instanceReplicaParams, Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _cluster.start();
			  AssertGetServersEventuallyMatchesOnAllCores( new CountsMatcher( this, 3, 1, 2, 3 ), PolicyContext( "all" ) );
			  // all cores have observed the full topology, now specific policies should all return the same result

			  foreach ( CoreClusterMember core in _cluster.coreMembers() )
			  {
					CoreGraphDatabase db = core.Database();

					assertThat( GetServers( db, PolicyContext( "default" ) ), new SpecificReplicasMatcher( this, 0, 1 ) );
					assertThat( GetServers( db, PolicyContext( "policy_one_two" ) ), new SpecificReplicasMatcher( this, 1, 2 ) );
					assertThat( GetServers( db, PolicyContext( "policy_zero_two" ) ), new SpecificReplicasMatcher( this, 0, 2 ) );
					assertThat( GetServers( db, PolicyContext( "policy_all_replicas" ) ), new SpecificReplicasMatcher( this, 0, 1, 2 ) );
			  }
		 }

		 private IDictionary<string, string> PolicyContext( string policyName )
		 {
			  return stringMap( Policies.POLICY_KEY, policyName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertGetServersEventuallyMatchesOnAllCores(org.hamcrest.Matcher<org.neo4j.causalclustering.routing.load_balancing.LoadBalancingResult> matcher) throws InterruptedException
		 private void AssertGetServersEventuallyMatchesOnAllCores( Matcher<LoadBalancingResult> matcher )
		 {
			  AssertGetServersEventuallyMatchesOnAllCores( matcher, emptyMap() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertGetServersEventuallyMatchesOnAllCores(org.hamcrest.Matcher<org.neo4j.causalclustering.routing.load_balancing.LoadBalancingResult> matcher, java.util.Map<String,String> context) throws InterruptedException
		 private void AssertGetServersEventuallyMatchesOnAllCores( Matcher<LoadBalancingResult> matcher, IDictionary<string, string> context )
		 {
			  foreach ( CoreClusterMember core in _cluster.coreMembers() )
			  {
					if ( core.Database() == null )
					{
						 // this core is shutdown
						 continue;
					}

					AssertEventually( matcher, () => GetServers(core.Database(), context) );
			  }
		 }

		 private LoadBalancingResult GetServers( CoreGraphDatabase db, IDictionary<string, string> context )
		 {
			  LoadBalancingResult lbResult = null;
			  using ( InternalTransaction tx = Db.beginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ) )
			  {
					IDictionary<string, object> parameters = MapUtil.map( ParameterNames.CONTEXT.parameterName(), context );
					using ( Result result = Db.execute( tx, "CALL " + GET_SERVERS_V2.callName(), ValueUtils.asMapValue(parameters) ) )
					{
						 while ( result.MoveNext() )
						 {
							  lbResult = ResultFormatV1.parse( result.Current );
						 }
					}
			  }
			  return lbResult;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private static <T, E extends Exception> void assertEventually(org.hamcrest.Matcher<? super T> matcher, org.neo4j.function.ThrowingSupplier<T,E> actual) throws InterruptedException, E
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static void AssertEventually<T, E, T1>( Matcher<T1> matcher, ThrowingSupplier<T, E> actual ) where E : Exception
		 {
			  Org.Neo4j.Test.assertion.Assert.assertEventually( "", actual, matcher, 120, SECONDS );
		 }

		 internal class CountsMatcher : BaseMatcher<LoadBalancingResult>
		 {
			 private readonly ServerPoliciesLoadBalancingIT _outerInstance;

			  internal readonly int NRouters;
			  internal readonly int NWriters;
			  internal readonly int NCoreReaders;
			  internal readonly int NReplicaReaders;

			  internal CountsMatcher( ServerPoliciesLoadBalancingIT outerInstance, int nRouters, int nWriters, int nCoreReaders, int nReplicaReaders )
			  {
				  this._outerInstance = outerInstance;
					this.NRouters = nRouters;
					this.NWriters = nWriters;
					this.NCoreReaders = nCoreReaders;
					this.NReplicaReaders = nReplicaReaders;
			  }

			  public override bool Matches( object item )
			  {
					LoadBalancingResult result = ( LoadBalancingResult ) item;

					if ( result.RouteEndpoints().Count != NRouters || result.WriteEndpoints().Count != NWriters )
					{
						 return false;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> allCoreBolts = outerInstance.cluster.CoreMembers().Select(c => c.clientConnectorAddresses().boltAddress()).collect(Collectors.toSet());

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> returnedCoreReaders = result.ReadEndpoints().Select(Endpoint::address).Where(allCoreBolts.contains).collect(Collectors.toSet());

					if ( returnedCoreReaders.Count != NCoreReaders )
					{
						 return false;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> allReplicaBolts = outerInstance.cluster.ReadReplicas().Select(c => c.clientConnectorAddresses().boltAddress()).collect(Collectors.toSet());

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> returnedReplicaReaders = result.ReadEndpoints().Select(Endpoint::address).Where(allReplicaBolts.contains).collect(Collectors.toSet());

					if ( returnedReplicaReaders.Count != NReplicaReaders )
					{
						 return false;
					}

					HashSet<AdvertisedSocketAddress> overlap = new HashSet<AdvertisedSocketAddress>( returnedCoreReaders );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'retainAll' method:
					overlap.retainAll( returnedReplicaReaders );

					if ( overlap.Count > 0 )
					{
						 return false;
					}

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> returnedWriters = result.WriteEndpoints().Select(Endpoint::address).collect(Collectors.toSet());

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
					if ( !allCoreBolts.containsAll( returnedWriters ) )
					{
						 return false;
					}

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> returnedRouters = result.RouteEndpoints().Select(Endpoint::address).collect(Collectors.toSet());

					//noinspection RedundantIfStatement
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
					if ( !allCoreBolts.containsAll( returnedRouters ) )
					{
						 return false;
					}

					return true;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "nRouters=" + NRouters );
					description.appendText( ", nWriters=" + NWriters );
					description.appendText( ", nCoreReaders=" + NCoreReaders );
					description.appendText( ", nReplicaReaders=" + NReplicaReaders );
			  }
		 }

		 internal class SpecificReplicasMatcher : BaseMatcher<LoadBalancingResult>
		 {
			 private readonly ServerPoliciesLoadBalancingIT _outerInstance;

			  internal readonly ISet<int> ReplicaIds;

			  internal SpecificReplicasMatcher( ServerPoliciesLoadBalancingIT outerInstance, params Integer[] replicaIds )
			  {
				  this._outerInstance = outerInstance;
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					this.ReplicaIds = java.util.replicaIds.collect( Collectors.toSet() );
			  }

			  public override bool Matches( object item )
			  {
					LoadBalancingResult result = ( LoadBalancingResult ) item;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> returnedReaders = result.ReadEndpoints().Select(Endpoint::address).collect(Collectors.toSet());

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<AdvertisedSocketAddress> expectedBolts = outerInstance.cluster.ReadReplicas().Where(r => ReplicaIds.Contains(r.serverId())).Select(r => r.clientConnectorAddresses().boltAddress()).collect(Collectors.toSet());

					return expectedBolts.SetEquals( returnedReaders );
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "replicaIds=" + ReplicaIds );
			  }
		 }
	}

}
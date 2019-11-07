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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using Timeout = org.junit.rules.Timeout;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Endpoint = Neo4Net.causalclustering.routing.Endpoint;
	using MultiClusterRoutingResult = Neo4Net.causalclustering.routing.multi_cluster.MultiClusterRoutingResult;
	using MultiClusterRoutingResultFormat = Neo4Net.causalclustering.routing.multi_cluster.procedure.MultiClusterRoutingResultFormat;
	using ProcedureNames = Neo4Net.causalclustering.routing.multi_cluster.procedure.ProcedureNames;
	using Result = Neo4Net.GraphDb.Result;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ParameterNames.DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_ALL_DATABASES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class BaseMultiClusterRoutingIT
	public abstract class BaseMultiClusterRoutingIT
	{

		 protected internal static ISet<string> DbNames_1 = Stream.of( "foo", "bar" ).collect( Collectors.toSet() );
		 protected internal static ISet<string> DbNames_2 = Collections.singleton( "default" );
		 protected internal static ISet<string> DbNames_3 = Stream.of( "foo", "bar", "baz" ).collect( Collectors.toSet() );

		 private readonly ISet<string> _dbNames;
		 private readonly ClusterRule _clusterRule;
		 private readonly DefaultFileSystemRule _fileSystemRule;
		 private readonly DiscoveryServiceType _discoveryType;
		 private readonly int _numCores;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain;
		 public readonly RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.Timeout globalTimeout = org.junit.rules.Timeout.seconds(300);
		 public Timeout GlobalTimeout = Timeout.seconds( 300 );

		 protected internal BaseMultiClusterRoutingIT( string ignoredName, int numCores, int numReplicas, ISet<string> dbNames, DiscoveryServiceType discoveryType )
		 {
			  this._dbNames = dbNames;
			  this._discoveryType = discoveryType;

			  this._clusterRule = ( new ClusterRule() ).withNumberOfCoreMembers(numCores).withNumberOfReadReplicas(numReplicas).withDatabaseNames(dbNames);
			  this._numCores = numCores;

			  this._fileSystemRule = new DefaultFileSystemRule();
			  this.RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _clusterRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _clusterRule.withDiscoveryServiceType( _discoveryType );
			  _fs = _fileSystemRule.get();
			  _cluster = _clusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void superCallShouldReturnAllRouters()
		 public virtual void SuperCallShouldReturnAllRouters()
		 {
			  IList<CoreGraphDatabase> dbs = _dbNames.Select( n => _cluster.getMemberWithAnyRole( n, Role.FOLLOWER, Role.LEADER ).database() ).ToList();

			  Stream<Optional<MultiClusterRoutingResult>> optResults = dbs.Select( db => CallProcedure( db, GET_ROUTERS_FOR_ALL_DATABASES, Collections.emptyMap() ) );

			  IList<MultiClusterRoutingResult> results = optResults.filter( Optional.isPresent ).map( Optional.get ).collect( Collectors.toList() );
			  assertEquals( "There should be a result for each database against which the procedure is executed.", _dbNames.Count, results.Count );

			  bool consistentResults = results.Distinct().Count() == 1;
			  assertThat( "The results should be the same, regardless of which database the procedure is executed against.", consistentResults );

			  System.Func<IDictionary<string, IList<Endpoint>>, int> countHosts = m => m.values().Select(System.Collections.IList.size).Sum();
			  int resultsAllCores = results.First().Select(r => countHosts(r.routers())).orElse(0);
			  assertEquals( "The results of the procedure should return all core hosts in the topology.", _numCores, resultsAllCores );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void subCallShouldReturnLocalRouters()
		 public virtual void SubCallShouldReturnLocalRouters()
		 {
			  string dbName = GetFirstDbName( _dbNames );
			  Stream<CoreGraphDatabase> members = _dbNames.Select( n => _cluster.getMemberWithAnyRole( n, Role.FOLLOWER, Role.LEADER ).database() );

			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params[DATABASE.parameterName()] = dbName;
			  Stream<Optional<MultiClusterRoutingResult>> optResults = members.map( db => CallProcedure( db, GET_ROUTERS_FOR_DATABASE, @params ) );
			  IList<MultiClusterRoutingResult> results = optResults.filter( Optional.isPresent ).map( Optional.get ).collect( Collectors.toList() );

			  bool consistentResults = results.Distinct().Count() == 1;
			  assertThat( "The results should be the same, regardless of which database the procedure is executed against.", consistentResults );

			  Optional<MultiClusterRoutingResult> firstResult = results.First();

			  int numRouterSets = firstResult.map( r => r.routers().size() ).orElse(0);
			  assertEquals( "There should only be routers returned for a single database.", 1, numRouterSets );

			  bool correctResultDbName = firstResult.map( r => r.routers().containsKey(dbName) ).orElse(false);
			  assertThat( "The results should contain routers for the database passed to the procedure.", correctResultDbName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void procedureCallsShouldReflectMembershipChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcedureCallsShouldReflectMembershipChanges()
		 {
			  string dbName = GetFirstDbName( _dbNames );
			  CoreClusterMember follower = _cluster.getMemberWithAnyRole( dbName, Role.FOLLOWER );
			  int followerId = follower.ServerId();

			  _cluster.removeCoreMemberWithServerId( followerId );

			  CoreGraphDatabase db = _cluster.getMemberWithAnyRole( dbName, Role.FOLLOWER, Role.LEADER ).database();

			  System.Func<CoreGraphDatabase, ISet<Endpoint>> getResult = database =>
			  {
				Optional<MultiClusterRoutingResult> optResult = CallProcedure( database, GET_ROUTERS_FOR_ALL_DATABASES, java.util.Collections.emptyMap() );

				return optResult.map( r => r.routers().values().stream().flatMap(System.Collections.IList.stream).collect(Collectors.toSet()) ).orElse(java.util.Collections.emptySet());
			  };

			  assertEventually( "The procedure should return one fewer routers when a core member has been removed.", () => getResult(db).size(), @is(_numCores - 1), 15, TimeUnit.SECONDS );

			  System.Func<ISet<Endpoint>, CoreClusterMember, bool> containsFollower = ( rs, f ) => rs.Any( r => r.address().ToString().Equals(f.boltAdvertisedAddress()) );

			  assertEventually( "The procedure should not return a host as a router after it has been removed from the cluster", () => containsFollower(getResult(db), follower), @is(false), 15, TimeUnit.SECONDS );

			  CoreClusterMember newFollower = _cluster.addCoreMemberWithId( followerId );
			  newFollower.Start();

			  assertEventually( "The procedure should return one more router when a core member has been added.", () => getResult(db).size(), @is(_numCores), 15, TimeUnit.SECONDS );
			  assertEventually( "The procedure should return a core member as a router after it has been added to the cluster", () => containsFollower(getResult(db), newFollower), @is(true), 15, TimeUnit.SECONDS );

		 }

		 private static string GetFirstDbName( ISet<string> dbNames )
		 {
			  return dbNames.First().orElseThrow(() => new System.ArgumentException("The dbNames parameter must not be empty."));
		 }

		 private static Optional<MultiClusterRoutingResult> CallProcedure( CoreGraphDatabase db, ProcedureNames procedure, IDictionary<string, object> @params )
		 {

			  Optional<MultiClusterRoutingResult> routingResult = null;
			  using ( InternalTransaction tx = Db.BeginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ), Result result = Db.execute( tx, "CALL " + procedure.callName(), ValueUtils.asMapValue(@params) ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( result.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 routingResult = MultiClusterRoutingResultFormat.parse( result.Next() );
					}
			  }
			  return routingResult;
		 }

	}

}
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
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
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
	using Result = Org.Neo4j.Graphdb.Result;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ServerGroupsIT
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
//ORIGINAL LINE: @Test public void shouldUpdateGroupsOnStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateGroupsOnStart()
		 {
			  AtomicReference<string> suffix = new AtomicReference<string>( "before" );
			  IList<IList<string>> expected;

			  IDictionary<string, System.Func<int, string>> instanceCoreParams = new Dictionary<string, System.Func<int, string>>();
			  instanceCoreParams[CausalClusteringSettings.server_groups.name()] = id => string.join(", ", MakeCoreGroups(suffix.get(), id));

			  IDictionary<string, System.Func<int, string>> instanceReplicaParams = new Dictionary<string, System.Func<int, string>>();
			  instanceReplicaParams[CausalClusteringSettings.server_groups.name()] = id => string.join(", ", MakeReplicaGroups(suffix.get(), id));

			  int nServers = 3;
			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), nServers, nServers, new HazelcastDiscoveryServiceFactory(), emptyMap(), instanceCoreParams, emptyMap(), instanceReplicaParams, Standard.LATEST_NAME, IpFamily.IPV4, false );

			  // when
			  _cluster.start();

			  // then
			  expected = new List<IList<string>>();
			  foreach ( CoreClusterMember core in _cluster.coreMembers() )
			  {
					expected.Add( MakeCoreGroups( suffix.get(), core.ServerId() ) );
					expected.Add( MakeReplicaGroups( suffix.get(), core.ServerId() ) );
			  }

			  foreach ( CoreClusterMember core in _cluster.coreMembers() )
			  {
					assertEventually( core + " should have groups", () => GetServerGroups(core.Database()), new GroupsMatcher(this, expected), 30, SECONDS );
			  }

			  // when
			  expected.Remove( MakeCoreGroups( suffix.get(), 1 ) );
			  expected.Remove( MakeReplicaGroups( suffix.get(), 2 ) );
			  _cluster.getCoreMemberById( 1 ).shutdown();
			  _cluster.getReadReplicaById( 2 ).shutdown();

			  suffix.set( "after" ); // should update groups of restarted servers
			  _cluster.addCoreMemberWithId( 1 ).start();
			  _cluster.addReadReplicaWithId( 2 ).start();
			  expected.Add( MakeCoreGroups( suffix.get(), 1 ) );
			  expected.Add( MakeReplicaGroups( suffix.get(), 2 ) );

			  // then
			  foreach ( CoreClusterMember core in _cluster.coreMembers() )
			  {
					assertEventually( core + " should have groups", () => GetServerGroups(core.Database()), new GroupsMatcher(this, expected), 30, SECONDS );
			  }
		 }

		 internal class GroupsMatcher : TypeSafeMatcher<IList<IList<string>>>
		 {
			 private readonly ServerGroupsIT _outerInstance;

			  internal readonly IList<IList<string>> Expected;

			  internal GroupsMatcher( ServerGroupsIT outerInstance, IList<IList<string>> expected )
			  {
				  this._outerInstance = outerInstance;
					this.Expected = expected;
			  }

			  protected internal override bool MatchesSafely( IList<IList<string>> actual )
			  {
					if ( actual.Count != Expected.Count )
					{
						 return false;
					}

					foreach ( IList<string> actualGroups in actual )
					{
						 bool matched = false;
						 foreach ( IList<string> expectedGroups in Expected )
						 {
							  if ( actualGroups.Count != expectedGroups.Count )
							  {
									continue;
							  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
							  if ( !actualGroups.containsAll( expectedGroups ) )
							  {
									continue;
							  }

							  matched = true;
							  break;
						 }

						 if ( !matched )
						 {
							  return false;
						 }
					}

					return true;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( Expected.ToString() );
			  }
		 }

		 private IList<string> MakeCoreGroups( string suffix, int id )
		 {
			  return new IList<string> { format( "core-%d-%s", id, suffix ), "core" };
		 }

		 private IList<string> MakeReplicaGroups( string suffix, int id )
		 {
			  return new IList<string> { format( "replica-%d-%s", id, suffix ), "replica" };
		 }

		 private IList<IList<string>> GetServerGroups( CoreGraphDatabase db )
		 {
			  IList<IList<string>> serverGroups = new List<IList<string>>();
			  using ( InternalTransaction tx = Db.beginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ) )
			  {
					using ( Result result = Db.execute( tx, "CALL dbms.cluster.overview", EMPTY_MAP ) )
					{
						 while ( result.MoveNext() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<String> groups = (java.util.List<String>) result.Current.get("groups");
							  IList<string> groups = ( IList<string> ) result.Current.get( "groups" );
							  serverGroups.Add( groups );
						 }
					}
			  }
			  return serverGroups;
		 }
	}

}
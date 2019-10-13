using System;
using System.Collections.Generic;

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
namespace Jmx
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Kernel = Neo4Net.Jmx.Kernel;
	using JmxKernelExtension = Neo4Net.Jmx.impl.JmxKernelExtension;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using BranchedStore = Neo4Net.management.BranchedStore;
	using ClusterMemberInfo = Neo4Net.management.ClusterMemberInfo;
	using HighAvailability = Neo4Net.management.HighAvailability;
	using Neo4jManager = Neo4Net.management.Neo4jManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.ha.ClusterRule.intBase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.ha.ClusterRule.stringWithIntBase;

	public class HaBeanIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withInstanceSetting(setting("jmx.port", STRING, org.neo4j.kernel.configuration.Settings.NO_DEFAULT), intBase(9912)).withInstanceSetting(org.neo4j.kernel.ha.HaSettings.ha_server, stringWithIntBase(":", 1136)).withInstanceSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.forced_kernel_id, stringWithIntBase("kernel", 0));
		 public readonly ClusterRule ClusterRule = new ClusterRule().withInstanceSetting(setting("jmx.port", STRING, Settings.NO_DEFAULT), intBase(9912)).withInstanceSetting(HaSettings.ha_server, stringWithIntBase(":", 1136)).withInstanceSetting(GraphDatabaseSettings.forced_kernel_id, stringWithIntBase("kernel", 0));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessHaBeans()
		 public virtual void ShouldAccessHaBeans()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();

			  // High Availability bean
			  HighAvailability ha = ha( cluster.Master );
			  assertNotNull( "could not get ha bean", ha );
			  AssertMasterInformation( ha );
			  AssertMasterAndSlaveInformation( ha.InstancesInCluster );
			  foreach ( ClusterMemberInfo info in ha.InstancesInCluster )
			  {
					assertTrue( info.Alive );
					assertTrue( info.Available );
			  }

			  // Branched data bean
			  BranchedStore bs = Beans( cluster.Master ).BranchedStoreBean;
			  assertNotNull( "could not get branched store bean", bs );
		 }

		 private void AssertMasterInformation( HighAvailability ha )
		 {
			  assertTrue( "should be available", ha.Available );
			  assertEquals( "should be master", HighAvailabilityModeSwitcher.MASTER, ha.Role );
		 }

		 private Neo4jManager Beans( HighlyAvailableGraphDatabase db )
		 {
			  return new Neo4jManager( Db.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Kernel ) ) );
		 }

		 private HighAvailability Ha( HighlyAvailableGraphDatabase db )
		 {
			  return Beans( db ).HighAvailabilityBean;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static java.net.URI getUriForScheme(final String scheme, Iterable<java.net.URI> uris)
		 private static URI GetUriForScheme( string scheme, IEnumerable<URI> uris )
		 {
			  return firstOrNull( filter( item => item.Scheme.Equals( scheme ), uris ) );
		 }

		 private void AssertMasterAndSlaveInformation( ClusterMemberInfo[] instancesInCluster )
		 {
			  ClusterMemberInfo master = Member( instancesInCluster, 1 );
			  assertEquals( 1137, GetUriForScheme( "ha", Iterables.map( URI.create, Arrays.asList( master.Uris ) ) ).Port );
			  assertEquals( HighAvailabilityModeSwitcher.MASTER, master.HaRole );

			  ClusterMemberInfo slave = Member( instancesInCluster, 2 );
			  assertEquals( 1138, GetUriForScheme( "ha", Iterables.map( URI.create, Arrays.asList( slave.Uris ) ) ).Port );
			  assertEquals( HighAvailabilityModeSwitcher.SLAVE, slave.HaRole );
			  assertTrue( "Slave not available", slave.Available );
		 }

		 private ClusterMemberInfo Member( ClusterMemberInfo[] members, int instanceId )
		 {
			  foreach ( ClusterMemberInfo member in members )
			  {
					if ( member.InstanceId.Equals( Convert.ToString( instanceId ) ) )
					{
						 return member;
					}
			  }
			  fail( "Couldn't find cluster member with cluster URI port " + instanceId + " among " + Arrays.ToString( members ) );
			  return null; // it will never get here.
		 }
	}

}
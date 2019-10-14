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
namespace Neo4Net.Kernel.ha.cluster
{
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MasterServer = Neo4Net.Kernel.ha.com.master.MasterServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class SwitchToMasterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToMasterShouldUseConfigSettingIfSuitable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchToMasterShouldUseConfigSettingIfSuitable()
		 {
			  // given
			  Config config = Config.defaults( stringMap( ClusterSettings.server_id.name(), "1", HaSettings.ha_server.name(), "192.168.1.99:6001" ) );
			  URI me = new URI( "ha://127.0.0.1" );

			  MasterServer masterServer = mock( typeof( MasterServer ) );

			  // when
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( "192.168.1.1", 6001 ) );

			  URI result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://192.168.1.99:6001?serverId=1", result.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToMasterShouldUseIPv6ConfigSettingIfSuitable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchToMasterShouldUseIPv6ConfigSettingIfSuitable()
		 {
			  // given
			  Config config = Config.defaults( stringMap( ClusterSettings.server_id.name(), "1", HaSettings.ha_server.name(), "[fe80::1]:6001" ) );
			  URI me = new URI( "ha://[::1]" );

			  MasterServer masterServer = mock( typeof( MasterServer ) );

			  // when
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( "[fe80::1]", 6001 ) );

			  URI result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://[fe80::1]:6001?serverId=1", result.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToMasterShouldIgnoreWildcardInConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchToMasterShouldIgnoreWildcardInConfig()
		 {
			  // SwitchToMaster is used to advertise to the rest of the cluster and advertising 0.0.0.0 makes no sense

			  // given
			  Config config = Config.defaults( stringMap( ClusterSettings.server_id.name(), "1", HaSettings.ha_server.name(), "0.0.0.0:6001" ) );
			  URI me = new URI( "ha://127.0.0.1" );

			  MasterServer masterServer = mock( typeof( MasterServer ) );

			  // when
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( "192.168.1.1", 6001 ) );

			  URI result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://192.168.1.1:6001?serverId=1", result.ToString() );

			  // when masterServer is 0.0.0.0
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( 6001 ) );

			  result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://127.0.0.1:6001?serverId=1", result.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToMasterShouldIgnoreIPv6WildcardInConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchToMasterShouldIgnoreIPv6WildcardInConfig()
		 {
			  // SwitchToMaster is used to advertise to the rest of the cluster and advertising 0.0.0.0 makes no sense

			  // given
			  Config config = Config.defaults( stringMap( ClusterSettings.server_id.name(), "1", HaSettings.ha_server.name(), "[::]:6001" ) );
			  URI me = new URI( "ha://[::1]" );

			  MasterServer masterServer = mock( typeof( MasterServer ) );

			  // when
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( "[fe80::1]", 6001 ) );

			  URI result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://[fe80:0:0:0:0:0:0:1]:6001?serverId=1", result.ToString() );

			  // when masterServer is 0.0.0.0
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( 6001 ) );

			  result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://[::1]:6001?serverId=1", result.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToMasterShouldHandleNoIpInConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchToMasterShouldHandleNoIpInConfig()
		 {
			  Config config = Config.defaults( stringMap( ClusterSettings.server_id.name(), "1", HaSettings.ha_server.name(), ":6001" ) );

			  MasterServer masterServer = mock( typeof( MasterServer ) );
			  URI me = new URI( "ha://127.0.0.1" );

			  // when
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( "192.168.1.1", 6001 ) );

			  URI result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://192.168.1.1:6001?serverId=1", result.ToString() );

			  // when masterServer is 0.0.0.0
			  when( masterServer.SocketAddress ).thenReturn( new InetSocketAddress( 6001 ) );

			  result = SwitchToMaster.GetMasterUri( me, masterServer, config );

			  // then
			  assertEquals( "Wrong address", "ha://127.0.0.1:6001?serverId=1", result.ToString() );
		 }
	}

}
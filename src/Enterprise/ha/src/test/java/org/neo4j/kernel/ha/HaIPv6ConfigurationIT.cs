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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using HighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.HighlyAvailableGraphDatabaseFactory;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	/// <summary>
	/// Test various IPv6 configuration options on a single HA instance.
	/// </summary>
	public class HaIPv6ConfigurationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClusterWithLocalhostAddresses()
		 public virtual void TestClusterWithLocalhostAddresses()
		 {
			  int clusterPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = ( new HighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Dir.storeDir()).setConfig(ClusterSettings.cluster_server, Ipv6HostPortSetting("::1", clusterPort)).setConfig(ClusterSettings.initial_hosts, Ipv6HostPortSetting("::1", clusterPort)).setConfig(HaSettings.HaServer, Ipv6HostPortSetting("::1", PortAuthority.allocatePort())).setConfig(ClusterSettings.server_id, "1").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClusterWithLinkLocalAddress() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClusterWithLinkLocalAddress()
		 {
			  InetAddress inetAddress;

			  IEnumerator<NetworkInterface> nics = NetworkInterface.NetworkInterfaces;
			  while ( nics.MoveNext() )
			  {
					NetworkInterface nic = nics.Current;
					IEnumerator<InetAddress> inetAddresses = nic.InetAddresses;
					while ( inetAddresses.MoveNext() )
					{
						 inetAddress = inetAddresses.Current;
						 if ( inetAddress is Inet6Address && inetAddress.LinkLocalAddress )
						 {
							  try
							  {
									if ( inetAddress.isReachable( 1000 ) )
									{
										 TestWithAddress( inetAddress );
									}
							  }
							  catch ( ConnectException )
							  {
									// fine, just ignore
							  }
						 }
					}
			  }
		 }

		 private void TestWithAddress( InetAddress inetAddress )
		 {
			  int clusterPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = ( new HighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Dir.storeDir()).setConfig(ClusterSettings.cluster_server, Ipv6HostPortSetting(inetAddress.HostAddress, clusterPort)).setConfig(ClusterSettings.initial_hosts, Ipv6HostPortSetting(inetAddress.HostAddress, clusterPort)).setConfig(HaSettings.HaServer, Ipv6HostPortSetting("::", PortAuthority.allocatePort())).setConfig(ClusterSettings.server_id, "1").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClusterWithWildcardAddresses()
		 public virtual void TestClusterWithWildcardAddresses()
		 {
			  int clusterPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = ( new HighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Dir.storeDir()).setConfig(ClusterSettings.cluster_server, Ipv6HostPortSetting("::", clusterPort)).setConfig(ClusterSettings.initial_hosts, Ipv6HostPortSetting("::1", clusterPort)).setConfig(HaSettings.HaServer, Ipv6HostPortSetting("::", PortAuthority.allocatePort())).setConfig(ClusterSettings.server_id, "1").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  Db.shutdown();
		 }

		 private static string Ipv6HostPortSetting( string address, int port )
		 {
			  return "[" + address + "]:" + port;
		 }
	}

}
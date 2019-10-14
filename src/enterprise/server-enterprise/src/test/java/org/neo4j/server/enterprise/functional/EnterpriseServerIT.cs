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
namespace Neo4Net.Server.enterprise.functional
{
	using Client = com.sun.jersey.api.client.Client;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.cluster_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.initial_hosts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.server_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.mode;

	public class EnterpriseServerIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TemporaryFolder folder = new org.junit.rules.TemporaryFolder();
		 public readonly TemporaryFolder Folder = new TemporaryFolder();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStartInHAMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStartInHAMode()
		 {
			  // Given
			  int clusterPort = PortAuthority.allocatePort();
			  NeoServer server = EnterpriseServerBuilder.serverOnRandomPorts().usingDataDir(Folder.Root.AbsolutePath).withProperty(mode.name(), "HA").withProperty(server_id.name(), "1").withProperty(cluster_server.name(), ":" + clusterPort).withProperty(initial_hosts.name(), ":" + clusterPort).persistent().build();

			  try
			  {
					server.Start();
					server.Database;

					assertThat( server.Database.Graph, @is( instanceOf( typeof( HighlyAvailableGraphDatabase ) ) ) );

					HTTP.Response haEndpoint = HTTP.GET( GetHaEndpoint( server ) );
					assertEquals( 200, haEndpoint.Status() );
					assertThat( haEndpoint.RawContent(), containsString("master") );

					HTTP.Response discovery = HTTP.GET( server.BaseUri().toASCIIString() );
					assertEquals( 200, discovery.Status() );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequireAuthorizationForHAStatusEndpoints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRequireAuthorizationForHAStatusEndpoints()
		 {
			  // Given
			  int clusterPort = PortAuthority.allocatePort();
			  NeoServer server = EnterpriseServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), "true").usingDataDir(Folder.Root.AbsolutePath).withProperty(mode.name(), "HA").withProperty(server_id.name(), "1").withProperty(cluster_server.name(), ":" + clusterPort).withProperty(initial_hosts.name(), ":" + clusterPort).persistent().build();

			  try
			  {
					server.Start();
					server.Database;

					assertThat( server.Database.Graph, @is( instanceOf( typeof( HighlyAvailableGraphDatabase ) ) ) );

					Client client = Client.create();
					ClientResponse r = client.resource( GetHaEndpoint( server ) ).accept( APPLICATION_JSON ).get( typeof( ClientResponse ) );
					assertEquals( 401, r.Status );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDisablingAuthorizationOnHAStatusEndpoints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDisablingAuthorizationOnHAStatusEndpoints()
		 {
			  // Given
			  int clusterPort = PortAuthority.allocatePort();
			  NeoServer server = EnterpriseServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), "true").withProperty(HaSettings.ha_status_auth_enabled.name(), "false").usingDataDir(Folder.Root.AbsolutePath).withProperty(mode.name(), "HA").withProperty(server_id.name(), "1").withProperty(cluster_server.name(), ":" + clusterPort).withProperty(initial_hosts.name(), ":" + clusterPort).persistent().build();

			  try
			  {
					server.Start();
					server.Database;

					assertThat( server.Database.Graph, @is( instanceOf( typeof( HighlyAvailableGraphDatabase ) ) ) );

					Client client = Client.create();
					ClientResponse r = client.resource( GetHaEndpoint( server ) ).accept( APPLICATION_JSON ).get( typeof( ClientResponse ) );
					assertEquals( 200, r.Status );
					assertThat( r.getEntity( typeof( string ) ), containsString( "master" ) );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }

		 private string GetHaEndpoint( NeoServer server )
		 {
			  return server.BaseUri().ToString() + "db/manage/server/ha";
		 }
	}

}
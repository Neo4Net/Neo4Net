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
namespace Org.Neo4j.Server.rest
{
	using Test = org.junit.Test;


	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	public class EnterpriseDiscoverableURIsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExposeBoltRoutingIfCore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExposeBoltRoutingIfCore()
		 {
			  // Given
			  BoltConnector bolt = new BoltConnector( "honestJakesBoltConnector" );
			  Config config = Config.builder().withSetting(EnterpriseEditionSettings.mode, EnterpriseEditionSettings.Mode.CORE.name()).withSetting(bolt.Enabled, "true").withSetting(bolt.Type, BoltConnector.ConnectorType.BOLT.name()).build();

			  // When
			  IDictionary<string, object> asd = ToMap( EnterpriseDiscoverableURIs.EnterpriseDiscoverableURIsConflict( config, new ConnectorPortRegister() ) );

			  // Then
			  assertThat( asd["bolt_routing"], equalTo( "bolt+routing://localhost:7687" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGrabPortFromRegisterIfSetTo0() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGrabPortFromRegisterIfSetTo0()
		 {
			  // Given
			  BoltConnector bolt = new BoltConnector( "honestJakesBoltConnector" );
			  Config config = Config.builder().withSetting(EnterpriseEditionSettings.mode, EnterpriseEditionSettings.Mode.CORE.name()).withSetting(bolt.Enabled, "true").withSetting(bolt.Type, BoltConnector.ConnectorType.BOLT.name()).withSetting(bolt.ListenAddress, ":0").build();
			  ConnectorPortRegister ports = new ConnectorPortRegister();
			  ports.Register( bolt.Key(), new InetSocketAddress(1337) );

			  // When
			  IDictionary<string, object> asd = ToMap( EnterpriseDiscoverableURIs.EnterpriseDiscoverableURIsConflict( config, ports ) );

			  // Then
			  assertThat( asd["bolt_routing"], equalTo( "bolt+routing://localhost:1337" ) );
		 }

		 private IDictionary<string, object> ToMap( DiscoverableURIs uris )
		 {
			  IDictionary<string, object> @out = new Dictionary<string, object>();
			  uris.ForEach( ( k, v ) => @out.put( k, v.toASCIIString() ) );
			  //uris.forEach( out::put );
			  return @out;
		 }
	}

}
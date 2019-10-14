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
namespace Neo4Net.causalclustering.discovery
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.initial_discovery_members;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class InitialDiscoveryMembersResolverTest
	public class InitialDiscoveryMembersResolverTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private HostnameResolver hostnameResolver;
		 private HostnameResolver _hostnameResolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyCollectionIfEmptyInitialMembers()
		 public virtual void ShouldReturnEmptyCollectionIfEmptyInitialMembers()
		 {
			  // given
			  Config config = Config.builder().withSetting(initial_discovery_members, "").build();

			  InitialDiscoveryMembersResolver hostnameResolvingInitialDiscoveryMembersResolver = new InitialDiscoveryMembersResolver( _hostnameResolver, config );

			  // when
			  ICollection<AdvertisedSocketAddress> result = hostnameResolvingInitialDiscoveryMembersResolver.Resolve( identity() );

			  // then
			  assertThat( result, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveAndReturnAllConfiguredAddresses()
		 public virtual void ShouldResolveAndReturnAllConfiguredAddresses()
		 {
			  // given
			  AdvertisedSocketAddress input1 = new AdvertisedSocketAddress( "foo.bar", 123 );
			  AdvertisedSocketAddress input2 = new AdvertisedSocketAddress( "baz.bar", 432 );
			  AdvertisedSocketAddress input3 = new AdvertisedSocketAddress( "quux.bar", 789 );

			  AdvertisedSocketAddress output1 = new AdvertisedSocketAddress( "a.b", 3 );
			  AdvertisedSocketAddress output2 = new AdvertisedSocketAddress( "b.b", 34 );
			  AdvertisedSocketAddress output3 = new AdvertisedSocketAddress( "c.b", 7 );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  string configString = Stream.of( input1, input2, input3 ).map( AdvertisedSocketAddress::toString ).collect( Collectors.joining( "," ) );

			  Config config = Config.builder().withSetting(initial_discovery_members, configString).build();

			  when( _hostnameResolver.resolve( input1 ) ).thenReturn( asList( output1, output2 ) );
			  when( _hostnameResolver.resolve( input2 ) ).thenReturn( emptyList() );
			  when( _hostnameResolver.resolve( input3 ) ).thenReturn( singletonList( output3 ) );

			  InitialDiscoveryMembersResolver hostnameResolvingInitialDiscoveryMembersResolver = new InitialDiscoveryMembersResolver( _hostnameResolver, config );

			  // when
			  ICollection<AdvertisedSocketAddress> result = hostnameResolvingInitialDiscoveryMembersResolver.Resolve( identity() );

			  // then
			  assertThat( result, containsInAnyOrder( output1, output2, output3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyTransform()
		 public virtual void ShouldApplyTransform()
		 {
			  // given
			  AdvertisedSocketAddress input1 = new AdvertisedSocketAddress( "foo.bar", 123 );

			  AdvertisedSocketAddress output1 = new AdvertisedSocketAddress( "a.b", 3 );

			  Config config = Config.builder().withSetting(initial_discovery_members, input1.ToString()).build();

			  when( _hostnameResolver.resolve( input1 ) ).thenReturn( singletonList( output1 ) );

			  InitialDiscoveryMembersResolver hostnameResolvingInitialDiscoveryMembersResolver = new InitialDiscoveryMembersResolver( _hostnameResolver, config );

			  // when
			  ICollection<string> result = hostnameResolvingInitialDiscoveryMembersResolver.Resolve( address => address.ToString().ToUpper() );

			  // then
			  assertThat( result, containsInAnyOrder( output1.ToString().ToUpper() ) );
		 }
	}

}
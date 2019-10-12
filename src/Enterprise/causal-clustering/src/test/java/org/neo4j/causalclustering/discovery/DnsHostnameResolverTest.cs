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
namespace Neo4Net.causalclustering.discovery
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.MultiRetryStrategyTest.testRetryStrategy;

	public class DnsHostnameResolverTest
	{
		private bool InstanceFieldsInitialized = false;

		public DnsHostnameResolverTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_resolver = new DnsHostnameResolver( new SimpleLogService( _userLogProvider, _logProvider ), _mockDomainNameResolver, _config, testRetryStrategy( 1 ) );
		}

		 private readonly MapDomainNameResolver _mockDomainNameResolver = new MapDomainNameResolver( new Dictionary<string, InetAddress[]>() );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private readonly Config _config = Config.defaults( CausalClusteringSettings.minimum_core_cluster_size_at_formation, "2" );

		 private DnsHostnameResolver _resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hostnamesAreResolvedByTheResolver()
		 public virtual void HostnamesAreResolvedByTheResolver()
		 {
			  // given
			  _mockDomainNameResolver.setHostnameAddresses( "google.com", asList( "1.2.3.4", "5.6.7.8" ) );

			  // when
			  ICollection<AdvertisedSocketAddress> resolvedAddresses = _resolver.resolve( new AdvertisedSocketAddress( "google.com", 80 ) );

			  // then
			  assertEquals( 2, resolvedAddresses.Count );
			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "1.2.3.4" ) ) );
			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "5.6.7.8" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resolvedHostnamesUseTheSamePort()
		 public virtual void ResolvedHostnamesUseTheSamePort()
		 {
			  // given
			  _mockDomainNameResolver.setHostnameAddresses( "google.com", asList( "1.2.3.4", "5.6.7.8" ) );

			  // when
			  IList<AdvertisedSocketAddress> resolvedAddresses = new List<AdvertisedSocketAddress>( _resolver.resolve( new AdvertisedSocketAddress( "google.com", 1234 ) ) );

			  // then
			  assertEquals( 2, resolvedAddresses.Count );
			  assertEquals( 1234, resolvedAddresses[0].Port );
			  assertEquals( 1234, resolvedAddresses[1].Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resolutionDetailsAreLoggedToUserLogs()
		 public virtual void ResolutionDetailsAreLoggedToUserLogs()
		 {
			  // given
			  _mockDomainNameResolver.setHostnameAddresses( "google.com", asList( "1.2.3.4", "5.6.7.8" ) );

			  // when
			  _resolver.resolve( new AdvertisedSocketAddress( "google.com", 1234 ) );

			  // then
			  _userLogProvider.rawMessageMatcher().assertContainsSingle(Matchers.allOf(Matchers.containsString("Resolved initial host '%s' to %s")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unknownHostExceptionsAreLoggedAsErrors()
		 public virtual void UnknownHostExceptionsAreLoggedAsErrors()
		 {
			  // when
			  _resolver.resolve( new AdvertisedSocketAddress( "google.com", 1234 ) );

			  // then
			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Failed to resolve host '%s'")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resolverRetriesUntilHostnamesAreFound()
		 public virtual void ResolverRetriesUntilHostnamesAreFound()
		 {
			  // given
			  _mockDomainNameResolver.setHostnameAddresses( "google.com", asList( "1.2.3.4", "5.6.7.8" ) );
			  DomainNameResolver mockResolver = spy( _mockDomainNameResolver );
			  when( mockResolver.ResolveDomainName( anyString() ) ).thenReturn(new InetAddress[0]).thenReturn(new InetAddress[0]).thenCallRealMethod();

			  DnsHostnameResolver resolver = new DnsHostnameResolver( new SimpleLogService( _userLogProvider, _logProvider ), mockResolver, _config, testRetryStrategy( 2 ) );

			  // when
			  IList<AdvertisedSocketAddress> resolvedAddresses = new List<AdvertisedSocketAddress>( resolver.Resolve( new AdvertisedSocketAddress( "google.com", 1234 ) ) );

			  // then
			  verify( mockResolver, times( 3 ) ).resolveDomainName( "google.com" );
			  assertEquals( 2, resolvedAddresses.Count );
			  assertEquals( 1234, resolvedAddresses[0].Port );
			  assertEquals( 1234, resolvedAddresses[1].Port );
		 }
	}

}
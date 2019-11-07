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
namespace Neo4Net.causalclustering.discovery
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;

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
//	import static Neo4Net.causalclustering.discovery.MultiRetryStrategyTest.testRetryStrategy;

	public class SrvHostnameResolverTest
	{
		private bool InstanceFieldsInitialized = false;

		public SrvHostnameResolverTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_resolver = new SrvHostnameResolver( new SimpleLogService( _userLogProvider, _logProvider ), mockSrvRecordResolver, _config, testRetryStrategy( 1 ) );
		}

		 private readonly MockSrvRecordResolver mockSrvRecordResolver = new MockSrvRecordResolver( new HashMapAnonymousInnerClass() );

		 private class HashMapAnonymousInnerClass : Dictionary<string, IList<SrvRecordResolver.SrvRecord>>
		 {
	//		 {
	//			  put("emptyrecord.com", new ArrayList<>());
	//		 }
		 }

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private readonly Config _config = Config.defaults( CausalClusteringSettings.minimum_core_cluster_size_at_formation, "2" );

		 private SrvHostnameResolver _resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hostnamesAndPortsAreResolvedByTheResolver()
		 public virtual void HostnamesAndPortsAreResolvedByTheResolver()
		 {
			  // given
			  mockSrvRecordResolver.addRecords( "_discovery._tcp.google.com", asList( SrvRecordResolver.SrvRecord.Parse( "1 1 80 1.2.3.4" ), SrvRecordResolver.SrvRecord.Parse( "1 1 8080 5.6.7.8" ) ) );

			  // when
			  ICollection<AdvertisedSocketAddress> resolvedAddresses = _resolver.resolve(new AdvertisedSocketAddress("_discovery._tcp.google.com", 0)
			 );

			  // then
			  assertEquals( 2, resolvedAddresses.Count );

			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "1.2.3.4" ) && address.Port == 80 ) );

			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "5.6.7.8" ) && address.Port == 8080 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resolutionDetailsAreLoggedToUserLogs()
		 public virtual void ResolutionDetailsAreLoggedToUserLogs()
		 {
			  // given
			  mockSrvRecordResolver.addRecord( "_resolutionDetailsAreLoggedToUserLogs._test.Neo4Net.com", SrvRecordResolver.SrvRecord.Parse( "1 1 4321 1.2.3.4" ) );

			  // when
			  _resolver.resolve(new AdvertisedSocketAddress("_resolutionDetailsAreLoggedToUserLogs._test.Neo4Net.com", 0)
			 );

			  // then
			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Resolved initial host '%s' to %s")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unknownHostExceptionsAreLoggedAsErrors()
		 public virtual void UnknownHostExceptionsAreLoggedAsErrors()
		 {
			  // when
			  _resolver.resolve( new AdvertisedSocketAddress( "unknown.com", 0 ) );

			  // then
			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Failed to resolve srv records for '%s'")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyRecordListsAreLoggedAsErrors()
		 public virtual void EmptyRecordListsAreLoggedAsErrors()
		 {
			  // when
			  _resolver.resolve( new AdvertisedSocketAddress( "emptyrecord.com", 0 ) );

			  // then
			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Failed to resolve srv records for '%s'")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resolverRetriesUntilHostnamesAreFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ResolverRetriesUntilHostnamesAreFound()
		 {
			  // given
			  mockSrvRecordResolver.addRecords( "_discovery._tcp.google.com", asList( SrvRecordResolver.SrvRecord.Parse( "1 1 80 1.2.3.4" ), SrvRecordResolver.SrvRecord.Parse( "1 1 8080 5.6.7.8" ) ) );
			  SrvRecordResolver mockResolver = spy( mockSrvRecordResolver );
			  when( mockResolver.ResolveSrvRecord( anyString() ) ).thenReturn(Stream.empty()).thenReturn(Stream.empty()).thenCallRealMethod();

			  SrvHostnameResolver resolver = new SrvHostnameResolver( new SimpleLogService( _userLogProvider, _logProvider ), mockResolver, _config, testRetryStrategy( 2 ) );

			  // when
			  ICollection<AdvertisedSocketAddress> resolvedAddresses = resolver.resolve(new AdvertisedSocketAddress("_discovery._tcp.google.com", 0)
			 );

			  // then
			  verify( mockResolver, times( 3 ) ).resolveSrvRecord( "_discovery._tcp.google.com" );

			  assertEquals( 2, resolvedAddresses.Count );

			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "1.2.3.4" ) && address.Port == 80 ) );

			  assertTrue( resolvedAddresses.removeIf( address => address.Hostname.Equals( "5.6.7.8" ) && address.Port == 8080 ) );
		 }
	}

}
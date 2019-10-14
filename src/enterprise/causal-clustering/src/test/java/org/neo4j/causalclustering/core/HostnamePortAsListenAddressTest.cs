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
namespace Neo4Net.causalclustering.core
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class HostnamePortAsListenAddressTest
	public class HostnamePortAsListenAddressTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { "[Should ignore port range correctly]", "localhost", 4343, int?.of( 4646 ), false },
				  new object[] { "[Should handle hostname with dashes]", "foo-bar", 1234, int?.empty(), false },
				  new object[] { "[Should handle standard hostname with port]", "localhost", 251, int?.empty(), false },
				  new object[] { "[Should handle hostname with tld ext]", "neo4j.org", 1212, int?.empty(), false },
				  new object[] { "[Should ignore port range for hostname with tld ext]", "neo4j.org", 1212, int?.of( 2121 ), false },
				  new object[] { "[Should handle hostname with sub-domain]", "test.neo4j.org", 1212, int?.empty(), false },
				  new object[] { "[Should handle ipv4 hostname]", "8.8.8.8", 1212, int?.of( 2121 ), false },
				  new object[] { "[Should handle ipv6 hostname]", "[2001:cdba:0000:0000:0000:0000:3257:9652]", 1212, int?.empty(), true },
				  new object[] { "[Should handle ipv6 hostname with port range]", "[2001:cdba::3257:9652]", 1212, int?.of( 2121 ), true }
			  });
		 }

		 private readonly string _hostname;
		 private readonly int _port;
		 private readonly int? _portRange;
		 private readonly bool _isIpV6;

		 public HostnamePortAsListenAddressTest( string ignoredName, string hostname, int port, int? portRange, bool isIpV6 )
		 {
			  this._hostname = hostname;
			  this._port = port;
			  this._portRange = portRange;
			  this._isIpV6 = isIpV6;
		 }

		 private string CombinedHostname()
		 {
			  string portRangeStr = _portRange.HasValue ? "-" + _portRange.Value : "";
			  return _hostname + ":" + _port + portRangeStr;
		 }

		 private string SanitizedHostname
		 {
			 get
			 {
				  if ( _isIpV6 )
				  {
						if ( !( _hostname.StartsWith( "[", StringComparison.Ordinal ) && _hostname.EndsWith( "]", StringComparison.Ordinal ) ) )
						{
							 throw new System.ArgumentException( "Test indicates an IpV6 hostname and port but isn't surrounded by []" );
						}
						return _hostname.Substring( 1, ( _hostname.Length - 1 ) - 1 );
				  }
				  else
				  {
						return _hostname;
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseHostnamePortCorrectly()
		 public virtual void ShouldParseHostnamePortCorrectly()
		 {
			  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_server, CombinedHostname()).build();
			  ListenSocketAddress listenSocketAddress = HostnamePortAsListenAddress.Resolve( config, OnlineBackupSettings.online_backup_server );

			  assertEquals( new ListenSocketAddress( SanitizedHostname, _port ), listenSocketAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.config.InvalidSettingException.class) public void shouldThrowInvalidSettingsExceptionOnEmptyConfig()
		 public virtual void ShouldThrowInvalidSettingsExceptionOnEmptyConfig()
		 {
			  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_server, "").build();
			  ListenSocketAddress listenSocketAddress = HostnamePortAsListenAddress.Resolve( config, OnlineBackupSettings.online_backup_server );
			  assertEquals( OnlineBackupSettings.online_backup_server.DefaultValue, listenSocketAddress.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.config.InvalidSettingException.class) public void shouldThrowInvalidSettingsExceptionOnBadFormat()
		 public virtual void ShouldThrowInvalidSettingsExceptionOnBadFormat()
		 {
			  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_server, "localhost").build();
			  ListenSocketAddress listenSocketAddress = HostnamePortAsListenAddress.Resolve( config, OnlineBackupSettings.online_backup_server );
			  assertEquals( OnlineBackupSettings.online_backup_server.DefaultValue, listenSocketAddress.ToString() );
		 }
	}

}
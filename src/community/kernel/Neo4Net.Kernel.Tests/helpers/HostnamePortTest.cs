using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Helpers
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assumptions.assumeFalse;

	internal class HostnamePortTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHostnameOnly()
		 internal virtual void TestHostnameOnly()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "myhost" );
			  assertThat( hostnamePort.Host, equalTo( "myhost" ) );
			  assertThat( hostnamePort.Port, equalTo( 0 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[]{ 0, 0 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHostnamePort()
		 internal virtual void TestHostnamePort()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "myhost:1234" );
			  assertThat( hostnamePort.Host, equalTo( "myhost" ) );
			  assertThat( hostnamePort.Port, equalTo( 1234 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[] { 1234, 1234 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHostnamePortRange()
		 internal virtual void TestHostnamePortRange()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "myhost:1234-1243" );
			  assertThat( hostnamePort.Host, equalTo( "myhost" ) );
			  assertThat( hostnamePort.Port, equalTo( 1234 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[] { 1234, 1243 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHostnamePortRangeInversed()
		 internal virtual void TestHostnamePortRangeInversed()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "myhost:1243-1234" );
			  assertThat( hostnamePort.Host, equalTo( "myhost" ) );
			  assertThat( hostnamePort.Port, equalTo( 1243 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[] { 1243, 1234 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testSinglePortOnly()
		 internal virtual void TestSinglePortOnly()
		 {
			  HostnamePort hostnamePort = new HostnamePort( ":1234" );
			  assertNull( hostnamePort.Host );
			  assertThat( hostnamePort.Port, equalTo( 1234 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[] { 1234, 1234 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPortRangeOnly()
		 internal virtual void TestPortRangeOnly()
		 {
			  HostnamePort hostnamePort = new HostnamePort( ":1230-1240" );
			  assertNull( hostnamePort.Host );
			  assertThat( hostnamePort.Port, equalTo( 1230 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[] { 1230, 1240 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDefaultHost()
		 internal virtual void TestDefaultHost()
		 {
			  HostnamePort hostnamePort = new HostnamePort( ":1234" );
			  assertThat( hostnamePort.GetHost( "1.2.3.4" ), equalTo( "1.2.3.4" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetHostAddress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestGetHostAddress()
		 {
			  // Given
			  string hostName = InetAddress.LocalHost.HostName;

			  // When & Then

			  // should return default, when host is null
			  assertThat( HostnamePort.GetHostAddress( null, "default" ), equalTo( "default" ) );

			  // should return host ip address when host is known
			  assertThat( HostnamePort.GetHostAddress( hostName, "default" ), equalTo( hostName ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetHostAddressUnknown()
		 internal virtual void TestGetHostAddressUnknown()
		 {
			  // Given
			  string unknownHost = "unknownHost";

			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost) );

			  // should return hostname when it is unknown
			  assertThat( HostnamePort.GetHostAddress( unknownHost, "default" ), equalTo( unknownHost ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesUnknownHosts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesUnknownHosts()
		 {
			  // Given
			  string knownHost = InetAddress.LocalHost.HostName;
			  string unknownHost1 = "unknownHost1";
			  string unknownHost2 = "unknownHost2";

			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost1) );
			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost2) );

			  HostnamePort hostnamePortSinglePort = new HostnamePort( unknownHost1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( unknownHost1 + ":1234-1236" );

			  // When & Then

			  // Should match, same host and port
			  assertTrue( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost1 + ":1234" ) ) );
			  // Should fail, different host or port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost1 + ":1235" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost2 + ":1234" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost2 + ":1235" ) ) );
			  // Should fail, no port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost1 + "" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost2 + "" ) ) );

			  // Unknown host should never match with any IP or known host
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://1.2.3.4:1234" ) ) );

			  // Should return false with any other known host
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + knownHost + ":1234" ) ) );

			  // Should match, port in range and host the same
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 + ":1234" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 + ":1235" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 + ":1236" ) ) );
			  // Should not match, different host
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost2 + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost2 + ":1235" ) ) );
			  // Should not match, port outside of range
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 + ":1233" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 + ":1237" ) ) );
			  // Should not match, no port
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost1 ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesKnownHostWithIP() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesKnownHostWithIP()
		 {
			  // Given

			  string hostname1 = InetAddress.LocalHost.HostName.replace( '.', '-' );
			  string host1 = InetAddress.LocalHost.HostAddress;
			  // Building fake IP for host2
			  StringBuilder host2 = new StringBuilder();
			  string[] host1Parts = host1.Split( "\\.", true );
			  foreach ( string part in host1Parts )
			  {
					int partnum = int.Parse( part );
					host2.Append( ++partnum % 256 + "." );
			  }
			  host2.Remove( host2.Length - 1, 1 );

			  HostnamePort hostnamePortSinglePort = new HostnamePort( hostname1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( hostname1 + ":1234-1236" );

			  // When & Then

			  // Should match, same host and port
			  assertTrue( hostnamePortSinglePort.Matches( URI.create( "ha://" + hostname1 + ":1234" ) ) );
			  // Should fail, different host or port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + hostname1 + ":1235" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 + ":1234" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 + ":1235" ) ) );
			  // Should fail, no port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 ) ) );

			  // Should match, port in range and host the same
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 + ":1234" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 + ":1235" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 + ":1236" ) ) );
			  // Should not match, different host
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 + ":1235" ) ) );
			  // Should not match, port outside of range
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 + ":1233" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 + ":1237" ) ) );
			  // Should not match, no port
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname1 ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesIPWithHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesIPWithHost()
		 {
			  // Given
			  string hostname1 = InetAddress.LocalHost.HostName;
			  string host1 = InetAddress.LocalHost.HostAddress;
			  string hostname2 = "Neo4Net.org";

			  assertDoesNotThrow( () => InetAddress.getByName(hostname2) );
			  assumeFalse( hostname1.Equals( hostname2 ) );

			  HostnamePort hostnamePortSinglePort = new HostnamePort( host1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( host1 + ":1234-1236" );

			  // When & Then

			  // Should match, same host and port
			  assertTrue( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 + ":1234" ) ) );
			  // Should fail, different host or port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 + ":1235" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + hostname2 + ":1234" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + hostname2 + ":1235" ) ) );
			  // Should fail, no port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + hostname2 ) ) );

			  // Should match, port in range and host the same
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1234" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1235" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1236" ) ) );
			  // Should not match, different host
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname2 + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname2 + ":1235" ) ) );
			  // Should not match, port outside of range
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1233" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1237" ) ) );
			  // Should not match, no port
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + hostname2 ) ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesIPWithHostUnknown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesIPWithHostUnknown()
		 {
			  // Given
			  string unknownHost = "unknownHost";
			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost) );

			  string host1 = InetAddress.LocalHost.HostAddress;

			  HostnamePort hostnamePortSinglePort = new HostnamePort( host1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( host1 + ":1234-1236" );

			  // When & Then

			  // should return false if matched with any unknown host
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1235" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1236" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesKnownHostWithHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesKnownHostWithHost()
		 {
			  // Given

			  string host1 = InetAddress.LocalHost.HostName.replace( '.', '-' );
			  // any other hostname?
			  string host2 = "Neo4Net.org";

			  assertDoesNotThrow( () => InetAddress.getByName(host2) );
			  assumeFalse( host1.Equals( host2 ) );

			  HostnamePort hostnamePortSinglePort = new HostnamePort( host1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( host1 + ":1234-1236" );

			  // When & Then

			  // Should match, same host and port
			  assertTrue( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 + ":1234" ) ) );
			  // Should fail, different host or port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 + ":1235" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 + ":1234" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 + ":1235" ) ) );
			  // Should fail, no port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host2 ) ) );

			  // Should match, port in range and host the same
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1234" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1235" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1236" ) ) );
			  // Should not match, different host
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 + ":1235" ) ) );
			  // Should not match, port outside of range
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1233" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 + ":1237" ) ) );
			  // Should not match, no port
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host1 ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + host2 ) ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesKnownHostWithHostUnknown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesKnownHostWithHostUnknown()
		 {
			  // Given
			  string host1 = InetAddress.LocalHost.HostName;
			  string unknownHost = "unknownHost";

			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost) );

			  HostnamePort hostnamePortSinglePort = new HostnamePort( host1 + ":1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( host1 + ":1234-1236" );

			  // When & Then

			  // should return false if matched with any unknown host
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1235" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://" + unknownHost + ":1236" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesIP()
		 internal virtual void TestMatchesIP()
		 {
			  // Given

			  HostnamePort hostnamePortSinglePort = new HostnamePort( "1.2.3.4:1234" );
			  HostnamePort hostnamePortWithRange = new HostnamePort( "1.2.3.4:1234-1236" );

			  // When & Then

			  // Should match, same host and port
			  assertTrue( hostnamePortSinglePort.Matches( URI.create( "ha://1.2.3.4:1234" ) ) );
			  // Should fail, different host or port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://1.2.3.4:1235" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://5.6.7.8:1234" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://5.6.7.8:1235" ) ) );
			  // Should fail, no port
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://1.2.3.4" ) ) );
			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://5.6.7.8" ) ) );

			  // Should match, port in range and host the same
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4:1234" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4:1235" ) ) );
			  assertTrue( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4:1236" ) ) );
			  // Should not match, different host
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://5.6.7.8:1234" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://5.6.7.8:1235" ) ) );
			  // Should not match, port outside of range
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4:1233" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4:1237" ) ) );
			  // Should not match, no port
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://1.2.3.4" ) ) );
			  assertFalse( hostnamePortWithRange.Matches( URI.create( "ha://5.6.7.8" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesNullHostWithUnknownHost()
		 internal virtual void TestMatchesNullHostWithUnknownHost()
		 {
			  // Given
			  HostnamePort hostnamePortSinglePort = new HostnamePort( ":1234" );
			  string unknownHost = "unknownHost";
			  assertThrows( typeof( UnknownHostException ), () => InetAddress.getByName(unknownHost) );
			  // When & Then

			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + unknownHost + ":1234" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesNullHostWithIP() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesNullHostWithIP()
		 {
			  HostnamePort hostnamePortSinglePort = new HostnamePort( ":1234" );
			  string host1IP = InetAddress.LocalHost.HostAddress;

			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1IP + ":1234" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatchesNullHostWithKnownHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestMatchesNullHostWithKnownHost()
		 {
			  HostnamePort hostnamePortSinglePort = new HostnamePort( ":1234" );
			  string host1 = InetAddress.LocalHost.HostName;

			  assertFalse( hostnamePortSinglePort.Matches( URI.create( "ha://" + host1 + ":1234" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIPv6Address()
		 internal virtual void TestIPv6Address()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "[2001:cdba:0:0:0:0:3257:9652]" );

			  assertThat( hostnamePort.GetHost( null ), equalTo( "[2001:cdba:0:0:0:0:3257:9652]" ) );
			  assertThat( hostnamePort.Port, equalTo( 0 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[]{ 0, 0 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIPv6AddressWithSchemeAndPort()
		 internal virtual void TestIPv6AddressWithSchemeAndPort()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "foo://[ff02::1:1]:9191" );

			  assertThat( hostnamePort.GetHost( null ), equalTo( "[ff02::1:1]" ) );
			  assertThat( hostnamePort.Port, equalTo( 9191 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[]{ 9191, 9191 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIPv6Localhost()
		 internal virtual void TestIPv6Localhost()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "[::1]" );

			  assertThat( hostnamePort.GetHost( null ), equalTo( "[::1]" ) );
			  assertThat( hostnamePort.Port, equalTo( 0 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[]{ 0, 0 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIPv6LocalhostWithSchemeAndPort()
		 internal virtual void TestIPv6LocalhostWithSchemeAndPort()
		 {
			  HostnamePort hostnamePort = new HostnamePort( "foo://[::1]:6362" );

			  assertThat( hostnamePort.GetHost( null ), equalTo( "[::1]" ) );
			  assertThat( hostnamePort.Port, equalTo( 6362 ) );
			  assertThat( hostnamePort.Ports, equalTo( new int[]{ 6362, 6362 } ) );
		 }
	}

}
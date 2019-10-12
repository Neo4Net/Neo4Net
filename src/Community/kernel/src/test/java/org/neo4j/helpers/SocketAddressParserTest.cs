/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class SocketAddressParserTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateAdvertisedSocketAddressWithLeadingWhitespace()
		 internal virtual void ShouldCreateAdvertisedSocketAddressWithLeadingWhitespace()
		 {
			  // given
			  string addressString = Whitespace( 1 ) + "localhost:9999";

			  // when
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress address = SocketAddressParser.SocketAddress( addressString, SocketAddress::new );

			  // then
			  assertEquals( "localhost", address.Hostname );
			  assertEquals( 9999, address.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateAdvertisedSocketAddressWithTrailingWhitespace()
		 internal virtual void ShouldCreateAdvertisedSocketAddressWithTrailingWhitespace()
		 {
			  // given
			  string addressString = "localhost:9999" + Whitespace( 2 );

			  // when
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress address = SocketAddressParser.SocketAddress( addressString, SocketAddress::new );

			  // then
			  assertEquals( "localhost", address.Hostname );
			  assertEquals( 9999, address.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailToCreateSocketAddressWithMixedInWhitespace()
		 internal virtual void ShouldFailToCreateSocketAddressWithMixedInWhitespace()
		 {
			  string addressString = "localhost" + Whitespace( 1 ) + ":9999";
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertThrows( typeof( System.ArgumentException ), () => SocketAddressParser.SocketAddress(addressString, SocketAddress::new) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailToCreateSocketWithTrailingNonNumbers()
		 internal virtual void ShouldFailToCreateSocketWithTrailingNonNumbers()
		 {
			  string addressString = "localhost:9999abc";
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertThrows( typeof( System.ArgumentException ), () => SocketAddressParser.SocketAddress(addressString, SocketAddress::new) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnMissingPort()
		 internal virtual void ShouldFailOnMissingPort()
		 {
			  string addressString = "localhost:";
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertThrows( typeof( System.ArgumentException ), () => SocketAddressParser.SocketAddress(addressString, SocketAddress::new) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportDomainNameWithPort()
		 internal virtual void ShouldSupportDomainNameWithPort()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "my.domain:123", SocketAddress::new );

			  assertEquals( "my.domain", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "my.domain:123", socketAddress.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportWildcardWithPort()
		 internal virtual void ShouldSupportWildcardWithPort()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "0.0.0.0:123", SocketAddress::new );

			  assertEquals( "0.0.0.0", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "0.0.0.0:123", socketAddress.ToString() );
			  assertTrue( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportPortOnly()
		 internal virtual void ShouldSupportPortOnly()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", ":123", "my.domain", 456, SocketAddress::new );

			  assertEquals( "my.domain", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "my.domain:123", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportDefaultValue()
		 internal virtual void ShouldSupportDefaultValue()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", null, "my.domain", 456, SocketAddress::new );

			  assertEquals( "my.domain", socketAddress.Hostname );
			  assertEquals( 456, socketAddress.Port );
			  assertEquals( "my.domain:456", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportDefaultWildcard()
		 internal virtual void ShouldSupportDefaultWildcard()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", null, "0.0.0.0", 456, SocketAddress::new );

			  assertEquals( "0.0.0.0", socketAddress.Hostname );
			  assertEquals( 456, socketAddress.Port );
			  assertEquals( "0.0.0.0:456", socketAddress.ToString() );
			  assertTrue( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportDefaultIPv6Wildcard()
		 internal virtual void ShouldSupportDefaultIPv6Wildcard()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", null, "::", 456, SocketAddress::new );

			  assertEquals( "::", socketAddress.Hostname );
			  assertEquals( 456, socketAddress.Port );
			  assertEquals( "[::]:456", socketAddress.ToString() );
			  assertTrue( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportDefaultIPv6Value()
		 internal virtual void ShouldSupportDefaultIPv6Value()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", null, "fe80:1:2::4", 456, SocketAddress::new );

			  assertEquals( "fe80:1:2::4", socketAddress.Hostname );
			  assertEquals( 456, socketAddress.Port );
			  assertEquals( "[fe80:1:2::4]:456", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotUseDefaultsWhenSettingValueSupplied()
		 internal virtual void ShouldNotUseDefaultsWhenSettingValueSupplied()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.DeriveSocketAddress( "setting.name", "[fe80:3:4::6]:456", "fe80:1:2::4", 123, SocketAddress::new );

			  assertEquals( "fe80:3:4::6", socketAddress.Hostname );
			  assertEquals( 456, socketAddress.Port );
			  assertEquals( "[fe80:3:4::6]:456", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6Wildcard()
		 internal virtual void ShouldSupportIPv6Wildcard()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "[::]:123", SocketAddress::new );

			  assertEquals( "::", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[::]:123", socketAddress.ToString() );
			  assertTrue( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6Localhost()
		 internal virtual void ShouldSupportIPv6Localhost()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "[::1]:123", SocketAddress::new );

			  assertEquals( "::1", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[::1]:123", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6WithZoneId()
		 internal virtual void ShouldSupportIPv6WithZoneId()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "[fe80::b279:2f%en0]:123", SocketAddress::new );

			  assertEquals( "fe80::b279:2f%en0", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[fe80::b279:2f%en0]:123", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6AddressWithBrackets()
		 internal virtual void ShouldSupportIPv6AddressWithBrackets()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "[fe80:1:2:3:4::5]:123", SocketAddress::new );

			  assertEquals( "fe80:1:2:3:4::5", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[fe80:1:2:3:4::5]:123", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6AddressWithoutBrackets()
		 internal virtual void ShouldSupportIPv6AddressWithoutBrackets()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( "fe80:1:2:3:4::5:123", SocketAddress::new );

			  assertEquals( "fe80:1:2:3:4::5", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[fe80:1:2:3:4::5]:123", socketAddress.ToString() );
			  assertFalse( socketAddress.Wildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIPv6WildcardWithoutBrackets()
		 internal virtual void ShouldSupportIPv6WildcardWithoutBrackets()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  SocketAddress socketAddress = SocketAddressParser.SocketAddress( ":::123", SocketAddress::new );

			  assertEquals( "::", socketAddress.Hostname );
			  assertEquals( 123, socketAddress.Port );
			  assertEquals( "[::]:123", socketAddress.ToString() );
			  assertTrue( socketAddress.Wildcard );
		 }

		 private static string Whitespace( int numberOfWhitespaces )
		 {
			  return StringUtils.repeat( " ", numberOfWhitespaces );
		 }
	}

}
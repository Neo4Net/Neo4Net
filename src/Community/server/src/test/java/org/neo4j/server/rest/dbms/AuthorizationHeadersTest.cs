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
namespace Neo4Net.Server.rest.dbms
{
	using Test = org.junit.Test;

	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.dbms.AuthorizationHeaders.decode;

	public class AuthorizationHeadersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseHappyPath()
		 public virtual void ShouldParseHappyPath()
		 {
			  // Given
			  string username = "jake";
			  string password = "qwerty123456";
			  string header = HTTP.basicAuthHeader( username, password );

			  // When
			  string[] parsed = decode( header );

			  // Then
			  assertEquals( username, parsed[0] );
			  assertEquals( password, parsed[1] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSadPaths()
		 public virtual void ShouldHandleSadPaths()
		 {
			  // When & then
			  assertNull( decode( "" ) );
			  assertNull( decode( "Basic" ) );
			  assertNull( decode( "Basic not valid value" ) );
			  assertNull( decode( "Basic " + Base64.Encoder.encodeToString( "".GetBytes() ) ) );
		 }
	}

}
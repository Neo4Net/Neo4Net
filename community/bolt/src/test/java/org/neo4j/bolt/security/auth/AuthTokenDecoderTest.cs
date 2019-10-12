using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.security.auth
{
	using Test = org.junit.jupiter.api.Test;


	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public abstract class AuthTokenDecoderTest
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void testShouldDecodeAuthToken(java.util.Map<String,Object> authToken, boolean checkDecodingResult) throws Exception;
		 protected internal abstract void TestShouldDecodeAuthToken( IDictionary<string, object> authToken, bool checkDecodingResult );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithStringCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithStringCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS, "password" ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithEmptyStringCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithEmptyStringCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS, "" ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithNullCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithNullCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS, null ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithStringNewCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithStringNewCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS, "password" ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithEmptyStringNewCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithEmptyStringNewCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS, "" ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithNullNewCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithNullNewCredentials()
		 {
			  TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS, null ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithCredentialsOfUnsupportedTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithCredentialsOfUnsupportedTypes()
		 {
			  foreach ( object value in _valuesWithInvalidTypes )
			  {
					TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS, value ), false );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAuthTokenWithNewCredentialsOfUnsupportedType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAuthTokenWithNewCredentialsOfUnsupportedType()
		 {
			  foreach ( object value in _valuesWithInvalidTypes )
			  {
					TestShouldDecodeAuthToken( AuthTokenMapWith( Org.Neo4j.Kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS, value ), false );
			  }
		 }

		 private static IDictionary<string, object> AuthTokenMapWith( string fieldName, object fieldValue )
		 {
			  return map( Org.Neo4j.Kernel.api.security.AuthToken_Fields.PRINCIPAL, "neo4j", fieldName, fieldValue );
		 }

		 private static object[] _valuesWithInvalidTypes = new object[]
		 {
			 new char[]{ 'p', 'a', 's', 's' },
			 Collections.emptyList(),
			 Collections.emptyMap()
		 };
	}

}
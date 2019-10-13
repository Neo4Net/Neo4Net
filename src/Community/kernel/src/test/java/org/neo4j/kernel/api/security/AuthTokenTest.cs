using System.Collections.Generic;

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
namespace Neo4Net.Kernel.api.security
{
	using Test = org.junit.Test;

	using UTF8 = Neo4Net.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class AuthTokenTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeBasicAuthToken()
		 public virtual void ShouldMakeBasicAuthToken()
		 {
			  IDictionary<string, object> token = AuthToken.newBasicAuthToken( "me", "my secret" );
			  assertThat( "Should have correct username", token[AuthToken_Fields.PRINCIPAL], equalTo( "me" ) );
			  assertThat( "Should have correct password", token[AuthToken_Fields.CREDENTIALS], equalTo( UTF8.encode( "my secret" ) ) );
			  assertThat( "Should have correct scheme", token[AuthToken_Fields.SCHEME_KEY], equalTo( AuthToken_Fields.BASIC_SCHEME ) );
			  assertThat( "Should have no realm", token[AuthToken_Fields.REALM_KEY], nullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeBasicAuthTokenWithRealm()
		 public virtual void ShouldMakeBasicAuthTokenWithRealm()
		 {
			  IDictionary<string, object> token = AuthToken.newBasicAuthToken( "me", "my secret", "my realm" );
			  assertThat( "Should have correct username", token[AuthToken_Fields.PRINCIPAL], equalTo( "me" ) );
			  assertThat( "Should have correct password", token[AuthToken_Fields.CREDENTIALS], equalTo( UTF8.encode( "my secret" ) ) );
			  assertThat( "Should have correct scheme", token[AuthToken_Fields.SCHEME_KEY], equalTo( AuthToken_Fields.BASIC_SCHEME ) );
			  assertThat( "Should have correct realm", token[AuthToken_Fields.REALM_KEY], equalTo( "my realm" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeCustomAuthTokenAndBasicScheme()
		 public virtual void ShouldMakeCustomAuthTokenAndBasicScheme()
		 {
			  IDictionary<string, object> token = AuthToken.newCustomAuthToken( "me", "my secret", "my realm", "basic" );
			  assertThat( "Should have correct username", token[AuthToken_Fields.PRINCIPAL], equalTo( "me" ) );
			  assertThat( "Should have correct password", token[AuthToken_Fields.CREDENTIALS], equalTo( UTF8.encode( "my secret" ) ) );
			  assertThat( "Should have correct scheme", token[AuthToken_Fields.SCHEME_KEY], equalTo( AuthToken_Fields.BASIC_SCHEME ) );
			  assertThat( "Should have correctno realm", token[AuthToken_Fields.REALM_KEY], equalTo( "my realm" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeCustomAuthTokenAndCustomcScheme()
		 public virtual void ShouldMakeCustomAuthTokenAndCustomcScheme()
		 {
			  IDictionary<string, object> token = AuthToken.newCustomAuthToken( "me", "my secret", "my realm", "my scheme" );
			  assertThat( "Should have correct username", token[AuthToken_Fields.PRINCIPAL], equalTo( "me" ) );
			  assertThat( "Should have correct password", token[AuthToken_Fields.CREDENTIALS], equalTo( UTF8.encode( "my secret" ) ) );
			  assertThat( "Should have correct scheme", token[AuthToken_Fields.SCHEME_KEY], equalTo( "my scheme" ) );
			  assertThat( "Should have correct realm", token[AuthToken_Fields.REALM_KEY], equalTo( "my realm" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeCustomAuthTokenAndCustomcSchemeWithParameters()
		 public virtual void ShouldMakeCustomAuthTokenAndCustomcSchemeWithParameters()
		 {
			  IDictionary<string, object> token = AuthToken.newCustomAuthToken( "me", "my secret", "my realm", "my scheme", map( "a", "A", "b", "B" ) );
			  assertThat( "Should have correct username", token[AuthToken_Fields.PRINCIPAL], equalTo( "me" ) );
			  assertThat( "Should have correct password", token[AuthToken_Fields.CREDENTIALS], equalTo( UTF8.encode( "my secret" ) ) );
			  assertThat( "Should have correct scheme", token[AuthToken_Fields.SCHEME_KEY], equalTo( "my scheme" ) );
			  assertThat( "Should have correct realm", token[AuthToken_Fields.REALM_KEY], equalTo( "my realm" ) );
			  assertThat( "Should have correct parameters", token[AuthToken_Fields.PARAMETERS], equalTo( map( "a", "A", "b", "B" ) ) );
		 }
	}

}
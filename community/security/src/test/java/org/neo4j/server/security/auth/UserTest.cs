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
namespace Org.Neo4j.Server.Security.Auth
{
	using Test = org.junit.Test;

	using User = Org.Neo4j.Kernel.impl.security.User;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	public class UserTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBuildImmutableUser()
		 public virtual void ShouldBuildImmutableUser()
		 {
			  LegacyCredential abc = LegacyCredential.ForPassword( "123abc" );
			  LegacyCredential fruit = LegacyCredential.ForPassword( "fruit" );
			  User u1 = ( new User.Builder( "Steve", abc ) ).build();
			  User u2 = ( new User.Builder( "Steve", fruit ) ).withRequiredPasswordChange( true ).withFlag( "nice_guy" ).build();
			  assertThat( u1, equalTo( u1 ) );
			  assertThat( u1, not( equalTo( u2 ) ) );

			  User u1AsU2 = u1.Augment().withCredentials(fruit).withRequiredPasswordChange(true).withFlag("nice_guy").build();
			  assertThat( u1, not( equalTo( u1AsU2 ) ) );
			  assertThat( u2, equalTo( u1AsU2 ) );

			  User u2AsU1 = u2.Augment().withCredentials(abc).withRequiredPasswordChange(false).withoutFlag("nice_guy").build();
			  assertThat( u2, not( equalTo( u2AsU1 ) ) );
			  assertThat( u1, equalTo( u2AsU1 ) );

			  assertThat( u1, not( equalTo( u2 ) ) );
		 }
	}

}
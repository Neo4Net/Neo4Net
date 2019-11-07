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
namespace Neo4Net.Kernel.impl.security
{
	using Test = org.junit.Test;

	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.auth.LegacyCredential.INACCESSIBLE;

	public class LegacyCredentialTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMatchesPassword()
		 public virtual void TestMatchesPassword()
		 {
			  LegacyCredential credential = LegacyCredential.forPassword( "foo" );
			  assertTrue( credential.MatchesPassword( "foo" ) );
			  assertFalse( credential.MatchesPassword( "fooo" ) );
			  assertFalse( credential.MatchesPassword( "fo" ) );
			  assertFalse( credential.MatchesPassword( "bar" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEquals()
		 public virtual void TestEquals()
		 {
			  LegacyCredential credential = LegacyCredential.forPassword( "foo" );
			  LegacyCredential sameCredential = new LegacyCredential( credential.Salt(), credential.PasswordHash() );
			  assertEquals( credential, sameCredential );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInaccessibleCredentials()
		 public virtual void TestInaccessibleCredentials()
		 {
			  LegacyCredential credential = new LegacyCredential( INACCESSIBLE.salt(), INACCESSIBLE.passwordHash() );

			  //equals
			  assertEquals( INACCESSIBLE, credential );
			  assertEquals( credential, INACCESSIBLE );
			  assertEquals( INACCESSIBLE, INACCESSIBLE );
			  assertNotEquals( INACCESSIBLE, LegacyCredential.forPassword( "" ) );
			  assertNotEquals( LegacyCredential.forPassword( "" ), INACCESSIBLE );

			  //matchesPassword
			  assertFalse( INACCESSIBLE.matchesPassword( StringHelper.NewString( new sbyte[]{} ) ) );
			  assertFalse( INACCESSIBLE.matchesPassword( "foo" ) );
			  assertFalse( INACCESSIBLE.matchesPassword( "" ) );
		 }
	}

}
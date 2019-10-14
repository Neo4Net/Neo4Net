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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class TokenRegistryTest
	{
		 private const string INBOUND2_TYPE = "inbound2";
		 private const string INBOUND1_TYPE = "inbound1";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addTokenWithDuplicatedNotAllowed()
		 public virtual void AddTokenWithDuplicatedNotAllowed()
		 {
			  TokenRegistry tokenCache = CreateTokenCache();
			  tokenCache.Put( new NamedToken( INBOUND1_TYPE, 1 ) );
			  tokenCache.Put( new NamedToken( INBOUND2_TYPE, 2 ) );

			  ExpectedException.expect( typeof( NonUniqueTokenException ) );
			  ExpectedException.expectMessage( "The testType \"inbound1\" is not unique" );

			  tokenCache.Put( new NamedToken( INBOUND1_TYPE, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keepOriginalTokenWhenAddDuplicate()
		 public virtual void KeepOriginalTokenWhenAddDuplicate()
		 {
			  TokenRegistry tokenCache = CreateTokenCache();
			  tokenCache.Put( new NamedToken( INBOUND1_TYPE, 1 ) );
			  tokenCache.Put( new NamedToken( INBOUND2_TYPE, 2 ) );

			  TryToAddDuplicate( tokenCache );

			  assertEquals( 1, tokenCache.GetId( INBOUND1_TYPE ).Value );
			  assertEquals( 2, tokenCache.GetId( INBOUND2_TYPE ).Value );
			  assertNull( tokenCache.GetToken( 3 ) );
		 }

		 private TokenRegistry CreateTokenCache()
		 {
			  return new TokenRegistry( "testType" );
		 }

		 private void TryToAddDuplicate( TokenRegistry tokenCache )
		 {
			  try
			  {
					tokenCache.Put( new NamedToken( INBOUND1_TYPE, 3 ) );
			  }
			  catch ( NonUniqueTokenException )
			  {
			  }
		 }

	}

}
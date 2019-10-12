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
namespace Org.Neo4j.@internal.Kernel.Api
{
	using Test = org.junit.Test;

	using Org.Neo4j.Function;
	using Org.Neo4j.Function;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class TokenTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelGetOrCreateForName()
		 public virtual void LabelGetOrCreateForName()
		 {
			  AssertIllegalToken( token => token.labelGetOrCreateForName( null ) );
			  AssertIllegalToken( token => token.labelGetOrCreateForName( "" ) );
			  int id = MapToken( token => token.labelGetOrCreateForName( "label" ) );
			  assertEquals( id, MapToken( token => token.nodeLabel( "label" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelGetOrCreateForNames()
		 public virtual void LabelGetOrCreateForNames()
		 {
			  AssertIllegalToken( token => token.labelGetOrCreateForNames( new string[]{ null }, new int[1] ) );
			  AssertIllegalToken( token => token.labelGetOrCreateForNames( new string[]{ "" }, new int[1] ) );
			  string[] names = new string[] { "a", "b" };
			  int[] ids = new int[2];
			  ForToken( token => token.labelGetOrCreateForNames( names, ids ) );
			  assertEquals( ids[0], MapToken( token => token.nodeLabel( "a" ) ) );
			  assertEquals( ids[1], MapToken( token => token.nodeLabel( "b" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertyKeyGetOrCreateForName()
		 public virtual void PropertyKeyGetOrCreateForName()
		 {
			  AssertIllegalToken( token => token.propertyKeyGetOrCreateForName( null ) );
			  AssertIllegalToken( token => token.propertyKeyGetOrCreateForName( "" ) );
			  int id = MapToken( token => token.propertyKeyGetOrCreateForName( "prop" ) );
			  assertEquals( id, MapToken( token => token.propertyKey( "prop" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertyKeyGetOrCreateForNames()
		 public virtual void PropertyKeyGetOrCreateForNames()
		 {
			  AssertIllegalToken( token => token.propertyKeyGetOrCreateForNames( new string[]{ null }, new int[1] ) );
			  AssertIllegalToken( token => token.propertyKeyGetOrCreateForNames( new string[]{ "" }, new int[1] ) );
			  string[] names = new string[] { "a", "b" };
			  int[] ids = new int[2];
			  ForToken( token => token.propertyKeyGetOrCreateForNames( names, ids ) );
			  assertEquals( ids[0], MapToken( token => token.propertyKey( "a" ) ) );
			  assertEquals( ids[1], MapToken( token => token.propertyKey( "b" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipTypeGetOrCreateForName()
		 public virtual void RelationshipTypeGetOrCreateForName()
		 {
			  AssertIllegalToken( token => token.relationshipTypeGetOrCreateForName( null ) );
			  AssertIllegalToken( token => token.relationshipTypeGetOrCreateForName( "" ) );
			  int id = MapToken( token => token.relationshipTypeGetOrCreateForName( "rel" ) );
			  assertEquals( id, MapToken( token => token.relationshipType( "rel" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipTypeGetOrCreateForNames()
		 public virtual void RelationshipTypeGetOrCreateForNames()
		 {
			  AssertIllegalToken( token => token.relationshipTypeGetOrCreateForNames( new string[]{ null }, new int[1] ) );
			  AssertIllegalToken( token => token.relationshipTypeGetOrCreateForNames( new string[]{ "" }, new int[1] ) );
			  string[] names = new string[] { "a", "b" };
			  int[] ids = new int[2];
			  ForToken( token => token.relationshipTypeGetOrCreateForNames( names, ids ) );
			  assertEquals( ids[0], MapToken( token => token.relationshipType( "a" ) ) );
			  assertEquals( ids[1], MapToken( token => token.relationshipType( "b" ) ) );
		 }

		 private void AssertIllegalToken( ThrowingConsumer<Token, KernelException> f )
		 {
			  try
			  {
					  using ( Transaction tx = beginTransaction() )
					  {
						f.Accept( tx.Token() );
						fail( "Expected IllegalTokenNameException" );
					  }
			  }
			  catch ( IllegalTokenNameException )
			  {
					// wanted
			  }
			  catch ( KernelException e )
			  {
					fail( "Unwanted exception: " + e.Message );
			  }
		 }

		 private int MapToken( ThrowingFunction<Token, int, KernelException> f )
		 {
			  try
			  {
					  using ( Transaction tx = beginTransaction() )
					  {
						return f.Apply( tx.Token() );
					  }
			  }
			  catch ( KernelException e )
			  {
					fail( "Unwanted exception: " + e.Message );
					return -1; // unreachable
			  }
		 }

		 private void ForToken( ThrowingConsumer<Token, KernelException> f )
		 {
			  try
			  {
					  using ( Transaction tx = beginTransaction() )
					  {
						f.Accept( tx.Token() );
					  }
			  }
			  catch ( KernelException e )
			  {
					fail( "Unwanted exception: " + e.Message );
			  }
		 }
	}

}
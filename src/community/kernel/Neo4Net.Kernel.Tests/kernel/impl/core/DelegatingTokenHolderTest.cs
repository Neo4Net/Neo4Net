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
namespace Neo4Net.Kernel.impl.core
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DelegatingTokenHolderTest
	{
		 private TokenCreator _creator;
		 private TokenHolder _holder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _creator = mock( typeof( TokenCreator ) );
			  _holder = new DelegatingTokenHolder( _creator, "Dummy" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCreateAndCacheNewTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustCreateAndCacheNewTokens()
		 {
			  when( _creator.createToken( "token" ) ).thenReturn( 42 );
			  assertThat( _holder.getOrCreateId( "token" ), @is( 42 ) );
			  assertThat( _holder.getOrCreateId( "token" ), @is( 42 ) );
			  // Verify implies that the call only happens once.
			  verify( _creator ).createToken( "token" );
			  verifyNoMoreInteractions( _creator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void batchTokenGetMustReturnWhetherThereWereUnresolvedTokens()
		 public virtual void BatchTokenGetMustReturnWhetherThereWereUnresolvedTokens()
		 {
			  _holder.InitialTokens = new IList<NamedToken> { Token( "a", 1 ), Token( "b", 2 ) };
			  string[] names;
			  int[] ids;

			  names = new string[]{ "a", "X", "b" };
			  ids = new int[]{ -1, -1, -1 };
			  assertTrue( _holder.getIdsByNames( names, ids ) );
			  assertThat( ids[0], @is( 1 ) );
			  assertThat( ids[1], @is( -1 ) );
			  assertThat( ids[2], @is( 2 ) );

			  names = new string[]{ "a", "b" };
			  ids = new int[]{ -1, -1 };
			  assertFalse( _holder.getIdsByNames( names, ids ) );
			  assertThat( ids[0], @is( 1 ) );
			  assertThat( ids[1], @is( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void batchTokenCreateMustIgnoreExistingTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BatchTokenCreateMustIgnoreExistingTokens()
		 {
			  InitialTokensABC();

			  AtomicInteger nextId = new AtomicInteger( 42 );
			  MockAssignNewTokenIdsInBatch( nextId );

			  string[] names = new string[]{ "b", "X", "a", "Y", "c" };
			  int[] ids = new int[names.Length];
			  _holder.getOrCreateIds( names, ids );
			  assertThat( ids.Length, @is( 5 ) );
			  assertThat( ids[0], @is( 2 ) );
			  assertThat( ids[1], isOneOf( 42, 43 ) );
			  assertThat( ids[2], @is( 1 ) );
			  assertThat( ids[3], isOneOf( 42, 43 ) );
			  assertThat( ids[4], @is( 3 ) );
			  assertThat( nextId.get(), @is(44) );

			  // And these should not throw.
			  _holder.getTokenById( 42 );
			  _holder.getTokenById( 43 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void mockAssignNewTokenIdsInBatch(java.util.concurrent.atomic.AtomicInteger nextId) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private void MockAssignNewTokenIdsInBatch( AtomicInteger nextId )
		 {
			  doAnswer(inv =>
			  {
				int[] ids = inv.getArgument( 1 );
				IntPredicate filter = inv.getArgument( 2 );
				for ( int i = 0; i < ids.Length; i++ )
				{
					 if ( filter.test( i ) )
					 {
						  ids[i] = nextId.AndIncrement;
					 }
				}
				return null;
			  }).when( _creator ).createTokens( any( typeof( string[] ) ), any( typeof( int[] ) ), any( typeof( IntPredicate ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initialTokensABC() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private void InitialTokensABC()
		 {
			  _holder.InitialTokens = new IList<NamedToken> { Token( "a", 1 ), Token( "b", 2 ) };

			  when( _creator.createToken( "c" ) ).thenReturn( 3 );
			  assertThat( _holder.getOrCreateId( "c" ), @is( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void batchTokenCreateMustDeduplicateTokenCreates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BatchTokenCreateMustDeduplicateTokenCreates()
		 {
			  InitialTokensABC();

			  AtomicInteger nextId = new AtomicInteger( 42 );
			  MockAssignNewTokenIdsInBatch( nextId );

			  // NOTE: the existing 'b', and the missing 'X', tokens are in here twice:
			  string[] names = new string[]{ "b", "b", "X", "a", "X", "c" };
			  int[] ids = new int[names.Length];
			  _holder.getOrCreateIds( names, ids );

			  assertThat( ids.Length, @is( 6 ) );
			  assertThat( ids[0], @is( 2 ) );
			  assertThat( ids[1], @is( 2 ) );
			  assertThat( ids[2], @is( 42 ) );
			  assertThat( ids[3], @is( 1 ) );
			  assertThat( ids[4], @is( 42 ) );
			  assertThat( ids[5], @is( 3 ) );
			  assertThat( nextId.get(), @is(43) );

			  // And this should not throw.
			  _holder.getTokenById( 42 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void batchTokenCreateMustThrowOnArraysOfDifferentLengths()
		 public virtual void BatchTokenCreateMustThrowOnArraysOfDifferentLengths()
		 {
			  _holder.getOrCreateIds( new string[3], new int[2] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTokensAsPartOfInitialTokenLoading()
		 public virtual void ShouldClearTokensAsPartOfInitialTokenLoading()
		 {
			  // GIVEN
			  _holder.InitialTokens = new IList<NamedToken> { Token( "one", 1 ), Token( "two", 2 ) };
			  AssertTokens( _holder.AllTokens, Token( "one", 1 ), Token( "two", 2 ) );

			  // WHEN
			  _holder.InitialTokens = new IList<NamedToken> { Token( "two", 2 ), Token( "three", 3 ), Token( "four", 4 ) };

			  // THEN
			  AssertTokens( _holder.AllTokens, Token( "two", 2 ), Token( "three", 3 ), Token( "four", 4 ) );
		 }

		 private void AssertTokens( IEnumerable<NamedToken> allTokens, params NamedToken[] expectedTokens )
		 {
			  IDictionary<string, NamedToken> existing = new Dictionary<string, NamedToken>();
			  foreach ( NamedToken token in allTokens )
			  {
					existing[token.Name()] = token;
			  }
			  IDictionary<string, NamedToken> expected = new Dictionary<string, NamedToken>();
			  foreach ( NamedToken token in expectedTokens )
			  {
					expected[token.Name()] = token;
			  }
			  assertEquals( expected, existing );
		 }

		 private NamedToken Token( string name, int id )
		 {
			  return new NamedToken( name, id );
		 }
	}

}
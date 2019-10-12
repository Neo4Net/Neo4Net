/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TermsTest
	{
		 private Terms _terms;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectInitialValues()
		 public virtual void ShouldHaveCorrectInitialValues()
		 {
			  // given
			  long prevIndex = 5;
			  long prevTerm = 10;
			  _terms = new Terms( prevIndex, prevTerm );

			  // then
			  assertTermInRange( -1, prevIndex, index => -1L );
			  assertEquals( prevTerm, _terms.get( prevIndex ) );
			  assertTermInRange( prevIndex + 1, prevIndex + 10, index => -1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAppendedTerms()
		 public virtual void ShouldReturnAppendedTerms()
		 {
			  // given
			  _terms = new Terms( -1, -1 );
			  int count = 10;

			  // when
			  appendRange( 0, count, index => index * 2L );

			  // then
			  assertTermInRange( 0, count, index => index * 2L );
			  assertEquals( -1, _terms.get( -1 ) );
			  assertEquals( -1, _terms.get( count ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAppendedTermsLongerRanges()
		 public virtual void ShouldReturnAppendedTermsLongerRanges()
		 {
			  _terms = new Terms( -1, -1 );
			  int count = 10;

			  // when
			  for ( long term = 0; term < count; term++ )
			  {
					AppendRange( term * count, ( term + 1 ) * count, term );
			  }

			  // then
			  for ( long term = 0; term < count; term++ )
			  {
					AssertTermInRange( term * count, ( term + 1 ) * count, term );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAcceptInOrderIndexes()
		 public virtual void ShouldOnlyAcceptInOrderIndexes()
		 {
			  // given
			  long prevIndex = 3;
			  long term = 3;
			  _terms = new Terms( prevIndex, term );

			  try
			  {
					// when
					_terms.append( prevIndex, term );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then: expected
			  }

			  _terms.append( prevIndex + 1, term ); // should work fine
			  _terms.append( prevIndex + 2, term ); // should work fine
			  _terms.append( prevIndex + 3, term ); // should work fine

			  try
			  {
					// when
					_terms.append( prevIndex + 5, term );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then: expected
			  }

			  _terms.append( prevIndex + 4, term ); // should work fine
			  _terms.append( prevIndex + 5, term ); // should work fine
			  _terms.append( prevIndex + 6, term ); // should work fine
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAcceptMonotonicTerms()
		 public virtual void ShouldOnlyAcceptMonotonicTerms()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  _terms.append( prevIndex + 1, term );
			  _terms.append( prevIndex + 2, term );
			  _terms.append( prevIndex + 3, term + 1 );
			  _terms.append( prevIndex + 4, term + 1 );
			  _terms.append( prevIndex + 5, term + 2 );
			  _terms.append( prevIndex + 6, term + 2 );

			  // when
			  try
			  {
					_terms.append( prevIndex + 7, term + 1 );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then: expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTruncateNegativeIndexes()
		 public virtual void ShouldNotTruncateNegativeIndexes()
		 {
			  // given
			  _terms = new Terms( -1, -1 );
			  _terms.append( 0, 0 );

			  // when
			  try
			  {
					_terms.truncate( -1 );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then: expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTruncateLessThanLowestIndex()
		 public virtual void ShouldNotTruncateLessThanLowestIndex()
		 {
			  // given
			  _terms = new Terms( 5, 1 );

			  // when
			  try
			  {
					_terms.truncate( 4 );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then: expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateInCurrentRange()
		 public virtual void ShouldTruncateInCurrentRange()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, 20, term );
			  assertEquals( term, _terms.get( 19 ) );

			  // when
			  long truncateFromIndex = 15;
			  _terms.truncate( truncateFromIndex );

			  // then
			  assertTermInRange( prevIndex + 1, truncateFromIndex, index => term );
			  assertTermInRange( truncateFromIndex, 30, index => -1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateAtExactBoundary()
		 public virtual void ShouldTruncateAtExactBoundary()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term );
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 ); // to be truncated

			  // when
			  long truncateFromIndex = prevIndex + 10;
			  _terms.truncate( truncateFromIndex );

			  // then
			  AssertTermInRange( prevIndex + 1, prevIndex + 10, term );
			  assertTermInRange( prevIndex + 10, truncateFromIndex, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateCompleteCurrentRange()
		 public virtual void ShouldTruncateCompleteCurrentRange()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term );
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 ); // to be half-truncated
			  AppendRange( prevIndex + 20, prevIndex + 30, term + 2 ); // to be truncated

			  // when
			  long truncateFromIndex = prevIndex + 15;
			  _terms.truncate( truncateFromIndex );

			  // then
			  AssertTermInRange( prevIndex + 1, prevIndex + 10, term );
			  AssertTermInRange( prevIndex + 10, truncateFromIndex, term + 1 );
			  assertTermInRange( truncateFromIndex, prevIndex + 30, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateSeveralCompleteRanges()
		 public virtual void ShouldTruncateSeveralCompleteRanges()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // to be half-truncated
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 ); // to be truncated
			  AppendRange( prevIndex + 20, prevIndex + 30, term + 2 ); // to be truncated

			  // when
			  long truncateFromIndex = prevIndex + 5;
			  _terms.truncate( truncateFromIndex );

			  // then
			  AssertTermInRange( prevIndex + 1, truncateFromIndex, term );
			  assertTermInRange( truncateFromIndex, prevIndex + 30, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAfterTruncate()
		 public virtual void ShouldAppendAfterTruncate()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // to be half-truncated
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 10 ); // to be truncated

			  // when
			  long truncateFromIndex = prevIndex + 5;
			  _terms.truncate( truncateFromIndex );
			  AppendRange( truncateFromIndex, truncateFromIndex + 20, term + 20 );

			  // then
			  AssertTermInRange( prevIndex + 1, truncateFromIndex, term );
			  AssertTermInRange( truncateFromIndex, truncateFromIndex + 20, term + 20 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAfterSkip()
		 public virtual void ShouldAppendAfterSkip()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term );
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  // when
			  long skipIndex = 30;
			  long skipTerm = term + 2;
			  _terms.skip( skipIndex, skipTerm );

			  // then
			  assertTermInRange( prevIndex, skipIndex, -1 );
			  assertEquals( skipTerm, _terms.get( skipIndex ) );

			  // when
			  AppendRange( skipIndex + 1, skipIndex + 20, skipTerm );

			  // then
			  AssertTermInRange( skipIndex + 1, skipIndex + 20, skipTerm );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPruneAnythingIfBeforeMin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPruneAnythingIfBeforeMin()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term );
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );

			  // when
			  _terms.prune( prevIndex );

			  // then
			  assertTermInRange( prevIndex - 10, prevIndex, -1 );
			  AssertTermInRange( prevIndex, prevIndex + 10, term );
			  AssertTermInRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneInMiddleOfFirstRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneInMiddleOfFirstRange()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // half-pruned
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );

			  // when
			  long pruneIndex = prevIndex + 5;
			  _terms.prune( pruneIndex );

			  // then
			  assertTermInRange( prevIndex - 10, pruneIndex, -1 );
			  AssertTermInRange( pruneIndex, prevIndex + 10, term );
			  AssertTermInRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneAtBoundaryOfRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneAtBoundaryOfRange()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // completely pruned
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );

			  // when
			  long pruneIndex = prevIndex + 10;
			  _terms.prune( pruneIndex );

			  // then
			  assertTermInRange( prevIndex - 10, pruneIndex, -1 );
			  AssertTermInRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 1, IndexesSize );
			  assertEquals( 1, TermsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneJustBeyondBoundaryOfRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneJustBeyondBoundaryOfRange()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // completely pruned
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 );

			  assertEquals( 2, IndexesSize );
			  assertEquals( 2, TermsSize );

			  // when
			  long pruneIndex = prevIndex + 11;
			  _terms.prune( pruneIndex );

			  // then
			  assertTermInRange( prevIndex - 10, pruneIndex, -1 );
			  AssertTermInRange( prevIndex + 11, prevIndex + 20, term + 1 );

			  assertEquals( 1, IndexesSize );
			  assertEquals( 1, TermsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneSeveralCompleteRanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneSeveralCompleteRanges()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  AppendRange( prevIndex + 1, prevIndex + 10, term ); // completely pruned
			  AppendRange( prevIndex + 10, prevIndex + 20, term + 1 ); // completely pruned
			  AppendRange( prevIndex + 20, prevIndex + 30, term + 2 ); // half-pruned
			  AppendRange( prevIndex + 30, prevIndex + 40, term + 3 );
			  AppendRange( prevIndex + 40, prevIndex + 50, term + 4 );

			  assertEquals( 5, IndexesSize );
			  assertEquals( 5, TermsSize );

			  // when
			  long pruneIndex = prevIndex + 25;
			  _terms.prune( pruneIndex );

			  // then
			  assertTermInRange( prevIndex - 10, pruneIndex, -1 );
			  AssertTermInRange( pruneIndex, prevIndex + 30, term + 2 );
			  AssertTermInRange( prevIndex + 30, prevIndex + 40, term + 3 );
			  AssertTermInRange( prevIndex + 40, prevIndex + 50, term + 4 );

			  assertEquals( 3, IndexesSize );
			  assertEquals( 3, TermsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendNewItemsIfThereAreNoEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendNewItemsIfThereAreNoEntries()
		 {
			  // given
			  long term = 5;
			  long prevIndex = 10;
			  _terms = new Terms( prevIndex, term );

			  // when
			  _terms.truncate( prevIndex );

			  // then
			  assertEquals( -1, _terms.get( prevIndex ) );
			  assertEquals( -1, _terms.latest() );
			  assertEquals( 0, IndexesSize );
			  assertEquals( 0, TermsSize );

			  // and when
			  _terms.append( prevIndex, 5 );

			  // then
			  assertEquals( term, _terms.get( prevIndex ) );
			  assertEquals( term, _terms.latest() );
			  assertEquals( 1, IndexesSize );
			  assertEquals( 1, TermsSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getTermsSize() throws NoSuchFieldException, IllegalAccessException
		 private int TermsSize
		 {
			 get
			 {
				  return GetField( "terms" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getIndexesSize() throws NoSuchFieldException, IllegalAccessException
		 private int IndexesSize
		 {
			 get
			 {
				  return GetField( "indexes" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getField(String name) throws NoSuchFieldException, IllegalAccessException
		 private int GetField( string name )
		 {
			  System.Reflection.FieldInfo field = typeof( Terms ).getDeclaredField( name );
			  field.Accessible = true;
			  long[] longs = ( long[] ) field.get( _terms );
			  return longs.Length;
		 }

		 private void AssertTermInRange( long from, long to, long expectedTerm )
		 {
			  assertTermInRange( from, to, index => expectedTerm );
		 }

		 private void AssertTermInRange( long from, long to, System.Func<long, long> expectedTermFunction )
		 {
			  for ( long index = from; index < to; index++ )
			  {
					assertEquals( "For index: " + index, ( long ) expectedTermFunction( index ), _terms.get( index ) );
			  }
		 }

		 private void AppendRange( long from, long to, long term )
		 {
			  appendRange( from, to, index => term );
		 }

		 private void AppendRange( long from, long to, System.Func<long, long> termFunction )
		 {
			  for ( long index = from; index < to; index++ )
			  {
					_terms.append( index, termFunction( index ) );
			  }
		 }
	}

}
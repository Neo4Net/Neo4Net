using System;
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
namespace Neo4Net.Index.Internal.gbptree
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using Assertions = org.junit.jupiter.api.Assertions;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using DelegatingPageCursor = Neo4Net.Io.pagecache.impl.DelegatingPageCursor;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GBPTree.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.SeekCursor.DEFAULT_MAX_READ_AHEAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.LEAF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.ValueMergers.overwrite;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) abstract class SeekCursorTestBase<KEY, VALUE>
	internal abstract class SeekCursorTestBase<KEY, VALUE>
	{
		 private const int PAGE_SIZE = 256;
		 private static long _stableGeneration = GenerationSafePointer.MIN_GENERATION;
		 private static long _unstableGeneration = _stableGeneration + 1;
		 private static readonly System.Func<long> _generationSupplier = () => Generation.GenerationConflict(_stableGeneration, _unstableGeneration);
		 private static readonly RootCatchup _failingRootCatchup = _id =>
		 {
		  throw new AssertionError( "Should not happen" );
		 };
		 private static readonly System.Action<Exception> _exceptionDecorator = t =>
		 {
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

		 private TestLayout<KEY, VALUE> _layout;
		 private TreeNode<KEY, VALUE> _node;
		 private InternalTreeLogic<KEY, VALUE> _treeLogic;
		 private StructurePropagation<KEY> _structurePropagation;

		 private PageAwareByteArrayCursor _cursor;
		 private PageAwareByteArrayCursor _utilCursor;
		 private SimpleIdProvider _id;

		 private long _rootId;
		 private long _rootGeneration;
		 private int _numberOfRootSplits;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _cursor = new PageAwareByteArrayCursor( PAGE_SIZE );
			  _utilCursor = _cursor.duplicate();
			  _id = new SimpleIdProvider( _cursor.duplicate );

			  _layout = Layout;
			  _node = GetTreeNode( PAGE_SIZE, _layout );
			  _treeLogic = new InternalTreeLogic<KEY, VALUE>( _id, _node, _layout, NO_MONITOR );
			  _structurePropagation = new StructurePropagation<KEY>( _layout.newKey(), _layout.newKey(), _layout.newKey() );

			  long firstPage = _id.acquireNewId( _stableGeneration, _unstableGeneration );
			  GoTo( _cursor, firstPage );
			  GoTo( _utilCursor, firstPage );

			  _node.initializeLeaf( _cursor, _stableGeneration, _unstableGeneration );
			  UpdateRoot();
		 }

		 internal abstract TestLayout<KEY, VALUE> Layout { get; }

		 internal abstract TreeNode<KEY, VALUE> GetTreeNode( int pageSize, TestLayout<KEY, VALUE> layout );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void goTo(org.Neo4Net.io.pagecache.PageCursor cursor, long pageId) throws java.io.IOException
		 private static void GoTo( PageCursor cursor, long pageId )
		 {
			  PageCursorUtil.GoTo( cursor, "test", pointer( pageId ) );
		 }

		 private void UpdateRoot()
		 {
			  _rootId = _cursor.CurrentPageId;
			  _rootGeneration = _unstableGeneration;
			  _treeLogic.initialize( _cursor );
		 }

		 /* NO CONCURRENT INSERT */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInBeginningOfSingleLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInBeginningOfSingleLeaf()
		 {
			  // GIVEN
			  long lastSeed = FullLeaf();
			  long fromInclusive = 0;
			  long toExclusive = lastSeed / 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInBeginningOfSingleLeafBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInBeginningOfSingleLeafBackwards()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = maxKeyCount / 2;
			  long toExclusive = -1;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInEndOfSingleLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInEndOfSingleLeaf()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = maxKeyCount / 2;
			  long toExclusive = maxKeyCount;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInEndOfSingleLeafBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInEndOfSingleLeafBackwards()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = maxKeyCount - 1;
			  long toExclusive = maxKeyCount / 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInMiddleOfSingleLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInMiddleOfSingleLeaf()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long middle = maxKeyCount / 2;
			  long fromInclusive = middle / 2;
			  long toExclusive = ( middle + maxKeyCount ) / 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesWithinRangeInMiddleOfSingleLeafBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesWithinRangeInMiddleOfSingleLeafBackwards()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long middle = maxKeyCount / 2;
			  long fromInclusive = ( middle + maxKeyCount ) / 2;
			  long toExclusive = middle / 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesSpanningTwoLeaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesSpanningTwoLeaves()
		 {
			  // GIVEN
			  long i = FullLeaf();
			  long left = CreateRightSibling( _cursor );
			  i = FullLeaf( i );
			  _cursor.next( left );

			  long fromInclusive = 0;
			  long toExclusive = i;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesSpanningTwoLeavesBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesSpanningTwoLeavesBackwards()
		 {
			  // GIVEN
			  long i = FullLeaf();
			  CreateRightSibling( _cursor );
			  i = FullLeaf( i );

			  long fromInclusive = i - 1;
			  long toExclusive = -1;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesOnSecondLeafWhenStartingFromFirstLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesOnSecondLeafWhenStartingFromFirstLeaf()
		 {
			  // GIVEN
			  long i = FullLeaf();
			  long left = CreateRightSibling( _cursor );
			  long j = FullLeaf( i );
			  _cursor.next( left );

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( i, j ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( i, j, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindEntriesOnSecondLeafWhenStartingFromFirstLeafBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindEntriesOnSecondLeafWhenStartingFromFirstLeafBackwards()
		 {
			  // GIVEN
			  long leftKeyCount = FullLeaf();
			  long left = CreateRightSibling( _cursor );
			  FullLeaf( leftKeyCount );
			  _cursor.next( left );

			  long fromInclusive = leftKeyCount - 1;
			  long toExclusive = -1;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotContinueToSecondLeafAfterFindingEndOfRangeInFirst() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotContinueToSecondLeafAfterFindingEndOfRangeInFirst()
		 {
			  AtomicBoolean nextCalled = new AtomicBoolean();
			  PageCursor pageCursorSpy = new DelegatingPageCursorAnonymousInnerClass( this, _cursor, nextCalled );

			  // GIVEN
			  long i = FullLeaf();
			  long left = CreateRightSibling( _cursor );
			  long j = FullLeaf( i );

			  long fromInclusive = j - 1;
			  long toExclusive = i;

			  // Reset
			  nextCalled.set( false );

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive, pageCursorSpy ) )
			  {
					// THEN
					AssertRangeInSingleLeaf( fromInclusive, toExclusive, cursor );
			  }
			  assertFalse( nextCalled.get(), "Cursor continued to next leaf even though end of range is within first leaf" );
		 }

		 private class DelegatingPageCursorAnonymousInnerClass : DelegatingPageCursor
		 {
			 private readonly SeekCursorTestBase<KEY, VALUE> _outerInstance;

			 private AtomicBoolean _nextCalled;

			 public DelegatingPageCursorAnonymousInnerClass( SeekCursorTestBase<KEY, VALUE> outerInstance, Neo4Net.Index.Internal.gbptree.PageAwareByteArrayCursor cursor, AtomicBoolean nextCalled ) : base( cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._nextCalled = nextCalled;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
			 public override bool next( long pageId )
			 {
				  _nextCalled.set( true );
				  return base.next( pageId );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptyRange()
		 internal virtual void ShouldHandleEmptyRange()
		 {
			  Assertions.assertTimeoutPreemptively(Duration.ofSeconds(5), () =>
			  {
			  Insert( 0 );
			  Insert( 2 );
			  long fromInclusive = 1;
			  long toExclusive = 2;
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
				  assertFalse( cursor.Next() );
			  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptyRangeBackwards()
		 internal virtual void ShouldHandleEmptyRangeBackwards()
		 {
			  Assertions.assertTimeoutPreemptively(Duration.ofSeconds(5), () =>
			  {
			  Insert( 0 );
			  Insert( 2 );
			  long fromInclusive = 1;
			  long toExclusive = 0;
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
				  assertFalse( cursor.Next() );
			  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleBackwardsWithNoExactHitOnFromInclusive()
		 internal virtual void ShouldHandleBackwardsWithNoExactHitOnFromInclusive()
		 {
			  Assertions.assertTimeoutPreemptively(Duration.ofSeconds(5), () =>
			  {
			  Insert( 0 );
			  Insert( 2 );
			  long fromInclusive = 3;
			  long toExclusive = 0;
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
				  assertTrue( cursor.Next() );
				  assertFalse( cursor.Next() );
			  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleBackwardsWithExactHitOnFromInclusive()
		 internal virtual void ShouldHandleBackwardsWithExactHitOnFromInclusive()
		 {
			  Assertions.assertTimeoutPreemptively(Duration.ofSeconds(5), () =>
			  {
			  Insert( 0 );
			  Insert( 2 );
			  long fromInclusive = 2;
			  long toExclusive = 0;
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
				  assertTrue( cursor.Next() );
				  assertFalse( cursor.Next() );
			  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeysWhenGivenRangeStartingOutsideStartOfData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeysWhenGivenRangeStartingOutsideStartOfData()
		 {
			  // Given
			  // [ 0 1... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();

			  long expectedKey = 0;
			  using ( SeekCursor<KEY, VALUE> seekCursor = seekCursor( -1, maxKeyCount - 1 ) )
			  {
					while ( seekCursor.Next() )
					{
						 AssertKeyAndValue( seekCursor, expectedKey );
						 expectedKey++;
					}
			  }
			  assertEquals( expectedKey, maxKeyCount - 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeysWhenGivenRangeStartingOutsideStartOfDataBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeysWhenGivenRangeStartingOutsideStartOfDataBackwards()
		 {
			  // Given
			  // [ 0 1... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();

			  long expectedKey = maxKeyCount - 1;
			  using ( SeekCursor<KEY, VALUE> seekCursor = seekCursor( maxKeyCount, 0 ) )
			  {
					while ( seekCursor.Next() )
					{
						 AssertKeyAndValue( seekCursor, expectedKey );
						 expectedKey--;
					}
			  }
			  assertEquals( expectedKey, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeysWhenGivenRangeEndingOutsideEndOfData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeysWhenGivenRangeEndingOutsideEndOfData()
		 {
			  // Given
			  // [ 0 1... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();

			  long expectedKey = 0;
			  using ( SeekCursor<KEY, VALUE> seekCursor = seekCursor( 0, maxKeyCount + 1 ) )
			  {
					while ( seekCursor.Next() )
					{
						 AssertKeyAndValue( seekCursor, expectedKey );
						 expectedKey++;
					}
			  }
			  assertEquals( expectedKey, maxKeyCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeysWhenGivenRangeEndingOutsideEndOfDataBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeysWhenGivenRangeEndingOutsideEndOfDataBackwards()
		 {
			  // Given
			  // [ 0 1... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();

			  long expectedKey = maxKeyCount - 1;
			  using ( SeekCursor<KEY, VALUE> seekCursor = seekCursor( maxKeyCount - 1, -2 ) )
			  {
					while ( seekCursor.Next() )
					{
						 AssertKeyAndValue( seekCursor, expectedKey );
						 expectedKey--;
					}
			  }
			  assertEquals( expectedKey, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustStartReadingFromCorrectLeafWhenRangeStartWithKeyEqualToPrimKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustStartReadingFromCorrectLeafWhenRangeStartWithKeyEqualToPrimKey()
		 {
			  // given
			  long lastSeed = RootWithTwoLeaves();
			  KEY primKey = _layout.newKey();
			  _node.keyAt( _cursor, primKey, 0, INTERNAL );
			  long expectedNext = GetSeed( primKey );
			  long rightChild = GenerationSafePointerPair.Pointer( _node.childAt( _cursor, 1, _stableGeneration, _unstableGeneration ) );

			  // when
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( expectedNext, lastSeed ) )
			  {
					assertEquals( rightChild, _cursor.CurrentPageId );
					while ( seek.Next() )
					{
						 AssertKeyAndValue( seek, expectedNext );
						 expectedNext++;
					}
			  }

			  // then
			  assertEquals( lastSeed, expectedNext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustStartReadingFromCorrectLeafWhenRangeStartWithKeyEqualToPrimKeyBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustStartReadingFromCorrectLeafWhenRangeStartWithKeyEqualToPrimKeyBackwards()
		 {
			  // given
			  RootWithTwoLeaves();
			  KEY primKey = _layout.newKey();
			  _node.keyAt( _cursor, primKey, 0, INTERNAL );
			  long expectedNext = GetSeed( primKey );
			  long rightChild = GenerationSafePointerPair.Pointer( _node.childAt( _cursor, 1, _stableGeneration, _unstableGeneration ) );

			  // when
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( expectedNext, -1 ) )
			  {
					assertEquals( rightChild, _cursor.CurrentPageId );
					while ( seek.Next() )
					{
						 AssertKeyAndValue( seek, expectedNext );
						 expectedNext--;
					}
			  }

			  // then
			  assertEquals( -1, expectedNext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void exactMatchInStableRoot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ExactMatchInStableRoot()
		 {
			  // given
			  long maxKeyCount = FullLeaf();

			  // when
			  for ( long i = 0; i < maxKeyCount; i++ )
			  {
					AssertExactMatch( i );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void exactMatchInLeaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ExactMatchInLeaves()
		 {
			  // given
			  long lastSeed = RootWithTwoLeaves();

			  // when
			  for ( long i = 0; i < lastSeed; i++ )
			  {
					AssertExactMatch( i );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long rootWithTwoLeaves() throws java.io.IOException
		 private long RootWithTwoLeaves()
		 {
			  long i = 0;
			  for ( ; _numberOfRootSplits < 1; i++ )
			  {
					Insert( i );
			  }
			  return i;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertExactMatch(long i) throws java.io.IOException
		 private void AssertExactMatch( long i )
		 {
			  using ( SeekCursor<KEY, VALUE> seeker = SeekCursor( i, i ) )
			  {
					// then
					assertTrue( seeker.Next() );
					AssertEqualsKey( Key( i ), seeker.Get().key() );
					AssertEqualsValue( Value( i ), seeker.Get().value() );
					assertFalse( seeker.Next() );
			  }
		 }

		 /* INSERT */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindNewKeyInsertedAfterOfSeekPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindNewKeyInsertedAfterOfSeekPoint()
		 {
			  // GIVEN
			  int middle = 2;
			  for ( int i = 0; i < middle; i++ )
			  {
					Append( i );
			  }
			  long fromInclusive = 0;
			  long toExclusive = middle + 1; // Will insert middle later

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 AssertKeyAndValue( cursor, readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key at the end of leaf
					Append( middle );
					this._cursor.forceRetry();

					// Seeker continue
					while ( cursor.Next() )
					{
						 AssertKeyAndValue( cursor, readKeys );
						 readKeys++;
					}
					assertEquals( toExclusive, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindNewKeyInsertedAfterOfSeekPointBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindNewKeyInsertedAfterOfSeekPointBackwards()
		 {
			  // GIVEN
			  int middle = 2;
			  for ( int i = 1; i <= middle; i++ )
			  {
					Append( i );
			  }
			  long fromInclusive = middle;
			  long toExclusive = 0; // Will insert 0 later

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 AssertKeyAndValue( cursor, middle - readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key at the end of leaf
					InsertIn( 0, 0 );
					this._cursor.forceRetry();

					// Seeker continue
					while ( cursor.Next() )
					{
						 AssertKeyAndValue( cursor, middle - readKeys );
						 readKeys++;
					}
					assertEquals( toExclusive, middle - readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyInsertedOnSeekPosition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyInsertedOnSeekPosition()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  int middle = 2;
			  for ( int i = 0; i < middle; i++ )
			  {
					long key = i * 2;
					Append( key );
					expected.Add( key );
			  }
			  long fromInclusive = 0;
			  long toExclusive = middle * 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key in position where seeker will read next
					long midInsert = expected[stopPoint] - 1;
					InsertIn( stopPoint, midInsert );
					expected.Insert( stopPoint, midInsert );
					this._cursor.forceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyInsertedOnSeekPositionBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyInsertedOnSeekPositionBackwards()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  int middle = 2;
			  for ( int i = middle; i > 0; i-- )
			  {
					long key = i * 2;
					Insert( key );
					expected.Add( key );
			  }
			  long fromInclusive = middle * 2;
			  long toExclusive = 0;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key in position where seeker will read next
					long midInsert = expected[stopPoint] + 1;
					Insert( midInsert );
					expected.Insert( stopPoint, midInsert );
					this._cursor.forceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotFindKeyInsertedBeforeOfSeekPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotFindKeyInsertedBeforeOfSeekPoint()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  int middle = 2;
			  for ( int i = 0; i < middle; i++ )
			  {
					long key = i * 2;
					Append( key );
					expected.Add( key );
			  }
			  long fromInclusive = 0;
			  long toExclusive = middle * 2;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key to the left of seekers next position
					long midInsert = expected[readKeys - 1] - 1;
					InsertIn( stopPoint - 1, midInsert );
					this._cursor.forceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotFindKeyInsertedBeforeOfSeekPointBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotFindKeyInsertedBeforeOfSeekPointBackwards()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  int middle = 2;
			  for ( int i = middle; i > 0; i-- )
			  {
					long key = i * 2;
					Insert( key );
					expected.Add( key );
			  }
			  long fromInclusive = middle * 2;
			  long toExclusive = 0;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					int stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key to the left of seekers next position
					long midInsert = expected[readKeys - 1] + 1;
					Insert( midInsert );
					this._cursor.forceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

		 /* INSERT INTO SPLIT */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToLeft() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToLeft()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  long maxKeyCount = FullLeaf( expected );
			  long fromInclusive = 0;
			  long toExclusive = maxKeyCount + 1; // We will add maxKeyCount later

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					long middle = maxKeyCount / 2;
					long stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key which causes a split
					expected.Add( maxKeyCount );
					Insert( maxKeyCount );

					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToRightBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToRightBackwards()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  long lastSeed = FullLeaf( 1, expected );
			  expected.Reverse(); // Because backwards
			  long fromInclusive = lastSeed - 1;
			  long toExclusive = -1; // We will add 0 later

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					long middle = lastSeed / 2;
					long stopPoint = middle / 2;
					int readKeys = 0;
					while ( readKeys < stopPoint && seeker.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( seeker, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key which causes a split
					expected.Add( 0L );
					Insert( 0L );

					seekCursor.ForceRetry();

					while ( seeker.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( seeker, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToRight() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToRight()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  long maxKeyCount = FullLeaf( expected );
			  long fromInclusive = 0;
			  long toExclusive = maxKeyCount + 1; // We will add maxKeyCount later

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					long middle = maxKeyCount / 2;
					long stopPoint = middle + ( middle / 2 );
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key which causes a split
					expected.Add( maxKeyCount );
					Insert( maxKeyCount );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToLeftBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustContinueToNextLeafWhenRangeIsSplitIntoRightLeafAndPosToLeftBackwards()
		 {
			  // GIVEN
			  IList<long> expected = new List<long>();
			  long lastSeed = FullLeaf( 1, expected );
			  expected.Reverse(); // Because backwards
			  long fromInclusive = lastSeed - 1;
			  long toExclusive = -1; // We will add 0 later

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					long middle = lastSeed / 2;
					long stopPoint = middle + ( middle / 2 );
					int readKeys = 0;
					while ( readKeys < stopPoint && cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer insert new key which causes a split
					expected.Add( 0L );
					Insert( 0L );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 long key = expected[readKeys];
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( expected.Count, readKeys );
			  }
		 }

		 /* REMOVE */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotFindKeyRemovedInFrontOfSeeker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotFindKeyRemovedInFrontOfSeeker()
		 {
			  // GIVEN
			  // [0 1 ... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = 0;
			  long toExclusive = maxKeyCount;

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = SeekCursor( fromInclusive, toExclusive ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && cursor.Next() )
					{
						 long key = readKeys;
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					// [0 1 ... maxKeyCount-2]
					RemoveAtPos( ( int ) maxKeyCount - 1 );
					this._cursor.forceRetry();

					while ( cursor.Next() )
					{
						 long key = readKeys;
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( maxKeyCount - 1, readKeys );
			  }
		 }

		 /* INCONSISTENCY */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowIfStuckInInfiniteRootCatchup()
		 internal virtual void MustThrowIfStuckInInfiniteRootCatchup()
		 {
			  assertTimeout(Duration.ofSeconds(10), () =>
			  {
				// given
				RootWithTwoLeaves();

				// Find left child and corrupt it by overwriting type to make it look like freelist node instead of tree node.
				GoTo( _utilCursor, _rootId );
				long leftChild = _node.childAt( _utilCursor, 0, _stableGeneration, _unstableGeneration );
				GoTo( _utilCursor, leftChild );
				_utilCursor.putByte( TreeNode.BYTE_POS_NODE_TYPE, TreeNode.NODE_TYPE_FREE_LIST_NODE );

				// when
				RootCatchup tripCountingRootCatchup = new TripCountingRootCatchup( () => new Root(_rootId, _rootGeneration) );
				assertThrows(typeof(TreeInconsistencyException), () =>
				{
					 using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0, 0, _cursor, _stableGeneration, _unstableGeneration, tripCountingRootCatchup ) )
					 {
						  seek.Next();
					 }
				});
			  });
		 }

		 private long FullLeaf( IList<long> expectedSeeds )
		 {
			  return FullLeaf( 0, expectedSeeds );
		 }

		 private long FullLeaf( long firstSeed )
		 {
			  return FullLeaf( firstSeed, new List<long>() );
		 }

		 private long FullLeaf( long firstSeed, IList<long> expectedSeeds )
		 {
			  int keyCount = 0;
			  KEY key = key( firstSeed + keyCount );
			  VALUE value = value( firstSeed + keyCount );
			  while ( _node.leafOverflow( _cursor, keyCount, key, value ) == TreeNode.Overflow.No )
			  {
					_node.insertKeyValueAt( _cursor, key, value, keyCount, keyCount );
					expectedSeeds.Add( firstSeed + keyCount );
					keyCount++;
					key = key( firstSeed + keyCount );
					value = value( firstSeed + keyCount );
			  }
			  TreeNode.SetKeyCount( _cursor, keyCount );
			  return firstSeed + keyCount;
		 }

		 /// <returns> next seed to be inserted </returns>
		 private long FullLeaf()
		 {
			  return fullLeaf( 0 );
		 }

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

		 private long GetSeed( KEY primKey )
		 {
			  return _layout.keySeed( primKey );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotFindKeyRemovedInFrontOfSeekerBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotFindKeyRemovedInFrontOfSeekerBackwards()
		 {
			  // GIVEN
			  // [1 2 ... maxKeyCount]
			  long lastSeed = fullLeaf( 1 );
			  long maxKeyCount = lastSeed - 1;
			  long fromInclusive = maxKeyCount;
			  long toExclusive = 0;

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && seeker.Next() )
					{
						 AssertKeyAndValue( seeker, maxKeyCount - readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					// [2 ... maxKeyCount]
					Remove( 1 );
					seekCursor.ForceRetry();

					while ( seeker.Next() )
					{
						 AssertKeyAndValue( seeker, maxKeyCount - readKeys );
						 readKeys++;
					}
					assertEquals( maxKeyCount - 1, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyMovedPassedSeekerBecauseOfRemove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyMovedPassedSeekerBecauseOfRemove()
		 {
			  // GIVEN
			  // [0 1 ... maxKeyCount-1]
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = 0;
			  long toExclusive = maxKeyCount;

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && cursor.Next() )
					{
						 long key = readKeys;
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					// [1 ... maxKeyCount-1]
					RemoveAtPos( 0 );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 long key = readKeys;
						 AssertKeyAndValue( cursor, key );
						 readKeys++;
					}
					assertEquals( maxKeyCount, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyMovedPassedSeekerBecauseOfRemoveBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyMovedPassedSeekerBecauseOfRemoveBackwards()
		 {
			  // GIVEN
			  // [1 2... maxKeyCount]
			  long lastSeed = fullLeaf( 1 );
			  long maxKeyCount = lastSeed - 1;
			  long fromInclusive = maxKeyCount;
			  long toExclusive = 0;

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && cursor.Next() )
					{
						 AssertKeyAndValue( cursor, maxKeyCount - readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					// [1 ... maxKeyCount-1]
					Remove( maxKeyCount );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 AssertKeyAndValue( cursor, maxKeyCount - readKeys );
						 readKeys++;
					}
					assertEquals( maxKeyCount, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyMovedSeekerBecauseOfRemoveOfMostRecentReturnedKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyMovedSeekerBecauseOfRemoveOfMostRecentReturnedKey()
		 {
			  // GIVEN
			  long maxKeyCount = FullLeaf();
			  long fromInclusive = 0;
			  long toExclusive = maxKeyCount;

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && cursor.Next() )
					{
						 AssertKeyAndValue( cursor, readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					Remove( readKeys - 1 );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 AssertKeyAndValue( cursor, readKeys );
						 readKeys++;
					}
					assertEquals( maxKeyCount, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindKeyMovedSeekerBecauseOfRemoveOfMostRecentReturnedKeyBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindKeyMovedSeekerBecauseOfRemoveOfMostRecentReturnedKeyBackwards()
		 {
			  // GIVEN
			  long i = fullLeaf( 1 );
			  long maxKeyCount = i - 1;
			  long fromInclusive = i - 1;
			  long toExclusive = 0;

			  // WHEN
			  PageAwareByteArrayCursor seekCursor = _cursor.duplicate();
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> cursor = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					// THEN
					long middle = maxKeyCount / 2;
					int readKeys = 0;
					while ( readKeys < middle && cursor.Next() )
					{
						 AssertKeyAndValue( cursor, maxKeyCount - readKeys );
						 readKeys++;
					}

					// Seeker pauses and writer remove rightmost key
					Remove( maxKeyCount - readKeys + 1 );
					seekCursor.ForceRetry();

					while ( cursor.Next() )
					{
						 AssertKeyAndValue( cursor, maxKeyCount - readKeys );
						 readKeys++;
					}
					assertEquals( maxKeyCount, readKeys );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRereadHeadersOnRetry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustRereadHeadersOnRetry()
		 {
			  // GIVEN
			  int keyCount = 2;
			  InsertKeysAndValues( keyCount );
			  KEY from = Key( 0 );
			  KEY to = Key( keyCount + 1 ); // +1 because we're adding one more down below

			  // WHEN
			  using ( SeekCursor<KEY, VALUE> cursor = new SeekCursor<KEY, VALUE>( this._cursor, _node, from, to, _layout, _stableGeneration, _unstableGeneration, () => 0L, _failingRootCatchup, _unstableGeneration, _exceptionDecorator, 1 ) )
			  {
					// reading a couple of keys
					assertTrue( cursor.Next() );
					AssertEqualsKey( Key( 0 ), cursor.Get().key() );

					// and WHEN a change happens
					Append( keyCount );
					this._cursor.forceRetry();

					// THEN at least keyCount should be re-read on next()
					assertTrue( cursor.Next() );

					// and the new key should be found in the end as well
					AssertEqualsKey( Key( 1 ), cursor.Get().key() );
					long lastFoundKey = 1;
					while ( cursor.Next() )
					{
						 AssertEqualsKey( Key( lastFoundKey + 1 ), cursor.Get().key() );
						 lastFoundKey = GetSeed( cursor.Get().key() );
					}
					assertEquals( keyCount, lastFoundKey );
			  }
		 }

		 /* REBALANCE (when rebalance is implemented) */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenCompletelyRebalancedToTheRightBeforeCallToNext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenCompletelyRebalancedToTheRightBeforeCallToNext()
		 {
			  // given
			  long key = 10;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  // ... enough keys in left child to be rebalanced to the right
			  for ( long smallKey = 0; smallKey < 2; smallKey++ )
			  {
					Insert( smallKey );
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );
			  readCursor.Next( pointer( leftChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  KEY readKey = _layout.newKey();
			  _node.keyAt( readCursor, readKey, keyCount - 1, LEAF );
			  long fromInclusive = GetSeed( readKey );
			  long toExclusive = fromInclusive + 1;

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					TriggerUnderflowAndSeekRange( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenCompletelyRebalancedToTheRightBeforeCallToNextBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenCompletelyRebalancedToTheRightBeforeCallToNextBackwards()
		 {
			  // given
			  long key = 10;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  // ... enough keys in left child to be rebalanced to the right
			  for ( long smallKey = 0; smallKey < 2; smallKey++ )
			  {
					Insert( smallKey );
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );
			  readCursor.Next( pointer( leftChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  KEY from = _layout.newKey();
			  _node.keyAt( readCursor, from, keyCount - 1, LEAF );
			  long fromInclusive = GetSeed( from );
			  long toExclusive = fromInclusive - 1;

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					TriggerUnderflowAndSeekRange( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenCompletelyRebalancedToTheRightAfterCallToNext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenCompletelyRebalancedToTheRightAfterCallToNext()
		 {
			  // given
			  long key = 10;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  // ... enough keys in left child to be rebalanced to the right
			  for ( long smallKey = 0; smallKey < 2; smallKey++ )
			  {
					Insert( smallKey );
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );
			  readCursor.Next( pointer( leftChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  KEY from = _layout.newKey();
			  KEY to = _layout.newKey();
			  _node.keyAt( readCursor, from, keyCount - 2, LEAF );
			  _node.keyAt( readCursor, to, keyCount - 1, LEAF );
			  long fromInclusive = GetSeed( from );
			  long toExclusive = GetSeed( to ) + 1;

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenCompletelyRebalancedToTheRightAfterCallToNextBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenCompletelyRebalancedToTheRightAfterCallToNextBackwards()
		 {
			  // given
			  long key = 10;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  // ... enough keys in left child to be rebalanced to the right
			  for ( long smallKey = 0; smallKey < 2; smallKey++ )
			  {
					Insert( smallKey );
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );
			  readCursor.Next( pointer( leftChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  KEY from = _layout.newKey();
			  KEY to = _layout.newKey();
			  _node.keyAt( readCursor, from, keyCount - 1, LEAF );
			  _node.keyAt( readCursor, to, keyCount - 2, LEAF );
			  long fromInclusive = GetSeed( from );
			  long toExclusive = GetSeed( to ) - 1;

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
			  }
		 }

		 /* MERGE */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenMergingFromCurrentSeekNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenMergingFromCurrentSeekNode()
		 {
			  // given
			  long key = 0;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );

			  // from first key in left child
			  readCursor.Next( pointer( leftChild ) );
			  KEY from = _layout.newKey();
			  _node.keyAt( readCursor, from, 0, LEAF );
			  long fromInclusive = GetSeed( from );
			  long toExclusive = GetSeed( from ) + 2;

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					assertThat( seekCursor.CurrentPageId, @is( leftChild ) );
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
					readCursor.Next( _rootId );
					assertTrue( TreeNode.IsLeaf( readCursor ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenMergingToCurrentSeekNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenMergingToCurrentSeekNode()
		 {
			  // given
			  long key = 0;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );

			  // from first key in left child
			  readCursor.Next( pointer( rightChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  long fromInclusive = KeyAt( readCursor, keyCount - 3, LEAF );
			  long toExclusive = KeyAt( readCursor, keyCount - 1, LEAF );

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					assertThat( seekCursor.CurrentPageId, @is( rightChild ) );
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, leftChild );
					readCursor.Next( _rootId );
					assertTrue( TreeNode.IsLeaf( readCursor ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenMergingToCurrentSeekNodeBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenMergingToCurrentSeekNodeBackwards()
		 {
			  // given
			  long key = 0;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );

			  // from first key in left child
			  readCursor.Next( pointer( rightChild ) );
			  int keyCount = TreeNode.KeyCount( readCursor );
			  long fromInclusive = KeyAt( readCursor, keyCount - 1, LEAF );
			  long toExclusive = KeyAt( readCursor, keyCount - 3, LEAF );

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					assertThat( seekCursor.CurrentPageId, @is( rightChild ) );
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, leftChild );
					readCursor.Next( _rootId );
					assertTrue( TreeNode.IsLeaf( readCursor ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFindRangeWhenMergingFromCurrentSeekNodeBackwards() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFindRangeWhenMergingFromCurrentSeekNodeBackwards()
		 {
			  // given
			  long key = 0;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( key );
					key++;
			  }

			  PageAwareByteArrayCursor readCursor = _cursor.duplicate( _rootId );
			  readCursor.Next();
			  long leftChild = ChildAt( readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( readCursor, 1, _stableGeneration, _unstableGeneration );

			  // from first key in left child
			  readCursor.Next( pointer( leftChild ) );
			  KEY from = _layout.newKey();
			  _node.keyAt( readCursor, from, 0, LEAF );
			  long fromInclusive = GetSeed( from ) + 2;
			  long toExclusive = GetSeed( from );

			  // when
			  TestPageCursor seekCursor = new TestPageCursor( _cursor.duplicate( _rootId ) );
			  seekCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seeker = seekCursor( fromInclusive, toExclusive, seekCursor ) )
			  {
					assertThat( seekCursor.CurrentPageId, @is( leftChild ) );
					SeekRangeWithUnderflowMidSeek( seeker, seekCursor, fromInclusive, toExclusive, rightChild );
					readCursor.Next( _rootId );
					assertTrue( TreeNode.IsLeaf( readCursor ) );
			  }
		 }

		 /* POINTER GENERATION TESTING */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRereadSiblingIfReadFailureCausedByConcurrentCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRereadSiblingIfReadFailureCausedByConcurrentCheckpoint()
		 {
			  // given
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  long currentNode = _cursor.CurrentPageId;
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0L, i, _cursor ) )
			  {
					// when right sibling gets an successor
					Checkpoint();
					PageAwareByteArrayCursor duplicate = _cursor.duplicate( currentNode );
					duplicate.Next();
					Insert( i, i * 10, duplicate );

					// then
					// we should not fail to read right sibling
					//noinspection StatementWithEmptyBody
					while ( seek.Next() )
					{
						 // ignore
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnSiblingReadFailureIfNotCausedByConcurrentCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailOnSiblingReadFailureIfNotCausedByConcurrentCheckpoint()
		 {
			  // given
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  long currentNode = _cursor.CurrentPageId;
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0L, i, _cursor ) )
			  {
					// when right sibling pointer is corrupt
					PageAwareByteArrayCursor duplicate = _cursor.duplicate( currentNode );
					duplicate.Next();
					long leftChild = ChildAt( duplicate, 0, _stableGeneration, _unstableGeneration );
					duplicate.Next( leftChild );
					CorruptGSPP( duplicate, TreeNode.BytePosRightsibling );

					// even if we DO have a checkpoint
					Checkpoint();

					// then
					// we should fail to read right sibling
					assertThrows(typeof(TreeInconsistencyException), () =>
					{
					 while ( seek.Next() )
					 {
						  // ignore
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRereadSuccessorIfReadFailureCausedByCheckpointInLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRereadSuccessorIfReadFailureCausedByCheckpointInLeaf()
		 {
			  // given
			  IList<long> expected = new List<long>();
			  IList<long> actual = new List<long>();
			  long i = 0L;
			  for ( ; i < 2; i++ )
			  {
					Insert( i );
					expected.Add( i );
			  }

			  long currentNode = _cursor.CurrentPageId;
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0L, 5, _cursor ) )
			  {
					// when
					Checkpoint();
					PageAwareByteArrayCursor duplicate = _cursor.duplicate( currentNode );
					duplicate.Next();
					Insert( i, i, duplicate ); // Create successor of leaf
					expected.Add( i );
					_cursor.forceRetry();

					while ( seek.Next() )
					{
						 Hit<KEY, VALUE> hit = seek.Get();
						 actual.Add( GetSeed( hit.Key() ) );
					}
			  }

			  // then
			  assertEquals( expected, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailSuccessorIfReadFailureNotCausedByCheckpointInLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailSuccessorIfReadFailureNotCausedByCheckpointInLeaf()
		 {
			  // given
			  long i = 0L;
			  for ( ; i < 2; i++ )
			  {
					Insert( i );
			  }

			  long currentNode = _cursor.CurrentPageId;
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0L, 5, _cursor ) )
			  {
					// when
					Checkpoint();
					PageAwareByteArrayCursor duplicate = _cursor.duplicate( currentNode );
					duplicate.Next();
					Insert( i, i, duplicate ); // Create successor of leaf

					// and corrupt successor pointer
					CorruptGSPP( duplicate, TreeNode.BytePosSuccessor );
					_cursor.forceRetry();

					// then
					assertThrows(typeof(TreeInconsistencyException), () =>
					{
					 while ( seek.Next() )
					 {
						  // ignore
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRereadSuccessorIfReadFailureCausedByCheckpointInInternal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRereadSuccessorIfReadFailureCausedByCheckpointInInternal()
		 {
			  // given
			  // a root with two leaves in old generation
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  // a checkpoint
			  long oldRootId = _rootId;
			  long oldStableGeneration = _stableGeneration;
			  long oldUnstableGeneration = _unstableGeneration;
			  Checkpoint();
			  int keyCount = TreeNode.KeyCount( _cursor );

			  // and update root with an insert in new generation
			  while ( keyCount( _rootId ) == keyCount )
			  {
					Insert( i );
					i++;
			  }
			  TreeNode.GoTo( _cursor, "root", _rootId );
			  long rightChild = ChildAt( _cursor, 2, _stableGeneration, _unstableGeneration );

			  // when
			  // starting a seek on the old root with generation that is not up to date, simulating a concurrent checkpoint
			  PageAwareByteArrayCursor pageCursorForSeeker = _cursor.duplicate( oldRootId );
			  BreadcrumbPageCursor breadcrumbCursor = new BreadcrumbPageCursor( pageCursorForSeeker );
			  breadcrumbCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( i, i + 1, breadcrumbCursor, oldStableGeneration, oldUnstableGeneration ) )
			  {
					//noinspection StatementWithEmptyBody
					while ( seek.Next() )
					{
					}
			  }

			  // then
			  // make sure seek cursor went to successor of root node
			  assertEquals( Arrays.asList( oldRootId, _rootId, rightChild ), breadcrumbCursor.Breadcrumbs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int keyCount(long nodeId) throws java.io.IOException
		 private int KeyCount( long nodeId )
		 {
			  long prevId = _cursor.CurrentPageId;
			  try
			  {
					TreeNode.GoTo( _cursor, "supplied", nodeId );
					return TreeNode.KeyCount( _cursor );
			  }
			  finally
			  {
					TreeNode.GoTo( _cursor, "prev", prevId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailSuccessorIfReadFailureNotCausedByCheckpointInInternal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailSuccessorIfReadFailureNotCausedByCheckpointInInternal()
		 {
			  // given
			  // a root with two leaves in old generation
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  // a checkpoint
			  long oldRootId = _rootId;
			  long oldStableGeneration = _stableGeneration;
			  long oldUnstableGeneration = _unstableGeneration;
			  Checkpoint();
			  int keyCount = TreeNode.KeyCount( _cursor );

			  // and update root with an insert in new generation
			  while ( keyCount( _rootId ) == keyCount )
			  {
					Insert( i );
					i++;
			  }

			  // and corrupt successor pointer
			  _cursor.next( oldRootId );
			  CorruptGSPP( _cursor, TreeNode.BytePosSuccessor );

			  // when
			  // starting a seek on the old root with generation that is not up to date, simulating a concurrent checkpoint
			  PageAwareByteArrayCursor pageCursorForSeeker = _cursor.duplicate( oldRootId );
			  pageCursorForSeeker.Next();
			  long position = i;
			  assertThrows( typeof( TreeInconsistencyException ), () => SeekCursor(position, position + 1, pageCursorForSeeker, oldStableGeneration, oldUnstableGeneration) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRereadChildPointerIfReadFailureCausedByCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRereadChildPointerIfReadFailureCausedByCheckpoint()
		 {
			  // given
			  // a root with two leaves in old generation
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  // a checkpoint
			  long oldStableGeneration = _stableGeneration;
			  long oldUnstableGeneration = _unstableGeneration;
			  Checkpoint();

			  // and an update to root with a child pointer in new generation
			  Insert( i );
			  i++;
			  long newRightChild = ChildAt( _cursor, 1, _stableGeneration, _unstableGeneration );

			  // when
			  // starting a seek on the old root with generation that is not up to date, simulating a concurrent checkpoint
			  PageAwareByteArrayCursor pageCursorForSeeker = _cursor.duplicate( _rootId );
			  BreadcrumbPageCursor breadcrumbCursor = new BreadcrumbPageCursor( pageCursorForSeeker );
			  breadcrumbCursor.Next();
			  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( i, i + 1, breadcrumbCursor, oldStableGeneration, oldUnstableGeneration ) )
			  {
					//noinspection StatementWithEmptyBody
					while ( seek.Next() )
					{
					}
			  }

			  // then
			  // make sure seek cursor went to successor of root node
			  assertEquals( Arrays.asList( _rootId, newRightChild ), breadcrumbCursor.Breadcrumbs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailChildPointerIfReadFailureNotCausedByCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailChildPointerIfReadFailureNotCausedByCheckpoint()
		 {
			  // given
			  // a root with two leaves in old generation
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }

			  // a checkpoint
			  long oldStableGeneration = _stableGeneration;
			  long oldUnstableGeneration = _unstableGeneration;
			  Checkpoint();

			  // and update root with an insert in new generation
			  Insert( i );
			  i++;

			  // and corrupt successor pointer
			  CorruptGSPP( _cursor, _node.childOffset( 1 ) );

			  // when
			  // starting a seek on the old root with generation that is not up to date, simulating a concurrent checkpoint
			  PageAwareByteArrayCursor pageCursorForSeeker = _cursor.duplicate( _rootId );
			  pageCursorForSeeker.Next();
			  long position = i;
			  assertThrows( typeof( TreeInconsistencyException ), () => SeekCursor(position, position + 1, pageCursorForSeeker, oldStableGeneration, oldUnstableGeneration) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCatchupRootWhenRootNodeHasTooNewGeneration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCatchupRootWhenRootNodeHasTooNewGeneration()
		 {
			  // given
			  long id = _cursor.CurrentPageId;
			  long generation = TreeNode.Generation( _cursor );
			  MutableBoolean triggered = new MutableBoolean( false );
			  RootCatchup rootCatchup = fromId =>
			  {
				triggered.setTrue();
				return new Root( id, generation );
			  };

			  // when
			  //noinspection EmptyTryBlock
			  using ( SeekCursor<KEY, VALUE> ignored = new SeekCursor<KEY, VALUE>( _cursor, _node, Key( 0 ), Key( 1 ), _layout, _stableGeneration, _unstableGeneration, _generationSupplier, rootCatchup, generation - 1, _exceptionDecorator, 1 ) )
			  {
					// do nothing
			  }

			  // then
			  assertTrue( triggered.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCatchupRootWhenNodeHasTooNewGenerationWhileTraversingDownTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCatchupRootWhenNodeHasTooNewGenerationWhileTraversingDownTree()
		 {
			  // given
			  long generation = TreeNode.Generation( _cursor );
			  MutableBoolean triggered = new MutableBoolean( false );
			  long rightChild = 999; // We don't care

			  // a newer leaf
			  long leftChild = _cursor.CurrentPageId;
			  _node.initializeLeaf( _cursor, _stableGeneration + 1, _unstableGeneration + 1 ); // A newer leaf
			  _cursor.next();

			  // a root
			  long rootId = _cursor.CurrentPageId;
			  _node.initializeInternal( _cursor, _stableGeneration, _unstableGeneration );
			  long keyInRoot = 10L;
			  _node.insertKeyAndRightChildAt( _cursor, Key( keyInRoot ), rightChild, 0, 0, _stableGeneration, _unstableGeneration );
			  TreeNode.SetKeyCount( _cursor, 1 );
			  // with old pointer to child (simulating reuse of child node)
			  _node.setChildAt( _cursor, leftChild, 0, _stableGeneration, _unstableGeneration );

			  // a root catchup that records usage
			  RootCatchup rootCatchup = fromId =>
			  {
				triggered.setTrue();

				// and set child generation to match pointer
				_cursor.next( leftChild );
				_cursor.zapPage();
				_node.initializeLeaf( _cursor, _stableGeneration, _unstableGeneration );

				_cursor.next( rootId );
				return new Root( rootId, generation );
			  };

			  // when
			  KEY from = Key( 1L );
			  KEY to = Key( 2L );
			  //noinspection EmptyTryBlock
			  using ( SeekCursor<KEY, VALUE> ignored = new SeekCursor<KEY, VALUE>( _cursor, _node, from, to, _layout, _stableGeneration, _unstableGeneration, _generationSupplier, rootCatchup, _unstableGeneration, _exceptionDecorator, 1 ) )
			  {
					// do nothing
			  }

			  // then
			  assertTrue( triggered.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCatchupRootWhenNodeHasTooNewGenerationWhileTraversingLeaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCatchupRootWhenNodeHasTooNewGenerationWhileTraversingLeaves()
		 {
			  // given
			  MutableBoolean triggered = new MutableBoolean( false );
			  long oldRightChild = 666; // We don't care

			  // a newer right leaf
			  long rightChild = _cursor.CurrentPageId;
			  _node.initializeLeaf( _cursor, _stableGeneration, _unstableGeneration );
			  _cursor.next();

			  RootCatchup rootCatchup = fromId =>
			  {
				// Use right child as new start over root to terminate test
				_cursor.next( rightChild );
				triggered.setTrue();
				return new Root( _cursor.CurrentPageId, TreeNode.Generation( _cursor ) );
			  };

			  // a left leaf
			  long leftChild = _cursor.CurrentPageId;
			  _node.initializeLeaf( _cursor, _stableGeneration - 1, _unstableGeneration - 1 );
			  // with an old pointer to right sibling
			  TreeNode.SetRightSibling( _cursor, rightChild, _stableGeneration - 1, _unstableGeneration - 1 );
			  _cursor.next();

			  // a root
			  _node.initializeInternal( _cursor, _stableGeneration - 1, _unstableGeneration - 1 );
			  long keyInRoot = 10L;
			  _node.insertKeyAndRightChildAt( _cursor, Key( keyInRoot ), oldRightChild, 0, 0, _stableGeneration, _unstableGeneration );
			  TreeNode.SetKeyCount( _cursor, 1 );
			  // with old pointer to child (simulating reuse of internal node)
			  _node.setChildAt( _cursor, leftChild, 0, _stableGeneration, _unstableGeneration );

			  // when
			  KEY from = Key( 1L );
			  KEY to = Key( 20L );
			  using ( SeekCursor<KEY, VALUE> seek = new SeekCursor<KEY, VALUE>( _cursor, _node, from, to, _layout, _stableGeneration - 1, _unstableGeneration - 1, _generationSupplier, rootCatchup, _unstableGeneration, _exceptionDecorator, 1 ) )
			  {
					while ( seek.Next() )
					{
						 seek.Get();
					}
			  }

			  // then
			  assertTrue( triggered.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowTreeInconsistencyExceptionOnBadReadWithoutShouldRetryWhileTraversingTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowTreeInconsistencyExceptionOnBadReadWithoutShouldRetryWhileTraversingTree()
		 {
			  // GIVEN
			  int keyCount = 10000;

			  // WHEN
			  _cursor.Offset = TreeNode.BytePosKeycount;
			  _cursor.putInt( keyCount ); // Bad key count

			  // THEN
			  //noinspection EmptyTryBlock
			  try
			  {
					  using ( SeekCursor<KEY, VALUE> ignored = SeekCursor( 0L, long.MaxValue ) )
					  {
						// Do nothing
					  }
			  }
			  catch ( TreeInconsistencyException e )
			  {
					assertThat( e.Message, containsString( "keyCount:" + keyCount ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowTreeInconsistencyExceptionOnBadReadWithoutShouldRetryWhileTraversingLeaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowTreeInconsistencyExceptionOnBadReadWithoutShouldRetryWhileTraversingLeaves()
		 {
			  // GIVEN
			  // a root with two leaves in old generation
			  int keyCount = 10000;
			  long i = 0L;
			  while ( _numberOfRootSplits == 0 )
			  {
					Insert( i );
					i++;
			  }
			  long rootId = _cursor.CurrentPageId;
			  long leftChild = _node.childAt( _cursor, 0, _stableGeneration, _unstableGeneration );

			  // WHEN
			  GoTo( _cursor, leftChild );
			  _cursor.Offset = TreeNode.BytePosKeycount;
			  _cursor.putInt( keyCount ); // Bad key count
			  GoTo( _cursor, rootId );

			  // THEN
			  try
			  {
					  using ( SeekCursor<KEY, VALUE> seek = SeekCursor( 0L, long.MaxValue ) )
					  {
						//noinspection StatementWithEmptyBody
						while ( seek.Next() )
						{
							 // Do nothing
						}
					  }
			  }
			  catch ( TreeInconsistencyException e )
			  {
					assertThat( e.Message, containsString( "keyCount:" + keyCount ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerUnderflowAndSeekRange(SeekCursor<KEY,VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long rightChild) throws java.io.IOException
		 private void TriggerUnderflowAndSeekRange( SeekCursor<KEY, VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long rightChild )
		 {
			  // ... then seeker should still find range
			  int stride = fromInclusive <= toExclusive ? 1 : -1;
			  TriggerUnderflowAndSeekRange( seeker, seekCursor, fromInclusive, toExclusive, rightChild, stride );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void seekRangeWithUnderflowMidSeek(SeekCursor<KEY,VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long underflowNode) throws java.io.IOException
		 private void SeekRangeWithUnderflowMidSeek( SeekCursor<KEY, VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long underflowNode )
		 {
			  // ... seeker has started seeking in range
			  assertTrue( seeker.Next() );
			  assertThat( GetSeed( seeker.Get().key() ), @is(fromInclusive) );

			  int stride = fromInclusive <= toExclusive ? 1 : -1;
			  TriggerUnderflowAndSeekRange( seeker, seekCursor, fromInclusive + stride, toExclusive, underflowNode, stride );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerUnderflowAndSeekRange(SeekCursor<KEY,VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long rightChild, int stride) throws java.io.IOException
		 private void TriggerUnderflowAndSeekRange( SeekCursor<KEY, VALUE> seeker, TestPageCursor seekCursor, long fromInclusive, long toExclusive, long rightChild, int stride )
		 {
			  // ... rebalance happens before first call to next
			  TriggerUnderflow( rightChild );
			  seekCursor.Changed(); // ByteArrayPageCursor is not aware of should retry, so fake it here

			  for ( long expected = fromInclusive; Long.compare( expected, toExclusive ) * stride < 0; expected += stride )
			  {
					assertTrue( seeker.Next() );
					assertThat( GetSeed( seeker.Get().key() ), @is(expected) );
			  }
			  assertFalse( seeker.Next() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerUnderflow(long nodeId) throws java.io.IOException
		 private void TriggerUnderflow( long nodeId )
		 {
			  // On underflow keys will move from left to right
			  // and key count of the right will increase.
			  // We don't know if keys will move from nodeId to
			  // right sibling or to nodeId from left sibling.
			  // So we monitor both nodeId and rightSibling.
			  PageCursor readCursor = _cursor.duplicate( nodeId );
			  readCursor.Next();
			  int midKeyCount = TreeNode.KeyCount( readCursor );
			  int prevKeyCount = midKeyCount + 1;

			  PageCursor rightSiblingCursor = null;
			  long rightSibling = TreeNode.RightSibling( readCursor, _stableGeneration, _unstableGeneration );
			  int rightKeyCount = 0;
			  int prevRightKeyCount = 1;
			  bool monitorRight = TreeNode.IsNode( rightSibling );
			  if ( monitorRight )
			  {
					rightSiblingCursor = _cursor.duplicate( GenerationSafePointerPair.Pointer( rightSibling ) );
					rightSiblingCursor.Next();
					rightKeyCount = TreeNode.KeyCount( rightSiblingCursor );
					prevRightKeyCount = rightKeyCount + 1;
			  }

			  while ( midKeyCount < prevKeyCount && rightKeyCount <= prevRightKeyCount )
			  {
					long toRemove = KeyAt( readCursor, 0, LEAF );
					Remove( toRemove );
					prevKeyCount = midKeyCount;
					midKeyCount = TreeNode.KeyCount( readCursor );
					if ( monitorRight )
					{
						 prevRightKeyCount = rightKeyCount;
						 rightKeyCount = TreeNode.KeyCount( rightSiblingCursor );
					}
			  }
		 }

		 private void Checkpoint()
		 {
			  _stableGeneration = _unstableGeneration;
			  _unstableGeneration++;
		 }

		 private void NewRootFromSplit( StructurePropagation<KEY> split )
		 {
			  assertTrue( split.HasRightKeyInsert );
			  long rootId = _id.acquireNewId( _stableGeneration, _unstableGeneration );
			  _cursor.next( rootId );
			  _node.initializeInternal( _cursor, _stableGeneration, _unstableGeneration );
			  _node.setChildAt( _cursor, split.MidChild, 0, _stableGeneration, _unstableGeneration );
			  _node.insertKeyAndRightChildAt( _cursor, split.RightKey, split.RightChild, 0, 0, _stableGeneration, _unstableGeneration );
			  TreeNode.SetKeyCount( _cursor, 1 );
			  split.HasRightKeyInsert = false;
			  _numberOfRootSplits++;
			  UpdateRoot();
		 }

		 private void CorruptGSPP( PageAwareByteArrayCursor duplicate, int offset )
		 {
			  int someBytes = duplicate.GetInt( offset );
			  duplicate.PutInt( offset, ~someBytes );
			  someBytes = duplicate.GetInt( offset + GenerationSafePointer.Size );
			  duplicate.PutInt( offset + GenerationSafePointer.Size, ~someBytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(long key) throws java.io.IOException
		 private void Insert( long key )
		 {
			  Insert( key, key );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(long key, long value) throws java.io.IOException
		 private void Insert( long key, long value )
		 {
			  Insert( key, value, _cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(long key, long value, org.Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void Insert( long key, long value, PageCursor cursor )
		 {
			  _treeLogic.insert( cursor, _structurePropagation, key( key ), value( value ), overwrite(), _stableGeneration, _unstableGeneration );
			  HandleAfterChange();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void remove(long key) throws java.io.IOException
		 private void Remove( long key )
		 {
			  _treeLogic.remove( _cursor, _structurePropagation, key( key ), _layout.newValue(), _stableGeneration, _unstableGeneration );
			  HandleAfterChange();
		 }

		 private void HandleAfterChange()
		 {
			  if ( _structurePropagation.hasRightKeyInsert )
			  {
					NewRootFromSplit( _structurePropagation );
			  }
			  if ( _structurePropagation.hasMidChildUpdate )
			  {
					_structurePropagation.hasMidChildUpdate = false;
					UpdateRoot();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SeekCursor<KEY,VALUE> seekCursor(long fromInclusive, long toExclusive) throws java.io.IOException
		 private SeekCursor<KEY, VALUE> SeekCursor( long fromInclusive, long toExclusive )
		 {
			  return SeekCursor( fromInclusive, toExclusive, _cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SeekCursor<KEY,VALUE> seekCursor(long fromInclusive, long toExclusive, org.Neo4Net.io.pagecache.PageCursor pageCursor) throws java.io.IOException
		 private SeekCursor<KEY, VALUE> SeekCursor( long fromInclusive, long toExclusive, PageCursor pageCursor )
		 {
			  return SeekCursor( fromInclusive, toExclusive, pageCursor, _stableGeneration, _unstableGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SeekCursor<KEY,VALUE> seekCursor(long fromInclusive, long toExclusive, org.Neo4Net.io.pagecache.PageCursor pageCursor, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private SeekCursor<KEY, VALUE> SeekCursor( long fromInclusive, long toExclusive, PageCursor pageCursor, long stableGeneration, long unstableGeneration )
		 {
			  return SeekCursor( fromInclusive, toExclusive, pageCursor, stableGeneration, unstableGeneration, _failingRootCatchup );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SeekCursor<KEY,VALUE> seekCursor(long fromInclusive, long toExclusive, org.Neo4Net.io.pagecache.PageCursor pageCursor, long stableGeneration, long unstableGeneration, RootCatchup rootCatchup) throws java.io.IOException
		 private SeekCursor<KEY, VALUE> SeekCursor( long fromInclusive, long toExclusive, PageCursor pageCursor, long stableGeneration, long unstableGeneration, RootCatchup rootCatchup )
		 {
			  return new SeekCursor<KEY, VALUE>( pageCursor, _node, Key( fromInclusive ), Key( toExclusive ), _layout, stableGeneration, unstableGeneration, _generationSupplier, rootCatchup, unstableGeneration, _exceptionDecorator, _random.Next( 1, DEFAULT_MAX_READ_AHEAD ) );
		 }

		 /// <summary>
		 /// Create a right sibling to node pointed to by cursor. Leave cursor on new right sibling when done,
		 /// and return id of left sibling.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createRightSibling(org.Neo4Net.io.pagecache.PageCursor pageCursor) throws java.io.IOException
		 private long CreateRightSibling( PageCursor pageCursor )
		 {
			  long left = pageCursor.CurrentPageId;
			  long right = left + 1;

			  TreeNode.SetRightSibling( pageCursor, right, _stableGeneration, _unstableGeneration );

			  pageCursor.Next( right );
			  _node.initializeLeaf( pageCursor, _stableGeneration, _unstableGeneration );
			  TreeNode.SetLeftSibling( pageCursor, left, _stableGeneration, _unstableGeneration );
			  return left;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRangeInSingleLeaf(long fromInclusive, long toExclusive, SeekCursor<KEY,VALUE> cursor) throws java.io.IOException
		 private void AssertRangeInSingleLeaf( long fromInclusive, long toExclusive, SeekCursor<KEY, VALUE> cursor )
		 {
			  int stride = fromInclusive <= toExclusive ? 1 : -1;
			  long expected = fromInclusive;
			  while ( cursor.Next() )
			  {
					KEY key = key( expected );
					VALUE value = value( expected );
					AssertKeyAndValue( cursor, key, value );
					expected += stride;
			  }
			  assertEquals( toExclusive, expected );
		 }

		 private void AssertKeyAndValue( SeekCursor<KEY, VALUE> cursor, long expectedKeySeed )
		 {
			  KEY key = key( expectedKeySeed );
			  VALUE value = value( expectedKeySeed );
			  AssertKeyAndValue( cursor, key, value );
		 }

		 private void AssertKeyAndValue( SeekCursor<KEY, VALUE> cursor, KEY expectedKey, VALUE expectedValue )
		 {
			  KEY foundKey = cursor.Get().key();
			  VALUE foundValue = cursor.Get().value();
			  AssertEqualsKey( expectedKey, foundKey );
			  AssertEqualsValue( expectedValue, foundValue );
		 }

		 private void AssertEqualsKey( KEY expected, KEY actual )
		 {
			  assertEquals( 0, _layout.Compare( expected, actual ), format( "expected equal, expected=%s, actual=%s", expected.ToString(), actual.ToString() ) );
		 }

		 private void AssertEqualsValue( VALUE expected, VALUE actual )
		 {
			  assertEquals( 0, _layout.compareValue( expected, actual ), format( "expected equal, expected=%s, actual=%s", expected.ToString(), actual.ToString() ) );
		 }

		 private void InsertKeysAndValues( int keyCount )
		 {
			  for ( int i = 0; i < keyCount; i++ )
			  {
					Append( i );
			  }
		 }

		 private void Append( long k )
		 {
			  int keyCount = TreeNode.KeyCount( _cursor );
			  _node.insertKeyValueAt( _cursor, Key( k ), Value( k ), keyCount, keyCount );
			  TreeNode.SetKeyCount( _cursor, keyCount + 1 );
		 }

		 private void InsertIn( int pos, long k )
		 {
			  int keyCount = TreeNode.KeyCount( _cursor );
			  KEY key = key( k );
			  VALUE value = value( k );
			  TreeNode.Overflow overflow = _node.leafOverflow( _cursor, keyCount, key, value );
			  if ( overflow != TreeNode.Overflow.No )
			  {
					throw new System.InvalidOperationException( "Can not insert another key in current node" );
			  }
			  _node.insertKeyValueAt( _cursor, key, value, pos, keyCount );
			  TreeNode.SetKeyCount( _cursor, keyCount + 1 );
		 }

		 private void RemoveAtPos( int pos )
		 {
			  int keyCount = TreeNode.KeyCount( _cursor );
			  _node.removeKeyValueAt( _cursor, pos, keyCount );
			  TreeNode.SetKeyCount( _cursor, keyCount - 1 );
		 }

		 private class BreadcrumbPageCursor : DelegatingPageCursor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<long> BreadcrumbsConflict = new List<long>();

			  internal BreadcrumbPageCursor( PageCursor @delegate ) : base( @delegate )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
			  public override bool Next()
			  {
					bool next = base.Next();
					BreadcrumbsConflict.Add( CurrentPageId );
					return next;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
			  public override bool Next( long pageId )
			  {
					bool next = base.Next( pageId );
					BreadcrumbsConflict.Add( CurrentPageId );
					return next;
			  }

			  internal virtual IList<long> Breadcrumbs
			  {
				  get
				  {
						return BreadcrumbsConflict;
				  }
			  }
		 }

		 private long ChildAt( PageCursor cursor, int pos, long stableGeneration, long unstableGeneration )
		 {
			  return pointer( _node.childAt( cursor, pos, stableGeneration, unstableGeneration ) );
		 }

		 private long KeyAt( PageCursor cursor, int pos, TreeNode.Type type )
		 {
			  KEY readKey = _layout.newKey();
			  _node.keyAt( cursor, readKey, pos, type );
			  return GetSeed( readKey );
		 }

		 // KEEP even if unused
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private void printTree() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void PrintTree()
		 {
			  long currentPageId = _cursor.CurrentPageId;
			  _cursor.next( _rootId );
			  PrintingGBPTreeVisitor<KEY, VALUE> printingVisitor = new PrintingGBPTreeVisitor<KEY, VALUE>( System.out, false, false, false, false, false );
			  ( new GBPTreeStructure<>( _node, _layout, _stableGeneration, _unstableGeneration ) ).visitTree( _cursor, _cursor, printingVisitor );
			  _cursor.next( currentPageId );
		 }
	}

}
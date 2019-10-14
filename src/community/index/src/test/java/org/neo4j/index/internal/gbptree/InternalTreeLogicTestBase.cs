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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTreeConsistencyChecker.assertNoCrashOrBrokenPointerInGSPP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Overflow.NO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Overflow.YES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.LEAF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.ValueMergers.overwrite;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class InternalTreeLogicTestBase<KEY,VALUE>
	public abstract class InternalTreeLogicTestBase<KEY, VALUE>
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal TestLayout<KEY, VALUE> LayoutConflict;
		 protected internal TreeNode<KEY, VALUE> Node;

		 private readonly int _pageSize = 256;
		 private PageAwareByteArrayCursor _cursor;
		 private PageAwareByteArrayCursor _readCursor;
		 private SimpleIdProvider _id;

		 private ValueMerger<KEY, VALUE> _adder;
		 private InternalTreeLogic<KEY, VALUE> _treeLogic;
		 private VALUE _dontCare;
		 private StructurePropagation<KEY> _structurePropagation;

		 private static long _stableGeneration = GenerationSafePointer.MIN_GENERATION;
		 private static long _unstableGeneration = _stableGeneration + 1;
		 private double _ratioToKeepInLeftOnSplit = InternalTreeLogic.DEFAULT_SPLIT_RATIO;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> generators()
		 public static ICollection<object[]> Generators()
		 {
			  IList<object[]> parameters = new List<object[]>();
			  // Initial state has same generation as update state
			  parameters.Add( new object[]{ "NoCheckpoint", GenerationManager.NO_OP_GENERATION, false } );
			  // Update state in next generation
			  parameters.Add( new object[]{ "Checkpoint", GenerationManager.DEFAULT, true } );
			  return parameters;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String name;
		 public string Name;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public GenerationManager generationManager;
		 public GenerationManager GenerationManager;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public boolean isCheckpointing;
		 public bool IsCheckpointing;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public RandomRule Random = new RandomRule();

		 internal Root Root;
		 internal int NumberOfRootSplits;
		 private int _numberOfRootSuccessors;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _cursor = new PageAwareByteArrayCursor( _pageSize );
			  _readCursor = _cursor.duplicate();
			  _id = new SimpleIdProvider( _cursor.duplicate );

			  _id.reset();
			  long newId = _id.acquireNewId( _stableGeneration, _unstableGeneration );
			  GoTo( _cursor, newId );
			  _readCursor.next( newId );

			  LayoutConflict = Layout;
			  Node = GetTreeNode( _pageSize, LayoutConflict );
			  _adder = Adder;
			  _treeLogic = new InternalTreeLogic<KEY, VALUE>( _id, Node, LayoutConflict, NO_MONITOR );
			  _dontCare = LayoutConflict.newValue();
			  _structurePropagation = new StructurePropagation<KEY>( LayoutConflict.newKey(), LayoutConflict.newKey(), LayoutConflict.newKey() );
		 }

		 protected internal abstract ValueMerger<KEY, VALUE> Adder { get; }

		 protected internal abstract TreeNode<KEY, VALUE> GetTreeNode( int pageSize, Layout<KEY, VALUE> layout );

		 protected internal abstract TestLayout<KEY, VALUE> Layout { get; }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustInsertAtFirstPositionInEmptyLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustInsertAtFirstPositionInEmptyLeaf()
		 {
			  // given
			  Initialize();
			  KEY key = key( 1L );
			  VALUE value = value( 1L );
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(0) );

			  // when
			  GenerationManager.checkpoint();
			  Insert( key, value );

			  // then
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(1) );
			  AssertEqualsKey( KeyAt( 0, LEAF ), key );
			  AssertEqualsValue( ValueAt( 0 ), value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSortCorrectlyOnInsertFirstInLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSortCorrectlyOnInsertFirstInLeaf()
		 {
			  // given
			  Initialize();
			  GenerationManager.checkpoint();

			  long someHighSeed = 1000L;
			  int keyCount = 0;
			  KEY newKey = Key( someHighSeed );
			  VALUE newValue = Value( someHighSeed );
			  while ( Node.leafOverflow( _cursor, keyCount, newKey, newValue ) == NO )
			  {
					Insert( newKey, newValue );

					// then
					Root.goTo( _readCursor );
					AssertEqualsKey( KeyAt( 0, LEAF ), newKey );
					AssertEqualsValue( ValueAt( 0 ), newValue );

					keyCount++;
					newKey = Key( someHighSeed - keyCount );
					newValue = Value( someHighSeed - keyCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSortCorrectlyOnInsertLastInLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSortCorrectlyOnInsertLastInLeaf()
		 {
			  // given
			  Initialize();
			  GenerationManager.checkpoint();
			  int keyCount = 0;
			  KEY key = key( keyCount );
			  VALUE value = value( keyCount );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					// when
					Insert( key, value );

					// then
					Root.goTo( _readCursor );
					AssertEqualsKey( KeyAt( keyCount, LEAF ), key );
					AssertEqualsValue( ValueAt( keyCount ), value );

					keyCount++;
					key = key( keyCount );
					value = value( keyCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSortCorrectlyOnInsertInMiddleOfLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSortCorrectlyOnInsertInMiddleOfLeaf()
		 {
			  // given
			  Initialize();
			  GenerationManager.checkpoint();
			  int keyCount = 0;
			  int someHighSeed = 1000;
			  long middleValue = keyCount % 2 == 0 ? keyCount / 2 : someHighSeed - keyCount / 2;
			  KEY key = key( middleValue );
			  VALUE value = value( middleValue );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );

					// then
					Root.goTo( _readCursor );
					AssertEqualsKey( KeyAt( ( keyCount + 1 ) / 2, LEAF ), key );

					keyCount++;
					middleValue = keyCount % 2 == 0 ? keyCount / 2 : someHighSeed - keyCount / 2;
					key = key( middleValue );
					value = value( middleValue );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSplitWhenInsertingMiddleOfFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSplitWhenInsertingMiddleOfFullLeaf()
		 {
			  // given
			  Initialize();
			  int someMiddleSeed = 1000;
			  int keyCount = 0;
			  int middle = keyCount % 2 == 0 ? keyCount : someMiddleSeed - keyCount;
			  KEY key = key( middle );
			  VALUE value = value( middle );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );

					keyCount++;
					middle = keyCount % 2 == 0 ? keyCount : someMiddleSeed - keyCount;
					key = key( middle );
					value = value( middle );
			  }

			  // when
			  GenerationManager.checkpoint();
			  Insert( key, value );

			  // then
			  assertEquals( 1, NumberOfRootSplits );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSplitWhenInsertingLastInFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSplitWhenInsertingLastInFullLeaf()
		 {
			  // given
			  Initialize();
			  int keyCount = 0;
			  KEY key = key( keyCount );
			  VALUE value = value( keyCount );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );
					assertFalse( _structurePropagation.hasRightKeyInsert );

					keyCount++;
					key = key( keyCount );
					value = value( keyCount );
			  }

			  // when
			  GenerationManager.checkpoint();
			  Insert( key, value );

			  // then
			  assertEquals( 1, NumberOfRootSplits ); // Should cause a split
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustSplitWhenInsertingFirstInFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustSplitWhenInsertingFirstInFullLeaf()
		 {
			  // given
			  Initialize();
			  int keyCount = 0;
			  int someHighSeed = 1000;
			  KEY key = key( someHighSeed - keyCount );
			  VALUE value = value( someHighSeed - keyCount );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );
					assertFalse( _structurePropagation.hasRightKeyInsert );

					keyCount++;
					key = key( someHighSeed - keyCount );
					value = value( someHighSeed - keyCount );
			  }

			  // when
			  GenerationManager.checkpoint();
			  Insert( key, value );

			  // then
			  assertEquals( 1, NumberOfRootSplits );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustUpdatePointersInSiblingsToSplit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustUpdatePointersInSiblingsToSplit()
		 {
			  // given
			  Initialize();
			  long someLargeSeed = 10000;
			  int keyCount = 0;
			  KEY key = key( someLargeSeed - keyCount );
			  VALUE value = value( someLargeSeed - keyCount );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );

					keyCount++;
					key = key( someLargeSeed - keyCount );
					value = value( someLargeSeed - keyCount );
			  }

			  // First split
			  GenerationManager.checkpoint();
			  Insert( key, value );
			  keyCount++;
			  key = key( someLargeSeed - keyCount );
			  value = value( keyCount );

			  // Assert child pointers and sibling pointers are intact after split in root
			  Root.goTo( _readCursor );
			  long child0 = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long child1 = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  AssertSiblingOrderAndPointers( child0, child1 );

			  // Insert until we have another split in leftmost leaf
			  while ( keyCount( Root.id() ) == 1 )
			  {
					Insert( key, value );
					keyCount++;
					key = key( someLargeSeed - keyCount );
					value = value( keyCount );
			  }

			  // Just to be sure
			  assertTrue( TreeNode.IsInternal( _readCursor ) );
			  assertThat( TreeNode.KeyCount( _readCursor ), @is( 2 ) );

			  // Assert child pointers and sibling pointers are intact
			  // AND that node not involved in split also has its left sibling pointer updated
			  child0 = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  child1 = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  long child2 = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration ); // <- right sibling to split-node before split

			  AssertSiblingOrderAndPointers( child0, child1, child2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void splitWithSplitRatio0() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SplitWithSplitRatio0()
		 {
			  // given
			  _ratioToKeepInLeftOnSplit = 0;
			  Initialize();
			  int keyCount = 0;
			  KEY key = key( Random.nextLong() );
			  VALUE value = value( Random.nextLong() );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );
					assertFalse( _structurePropagation.hasRightKeyInsert );

					keyCount++;
					key = key( Random.nextLong() );
					value = value( Random.nextLong() );
			  }

			  // when
			  Insert( key, value );

			  // then
			  Root.goTo( _readCursor );
			  long child0 = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long child1 = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  int leftKeyCount = keyCount( child0 );
			  int rightKeyCount = keyCount( child1 );
			  assertEquals( 1, NumberOfRootSplits );

			  // Left node should hold as few keys as possible, such that nothing more can be moved to right.
			  KEY rightmostKeyInLeftChild = KeyAt( child0,leftKeyCount - 1, LEAF );
			  VALUE rightmostValueInLeftChild = ValueAt( child0, leftKeyCount - 1 );
			  GoTo( _readCursor, child1 );
			  assertEquals( YES, Node.leafOverflow( _readCursor, rightKeyCount, rightmostKeyInLeftChild, rightmostValueInLeftChild ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void splitWithSplitRatio1() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SplitWithSplitRatio1()
		 {
			  // given
			  _ratioToKeepInLeftOnSplit = 1;
			  Initialize();
			  int keyCount = 0;
			  KEY key = key( Random.nextLong() );
			  VALUE value = value( Random.nextLong() );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );
					assertFalse( _structurePropagation.hasRightKeyInsert );

					keyCount++;
					key = key( Random.nextLong() );
					value = value( Random.nextLong() );
			  }

			  // when
			  Insert( key, value );

			  // then
			  Root.goTo( _readCursor );
			  long child0 = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long child1 = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  int leftKeyCount = keyCount( child0 );
			  assertEquals( 1, NumberOfRootSplits );

			  // Right node should hold as few keys as possible, such that nothing more can be moved to left.
			  KEY leftmostKeyInRightChild = KeyAt( child1,0, LEAF );
			  VALUE leftmostValueInRightChild = ValueAt( child1, 0 );
			  GoTo( _readCursor, child0 );
			  assertEquals( YES, Node.leafOverflow( _readCursor, leftKeyCount, leftmostKeyInRightChild, leftmostValueInRightChild ) );
		 }

		 /* REMOVE */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveFirstInEmptyLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveFirstInEmptyLeaf()
		 {
			  // given
			  Initialize();
			  long keyValue = 1L;
			  long valueValue = 1L;
			  KEY key = key( keyValue );
			  VALUE value = value( valueValue );
			  Insert( key, value );

			  // when
			  GenerationManager.checkpoint();
			  VALUE readValue = LayoutConflict.newValue();
			  Remove( key, readValue );

			  // then
			  Root.goTo( _readCursor );
			  assertThat( TreeNode.KeyCount( _cursor ), @is( 0 ) );
			  AssertEqualsValue( value, readValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveFirstInFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveFirstInFullLeaf()
		 {
			  // given
			  Initialize();
			  int maxKeyCount = 0;
			  KEY key = key( maxKeyCount );
			  VALUE value = value( maxKeyCount );
			  while ( Node.leafOverflow( _cursor, maxKeyCount, key, value ) == NO )
			  {
					Insert( key, value );

					maxKeyCount++;
					key = key( maxKeyCount );
					value = value( maxKeyCount );
			  }

			  // when
			  GenerationManager.checkpoint();
			  VALUE readValue = LayoutConflict.newValue();
			  Remove( key( 0 ), readValue );

			  // then
			  AssertEqualsValue( value( 0 ), readValue );
			  Root.goTo( _readCursor );
			  assertThat( TreeNode.KeyCount( _readCursor ), @is( maxKeyCount - 1 ) );
			  for ( int i = 0; i < maxKeyCount - 1; i++ )
			  {
					AssertEqualsKey( KeyAt( i, LEAF ), key( i + 1L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveInMiddleInFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveInMiddleInFullLeaf()
		 {
			  // given
			  Initialize();
			  int maxKeyCount = 0;
			  KEY key = key( maxKeyCount );
			  VALUE value = value( maxKeyCount );
			  while ( Node.leafOverflow( _cursor, maxKeyCount, key, value ) == NO )
			  {
					Insert( key, value );

					maxKeyCount++;
					key = key( maxKeyCount );
					value = value( maxKeyCount );
			  }
			  int middle = maxKeyCount / 2;

			  // when
			  GenerationManager.checkpoint();
			  VALUE readValue = LayoutConflict.newValue();
			  Remove( key( middle ), readValue );

			  // then
			  AssertEqualsValue( value( middle ), readValue );
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(maxKeyCount - 1) );
			  AssertEqualsKey( KeyAt( middle, LEAF ), key( middle + 1L ) );
			  for ( int i = 0; i < maxKeyCount - 1; i++ )
			  {
					long expected = i < middle ? i : i + 1L;
					AssertEqualsKey( KeyAt( i, LEAF ), key( expected ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveLastInFullLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveLastInFullLeaf()
		 {
			  Initialize();
			  int maxKeyCount = 0;
			  KEY key = key( maxKeyCount );
			  VALUE value = value( maxKeyCount );
			  while ( Node.leafOverflow( _cursor, maxKeyCount, key, value ) == NO )
			  {
					Insert( key, value );

					maxKeyCount++;
					key = key( maxKeyCount );
					value = value( maxKeyCount );
			  }

			  // when
			  GenerationManager.checkpoint();
			  VALUE readValue = LayoutConflict.newValue();
			  Remove( key( maxKeyCount - 1 ), readValue );

			  // then
			  AssertEqualsValue( value( maxKeyCount - 1 ), readValue );
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(maxKeyCount - 1) );
			  for ( int i = 0; i < maxKeyCount - 1; i++ )
			  {
					AssertEqualsKey( KeyAt( i, LEAF ), key( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveFromLeftChild() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveFromLeftChild()
		 {
			  Initialize();
			  for ( int i = 0; NumberOfRootSplits == 0; i++ )
			  {
					Insert( Key( i ), Value( i ) );
			  }

			  // when
			  GenerationManager.checkpoint();
			  GoTo( _readCursor, _structurePropagation.midChild );
			  AssertEqualsKey( KeyAt( 0, LEAF ), Key( 0L ) );
			  VALUE readValue = LayoutConflict.newValue();
			  Remove( Key( 0 ), readValue );

			  // then
			  AssertEqualsValue( Value( 0 ), readValue );
			  GoTo( _readCursor, _structurePropagation.midChild );
			  AssertEqualsKey( KeyAt( 0, LEAF ), Key( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustRemoveFromRightChildButNotFromInternalWithHitOnInternalSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustRemoveFromRightChildButNotFromInternalWithHitOnInternalSearch()
		 {
			  Initialize();
			  int i;
			  for ( i = 0; NumberOfRootSplits == 0; i++ )
			  {
					Insert( Key( i ), Value( i ) );
			  }
			  Insert( Key( i ), Value( i ) ); // And one more to avoid rebalance

			  // when key to remove exists in internal
			  KEY internalKey = _structurePropagation.rightKey;
			  Root.goTo( _readCursor );
			  AssertEqualsKey( KeyAt( 0, INTERNAL ), internalKey );

			  // and as first key in right child
			  long rightChild = _structurePropagation.rightChild;
			  GoTo( _readCursor, rightChild );
			  int keyCountInRightChild = KeyCount();
			  KEY keyToRemove = KeyAt( 0, LEAF );
			  assertEquals( "expected same seed", GetSeed( keyToRemove ), GetSeed( internalKey ) );

			  // and we remove it
			  GenerationManager.checkpoint();
			  Remove( keyToRemove, _dontCare );

			  // then we should still find it in internal
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(1) );
			  assertEquals( "expected same seed", GetSeed( KeyAt( 0, INTERNAL ) ), GetSeed( keyToRemove ) );

			  // but not in right leaf
			  rightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, rightChild );
			  assertThat( KeyCount(), @is(keyCountInRightChild - 1) );
			  AssertEqualsKey( KeyAt( 0, LEAF ), Key( GetSeed( keyToRemove ) + 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustNotRemoveWhenKeyDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustNotRemoveWhenKeyDoesNotExist()
		 {
			  // given
			  Initialize();
			  int maxKeyCount = 0;
			  KEY key = key( maxKeyCount );
			  VALUE value = value( maxKeyCount );
			  while ( Node.leafOverflow( _cursor, maxKeyCount, key, value ) == NO )
			  {
					Insert( key, value );

					maxKeyCount++;
					key = key( maxKeyCount );
					value = value( maxKeyCount );
			  }

			  // when
			  GenerationManager.checkpoint();
			  assertNull( Remove( key( maxKeyCount ), _dontCare ) );

			  // then
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(maxKeyCount) );
			  for ( int i = 0; i < maxKeyCount; i++ )
			  {
					AssertEqualsKey( KeyAt( i, LEAF ), key( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustNotRemoveWhenKeyOnlyExistInInternal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustNotRemoveWhenKeyOnlyExistInInternal()
		 {
			  // given
			  Initialize();
			  int i;
			  for ( i = 0; NumberOfRootSplits == 0; i++ )
			  {
					Insert( Key( i ), Value( i ) );
			  }
			  Insert( Key( i ), Value( i ) ); // And an extra to not cause rebalance

			  // when key to remove exists in internal
			  long currentRightChild = _structurePropagation.rightChild;
			  KEY keyToRemove = KeyAt( currentRightChild, 0, LEAF );
			  assertEquals( GetSeed( KeyAt( Root.id(), 0, INTERNAL ) ), GetSeed(keyToRemove) );

			  // and as first key in right child
			  GoTo( _readCursor, currentRightChild );
			  int keyCountInRightChild = KeyCount();
			  assertEquals( "same seed", GetSeed( keyToRemove ), GetSeed( KeyAt( 0, LEAF ) ) );

			  // and we remove it
			  GenerationManager.checkpoint();
			  Remove( keyToRemove, _dontCare ); // Possibly create successor of right child
			  Root.goTo( _readCursor );
			  currentRightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );

			  // then we should still find it in internal
			  assertThat( KeyCount(), @is(1) );
			  assertEquals( "same seed", GetSeed( KeyAt( 0, INTERNAL ) ), GetSeed( keyToRemove ) );

			  // but not in right leaf
			  GoTo( _readCursor, currentRightChild );
			  assertThat( KeyCount(), @is(keyCountInRightChild - 1) );
			  assertEquals( "same seed", GetSeed( KeyAt( 0, LEAF ) ), GetSeed( Key( GetSeed( keyToRemove ) + 1 ) ) );

			  // and when we remove same key again, nothing should change
			  assertNull( Remove( keyToRemove, _dontCare ) );
		 }

		 /* REBALANCE */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotRebalanceFromRightToLeft() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotRebalanceFromRightToLeft()
		 {
			  // given
			  Initialize();
			  long key = 0;
			  while ( NumberOfRootSplits == 0 )
			  {
					Insert( key( key ), Value( key ) );
					key++;
			  }

			  // ... enough keys in right child to share with left child if rebalance is needed
			  Insert( key( key ), Value( key ) );
			  key++;

			  // ... and the prim key diving key range for left child and right child
			  Root.goTo( _readCursor );
			  KEY primKey = KeyAt( 0, INTERNAL );

			  // ... and knowing key count of right child
			  long rightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, rightChild );
			  int expectedKeyCount = TreeNode.KeyCount( _readCursor );

			  // when
			  // ... removing all keys from left child
			  for ( long i = 0; ; i++ )
			  {
					KEY removeKey = key( i );
					if ( LayoutConflict.Compare( removeKey, primKey ) >= 0 )
					{
						 break;
					}
					Remove( removeKey, _dontCare );
			  }

			  // then
			  // ... looking a right child
			  GoTo( _readCursor, rightChild );

			  // ... no keys should have moved from right sibling
			  int actualKeyCount = TreeNode.KeyCount( _readCursor );
			  assertEquals( "actualKeyCount=" + actualKeyCount + ", expectedKeyCount=" + expectedKeyCount, expectedKeyCount, actualKeyCount );
			  assertEquals( "same seed", GetSeed( primKey ), GetSeed( KeyAt( 0, LEAF ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPropagateAllStructureChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPropagateAllStructureChanges()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  //given
			  Initialize();
			  long key = 10;
			  while ( NumberOfRootSplits == 0 )
			  {
					Insert( key( key ), Value( key ) );
					key++;
			  }
			  // ... enough keys in left child to share with right child if rebalance is needed
			  for ( long smallKey = 0; smallKey < 2; smallKey++ )
			  {
					Insert( key( smallKey ), Value( smallKey ) );
			  }

			  // ... and the prim key dividing key range for left and right child
			  Root.goTo( _readCursor );
			  KEY oldPrimKey = KeyAt( 0, INTERNAL );

			  // ... and left and right child
			  long originalLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long originalRightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, originalRightChild );
			  IList<KEY> keysInRightChild = AllKeys( _readCursor, LEAF );

			  // when
			  // ... after checkpoint
			  GenerationManager.checkpoint();

			  // ... removing keys from right child until rebalance is triggered
			  int index = 0;
			  long rightChild;
			  KEY originalLeftmost = keysInRightChild[0];
			  KEY leftmostInRightChild;
			  do
			  {
					Remove( keysInRightChild[index], _dontCare );
					index++;
					Root.goTo( _readCursor );
					rightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
					GoTo( _readCursor, rightChild );
					leftmostInRightChild = KeyAt( 0, LEAF );
			  } while ( LayoutConflict.Compare( leftmostInRightChild, originalLeftmost ) >= 0 );

			  // then
			  // ... primKey in root is updated
			  Root.goTo( _readCursor );
			  KEY primKey = KeyAt( 0, INTERNAL );
			  AssertEqualsKey( primKey, leftmostInRightChild );
			  AssertNotEqualsKey( primKey, oldPrimKey );

			  // ... new versions of left and right child
			  long newLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long newRightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  assertThat( newLeftChild, @is( not( originalLeftChild ) ) );
			  assertThat( newRightChild, @is( not( originalRightChild ) ) );
		 }

		 /* MERGE */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPropagateStructureOnMergeFromLeft() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPropagateStructureOnMergeFromLeft()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN:
			  //       ------root-------
			  //      /        |         \
			  //     v         v          v
			  //   left <--> middle <--> right
			  IList<KEY> allKeys = new List<KEY>();
			  Initialize();
			  long targetLastId = _id.lastId() + 3; // 2 splits and 1 new allocated root
			  long i = 0;
			  for ( ; _id.lastId() < targetLastId; i++ )
			  {
					KEY key = key( i );
					Insert( key, Value( i ) );
					allKeys.Add( key );
			  }
			  Root.goTo( _readCursor );
			  assertEquals( 2, KeyCount() );
			  long oldRootId = _readCursor.CurrentPageId;
			  long oldLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long oldMiddleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  long oldRightChild = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration );
			  AssertSiblings( oldLeftChild, oldMiddleChild, oldRightChild );

			  // WHEN
			  GenerationManager.checkpoint();
			  KEY middleKey = KeyAt( oldMiddleChild,0, LEAF ); // Should be located in middle leaf
			  Remove( middleKey, _dontCare );
			  allKeys.Remove( middleKey );

			  // THEN
			  // old root should still have 2 keys
			  GoTo( _readCursor, oldRootId );
			  assertEquals( 2, KeyCount() );

			  // new root should have only 1 key
			  Root.goTo( _readCursor );
			  assertEquals( 1, KeyCount() );

			  // left child should be a new node
			  long newLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  assertNotEquals( newLeftChild, oldLeftChild );
			  assertNotEquals( newLeftChild, oldMiddleChild );

			  // right child should be same old node
			  long newRightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  assertEquals( newRightChild, oldRightChild );

			  // old left and old middle has new left as successor
			  GoTo( _readCursor, oldLeftChild );
			  assertEquals( newLeftChild, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );
			  GoTo( _readCursor, oldMiddleChild );
			  assertEquals( newLeftChild, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );

			  // new left child contain keys from old left and old middle
			  GoTo( _readCursor, oldRightChild );
			  KEY firstKeyOfOldRightChild = KeyAt( 0, LEAF );
			  int index = IndexOf( firstKeyOfOldRightChild, allKeys, LayoutConflict );
			  IList<KEY> expectedKeysInNewLeftChild = allKeys.subList( 0, index );
			  GoTo( _readCursor, newLeftChild );
			  AssertNodeContainsExpectedKeys( expectedKeysInNewLeftChild, LEAF );

			  // new children are siblings
			  AssertSiblings( newLeftChild, oldRightChild, TreeNode.NO_NODE_FLAG );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPropagateStructureOnMergeToRight() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPropagateStructureOnMergeToRight()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN:
			  //        ---------root---------
			  //       /           |          \
			  //      v            v           v
			  //   oldleft <-> oldmiddle <-> oldright
			  IList<KEY> allKeys = new List<KEY>();
			  Initialize();
			  long targetLastId = _id.lastId() + 3; // 2 splits and 1 new allocated root
			  long i = 0;
			  for ( ; _id.lastId() < targetLastId; i++ )
			  {
					KEY key = key( i );
					Insert( key, Value( i ) );
					allKeys.Add( key );
			  }
			  Root.goTo( _readCursor );
			  assertEquals( 2, KeyCount() );
			  long oldLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long oldMiddleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  long oldRightChild = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration );
			  AssertSiblings( oldLeftChild, oldMiddleChild, oldRightChild );
			  GoTo( _readCursor, oldLeftChild );
			  KEY keyInLeftChild = KeyAt( 0, LEAF );

			  // WHEN
			  GenerationManager.checkpoint();
			  // removing key in left child
			  Root.goTo( _readCursor );
			  Remove( keyInLeftChild, _dontCare );
			  allKeys.Remove( keyInLeftChild );
			  // New structure
			  // NOTE: oldleft gets a successor (intermediate) before removing key and then another one once it is merged,
			  //       effectively creating a chain of successor pointers to our newleft that in the end contain keys from
			  //       oldleft and oldmiddle
			  //                                                         ----root----
			  //                                                        /            |
			  //                                                       v             v
			  // oldleft -[successor]-> intermediate -[successor]-> newleft <-> oldright
			  //                                                      ^
			  //                                                       \-[successor]- oldmiddle

			  // THEN
			  // old root should still have 2 keys
			  assertEquals( 2, KeyCount() );

			  // new root should have only 1 key
			  Root.goTo( _readCursor );
			  assertEquals( 1, KeyCount() );

			  // left child should be a new node
			  long newLeftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  assertNotEquals( newLeftChild, oldLeftChild );
			  assertNotEquals( newLeftChild, oldMiddleChild );

			  // right child should be same old node
			  long newRightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  assertEquals( newRightChild, oldRightChild );

			  // old left and old middle has new left as successor
			  GoTo( _readCursor, oldLeftChild );
			  assertEquals( newLeftChild, NewestGeneration( _readCursor, _stableGeneration, _unstableGeneration ) );
			  GoTo( _readCursor, oldMiddleChild );
			  assertEquals( newLeftChild, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );

			  // new left child contain keys from old left and old middle
			  GoTo( _readCursor, oldRightChild );
			  KEY firstKeyInOldRightChild = KeyAt( 0, LEAF );
			  int index = IndexOf( firstKeyInOldRightChild, allKeys, LayoutConflict );
			  IList<KEY> expectedKeysInNewLeftChild = allKeys.subList( 0, index );
			  GoTo( _readCursor, newLeftChild );
			  AssertNodeContainsExpectedKeys( expectedKeysInNewLeftChild, LEAF );

			  // new children are siblings
			  AssertSiblings( newLeftChild, oldRightChild, TreeNode.NO_NODE_FLAG );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPropagateStructureWhenMergingBetweenDifferentSubtrees() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPropagateStructureWhenMergingBetweenDifferentSubtrees()
		 {
			  // GIVEN
			  // We will merge oldLeft into oldRight
			  //                               -----root----
			  //                             /               \
			  //                            v                 v
			  //                 _____leftParent    <->      rightParent_____
			  //                / / /         \              /           \ \ \
			  //               v v v           v            v             v v v
			  // [some more children]       oldLeft <-> oldRight         [some more children]
			  Initialize();
			  long i = 0;
			  while ( NumberOfRootSplits < 2 )
			  {
					Insert( Key( i ), Value( i ) );
					i++;
			  }

			  Root.goTo( _readCursor );
			  long oldLeft = RightmostLeafInSubtree( Root.id(), 0 );
			  long oldRight = LeftmostLeafInSubtree( Root.id(), 1 );
			  KEY oldSplitter = KeyAt( 0, INTERNAL );
			  KEY rightmostKeyInLeftSubtree = RightmostInternalKeyInSubtree( Root.id(), 0 );

			  List<KEY> allKeysInOldLeftAndOldRight = new List<KEY>();
			  GoTo( _readCursor, oldLeft );
			  AllKeys( _readCursor, allKeysInOldLeftAndOldRight, LEAF );
			  GoTo( _readCursor, oldRight );
			  AllKeys( _readCursor, allKeysInOldLeftAndOldRight, LEAF );

			  KEY keyInOldRight = KeyAt( 0, LEAF );

			  // WHEN
			  GenerationManager.checkpoint();
			  Remove( keyInOldRight, _dontCare );
			  Remove( keyInOldRight, allKeysInOldLeftAndOldRight, LayoutConflict );

			  // THEN
			  // oldSplitter in root should have been replaced by rightmostKeyInLeftSubtree
			  Root.goTo( _readCursor );
			  KEY newSplitter = KeyAt( 0, INTERNAL );
			  AssertNotEqualsKey( newSplitter, oldSplitter );
			  AssertEqualsKey( newSplitter, rightmostKeyInLeftSubtree );

			  // rightmostKeyInLeftSubtree should have been removed from successor version of leftParent
			  KEY newRightmostInternalKeyInLeftSubtree = RightmostInternalKeyInSubtree( Root.id(), 0 );
			  AssertNotEqualsKey( newRightmostInternalKeyInLeftSubtree, rightmostKeyInLeftSubtree );

			  // newRight contain all
			  GoToSuccessor( _readCursor, oldRight );
			  IList<KEY> allKeysInNewRight = AllKeys( _readCursor, LEAF );
			  assertThat( allKeysInNewRight.Count, @is( allKeysInOldLeftAndOldRight.Count ) );
			  for ( int index = 0; index < allKeysInOldLeftAndOldRight.Count; index++ )
			  {
					AssertEqualsKey( allKeysInOldLeftAndOldRight[index], allKeysInNewRight[index] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLeaveSingleLeafAsRootWhenEverythingIsRemoved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustLeaveSingleLeafAsRootWhenEverythingIsRemoved()
		 {
			  // GIVEN
			  // a tree with some keys
			  IList<KEY> allKeys = new List<KEY>();
			  Initialize();
			  long i = 0;
			  while ( NumberOfRootSplits < 3 )
			  {
					KEY key = key( i );
					Insert( key, Value( i ) );
					allKeys.Add( key );
					i++;
			  }

			  // WHEN
			  // removing all keys but one
			  GenerationManager.checkpoint();
			  for ( int j = 0; j < allKeys.Count - 1; j++ )
			  {
					Remove( allKeys[j], _dontCare );
			  }

			  // THEN
			  Root.goTo( _readCursor );
			  assertTrue( TreeNode.IsLeaf( _readCursor ) );
		 }

		 /* OVERALL CONSISTENCY */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustProduceConsistentTreeWithRandomInserts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustProduceConsistentTreeWithRandomInserts()
		 {
			  // given
			  Initialize();
			  int numberOfEntries = 100_000;
			  for ( int i = 0; i < numberOfEntries; i++ )
			  {
					// when
					long keySeed = Random.nextLong();
					Insert( Key( keySeed ), Value( Random.nextLong() ) );
					if ( i == numberOfEntries / 2 )
					{
						 GenerationManager.checkpoint();
					}
			  }

			  // then
			  Root.goTo( _readCursor );
			  ConsistencyCheck();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustProduceConsistentTreeWithRandomInsertsWithConflictingKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustProduceConsistentTreeWithRandomInsertsWithConflictingKeys()
		 {
			  // given
			  Initialize();
			  int numberOfEntries = 100_000;
			  for ( int i = 0; i < numberOfEntries; i++ )
			  {
					// when
					long keySeed = Random.nextLong( 1000 );
					Insert( Key( keySeed ), Value( Random.nextLong() ) );
					if ( i == numberOfEntries / 2 )
					{
						 GenerationManager.checkpoint();
					}
			  }

			  // then
			  ConsistencyCheck();
		 }

		 /* TEST VALUE MERGER */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustOverwriteWithOverwriteMerger() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustOverwriteWithOverwriteMerger()
		 {
			  // given
			  Initialize();
			  KEY key = key( Random.nextLong() );
			  VALUE firstValue = Value( Random.nextLong() );
			  Insert( key, firstValue );

			  // when
			  GenerationManager.checkpoint();
			  VALUE secondValue = Value( Random.nextLong() );
			  Insert( key, secondValue, ValueMergers.Overwrite() );

			  // then
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(1) );
			  AssertEqualsValue( ValueAt( 0 ), secondValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifierMustKeepExistingWithKeepExistingMerger() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ModifierMustKeepExistingWithKeepExistingMerger()
		 {
			  // given
			  Initialize();
			  KEY key = key( Random.nextLong() );
			  VALUE firstValue = Value( Random.nextLong() );
			  Insert( key, firstValue, ValueMergers.KeepExisting() );
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(1) );
			  VALUE actual = ValueAt( 0 );
			  AssertEqualsValue( actual, firstValue );

			  // when
			  GenerationManager.checkpoint();
			  VALUE secondValue = Value( Random.nextLong() );
			  Insert( key, secondValue, ValueMergers.KeepExisting() );

			  // then
			  Root.goTo( _readCursor );
			  assertThat( KeyCount(), @is(1) );
			  actual = ValueAt( 0 );
			  AssertEqualsValue( actual, firstValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMergeValue()
		 {
			  // GIVEN
			  Initialize();
			  KEY key = key( 10 );
			  long baseValue = 100;
			  Insert( key, Value( baseValue ) );

			  // WHEN
			  GenerationManager.checkpoint();
			  long toAdd = 5;
			  Insert( key, Value( toAdd ), _adder );

			  // THEN
			  Root.goTo( _readCursor );
			  int searchResult = KeySearch.Search( _readCursor, Node, LEAF, key, LayoutConflict.newKey(), KeyCount() );
			  assertTrue( KeySearch.IsHit( searchResult ) );
			  int pos = KeySearch.PositionOf( searchResult );
			  assertEquals( 0, pos );
			  AssertEqualsKey( key, KeyAt( pos, LEAF ) );
			  AssertEqualsValue( Value( baseValue + toAdd ), ValueAt( pos ) );
		 }

		 /* CREATE NEW VERSION ON UPDATE */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenInsertInStableRootAsLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenInsertInStableRootAsLeaf()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN root
			  Initialize();
			  long oldGenerationId = _cursor.CurrentPageId;

			  // WHEN root -[successor]-> successor of root
			  GenerationManager.checkpoint();
			  Insert( Key( 1L ), Value( 1L ) );
			  long successor = _cursor.CurrentPageId;

			  // THEN
			  Root.goTo( _readCursor );
			  assertEquals( 1, _numberOfRootSuccessors );
			  assertEquals( successor, _structurePropagation.midChild );
			  assertNotEquals( oldGenerationId, successor );
			  assertEquals( 1, KeyCount() );

			  GoTo( _readCursor, oldGenerationId );
			  assertEquals( successor, successor( _readCursor, _stableGeneration, _unstableGeneration ) );
			  assertEquals( 0, KeyCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenRemoveInStableRootAsLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenRemoveInStableRootAsLeaf()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN root
			  Initialize();
			  KEY key = key( 1L );
			  VALUE value = value( 10L );
			  Insert( key, value );
			  long oldGenerationId = _cursor.CurrentPageId;

			  // WHEN root -[successor]-> successor of root
			  GenerationManager.checkpoint();
			  Remove( key, _dontCare );
			  long successor = _cursor.CurrentPageId;

			  // THEN
			  Root.goTo( _readCursor );
			  assertEquals( 1, _numberOfRootSuccessors );
			  assertEquals( successor, _structurePropagation.midChild );
			  assertNotEquals( oldGenerationId, successor );
			  assertEquals( 0, KeyCount() );

			  GoTo( _readCursor, oldGenerationId );
			  assertEquals( successor, successor( _readCursor, _stableGeneration, _unstableGeneration ) );
			  assertEquals( 1, KeyCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenInsertInStableLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenInsertInStableLeaf()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN:
			  //       ------root-------
			  //      /        |         \
			  //     v         v          v
			  //   left <--> middle <--> right
			  Initialize();
			  long targetLastId = _id.lastId() + 3; // 2 splits and 1 new allocated root
			  long i = 0;
			  for ( ; _id.lastId() < targetLastId; i++ )
			  {
					Insert( Key( i ), Value( i ) );
			  }
			  Root.goTo( _readCursor );
			  assertEquals( 2, KeyCount() );
			  long leftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long middleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration );
			  AssertSiblings( leftChild, middleChild, rightChild );

			  // WHEN
			  GenerationManager.checkpoint();
			  long middle = i / 2;
			  KEY middleKey = Key( middle ); // Should be located in middle leaf
			  VALUE oldValue = Value( middle );
			  VALUE newValue = Value( middle * 11 );
			  Insert( middleKey, newValue );

			  // THEN
			  // root have new middle child
			  long expectedNewMiddleChild = targetLastId + 1;
			  assertEquals( expectedNewMiddleChild, _id.lastId() );
			  long newMiddleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  assertEquals( expectedNewMiddleChild, newMiddleChild );

			  // old middle child has successor
			  GoTo( _readCursor, middleChild );
			  assertEquals( newMiddleChild, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );

			  // old middle child has seen no change
			  AssertKeyAssociatedWithValue( middleKey, oldValue );

			  // new middle child has seen change
			  GoTo( _readCursor, newMiddleChild );
			  AssertKeyAssociatedWithValue( middleKey, newValue );

			  // sibling pointers updated
			  AssertSiblings( leftChild, newMiddleChild, rightChild );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenRemoveInStableLeaf() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenRemoveInStableLeaf()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN:
			  //       ------root-------
			  //      /        |         \
			  //     v         v          v
			  //   left <--> middle <--> right
			  Initialize();
			  long targetLastId = _id.lastId() + 3; // 2 splits and 1 new allocated root
			  long i = 0;
			  for ( ; _id.lastId() < targetLastId; i += 2 )
			  {
					Insert( Key( i ), Value( i ) );
			  }
			  Root.goTo( _readCursor );
			  assertEquals( 2, KeyCount() );

			  long leftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long middleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  long rightChild = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration );

			  // add some more keys to middleChild to not have remove trigger a merge
			  GoTo( _readCursor, middleChild );
			  KEY firstKeyInMiddleChild = KeyAt( 0, LEAF );
			  VALUE firstValueInMiddleChild = ValueAt( 0 );
			  long seed = GetSeed( firstKeyInMiddleChild );
			  Insert( Key( seed + 1 ), Value( seed + 1 ) );
			  Insert( Key( seed + 3 ), Value( seed + 3 ) );
			  Root.goTo( _readCursor );

			  AssertSiblings( leftChild, middleChild, rightChild );

			  // WHEN
			  GenerationManager.checkpoint();
			  assertNotNull( Remove( firstKeyInMiddleChild, _dontCare ) );

			  // THEN
			  // root have new middle child
			  long expectedNewMiddleChild = targetLastId + 1;
			  assertEquals( expectedNewMiddleChild, _id.lastId() );
			  long newMiddleChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  assertEquals( expectedNewMiddleChild, newMiddleChild );

			  // old middle child has successor
			  GoTo( _readCursor, middleChild );
			  assertEquals( newMiddleChild, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );

			  // old middle child has seen no change
			  AssertKeyAssociatedWithValue( firstKeyInMiddleChild, firstValueInMiddleChild );

			  // new middle child has seen change
			  GoTo( _readCursor, newMiddleChild );
			  AssertKeyNotFound( firstKeyInMiddleChild, LEAF );

			  // sibling pointers updated
			  AssertSiblings( leftChild, newMiddleChild, rightChild );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenInsertInStableRootAsInternal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenInsertInStableRootAsInternal()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN:
			  //                       root
			  //                   ----   ----
			  //                  /           \
			  //                 v             v
			  //               left <-------> right
			  Initialize();

			  // Fill root
			  int keyCount = 0;
			  KEY key = key( keyCount );
			  VALUE value = value( keyCount );
			  while ( Node.leafOverflow( _cursor, keyCount, key, value ) == NO )
			  {
					Insert( key, value );
					keyCount++;
					key = key( keyCount );
					value = value( keyCount );
			  }

			  // Split
			  Insert( key, value );
			  keyCount++;
			  key = key( keyCount );
			  value = value( keyCount );

			  // Fill right child
			  Root.goTo( _readCursor );
			  long rightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, rightChild );
			  int rightChildKeyCount = TreeNode.KeyCount( _readCursor );
			  while ( Node.leafOverflow( _readCursor, rightChildKeyCount, key, value ) == NO )
			  {
					Insert( key, value );
					keyCount++;
					rightChildKeyCount++;
					key = key( keyCount );
					value = value( keyCount );
			  }

			  long oldRootId = Root.id();
			  Root.goTo( _readCursor );
			  assertEquals( 1, keyCount() );
			  long leftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  AssertSiblings( leftChild, rightChild, TreeNode.NO_NODE_FLAG );

			  // WHEN
			  //                       root(successor)
			  //                   ----  | ---------------
			  //                  /      |                \
			  //                 v       v                 v
			  //               left <-> right(successor) <--> farRight
			  GenerationManager.checkpoint();
			  Insert( key, value );
			  assertEquals( 1, _numberOfRootSuccessors );
			  Root.goTo( _readCursor );
			  leftChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  rightChild = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );

			  // THEN
			  // siblings are correct
			  long farRightChild = ChildAt( _readCursor, 2, _stableGeneration, _unstableGeneration );
			  AssertSiblings( leftChild, rightChild, farRightChild );

			  // old root points to successor of root
			  GoTo( _readCursor, oldRootId );
			  assertEquals( Root.id(), Successor(_readCursor, _stableGeneration, _unstableGeneration) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNewVersionWhenInsertInStableInternal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNewVersionWhenInsertInStableInternal()
		 {
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );

			  // GIVEN
			  Initialize();
			  long someHighMultiplier = 1000;
			  for ( int i = 0; NumberOfRootSplits < 2; i++ )
			  {
					long seed = i * someHighMultiplier;
					Insert( Key( seed ), Value( seed ) );
			  }
			  long rootAfterInitialData = Root.id();
			  Root.goTo( _readCursor );
			  assertEquals( 1, KeyCount() );
			  long leftInternal = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  long rightInternal = ChildAt( _readCursor, 1, _stableGeneration, _unstableGeneration );
			  AssertSiblings( leftInternal, rightInternal, TreeNode.NO_NODE_FLAG );
			  GoTo( _readCursor, leftInternal );
			  int leftInternalKeyCount = KeyCount();
			  assertTrue( TreeNode.IsInternal( _readCursor ) );
			  long leftLeaf = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, leftLeaf );
			  KEY firstKeyInLeaf = KeyAt( 0, LEAF );
			  long seedOfFirstKeyInLeaf = GetSeed( firstKeyInLeaf );

			  // WHEN
			  GenerationManager.checkpoint();
			  long targetLastId = _id.lastId() + 3; //one for successor in leaf, one for split leaf, one for successor in internal
			  for ( int i = 0; _id.lastId() < targetLastId; i++ )
			  {
					Insert( Key( seedOfFirstKeyInLeaf + i ), Value( seedOfFirstKeyInLeaf + i ) );
					assertFalse( _structurePropagation.hasRightKeyInsert ); // there should be no root split
			  }

			  // THEN
			  // root hasn't been split further
			  assertEquals( rootAfterInitialData, Root.id() );

			  // there's an successor to left internal w/ one more key in
			  Root.goTo( _readCursor );
			  long successorLeftInternal = _id.lastId();
			  assertEquals( successorLeftInternal, ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration ) );
			  GoTo( _readCursor, successorLeftInternal );
			  int successorLeftInternalKeyCount = KeyCount();
			  assertEquals( leftInternalKeyCount + 1, successorLeftInternalKeyCount );

			  // and left internal points to the successor
			  GoTo( _readCursor, leftInternal );
			  assertEquals( successorLeftInternal, Successor( _readCursor, _stableGeneration, _unstableGeneration ) );
			  AssertSiblings( successorLeftInternal, rightInternal, TreeNode.NO_NODE_FLAG );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverwriteInheritedSuccessorOnSuccessor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverwriteInheritedSuccessorOnSuccessor()
		 {
			  // GIVEN
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );
			  Initialize();
			  long originalNodeId = Root.id();
			  GenerationManager.checkpoint();
			  Insert( Key( 1L ), Value( 10L ) ); // TX1 will create successor
			  assertEquals( 1, _numberOfRootSuccessors );

			  // WHEN
			  // recovery happens
			  GenerationManager.recovery();
			  // start up on stable root
			  GoTo( _cursor, originalNodeId );
			  _treeLogic.initialize( _cursor );
			  // replay transaction TX1 will create a new successor
			  Insert( Key( 1L ), Value( 10L ) );
			  assertEquals( 2, _numberOfRootSuccessors );

			  // THEN
			  Root.goTo( _readCursor );
			  // successor pointer for successor should not have broken or crashed GSPP slot
			  AssertSuccessorPointerNotCrashOrBroken();
			  // and previously crashed successor GSPP slot should have been overwritten
			  GoTo( _readCursor, originalNodeId );
			  AssertSuccessorPointerNotCrashOrBroken();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustThrowIfReachingNodeWithValidSuccessor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfReachingNodeWithValidSuccessor()
		 {
			  // GIVEN
			  // root with two children
			  assumeTrue( "No checkpointing, no successor", IsCheckpointing );
			  Initialize();
			  long someHighMultiplier = 1000;
			  for ( int i = 1; NumberOfRootSplits < 1; i++ )
			  {
					long seed = i * someHighMultiplier;
					Insert( Key( seed ), Value( seed ) );
			  }
			  GenerationManager.checkpoint();

			  // and leftmost child has successor that is not pointed to by parent (root)
			  Root.goTo( _readCursor );
			  long leftmostChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
			  GiveSuccessor( _readCursor, leftmostChild );

			  // WHEN
			  // insert in leftmostChild
			  try
			  {
					Insert( Key( 0 ), Value( 0 ) );
					fail( "Expected insert to throw because child targeted for insertion has a valid new successor." );
			  }
			  catch ( TreeInconsistencyException e )
			  {
					// THEN
					assertThat( e.Message, containsString( PointerChecking.WriterTraverseOldStateMessage ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void consistencyCheck() throws java.io.IOException
		 private void ConsistencyCheck()
		 {
			  long currentPageId = _readCursor.CurrentPageId;
			  Root.goTo( _readCursor );
			  GBPTreeConsistencyChecker<KEY> consistencyChecker = new GBPTreeConsistencyChecker<KEY>( Node, LayoutConflict, _id, _stableGeneration, _unstableGeneration );
			  ThrowingConsistencyCheckVisitor<KEY> visitor = new ThrowingConsistencyCheckVisitor<KEY>();
			  consistencyChecker.Check( null, _readCursor, Root, visitor );
			  GoTo( _readCursor, currentPageId );
		 }

		 private void Remove( KEY toRemove, IList<KEY> list, IComparer<KEY> comparator )
		 {
			  int i = IndexOf( toRemove, list, comparator );
			  list.RemoveAt( i );
		 }

		 private int IndexOf( KEY theKey, IList<KEY> keys, IComparer<KEY> comparator )
		 {
			  int i = 0;
			  foreach ( KEY key in keys )
			  {
					if ( comparator.Compare( theKey, key ) == 0 )
					{
						 return i;
					}
					i++;
			  }
			  return -1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void giveSuccessor(org.neo4j.io.pagecache.PageCursor cursor, long nodeId) throws java.io.IOException
		 private void GiveSuccessor( PageCursor cursor, long nodeId )
		 {
			  GoTo( cursor, nodeId );
			  TreeNode.SetSuccessor( cursor, 42, _stableGeneration, _unstableGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private KEY rightmostInternalKeyInSubtree(long parentNodeId, int subtreePosition) throws java.io.IOException
		 private KEY RightmostInternalKeyInSubtree( long parentNodeId, int subtreePosition )
		 {
			  long current = _readCursor.CurrentPageId;
			  GoToSubtree( parentNodeId, subtreePosition );
			  bool found = false;
			  KEY rightmostKeyInSubtree = LayoutConflict.newKey();
			  while ( TreeNode.IsInternal( _readCursor ) )
			  {
					int keyCount = TreeNode.KeyCount( _readCursor );
					if ( keyCount <= 0 )
					{
						 break;
					}
					rightmostKeyInSubtree = KeyAt( keyCount - 1, INTERNAL );
					found = true;
					long rightmostChild = ChildAt( _readCursor, keyCount, _stableGeneration, _unstableGeneration );
					GoTo( _readCursor, rightmostChild );
			  }
			  if ( !found )
			  {
					throw new System.ArgumentException( "Subtree on position " + subtreePosition + " in node " + parentNodeId + " did not contain a rightmost internal key." );
			  }

			  GoTo( _readCursor, current );
			  return rightmostKeyInSubtree;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void goToSubtree(long parentNodeId, int subtreePosition) throws java.io.IOException
		 private void GoToSubtree( long parentNodeId, int subtreePosition )
		 {
			  GoTo( _readCursor, parentNodeId );
			  long subtree = ChildAt( _readCursor, subtreePosition, _stableGeneration, _unstableGeneration );
			  GoTo( _readCursor, subtree );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long leftmostLeafInSubtree(long parentNodeId, int subtreePosition) throws java.io.IOException
		 private long LeftmostLeafInSubtree( long parentNodeId, int subtreePosition )
		 {
			  long current = _readCursor.CurrentPageId;
			  GoToSubtree( parentNodeId, subtreePosition );
			  long leftmostChild = current;
			  while ( TreeNode.IsInternal( _readCursor ) )
			  {
					leftmostChild = ChildAt( _readCursor, 0, _stableGeneration, _unstableGeneration );
					GoTo( _readCursor, leftmostChild );
			  }

			  GoTo( _readCursor, current );
			  return leftmostChild;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long rightmostLeafInSubtree(long parentNodeId, int subtreePosition) throws java.io.IOException
		 private long RightmostLeafInSubtree( long parentNodeId, int subtreePosition )
		 {
			  long current = _readCursor.CurrentPageId;
			  GoToSubtree( parentNodeId, subtreePosition );
			  long rightmostChild = current;
			  while ( TreeNode.IsInternal( _readCursor ) )
			  {
					int keyCount = TreeNode.KeyCount( _readCursor );
					rightmostChild = ChildAt( _readCursor, keyCount, _stableGeneration, _unstableGeneration );
					GoTo( _readCursor, rightmostChild );
			  }

			  GoTo( _readCursor, current );
			  return rightmostChild;
		 }

		 private void AssertNodeContainsExpectedKeys( IList<KEY> expectedKeys, TreeNode.Type type )
		 {
			  IList<KEY> actualKeys = AllKeys( _readCursor, type );
			  foreach ( KEY actualKey in actualKeys )
			  {
					GBPTreeTestUtil.Contains( expectedKeys, actualKey, LayoutConflict );
			  }
			  foreach ( KEY expectedKey in expectedKeys )
			  {
					GBPTreeTestUtil.Contains( actualKeys, expectedKey, LayoutConflict );
			  }
		 }

		 private IList<KEY> AllKeys( PageCursor cursor, TreeNode.Type type )
		 {
			  IList<KEY> keys = new List<KEY>();
			  return AllKeys( cursor, keys, type );
		 }

		 private IList<KEY> AllKeys( PageCursor cursor, IList<KEY> keys, TreeNode.Type type )
		 {
			  int keyCount = TreeNode.KeyCount( cursor );
			  for ( int i = 0; i < keyCount; i++ )
			  {
					KEY into = LayoutConflict.newKey();
					Node.keyAt( cursor, into, i, type );
					keys.Add( into );
			  }
			  return keys;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int keyCount(long nodeId) throws java.io.IOException
		 private int KeyCount( long nodeId )
		 {
			  long prevId = _readCursor.CurrentPageId;
			  try
			  {
					GoTo( _readCursor, nodeId );
					return TreeNode.KeyCount( _readCursor );
			  }
			  finally
			  {
					GoTo( _readCursor, prevId );
			  }
		 }

		 private int KeyCount()
		 {
			  return TreeNode.KeyCount( _readCursor );
		 }

		 internal virtual void Initialize()
		 {
			  Node.initializeLeaf( _cursor, _stableGeneration, _unstableGeneration );
			  UpdateRoot();
		 }

		 private void UpdateRoot()
		 {
			  Root = new Root( _cursor.CurrentPageId, _unstableGeneration );
			  _treeLogic.initialize( _cursor, _ratioToKeepInLeftOnSplit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSuccessorPointerNotCrashOrBroken() throws java.io.IOException
		 private void AssertSuccessorPointerNotCrashOrBroken()
		 {
			  assertNoCrashOrBrokenPointerInGSPP( null, _readCursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.successor(), TreeNode.BytePosSuccessor, new ThrowingConsistencyCheckVisitor<>() );
		 }

		 private void AssertKeyAssociatedWithValue( KEY key, VALUE expectedValue )
		 {
			  KEY readKey = LayoutConflict.newKey();
			  VALUE readValue = LayoutConflict.newValue();
			  int search = KeySearch.Search( _readCursor, Node, LEAF, key, readKey, TreeNode.KeyCount( _readCursor ) );
			  assertTrue( KeySearch.IsHit( search ) );
			  int keyPos = KeySearch.PositionOf( search );
			  Node.valueAt( _readCursor, readValue, keyPos );
			  AssertEqualsValue( expectedValue, readValue );
		 }

		 private void AssertKeyNotFound( KEY key, TreeNode.Type type )
		 {
			  KEY readKey = LayoutConflict.newKey();
			  int search = KeySearch.Search( _readCursor, Node, type, key, readKey, TreeNode.KeyCount( _readCursor ) );
			  assertFalse( KeySearch.IsHit( search ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSiblings(long left, long middle, long right) throws java.io.IOException
		 private void AssertSiblings( long left, long middle, long right )
		 {
			  long origin = _readCursor.CurrentPageId;
			  GoTo( _readCursor, middle );
			  assertEquals( right, RightSibling( _readCursor, _stableGeneration, _unstableGeneration ) );
			  assertEquals( left, LeftSibling( _readCursor, _stableGeneration, _unstableGeneration ) );
			  if ( left != TreeNode.NO_NODE_FLAG )
			  {
					GoTo( _readCursor, left );
					assertEquals( middle, RightSibling( _readCursor, _stableGeneration, _unstableGeneration ) );
			  }
			  if ( right != TreeNode.NO_NODE_FLAG )
			  {
					GoTo( _readCursor, right );
					assertEquals( middle, LeftSibling( _readCursor, _stableGeneration, _unstableGeneration ) );
			  }
			  GoTo( _readCursor, origin );
		 }

		 // KEEP even if unused
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void printTree() throws java.io.IOException
		 private void PrintTree()
		 {
			  long currentPageId = _cursor.CurrentPageId;
			  _cursor.next( Root.id() );
			  PrintingGBPTreeVisitor<KEY, VALUE> printingVisitor = new PrintingGBPTreeVisitor<KEY, VALUE>( System.out, false, false, false, false, false );
			  ( new GBPTreeStructure<>( Node, LayoutConflict, _stableGeneration, _unstableGeneration ) ).visitTree( _cursor, _cursor, printingVisitor );
			  _cursor.next( currentPageId );
		 }

		 internal virtual KEY Key( long seed )
		 {
			  return LayoutConflict.key( seed );
		 }

		 internal virtual VALUE Value( long seed )
		 {
			  return LayoutConflict.value( seed );
		 }

		 private long GetSeed( KEY key )
		 {
			  return LayoutConflict.keySeed( key );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void newRootFromSplit(StructurePropagation<KEY> split) throws java.io.IOException
		 private void NewRootFromSplit( StructurePropagation<KEY> split )
		 {
			  assertTrue( split.HasRightKeyInsert );
			  long rootId = _id.acquireNewId( _stableGeneration, _unstableGeneration );
			  GoTo( _cursor, rootId );
			  Node.initializeInternal( _cursor, _stableGeneration, _unstableGeneration );
			  Node.setChildAt( _cursor, split.MidChild, 0, _stableGeneration, _unstableGeneration );
			  Node.insertKeyAndRightChildAt( _cursor, split.RightKey, split.RightChild, 0, 0, _stableGeneration, _unstableGeneration );
			  TreeNode.SetKeyCount( _cursor, 1 );
			  split.HasRightKeyInsert = false;
			  UpdateRoot();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSiblingOrderAndPointers(long... children) throws java.io.IOException
		 private void AssertSiblingOrderAndPointers( params long[] children )
		 {
			  long currentPageId = _readCursor.CurrentPageId;
			  RightmostInChain rightmost = new RightmostInChain( null );
			  GenerationKeeper generationTarget = new GenerationKeeper();
			  ThrowingConsistencyCheckVisitor visitor = new ThrowingConsistencyCheckVisitor();
			  foreach ( long child in children )
			  {
					GoTo( _readCursor, child );
					long leftSibling = TreeNode.LeftSibling( _readCursor, _stableGeneration, _unstableGeneration, generationTarget );
					long leftSiblingGeneration = generationTarget.Generation;
					long rightSibling = TreeNode.RightSibling( _readCursor, _stableGeneration, _unstableGeneration, generationTarget );
					long rightSiblingGeneration = generationTarget.Generation;
					rightmost.AssertNext( _readCursor, TreeNode.Generation( _readCursor ), pointer( leftSibling ), leftSiblingGeneration, pointer( rightSibling ), rightSiblingGeneration, visitor );
			  }
			  rightmost.AssertLast( visitor );
			  GoTo( _readCursor, currentPageId );
		 }

		 internal virtual KEY KeyAt( long nodeId, int pos, TreeNode.Type type )
		 {
			  KEY readKey = LayoutConflict.newKey();
			  long prevId = _readCursor.CurrentPageId;
			  try
			  {
					_readCursor.next( nodeId );
					return Node.keyAt( _readCursor, readKey, pos, type );
			  }
			  finally
			  {
					_readCursor.next( prevId );
			  }
		 }

		 private KEY KeyAt( int pos, TreeNode.Type type )
		 {
			  return Node.keyAt( _readCursor, LayoutConflict.newKey(), pos, type );
		 }

		 private VALUE ValueAt( long nodeId, int pos )
		 {
			  VALUE readValue = LayoutConflict.newValue();
			  long prevId = _readCursor.CurrentPageId;
			  try
			  {
					_readCursor.next( nodeId );
					return Node.valueAt( _readCursor, readValue, pos );
			  }
			  finally
			  {
					_readCursor.next( prevId );
			  }
		 }

		 private VALUE ValueAt( int pos )
		 {
			  return Node.valueAt( _readCursor, LayoutConflict.newValue(), pos );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void insert(KEY key, VALUE value) throws java.io.IOException
		 internal virtual void Insert( KEY key, VALUE value )
		 {
			  Insert( key, value, overwrite() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(KEY key, VALUE value, ValueMerger<KEY,VALUE> valueMerger) throws java.io.IOException
		 private void Insert( KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger )
		 {
			  _structurePropagation.hasRightKeyInsert = false;
			  _structurePropagation.hasMidChildUpdate = false;
			  _treeLogic.insert( _cursor, _structurePropagation, key, value, valueMerger, _stableGeneration, _unstableGeneration );
			  HandleAfterChange();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void handleAfterChange() throws java.io.IOException
		 private void HandleAfterChange()
		 {
			  if ( _structurePropagation.hasRightKeyInsert )
			  {
					NewRootFromSplit( _structurePropagation );
					NumberOfRootSplits++;
			  }
			  if ( _structurePropagation.hasMidChildUpdate )
			  {
					_structurePropagation.hasMidChildUpdate = false;
					UpdateRoot();
					_numberOfRootSuccessors++;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private VALUE remove(KEY key, VALUE into) throws java.io.IOException
		 private VALUE Remove( KEY key, VALUE into )
		 {
			  VALUE result = _treeLogic.remove( _cursor, _structurePropagation, key, into, _stableGeneration, _unstableGeneration );
			  HandleAfterChange();
			  return result;
		 }

		 private interface GenerationManager
		 {
			  void Checkpoint();

			  void Recovery();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//		  GenerationManager NO_OP_GENERATION = new GenerationManager()
	//		  {
	//				@@Override public void checkpoint()
	//				{
	//					 // Do nothing
	//				}
	//
	//				@@Override public void recovery()
	//				{
	//					 // Do nothing
	//				}
	//		  };

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//		  GenerationManager DEFAULT = new GenerationManager()
	//		  {
	//				@@Override public void checkpoint()
	//				{
	//					 stableGeneration = unstableGeneration;
	//					 unstableGeneration++;
	//				}
	//
	//				@@Override public void recovery()
	//				{
	//					 unstableGeneration++;
	//				}
	//		  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void goTo(org.neo4j.io.pagecache.PageCursor cursor, long pageId) throws java.io.IOException
		 private static void GoTo( PageCursor cursor, long pageId )
		 {
			  PageCursorUtil.GoTo( cursor, "test", pointer( pageId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void goToSuccessor(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void GoToSuccessor( PageCursor cursor )
		 {
			  long newestGeneration = newestGeneration( cursor, _stableGeneration, _unstableGeneration );
			  GoTo( cursor, newestGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void goToSuccessor(org.neo4j.io.pagecache.PageCursor cursor, long targetNode) throws java.io.IOException
		 private void GoToSuccessor( PageCursor cursor, long targetNode )
		 {
			  GoTo( cursor, targetNode );
			  GoToSuccessor( cursor );
		 }

		 private long ChildAt( PageCursor cursor, int pos, long stableGeneration, long unstableGeneration )
		 {
			  return pointer( Node.childAt( cursor, pos, stableGeneration, unstableGeneration ) );
		 }

		 private long RightSibling( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return pointer( TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration ) );
		 }

		 private long LeftSibling( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return pointer( TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration ) );
		 }

		 private long Successor( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  return pointer( TreeNode.Successor( cursor, stableGeneration, unstableGeneration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long newestGeneration(org.neo4j.io.pagecache.PageCursor cursor, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private long NewestGeneration( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  long current = cursor.CurrentPageId;
			  long successor = current;
			  do
			  {
					GoTo( cursor, successor );
					successor = pointer( TreeNode.Successor( cursor, stableGeneration, unstableGeneration ) );
			  } while ( successor != TreeNode.NO_NODE_FLAG );
			  successor = cursor.CurrentPageId;
			  GoTo( cursor, current );
			  return successor;
		 }

		 private void AssertNotEqualsKey( KEY key1, KEY key2 )
		 {
			  assertNotEquals( string.Format( "expected no not equal, key1={0}, key2={1}", key1.ToString(), key2.ToString() ), 0, LayoutConflict.Compare(key1, key2) );
		 }

		 private void AssertEqualsKey( KEY expected, KEY actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, LayoutConflict.Compare(expected, actual) );
		 }

		 private void AssertEqualsValue( VALUE expected, VALUE actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, LayoutConflict.compareValue(expected, actual) );
		 }
	}

}
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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Overflow = Neo4Net.Index.Internal.gbptree.TreeNode.Overflow;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTreeTestUtil.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.resultIsFromSlotA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.TreeNode.NO_NODE_FLAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.TreeNode.Overflow.NO_NEED_DEFRAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.TreeNode.Overflow.YES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.TreeNode.Type.LEAF;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) public abstract class TreeNodeTestBase<KEY,VALUE>
	public abstract class TreeNodeTestBase<KEY, VALUE>
	{
		 internal const int STABLE_GENERATION = 1;
		 internal const int UNSTABLE_GENERATION = 3;
		 private const int HIGH_GENERATION = 4;

		 internal const int PAGE_SIZE = 512;
		 internal PageCursor Cursor;

		 private TestLayout<KEY, VALUE> _layout;
		 private TreeNode<KEY, VALUE> _node;
		 private readonly GenerationKeeper _generationTarget = new GenerationKeeper();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void prepareCursor() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrepareCursor()
		 {
			  Cursor = new PageAwareByteArrayCursor( PAGE_SIZE );
			  Cursor.next();
			  _layout = Layout;
			  _node = GetNode( PAGE_SIZE, _layout );
		 }

		 protected internal abstract TestLayout<KEY, VALUE> Layout { get; }

		 protected internal abstract TreeNode<KEY, VALUE> GetNode( int pageSize, Layout<KEY, VALUE> layout );

		 internal abstract void AssertAdditionalHeader( PageCursor cursor, TreeNode<KEY, VALUE> node, int pageSize );

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInitializeLeaf()
		 internal virtual void ShouldInitializeLeaf()
		 {
			  // WHEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // THEN
			  assertEquals( TreeNode.NODE_TYPE_TREE_NODE, TreeNode.NodeType( Cursor ) );
			  assertTrue( TreeNode.IsLeaf( Cursor ) );
			  assertFalse( TreeNode.IsInternal( Cursor ) );
			  assertEquals( UNSTABLE_GENERATION, TreeNode.Generation( Cursor ) );
			  assertEquals( 0, TreeNode.KeyCount( Cursor ) );
			  assertEquals( NO_NODE_FLAG, LeftSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  assertEquals( NO_NODE_FLAG, RightSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  assertEquals( NO_NODE_FLAG, Successor( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  AssertAdditionalHeader( Cursor, _node, PAGE_SIZE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInitializeInternal()
		 internal virtual void ShouldInitializeInternal()
		 {
			  // WHEN
			  _node.initializeInternal( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // THEN
			  assertEquals( TreeNode.NODE_TYPE_TREE_NODE, TreeNode.NodeType( Cursor ) );
			  assertFalse( TreeNode.IsLeaf( Cursor ) );
			  assertTrue( TreeNode.IsInternal( Cursor ) );
			  assertEquals( UNSTABLE_GENERATION, TreeNode.Generation( Cursor ) );
			  assertEquals( 0, TreeNode.KeyCount( Cursor ) );
			  assertEquals( NO_NODE_FLAG, LeftSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  assertEquals( NO_NODE_FLAG, RightSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  assertEquals( NO_NODE_FLAG, Successor( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  AssertAdditionalHeader( Cursor, _node, PAGE_SIZE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadMaxGeneration()
		 internal virtual void ShouldWriteAndReadMaxGeneration()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // WHEN
			  TreeNode.SetGeneration( Cursor, GenerationSafePointer.MAX_GENERATION );

			  // THEN
			  long generation = TreeNode.Generation( Cursor );
			  assertEquals( GenerationSafePointer.MAX_GENERATION, generation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfWriteTooLargeGeneration()
		 internal virtual void ShouldThrowIfWriteTooLargeGeneration()
		 {
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  assertThrows( typeof( System.ArgumentException ), () => TreeNode.setGeneration(Cursor, GenerationSafePointer.MAX_GENERATION + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfWriteTooSmallGeneration()
		 internal virtual void ShouldThrowIfWriteTooSmallGeneration()
		 {
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  assertThrows( typeof( System.ArgumentException ), () => TreeNode.setGeneration(Cursor, GenerationSafePointer.MIN_GENERATION - 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void keyValueOperationsInLeaf()
		 internal virtual void KeyValueOperationsInLeaf()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY readKey = _layout.newKey();
			  VALUE readValue = _layout.newValue();

			  // WHEN
			  KEY firstKey = Key( 1 );
			  VALUE firstValue = Value( 10 );
			  _node.insertKeyValueAt( Cursor, firstKey, firstValue, 0, 0 );
			  TreeNode.SetKeyCount( Cursor, 1 );

			  // THEN
			  AssertKeyEquals( firstKey, _node.keyAt( Cursor, readKey, 0, LEAF ) );
			  AssertValueEquals( firstValue, _node.valueAt( Cursor, readValue, 0 ) );

			  // WHEN
			  KEY secondKey = Key( 3 );
			  VALUE secondValue = Value( 30 );
			  _node.insertKeyValueAt( Cursor, secondKey, secondValue, 1, 1 );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // THEN
			  AssertKeyEquals( firstKey, _node.keyAt( Cursor, readKey, 0, LEAF ) );
			  AssertValueEquals( firstValue, _node.valueAt( Cursor, readValue, 0 ) );
			  AssertKeyEquals( secondKey, _node.keyAt( Cursor, readKey, 1, LEAF ) );
			  AssertValueEquals( secondValue, _node.valueAt( Cursor, readValue, 1 ) );

			  // WHEN
			  KEY removedKey = Key( 2 );
			  VALUE removedValue = Value( 20 );
			  _node.insertKeyValueAt( Cursor, removedKey, removedValue, 1, 2 );
			  TreeNode.SetKeyCount( Cursor, 3 );

			  // THEN
			  AssertKeyEquals( firstKey, _node.keyAt( Cursor, readKey, 0, LEAF ) );
			  AssertValueEquals( firstValue, _node.valueAt( Cursor, readValue, 0 ) );
			  AssertKeyEquals( removedKey, _node.keyAt( Cursor, readKey, 1, LEAF ) );
			  AssertValueEquals( removedValue, _node.valueAt( Cursor, readValue, 1 ) );
			  AssertKeyEquals( secondKey, _node.keyAt( Cursor, readKey, 2, LEAF ) );
			  AssertValueEquals( secondValue, _node.valueAt( Cursor, readValue, 2 ) );

			  // WHEN
			  _node.removeKeyValueAt( Cursor, 1, 3 );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // THEN
			  AssertKeyEquals( firstKey, _node.keyAt( Cursor, readKey, 0, LEAF ) );
			  AssertValueEquals( firstValue, _node.valueAt( Cursor, readValue, 0 ) );
			  AssertKeyEquals( secondKey, _node.keyAt( Cursor, readKey, 1, LEAF ) );
			  AssertValueEquals( secondValue, _node.valueAt( Cursor, readValue, 1 ) );

			  // WHEN
			  VALUE overwriteValue = Value( 666 );
			  assertTrue( _node.setValueAt( Cursor, overwriteValue, 0 ), string.Format( "Could not overwrite value, oldValue={0}, newValue={1}", firstValue, overwriteValue ) );

			  // THEN
			  AssertKeyEquals( firstKey, _node.keyAt( Cursor, readKey, 0, LEAF ) );
			  AssertValueEquals( overwriteValue, _node.valueAt( Cursor, readValue, 0 ) );
			  AssertKeyEquals( secondKey, _node.keyAt( Cursor, readKey, 1, LEAF ) );
			  AssertValueEquals( secondValue, _node.valueAt( Cursor, readValue, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void keyChildOperationsInInternal()
		 internal virtual void KeyChildOperationsInInternal()
		 {
			  // GIVEN
			  _node.initializeInternal( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  long stable = 3;
			  long unstable = 4;
			  long zeroChild = 5;

			  // WHEN
			  _node.setChildAt( Cursor, zeroChild, 0, stable, unstable );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, zeroChild );

			  // WHEN
			  long firstKey = 1;
			  long firstChild = 10;
			  _node.insertKeyAndRightChildAt( Cursor, Key( firstKey ), firstChild, 0, 0, stable, unstable );
			  TreeNode.SetKeyCount( Cursor, 1 );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, zeroChild, firstKey, firstChild );

			  // WHEN
			  long secondKey = 3;
			  long secondChild = 30;
			  _node.insertKeyAndRightChildAt( Cursor, Key( secondKey ), secondChild, 1, 1, stable, unstable );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, zeroChild, firstKey, firstChild, secondKey, secondChild );

			  // WHEN
			  long removedKey = 2;
			  long removedChild = 20;
			  _node.insertKeyAndRightChildAt( Cursor, Key( removedKey ), removedChild, 1, 2, stable, unstable );
			  TreeNode.SetKeyCount( Cursor, 3 );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, zeroChild, firstKey, firstChild, removedKey, removedChild, secondKey, secondChild );

			  // WHEN
			  _node.removeKeyAndRightChildAt( Cursor, 1, 3 );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, zeroChild, firstKey, firstChild, secondKey, secondChild );

			  // WHEN
			  _node.removeKeyAndLeftChildAt( Cursor, 0, 2 );
			  TreeNode.SetKeyCount( Cursor, 1 );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, firstChild, secondKey, secondChild );

			  // WHEN
			  long overwriteChild = 666;
			  _node.setChildAt( Cursor, overwriteChild, 0, stable, unstable );

			  // THEN
			  AssertKeysAndChildren( stable, unstable, overwriteChild, secondKey, secondChild );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFillInternal()
		 internal virtual void ShouldFillInternal()
		 {
			  _node.initializeInternal( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  long stable = 3;
			  long unstable = 4;
			  int keyCount = 0;
			  long childId = 10;
			  _node.setChildAt( Cursor, childId, 0, stable, unstable );
			  childId++;
			  KEY key = key( childId );
			  for ( ; _node.internalOverflow( Cursor, keyCount, key ) == Overflow.NO; childId++, keyCount++, key = key( childId ) )
			  {
					_node.insertKeyAndRightChildAt( Cursor, key, childId, keyCount, keyCount, stable, unstable );
			  }

			  // Assert children
			  long firstChild = 10;
			  for ( int i = 0; i <= keyCount; i++ )
			  {
					assertEquals( firstChild + i, pointer( _node.childAt( Cursor, i, stable, unstable ) ) );
			  }

			  // Assert keys
			  int firstKey = 11;
			  KEY readKey = _layout.newKey();
			  for ( int i = 0; i < keyCount; i++ )
			  {
					AssertKeyEquals( key( firstKey + i ), _node.keyAt( Cursor, readKey, i, INTERNAL ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetAndGetKeyCount()
		 internal virtual void ShouldSetAndGetKeyCount()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  assertEquals( 0, TreeNode.KeyCount( Cursor ) );

			  // WHEN
			  int keyCount = 5;
			  TreeNode.SetKeyCount( Cursor, keyCount );

			  // THEN
			  assertEquals( keyCount, TreeNode.KeyCount( Cursor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetAndGetSiblings()
		 internal virtual void ShouldSetAndGetSiblings()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // WHEN
			  TreeNode.SetLeftSibling( Cursor, 123, STABLE_GENERATION, UNSTABLE_GENERATION );
			  TreeNode.SetRightSibling( Cursor, 456, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // THEN
			  assertEquals( 123, LeftSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
			  assertEquals( 456, RightSibling( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetAndGetSuccessor()
		 internal virtual void ShouldSetAndGetSuccessor()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // WHEN
			  TreeNode.SetSuccessor( Cursor, 123, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // THEN
			  assertEquals( 123, Successor( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefragLeafWithTombstoneOnLast()
		 internal virtual void ShouldDefragLeafWithTombstoneOnLast()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY key = key( 1 );
			  VALUE value = value( 1 );
			  _node.insertKeyValueAt( Cursor, key, value, 0, 0 );
			  key = key( 2 );
			  value = value( 2 );
			  _node.insertKeyValueAt( Cursor, key, value, 1, 1 );

			  // AND
			  _node.removeKeyValueAt( Cursor, 1, 2 );
			  TreeNode.SetKeyCount( Cursor, 1 );

			  // WHEN
			  _node.defragmentLeaf( Cursor );

			  // THEN
			  AssertKeyEquals( key( 1 ), _node.keyAt( Cursor, _layout.newKey(), 0, LEAF ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefragLeafWithTombstoneOnFirst()
		 internal virtual void ShouldDefragLeafWithTombstoneOnFirst()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY key = key( 1 );
			  VALUE value = value( 1 );
			  _node.insertKeyValueAt( Cursor, key, value, 0, 0 );
			  key = key( 2 );
			  value = value( 2 );
			  _node.insertKeyValueAt( Cursor, key, value, 1, 1 );

			  // AND
			  _node.removeKeyValueAt( Cursor, 0, 2 );
			  TreeNode.SetKeyCount( Cursor, 1 );

			  // WHEN
			  _node.defragmentLeaf( Cursor );

			  // THEN
			  AssertKeyEquals( key( 2 ), _node.keyAt( Cursor, _layout.newKey(), 0, LEAF ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefragLeafWithTombstoneInterleaved()
		 internal virtual void ShouldDefragLeafWithTombstoneInterleaved()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY key = key( 1 );
			  VALUE value = value( 1 );
			  _node.insertKeyValueAt( Cursor, key, value, 0, 0 );
			  key = key( 2 );
			  value = value( 2 );
			  _node.insertKeyValueAt( Cursor, key, value, 1, 1 );
			  key = key( 3 );
			  value = value( 3 );
			  _node.insertKeyValueAt( Cursor, key, value, 2, 2 );

			  // AND
			  _node.removeKeyValueAt( Cursor, 1, 3 );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // WHEN
			  _node.defragmentLeaf( Cursor );

			  // THEN
			  AssertKeyEquals( key( 1 ), _node.keyAt( Cursor, _layout.newKey(), 0, LEAF ) );
			  AssertKeyEquals( key( 3 ), _node.keyAt( Cursor, _layout.newKey(), 1, LEAF ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefragLeafWithMultipleTombstonesInterleavedOdd()
		 internal virtual void ShouldDefragLeafWithMultipleTombstonesInterleavedOdd()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY key = key( 1 );
			  VALUE value = value( 1 );
			  _node.insertKeyValueAt( Cursor, key, value, 0, 0 );
			  key = key( 2 );
			  value = value( 2 );
			  _node.insertKeyValueAt( Cursor, key, value, 1, 1 );
			  key = key( 3 );
			  value = value( 3 );
			  _node.insertKeyValueAt( Cursor, key, value, 2, 2 );
			  key = key( 4 );
			  value = value( 4 );
			  _node.insertKeyValueAt( Cursor, key, value, 3, 3 );
			  key = key( 5 );
			  value = value( 5 );
			  _node.insertKeyValueAt( Cursor, key, value, 4, 4 );

			  // AND
			  _node.removeKeyValueAt( Cursor, 1, 5 );
			  _node.removeKeyValueAt( Cursor, 2, 4 );
			  TreeNode.SetKeyCount( Cursor, 3 );

			  // WHEN
			  _node.defragmentLeaf( Cursor );

			  // THEN
			  AssertKeyEquals( key( 1 ), _node.keyAt( Cursor, _layout.newKey(), 0, LEAF ) );
			  AssertKeyEquals( key( 3 ), _node.keyAt( Cursor, _layout.newKey(), 1, LEAF ) );
			  AssertKeyEquals( key( 5 ), _node.keyAt( Cursor, _layout.newKey(), 2, LEAF ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefragLeafWithMultipleTombstonesInterleavedEven()
		 internal virtual void ShouldDefragLeafWithMultipleTombstonesInterleavedEven()
		 {
			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  KEY key = key( 1 );
			  VALUE value = value( 1 );
			  _node.insertKeyValueAt( Cursor, key, value, 0, 0 );
			  key = key( 2 );
			  value = value( 2 );
			  _node.insertKeyValueAt( Cursor, key, value, 1, 1 );
			  key = key( 3 );
			  value = value( 3 );
			  _node.insertKeyValueAt( Cursor, key, value, 2, 2 );
			  key = key( 4 );
			  value = value( 4 );
			  _node.insertKeyValueAt( Cursor, key, value, 3, 3 );
			  key = key( 5 );
			  value = value( 5 );
			  _node.insertKeyValueAt( Cursor, key, value, 4, 4 );

			  // AND
			  _node.removeKeyValueAt( Cursor, 0, 5 );
			  _node.removeKeyValueAt( Cursor, 1, 4 );
			  _node.removeKeyValueAt( Cursor, 2, 3 );
			  TreeNode.SetKeyCount( Cursor, 2 );

			  // WHEN
			  _node.defragmentLeaf( Cursor );

			  // THEN
			  AssertKeyEquals( key( 2 ), _node.keyAt( Cursor, _layout.newKey(), 0, LEAF ) );
			  AssertKeyEquals( key( 4 ), _node.keyAt( Cursor, _layout.newKey(), 1, LEAF ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInsertAndRemoveRandomKeysAndValues()
		 internal virtual void ShouldInsertAndRemoveRandomKeysAndValues()
		 {
			  // This test doesn't care about sorting, that's an aspect that lies outside of TreeNode, really

			  // GIVEN
			  _node.initializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );
			  // add +1 to these to simplify some array logic in the test itself
			  IList<KEY> expectedKeys = new List<KEY>();
			  IList<VALUE> expectedValues = new List<VALUE>();
			  int expectedKeyCount = 0;
			  KEY readKey = _layout.newKey();
			  VALUE readValue = _layout.newValue();

			  // WHEN/THEN
			  for ( int i = 0; i < 1000; i++ )
			  {
					if ( _random.nextFloat() < 0.7 )
					{ // 70% insert
						 KEY newKey;
						 do
						 {
							  newKey = Key( _random.nextLong() );
						 } while ( contains( expectedKeys, newKey, _layout ) );
						 VALUE newValue = Value( _random.nextLong() );

						 Overflow overflow = _node.leafOverflow( Cursor, expectedKeyCount, newKey, newValue );
						 if ( overflow == NO_NEED_DEFRAG )
						 {
							  _node.defragmentLeaf( Cursor );
						 }
						 if ( overflow != YES )
						 { // there's room
							  int position = expectedKeyCount == 0 ? 0 : _random.Next( expectedKeyCount );
							  // ensure unique
							  _node.insertKeyValueAt( Cursor, newKey, newValue, position, expectedKeyCount );
							  expectedKeys.Insert( position, newKey );
							  expectedValues.Insert( position, newValue );

							  TreeNode.SetKeyCount( Cursor, ++expectedKeyCount );
						 }
					}
					else
					{ // 30% remove
						 if ( expectedKeyCount > 0 )
						 { // there are things to remove
							  int position = _random.Next( expectedKeyCount );
							  _node.keyAt( Cursor, readKey, position, LEAF );
							  _node.valueAt( Cursor, readValue, position );
							  _node.removeKeyValueAt( Cursor, position, expectedKeyCount );
							  KEY expectedKey = expectedKeys.RemoveAt( position );
							  VALUE expectedValue = expectedValues.RemoveAt( position );
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: assertEquals(0, layout.compare(expectedKey, readKey), String.format("Key differ with expected%n    readKey=%s %nexpectedKey=%s%n", readKey, expectedKey));
							  assertEquals( 0, _layout.Compare( expectedKey, readKey ), string.Format( "Key differ with expected%n    readKey=%s %nexpectedKey=%s%n", readKey, expectedKey ) );
							  assertEquals( 0, _layout.compareValue( expectedValue, readValue ), "Value differ with expected, value=" + readValue + ", expectedValue=" + expectedValue );

							  TreeNode.SetKeyCount( Cursor, --expectedKeyCount );
						 }
					}
			  }

			  // THEN
			  AssertContent( expectedKeys, expectedValues, expectedKeyCount );
		 }

		 private void AssertContent( IList<KEY> expectedKeys, IList<VALUE> expectedValues, int expectedKeyCount )
		 {
			  KEY actualKey = _layout.newKey();
			  VALUE actualValue = _layout.newValue();
			  assertEquals( expectedKeyCount, TreeNode.KeyCount( Cursor ) );
			  for ( int i = 0; i < expectedKeyCount; i++ )
			  {
					KEY expectedKey = expectedKeys[i];
					_node.keyAt( Cursor, actualKey, i, LEAF );
					assertEquals( 0, _layout.Compare( expectedKey, actualKey ), "Key differ with expected, actualKey=" + actualKey + ", expectedKey=" + expectedKey );

					VALUE expectedValue = expectedValues[i];
					_node.valueAt( Cursor, actualValue, i );
					assertEquals( 0, _layout.compareValue( expectedValue, actualValue ), "Value differ with expected, actualValue=" + actualValue + ", expectedValue=" + expectedValue );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAssertPageSizeBigEnoughForAtLeastTwoKeys()
		 internal virtual void ShouldAssertPageSizeBigEnoughForAtLeastTwoKeys()
		 {
			  assertThrows( typeof( MetadataMismatchException ), () => new TreeNodeFixedSize<>(TreeNode.BaseHeaderLength + _layout.keySize(default(KEY)) + _layout.valueSize(default(VALUE)), _layout) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromAbsoluteOffsetSlotA()
		 internal virtual void ShouldReadPointerGenerationFromAbsoluteOffsetSlotA()
		 {
			  // GIVEN
			  long generation = UNSTABLE_GENERATION;
			  long pointer = 12;
			  TreeNode.SetRightSibling( Cursor, pointer, STABLE_GENERATION, generation );

			  // WHEN
			  long readResult = TreeNode.RightSibling( Cursor, STABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertTrue( resultIsFromSlotA( readResult ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromAbsoluteOffsetSlotB()
		 internal virtual void ShouldReadPointerGenerationFromAbsoluteOffsetSlotB()
		 {
			  // GIVEN
			  long generation = HIGH_GENERATION;
			  long oldPointer = 12;
			  long pointer = 123;
			  TreeNode.SetRightSibling( Cursor, oldPointer, STABLE_GENERATION, UNSTABLE_GENERATION );
			  TreeNode.SetRightSibling( Cursor, pointer, UNSTABLE_GENERATION, generation );

			  // WHEN
			  long readResult = TreeNode.RightSibling( Cursor, UNSTABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertFalse( resultIsFromSlotA( readResult ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromLogicalPosSlotA()
		 internal virtual void ShouldReadPointerGenerationFromLogicalPosSlotA()
		 {
			  // GIVEN
			  long generation = UNSTABLE_GENERATION;
			  long pointer = 12;
			  int childPos = 2;
			  _node.setChildAt( Cursor, pointer, childPos, STABLE_GENERATION, generation );

			  // WHEN
			  long readResult = _node.childAt( Cursor, childPos, STABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertTrue( resultIsFromSlotA( readResult ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromLogicalPosZeroSlotA()
		 internal virtual void ShouldReadPointerGenerationFromLogicalPosZeroSlotA()
		 {
			  // GIVEN
			  long generation = UNSTABLE_GENERATION;
			  long pointer = 12;
			  int childPos = 0;
			  _node.setChildAt( Cursor, pointer, childPos, STABLE_GENERATION, generation );

			  // WHEN
			  long readResult = _node.childAt( Cursor, childPos, STABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertTrue( resultIsFromSlotA( readResult ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromLogicalPosZeroSlotB()
		 internal virtual void ShouldReadPointerGenerationFromLogicalPosZeroSlotB()
		 {
			  // GIVEN
			  long generation = HIGH_GENERATION;
			  long oldPointer = 13;
			  long pointer = 12;
			  int childPos = 0;
			  _node.setChildAt( Cursor, oldPointer, childPos, STABLE_GENERATION, UNSTABLE_GENERATION );
			  _node.setChildAt( Cursor, pointer, childPos, UNSTABLE_GENERATION, generation );

			  // WHEN
			  long readResult = _node.childAt( Cursor, childPos, UNSTABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertFalse( resultIsFromSlotA( readResult ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadPointerGenerationFromLogicalPosSlotB()
		 internal virtual void ShouldReadPointerGenerationFromLogicalPosSlotB()
		 {
			  // GIVEN
			  long generation = HIGH_GENERATION;
			  long oldPointer = 12;
			  long pointer = 123;
			  int childPos = 2;
			  _node.setChildAt( Cursor, oldPointer, childPos, STABLE_GENERATION, UNSTABLE_GENERATION );
			  _node.setChildAt( Cursor, pointer, childPos, UNSTABLE_GENERATION, generation );

			  // WHEN
			  long readResult = _node.childAt( Cursor, childPos, UNSTABLE_GENERATION, generation, _generationTarget );
			  long readGeneration = _generationTarget.generation;

			  // THEN
			  assertEquals( pointer, pointer( readResult ) );
			  assertEquals( generation, readGeneration );
			  assertFalse( resultIsFromSlotA( readResult ) );
		 }

		 private void AssertKeyEquals( KEY expectedKey, KEY actualKey )
		 {
			  assertEquals( 0, _layout.Compare( expectedKey, actualKey ), string.Format( "expectedKey={0}, actualKey={1}", expectedKey, actualKey ) );
		 }

		 private void AssertValueEquals( VALUE expectedValue, VALUE actualValue )
		 {
			  assertEquals( 0, _layout.compareValue( expectedValue, actualValue ), string.Format( "expectedValue={0}, actualKey={1}", expectedValue, actualValue ) );
		 }

		 private void AssertKeysAndChildren( long stable, long unstable, params long[] keysAndChildren )
		 {
			  KEY actualKey = _layout.newKey();
			  int pos;
			  for ( int i = 0; i < keysAndChildren.Length; i++ )
			  {
					pos = i / 2;
					if ( i % 2 == 0 )
					{
						 assertEquals( keysAndChildren[i], GenerationSafePointerPair.Pointer( _node.childAt( Cursor, pos, stable, unstable ) ) );
					}
					else
					{
						 KEY expectedKey = Key( keysAndChildren[i] );
						 _node.keyAt( Cursor, actualKey, pos, INTERNAL );
						 assertEquals( 0, _layout.Compare( expectedKey, actualKey ) );
					}
			  }
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
	}

}
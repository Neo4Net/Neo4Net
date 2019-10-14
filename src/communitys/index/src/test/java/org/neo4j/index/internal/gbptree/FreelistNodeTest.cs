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
namespace Neo4Net.Index.@internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;

	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class FreelistNodeTest
	{
		private bool InstanceFieldsInitialized = false;

		public FreelistNodeTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_maxEntries = _freelist.maxEntries();
		}

		 private const int PAGE_SIZE = 128;

		 private readonly PageCursor _cursor = ByteArrayPageCursor.wrap( PAGE_SIZE );
		 private readonly FreelistNode _freelist = new FreelistNode( PAGE_SIZE );
		 private int _maxEntries;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInitializeTreeNode()
		 internal virtual void ShouldInitializeTreeNode()
		 {
			  // GIVEN
			  FreelistNode.Initialize( _cursor );

			  // WHEN
			  sbyte nodeType = TreeNode.NodeType( _cursor );

			  // THEN
			  assertEquals( TreeNode.NODE_TYPE_FREE_LIST_NODE, nodeType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNodeOverwriteNodeType()
		 internal virtual void ShouldNodeOverwriteNodeType()
		 {
			  // GIVEN
			  FreelistNode.Initialize( _cursor );
			  sbyte nodeType = TreeNode.NodeType( _cursor );
			  assertEquals( TreeNode.NODE_TYPE_FREE_LIST_NODE, nodeType );

			  // WHEN
			  long someId = 1234;
			  FreelistNode.SetNext( _cursor, someId );

			  // THEN
			  nodeType = TreeNode.NodeType( _cursor );
			  assertEquals( TreeNode.NODE_TYPE_FREE_LIST_NODE, nodeType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetAndGetNext()
		 internal virtual void ShouldSetAndGetNext()
		 {
			  // GIVEN
			  long nextId = 12345;

			  // WHEN
			  FreelistNode.SetNext( _cursor, nextId );
			  long readNextId = FreelistNode.Next( _cursor );

			  // THEN
			  assertEquals( nextId, readNextId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadAndWriteFreeListEntries()
		 internal virtual void ShouldReadAndWriteFreeListEntries()
		 {
			  // GIVEN
			  long generationA = 34;
			  long pointerA = 56;
			  long generationB = 78;
			  long pointerB = 90;

			  // WHEN
			  _freelist.write( _cursor, generationA, pointerA, 0 );
			  _freelist.write( _cursor, generationB, pointerB, 1 );
			  long readPointerA = _freelist.read( _cursor, generationA + 1, 0 );
			  long readPointerB = _freelist.read( _cursor, generationB + 1, 1 );

			  // THEN
			  assertEquals( pointerA, readPointerA );
			  assertEquals( pointerB, readPointerB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnWritingBeyondMaxEntries()
		 internal virtual void ShouldFailOnWritingBeyondMaxEntries()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _freelist.write(_cursor, 1, 10, _maxEntries) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnWritingTooBigPointer()
		 internal virtual void ShouldFailOnWritingTooBigPointer()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _freelist.write(_cursor, 1, PageCursorUtil._6_B_MASK + 1, 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnWritingTooBigGeneration()
		 internal virtual void ShouldFailOnWritingTooBigGeneration()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _freelist.write(_cursor, GenerationSafePointer.MAX_GENERATION + 1, 1, 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnNoPageOnUnstableEntry()
		 internal virtual void ShouldReturnNoPageOnUnstableEntry()
		 {
			  // GIVEN
			  long stableGeneration = 10;
			  long unstableGeneration = stableGeneration + 1;
			  long pageId = 20;
			  int pos = 2;
			  _freelist.write( _cursor, unstableGeneration, pageId, pos );

			  // WHEN
			  long read = _freelist.read( _cursor, stableGeneration, pos );

			  // THEN
			  assertEquals( FreelistNode.NoPageId, read );
		 }
	}

}
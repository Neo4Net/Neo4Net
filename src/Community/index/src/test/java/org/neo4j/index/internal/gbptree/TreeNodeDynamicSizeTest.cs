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
namespace Neo4Net.Index.@internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	public class TreeNodeDynamicSizeTest : TreeNodeTestBase<RawBytes, RawBytes>
	{
		 private SimpleByteArrayLayout _layout = new SimpleByteArrayLayout();

		 protected internal override TestLayout<RawBytes, RawBytes> Layout
		 {
			 get
			 {
				  return _layout;
			 }
		 }

		 protected internal override TreeNodeDynamicSize<RawBytes, RawBytes> GetNode( int pageSize, Layout<RawBytes, RawBytes> layout )
		 {
			  return new TreeNodeDynamicSize<RawBytes, RawBytes>( pageSize, layout );
		 }

		 internal override void AssertAdditionalHeader( PageCursor cursor, TreeNode<RawBytes, RawBytes> node, int pageSize )
		 {
			  // When
			  int currentAllocSpace = ( ( TreeNodeDynamicSize ) node ).getAllocOffset( cursor );

			  // Then
			  assertEquals( pageSize, currentAllocSpace, "allocSpace point to end of page" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCompactKeyValueSizeHeader()
		 internal virtual void MustCompactKeyValueSizeHeader()
		 {
			  int oneByteKeyMax = DynamicSizeUtil.MASK_ONE_BYTE_KEY_SIZE;
			  int oneByteValueMax = DynamicSizeUtil.MASK_ONE_BYTE_VALUE_SIZE;

			  TreeNodeDynamicSize<RawBytes, RawBytes> node = GetNode( PAGE_SIZE, _layout );

			  VerifyOverhead( node, oneByteKeyMax, 0, 1 );
			  VerifyOverhead( node, oneByteKeyMax, 1, 2 );
			  VerifyOverhead( node, oneByteKeyMax, oneByteValueMax, 2 );
			  VerifyOverhead( node, oneByteKeyMax, oneByteValueMax + 1, 3 );
			  VerifyOverhead( node, oneByteKeyMax + 1, 0, 2 );
			  VerifyOverhead( node, oneByteKeyMax + 1, 1, 3 );
			  VerifyOverhead( node, oneByteKeyMax + 1, oneByteValueMax, 3 );
			  VerifyOverhead( node, oneByteKeyMax + 1, oneByteValueMax + 1, 4 );
		 }

		 private void VerifyOverhead( TreeNodeDynamicSize<RawBytes, RawBytes> node, int keySize, int valueSize, int expectedOverhead )
		 {
			  Cursor.zapPage();
			  node.InitializeLeaf( Cursor, STABLE_GENERATION, UNSTABLE_GENERATION );

			  RawBytes key = _layout.newKey();
			  RawBytes value = _layout.newValue();
			  key.Bytes = new sbyte[keySize];
			  value.Bytes = new sbyte[valueSize];

			  int allocOffsetBefore = node.GetAllocOffset( Cursor );
			  node.InsertKeyValueAt( Cursor, key, value, 0, 0 );
			  int allocOffsetAfter = node.GetAllocOffset( Cursor );
			  assertEquals( allocOffsetBefore - keySize - valueSize - expectedOverhead, allocOffsetAfter );
		 }
	}

}
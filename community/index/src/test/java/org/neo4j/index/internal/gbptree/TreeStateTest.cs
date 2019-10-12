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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TreeStateTest
	{
		 private readonly int _pageSize = 256;
		 private PageAwareByteArrayCursor _cursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void initiateCursor()
		 internal virtual void InitiateCursor()
		 {
			  _cursor = new PageAwareByteArrayCursor( _pageSize );
			  _cursor.next();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readEmptyStateShouldThrow()
		 internal virtual void ReadEmptyStateShouldThrow()
		 {
			  // GIVEN empty state

			  // WHEN
			  TreeState state = TreeState.Read( _cursor );

			  // THEN
			  assertFalse( state.Valid );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadValidPage()
		 internal virtual void ShouldReadValidPage()
		 {
			  // GIVEN valid state
			  long pageId = _cursor.CurrentPageId;
			  TreeState expected = new TreeState( pageId, 1, 2, 3, 4, 5, 6, 7, 8, 9, true, true );
			  Write( _cursor, expected );
			  _cursor.rewind();

			  // WHEN
			  TreeState read = TreeState.Read( _cursor );

			  // THEN
			  assertEquals( expected, read );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readBrokenStateShouldFail()
		 internal virtual void ReadBrokenStateShouldFail()
		 {
			  // GIVEN broken state
			  long pageId = _cursor.CurrentPageId;
			  TreeState expected = new TreeState( pageId, 1, 2, 3, 4, 5, 6, 7, 8, 9, true, true );
			  Write( _cursor, expected );
			  _cursor.rewind();
			  assertTrue( TreeState.Read( _cursor ).Valid );
			  _cursor.rewind();
			  BreakChecksum( _cursor );

			  // WHEN
			  TreeState state = TreeState.Read( _cursor );

			  // THEN
			  assertFalse( state.Valid );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotWriteInvalidStableGeneration()
		 internal virtual void ShouldNotWriteInvalidStableGeneration()
		 {
			  long generation = GenerationSafePointer.MAX_GENERATION + 1;

			  assertThrows(typeof(System.ArgumentException), () =>
			  {
				long pageId = _cursor.CurrentPageId;
				Write( _cursor, new TreeState( pageId, generation, 2, 3, 4, 5, 6, 7, 8, 9, true, true ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotWriteInvalidUnstableGeneration()
		 internal virtual void ShouldNotWriteInvalidUnstableGeneration()
		 {
			  long generation = GenerationSafePointer.MAX_GENERATION + 1;

			  assertThrows(typeof(System.ArgumentException), () =>
			  {
				long pageId = _cursor.CurrentPageId;
				Write( _cursor, new TreeState( pageId, 1, generation, 3, 4, 5, 6, 7, 8, 9, true, true ) );
			  });
		 }

		 private void BreakChecksum( PageCursor cursor )
		 {
			  // Doesn't matter which bits we destroy actually. Destroying the first ones requires
			  // no additional knowledge about where checksum is stored
			  long existing = cursor.GetLong( cursor.Offset );
			  cursor.PutLong( cursor.Offset, ~existing );
		 }

		 private void Write( PageCursor cursor, TreeState origin )
		 {
			  TreeState.Write( cursor, origin.StableGeneration(), origin.UnstableGeneration(), origin.RootId(), origin.RootGeneration(), origin.LastId(), origin.FreeListWritePageId(), origin.FreeListReadPageId(), origin.FreeListWritePos(), origin.FreeListReadPos(), origin.Clean );
		 }
	}

}
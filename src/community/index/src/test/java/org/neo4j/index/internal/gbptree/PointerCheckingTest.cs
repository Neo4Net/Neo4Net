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
	using Test = org.junit.jupiter.api.Test;

	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GBPTreeGenerationTarget_Fields.NO_GENERATION_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.read;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.write;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.PageCursorUtil.put6BLong;

	internal class PointerCheckingTest
	{
		 private readonly PageCursor _cursor = ByteArrayPageCursor.wrap( GenerationSafePointerPair.Size );
		 private readonly long _firstGeneration = 1;
		 private readonly long _secondGeneration = 2;
		 private readonly long _thirdGeneration = 3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkChildShouldThrowOnNoNode()
		 internal virtual void CheckChildShouldThrowOnNoNode()
		 {
			  assertThrows( typeof( TreeInconsistencyException ), () => PointerChecking.checkPointer(TreeNode.NO_NODE_FLAG, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkChildShouldThrowOnReadFailure()
		 internal virtual void CheckChildShouldThrowOnReadFailure()
		 {
			  long result = GenerationSafePointerPair.Read( _cursor, 0, 1, NO_GENERATION_TARGET );

			  assertThrows( typeof( TreeInconsistencyException ), () => PointerChecking.checkPointer(result, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkChildShouldThrowOnWriteFailure()
		 internal virtual void CheckChildShouldThrowOnWriteFailure()
		 {
			  // GIVEN
			  write( _cursor, 123, 0, _firstGeneration );
			  _cursor.rewind();
			  write( _cursor, 456, _firstGeneration, _secondGeneration );
			  _cursor.rewind();

			  // WHEN
			  // This write will see first and second written pointers and think they belong to CRASHed generation
			  long result = write( _cursor, 789, 0, _thirdGeneration );
			  assertThrows( typeof( TreeInconsistencyException ), () => PointerChecking.checkPointer(result, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkChildShouldPassOnReadSuccess()
		 internal virtual void CheckChildShouldPassOnReadSuccess()
		 {
			  // GIVEN
			  PointerChecking.CheckPointer( write( _cursor, 123, 0, _firstGeneration ), false );
			  _cursor.rewind();

			  // WHEN
			  long result = read( _cursor, 0, _firstGeneration, NO_GENERATION_TARGET );

			  // THEN
			  PointerChecking.CheckPointer( result, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkChildShouldPassOnWriteSuccess()
		 internal virtual void CheckChildShouldPassOnWriteSuccess()
		 {
			  // WHEN
			  long result = write( _cursor, 123, 0, _firstGeneration );

			  // THEN
			  PointerChecking.CheckPointer( result, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkSiblingShouldPassOnReadSuccessForNoNodePointer()
		 internal virtual void CheckSiblingShouldPassOnReadSuccessForNoNodePointer()
		 {
			  // GIVEN
			  write( _cursor, TreeNode.NO_NODE_FLAG, _firstGeneration, _secondGeneration );
			  _cursor.rewind();

			  // WHEN
			  long result = read( _cursor, _firstGeneration, _secondGeneration, NO_GENERATION_TARGET );

			  // THEN
			  PointerChecking.CheckPointer( result, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkSiblingShouldPassOnReadSuccessForNodePointer()
		 internal virtual void CheckSiblingShouldPassOnReadSuccessForNodePointer()
		 {
			  // GIVEN
			  long pointer = 101;
			  write( _cursor, pointer, _firstGeneration, _secondGeneration );
			  _cursor.rewind();

			  // WHEN
			  long result = read( _cursor, _firstGeneration, _secondGeneration, NO_GENERATION_TARGET );

			  // THEN
			  PointerChecking.CheckPointer( result, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkSiblingShouldThrowOnReadFailure()
		 internal virtual void CheckSiblingShouldThrowOnReadFailure()
		 {
			  long result = read( _cursor, _firstGeneration, _secondGeneration, NO_GENERATION_TARGET );

			  assertThrows( typeof( TreeInconsistencyException ), () => PointerChecking.checkPointer(result, true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkSiblingShouldThrowOnReadIllegalPointer()
		 internal virtual void CheckSiblingShouldThrowOnReadIllegalPointer()
		 {
			  // GIVEN
			  long generation = IdSpace.STATE_PAGE_A;
			  long pointer = this._secondGeneration;

			  // Can not use GenerationSafePointer.write because it will fail on pointer assertion.
			  _cursor.putInt( ( int ) pointer );
			  put6BLong( _cursor, generation );
			  _cursor.putShort( GenerationSafePointer.ChecksumOf( generation, pointer ) );
			  _cursor.rewind();

			  // WHEN
			  long result = read( _cursor, _firstGeneration, pointer, NO_GENERATION_TARGET );

			  assertThrows( typeof( TreeInconsistencyException ), () => PointerChecking.checkPointer(result, true) );
		 }
	}

}
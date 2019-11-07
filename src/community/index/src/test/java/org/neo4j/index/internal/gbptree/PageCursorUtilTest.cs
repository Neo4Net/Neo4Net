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
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.PageCursorUtil._6B_MASK;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class PageCursorUtilTest
	internal class PageCursorUtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGet6BLongs()
		 internal virtual void ShouldPutAndGet6BLongs()
		 {
			  // GIVEN
			  PageCursor cursor = ByteArrayPageCursor.wrap( 10 );

			  // WHEN
			  for ( int i = 0; i < 1_000; i++ )
			  {
					long expected = _random.nextLong() & _6B_MASK;
					cursor.Offset = 0;
					PageCursorUtil.Put6BLong( cursor, expected );
					cursor.Offset = 0;
					long read = PageCursorUtil.Get6BLong( cursor );

					// THEN
					assertEquals( expected, read );
					assertTrue( read >= 0 );
					assertEquals( 0, read & ~_6B_MASK );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnInvalidValues()
		 internal virtual void ShouldFailOnInvalidValues()
		 {
			  // GIVEN
			  PageCursor cursor = ByteArrayPageCursor.wrap( 10 );

			  // WHEN
			  for ( int i = 0; i < 1_000; )
			  {
					long expected = _random.nextLong();
					if ( ( expected & ~_6B_MASK ) != 0 )
					{
						 // OK here we have an invalid value
						 cursor.Offset = 0;
						 assertThrows( typeof( System.ArgumentException ), () => PageCursorUtil.put6BLong(cursor, expected) );
						 i++;
					}
			  }
		 }
	}

}
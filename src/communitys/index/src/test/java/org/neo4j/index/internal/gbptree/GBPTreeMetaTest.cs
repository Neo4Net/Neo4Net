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
	using Test = org.junit.Test;

	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class GBPTreeMetaTest
	{
		 private const int _pageSize = Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE;
		 private readonly PageCursor _cursor = ByteArrayPageCursor.wrap( new sbyte[_pageSize] );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReadWhatIsWritten() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReadWhatIsWritten()
		 {
			  // given
			  Layout layout = SimpleLongLayout.LongLayout().withIdentifier(666).withMajorVersion(10).withMinorVersion(100).build();
			  Meta written = new Meta( ( sbyte ) 1, ( sbyte ) 2, _pageSize, layout );
			  int offset = _cursor.Offset;
			  written.Write( _cursor, layout );

			  // when
			  _cursor.Offset = offset;
			  Meta read = Meta.Read( _cursor, layout );

			  // then
			  assertEquals( written.FormatIdentifier, read.FormatIdentifier );
			  assertEquals( written.FormatVersion, read.FormatVersion );
			  assertEquals( written.UnusedVersionSlot3, read.UnusedVersionSlot3 );
			  assertEquals( written.UnusedVersionSlot4, read.UnusedVersionSlot4 );
			  assertEquals( written.LayoutIdentifier, read.LayoutIdentifier );
			  assertEquals( written.LayoutMajorVersion, read.LayoutMajorVersion );
			  assertEquals( written.LayoutMinorVersion, read.LayoutMinorVersion );
			  assertEquals( written.PageSize, read.PageSize );
		 }
	}

}
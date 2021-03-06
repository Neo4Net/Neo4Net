﻿/*
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
namespace Org.Neo4j.Kernel.impl.store.format
{
	using Test = org.junit.Test;

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using StubPageCursor = Org.Neo4j.Io.pagecache.StubPageCursor;
	using DynamicRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.DynamicRecordFormat;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class BaseRecordFormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecognizeDesignatedInUseBit()
		 public virtual void ShouldRecognizeDesignatedInUseBit()
		 {
			  // GIVEN
			  RecordFormat<DynamicRecord> format = new DynamicRecordFormat();
			  PageCursor cursor = new StubPageCursor( 0, 1_000 );

			  sbyte inUseByte = 0;
			  for ( int i = 0; i < 8; i++ )
			  {
					// WHEN
					cursor.Offset = 68;
					cursor.PutByte( cursor.Offset, inUseByte );

					// THEN
					assertEquals( ShouldBeInUse( inUseByte ), format.IsInUse( cursor ) );
					inUseByte <<= 1;
					inUseByte |= 1;
			  }
		 }

		 private bool ShouldBeInUse( sbyte inUseByte )
		 {
			  return ( inUseByte & 0x10 ) != 0;
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.store.format.standard
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RelationshipGroupRecordFormatTest
	public class RelationshipGroupRecordFormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<org.neo4j.kernel.impl.store.format.RecordFormats> formats()
		 public static ICollection<RecordFormats> Formats()
		 {
			  return asList( StandardV2_3.RecordFormats, StandardV3_0.RecordFormats );
		 }

		 private readonly RecordFormat<RelationshipGroupRecord> _format;
		 private readonly int _recordSize;

		 public RelationshipGroupRecordFormatTest( RecordFormats formats )
		 {
			  this._format = formats.RelationshipGroup();
			  this._recordSize = _format.getRecordSize( NO_STORE_HEADER );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadUnsignedRelationshipTypeId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadUnsignedRelationshipTypeId()
		 {
			  // GIVEN
			  using ( PageCursor cursor = new StubPageCursor( 1, _recordSize * 10 ) )
			  {
					int offset = 10;
					cursor.Next();
					RelationshipGroupRecord group = ( new RelationshipGroupRecord( 2 ) ).initialize( true, short.MaxValue + offset, 1, 2, 3, 4, 5 );
					cursor.Offset = offset;
					_format.write( group, cursor, _recordSize );

					// WHEN
					RelationshipGroupRecord read = new RelationshipGroupRecord( group.Id );
					cursor.Offset = offset;
					_format.read( read, cursor, NORMAL, _recordSize );

					// THEN
					assertEquals( group, read );
			  }
		 }
	}

}
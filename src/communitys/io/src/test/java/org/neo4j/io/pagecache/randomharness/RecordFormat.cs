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
namespace Neo4Net.Io.pagecache.randomharness
{

	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;

	public abstract class RecordFormat
	{
		 public abstract int RecordSize { get; }

		 public abstract Record CreateRecord( File file, int recordId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract Record readRecord(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException;
		 public abstract Record ReadRecord( PageCursor cursor );

		 public abstract Record ZeroRecord();

		 public abstract void Write( Record record, PageCursor cursor );

		 public void WriteRecord( PageCursor cursor )
		 {
			  int recordsPerPage = cursor.CurrentPageSize / RecordSize;
			  WriteRecordToPage( cursor, cursor.CurrentPageId, recordsPerPage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeRecord(Record record, org.neo4j.io.fs.StoreChannel channel) throws java.io.IOException
		 public void WriteRecord( Record record, StoreChannel channel )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( RecordSize );
			  StubPageCursor cursor = new StubPageCursor( 0, buffer );
			  Write( record, cursor );
			  channel.WriteAll( buffer );
		 }

		 public void FillWithRecords( PageCursor cursor )
		 {
			  cursor.Offset = 0;
			  int recordsPerPage = cursor.CurrentPageSize / RecordSize;
			  for ( int i = 0; i < recordsPerPage; i++ )
			  {
					WriteRecordToPage( cursor, cursor.CurrentPageId, recordsPerPage );
			  }
		 }

		 private void WriteRecordToPage( PageCursor cursor, long pageId, int recordsPerPage )
		 {
			  int pageRecordId = cursor.Offset / RecordSize;
			  int recordId = ( int )( pageId * recordsPerPage + pageRecordId );
			  Record record = CreateRecord( cursor.CurrentFile, recordId );
			  Write( record, cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void assertRecordsWrittenCorrectly(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 public void AssertRecordsWrittenCorrectly( PageCursor cursor )
		 {
			  int currentPageSize = cursor.CurrentPageSize;
			  int recordSize = RecordSize;
			  int recordsPerPage = currentPageSize / recordSize;
			  for ( int pageRecordId = 0; pageRecordId < recordsPerPage; pageRecordId++ )
			  {
					long currentPageId = cursor.CurrentPageId;
					int recordId = ( int )( currentPageId * recordsPerPage + pageRecordId );
					Record expectedRecord = CreateRecord( cursor.CurrentFile, recordId );
					Record actualRecord;
					actualRecord = ReadRecord( cursor );
					assertThat( actualRecord, isOneOf( expectedRecord, ZeroRecord() ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void assertRecordsWrittenCorrectly(java.io.File file, org.neo4j.io.fs.StoreChannel channel) throws java.io.IOException
		 public void AssertRecordsWrittenCorrectly( File file, StoreChannel channel )
		 {
			  int recordSize = RecordSize;
			  long recordsInFile = channel.size() / recordSize;
			  ByteBuffer buffer = ByteBuffer.allocate( recordSize );
			  StubPageCursor cursor = new StubPageCursor( 0, buffer );
			  for ( int i = 0; i < recordsInFile; i++ )
			  {
					assertThat( "reading record id " + i, channel.read( buffer ), @is( recordSize ) );
					buffer.flip();
					Record expectedRecord = CreateRecord( file, i );
					cursor.Offset = 0;
					Record actualRecord = ReadRecord( cursor );
					buffer.clear();
					assertThat( actualRecord, isOneOf( expectedRecord, ZeroRecord() ) );
			  }
		 }
	}

}
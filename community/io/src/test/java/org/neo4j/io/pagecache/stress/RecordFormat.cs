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
namespace Org.Neo4j.Io.pagecache.stress
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;

	public class RecordFormat
	{
		 private readonly int _numberOfThreads;
		 private readonly int _cachePageSize;
		 private readonly int _fieldSize;
		 private readonly int _checksumFieldOffset;
		 private readonly int _recordSize;

		 public RecordFormat( int numberOfThreads, int cachePageSize )
		 {
			  this._numberOfThreads = numberOfThreads;
			  this._cachePageSize = cachePageSize;
			  this._fieldSize = Long.BYTES;
			  this._checksumFieldOffset = numberOfThreads * _fieldSize;
			  this._recordSize = _checksumFieldOffset + _fieldSize; // extra field for keeping the checksum.
		 }

		 public virtual int RecordSize
		 {
			 get
			 {
				  return _recordSize;
			 }
		 }

		 public virtual int RecordsPerPage
		 {
			 get
			 {
				  return _cachePageSize / RecordSize;
			 }
		 }

		 public virtual int FilePageSize
		 {
			 get
			 {
				  return RecordsPerPage * RecordSize;
			 }
		 }

		 /// <summary>
		 /// Assume the given cursor is writable and has already been positioned at the record offset.
		 /// </summary>
		 public virtual long IncrementCounter( PageCursor cursor, int threadId )
		 {
			  int recordOffset = cursor.Offset;
			  int fieldOffset = recordOffset + ( _fieldSize * threadId );
			  int checksumOffset = recordOffset + _checksumFieldOffset;

			  long newValue = 1 + cursor.GetLong( fieldOffset );
			  cursor.PutLong( fieldOffset, newValue );
			  cursor.PutLong( checksumOffset, 1 + cursor.GetLong( checksumOffset ) );
			  return newValue;
		 }

		 /// <summary>
		 /// Sum up the fields for the given thread for all records on the given page.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long sumCountsForThread(org.neo4j.io.pagecache.PageCursor cursor, int threadId) throws java.io.IOException
		 public virtual long SumCountsForThread( PageCursor cursor, int threadId )
		 {
			  int recordsPerPage = RecordsPerPage;
			  int fieldOffset = _fieldSize * threadId;
			  long sum;
			  do
			  {
					sum = 0;
					for ( int i = 0; i < recordsPerPage; i++ )
					{
						 sum += cursor.GetLong( ( i * _recordSize ) + fieldOffset );
					}
			  } while ( cursor.ShouldRetry() );
			  return sum;
		 }

		 /// <summary>
		 /// Verify the checksums on all the records on the given page
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyCheckSums(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 public virtual void VerifyCheckSums( PageCursor cursor )
		 {
			  int recordsPerPage = RecordsPerPage;
			  for ( int i = 0; i < recordsPerPage; i++ )
			  {
					int recordOffset = i * _recordSize;
					long expectedChecksum;
					long actualChecksum;
					do
					{
						 actualChecksum = 0;
						 for ( int j = 0; j < _numberOfThreads; j++ )
						 {
							  actualChecksum += cursor.GetLong( recordOffset + ( j * _fieldSize ) );
						 }
						 expectedChecksum = cursor.GetLong( recordOffset + _checksumFieldOffset );
					} while ( cursor.ShouldRetry() );
					string msg = "Checksum for record " + i + " on page " + cursor.CurrentPageId;
					assertThat( msg, actualChecksum, @is( expectedChecksum ) );
			  }
		 }
	}

}
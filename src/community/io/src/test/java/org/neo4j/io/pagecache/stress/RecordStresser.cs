using System;

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
namespace Neo4Net.Io.pagecache.stress
{


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;

	public class RecordStresser : Callable<Void>
	{
		 private readonly PagedFile _pagedFile;
		 private readonly Condition _condition;
		 private readonly int _maxRecords;
		 private readonly RecordFormat _format;
		 private readonly int _threadId;
		 private readonly TinyLockManager _locks;
		 private long _countSum;

		 public RecordStresser( PagedFile pagedFile, Condition condition, int maxRecords, RecordFormat format, int threadId, TinyLockManager locks )
		 {
			  this._pagedFile = pagedFile;
			  this._condition = condition;
			  this._maxRecords = maxRecords;
			  this._format = format;
			  this._threadId = threadId;
			  this._locks = locks;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws Exception
		 public override Void Call()
		 {
			  Random random = new Random();
			  int recordsPerPage = _format.RecordsPerPage;
			  int recordSize = _format.RecordSize;
			  using ( PageCursor cursor = _pagedFile.io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					while ( !_condition.fulfilled() )
					{
						 int recordId = random.Next( _maxRecords );
						 int pageId = recordId / recordsPerPage;
						 int recordOffset = ( recordId % recordsPerPage ) * recordSize;

						 _locks.@lock( recordId );
						 try
						 {
							  assertTrue( cursor.Next( pageId ), "I must be able to access pages" );
							  cursor.Offset = recordOffset;
							  long newValue = _format.incrementCounter( cursor, _threadId );
							  _countSum++;
							  assertFalse( cursor.ShouldRetry(), "Write lock, so never a need to retry" );
							  assertThat( "Record-local count must be less than or equal to thread-local count sum", newValue, lessThanOrEqualTo( _countSum ) );
						 }
						 finally
						 {
							  _locks.unlock( recordId );
						 }
					}
			  }

			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyCounts() throws java.io.IOException
		 public virtual void VerifyCounts()
		 {
			  long actualSum = 0;
			  using ( PageCursor cursor = _pagedFile.io( 0, PF_SHARED_READ_LOCK ) )
			  {
					while ( cursor.Next() )
					{
						 actualSum += _format.sumCountsForThread( cursor, _threadId );
					}
			  }
			  assertThat( "Thread specific sum across all records", actualSum, @is( _countSum ) );
		 }
	}

}
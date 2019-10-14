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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class EntryCountThresholdTest
	{
		 private LogFileInformation _info = mock( typeof( LogFileInformation ) );
		 private File _file = mock( typeof( File ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportThresholdReachedWhenThresholdIsReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportThresholdReachedWhenThresholdIsReached()
		 {
			  long version = 10L;

			  when( _info.getFirstEntryId( version + 1 ) ).thenReturn( 1L );
			  when( _info.LastEntryId ).thenReturn( 2L );

			  EntryCountThreshold threshold = new EntryCountThreshold( 1 );
			  bool reached = threshold.Reached( _file, version, _info );

			  assertTrue( reached );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportThresholdNotReachedWhenThresholdIsNotReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportThresholdNotReachedWhenThresholdIsNotReached()
		 {
			  long version = 10L;

			  when( _info.getFirstEntryId( version ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( version + 1 ) ).thenReturn( 1L );

			  when( _info.LastEntryId ).thenReturn( 1L );

			  EntryCountThreshold threshold = new EntryCountThreshold( 1 );

			  assertFalse( threshold.Reached( _file, version, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleCaseWithOneEntryPerLogFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyHandleCaseWithOneEntryPerLogFile()
		 {
			  // Given 2 files with one entry each
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 2L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 3L );

			  when( _info.LastEntryId ).thenReturn( 3L );

			  // When the threshold is 1 entries
			  EntryCountThreshold threshold = new EntryCountThreshold( 1 );

			  // Then the last file should be kept around
			  assertFalse( threshold.Reached( _file, 2L, _info ) );
			  assertTrue( threshold.Reached( _file, 1L, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhenCalledMultipleTimesKeeping2Files() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWhenCalledMultipleTimesKeeping2Files()
		 {
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 15L );
			  when( _info.getFirstEntryId( 4L ) ).thenReturn( 18L );
			  when( _info.LastEntryId ).thenReturn( 18L );

			  EntryCountThreshold threshold = new EntryCountThreshold( 8 );

			  assertTrue( threshold.Reached( _file, 1L, _info ) );

			  assertFalse( threshold.Reached( _file, 2L, _info ) );

			  assertFalse( threshold.Reached( _file, 3L, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhenCalledMultipleTimesKeeping3Files() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWhenCalledMultipleTimesKeeping3Files()
		 {
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 15L );
			  when( _info.getFirstEntryId( 4L ) ).thenReturn( 18L );
			  when( _info.LastEntryId ).thenReturn( 18L );

			  EntryCountThreshold threshold = new EntryCountThreshold( 15 );

			  assertFalse( threshold.Reached( _file, 1L, _info ) );

			  assertFalse( threshold.Reached( _file, 2L, _info ) );

			  assertFalse( threshold.Reached( _file, 3L, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhenCalledMultipleTimesKeeping1FileOnBoundary() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWhenCalledMultipleTimesKeeping1FileOnBoundary()
		 {
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 15L );
			  when( _info.getFirstEntryId( 4L ) ).thenReturn( 18L );
			  when( _info.LastEntryId ).thenReturn( 18L );

			  EntryCountThreshold threshold = new EntryCountThreshold( 3 );

			  assertTrue( threshold.Reached( _file, 1L, _info ) );
			  assertTrue( threshold.Reached( _file, 2L, _info ) );
			  assertFalse( threshold.Reached( _file, 3L, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipEmptyLogsBetweenLogsThatWillBeKept() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipEmptyLogsBetweenLogsThatWillBeKept()
		 {
			  // Given
			  // 1, 3 and 4 are empty. 2 has 5 transactions, 5 has 8, 6 is the current version
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 4L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 5L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 6L ) ).thenReturn( 13L );
			  when( _info.LastEntryId ).thenReturn( 13L );

			  // The threshold is 9, which is one more than what version 5 has, which means 2 should be kept
			  EntryCountThreshold threshold = new EntryCountThreshold( 9 );

			  assertFalse( threshold.Reached( _file, 5L, _info ) );
			  assertFalse( threshold.Reached( _file, 4L, _info ) );
			  assertFalse( threshold.Reached( _file, 3L, _info ) );
			  assertFalse( threshold.Reached( _file, 2L, _info ) );
			  assertTrue( threshold.Reached( _file, 1L, _info ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteNonEmptyLogThatIsAfterASeriesOfEmptyLogs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteNonEmptyLogThatIsAfterASeriesOfEmptyLogs()
		 {
			  // Given
			  // 1, 3 and 4 are empty. 2 has 5 transactions, 5 has 8, 6 is the current version
			  when( _info.getFirstEntryId( 1L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 2L ) ).thenReturn( 1L );
			  when( _info.getFirstEntryId( 3L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 4L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 5L ) ).thenReturn( 5L );
			  when( _info.getFirstEntryId( 6L ) ).thenReturn( 13L );
			  when( _info.LastEntryId ).thenReturn( 13L );

			  // The threshold is 8, which is exactly what version 5 has, which means 2 should be deleted
			  EntryCountThreshold threshold = new EntryCountThreshold( 8 );

			  assertFalse( threshold.Reached( _file, 5L, _info ) );
			  assertTrue( threshold.Reached( _file, 4L, _info ) );
			  assertTrue( threshold.Reached( _file, 3L, _info ) );
			  assertTrue( threshold.Reached( _file, 2L, _info ) );
			  assertTrue( threshold.Reached( _file, 1L, _info ) );
		 }
	}

}
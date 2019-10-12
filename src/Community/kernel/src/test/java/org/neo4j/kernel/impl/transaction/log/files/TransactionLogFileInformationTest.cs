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
namespace Neo4Net.Kernel.impl.transaction.log.files
{
	using Test = org.junit.Test;

	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransactionLogFileInformationTest
	{
		 private LogFiles _logFiles = mock( typeof( TransactionLogFiles ) );
		 private LogHeaderCache _logHeaderCache = mock( typeof( LogHeaderCache ) );
		 private TransactionLogFilesContext _context = mock( typeof( TransactionLogFilesContext ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadAndCacheFirstCommittedTransactionIdForAGivenVersionWhenNotCached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadAndCacheFirstCommittedTransactionIdForAGivenVersionWhenNotCached()
		 {
			  TransactionLogFileInformation info = new TransactionLogFileInformation( _logFiles, _logHeaderCache, _context );
			  long expected = 5;

			  long version = 10L;
			  when( _logHeaderCache.getLogHeader( version ) ).thenReturn( null );
			  when( _logFiles.versionExists( version ) ).thenReturn( true );
			  when( _logFiles.extractHeader( version ) ).thenReturn(new LogHeader((sbyte) -1, -1L, expected - 1L)
			 );

			  long firstCommittedTxId = info.GetFirstEntryId( version );
			  assertEquals( expected, firstCommittedTxId );
			  verify( _logHeaderCache, times( 1 ) ).putHeader( version, expected - 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFirstCommittedTransactionIdForAGivenVersionWhenCached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadFirstCommittedTransactionIdForAGivenVersionWhenCached()
		 {
			  TransactionLogFileInformation info = new TransactionLogFileInformation( _logFiles, _logHeaderCache, _context );
			  long expected = 5;

			  long version = 10L;
			  when( _logHeaderCache.getLogHeader( version ) ).thenReturn( expected - 1 );

			  long firstCommittedTxId = info.GetFirstEntryId( version );
			  assertEquals( expected, firstCommittedTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadAndCacheFirstCommittedTransactionIdWhenNotCached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadAndCacheFirstCommittedTransactionIdWhenNotCached()
		 {
			  TransactionLogFileInformation info = new TransactionLogFileInformation( _logFiles, _logHeaderCache, _context );
			  long expected = 5;

			  long version = 10L;
			  when( _logFiles.HighestLogVersion ).thenReturn( version );
			  when( _logHeaderCache.getLogHeader( version ) ).thenReturn( null );
			  when( _logFiles.versionExists( version ) ).thenReturn( true );
			  when( _logFiles.extractHeader( version ) ).thenReturn(new LogHeader((sbyte) -1, -1L, expected - 1L)
			 );
			  when( _logFiles.hasAnyEntries( version ) ).thenReturn( true );

			  long firstCommittedTxId = info.FirstExistingEntryId;
			  assertEquals( expected, firstCommittedTxId );
			  verify( _logHeaderCache, times( 1 ) ).putHeader( version, expected - 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFirstCommittedTransactionIdWhenCached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadFirstCommittedTransactionIdWhenCached()
		 {
			  TransactionLogFileInformation info = new TransactionLogFileInformation( _logFiles, _logHeaderCache, _context );
			  long expected = 5;

			  long version = 10L;
			  when( _logFiles.HighestLogVersion ).thenReturn( version );
			  when( _logFiles.versionExists( version ) ).thenReturn( true );
			  when( _logHeaderCache.getLogHeader( version ) ).thenReturn( expected - 1 );
			  when( _logFiles.hasAnyEntries( version ) ).thenReturn( true );

			  long firstCommittedTxId = info.FirstExistingEntryId;
			  assertEquals( expected, firstCommittedTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNothingWhenThereAreNoTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNothingWhenThereAreNoTransactions()
		 {
			  TransactionLogFileInformation info = new TransactionLogFileInformation( _logFiles, _logHeaderCache, _context );

			  long version = 10L;
			  when( _logFiles.HighestLogVersion ).thenReturn( version );
			  when( _logFiles.hasAnyEntries( version ) ).thenReturn( false );

			  long firstCommittedTxId = info.FirstExistingEntryId;
			  assertEquals( -1, firstCommittedTxId );
		 }
	}

}
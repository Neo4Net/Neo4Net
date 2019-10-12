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
namespace Org.Neo4j.Kernel.impl.transaction.log.stresstest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryByteCodes = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryByteCodes;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Runner = Org.Neo4j.Kernel.impl.transaction.log.stresstest.workload.Runner;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.untilTimeExpired;

	public class TransactionAppenderStressTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentTransactionAppendingTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentTransactionAppendingTest()
		 {
			  int threads = 10;
			  Callable<long> runner = ( new Builder() ).With(untilTimeExpired(10, SECONDS)).withWorkingDirectory(Directory.databaseLayout()).withNumThreads(threads).build();

			  long appendedTxs = runner.call();

			  assertEquals( ( new TransactionIdChecker( Directory.databaseLayout().databaseDirectory() ) ).parseAllTxLogs(), appendedTxs );
		 }

		 public class Builder
		 {
			  internal System.Func<bool> Condition;
			  internal DatabaseLayout DatabaseLayout;
			  internal int Threads;

			  public virtual Builder With( System.Func<bool> condition )
			  {
					this.Condition = condition;
					return this;
			  }

			  public virtual Builder WithWorkingDirectory( DatabaseLayout databaseLayout )
			  {
					this.DatabaseLayout = databaseLayout;
					return this;
			  }

			  public virtual Builder WithNumThreads( int threads )
			  {
					this.Threads = threads;
					return this;
			  }

			  public virtual Callable<long> Build()
			  {
					return new Runner( DatabaseLayout, Condition, Threads );
			  }
		 }

		 public class TransactionIdChecker
		 {
			  internal readonly File WorkingDirectory;

			  public TransactionIdChecker( File workingDirectory )
			  {
					this.WorkingDirectory = workingDirectory;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long parseAllTxLogs() throws java.io.IOException
			  public virtual long ParseAllTxLogs()
			  {
					// Initialize this txId to the BASE_TX_ID because if we don't find any tx log that means that
					// no transactions have been appended in this test and that getLastCommittedTransactionId()
					// will also return this constant. Why this is, is another question - but thread scheduling and
					// I/O spikes on some build machines can be all over the place and also the test duration is
					// configurable.
					long txId = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

					using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction(), ReadableLogChannel channel = OpenLogFile(fs, 0) )
					{
						 LogEntryReader<ReadableLogChannel> reader = new VersionAwareLogEntryReader<ReadableLogChannel>();
						 LogEntry logEntry = reader.ReadLogEntry( channel );
						 for ( ; logEntry != null; logEntry = reader.ReadLogEntry( channel ) )
						 {
							  if ( logEntry.Type == LogEntryByteCodes.TX_COMMIT )
							  {
									txId = logEntry.As<LogEntryCommit>().TxId;
							  }
						 }
					}
					return txId;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.ReadableLogChannel openLogFile(org.neo4j.io.fs.FileSystemAbstraction fs, int version) throws java.io.IOException
			  internal virtual ReadableLogChannel OpenLogFile( FileSystemAbstraction fs, int version )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( WorkingDirectory, fs ).build();
					PhysicalLogVersionedStoreChannel channel = logFiles.OpenForVersion( version );
					return new ReadAheadLogChannel( channel, new ReaderLogVersionBridge( logFiles ) );
			  }
		 }
	}

}
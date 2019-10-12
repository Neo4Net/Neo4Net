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
namespace Org.Neo4j.Test
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using PhysicalLogVersionedStoreChannel = Org.Neo4j.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReadAheadLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableLogChannel;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogHeader = Org.Neo4j.Kernel.impl.transaction.log.entry.LogHeader;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;

	/// <summary>
	/// Utility for reading and filtering logical logs as well as tx logs.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public class LogTestUtils
	{
		 private LogTestUtils()
		 {
		 }

		 public interface LogHook<RECORD> : System.Predicate<RECORD>
		 {
			  void File( File file );

			  void Done( File file );
		 }

		 public abstract class LogHookAdapter<RECORD> : LogHook<RECORD>
		 {
			  public override void File( File file )
			  { // Do nothing
			  }

			  public override void Done( File file )
			  { // Do nothing
			  }
		 }

		 public class CountingLogHook<RECORD> : LogHookAdapter<RECORD>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int CountConflict;

			  public override bool Test( RECORD item )
			  {
					CountConflict++;
					return true;
			  }

			  public virtual int Count
			  {
				  get
				  {
						return CountConflict;
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File[] filterNeostoreLogicalLog(org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles, org.neo4j.io.fs.FileSystemAbstraction fileSystem, LogHook<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> filter) throws java.io.IOException
		 public static File[] FilterNeostoreLogicalLog( LogFiles logFiles, FileSystemAbstraction fileSystem, LogHook<LogEntry> filter )
		 {
			  File[] files = logFiles.LogFilesConflict();
			  foreach ( File file in files )
			  {
					FilterTransactionLogFile( fileSystem, file, filter );
			  }

			  return files;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void filterTransactionLogFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, final LogHook<org.neo4j.kernel.impl.transaction.log.entry.LogEntry> filter) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal static void FilterTransactionLogFile( FileSystemAbstraction fileSystem, File file, LogHook<LogEntry> filter )
		 {
			  filter.File( file );
			  using ( StoreChannel @in = fileSystem.Open( file, OpenMode.READ ) )
			  {
					LogHeader logHeader = readLogHeader( ByteBuffer.allocate( LOG_HEADER_SIZE ), @in, true, file );
					PhysicalLogVersionedStoreChannel inChannel = new PhysicalLogVersionedStoreChannel( @in, logHeader.LogVersion, logHeader.LogFormatVersion );
					ReadableLogChannel inBuffer = new ReadAheadLogChannel( inChannel );
					LogEntryReader<ReadableLogChannel> entryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();

					LogEntry entry;
					while ( ( entry = entryReader.ReadLogEntry( inBuffer ) ) != null )
					{
						 filter.test( entry );
					}
			  }
		 }
	}

}
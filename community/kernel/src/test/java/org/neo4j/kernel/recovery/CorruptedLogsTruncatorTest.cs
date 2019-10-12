using System;

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
namespace Org.Neo4j.Kernel.recovery
{
	using FilenameUtils = org.apache.commons.io.FilenameUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ArrayUtil = Org.Neo4j.Helpers.ArrayUtil;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using SimpleLogVersionRepository = Org.Neo4j.Kernel.impl.transaction.SimpleLogVersionRepository;
	using SimpleTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.SimpleTransactionIdStore;
	using FlushablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogHeader = Org.Neo4j.Kernel.impl.transaction.log.entry.LogHeader;
	using LogFile = Org.Neo4j.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CorruptedLogsTruncatorTest
	{
		 private const int SINGLE_LOG_FILE_SIZE = 25;
		 private const int TOTAL_NUMBER_OF_LOG_FILES = 12;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly FileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule();
		 public readonly LifeRule Life = new LifeRule();
		 private File _databaseDirectory;
		 private LogFiles _logFiles;
		 private CorruptedLogsTruncator _logPruner;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _databaseDirectory = TestDirectory.databaseDir();
			  SimpleLogVersionRepository logVersionRepository = new SimpleLogVersionRepository();
			  SimpleTransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  _logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _databaseDirectory, FileSystemRule ).withRotationThreshold( LogHeader.LOG_HEADER_SIZE + 9L ).withLogVersionRepository( logVersionRepository ).withTransactionIdStore( transactionIdStore ).build();
			  Life.add( _logFiles );
			  _logPruner = new CorruptedLogsTruncator( _databaseDirectory, _logFiles, FileSystemRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotPruneEmptyLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotPruneEmptyLogs()
		 {
			  _logPruner.truncate( LogPosition.start( 0 ) );
			  assertTrue( FileUtils.isEmptyDirectory( _databaseDirectory ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotPruneNonCorruptedLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotPruneNonCorruptedLogs()
		 {
			  Life.start();
			  GenerateTransactionLogFiles( _logFiles );

			  long highestLogVersion = _logFiles.HighestLogVersion;
			  long fileSizeBeforePrune = _logFiles.HighestLogFile.length();
			  LogPosition endOfLogsPosition = new LogPosition( highestLogVersion, fileSizeBeforePrune );
			  assertEquals( TOTAL_NUMBER_OF_LOG_FILES - 1, highestLogVersion );

			  _logPruner.truncate( endOfLogsPosition );

			  assertEquals( TOTAL_NUMBER_OF_LOG_FILES, _logFiles.logFiles().Length );
			  assertEquals( fileSizeBeforePrune, _logFiles.HighestLogFile.length() );
			  assertTrue( ArrayUtil.isEmpty( _databaseDirectory.listFiles( File.isDirectory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneAndArchiveLastLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneAndArchiveLastLog()
		 {
			  Life.start();
			  GenerateTransactionLogFiles( _logFiles );

			  long highestLogVersion = _logFiles.HighestLogVersion;
			  File highestLogFile = _logFiles.HighestLogFile;
			  long fileSizeBeforePrune = highestLogFile.length();
			  int bytesToPrune = 5;
			  long byteOffset = fileSizeBeforePrune - bytesToPrune;
			  LogPosition prunePosition = new LogPosition( highestLogVersion, byteOffset );

			  _logPruner.truncate( prunePosition );

			  assertEquals( TOTAL_NUMBER_OF_LOG_FILES, _logFiles.logFiles().Length );
			  assertEquals( byteOffset, highestLogFile.length() );

			  File corruptedLogsDirectory = new File( _databaseDirectory, CorruptedLogsTruncator.CorruptedTxLogsBaseName );
			  assertTrue( corruptedLogsDirectory.exists() );
			  File[] files = corruptedLogsDirectory.listFiles();
			  assertEquals( 1, Files.Length );

			  File corruptedLogsArchive = files[0];
			  CheckArchiveName( highestLogVersion, byteOffset, corruptedLogsArchive );
			  using ( ZipFile zipFile = new ZipFile( corruptedLogsArchive ) )
			  {
					assertEquals( 1, zipFile.size() );
					CheckEntryNameAndSize( zipFile, highestLogFile.Name, bytesToPrune );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneAndArchiveMultipleLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneAndArchiveMultipleLogs()
		 {
			  Life.start();
			  GenerateTransactionLogFiles( _logFiles );

			  long highestCorrectLogFileIndex = 5;
			  File highestCorrectLogFile = _logFiles.getLogFileForVersion( highestCorrectLogFileIndex );
			  long fileSizeBeforePrune = highestCorrectLogFile.length();
			  int bytesToPrune = 7;
			  long byteOffset = fileSizeBeforePrune - bytesToPrune;
			  LogPosition prunePosition = new LogPosition( highestCorrectLogFileIndex, byteOffset );
			  Life.shutdown();

			  _logPruner.truncate( prunePosition );

			  Life.start();
			  assertEquals( 6, _logFiles.logFiles().Length );
			  assertEquals( byteOffset, highestCorrectLogFile.length() );

			  File corruptedLogsDirectory = new File( _databaseDirectory, CorruptedLogsTruncator.CorruptedTxLogsBaseName );
			  assertTrue( corruptedLogsDirectory.exists() );
			  File[] files = corruptedLogsDirectory.listFiles();
			  assertEquals( 1, Files.Length );

			  File corruptedLogsArchive = files[0];
			  CheckArchiveName( highestCorrectLogFileIndex, byteOffset, corruptedLogsArchive );
			  using ( ZipFile zipFile = new ZipFile( corruptedLogsArchive ) )
			  {
					assertEquals( 7, zipFile.size() );
					CheckEntryNameAndSize( zipFile, highestCorrectLogFile.Name, bytesToPrune );
					long nextLogFileIndex = highestCorrectLogFileIndex + 1;
					int lastFileIndex = TOTAL_NUMBER_OF_LOG_FILES - 1;
					for ( long index = nextLogFileIndex; index < lastFileIndex; index++ )
					{
						 CheckEntryNameAndSize( zipFile, TransactionLogFiles.DEFAULT_NAME + "." + index, SINGLE_LOG_FILE_SIZE );
					}
					CheckEntryNameAndSize( zipFile, TransactionLogFiles.DEFAULT_NAME + "." + lastFileIndex, SINGLE_LOG_FILE_SIZE - 1 );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkEntryNameAndSize(java.util.zip.ZipFile zipFile, String entryName, int expectedSize) throws java.io.IOException
		 private void CheckEntryNameAndSize( ZipFile zipFile, string entryName, int expectedSize )
		 {
			  ZipEntry entry = zipFile.getEntry( entryName );
			  Stream inputStream = zipFile.getInputStream( entry );
			  int entryBytes = 0;
			  while ( inputStream.Read() >= 0 )
			  {
					entryBytes++;
			  }
			  assertEquals( expectedSize, entryBytes );
		 }

		 private void CheckArchiveName( long highestLogVersion, long byteOffset, File corruptedLogsArchive )
		 {
			  string name = corruptedLogsArchive.Name;
			  assertTrue( name.StartsWith( "corrupted-neostore.transaction.db-" + highestLogVersion + "-" + byteOffset, StringComparison.Ordinal ) );
			  assertTrue( FilenameUtils.isExtension( name, "zip" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateTransactionLogFiles(org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
		 private void GenerateTransactionLogFiles( LogFiles logFiles )
		 {
			  LogFile logFile = logFiles.LogFile;
			  FlushablePositionAwareChannel writer = logFile.Writer;
			  for ( sbyte i = 0; i < 107; i++ )
			  {
					writer.Put( i );
					writer.PrepareForFlush();
					if ( logFile.RotationNeeded() )
					{
						 logFile.Rotate();
					}
			  }
		 }
	}

}
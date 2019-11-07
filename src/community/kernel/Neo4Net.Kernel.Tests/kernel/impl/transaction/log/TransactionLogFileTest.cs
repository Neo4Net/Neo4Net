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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using IncompleteLogHeaderException = Neo4Net.Kernel.impl.transaction.log.entry.IncompleteLogHeaderException;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;

	public class TransactionLogFileTest
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionLogFileTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _life );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly LifeRule _life = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(life);
		 public RuleChain RuleChain;

		 private readonly LogVersionRepository _logVersionRepository = new SimpleLogVersionRepository( 1L );
		 private readonly TransactionIdStore _transactionIdStore = new SimpleTransactionIdStore( 2L, 0, BASE_TX_COMMIT_TIMESTAMP, 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipLogFileWithoutHeader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SkipLogFileWithoutHeader()
		 {
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  _life.add( logFiles );
			  _life.start();

			  // simulate new file without header presence
			  _logVersionRepository.incrementAndGetVersion();
			  fs.Create( logFiles.GetLogFileForVersion( _logVersionRepository.CurrentLogVersion ) ).close();
			  _transactionIdStore.transactionCommitted( 5L, 5L, 5L );

			  PhysicalLogicalTransactionStore.LogVersionLocator versionLocator = new PhysicalLogicalTransactionStore.LogVersionLocator( 4L );
			  logFiles.Accept( versionLocator );

			  LogPosition logPosition = versionLocator.LogPosition;
			  assertEquals( 1, logPosition.LogVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOpenInFreshDirectoryAndFinallyAddHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOpenInFreshDirectoryAndFinallyAddHeader()
		 {
			  // GIVEN
			  string name = "log";
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();

			  // WHEN
			  _life.start();
			  _life.add( logFiles );
			  _life.shutdown();

			  // THEN
			  File file = LogFilesBuilder.logFilesBasedOnlyBuilder( _directory.databaseDir(), fs ).build().getLogFileForVersion(1L);
			  LogHeader header = readLogHeader( fs, file );
			  assertEquals( 1L, header.LogVersion );
			  assertEquals( 2L, header.LastCommittedTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteSomeDataIntoTheLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteSomeDataIntoTheLog()
		 {
			  // GIVEN
			  string name = "log";
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  _life.start();
			  _life.add( logFiles );

			  // WHEN
			  FlushablePositionAwareChannel writer = logFiles.LogFile.Writer;
			  LogPositionMarker positionMarker = new LogPositionMarker();
			  writer.GetCurrentPosition( positionMarker );
			  int intValue = 45;
			  long longValue = 4854587;
			  writer.PutInt( intValue );
			  writer.PutLong( longValue );
			  writer.PrepareForFlush().flush();

			  // THEN
			  using ( ReadableClosableChannel reader = logFiles.LogFile.getReader( positionMarker.NewPosition() ) )
			  {
					assertEquals( intValue, reader.Int );
					assertEquals( longValue, reader.Long );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadOlderLogs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadOlderLogs()
		 {
			  // GIVEN
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  _life.start();
			  _life.add( logFiles );

			  // WHEN
			  LogFile logFile = logFiles.LogFile;
			  FlushablePositionAwareChannel writer = logFile.Writer;
			  LogPositionMarker positionMarker = new LogPositionMarker();
			  writer.GetCurrentPosition( positionMarker );
			  LogPosition position1 = positionMarker.NewPosition();
			  int intValue = 45;
			  long longValue = 4854587;
			  sbyte[] someBytes = someBytes( 40 );
			  writer.PutInt( intValue );
			  writer.PutLong( longValue );
			  writer.Put( someBytes, someBytes.Length );
			  writer.PrepareForFlush().flush();
			  writer.GetCurrentPosition( positionMarker );
			  LogPosition position2 = positionMarker.NewPosition();
			  long longValue2 = 123456789L;
			  writer.PutLong( longValue2 );
			  writer.Put( someBytes, someBytes.Length );
			  writer.PrepareForFlush().flush();

			  // THEN
			  using ( ReadableClosableChannel reader = logFile.GetReader( position1 ) )
			  {
					assertEquals( intValue, reader.Int );
					assertEquals( longValue, reader.Long );
					assertArrayEquals( someBytes, ReadBytes( reader, 40 ) );
			  }
			  using ( ReadableClosableChannel reader = logFile.GetReader( position2 ) )
			  {
					assertEquals( longValue2, reader.Long );
					assertArrayEquals( someBytes, ReadBytes( reader, 40 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVisitLogFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitLogFile()
		 {
			  // GIVEN
			  string name = "log";
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  _life.start();
			  _life.add( logFiles );

			  LogFile logFile = logFiles.LogFile;
			  FlushablePositionAwareChannel writer = logFile.Writer;
			  LogPositionMarker mark = new LogPositionMarker();
			  writer.GetCurrentPosition( mark );
			  for ( int i = 0; i < 5; i++ )
			  {
					writer.Put( ( sbyte )i );
			  }
			  writer.PrepareForFlush();

			  // WHEN/THEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean called = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean called = new AtomicBoolean();
			  logFile.Accept(channel =>
			  {
				for ( int i = 0; i < 5; i++ )
				{
					 assertEquals( ( sbyte )i, channel.get() );
				}
				called.set( true );
				return true;
			  }, mark.NewPosition());
			  assertTrue( called.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseChannelInFailedAttemptToReadHeaderAfterOpen() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseChannelInFailedAttemptToReadHeaderAfterOpen()
		 {
			  // GIVEN a file which returns 1/2 log header size worth of bytes
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  int logVersion = 0;
			  File logFile = logFiles.GetLogFileForVersion( logVersion );
			  StoreChannel channel = mock( typeof( StoreChannel ) );
			  when( channel.read( any( typeof( ByteBuffer ) ) ) ).thenReturn( LogHeader.LOG_HEADER_SIZE / 2 );
			  when( fs.FileExists( logFile ) ).thenReturn( true );
			  when( fs.Open( eq( logFile ), any( typeof( OpenMode ) ) ) ).thenReturn( channel );

			  // WHEN
			  try
			  {
					logFiles.OpenForVersion( logVersion );
					fail( "Should have failed" );
			  }
			  catch ( IncompleteLogHeaderException )
			  {
					// THEN good
					verify( channel ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuppressFailureToCloseChannelInFailedAttemptToReadHeaderAfterOpen() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuppressFailureToCloseChannelInFailedAttemptToReadHeaderAfterOpen()
		 {
			  // GIVEN a file which returns 1/2 log header size worth of bytes
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), fs ).withTransactionIdStore(_transactionIdStore).withLogVersionRepository(_logVersionRepository).build();
			  int logVersion = 0;
			  File logFile = logFiles.GetLogFileForVersion( logVersion );
			  StoreChannel channel = mock( typeof( StoreChannel ) );
			  when( channel.read( any( typeof( ByteBuffer ) ) ) ).thenReturn( LogHeader.LOG_HEADER_SIZE / 2 );
			  when( fs.FileExists( logFile ) ).thenReturn( true );
			  when( fs.Open( eq( logFile ), any( typeof( OpenMode ) ) ) ).thenReturn( channel );
			  doThrow( typeof( IOException ) ).when( channel ).close();

			  // WHEN
			  try
			  {
					logFiles.OpenForVersion( logVersion );
					fail( "Should have failed" );
			  }
			  catch ( IncompleteLogHeaderException e )
			  {
					// THEN good
					verify( channel ).close();
					assertEquals( 1, e.Suppressed.length );
					assertTrue( e.Suppressed[0] is IOException );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static byte[] readBytes(ReadableClosableChannel reader, int length) throws java.io.IOException
		 private static sbyte[] ReadBytes( ReadableClosableChannel reader, int length )
		 {
			  sbyte[] result = new sbyte[length];
			  reader.Get( result, length );
			  return result;
		 }

		 private static sbyte[] SomeBytes( int length )
		 {
			  sbyte[] result = new sbyte[length];
			  for ( int i = 0; i < length; i++ )
			  {
					result[i] = ( sbyte )( i % 5 );
			  }
			  return result;
		 }
	}

}
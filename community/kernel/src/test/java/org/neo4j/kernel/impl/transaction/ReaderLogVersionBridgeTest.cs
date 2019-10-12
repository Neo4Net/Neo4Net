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
namespace Org.Neo4j.Kernel.impl.transaction
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using LogVersionedStoreChannel = Org.Neo4j.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using PhysicalLogVersionedStoreChannel = Org.Neo4j.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReaderLogVersionBridge = Org.Neo4j.Kernel.impl.transaction.log.ReaderLogVersionBridge;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.encodeLogVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;

	public class ReaderLogVersionBridgeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly FileSystemAbstraction _fs = mock( typeof( FileSystemAbstraction ) );
		 private readonly LogVersionedStoreChannel _channel = mock( typeof( LogVersionedStoreChannel ) );

		 private readonly long _version = 10L;
		 private LogFiles _logFiles;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _logFiles = PrepareLogFiles();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOpenTheNextChannelWhenItExists() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOpenTheNextChannelWhenItExists()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.StoreChannel newStoreChannel = mock(org.neo4j.io.fs.StoreChannel.class);
			  StoreChannel newStoreChannel = mock( typeof( StoreChannel ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge bridge = new org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge(logFiles);
			  ReaderLogVersionBridge bridge = new ReaderLogVersionBridge( _logFiles );

			  when( _channel.Version ).thenReturn( _version );
			  when( _channel.LogFormatVersion ).thenReturn( CURRENT_LOG_VERSION );
			  when( _fs.fileExists( any( typeof( File ) ) ) ).thenReturn( true );
			  when( _fs.open( any( typeof( File ) ), eq( OpenMode.READ ) ) ).thenReturn( newStoreChannel );
			  when( newStoreChannel.read( ArgumentMatchers.any<ByteBuffer>() ) ).then(invocationOnMock =>
			  {
				ByteBuffer buffer = invocationOnMock.getArgument( 0 );
				buffer.putLong( encodeLogVersion( _version + 1 ) );
				buffer.putLong( 42 );
				return LOG_HEADER_SIZE;
			  });

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogVersionedStoreChannel result = bridge.next(channel);
			  LogVersionedStoreChannel result = bridge.Next( _channel );

			  // then
			  PhysicalLogVersionedStoreChannel expected = new PhysicalLogVersionedStoreChannel( newStoreChannel, _version + 1, CURRENT_LOG_VERSION );
			  assertEquals( expected, result );
			  verify( _channel, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnOldChannelWhenThereIsNoNextChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnOldChannelWhenThereIsNoNextChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge bridge = new org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge(logFiles);
			  ReaderLogVersionBridge bridge = new ReaderLogVersionBridge( _logFiles );

			  when( _channel.Version ).thenReturn( _version );
			  when( _fs.open( any( typeof( File ) ), eq( OpenMode.READ ) ) ).thenThrow( new FileNotFoundException() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogVersionedStoreChannel result = bridge.next(channel);
			  LogVersionedStoreChannel result = bridge.Next( _channel );

			  // then
			  assertEquals( _channel, result );
			  verify( _channel, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnOldChannelWhenNextChannelHasntGottenCompleteHeaderYet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnOldChannelWhenNextChannelHasntGottenCompleteHeaderYet()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge bridge = new org.neo4j.kernel.impl.transaction.log.ReaderLogVersionBridge(logFiles);
			  ReaderLogVersionBridge bridge = new ReaderLogVersionBridge( _logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.StoreChannel nextVersionWithIncompleteHeader = mock(org.neo4j.io.fs.StoreChannel.class);
			  StoreChannel nextVersionWithIncompleteHeader = mock( typeof( StoreChannel ) );
			  when( nextVersionWithIncompleteHeader.read( any( typeof( ByteBuffer ) ) ) ).thenReturn( LOG_HEADER_SIZE / 2 );

			  when( _channel.Version ).thenReturn( _version );
			  when( _fs.fileExists( any( typeof( File ) ) ) ).thenReturn( true );
			  when( _fs.open( any( typeof( File ) ), eq( OpenMode.READ ) ) ).thenReturn( nextVersionWithIncompleteHeader );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogVersionedStoreChannel result = bridge.next(channel);
			  LogVersionedStoreChannel result = bridge.Next( _channel );

			  // then
			  assertEquals( _channel, result );
			  verify( _channel, never() ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.files.LogFiles prepareLogFiles() throws java.io.IOException
		 private LogFiles PrepareLogFiles()
		 {
			  return LogFilesBuilder.logFilesBasedOnlyBuilder( TestDirectory.directory(), _fs ).build();
		 }
	}

}
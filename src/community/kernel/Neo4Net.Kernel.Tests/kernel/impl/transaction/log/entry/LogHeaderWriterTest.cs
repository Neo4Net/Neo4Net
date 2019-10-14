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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.decodeLogFormatVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.decodeLogVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.encodeLogVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.writeLogHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;

	public class LogHeaderWriterTest
	{
		 private readonly long _expectedLogVersion = CURRENT_LOG_VERSION;
		 private readonly long _expectedTxId = 42;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteALogHeaderInTheGivenChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteALogHeaderInTheGivenChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  // when
			  writeLogHeader( channel, _expectedLogVersion, _expectedTxId );

			  // then
			  long encodedLogVersions = channel.Long;
			  assertEquals( encodeLogVersion( _expectedLogVersion ), encodedLogVersions );

			  sbyte logFormatVersion = decodeLogFormatVersion( encodedLogVersions );
			  assertEquals( CURRENT_LOG_VERSION, logFormatVersion );

			  long logVersion = decodeLogVersion( encodedLogVersions );
			  assertEquals( _expectedLogVersion, logVersion );

			  long txId = channel.Long;
			  assertEquals( _expectedTxId, txId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteALogHeaderInTheGivenBuffer()
		 public virtual void ShouldWriteALogHeaderInTheGivenBuffer()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buffer = ByteBuffer.allocate(LOG_HEADER_SIZE);
			  ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer result = writeLogHeader(buffer, expectedLogVersion, expectedTxId);
			  ByteBuffer result = writeLogHeader( buffer, _expectedLogVersion, _expectedTxId );

			  // then
			  assertSame( buffer, result );

			  long encodedLogVersions = result.Long;
			  assertEquals( encodeLogVersion( _expectedLogVersion ), encodedLogVersions );

			  sbyte logFormatVersion = decodeLogFormatVersion( encodedLogVersions );
			  assertEquals( CURRENT_LOG_VERSION, logFormatVersion );

			  long logVersion = decodeLogVersion( encodedLogVersions );
			  assertEquals( _expectedLogVersion, logVersion );

			  long txId = result.Long;
			  assertEquals( _expectedTxId, txId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteALogHeaderInAFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteALogHeaderInAFile()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = testDirectory.file("WriteLogHeader");
			  File file = TestDirectory.file( "WriteLogHeader" );

			  // when
			  writeLogHeader( FileSystemRule.get(), file, _expectedLogVersion, _expectedTxId );

			  // then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] array = new byte[LOG_HEADER_SIZE];
			  sbyte[] array = new sbyte[LOG_HEADER_SIZE];
			  using ( Stream stream = FileSystemRule.get().openAsInputStream(file) )
			  {
					int read = stream.Read( array, 0, array.Length );
					assertEquals( LOG_HEADER_SIZE, read );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer result = ByteBuffer.wrap(array);
			  ByteBuffer result = ByteBuffer.wrap( array );

			  long encodedLogVersions = result.Long;
			  assertEquals( encodeLogVersion( _expectedLogVersion ), encodedLogVersions );

			  sbyte logFormatVersion = decodeLogFormatVersion( encodedLogVersions );
			  assertEquals( CURRENT_LOG_VERSION, logFormatVersion );

			  long logVersion = decodeLogVersion( encodedLogVersions );
			  assertEquals( _expectedLogVersion, logVersion );

			  long txId = result.Long;
			  assertEquals( _expectedTxId, txId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteALogHeaderInAStoreChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteALogHeaderInAStoreChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = testDirectory.file("WriteLogHeader");
			  File file = TestDirectory.file( "WriteLogHeader" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.StoreChannel channel = fileSystemRule.get().open(file, org.neo4j.io.fs.OpenMode.READ_WRITE);
			  StoreChannel channel = FileSystemRule.get().open(file, OpenMode.READ_WRITE);

			  // when
			  writeLogHeader( channel, _expectedLogVersion, _expectedTxId );

			  channel.close();

			  // then
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] array = new byte[LOG_HEADER_SIZE];
			  sbyte[] array = new sbyte[LOG_HEADER_SIZE];
			  using ( Stream stream = FileSystemRule.get().openAsInputStream(file) )
			  {
					int read = stream.Read( array, 0, array.Length );
					assertEquals( LOG_HEADER_SIZE, read );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer result = ByteBuffer.wrap(array);
			  ByteBuffer result = ByteBuffer.wrap( array );

			  long encodedLogVersions = result.Long;
			  assertEquals( encodeLogVersion( _expectedLogVersion ), encodedLogVersions );

			  sbyte logFormatVersion = decodeLogFormatVersion( encodedLogVersions );
			  assertEquals( CURRENT_LOG_VERSION, logFormatVersion );

			  long logVersion = decodeLogVersion( encodedLogVersions );
			  assertEquals( _expectedLogVersion, logVersion );

			  long txId = result.Long;
			  assertEquals( _expectedTxId, txId );
		 }
	}

}
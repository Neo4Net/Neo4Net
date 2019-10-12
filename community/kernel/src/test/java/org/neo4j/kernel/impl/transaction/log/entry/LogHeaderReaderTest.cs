using System.Text;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IoPrimitiveUtils = Org.Neo4j.Kernel.impl.util.IoPrimitiveUtils;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.encodeLogVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;

	public class LogHeaderReaderTest
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
//ORIGINAL LINE: @Test public void shouldReadALogHeaderFromAByteChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadALogHeaderFromAByteChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buffer = ByteBuffer.allocate(LOG_HEADER_SIZE);
			  ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.ReadableByteChannel channel = mock(java.nio.channels.ReadableByteChannel.class);
			  ReadableByteChannel channel = mock( typeof( ReadableByteChannel ) );

			  when( channel.read( buffer ) ).thenAnswer(invocation =>
			  {
				buffer.putLong( encodeLogVersion( _expectedLogVersion ) );
				buffer.putLong( _expectedTxId );
				return 8 + 8;
			  });

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogHeader result = readLogHeader(buffer, channel, true, null);
			  LogHeader result = readLogHeader( buffer, channel, true, null );

			  // then
			  assertEquals( new LogHeader( CURRENT_LOG_VERSION, _expectedLogVersion, _expectedTxId ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenUnableToReadALogHeaderFromAChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenUnableToReadALogHeaderFromAChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buffer = ByteBuffer.allocate(LOG_HEADER_SIZE);
			  ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.ReadableByteChannel channel = mock(java.nio.channels.ReadableByteChannel.class);
			  ReadableByteChannel channel = mock( typeof( ReadableByteChannel ) );

			  when( channel.read( buffer ) ).thenReturn( 1 );

			  try
			  {
					// when
					readLogHeader( buffer, channel, true, null );
					fail( "should have thrown" );
			  }
			  catch ( IncompleteLogHeaderException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadALogHeaderFromAFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadALogHeaderFromAFile()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = testDirectory.file("ReadLogHeader");
			  File file = TestDirectory.file( "ReadLogHeader" );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buffer = ByteBuffer.allocate(LOG_HEADER_SIZE);
			  ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
			  buffer.putLong( encodeLogVersion( _expectedLogVersion ) );
			  buffer.putLong( _expectedTxId );

			  using ( Stream stream = FileSystemRule.get().openAsOutputStream(file, false) )
			  {
					stream.WriteByte( buffer.array() );
			  }

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogHeader result = readLogHeader(fileSystemRule.get(), file);
			  LogHeader result = readLogHeader( FileSystemRule.get(), file );

			  // then
			  assertEquals( new LogHeader( CURRENT_LOG_VERSION, _expectedLogVersion, _expectedTxId ), result );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenUnableToReadALogHeaderFromAFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenUnableToReadALogHeaderFromAFile()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = testDirectory.file("ReadLogHeader");
			  File file = TestDirectory.file( "ReadLogHeader" );
			  FileSystemRule.create( file ).close();
			  try
			  {
					// when
					readLogHeader( FileSystemRule.get(), file );
					fail( "should have thrown" );
			  }
			  catch ( IncompleteLogHeaderException ex )
			  {
					// then
					assertTrue( ex.Message, ex.Message.contains( file.Name ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadALongString() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadALongString()
		 {
			  // given

			  // build a string longer than 32k
			  int stringSize = 32 * 1024 + 1;
			  StringBuilder sb = new StringBuilder();
			  for ( int i = 0; i < stringSize; i++ )
			  {
					sb.Append( "x" );
			  }
			  string lengthyString = sb.ToString();

			  // we need 3 more bytes for writing the string length
			  InMemoryClosableChannel channel = new InMemoryClosableChannel( stringSize + 3 );

			  IoPrimitiveUtils.write3bLengthAndString( channel, lengthyString );

			  // when
			  string stringFromChannel = IoPrimitiveUtils.read3bLengthAndString( channel );

			  // then
			  assertEquals( lengthyString, stringFromChannel );
		 }
	}

}
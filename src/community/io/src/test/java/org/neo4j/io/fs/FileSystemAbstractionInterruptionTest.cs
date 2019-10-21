using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.fs
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.Functions;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FileSystemAbstractionInterruptionTest
	public class FileSystemAbstractionInterruptionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly IFactory<FileSystemAbstraction> _ephemeral = EphemeralFileSystemAbstraction::new;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly IFactory<FileSystemAbstraction> _real = DefaultFileSystemAbstraction::new;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> dataPoints()
		 public static IEnumerable<object[]> DataPoints()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { "ephemeral", _ephemeral },
				  new object[] { "real", _real }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testdir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Testdir = TestDirectory.testDirectory();

		 private FileSystemAbstraction _fs;
		 private File _file;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public FileSystemAbstractionInterruptionTest(@SuppressWarnings("UnusedParameters") String name, org.neo4j.function.Factory<FileSystemAbstraction> factory)
		 public FileSystemAbstractionInterruptionTest( string name, IFactory<FileSystemAbstraction> factory )
		 {
			  _fs = factory.NewInstance();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createWorkingDirectoryAndTestFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateWorkingDirectoryAndTestFile()
		 {
			  Thread.interrupted();
			  _fs.mkdirs( Testdir.directory() );
			  _file = Testdir.file( "a" );
			  _fs.create( _file ).close();
			  _channel = null;
			  _channelShouldBeClosed = false;
			  Thread.CurrentThread.Interrupt();
		 }

		 private StoreChannel _channel;
		 private bool _channelShouldBeClosed;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void verifyInterruptionAndChannelState() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyInterruptionAndChannelState()
		 {
			  assertTrue( Thread.interrupted() );
			  assertThat( "channelShouldBeClosed? " + _channelShouldBeClosed, _channel.Open, @is( !_channelShouldBeClosed ) );

			  if ( _channelShouldBeClosed )
			  {
					try
					{
						 _channel.force( true );
						 fail( "Operating on a closed channel should fail" );
					}
					catch ( ClosedChannelException )
					{
						 // This is good. What we expect to see.
					}
			  }
			  _channel.close();
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private StoreChannel chan(boolean channelShouldBeClosed) throws java.io.IOException
		 private StoreChannel Chan( bool channelShouldBeClosed )
		 {
			  this._channelShouldBeClosed = channelShouldBeClosed;
			  _channel = _fs.open( _file, OpenMode.ReadWrite );
			  return _channel;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fs_openClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FsOpenClose()
		 {
			  Chan( true ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ch_tryLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChTryLock()
		 {
			  Chan( false ).tryLock().release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_setPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChSetPosition()
		 {
			  Chan( true ).position( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_getPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChGetPosition()
		 {
			  Chan( true ).position();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_truncate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChTruncate()
		 {
			  Chan( true ).truncate( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_force() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChForce()
		 {
			  Chan( true ).force( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_writeAll_ByteBuffer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChWriteAllByteBuffer()
		 {
			  Chan( true ).writeAll( ByteBuffer.allocate( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_writeAll_ByteBuffer_position() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChWriteAllByteBufferPosition()
		 {
			  Chan( true ).writeAll( ByteBuffer.allocate( 1 ), 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_read_ByteBuffer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChReadByteBuffer()
		 {
			  Chan( true ).read( ByteBuffer.allocate( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_write_ByteBuffer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChWriteByteBuffer()
		 {
			  Chan( true ).write( ByteBuffer.allocate( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_size() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChSize()
		 {
			  Chan( true ).size();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ch_isOpen() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChIsOpen()
		 {
			  Chan( false ).Open;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_write_ByteBuffers_offset_length() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChWriteByteBuffersOffsetLength()
		 {
			  Chan( true ).write( new ByteBuffer[]{ ByteBuffer.allocate( 1 ) }, 0, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_write_ByteBuffers() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChWriteByteBuffers()
		 {
			  Chan( true ).write( new ByteBuffer[]{ ByteBuffer.allocate( 1 ) } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_read_ByteBuffers_offset_length() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChReadByteBuffersOffsetLength()
		 {
			  Chan( true ).read( new ByteBuffer[]{ ByteBuffer.allocate( 1 ) }, 0, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.nio.channels.ClosedByInterruptException.class) public void ch_read_ByteBuffers() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChReadByteBuffers()
		 {
			  Chan( true ).read( new ByteBuffer[]{ ByteBuffer.allocate( 1 ) } );
		 }
	}

}
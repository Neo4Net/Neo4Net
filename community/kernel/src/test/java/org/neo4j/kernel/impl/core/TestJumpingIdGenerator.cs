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
namespace Org.Neo4j.Kernel.impl.core
{
	using Test = org.junit.Test;


	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using JumpingFileChannel = Org.Neo4j.Kernel.impl.core.JumpingFileSystemAbstraction.JumpingFileChannel;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.NodeRecordFormat.RECORD_SIZE;

	public class TestJumpingIdGenerator
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIt()
		 public virtual void TestIt()
		 {
			  int sizePerJump = 1000;
			  IdGeneratorFactory factory = new JumpingIdGeneratorFactory( sizePerJump );
			  IdGenerator generator = factory.Get( IdType.NODE );
			  for ( int i = 0; i < sizePerJump / 2; i++ )
			  {
					assertEquals( i, generator.NextId() );
			  }

			  for ( int i = 0; i < sizePerJump - 1; i++ )
			  {
					long expected = 0x100000000L - sizePerJump / 2 + i;
					if ( expected >= 0xFFFFFFFFL )
					{
						 expected++;
					}
					assertEquals( expected, generator.NextId() );
			  }

			  for ( int i = 0; i < sizePerJump; i++ )
			  {
					assertEquals( 0x200000000L - sizePerJump / 2 + i, generator.NextId() );
			  }

			  for ( int i = 0; i < sizePerJump; i++ )
			  {
					assertEquals( 0x300000000L - sizePerJump / 2 + i, generator.NextId() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOffsetFileChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestOffsetFileChannel()
		 {
			  using ( JumpingFileSystemAbstraction offsetFileSystem = new JumpingFileSystemAbstraction( 10 ) )
			  {
					File fileName = new File( "target/var/neostore.nodestore.db" );
					offsetFileSystem.DeleteFile( fileName );
					offsetFileSystem.Mkdirs( fileName.ParentFile );
					IdGenerator idGenerator = ( new JumpingIdGeneratorFactory( 10 ) ).Get( IdType.NODE );

					using ( JumpingFileChannel channel = ( JumpingFileChannel ) offsetFileSystem.Open( fileName, OpenMode.READ_WRITE ) )
					{
						 for ( int i = 0; i < 16; i++ )
						 {
							  WriteSomethingLikeNodeRecord( channel, idGenerator.NextId(), i );
						 }

					}
					using ( JumpingFileChannel channel = ( JumpingFileChannel ) offsetFileSystem.Open( fileName, OpenMode.READ_WRITE ) )
					{
						 idGenerator = ( new JumpingIdGeneratorFactory( 10 ) ).Get( IdType.NODE );

						 for ( int i = 0; i < 16; i++ )
						 {
							  assertEquals( i, ReadSomethingLikeNodeRecord( channel, idGenerator.NextId() ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte readSomethingLikeNodeRecord(org.neo4j.kernel.impl.core.JumpingFileSystemAbstraction.JumpingFileChannel channel, long id) throws java.io.IOException
		 private sbyte ReadSomethingLikeNodeRecord( JumpingFileChannel channel, long id )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( RECORD_SIZE );
			  channel.Position( id * RECORD_SIZE );
			  channel.Read( buffer );
			  buffer.flip();
			  buffer.Long;
			  return buffer.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeSomethingLikeNodeRecord(org.neo4j.kernel.impl.core.JumpingFileSystemAbstraction.JumpingFileChannel channel, long id, int justAByte) throws java.io.IOException
		 private void WriteSomethingLikeNodeRecord( JumpingFileChannel channel, long id, int justAByte )
		 {
			  channel.Position( id * RECORD_SIZE );
			  ByteBuffer buffer = ByteBuffer.allocate( RECORD_SIZE );
			  buffer.putLong( 4321 );
			  buffer.put( ( sbyte ) justAByte );
			  buffer.flip();
			  channel.Write( buffer );
		 }
	}

}
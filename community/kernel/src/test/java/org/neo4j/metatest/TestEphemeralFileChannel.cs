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
namespace Org.Neo4j.Metatest
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static ByteBuffer.allocate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(EphemeralFileSystemExtension.class) class TestEphemeralFileChannel
	internal class TestEphemeralFileChannel
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fileSystem;
		 private EphemeralFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smoke() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Smoke()
		 {
			  StoreChannel channel = _fileSystem.open( new File( "yo" ), OpenMode.READ_WRITE );

			  // Clear it because we depend on it to be zeros where we haven't written
			  ByteBuffer buffer = allocate( 23 );
			  buffer.put( new sbyte[23] ); // zeros
			  buffer.flip();
			  channel.write( buffer );
			  channel = _fileSystem.open( new File( "yo" ), OpenMode.READ_WRITE );
			  long longValue = 1234567890L;

			  // [1].....[2]........[1234567890L]...

			  buffer.clear();
			  buffer.limit( 1 );
			  buffer.put( ( sbyte ) 1 );
			  buffer.flip();
			  channel.write( buffer );

			  buffer.clear();
			  buffer.limit( 1 );
			  buffer.put( ( sbyte ) 2 );
			  buffer.flip();
			  channel.Position( 6 );
			  channel.write( buffer );

			  buffer.clear();
			  buffer.limit( 8 );
			  buffer.putLong( longValue );
			  buffer.flip();
			  channel.Position( 15 );
			  channel.write( buffer );
			  assertEquals( 23, channel.size() );

			  // Read with position
			  // byte 0
			  buffer.clear();
			  buffer.limit( 1 );
			  channel.Read( buffer, 0 );
			  buffer.flip();
			  assertEquals( ( sbyte ) 1, buffer.get() );

			  // bytes 5-7
			  buffer.clear();
			  buffer.limit( 3 );
			  channel.Read( buffer, 5 );
			  buffer.flip();
			  assertEquals( ( sbyte ) 0, buffer.get() );
			  assertEquals( ( sbyte ) 2, buffer.get() );
			  assertEquals( ( sbyte ) 0, buffer.get() );

			  // bytes 15-23
			  buffer.clear();
			  buffer.limit( 8 );
			  channel.Read( buffer, 15 );
			  buffer.flip();
			  assertEquals( longValue, buffer.Long );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void absoluteVersusRelative() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AbsoluteVersusRelative()
		 {
			  // GIVEN
			  File file = new File( "myfile" );
			  StoreChannel channel = _fileSystem.open( file, OpenMode.READ_WRITE );
			  sbyte[] bytes = "test".GetBytes();
			  channel.write( ByteBuffer.wrap( bytes ) );
			  channel.close();

			  // WHEN
			  channel = _fileSystem.open( new File( file.AbsolutePath ), OpenMode.READ );
			  sbyte[] readBytes = new sbyte[bytes.Length];
			  channel.ReadAll( ByteBuffer.wrap( readBytes ) );

			  // THEN
			  assertTrue( Arrays.Equals( bytes, readBytes ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListFiles()
		 {
			  /* GIVEN
			   *                        root
			   *                       /    \
			   *         ----------- dir1   dir2
			   *        /       /     |       \
			   *    subdir1  file  file2      file
			   *       |
			   *     file
			   */
			  File root = ( new File( "/root" ) ).CanonicalFile;
			  File dir1 = new File( root, "dir1" );
			  File dir2 = new File( root, "dir2" );
			  File subdir1 = new File( dir1, "sub" );
			  File file1 = new File( dir1, "file" );
			  File file2 = new File( dir1, "file2" );
			  File file3 = new File( dir2, "file" );
			  File file4 = new File( subdir1, "file" );

			  _fileSystem.mkdirs( dir2 );
			  _fileSystem.mkdirs( dir1 );
			  _fileSystem.mkdirs( subdir1 );

			  _fileSystem.create( file1 );
			  _fileSystem.create( file2 );
			  _fileSystem.create( file3 );
			  _fileSystem.create( file4 );

			  // THEN
			  assertEquals( asSet( dir1, dir2 ), asSet( _fileSystem.listFiles( root ) ) );
			  assertEquals( asSet( subdir1, file1, file2 ), asSet( _fileSystem.listFiles( dir1 ) ) );
			  assertEquals( asSet( file3 ), asSet( _fileSystem.listFiles( dir2 ) ) );
			  assertEquals( asSet( file4 ), asSet( _fileSystem.listFiles( subdir1 ) ) );
		 }
	}

}
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
namespace Org.Neo4j.Kernel.impl.store.id
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreFileChannel = Org.Neo4j.Io.fs.StoreFileChannel;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;


	public class IdContainerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly FileSystemRule FileSystemRule = new DefaultFileSystemRule();
		 private FileSystemAbstraction _fs;
		 private File _file;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fs = FileSystemRule.get();
			  _file = TestDirectory.file( "ids" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void includeFileNameIntoReadHeaderException() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncludeFileNameIntoReadHeaderException()
		 {
			  CreateEmptyFile();
			  _fs.truncate( _file, 0 );

			  try
			  {
					IdContainer idContainer = new IdContainer( _fs, _file, 100, false );
					idContainer.Init();
			  }
			  catch ( InvalidIdGeneratorException e )
			  {
					assertThat( e.Message, Matchers.containsString( _file.AbsolutePath ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteIfOpen()
		 public virtual void ShouldDeleteIfOpen()
		 {
			  // GIVEN
			  CreateEmptyFile();
			  IdContainer idContainer = new IdContainer( _fs, _file, 100, false );
			  idContainer.Init();

			  // WHEN
			  idContainer.Delete();

			  // THEN
			  assertFalse( _fs.fileExists( _file ) );

			  idContainer.Close( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteIfClosed()
		 public virtual void ShouldDeleteIfClosed()
		 {
			  // GIVEN
			  CreateEmptyFile();
			  IdContainer idContainer = new IdContainer( _fs, _file, 100, false );
			  idContainer.Init();
			  idContainer.Close( 0 );

			  // WHEN
			  idContainer.Delete();

			  // THEN
			  assertFalse( _fs.fileExists( _file ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceStickyMark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForceStickyMark()
		 {
			  // GIVEN
			  CreateEmptyFile();

			  // WHEN opening the id generator, where the jvm crashes right after
			  IdContainer idContainer = new IdContainer( _fs, _file, 100, false );
			  idContainer.Init();

			  // THEN
			  try
			  {
					IdContainer.ReadHighId( _fs, _file );
					fail( "Should have thrown, saying something with sticky generator" );
			  }
			  catch ( InvalidIdGeneratorException )
			  {
					// THEN Good
			  }
			  finally
			  {
					idContainer.Close( 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateTheFileIfOverwriting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateTheFileIfOverwriting()
		 {
			  // GIVEN
			  IdContainer.CreateEmptyIdFile( _fs, _file, 30, false );
			  IdContainer idContainer = new IdContainer( _fs, _file, 5, false );
			  idContainer.Init();
			  for ( int i = 0; i < 17; i++ )
			  {
					idContainer.FreeId( i );
			  }
			  idContainer.Close( 30 );
			  assertThat( ( int ) _fs.getFileSize( _file ), greaterThan( IdContainer.HeaderSize ) );

			  // WHEN
			  IdContainer.CreateEmptyIdFile( _fs, _file, 30, false );

			  // THEN
			  assertEquals( IdContainer.HeaderSize, ( int ) _fs.getFileSize( _file ) );
			  assertEquals( 30, IdContainer.ReadHighId( _fs, _file ) );
			  idContainer = new IdContainer( _fs, _file, 5, false );
			  idContainer.Init();
			  assertEquals( 30, idContainer.InitialHighId );

			  idContainer.Close( 30 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseOnInitIfTheFileWasCreated()
		 public virtual void ShouldReturnFalseOnInitIfTheFileWasCreated()
		 {
			  // When
			  // An IdContainer is created with no underlying file
			  IdContainer idContainer = new IdContainer( _fs, _file, 100, false );

			  // Then
			  // Init should return false
			  assertFalse( idContainer.Init() );
			  idContainer.Close( 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueOnInitIfAProperFileWasThere()
		 public virtual void ShouldReturnTrueOnInitIfAProperFileWasThere()
		 {
			  // Given
			  // A properly created and closed id file
			  IdContainer idContainer = new IdContainer( _fs, _file, 100, false );
			  idContainer.Init();
			  idContainer.Close( 100 );

			  // When
			  // An IdContainer is created over it
			  idContainer = new IdContainer( _fs, _file, 100, false );

			  // Then
			  // init() should return true
			  assertTrue( idContainer.Init() );
			  idContainer.Close( 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void idContainerReadWriteBySingleByte() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IdContainerReadWriteBySingleByte()
		 {
			  SingleByteFileSystemAbstraction fileSystem = new SingleByteFileSystemAbstraction();
			  IdContainer idContainer = new IdContainer( fileSystem, _file, 100, false );
			  idContainer.Init();
			  idContainer.Close( 100 );

			  idContainer = new IdContainer( fileSystem, _file, 100, false );
			  idContainer.Init();
			  assertEquals( 100, idContainer.InitialHighId );
			  fileSystem.Dispose();
			  idContainer.Close( 100 );
		 }

		 private void CreateEmptyFile()
		 {
			  IdContainer.CreateEmptyIdFile( _fs, _file, 42, false );
		 }

		 private class SingleByteFileSystemAbstraction : DefaultFileSystemAbstraction
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreFileChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode mode) throws java.io.IOException
			  public override StoreFileChannel Open( File fileName, OpenMode mode )
			  {
					return new SingleByteBufferChannel( base.Open( fileName, mode ) );
			  }
		 }

		 private class SingleByteBufferChannel : StoreFileChannel
		 {

			  internal SingleByteBufferChannel( StoreFileChannel channel ) : base( channel )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
			  public override int Write( ByteBuffer src )
			  {
					sbyte b = src.get();
					ByteBuffer byteBuffer = ByteBuffer.wrap( new sbyte[]{ b } );
					return base.Write( byteBuffer );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
			  public override int Read( ByteBuffer dst )
			  {
					ByteBuffer byteBuffer = ByteBuffer.allocate( 1 );
					int read = base.Read( byteBuffer );
					if ( read > 0 )
					{
						 byteBuffer.flip();
						 dst.put( byteBuffer.get() );
					}
					return read;
			  }
		 }
	}

}
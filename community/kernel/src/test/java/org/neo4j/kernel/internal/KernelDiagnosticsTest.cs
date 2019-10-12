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
namespace Org.Neo4j.Kernel.@internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.kibiBytes;

	public class KernelDiagnosticsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fs = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly TestDirectory DirectoryConflict = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintDiskUsage()
		 public virtual void ShouldPrintDiskUsage()
		 {
			  // Not sure how to get around this w/o spying. The method that we're unit testing will construct
			  // other File instances with this guy as parent and internally the File constructor uses the field 'path'
			  // which, if purely mocked, won't be assigned. At the same time we want to control the total/free space methods
			  // and what they return... a tough one.
			  File storeDir = Mockito.spy( new File( "storeDir" ) );
			  DatabaseLayout layout = mock( typeof( DatabaseLayout ) );
			  when( layout.DatabaseDirectory() ).thenReturn(storeDir);
			  when( storeDir.TotalSpace ).thenReturn( 100L );
			  when( storeDir.FreeSpace ).thenReturn( 40L );

			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  KernelDiagnostics.StoreFiles storeFiles = new KernelDiagnostics.StoreFiles( layout );
			  storeFiles.Dump( logProvider.getLog( this.GetType() ).debugLogger() );

			  logProvider.RawMessageMatcher().assertContains("100 / 40 / 40");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountFileSizeRecursively() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountFileSizeRecursively()
		 {
			  // file structure:
			  //   storeDir/indexDir/indexFile (1 kB)
			  //   storeDir/neostore (3 kB)
			  File storeDir = DirectoryConflict.directory( "storeDir" );
			  DatabaseLayout layout = DatabaseLayout.of( storeDir );
			  File indexDir = Directory( storeDir, "indexDir" );
			  File( indexDir, "indexFile", ( int ) kibiBytes( 1 ) );
			  File( storeDir, layout.MetadataStore().Name, (int) kibiBytes(3) );

			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  KernelDiagnostics.StoreFiles storeFiles = new KernelDiagnostics.StoreFiles( layout );
			  storeFiles.Dump( logProvider.getLog( this.GetType() ).debugLogger() );

			  logProvider.RawMessageMatcher().assertContains("Total size of store: 4.00 kB");
			  logProvider.RawMessageMatcher().assertContains("Total size of mapped files: 3.00 kB");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File directory(java.io.File parent, String name) throws java.io.IOException
		 private File Directory( File parent, string name )
		 {
			  File dir = new File( parent, name );
			  Fs.mkdirs( dir );
			  return dir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File file(java.io.File parent, String name, int size) throws java.io.IOException
		 private File File( File parent, string name, int size )
		 {
			  File file = new File( parent, name );
			  using ( StoreChannel channel = Fs.create( file ) )
			  {
					ByteBuffer buffer = ByteBuffer.allocate( size );
					buffer.position( size ).flip();
					channel.write( buffer );
			  }
			  return file;
		 }
	}

}
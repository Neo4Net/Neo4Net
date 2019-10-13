using System;
using System.IO;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.com.storecopy
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class FileMoveProviderTest
	{
		private bool InstanceFieldsInitialized = false;

		public FileMoveProviderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDirectory = TestDirectory.testDirectory( _defaultFileSystemAbstraction );
		}

		 private DefaultFileSystemAbstraction _defaultFileSystemAbstraction = new DefaultFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory(defaultFileSystemAbstraction);
		 public TestDirectory TestDirectory;

		 private FileMoveProvider _subject;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _subject = new FileMoveProvider( _defaultFileSystemAbstraction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveSingleFiles() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveSingleFiles()
		 {
			  // given
			  File sharedParent = TestDirectory.cleanDirectory( "shared_parent" );
			  File sourceParent = new File( sharedParent, "source" );
			  assertTrue( sourceParent.mkdirs() );
			  File sourceFile = new File( sourceParent, "file.txt" );
			  assertTrue( sourceFile.createNewFile() );
			  WriteToFile( sourceFile, "Garbage data" );
			  File targetParent = new File( sharedParent, "target" );
			  assertTrue( targetParent.mkdirs() );
			  File targetFile = new File( targetParent, "file.txt" );

			  // when
			  _subject.traverseForMoving( sourceFile ).forEach( MoveToDirectory( targetParent ) );

			  // then
			  assertEquals( "Garbage data", ReadFromFile( targetFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void singleDirectoriesAreNotMoved() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SingleDirectoriesAreNotMoved()
		 {
			  // given
			  File sharedParent = TestDirectory.cleanDirectory( "shared_parent" );
			  File sourceParent = new File( sharedParent, "source" );
			  assertTrue( sourceParent.mkdirs() );
			  File sourceDirectory = new File( sourceParent, "directory" );
			  assertTrue( sourceDirectory.mkdirs() );

			  // and
			  File targetParent = new File( sharedParent, "target" );
			  assertTrue( targetParent.mkdirs() );
			  File targetDirectory = new File( targetParent, "directory" );
			  assertFalse( targetDirectory.exists() );

			  // when
			  _subject.traverseForMoving( sourceParent ).forEach( MoveToDirectory( targetDirectory ) );

			  // then
			  assertTrue( sourceDirectory.exists() );
			  assertFalse( targetDirectory.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveNestedFiles() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveNestedFiles()
		 {
			  // given
			  File sharedParent = TestDirectory.cleanDirectory( "shared_parent" );
			  File sourceParent = new File( sharedParent, "source" );
			  assertTrue( sourceParent.mkdirs() );
			  File targetParent = new File( sharedParent, "target" );
			  assertTrue( targetParent.mkdirs() );

			  // and
			  File dirA = new File( sourceParent, "A" );
			  assertTrue( dirA.mkdirs() );
			  File nestedFileOne = new File( dirA, "file.txt" );
			  assertTrue( nestedFileOne.createNewFile() );
			  File dirB = new File( sourceParent, "B" );
			  assertTrue( dirB.mkdirs() );
			  File nestedFileTwo = new File( dirB, "file.txt" );
			  assertTrue( nestedFileTwo.createNewFile() );
			  WriteToFile( nestedFileOne, "This is the file contained in directory A" );
			  WriteToFile( nestedFileTwo, "This is the file contained in directory B" );

			  // and
			  File targetFileOne = new File( targetParent, "A/file.txt" );
			  File targetFileTwo = new File( targetParent, "B/file.txt" );

			  // when
			  _subject.traverseForMoving( sourceParent ).forEach( MoveToDirectory( targetParent ) );

			  // then
			  assertEquals( "This is the file contained in directory A", ReadFromFile( targetFileOne ) );
			  assertEquals( "This is the file contained in directory B", ReadFromFile( targetFileTwo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filesAreMovedBeforeDirectories() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FilesAreMovedBeforeDirectories()
		 {
			  // given there is a file contained in a directory
			  File parentDirectory = TestDirectory.cleanDirectory( "parent" );
			  File sourceDirectory = new File( parentDirectory, "source" );
			  assertTrue( sourceDirectory.mkdirs() );
			  File childFile = new File( sourceDirectory, "child" );
			  assertTrue( childFile.createNewFile() );
			  WriteToFile( childFile, "Content" );

			  // and we have an expected target directory
			  File targetDirectory = new File( parentDirectory, "target" );
			  assertTrue( targetDirectory.mkdirs() );

			  // when
			  _subject.traverseForMoving( sourceDirectory ).forEach( MoveToDirectory( targetDirectory ) );

			  // then no exception due to files happening before empty target directory
		 }

		 private System.Action<FileMoveAction> MoveToDirectory( File toDirectory )
		 {
			  return fileMoveAction =>
			  {
				try
				{
					 fileMoveAction.move( toDirectory );
				}
				catch ( Exception throwable )
				{
					 throw new AssertionError( throwable );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String readFromFile(java.io.File input) throws java.io.IOException
		 private string ReadFromFile( File input )
		 {
			  StreamReader fileReader = new StreamReader( input );
			  StringBuilder stringBuilder = new StringBuilder();
			  char[] data = new char[32];
			  int read;
			  while ( ( read = fileReader.Read( data, 0, data.Length ) ) != -1 )
			  {
					stringBuilder.Append( data, 0, read );
			  }
			  return stringBuilder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeToFile(java.io.File output, String input) throws java.io.IOException
		 private void WriteToFile( File output, string input )
		 {
			  using ( StreamWriter bw = new StreamWriter( output ) )
			  {
					bw.Write( input );
			  }
		 }
	}

}
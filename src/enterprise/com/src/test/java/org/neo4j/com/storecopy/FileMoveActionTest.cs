/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com.storecopy
{

	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class FileMoveActionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private FileSystemAbstraction _fileSystemAbstraction = new DefaultFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonPageCacheFilesMovedDoNotLeaveOriginal() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NonPageCacheFilesMovedDoNotLeaveOriginal()
		 {
			  // given
			  File baseDirectory = TestDirectory.directory();
			  File sourceDirectory = new File( baseDirectory, "source" );
			  File targetDirectory = new File( baseDirectory, "destination" );
			  File sourceFile = new File( sourceDirectory, "theFileName" );
			  File targetFile = new File( targetDirectory, "theFileName" );
			  sourceFile.ParentFile.mkdirs();
			  targetDirectory.mkdirs();

			  // and sanity check
			  assertTrue( sourceFile.createNewFile() );
			  assertTrue( sourceFile.exists() );
			  assertFalse( targetFile.exists() );

			  // when
			  FileMoveAction.moveViaFileSystem( sourceFile, sourceDirectory ).move( targetDirectory );

			  // then
			  assertTrue( targetFile.exists() );
			  assertFalse( sourceFile.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonPageCacheFilesCopiedLeaveOriginal() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NonPageCacheFilesCopiedLeaveOriginal()
		 {
			  // given
			  File baseDirectory = TestDirectory.directory();
			  File sourceDirectory = new File( baseDirectory, "source" );
			  File targetDirectory = new File( baseDirectory, "destination" );
			  File sourceFile = new File( sourceDirectory, "theFileName" );
			  File targetFile = new File( targetDirectory, "theFileName" );
			  sourceFile.ParentFile.mkdirs();
			  targetDirectory.mkdirs();

			  // and sanity check
			  assertTrue( sourceFile.createNewFile() );
			  assertTrue( sourceFile.exists() );
			  assertFalse( targetFile.exists() );

			  // when
			  FileMoveAction.copyViaFileSystem( sourceFile, sourceDirectory ).move( targetDirectory );

			  // then
			  assertTrue( targetFile.exists() );
			  assertTrue( sourceFile.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void symbolicLinkAsTargetShouldNotBreakTheMove() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SymbolicLinkAsTargetShouldNotBreakTheMove()
		 {
			  /*
			   * Setup the following structure
			   * - realSourceFile: a dummy file serving as the file to copy, the original source
			   * - realTargetDirectory: the real directory to move the file into
			   * - linkTargetDirectory: a symbolic link pointing to realTargetDirectory.
			   */
			  string realFileFilename = "realFile"; // we need this for the assert at the end
			  Path realSourceFile = Files.createFile( ( new File( TestDirectory.absolutePath(), realFileFilename ) ).toPath() );
			  Path realTargetDirectory = Files.createDirectory( ( new File( TestDirectory.absolutePath(), "realTargetDirectory" ) ).toPath() );
			  Path linkTargetDirectory = Files.createSymbolicLink( ( new File( TestDirectory.absolutePath(), "linkToTarget" ) ).toPath(), realTargetDirectory );

			  /*
			   * We now try to copy the realSourceFile to the linkTargetDirectory. This must succeed.
			   * As a reminder, the FileMoveAction.copyViaFileSystem() will prepare a file move operation for the real source file
			   *  (contained in the top level test directory). The move() call will accept as an argument the symbolic link and
			   *  try to move the source in there.
			   */
			  FileMoveAction.copyViaFileSystem( realSourceFile.toFile(), TestDirectory.absolutePath() ).move(linkTargetDirectory.toFile());

			  File target = new File( linkTargetDirectory.toFile(), realFileFilename );
			  assertTrue( Files.exists( target.toPath() ) );
		 }
	}

}
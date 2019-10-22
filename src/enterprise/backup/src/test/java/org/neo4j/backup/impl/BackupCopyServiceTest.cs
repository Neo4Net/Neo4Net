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
namespace Neo4Net.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileMoveAction = Neo4Net.com.storecopy.FileMoveAction;
	using FileMoveProvider = Neo4Net.com.storecopy.FileMoveProvider;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BackupCopyServiceTest
	{
		 private FileMoveProvider _fileMoveProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 internal BackupCopyService Subject;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  PageCache pageCache = mock( typeof( PageCache ) );
			  _fileMoveProvider = mock( typeof( FileMoveProvider ) );
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  Subject = new BackupCopyService( fs, _fileMoveProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logicForMovingBackupsIsDelegatedToFileMovePropagator() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogicForMovingBackupsIsDelegatedToFileMovePropagator()
		 {
			  // given
			  Path parentDirectory = TestDirectory.directory( "parent" ).toPath();
			  Path oldLocation = parentDirectory.resolve( "oldLocation" );
			  Files.createDirectories( oldLocation );
			  Path newLocation = parentDirectory.resolve( "newLocation" );

			  // and
			  FileMoveAction fileOneMoveAction = mock( typeof( FileMoveAction ) );
			  FileMoveAction fileTwoMoveAction = mock( typeof( FileMoveAction ) );
			  when( _fileMoveProvider.traverseForMoving( any() ) ).thenReturn(Stream.of(fileOneMoveAction, fileTwoMoveAction));

			  // when
			  Subject.moveBackupLocation( oldLocation, newLocation );

			  // then file move propagator was requested with correct source and baseDirectory
			  verify( _fileMoveProvider ).traverseForMoving( oldLocation.toFile() );

			  // and files were moved to correct target directory
			  verify( fileOneMoveAction ).move( newLocation.toFile() );
			  verify( fileTwoMoveAction ).move( newLocation.toFile() );
		 }
	}

}
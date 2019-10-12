using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class FileNamesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyFormatFilenameForVersion()
		 public virtual void ShouldProperlyFormatFilenameForVersion()
		 {
			  // Given
			  File @base = new File( "base" );
			  FileNames fileNames = new FileNames( @base );

			  // When - Then
			  // when asking for a given version...
			  for ( int i = 0; i < 100; i++ )
			  {
					File forVersion = fileNames.GetForVersion( i );
					// ...then the expected thing is returned
					assertEquals( forVersion, new File( @base, FileNames.BASE_FILE_NAME + i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkCorrectlyOnReasonableDirectoryContents()
		 public virtual void ShouldWorkCorrectlyOnReasonableDirectoryContents()
		 {
			  // Given
			  // a raft log directory with just the expected files, without gaps
			  File @base = new File( "base" );
			  FileNames fileNames = new FileNames( @base );
			  FileSystemAbstraction fsa = mock( typeof( FileSystemAbstraction ) );
			  Log log = mock( typeof( Log ) );
			  IList<File> filesPresent = new LinkedList<File>();
			  int lower = 0;
			  int upper = 24;
			  // the files are added in reverse order, so we can verify that FileNames orders based on version
			  for ( int i = upper; i >= lower; i-- )
			  {
					filesPresent.Add( fileNames.GetForVersion( i ) );
			  }
			  when( fsa.ListFiles( @base ) ).thenReturn( filesPresent.ToArray() );

			  // When
			  // asked for the contents of the directory
			  SortedDictionary<long, File> allFiles = fileNames.GetAllFiles( fsa, log );

			  // Then
			  // all the things we added above should be returned
			  assertEquals( upper - lower + 1, allFiles.Count );
			  long currentVersion = lower;
			  foreach ( KeyValuePair<long, File> longFileEntry in allFiles.SetOfKeyValuePairs() )
			  {
					assertEquals( currentVersion, longFileEntry.Key.longValue() );
					assertEquals( fileNames.GetForVersion( currentVersion ), longFileEntry.Value );
					currentVersion++;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreUnexpectedLogDirectoryContents()
		 public virtual void ShouldIgnoreUnexpectedLogDirectoryContents()
		 {
			  // Given
			  // a raft log directory with just the expected files, without gaps
			  File @base = new File( "base" );
			  FileNames fileNames = new FileNames( @base );
			  FileSystemAbstraction fsa = mock( typeof( FileSystemAbstraction ) );
			  Log log = mock( typeof( Log ) );
			  IList<File> filesPresent = new LinkedList<File>();

			  filesPresent.Add( fileNames.GetForVersion( 0 ) ); // should be included
			  filesPresent.Add( fileNames.GetForVersion( 1 ) ); // should be included
			  filesPresent.Add( fileNames.GetForVersion( 10 ) ); // should be included
			  filesPresent.Add( fileNames.GetForVersion( 11 ) ); // should be included
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "01" ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "001" ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "-1" ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "1a" ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "a1" ) ); // should be ignored
			  filesPresent.Add( new File( @base, FileNames.BASE_FILE_NAME + "ab" ) ); // should be ignored

			  when( fsa.ListFiles( @base ) ).thenReturn( filesPresent.ToArray() );

			  // When
			  // asked for the contents of the directory
			  SortedDictionary<long, File> allFiles = fileNames.GetAllFiles( fsa, log );

			  // Then
			  // only valid things should be returned
			  assertEquals( 4, allFiles.Count );
			  assertEquals( allFiles[0L], fileNames.GetForVersion( 0 ) );
			  assertEquals( allFiles[1L], fileNames.GetForVersion( 1 ) );
			  assertEquals( allFiles[10L], fileNames.GetForVersion( 10 ) );
			  assertEquals( allFiles[11L], fileNames.GetForVersion( 11 ) );

			  // and the invalid ones should be logged
			  verify( log, times( 7 ) ).warn( anyString() );
		 }
	}

}
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
namespace Org.Neo4j.Io.fs
{
	using Test = org.junit.jupiter.api.Test;


	using SelectiveFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.SelectiveFileSystemAbstraction;
	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;
	using WatchedResource = Org.Neo4j.Io.fs.watcher.resource.WatchedResource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class SelectiveFileSystemAbstractionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCorrectFileSystemForChosenFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseCorrectFileSystemForChosenFile()
		 {
			  // given
			  File specialFile = new File( "special" );
			  FileSystemAbstraction normal = mock( typeof( FileSystemAbstraction ) );
			  FileSystemAbstraction special = mock( typeof( FileSystemAbstraction ) );

			  // when
			  using ( SelectiveFileSystemAbstraction systemAbstraction = new SelectiveFileSystemAbstraction( specialFile, special, normal ) )
			  {
					systemAbstraction.Open( specialFile, OpenMode.Read );

					// then
					verify( special ).open( specialFile, OpenMode.Read );
					verifyNoMoreInteractions( special );
					verifyNoMoreInteractions( normal );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseDefaultFileSystemForOtherFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseDefaultFileSystemForOtherFiles()
		 {
			  // given
			  File specialFile = new File( "special" );
			  File otherFile = new File( "other" );

			  FileSystemAbstraction normal = mock( typeof( FileSystemAbstraction ) );
			  FileSystemAbstraction special = mock( typeof( FileSystemAbstraction ) );

			  // when
			  using ( SelectiveFileSystemAbstraction fs = new SelectiveFileSystemAbstraction( specialFile, special, normal ) )
			  {
					fs.Create( otherFile );
					fs.Open( otherFile, OpenMode.Read );

					// then
					verify( normal ).create( otherFile );
					verify( normal ).open( otherFile, OpenMode.Read );
					verifyNoMoreInteractions( special );
					verifyNoMoreInteractions( normal );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void provideSelectiveWatcher() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ProvideSelectiveWatcher()
		 {
			  File specialFile = new File( "special" );
			  File otherFile = new File( "other" );

			  FileSystemAbstraction normal = mock( typeof( FileSystemAbstraction ) );
			  FileSystemAbstraction special = mock( typeof( FileSystemAbstraction ) );

			  FileWatcher specialWatcher = mock( typeof( FileWatcher ) );
			  FileWatcher normalWatcher = mock( typeof( FileWatcher ) );
			  WatchedResource specialResource = mock( typeof( WatchedResource ) );
			  WatchedResource normalResource = mock( typeof( WatchedResource ) );

			  when( special.FileWatcher() ).thenReturn(specialWatcher);
			  when( normal.FileWatcher() ).thenReturn(normalWatcher);
			  when( specialWatcher.Watch( specialFile ) ).thenReturn( specialResource );
			  when( normalWatcher.Watch( otherFile ) ).thenReturn( normalResource );

			  using ( SelectiveFileSystemAbstraction fs = new SelectiveFileSystemAbstraction( specialFile, special, normal ) )
			  {
					FileWatcher fileWatcher = fs.FileWatcher();
					assertSame( specialResource, fileWatcher.Watch( specialFile ) );
					assertSame( normalResource, fileWatcher.Watch( otherFile ) );
			  }
		 }
	}

}
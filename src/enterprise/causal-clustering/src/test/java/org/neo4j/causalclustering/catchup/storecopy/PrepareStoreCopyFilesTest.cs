using System;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using NeoStoreFileIndexListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileIndexListing;
	using NeoStoreFileListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileListing;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.CALLS_REAL_METHODS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.fs.FileUtils.relativePath;

	public class PrepareStoreCopyFilesTest
	{
		private bool InstanceFieldsInitialized = false;

		public PrepareStoreCopyFilesTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDirectory = TestDirectory.testDirectory( _fileSystemAbstraction );
		}

		 private readonly FileSystemAbstraction _fileSystemAbstraction = new DefaultFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory(fileSystemAbstraction);
		 public TestDirectory TestDirectory;
		 private PrepareStoreCopyFiles _prepareStoreCopyFiles;
		 private NeoStoreFileIndexListing _indexListingMock;
		 private DatabaseLayout _databaseLayout;
		 private NeoStoreFileListing.StoreFileListingBuilder _fileListingBuilder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  _fileListingBuilder = mock( typeof( NeoStoreFileListing.StoreFileListingBuilder ), CALLS_REAL_METHODS );
			  _databaseLayout = TestDirectory.databaseLayout();
			  when( dataSource.DatabaseLayout ).thenReturn( _databaseLayout );
			  _indexListingMock = mock( typeof( NeoStoreFileIndexListing ) );
			  when( _indexListingMock.IndexIds ).thenReturn( new LongHashSet() );
			  NeoStoreFileListing storeFileListing = mock( typeof( NeoStoreFileListing ) );
			  when( storeFileListing.NeoStoreFileIndexListing ).thenReturn( _indexListingMock );
			  when( storeFileListing.Builder() ).thenReturn(_fileListingBuilder);
			  when( dataSource.NeoStoreFileListing ).thenReturn( storeFileListing );
			  _prepareStoreCopyFiles = new PrepareStoreCopyFiles( dataSource, _fileSystemAbstraction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHanldeEmptyListOfFilesForeEachType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHanldeEmptyListOfFilesForeEachType()
		 {
			  ExpectedFiles = new StoreFileMetadata[0];
			  File[] files = _prepareStoreCopyFiles.listReplayableFiles();
			  StoreResource[] atomicFilesSnapshot = _prepareStoreCopyFiles.AtomicFilesSnapshot;
			  assertEquals( 0, Files.Length );
			  assertEquals( 0, atomicFilesSnapshot.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setExpectedFiles(Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata[] expectedFiles) throws java.io.IOException
		 private StoreFileMetadata[] ExpectedFiles
		 {
			 set
			 {
				  doAnswer( invocation => Iterators.asResourceIterator( Iterators.iterator( value ) ) ).when( _fileListingBuilder ).build();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnExpectedListOfFileNamesForEachType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnExpectedListOfFileNamesForEachType()
		 {
			  // given
			  StoreFileMetadata[] expectedFiles = new StoreFileMetadata[]
			  {
				  new StoreFileMetadata( _databaseLayout.file( "a" ), 1 ),
				  new StoreFileMetadata( _databaseLayout.file( "b" ), 2 )
			  };
			  ExpectedFiles = expectedFiles;

			  //when
			  File[] files = _prepareStoreCopyFiles.listReplayableFiles();
			  StoreResource[] atomicFilesSnapshot = _prepareStoreCopyFiles.AtomicFilesSnapshot;

			  //then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  File[] expectedFilesConverted = java.util.expectedFiles.Select( StoreFileMetadata::file ).ToArray( File[]::new );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  StoreResource[] exeptedAtomicFilesConverted = java.util.expectedFiles.Select( f => new StoreResource( f.file(), GetRelativePath(f), f.recordSize(), _fileSystemAbstraction ) ).ToArray(StoreResource[]::new);
			  assertArrayEquals( expectedFilesConverted, files );
			  assertEquals( exeptedAtomicFilesConverted.Length, atomicFilesSnapshot.Length );
			  for ( int i = 0; i < exeptedAtomicFilesConverted.Length; i++ )
			  {
					StoreResource expected = exeptedAtomicFilesConverted[i];
					StoreResource storeResource = atomicFilesSnapshot[i];
					assertEquals( expected.Path(), storeResource.Path() );
					assertEquals( expected.RecordSize(), storeResource.RecordSize() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyDescriptors()
		 public virtual void ShouldHandleEmptyDescriptors()
		 {
			  LongSet indexIds = _prepareStoreCopyFiles.NonAtomicIndexIds;

			  assertEquals( 0, indexIds.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptySetOfIdsAndIgnoreIndexListing()
		 public virtual void ShouldReturnEmptySetOfIdsAndIgnoreIndexListing()
		 {
			  LongSet expectedIndexIds = LongSets.immutable.of( 42 );
			  when( _indexListingMock.IndexIds ).thenReturn( expectedIndexIds );

			  LongSet actualIndexIndexIds = _prepareStoreCopyFiles.NonAtomicIndexIds;

			  assertTrue( actualIndexIndexIds.Empty );
		 }

		 private string GetRelativePath( StoreFileMetadata f )
		 {
			  try
			  {
					return relativePath( _databaseLayout.databaseDirectory(), f.File() );
			  }
			  catch ( IOException )
			  {
					throw new Exception( "Failed to create relative path" );
			  }
		 }
	}

}
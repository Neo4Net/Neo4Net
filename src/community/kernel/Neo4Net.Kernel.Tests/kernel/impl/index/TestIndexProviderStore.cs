/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NotCurrentStoreVersionException = Neo4Net.Kernel.impl.store.NotCurrentStoreVersionException;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TestIndexProviderStore
	{
		 private File _file;
		 private FileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createStore()
		 public virtual void CreateStore()
		 {
			  _file = new File( "target/test-data/index-provider-store" );
			  _fileSystem = new DefaultFileSystemAbstraction();
			  _file.mkdirs();
			  _fileSystem.deleteFile( _file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fileSystem.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastCommitedTxGetsStoredBetweenSessions()
		 public virtual void LastCommitedTxGetsStoredBetweenSessions()
		 {
			  IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, 0, false );
			  store.Version = 5;
			  store.LastCommittedTx = 12;
			  store.Close();
			  store = new IndexProviderStore( _file, _fileSystem, 0, false );
			  assertEquals( 5, store.Version );
			  assertEquals( 12, store.LastCommittedTx );
			  store.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailUpgradeIfNotAllowed()
		 public virtual void ShouldFailUpgradeIfNotAllowed()
		 {
			  IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( "3.1" ), true );
			  store.Close();
			  store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( "3.1" ), false );
			  store.Close();
			  try
			  {
					new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( "3.5" ), false );
					fail( "Shouldn't be able to upgrade there" );
			  }
			  catch ( UpgradeNotAllowedByConfigurationException )
			  { // Good
			  }
			  store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( "3.5" ), true );
			  assertEquals( "3.5", MetaDataStore.versionLongToString( store.IndexVersion ) );
			  store.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.kernel.impl.store.NotCurrentStoreVersionException.class) public void shouldFailToGoBackToOlderVersion()
		 public virtual void ShouldFailToGoBackToOlderVersion()
		 {
			  string newerVersion = "3.5";
			  string olderVersion = "3.1";
			  try
			  {
					IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( newerVersion ), true );
					store.Close();
					store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( olderVersion ), false );
			  }
			  catch ( NotCurrentStoreVersionException e )
			  {
					assertTrue( e.Message.contains( newerVersion ) );
					assertTrue( e.Message.contains( olderVersion ) );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.kernel.impl.store.NotCurrentStoreVersionException.class) public void shouldFailToGoBackToOlderVersionEvenIfAllowUpgrade()
		 public virtual void ShouldFailToGoBackToOlderVersionEvenIfAllowUpgrade()
		 {
			  string newerVersion = "3.5";
			  string olderVersion = "3.1";
			  try
			  {
					IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( newerVersion ), true );
					store.Close();
					store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( olderVersion ), true );
			  }
			  catch ( NotCurrentStoreVersionException e )
			  {
					assertTrue( e.Message.contains( newerVersion ) );
					assertTrue( e.Message.contains( olderVersion ) );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void upgradeForMissingVersionRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpgradeForMissingVersionRecord()
		 {
			  // This was before 1.6.M02
			  IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, 0, false );
			  store.Close();
			  FileUtils.truncateFile( _file, 4 * 8 );
			  try
			  {
					store = new IndexProviderStore( _file, _fileSystem, 0, false );
					fail( "Should have thrown upgrade exception" );
			  }
			  catch ( UpgradeNotAllowedByConfigurationException )
			  { // Good
			  }

			  store = new IndexProviderStore( _file, _fileSystem, 0, true );
			  store.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceChannelAfterWritingMetadata() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForceChannelAfterWritingMetadata()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.io.fs.StoreChannel[] channelUsedToCreateFile = {null};
			  StoreChannel[] channelUsedToCreateFile = new StoreChannel[] { null };

			  FileSystemAbstraction fs = spy( _fileSystem );
			  StoreChannel tempChannel;
			  when( tempChannel = fs.Open( _file, OpenMode.READ_WRITE ) ).then(ignored =>
			  {
				StoreChannel channel = _fileSystem.open( _file, OpenMode.READ_WRITE );
				if ( channelUsedToCreateFile[0] == null )
				{
					 StoreChannel channelSpy = spy( channel );
					 channelUsedToCreateFile[0] = channelSpy;
					 channel = channelSpy;
				}
				return channel;
			  });

			  // Doing the FSA spying above, calling fs.open, actually invokes that method and so a channel
			  // is opened. We put that in tempChannel and close it before deleting the file below.
			  tempChannel.close();
			  fs.DeleteFile( _file );

			  // When
			  IndexProviderStore store = new IndexProviderStore( _file, fs, MetaDataStore.versionStringToLong( "3.5" ), false );

			  // Then
			  StoreChannel channel = channelUsedToCreateFile[0];
			  verify( channel ).writeAll( any( typeof( ByteBuffer ) ), eq( 0L ) );
			  verify( channel ).force( true );
			  verify( channel ).close();
			  verifyNoMoreInteractions( channel );
			  store.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowWhenTryingToCreateFileThatAlreadyExists()
		 public virtual void ShouldThrowWhenTryingToCreateFileThatAlreadyExists()
		 {
			  // Given
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  when( fs.FileExists( _file ) ).thenReturn( false ).thenReturn( true );
			  when( fs.GetFileSize( _file ) ).thenReturn( 42L );

			  // When
			  new IndexProviderStore( _file, fs, MetaDataStore.versionStringToLong( "3.5" ), false );

			  // Then
			  // exception is thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteNewFileWhenExistingFileHasZeroLength() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteNewFileWhenExistingFileHasZeroLength()
		 {
			  // Given
			  _file.createNewFile();

			  // When
			  IndexProviderStore store = new IndexProviderStore( _file, _fileSystem, MetaDataStore.versionStringToLong( "3.5" ), false );

			  // Then
			  assertTrue( _fileSystem.getFileSize( _file ) > 0 );
			  store.Close();
		 }
	}

}
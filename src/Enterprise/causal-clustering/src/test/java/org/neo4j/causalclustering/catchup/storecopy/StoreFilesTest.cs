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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StoreFilesTest
	{
		 protected internal TestDirectory TestDirectory;
		 protected internal System.Func<FileSystemAbstraction> FileSystemRule;
		 protected internal PageCacheRule PageCacheRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules;
		 public RuleChain Rules;

		 private FileSystemAbstraction _fs;
		 private PageCache _pageCache;
		 private StoreFiles _storeFiles;
		 private LogFiles _logFiles;

		 public StoreFilesTest()
		 {
			  CreateRules();
		 }

		 protected internal virtual void CreateRules()
		 {
			  TestDirectory = TestDirectory.testDirectory();
			  EphemeralFileSystemRule ephemeralFileSystemRule = new EphemeralFileSystemRule();
			  FileSystemRule = ephemeralFileSystemRule;
			  PageCacheRule = new PageCacheRule();
			  Rules = RuleChain.outerRule( ephemeralFileSystemRule ).around( TestDirectory ).around( PageCacheRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _fs = FileSystemRule.get();
			  _pageCache = PageCacheRule.getPageCache( _fs );
			  _storeFiles = new StoreFiles( _fs, _pageCache );
			  _logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( TestDirectory.directory(), _fs ).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createOnFileSystem(java.io.File file) throws java.io.IOException
		 private void CreateOnFileSystem( File file )
		 {
			  CreateFile( _fs, file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file) throws java.io.IOException
		 private void CreateFile( FileSystemAbstraction fs, File file )
		 {
			  fs.Mkdirs( file.ParentFile );
			  fs.Open( file, OpenMode.READ_WRITE ).close();
		 }

		 protected internal virtual File BaseDir
		 {
			 get
			 {
				  return new File( TestDirectory.directory(), "dir" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteMustRecursivelyRemoveFilesInGivenDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteMustRecursivelyRemoveFilesInGivenDirectory()
		 {
			  File dir = BaseDir;
			  File a = new File( dir, "a" );
			  File b = new File( dir, "b" );

			  CreateOnFileSystem( a );
			  assertTrue( _fs.fileExists( a ) );
			  assertFalse( _fs.fileExists( b ) );

			  _storeFiles.delete( dir, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteMustNotDeleteIgnoredFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteMustNotDeleteIgnoredFiles()
		 {
			  File dir = BaseDir;
			  File a = new File( dir, "a" );
			  File c = new File( dir, "c" );

			  CreateOnFileSystem( a );
			  CreateOnFileSystem( c );

			  FilenameFilter filter = ( directory, name ) => !name.Equals( "c" ) && !name.Equals( "d" );
			  _storeFiles = new StoreFiles( _fs, _pageCache, filter );
			  _storeFiles.delete( dir, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
			  assertTrue( _fs.fileExists( c ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteMustNotDeleteFilesInIgnoredDirectories() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteMustNotDeleteFilesInIgnoredDirectories()
		 {
			  File dir = BaseDir;
			  File ignore = new File( dir, "ignore" );
			  File a = new File( dir, "a" );
			  File c = new File( ignore, "c" );

			  CreateOnFileSystem( a );
			  CreateOnFileSystem( c );

			  FilenameFilter filter = ( directory, name ) => !name.StartsWith( "ignore" );
			  _storeFiles = new StoreFiles( _fs, _pageCache, filter );
			  _storeFiles.delete( dir, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
			  assertTrue( _fs.fileExists( c ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteMustSilentlyIgnoreMissingDirectories() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteMustSilentlyIgnoreMissingDirectories()
		 {
			  File dir = BaseDir;
			  File sub = new File( dir, "sub" );

			  _storeFiles.delete( sub, _logFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustMoveFilesToTargetDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustMoveFilesToTargetDirectory()
		 {
			  File @base = BaseDir;
			  File src = new File( @base, "src" );
			  File tgt = new File( @base, "tgt" );
			  File a = new File( src, "a" );

			  CreateOnFileSystem( a );

			  // Ensure the 'tgt' directory exists
			  CreateOnFileSystem( new File( tgt, ".fs-ignore" ) );

			  _storeFiles.moveTo( src, tgt, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
			  assertTrue( _fs.fileExists( new File( tgt, "a" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void movedFilesMustRetainTheirRelativePaths() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MovedFilesMustRetainTheirRelativePaths()
		 {
			  File @base = BaseDir;
			  File src = new File( @base, "src" );
			  File tgt = new File( @base, "tgt" );
			  File dir = new File( src, "dir" );
			  File a = new File( dir, "a" );

			  CreateOnFileSystem( a );

			  // Ensure the 'tgt' directory exists
			  CreateOnFileSystem( new File( tgt, ".fs-ignore" ) );

			  _storeFiles.moveTo( src, tgt, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
			  assertTrue( _fs.fileExists( new File( new File( tgt, "dir" ), "a" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveMustIgnoreFilesFilteredOut() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveMustIgnoreFilesFilteredOut()
		 {
			  File @base = BaseDir;
			  File src = new File( @base, "src" );
			  File a = new File( src, "a" );
			  File ignore = new File( src, "ignore" );
			  File c = new File( ignore, "c" );
			  File tgt = new File( @base, "tgt" );

			  CreateOnFileSystem( a );
			  CreateOnFileSystem( c );

			  // Ensure the 'tgt' directory exists
			  CreateOnFileSystem( new File( tgt, ".fs-ignore" ) );

			  FilenameFilter filter = ( directory, name ) => !name.StartsWith( "ignore" );
			  _storeFiles = new StoreFiles( _fs, _pageCache, filter );
			  _storeFiles.moveTo( src, tgt, _logFiles );

			  assertFalse( _fs.fileExists( a ) );
			  assertTrue( _fs.fileExists( c ) );
			  assertTrue( _fs.fileExists( new File( tgt, "a" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isEmptyMustFindFilesBothOnFileSystemAndPageCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isEmptyMustFindFilesBothOnFileSystemAndPageCache()
		 {
			  File dir = BaseDir;
			  File ignore = new File( dir, "ignore" );
			  File a = new File( dir, "a" );
			  File c = new File( dir, "c" );

			  CreateOnFileSystem( a );
			  CreateOnFileSystem( c );
			  CreateOnFileSystem( ignore );

			  FilenameFilter filter = ( directory, name ) => !name.StartsWith( "ignore" );
			  _storeFiles = new StoreFiles( _fs, _pageCache, filter );

			  IList<File> filesOnFilesystem = new IList<File> { a, c };
			  IList<File> fileOnFilesystem = singletonList( a );
			  IList<File> ignoredList = singletonList( ignore );

			  assertFalse( _storeFiles.isEmpty( dir, filesOnFilesystem ) );
			  assertFalse( _storeFiles.isEmpty( dir, fileOnFilesystem ) );
			  assertTrue( _storeFiles.isEmpty( dir, Collections.emptyList() ) );
			  assertTrue( _storeFiles.isEmpty( dir, ignoredList ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReadStoreId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReadStoreId()
		 {
			  File dir = BaseDir;
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout( dir );
			  File neostore = databaseLayout.MetadataStore();
			  ThreadLocalRandom rng = ThreadLocalRandom.current();
			  long time = rng.nextLong();
			  long randomNumber = rng.nextLong();
			  long upgradeTime = rng.nextLong();
			  long upgradeTransactionId = rng.nextLong();

			  CreateOnFileSystem( neostore );

			  MetaDataStore.setRecord( _pageCache, neostore, MetaDataStore.Position.TIME, time );
			  MetaDataStore.setRecord( _pageCache, neostore, MetaDataStore.Position.RANDOM_NUMBER, randomNumber );
			  MetaDataStore.setRecord( _pageCache, neostore, MetaDataStore.Position.STORE_VERSION, rng.nextLong() );
			  MetaDataStore.setRecord( _pageCache, neostore, MetaDataStore.Position.UPGRADE_TIME, upgradeTime );
			  MetaDataStore.setRecord( _pageCache, neostore, MetaDataStore.Position.UPGRADE_TRANSACTION_ID, upgradeTransactionId );

			  StoreId storeId = _storeFiles.readStoreId( databaseLayout );

			  assertThat( storeId.CreationTime, @is( time ) );
			  assertThat( storeId.RandomId, @is( randomNumber ) );
			  assertThat( storeId.UpgradeTime, @is( upgradeTime ) );
			  assertThat( storeId.UpgradeId, @is( upgradeTransactionId ) );
		 }
	}

}
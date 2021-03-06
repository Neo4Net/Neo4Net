﻿/*
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
namespace Org.Neo4j.Commandline.dbms
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using CommandLocator = Org.Neo4j.Commandline.admin.CommandLocator;
	using Usage = Org.Neo4j.Commandline.admin.Usage;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using Org.Neo4j.Test.mockito.matcher;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;

	public class StoreInfoCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();

		 private Path _databaseDirectory;
		 private ArgumentCaptor<string> _outCaptor;
		 private StoreInfoCommand _command;
		 private System.Action<string> @out;
		 private DatabaseLayout _databaseLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  Path homeDir = TestDirectory.directory( "home-dir" ).toPath();
			  _databaseDirectory = homeDir.resolve( "data/databases/foo.db" );
			  _databaseLayout = DatabaseLayout.of( _databaseDirectory.toFile() );
			  Files.createDirectories( _databaseDirectory );

			  _outCaptor = ArgumentCaptor.forClass( typeof( string ) );
			  @out = mock( typeof( System.Action ) );
			  _command = new StoreInfoCommand( @out );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintNiceHelp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );
					Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new StoreInfoCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: neo4j-admin store-info --store=<path-to-dir>%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Prints information about a Neo4j database store, such as what version of Neo4j%n" + "created it. Note that this command expects a path to a store directory, for%n" + "example --store=data/databases/graph.db.%n" + "%n" + "options:%n" + "  --store=<path-to-dir>   Path to database store.%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noArgFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoArgFails()
		 {
			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "Missing argument 'store'" );

			  _command.execute( new string[]{} );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyArgFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EmptyArgFails()
		 {
			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "Missing argument 'store'" );

			  _command.execute( new string[]{ "--store=" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonExistingDatabaseShouldThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NonExistingDatabaseShouldThrow()
		 {
			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "does not contain a database" );

			  Execute( Paths.get( "yaba", "daba", "doo" ).ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readsLatestStoreVersionCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadsLatestStoreVersionCorrectly()
		 {
			  RecordFormats currentFormat = RecordFormatSelector.defaultFormat();
			  PrepareNeoStoreFile( currentFormat.StoreVersion() );

			  Execute( _databaseDirectory.ToString() );

			  verify( @out, times( 2 ) ).accept( _outCaptor.capture() );

			  assertEquals( Arrays.asList( string.Format( "Store format version:         {0}", currentFormat.StoreVersion() ), string.Format("Store format introduced in:   {0}", currentFormat.IntroductionVersion()) ), _outCaptor.AllValues );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readsOlderStoreVersionCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadsOlderStoreVersionCorrectly()
		 {
			  PrepareNeoStoreFile( StandardV2_3.RECORD_FORMATS.storeVersion() );

			  Execute( _databaseDirectory.ToString() );

			  verify( @out, times( 3 ) ).accept( _outCaptor.capture() );

			  assertEquals( Arrays.asList( "Store format version:         v0.A.6", "Store format introduced in:   2.3.0", "Store format superseded in:   3.0.0" ), _outCaptor.AllValues );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsOnUnknownVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowsOnUnknownVersion()
		 {
			  PrepareNeoStoreFile( "v9.9.9" );

			  Expected.expect( new RootCauseMatcher( typeof( System.ArgumentException ) ) );
			  Expected.expectMessage( "Unknown store version 'v9.9.9'" );

			  Execute( _databaseDirectory.ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute(String storePath) throws Exception
		 private void Execute( string storePath )
		 {
			  _command.execute( new string[]{ "--store=" + storePath } );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareNeoStoreFile(String storeVersion) throws java.io.IOException
		 private void PrepareNeoStoreFile( string storeVersion )
		 {
			  File neoStoreFile = CreateNeoStoreFile();
			  long value = MetaDataStore.versionStringToLong( storeVersion );
			  using ( PageCache pageCache = PageCacheRule.getPageCache( FsRule.get() ) )
			  {
					MetaDataStore.setRecord( pageCache, neoStoreFile, STORE_VERSION, value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNeoStoreFile() throws java.io.IOException
		 private File CreateNeoStoreFile()
		 {
			  File neoStoreFile = _databaseLayout.metadataStore();
			  FsRule.get().create(neoStoreFile).close();
			  return neoStoreFile;
		 }
	}

}
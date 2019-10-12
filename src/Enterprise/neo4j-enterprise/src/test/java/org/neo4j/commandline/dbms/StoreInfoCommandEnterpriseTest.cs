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
namespace Neo4Net.Commandline.dbms
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using HighLimitV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

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

	public class StoreInfoCommandEnterpriseTest
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  Path homeDir = TestDirectory.directory( "home-dir" ).toPath();
			  _databaseDirectory = homeDir.resolve( "data/databases/foo.db" );
			  Files.createDirectories( _databaseDirectory );
			  _outCaptor = ArgumentCaptor.forClass( typeof( string ) );
			  @out = mock( typeof( System.Action ) );
			  _command = new StoreInfoCommand( @out );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readsEnterpriseStoreVersionCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadsEnterpriseStoreVersionCorrectly()
		 {
			  PrepareNeoStoreFile( HighLimitV3_0_0.RECORD_FORMATS.storeVersion() );

			  Execute( _databaseDirectory.ToString() );

			  verify( @out, times( 3 ) ).accept( _outCaptor.capture() );

			  assertEquals( Arrays.asList( "Store format version:         vE.H.0", "Store format introduced in:   3.0.0", "Store format superseded in:   3.0.6" ), _outCaptor.AllValues );
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
			  MetaDataStore.setRecord( PageCacheRule.getPageCache( FsRule.get() ), neoStoreFile, STORE_VERSION, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNeoStoreFile() throws java.io.IOException
		 private File CreateNeoStoreFile()
		 {
			  FsRule.get().mkdir(_databaseDirectory.toFile());
			  File neoStoreFile = DatabaseLayout.of( _databaseDirectory.toFile() ).metadataStore();
			  FsRule.get().create(neoStoreFile).close();
			  return neoStoreFile;
		 }
	}

}
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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CommitStateHelperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private Config _config;
		 private CommitStateHelper _commitStateHelper;
		 private DatabaseLayout _databaseLayout;
		 private FileSystemAbstraction _fsa;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  File txLogLocation = new File( TestDirectory.directory(), "txLogLocation" );
			  _config = Config.builder().withSetting(GraphDatabaseSettings.logical_logs_location, txLogLocation.AbsolutePath).build();
			  File storeDir = TestDirectory.storeDir();
			  _databaseLayout = DatabaseLayout.of( storeDir, _config.get( GraphDatabaseSettings.active_database ) );
			  _fsa = TestDirectory.FileSystem;
			  _commitStateHelper = new CommitStateHelper( PageCacheRule.getPageCache( _fsa ), _fsa, _config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveTxLogsIfDirectoryDoesNotExist() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveTxLogsIfDirectoryDoesNotExist()
		 {
			  File txDir = _config.get( GraphDatabaseSettings.logical_logs_location );
			  assertFalse( txDir.exists() );
			  assertFalse( _commitStateHelper.hasTxLogs( _databaseLayout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveTxLogsIfDirectoryIsEmpty() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveTxLogsIfDirectoryIsEmpty()
		 {
			  File txDir = _config.get( GraphDatabaseSettings.logical_logs_location );
			  _fsa.mkdir( txDir );

			  assertFalse( _commitStateHelper.hasTxLogs( _databaseLayout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveTxLogsIfDirectoryHasFilesWithIncorrectName() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveTxLogsIfDirectoryHasFilesWithIncorrectName()
		 {
			  File txDir = _config.get( GraphDatabaseSettings.logical_logs_location );
			  _fsa.mkdir( txDir );

			  _fsa.create( new File( txDir, "foo.bar" ) ).close();

			  assertFalse( _commitStateHelper.hasTxLogs( _databaseLayout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveTxLogsIfDirectoryHasTxFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveTxLogsIfDirectoryHasTxFile()
		 {
			  File txDir = _config.get( GraphDatabaseSettings.logical_logs_location );
			  _fsa.mkdir( txDir );
			  _fsa.create( new File( txDir, TransactionLogFiles.DEFAULT_NAME + ".0" ) ).close();

			  assertTrue( _commitStateHelper.hasTxLogs( _databaseLayout ) );
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecoveryRequiredChecker = Neo4Net.Kernel.impl.recovery.RecoveryRequiredChecker;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestStoreAccess
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private readonly Monitors _monitors = new Monitors();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void openingThroughStoreAccessShouldNotTriggerRecovery() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OpeningThroughStoreAccessShouldNotTriggerRecovery()
		 {
			  using ( EphemeralFileSystemAbstraction snapshot = ProduceUncleanStore() )
			  {
					assertTrue( "Store should be unclean", IsUnclean( snapshot ) );

					PageCache pageCache = PageCacheRule.getPageCache( snapshot );
					( new StoreAccess( snapshot, pageCache, TestDirectory.databaseLayout(), Config.defaults() ) ).initialize().close();
					assertTrue( "Store should be unclean", IsUnclean( snapshot ) );
			  }
		 }

		 private EphemeralFileSystemAbstraction ProduceUncleanStore()
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabase(TestDirectory.databaseDir());
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  EphemeralFileSystemAbstraction snapshot = Fs.get().snapshot();
			  Db.shutdown();
			  return snapshot;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isUnclean(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem) throws java.io.IOException
		 private bool IsUnclean( FileSystemAbstraction fileSystem )
		 {
			  PageCache pageCache = PageCacheRule.getPageCache( fileSystem );
			  RecoveryRequiredChecker requiredChecker = new RecoveryRequiredChecker( fileSystem, pageCache, Config.defaults(), _monitors );
			  return requiredChecker.IsRecoveryRequiredAt( TestDirectory.databaseLayout() );
		 }
	}

}
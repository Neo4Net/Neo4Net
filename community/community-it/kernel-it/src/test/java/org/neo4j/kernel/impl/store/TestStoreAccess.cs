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
namespace Org.Neo4j.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecoveryRequiredChecker = Org.Neo4j.Kernel.impl.recovery.RecoveryRequiredChecker;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestStoreAccess
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
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
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabase(TestDirectory.databaseDir());
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
//ORIGINAL LINE: private boolean isUnclean(org.neo4j.io.fs.FileSystemAbstraction fileSystem) throws java.io.IOException
		 private bool IsUnclean( FileSystemAbstraction fileSystem )
		 {
			  PageCache pageCache = PageCacheRule.getPageCache( fileSystem );
			  RecoveryRequiredChecker requiredChecker = new RecoveryRequiredChecker( fileSystem, pageCache, Config.defaults(), _monitors );
			  return requiredChecker.IsRecoveryRequiredAt( TestDirectory.databaseLayout() );
		 }
	}

}
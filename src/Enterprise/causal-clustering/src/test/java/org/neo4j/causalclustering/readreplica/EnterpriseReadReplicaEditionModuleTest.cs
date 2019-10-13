﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.readreplica
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using PageCacheWarmer = Neo4Net.Kernel.impl.pagecache.PageCacheWarmer;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class EnterpriseReadReplicaEditionModuleTest
	internal class EnterpriseReadReplicaEditionModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileWatcherFileNameFilter()
		 internal virtual void FileWatcherFileNameFilter()
		 {
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  System.Predicate<string> filter = EnterpriseReadReplicaEditionModule.FileWatcherFileNameFilter();
			  string metadataStoreName = databaseLayout.MetadataStore().Name;

			  assertFalse( filter( metadataStoreName ) );
			  assertFalse( filter( databaseLayout.NodeStore().Name ) );
			  assertTrue( filter( TransactionLogFiles.DEFAULT_NAME + ".1" ) );
			  assertTrue( filter( IndexConfigStore.INDEX_DB_FILE_NAME + ".any" ) );
			  assertTrue( filter( StoreUtil.BRANCH_SUBDIRECTORY ) );
			  assertTrue( filter( StoreUtil.TEMP_COPY_DIRECTORY_NAME ) );
			  assertTrue( filter( metadataStoreName + PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }
	}

}
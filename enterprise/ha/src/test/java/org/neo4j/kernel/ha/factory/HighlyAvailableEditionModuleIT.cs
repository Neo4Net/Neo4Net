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
namespace Org.Neo4j.Kernel.ha.factory
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using StoreUtil = Org.Neo4j.com.storecopy.StoreUtil;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class HighlyAvailableEditionModuleIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createBufferedIdComponentsByDefault()
		 public virtual void CreateBufferedIdComponentsByDefault()
		 {
			  ClusterManager.ManagedCluster managedCluster = ClusterRule.startCluster();
			  DependencyResolver dependencyResolver = managedCluster.Master.DependencyResolver;

			  IdController idController = dependencyResolver.ResolveDependency( typeof( IdController ) );
			  IdGeneratorFactory idGeneratorFactory = dependencyResolver.ResolveDependency( typeof( IdGeneratorFactory ) );

			  assertThat( idController, instanceOf( typeof( BufferedIdController ) ) );
			  assertThat( idGeneratorFactory, instanceOf( typeof( BufferingIdGeneratorFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fileWatcherFileNameFilter()
		 public virtual void FileWatcherFileNameFilter()
		 {
			  DatabaseLayout databaseLayout = ClusterRule.TestDirectory.databaseLayout();
			  System.Predicate<string> filter = HighlyAvailableEditionModule.FileWatcherFileNameFilter();
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
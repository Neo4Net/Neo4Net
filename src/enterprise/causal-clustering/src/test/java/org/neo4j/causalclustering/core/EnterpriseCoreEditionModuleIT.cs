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
namespace Neo4Net.causalclustering.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using FreeIdFilteredIdGeneratorFactory = Neo4Net.causalclustering.core.state.machines.id.FreeIdFilteredIdGeneratorFactory;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using PageCacheWarmer = Neo4Net.Kernel.impl.pagecache.PageCacheWarmer;
	using BufferedIdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class EnterpriseCoreEditionModuleIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.causalclustering.ClusterRule clusterRule = new Neo4Net.test.causalclustering.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createBufferedIdComponentsByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateBufferedIdComponentsByDefault()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  CoreClusterMember leader = cluster.AwaitLeader();
			  DependencyResolver dependencyResolver = leader.Database().DependencyResolver;

			  IdController idController = dependencyResolver.ResolveDependency( typeof( IdController ) );
			  IdGeneratorFactory idGeneratorFactory = dependencyResolver.ResolveDependency( typeof( IdGeneratorFactory ) );

			  assertThat( idController, instanceOf( typeof( BufferedIdController ) ) );
			  assertThat( idGeneratorFactory, instanceOf( typeof( FreeIdFilteredIdGeneratorFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fileWatcherFileNameFilter()
		 public virtual void FileWatcherFileNameFilter()
		 {
			  DatabaseLayout layout = ClusterRule.testDirectory().databaseLayout();
			  System.Predicate<string> filter = EnterpriseCoreEditionModule.FileWatcherFileNameFilter();
			  string metadataStoreName = layout.MetadataStore().Name;
			  assertFalse( filter( metadataStoreName ) );
			  assertFalse( filter( layout.NodeStore().Name ) );
			  assertTrue( filter( TransactionLogFiles.DEFAULT_NAME + ".1" ) );
			  assertTrue( filter( IndexConfigStore.INDEX_DB_FILE_NAME + ".any" ) );
			  assertTrue( filter( StoreUtil.TEMP_COPY_DIRECTORY_NAME ) );
			  assertTrue( filter( metadataStoreName + PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }
	}

}
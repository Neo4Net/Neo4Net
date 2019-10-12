/*
 *
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Modifications Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Reference: https://github.com/neo4j/neo4j/blob/3.4/enterprise/causal-clustering/src/test/java/org/neo4j/causalclustering/core/EnterpriseCoreEditionModuleIT.java
 * https://github.com/neo4j/neo4j/blob/fc43ec9b0d751b2a1654866a6cd1927268294351/community/neo4j/src/test/java/org/neo4j/graphdb/factory/module/edition/CommunityEditionModuleIntegrationTest.java
 */

namespace Org.Neo4j.causalclustering.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using FreeIdFilteredIdGeneratorFactory = Org.Neo4j.causalclustering.core.state.machines.id.FreeIdFilteredIdGeneratorFactory;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	//import org.neo4j.kernel.impl.store.StoreFile;
	//import org.neo4j.kernel.impl.storemigration.StoreFileType;
	//import static org.junit.Assert.assertFalse;
	//import static org.junit.Assert.assertThat;
	//import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public class OpenEnterpriseCoreEditionModuleIT
	public class OpenEnterpriseCoreEditionModuleIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createBufferedIdComponentsByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateBufferedIdComponentsByDefault()
		 {
			  Cluster cluster = ClusterRule.startCluster();
			  CoreClusterMember leader = cluster.awaitLeader();
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
			  DatabaseLayout layout = _testDirectory.databaseLayout();

			  System.Predicate<string> filter = EnterpriseCoreEditionModule.FileWatcherFileNameFilter();
			  assertFalse( filter( layout.MetadataStore().Name ) );
			  assertFalse( filter( layout.NodeStore().Name ) );
			  assertTrue( filter( TransactionLogFiles.DEFAULT_NAME + ".1" ) );
			  assertTrue( filter( IndexConfigStore.INDEX_DB_FILE_NAME + ".any" ) );
			  assertTrue( filter( layout.MetadataStore().Name + PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }
	}

}
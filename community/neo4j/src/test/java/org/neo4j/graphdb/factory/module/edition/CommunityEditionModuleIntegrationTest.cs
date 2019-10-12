/*
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
namespace Org.Neo4j.Graphdb.factory.module.edition
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class CommunityEditionModuleIntegrationTest
	internal class CommunityEditionModuleIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createBufferedIdComponentsByDefault()
		 internal virtual void CreateBufferedIdComponentsByDefault()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) ( new GraphDatabaseFactory() ).newEmbeddedDatabase(_testDirectory.storeDir());
			  try
			  {
					DependencyResolver dependencyResolver = database.DependencyResolver;
					IdController idController = dependencyResolver.ResolveDependency( typeof( IdController ) );
					IdGeneratorFactory idGeneratorFactory = dependencyResolver.ResolveDependency( typeof( IdGeneratorFactory ) );

					assertThat( idController, instanceOf( typeof( BufferedIdController ) ) );
					assertThat( idGeneratorFactory, instanceOf( typeof( BufferingIdGeneratorFactory ) ) );
			  }
			  finally
			  {
					database.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileWatcherFileNameFilter()
		 internal virtual void FileWatcherFileNameFilter()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  System.Predicate<string> filter = CommunityEditionModule.CommunityFileWatcherFileNameFilter();
			  assertFalse( filter( layout.MetadataStore().Name ) );
			  assertFalse( filter( layout.NodeStore().Name ) );
			  assertTrue( filter( TransactionLogFiles.DEFAULT_NAME + ".1" ) );
			  assertTrue( filter( IndexConfigStore.INDEX_DB_FILE_NAME + ".any" ) );
		 }

	}

}
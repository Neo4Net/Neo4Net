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
namespace Neo4Net.GraphDb.factory.module.edition
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using BufferedIdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

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
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDirectory;
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
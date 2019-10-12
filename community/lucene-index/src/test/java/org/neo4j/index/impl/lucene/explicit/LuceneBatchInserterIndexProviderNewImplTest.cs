using System.Collections.Generic;

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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using LuceneIndexProviderFactory = Org.Neo4j.Kernel.Api.Impl.Schema.LuceneIndexProviderFactory;
	using Org.Neo4j.Kernel.extension;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using BatchInserter = Org.Neo4j.@unsafe.Batchinsert.BatchInserter;
	using BatchInserters = Org.Neo4j.@unsafe.Batchinsert.BatchInserters;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneBatchInserterIndexProviderNewImplTest
	internal class LuceneBatchInserterIndexProviderNewImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createBatchIndexFromAnyIndexStoreProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreateBatchIndexFromAnyIndexStoreProvider()
		 {
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir ) );
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir, _fileSystem ) );
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir, Config ) );
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir, ConfigWithProvider, Extensions ) );
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir, _fileSystem, Config ) );
			  CreateEndCloseIndexProvider( BatchInserters.inserter( StoreDir, _fileSystem, ConfigWithProvider, Extensions ) );
		 }

		 private static void CreateEndCloseIndexProvider( BatchInserter inserter )
		 {
			  LuceneBatchInserterIndexProviderNewImpl provider = new LuceneBatchInserterIndexProviderNewImpl( inserter );
			  provider.Shutdown();
			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getExtensions()
		 private static IEnumerable<KernelExtensionFactory<object>> Extensions
		 {
			 get
			 {
				  return Iterables.asIterable( new LuceneIndexProviderFactory() );
			 }
		 }

		 private static IDictionary<string, string> ConfigWithProvider
		 {
			 get
			 {
				  return GetConfig( GraphDatabaseSettings.default_schema_provider.name(), LuceneIndexProviderFactory.PROVIDER_DESCRIPTOR.name() );
			 }
		 }

		 private static IDictionary<string, string> GetConfig( params string[] entries )
		 {
			  return MapUtil.stringMap( entries );
		 }

		 private File StoreDir
		 {
			 get
			 {
				  return _testDirectory.databaseDir();
			 }
		 }
	}

}
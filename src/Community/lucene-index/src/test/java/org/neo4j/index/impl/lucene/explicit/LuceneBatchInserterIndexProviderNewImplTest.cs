using System.Collections.Generic;

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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using LuceneIndexProviderFactory = Neo4Net.Kernel.Api.Impl.Schema.LuceneIndexProviderFactory;
	using Neo4Net.Kernel.extension;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserters = Neo4Net.@unsafe.Batchinsert.BatchInserters;

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
﻿/*
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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.forSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneIndexProviderTest
	internal class LuceneIndexProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 private File _graphDbDir;
		 private static readonly StoreIndexDescriptor _descriptor = forSchema( forLabel( 1, 1 ), PROVIDER_DESCRIPTOR ).withId( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  _graphDbDir = _testDir.databaseDir();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailToInvokePopulatorInReadOnlyMode()
		 internal virtual void ShouldFailToInvokePopulatorInReadOnlyMode()
		 {
			  Config readOnlyConfig = Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE );
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );
			  assertThrows( typeof( System.NotSupportedException ), () => readOnlyIndexProvider.GetPopulator(_descriptor, new IndexSamplingConfig(readOnlyConfig), heapBufferFactory(1024)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateReadOnlyAccessorInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateReadOnlyAccessorInReadOnlyMode()
		 {
			  DirectoryFactory directoryFactory = DirectoryFactory.PERSISTENT;
			  CreateEmptySchemaIndex( directoryFactory );

			  Config readOnlyConfig = Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE );
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, directoryFactory, _fileSystem, _graphDbDir );
			  IndexAccessor onlineAccessor = GetIndexAccessor( readOnlyConfig, readOnlyIndexProvider );

			  assertThrows( typeof( System.NotSupportedException ), onlineAccessor.drop );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexUpdateNotAllowedInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void IndexUpdateNotAllowedInReadOnlyMode()
		 {
			  Config readOnlyConfig = Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE );
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );

			  assertThrows( typeof( System.NotSupportedException ), () => GetIndexAccessor(readOnlyConfig, readOnlyIndexProvider).newUpdater(IndexUpdateMode.ONLINE) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexForceMustBeAllowedInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void IndexForceMustBeAllowedInReadOnlyMode()
		 {
			  // IndexAccessor.force is used in check-pointing, and must be allowed in read-only mode as it would otherwise
			  // prevent backups from working.
			  Config readOnlyConfig = Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE );
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );

			  // We assert that 'force' does not throw an exception
			  GetIndexAccessor( readOnlyConfig, readOnlyIndexProvider ).force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createEmptySchemaIndex(Neo4Net.kernel.api.impl.index.storage.DirectoryFactory directoryFactory) throws java.io.IOException
		 private void CreateEmptySchemaIndex( DirectoryFactory directoryFactory )
		 {
			  Config config = Config.defaults();
			  LuceneIndexProvider indexProvider = GetLuceneIndexProvider( config, directoryFactory, _fileSystem, _graphDbDir );
			  IndexAccessor onlineAccessor = GetIndexAccessor( config, indexProvider );
			  onlineAccessor.Dispose();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.kernel.api.index.IndexAccessor getIndexAccessor(Neo4Net.kernel.configuration.Config readOnlyConfig, LuceneIndexProvider indexProvider) throws java.io.IOException
		 private IndexAccessor GetIndexAccessor( Config readOnlyConfig, LuceneIndexProvider indexProvider )
		 {
			  return indexProvider.GetOnlineAccessor( _descriptor, new IndexSamplingConfig( readOnlyConfig ) );
		 }

		 private LuceneIndexProvider GetLuceneIndexProvider( Config config, DirectoryFactory directoryFactory, FileSystemAbstraction fs, File graphDbDir )
		 {
			  return new LuceneIndexProvider( fs, directoryFactory, defaultDirectoryStructure( graphDbDir ), IndexProvider.Monitor_Fields.EMPTY, config, OperationalMode.single );
		 }
	}

}
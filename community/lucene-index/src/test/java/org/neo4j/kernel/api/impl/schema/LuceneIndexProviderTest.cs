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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneIndexProviderTest
	internal class LuceneIndexProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
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
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );
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
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );

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
			  LuceneIndexProvider readOnlyIndexProvider = GetLuceneIndexProvider( readOnlyConfig, new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory(), _fileSystem, _graphDbDir );

			  // We assert that 'force' does not throw an exception
			  GetIndexAccessor( readOnlyConfig, readOnlyIndexProvider ).force( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createEmptySchemaIndex(org.neo4j.kernel.api.impl.index.storage.DirectoryFactory directoryFactory) throws java.io.IOException
		 private void CreateEmptySchemaIndex( DirectoryFactory directoryFactory )
		 {
			  Config config = Config.defaults();
			  LuceneIndexProvider indexProvider = GetLuceneIndexProvider( config, directoryFactory, _fileSystem, _graphDbDir );
			  IndexAccessor onlineAccessor = GetIndexAccessor( config, indexProvider );
			  onlineAccessor.Dispose();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.api.index.IndexAccessor getIndexAccessor(org.neo4j.kernel.configuration.Config readOnlyConfig, LuceneIndexProvider indexProvider) throws java.io.IOException
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
using System;

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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using CorruptIndexException = Org.Apache.Lucene.Index.CorruptIndexException;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexStorageFactory = Neo4Net.Kernel.Api.Impl.Index.storage.IndexStorageFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using LoggingMonitor = Neo4Net.Kernel.Api.Index.LoggingMonitor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexCorruptionTest
	internal class LuceneSchemaIndexCorruptionTest
	{
		private bool InstanceFieldsInitialized = false;

		public LuceneSchemaIndexCorruptionTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_monitor = new LoggingMonitor( _logProvider.getLog( "test" ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private IndexProvider.Monitor _monitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRequestIndexPopulationIfTheIndexIsCorrupt()
		 internal virtual void ShouldRequestIndexPopulationIfTheIndexIsCorrupt()
		 {
			  // Given
			  long faultyIndexId = 1;
			  CorruptIndexException error = new CorruptIndexException( "It's broken.", "" );

			  LuceneIndexProvider provider = NewFaultyIndexProvider( faultyIndexId, error );

			  // When
			  StoreIndexDescriptor descriptor = forSchema( forLabel( 1, 1 ), provider.ProviderDescriptor ).withId( faultyIndexId );
			  InternalIndexState initialState = provider.GetInitialState( descriptor );

			  // Then
			  assertThat( initialState, equalTo( InternalIndexState.POPULATING ) );
			  _logProvider.assertAtLeastOnce( LoggedException( error ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRequestIndexPopulationFailingWithFileNotFoundException()
		 internal virtual void ShouldRequestIndexPopulationFailingWithFileNotFoundException()
		 {
			  // Given
			  long faultyIndexId = 1;
			  FileNotFoundException error = new FileNotFoundException( "/some/path/somewhere" );

			  LuceneIndexProvider provider = NewFaultyIndexProvider( faultyIndexId, error );

			  // When
			  StoreIndexDescriptor descriptor = forSchema( forLabel( 1, 1 ), provider.ProviderDescriptor ).withId( faultyIndexId );
			  InternalIndexState initialState = provider.GetInitialState( descriptor );

			  // Then
			  assertThat( initialState, equalTo( InternalIndexState.POPULATING ) );
			  _logProvider.assertAtLeastOnce( LoggedException( error ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRequestIndexPopulationWhenFailingWithEOFException()
		 internal virtual void ShouldRequestIndexPopulationWhenFailingWithEOFException()
		 {
			  // Given
			  long faultyIndexId = 1;
			  EOFException error = new EOFException( "/some/path/somewhere" );

			  LuceneIndexProvider provider = NewFaultyIndexProvider( faultyIndexId, error );

			  // When
			  StoreIndexDescriptor descriptor = forSchema( forLabel( 1, 1 ), provider.ProviderDescriptor ).withId( faultyIndexId );
			  InternalIndexState initialState = provider.GetInitialState( descriptor );

			  // Then
			  assertThat( initialState, equalTo( InternalIndexState.POPULATING ) );
			  _logProvider.assertAtLeastOnce( LoggedException( error ) );
		 }

		 private LuceneIndexProvider NewFaultyIndexProvider( long faultyIndexId, Exception error )
		 {
			  DirectoryFactory directoryFactory = mock( typeof( DirectoryFactory ) );
			  File indexRootFolder = _testDirectory.databaseDir();
			  AtomicReference<FaultyIndexStorageFactory> reference = new AtomicReference<FaultyIndexStorageFactory>();
			  return new LuceneIndexProviderAnonymousInnerClass( this, _fs, directoryFactory, defaultDirectoryStructure( indexRootFolder ), _monitor, Config.defaults(), faultyIndexId, error, reference );
		 }

		 private class LuceneIndexProviderAnonymousInnerClass : LuceneIndexProvider
		 {
			 private readonly LuceneSchemaIndexCorruptionTest _outerInstance;

			 private long _faultyIndexId;
			 private Exception _error;
			 private DirectoryFactory _directoryFactory;
			 private AtomicReference<FaultyIndexStorageFactory> _reference;

			 public LuceneIndexProviderAnonymousInnerClass( LuceneSchemaIndexCorruptionTest outerInstance, EphemeralFileSystemAbstraction fs, DirectoryFactory directoryFactory, UnknownType defaultDirectoryStructure, IndexProvider.Monitor monitor, Config defaults, long faultyIndexId, Exception error, AtomicReference<FaultyIndexStorageFactory> reference ) : base( fs, directoryFactory, defaultDirectoryStructure, monitor, defaults, OperationalMode.single )
			 {
				 this.outerInstance = outerInstance;
				 this._faultyIndexId = faultyIndexId;
				 this._error = error;
				 this._directoryFactory = directoryFactory;
				 this._reference = reference;
			 }

			 protected internal override IndexStorageFactory buildIndexStorageFactory( FileSystemAbstraction fileSystem, DirectoryFactory directoryFactory )
			 {
				  FaultyIndexStorageFactory storageFactory = new FaultyIndexStorageFactory( _outerInstance, _faultyIndexId, _error, directoryFactory, directoryStructure() );
				  _reference.set( storageFactory );
				  return storageFactory;
			 }
		 }

		 private class FaultyIndexStorageFactory : IndexStorageFactory
		 {
			 private readonly LuceneSchemaIndexCorruptionTest _outerInstance;

			  internal readonly long FaultyIndexId;
			  internal readonly Exception Error;

			  internal FaultyIndexStorageFactory( LuceneSchemaIndexCorruptionTest outerInstance, long faultyIndexId, Exception error, DirectoryFactory directoryFactory, IndexDirectoryStructure directoryStructure ) : base( directoryFactory, outerInstance.fs, directoryStructure )
			  {
				  this._outerInstance = outerInstance;
					this.FaultyIndexId = faultyIndexId;
					this.Error = error;
			  }

			  public override PartitionedIndexStorage IndexStorageOf( long indexId )
			  {
					return indexId == FaultyIndexId ? NewFaultyPartitionedIndexStorage() : base.IndexStorageOf(indexId);
			  }

			  internal virtual PartitionedIndexStorage NewFaultyPartitionedIndexStorage()
			  {
					try
					{
						 PartitionedIndexStorage storage = mock( typeof( PartitionedIndexStorage ) );
						 when( storage.ListFolders() ).thenReturn(singletonList(new File("/some/path/somewhere/1")));
						 when( storage.OpenDirectory( any() ) ).thenThrow(Error);
						 return storage;
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
		 }

		 private static AssertableLogProvider.LogMatcher LoggedException( Exception exception )
		 {
			  return inLog( CoreMatchers.any( typeof( string ) ) ).error( CoreMatchers.any( typeof( string ) ), sameInstance( exception ) );
		 }
	}

}
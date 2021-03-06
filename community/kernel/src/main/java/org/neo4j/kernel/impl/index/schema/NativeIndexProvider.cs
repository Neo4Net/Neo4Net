﻿using System;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using MetadataMismatchException = Org.Neo4j.Index.@internal.gbptree.MetadataMismatchException;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using Factory = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure.Factory;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using StoreMigrationParticipant = Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Base class for native indexes on top of <seealso cref="GBPTree"/>.
	/// </summary>
	/// @param <KEY> type of <seealso cref="NativeIndexSingleValueKey"/> </param>
	/// @param <VALUE> type of <seealso cref="NativeIndexValue"/> </param>
	/// @param <LAYOUT> type of <seealso cref="IndexLayout"/> </param>
	internal abstract class NativeIndexProvider<KEY, VALUE, LAYOUT> : IndexProvider where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue where LAYOUT : IndexLayout<KEY,VALUE>
	{
		 protected internal readonly PageCache PageCache;
		 protected internal readonly FileSystemAbstraction Fs;
		 protected internal readonly Monitor Monitor;
		 protected internal readonly RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector;
		 protected internal readonly bool ReadOnly;

		 protected internal NativeIndexProvider( IndexProviderDescriptor descriptor, Factory directoryStructureFactory, PageCache pageCache, FileSystemAbstraction fs, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly ) : base( descriptor, directoryStructureFactory )
		 {
			  this.PageCache = pageCache;
			  this.Fs = fs;
			  this.Monitor = monitor;
			  this.RecoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
			  this.ReadOnly = readOnly;
		 }

		 /// <summary>
		 /// Instantiates the <seealso cref="Layout"/> which is used in the index backing this native index provider.
		 /// </summary>
		 /// <param name="descriptor"> the <seealso cref="StoreIndexDescriptor"/> for this index. </param>
		 /// <param name="storeFile"> index store file, since some layouts may depend on contents of the header.
		 /// If {@code null} it means that nothing must be read from the file before or while instantiating the layout. </param>
		 /// <returns> the correct <seealso cref="Layout"/> for the index. </returns>
		 internal abstract LAYOUT Layout( StoreIndexDescriptor descriptor, File storeFile );

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  if ( ReadOnly )
			  {
					throw new System.NotSupportedException( "Can't create populator for read only index" );
			  }

			  File storeFile = NativeIndexFileFromIndexId( descriptor.Id );
			  return NewIndexPopulator( storeFile, Layout( descriptor, null ), descriptor, bufferFactory );
		 }

		 protected internal abstract IndexPopulator NewIndexPopulator( File storeFile, LAYOUT layout, StoreIndexDescriptor descriptor, ByteBufferFactory bufferFactory );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  File storeFile = NativeIndexFileFromIndexId( descriptor.Id );
			  return NewIndexAccessor( storeFile, Layout( descriptor, storeFile ), descriptor, ReadOnly );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.neo4j.kernel.api.index.IndexAccessor newIndexAccessor(java.io.File storeFile, LAYOUT layout, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, boolean readOnly) throws java.io.IOException;
		 protected internal abstract IndexAccessor NewIndexAccessor( File storeFile, LAYOUT layout, StoreIndexDescriptor descriptor, bool readOnly );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  try
			  {
					string failureMessage = NativeIndexes.ReadFailureMessage( PageCache, NativeIndexFileFromIndexId( descriptor.Id ) );
					if ( string.ReferenceEquals( failureMessage, null ) )
					{
						 throw new System.InvalidOperationException( "Index " + descriptor.Id + " isn't failed" );
					}
					return failureMessage;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  try
			  {
					return NativeIndexes.ReadState( PageCache, NativeIndexFileFromIndexId( descriptor.Id ) );
			  }
			  catch ( Exception e ) when ( e is MetadataMismatchException || e is IOException )
			  {
					Monitor.failedToOpenIndex( descriptor, "Requesting re-population.", e );
					return InternalIndexState.POPULATING;
			  }
		 }

		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  // Since this native provider is a new one, there's no need for migration on this level.
			  // Migration should happen in the combined layer for the time being.
			  return Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
		 }

		 private File NativeIndexFileFromIndexId( long indexId )
		 {
			  return new File( directoryStructure().directoryForIndex(indexId), IndexFileName(indexId) );
		 }

		 private static string IndexFileName( long indexId )
		 {
			  return "index-" + indexId;
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;


	using Resource = Neo4Net.Graphdb.Resource;
	using Neo4Net.Graphdb;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Neo4Net.Kernel.impl.store.format;
	using MultiResource = Neo4Net.Kernel.impl.util.MultiResource;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.resourceIterator;

	public class NeoStoreFileIndexListing
	{
		 private readonly LabelScanStore _labelScanStore;
		 private readonly IndexingService _indexingService;
		 private readonly ExplicitIndexProvider _explicitIndexProviders;

		 private static readonly System.Func<File, StoreFileMetadata> _toStoreFileMetatadata = file => new StoreFileMetadata( file, Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE );

		 internal NeoStoreFileIndexListing( LabelScanStore labelScanStore, IndexingService indexingService, ExplicitIndexProvider explicitIndexProviders )
		 {
			  this._labelScanStore = labelScanStore;
			  this._indexingService = indexingService;
			  this._explicitIndexProviders = explicitIndexProviders;
		 }

		 public virtual LongSet IndexIds
		 {
			 get
			 {
				  return _indexingService.IndexIds;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.Resource gatherSchemaIndexFiles(java.util.Collection<org.neo4j.storageengine.api.StoreFileMetadata> targetFiles) throws java.io.IOException
		 internal virtual Resource GatherSchemaIndexFiles( ICollection<StoreFileMetadata> targetFiles )
		 {
			  ResourceIterator<File> snapshot = _indexingService.snapshotIndexFiles();
			  GetSnapshotFilesMetadata( snapshot, targetFiles );
			  // Intentionally don't close the snapshot here, return it for closing by the consumer of
			  // the targetFiles list.
			  return snapshot;
		 }

		 internal virtual Resource GatherLabelScanStoreFiles( ICollection<StoreFileMetadata> targetFiles )
		 {
			  ResourceIterator<File> snapshot = _labelScanStore.snapshotStoreFiles();
			  GetSnapshotFilesMetadata( snapshot, targetFiles );
			  // Intentionally don't close the snapshot here, return it for closing by the consumer of
			  // the targetFiles list.
			  return snapshot;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.Resource gatherExplicitIndexFiles(java.util.Collection<org.neo4j.storageengine.api.StoreFileMetadata> files) throws java.io.IOException
		 internal virtual Resource GatherExplicitIndexFiles( ICollection<StoreFileMetadata> files )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.neo4j.graphdb.ResourceIterator<java.io.File>> snapshots = new java.util.ArrayList<>();
			  ICollection<ResourceIterator<File>> snapshots = new List<ResourceIterator<File>>();
			  foreach ( IndexImplementation indexProvider in _explicitIndexProviders.allIndexProviders() )
			  {
					ResourceIterator<File> snapshot = indexProvider.ListStoreFiles();
					snapshots.Add( snapshot );
					GetSnapshotFilesMetadata( snapshot, files );
			  }
			  // Intentionally don't close the snapshot here, return it for closing by the consumer of
			  // the targetFiles list.
			  return new MultiResource( snapshots );
		 }

		 private void GetSnapshotFilesMetadata( ResourceIterator<File> snapshot, ICollection<StoreFileMetadata> targetFiles )
		 {
			  snapshot.Select( _toStoreFileMetatadata ).ForEach( targetFiles.add );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<org.neo4j.storageengine.api.StoreFileMetadata> getSnapshot(long indexId) throws java.io.IOException
		 public virtual ResourceIterator<StoreFileMetadata> GetSnapshot( long indexId )
		 {
			  try
			  {
					ResourceIterator<File> snapshot = _indexingService.getIndexProxy( indexId ).snapshotFiles();
					List<StoreFileMetadata> files = new List<StoreFileMetadata>();
					GetSnapshotFilesMetadata( snapshot, files );
					return resourceIterator( Files.GetEnumerator(), snapshot );
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					// Perhaps it got dropped after getIndexIds() was called.
					return Iterators.emptyResourceIterator();
			  }
		 }
	}

}
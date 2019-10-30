using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using IndexReaderFactory = Neo4Net.Kernel.Impl.Api.IndexReaderFactory;
	using IndexProxy = Neo4Net.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using SchemaCache = Neo4Net.Kernel.Impl.Api.store.SchemaCache;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using Register = Neo4Net.Register.Register;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using Neo4Net.Kernel.Api.StorageEngine;
	using StoragePropertyCursor = Neo4Net.Kernel.Api.StorageEngine.StoragePropertyCursor;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using StorageRelationshipGroupCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipGroupCursor;
	using StorageRelationshipTraversalCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipTraversalCursor;
	using StorageSchemaReader = Neo4Net.Kernel.Api.StorageEngine.StorageSchemaReader;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using LabelScanReader = Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.register.Registers.newDoubleLongRegister;

	/// <summary>
	/// Default implementation of StorageReader. Delegates to NeoStores and indexes.
	/// </summary>
	public class RecordStorageReader : StorageReader
	{
		 // These token holders should perhaps move to the cache layer.. not really any reason to have them here?
		 private readonly TokenHolders _tokenHolders;
		 private readonly IndexingService _indexService;
		 private readonly NeoStores _neoStores;
		 private readonly NodeStore _nodeStore;
		 private readonly RelationshipStore _relationshipStore;
		 private readonly RelationshipGroupStore _relationshipGroupStore;
		 private readonly PropertyStore _propertyStore;
		 private readonly SchemaStorage _schemaStorage;
		 private readonly CountsTracker _counts;
		 private readonly SchemaCache _schemaCache;

		 private readonly System.Func<IndexReaderFactory> _indexReaderFactorySupplier;
		 private readonly System.Func<LabelScanReader> _labelScanReaderSupplier;
		 private readonly RecordStorageCommandCreationContext _commandCreationContext;

		 private IndexReaderFactory _indexReaderFactory;
		 private LabelScanReader _labelScanReader;

		 private bool _acquired;
		 private bool _closed;

		 internal RecordStorageReader( TokenHolders tokenHolders, SchemaStorage schemaStorage, NeoStores neoStores, IndexingService indexService, SchemaCache schemaCache, System.Func<IndexReaderFactory> indexReaderFactory, System.Func<LabelScanReader> labelScanReaderSupplier, RecordStorageCommandCreationContext commandCreationContext )
		 {
			  this._tokenHolders = tokenHolders;
			  this._neoStores = neoStores;
			  this._schemaStorage = schemaStorage;
			  this._indexService = indexService;
			  this._nodeStore = neoStores.NodeStore;
			  this._relationshipStore = neoStores.RelationshipStore;
			  this._relationshipGroupStore = neoStores.RelationshipGroupStore;
			  this._propertyStore = neoStores.PropertyStore;
			  this._counts = neoStores.Counts;
			  this._schemaCache = schemaCache;
			  this._indexReaderFactorySupplier = indexReaderFactory;
			  this._labelScanReaderSupplier = labelScanReaderSupplier;
			  this._commandCreationContext = commandCreationContext;
		 }

		 /// <summary>
		 /// All the nulls in this method is a testament to the fact that we probably need to break apart this reader,
		 /// separating index stuff out from store stuff.
		 /// </summary>
		 public RecordStorageReader( NeoStores stores ) : this( null, null, stores, null, null, null, null, null )
		 {
		 }

		 public override PrimitiveLongResourceIterator NodesGetForLabel( int labelId )
		 {
			  return LabelScanReader.nodesWithLabel( labelId );
		 }

		 public override CapableIndexDescriptor IndexGetForSchema( SchemaDescriptor descriptor )
		 {
			  return _schemaCache.indexDescriptor( descriptor );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForLabel( int labelId )
		 {
			  return _schemaCache.indexDescriptorsForLabel( labelId );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForRelationshipType( int relationshipType )
		 {
			  return _schemaCache.indexDescriptorsForRelationshipType( relationshipType );
		 }

		 public override CapableIndexDescriptor IndexGetForName( string name )
		 {
			  return _schemaCache.indexDescriptorForName( name );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetAll()
		 {
			  return _schemaCache.indexDescriptors().GetEnumerator();
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetRelatedToProperty( int propertyId )
		 {
			  return _schemaCache.indexesByProperty( propertyId );
		 }

		 public override long? IndexGetOwningUniquenessConstraintId( IndexDescriptor index )
		 {
			  StoreIndexDescriptor storeIndexDescriptor = GetStoreIndexDescriptor( index );
			  if ( storeIndexDescriptor != null )
			  {
					// Think of the index as being orphaned if the owning constraint is missing or broken.
					long? owningConstraint = storeIndexDescriptor.OwningConstraint;
					return _schemaCache.hasConstraintRule( owningConstraint ) ? owningConstraint : null;
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.InternalIndexState indexGetState(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override InternalIndexState IndexGetState( IndexDescriptor descriptor )
		 {
			  return _indexService.getIndexProxy( descriptor.Schema() ).State;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.IndexReference indexReference(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override IndexReference IndexReference( IndexDescriptor descriptor )
		 {
			  IndexProxy indexProxy = _indexService.getIndexProxy( descriptor.Schema() );
			  return indexProxy.Descriptor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress indexGetPopulationProgress(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override PopulationProgress IndexGetPopulationProgress( SchemaDescriptor descriptor )
		 {
			  return _indexService.getIndexProxy( descriptor ).IndexPopulationProgress;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long indexSize(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override long IndexSize( SchemaDescriptor descriptor )
		 {
			  Register_DoubleLongRegister result = _indexService.indexUpdatesAndSize( descriptor );
			  return result.ReadSecond();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double indexUniqueValuesPercentage(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override double IndexUniqueValuesPercentage( SchemaDescriptor descriptor )
		 {
			  return _indexService.indexUniqueValuesPercentage( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String indexGetFailure(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override string IndexGetFailure( SchemaDescriptor descriptor )
		 {
			  return _indexService.getIndexProxy( descriptor ).PopulationFailure.asString();
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor )
		 {
			  return _schemaCache.constraintsForSchema( descriptor );
		 }

		 public override bool ConstraintExists( ConstraintDescriptor descriptor )
		 {
			  return _schemaCache.hasConstraintRule( descriptor );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId )
		 {
			  return _schemaCache.constraintsForLabel( labelId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId )
		 {
			  return _schemaCache.constraintsForRelationshipType( typeId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetAll()
		 {
			  return _schemaCache.constraints();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Exception> void relationshipVisit(long relationshipId, org.Neo4Net.Kernel.Api.StorageEngine.RelationshipVisitor<EXCEPTION> relationshipVisitor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException, EXCEPTION
		 public override void RelationshipVisit<EXCEPTION>( long relationshipId, RelationshipVisitor<EXCEPTION> relationshipVisitor ) where EXCEPTION : Exception
		 {
			  // TODO Please don't create a record for this, it's ridiculous
			  RelationshipRecord record = _relationshipStore.getRecord( relationshipId, _relationshipStore.newRecord(), CHECK );
			  if ( !record.InUse() )
			  {
					throw new IEntityNotFoundException( EntityType.RELATIONSHIP, relationshipId );
			  }
			  relationshipVisitor.Visit( relationshipId, record.Type, record.FirstNode, record.SecondNode );
		 }

		 public override void ReleaseNode( long id )
		 {
			  _nodeStore.freeId( id );
		 }

		 public override void ReleaseRelationship( long id )
		 {
			  _relationshipStore.freeId( id );
		 }

		 public override long CountsForNode( int labelId )
		 {
			  return _counts.nodeCount( labelId, newDoubleLongRegister() ).readSecond();
		 }

		 public override long CountsForRelationship( int startLabelId, int typeId, int endLabelId )
		 {
			  if ( !( startLabelId == StatementConstants.ANY_LABEL || endLabelId == StatementConstants.ANY_LABEL ) )
			  {
					throw new System.NotSupportedException( "not implemented" );
			  }
			  return _counts.relationshipCount( startLabelId, typeId, endLabelId, newDoubleLongRegister() ).readSecond();
		 }

		 public override long NodesGetCount()
		 {
			  return _nodeStore.NumberOfIdsInUse;
		 }

		 public override long RelationshipsGetCount()
		 {
			  return _relationshipStore.NumberOfIdsInUse;
		 }

		 public override int LabelCount()
		 {
			  return _tokenHolders.labelTokens().size();
		 }

		 public override int PropertyKeyCount()
		 {
			  return _tokenHolders.propertyKeyTokens().size();
		 }

		 public override int RelationshipTypeCount()
		 {
			  return _tokenHolders.relationshipTypeTokens().size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.register.Register_DoubleLongRegister indexUpdatesAndSize(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override Register_DoubleLongRegister IndexUpdatesAndSize( SchemaDescriptor descriptor, Register_DoubleLongRegister target )
		 {
			  return _counts.indexUpdatesAndSize( TryGetIndexId( descriptor ), target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.register.Register_DoubleLongRegister indexSample(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override Register_DoubleLongRegister IndexSample( SchemaDescriptor descriptor, Register_DoubleLongRegister target )
		 {
			  return _counts.indexSample( TryGetIndexId( descriptor ), target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long tryGetIndexId(org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 private long TryGetIndexId( SchemaDescriptor descriptor )
		 {
			  return _indexService.getIndexId( descriptor );
		 }

		 public override bool NodeExists( long id )
		 {
			  return _nodeStore.isInUse( id );
		 }

		 public override bool RelationshipExists( long id )
		 {
			  return _relationshipStore.isInUse( id );
		 }

		 private StoreIndexDescriptor GetStoreIndexDescriptor( IndexDescriptor index )
		 {
			  foreach ( StoreIndexDescriptor descriptor in _schemaCache.indexDescriptors() )
			  {
					if ( descriptor.Equals( index ) )
					{
						 return descriptor;
					}
			  }

			  return _schemaStorage.indexGetForSchema( index );
		 }

		 public override T GetOrCreateSchemaDependantState<T>( Type type, System.Func<StorageReader, T> factory )
		 {
				 type = typeof( T );
			  return _schemaCache.getOrCreateDependantState( type, factory, this );
		 }

		 public override void Acquire()
		 {
			  Debug.Assert( !_closed );
			  Debug.Assert( !_acquired );
			  this._acquired = true;
		 }

		 public override void Release()
		 {
			  Debug.Assert( !_closed );
			  Debug.Assert( _acquired );
			  CloseSchemaResources();
			  _acquired = false;
		 }

		 public override void Close()
		 {
			  Debug.Assert( !_closed );
			  CloseSchemaResources();
			  if ( _commandCreationContext != null )
			  {
					_commandCreationContext.close();
			  }
			  _closed = true;
		 }

		 private void CloseSchemaResources()
		 {
			  if ( _indexReaderFactory != null )
			  {
					_indexReaderFactory.close();
					// we can actually keep this object around
			  }
			  if ( _labelScanReader != null )
			  {
					_labelScanReader.close();
					_labelScanReader = null;
			  }
		 }

		 public virtual LabelScanReader LabelScanReader
		 {
			 get
			 {
				  return _labelScanReader != null ? _labelScanReader : ( _labelScanReader = _labelScanReaderSupplier.get() );
			 }
		 }

		 private IndexReaderFactory IndexReaderFactory()
		 {
			  return _indexReaderFactory != null ? _indexReaderFactory : ( _indexReaderFactory = _indexReaderFactorySupplier.get() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader getIndexReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override IndexReader GetIndexReader( IndexDescriptor descriptor )
		 {
			  return IndexReaderFactory().newReader(descriptor);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader getFreshIndexReader(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public override IndexReader GetFreshIndexReader( IndexDescriptor descriptor )
		 {
			  return IndexReaderFactory().newUnCachedReader(descriptor);
		 }

		 internal virtual RecordStorageCommandCreationContext CommandCreationContext
		 {
			 get
			 {
				  return _commandCreationContext;
			 }
		 }

		 public override long ReserveNode()
		 {
			  return _commandCreationContext.nextId( StoreType.NODE );
		 }

		 public override long ReserveRelationship()
		 {
			  return _commandCreationContext.nextId( StoreType.RELATIONSHIP );
		 }

		 public override int ReserveRelationshipTypeTokenId()
		 {
			  return toIntExact( _neoStores.RelationshipTypeTokenStore.nextId() );
		 }

		 public override int ReservePropertyKeyTokenId()
		 {
			  return toIntExact( _neoStores.PropertyKeyTokenStore.nextId() );
		 }

		 public override int ReserveLabelTokenId()
		 {
			  return toIntExact( _neoStores.LabelTokenStore.nextId() );
		 }

		 public virtual long GraphPropertyReference
		 {
			 get
			 {
				  return _neoStores.MetaDataStore.GraphNextProp;
			 }
		 }

		 public override RecordNodeCursor AllocateNodeCursor()
		 {
			  return new RecordNodeCursor( _nodeStore );
		 }

		 public override StorageRelationshipGroupCursor AllocateRelationshipGroupCursor()
		 {
			  return new RecordRelationshipGroupCursor( _relationshipStore, _relationshipGroupStore );
		 }

		 public override StorageRelationshipTraversalCursor AllocateRelationshipTraversalCursor()
		 {
			  return new RecordRelationshipTraversalCursor( _relationshipStore, _relationshipGroupStore );
		 }

		 public override RecordRelationshipScanCursor AllocateRelationshipScanCursor()
		 {
			  return new RecordRelationshipScanCursor( _relationshipStore );
		 }

		 public override StorageSchemaReader SchemaSnapshot()
		 {
			  return new StorageSchemaReaderSnapshot( _schemaCache.snapshot(), this );
		 }

		 public override StoragePropertyCursor AllocatePropertyCursor()
		 {
			  return new RecordPropertyCursor( _propertyStore );
		 }
	}

}
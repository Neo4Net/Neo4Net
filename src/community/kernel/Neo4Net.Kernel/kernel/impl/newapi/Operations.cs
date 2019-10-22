using System;
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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using ExplicitIndexRead = Neo4Net.Internal.Kernel.Api.ExplicitIndexRead;
	using ExplicitIndexWrite = Neo4Net.Internal.Kernel.Api.ExplicitIndexWrite;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using Locks = Neo4Net.Internal.Kernel.Api.Locks;
	using NodeLabelIndexCursor = Neo4Net.Internal.Kernel.Api.NodeLabelIndexCursor;
	using Procedures = Neo4Net.Internal.Kernel.Api.Procedures;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using Token = Neo4Net.Internal.Kernel.Api.Token;
	using Write = Neo4Net.Internal.Kernel.Api.Write;
	using IEntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using AutoIndexingKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using ExplicitIndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using IndexNotApplicableKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using AlreadyConstrainedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using AlreadyIndexedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyIndexedException;
	using DropConstraintFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropConstraintFailureException;
	using DropIndexFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropIndexFailureException;
	using IndexBelongsToConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.IndexBelongsToConstraintException;
	using IndexBrokenKernelException = Neo4Net.Kernel.Api.Exceptions.schema.IndexBrokenKernelException;
	using NoSuchConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.NoSuchConstraintException;
	using NoSuchIndexException = Neo4Net.Kernel.Api.Exceptions.schema.NoSuchIndexException;
	using RepeatedLabelInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedLabelInSchemaException;
	using RepeatedPropertyInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedPropertyInSchemaException;
	using RepeatedRelationshipTypeInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedRelationshipTypeInSchemaException;
	using RepeatedSchemaComponentException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedSchemaComponentException;
	using UnableToValidateConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.UnableToValidateConstraintException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.exceptions.schema.ConstraintValidationException.Phase.VALIDATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.exceptions.schema.SchemaKernelException.OperationContext.CONSTRAINT_CREATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.SchemaDescriptor.schemaTokenLockingIds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.NO_SUCH_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.NO_SUCH_NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.NO_SUCH_PROPERTY_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.INDEX_ENTRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.indexEntryResourceId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.newapi.IndexTxStateUpdater.LabelChangeType.ADDED_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.newapi.IndexTxStateUpdater.LabelChangeType.REMOVED_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.storageengine.api.EntityType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Collects all Kernel API operations and guards them from being used outside of transaction.
	/// 
	/// Many methods assume cursors to be initialized before use in private methods, even if they're not passed in explicitly.
	/// Keep that in mind: e.g. nodeCursor, propertyCursor and relationshipCursor
	/// </summary>
	public class Operations : Write, ExplicitIndexWrite, SchemaWrite
	{
		 private static readonly int[] _emptyIntArray = new int[0];

		 private readonly KernelTransactionImplementation _ktx;
		 private readonly AllStoreHolder _allStoreHolder;
		 private readonly KernelToken _token;
		 private readonly StorageReader _statement;
		 private readonly AutoIndexing _autoIndexing;
		 private readonly IndexTxStateUpdater _updater;
		 private readonly DefaultCursors _cursors;
		 private readonly ConstraintIndexCreator _constraintIndexCreator;
		 private readonly ConstraintSemantics _constraintSemantics;
		 private readonly IndexingService _indexingService;
		 private readonly Config _config;
		 private DefaultNodeCursor _nodeCursor;
		 private DefaultPropertyCursor _propertyCursor;
		 private DefaultRelationshipScanCursor _relationshipCursor;

		 public Operations( AllStoreHolder allStoreHolder, IndexTxStateUpdater updater, StorageReader statement, KernelTransactionImplementation ktx, KernelToken token, DefaultCursors cursors, AutoIndexing autoIndexing, ConstraintIndexCreator constraintIndexCreator, ConstraintSemantics constraintSemantics, IndexingService indexingService, Config config )
		 {
			  this._token = token;
			  this._autoIndexing = autoIndexing;
			  this._allStoreHolder = allStoreHolder;
			  this._ktx = ktx;
			  this._statement = statement;
			  this._updater = updater;
			  this._cursors = cursors;
			  this._constraintIndexCreator = constraintIndexCreator;
			  this._constraintSemantics = constraintSemantics;
			  this._indexingService = indexingService;
			  this._config = config;
		 }

		 public virtual void Initialize()
		 {
			  this._nodeCursor = _cursors.allocateNodeCursor();
			  this._propertyCursor = _cursors.allocatePropertyCursor();
			  this._relationshipCursor = _cursors.allocateRelationshipScanCursor();
		 }

		 public override long NodeCreate()
		 {
			  _ktx.assertOpen();
			  long nodeId = _statement.reserveNode();
			  _ktx.txState().nodeDoCreate(nodeId);
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long nodeCreateWithLabels(int[] labels) throws org.Neo4Net.internal.kernel.api.exceptions.schema.ConstraintValidationException
		 public override long NodeCreateWithLabels( int[] labels )
		 {
			  if ( labels == null || labels.Length == 0 )
			  {
					return NodeCreate();
			  }

			  // We don't need to check the node for existence, like we do in nodeAddLabel, because we just created it.
			  // We also don't need to check if the node already has some of the labels, because we know it has none.
			  // And we don't need to take the exclusive lock on the node, because it was created in this transaction and
			  // isn't visible to anyone else yet.
			  _ktx.assertOpen();
			  long[] lockingIds = SchemaDescriptor.schemaTokenLockingIds( labels );
			  Arrays.sort( lockingIds ); // Sort to ensure labels are locked and assigned in order.
			  _ktx.statementLocks().optimistic().acquireShared(_ktx.lockTracer(), ResourceTypes.LABEL, lockingIds);
			  long nodeId = _statement.reserveNode();
			  _ktx.txState().nodeDoCreate(nodeId);
			  _nodeCursor.single( nodeId, _allStoreHolder );
			  _nodeCursor.next();

			  int prevLabel = NO_SUCH_LABEL;
			  foreach ( long lockingId in lockingIds )
			  {
					int label = ( int ) lockingId;
					if ( label != prevLabel ) // Filter out duplicates.
					{
						 CheckConstraintsAndAddLabelToNode( nodeId, label );
						 prevLabel = label;
					}
			  }
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean nodeDelete(long node) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override bool NodeDelete( long node )
		 {
			  _ktx.assertOpen();
			  return NodeDelete( node, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int nodeDetachDelete(final long nodeId) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override int NodeDetachDelete( long nodeId )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt count = new org.apache.commons.lang3.mutable.MutableInt();
			  MutableInt count = new MutableInt();
			  TwoPhaseNodeForRelationshipLocking locking = new TwoPhaseNodeForRelationshipLocking(relId =>
			  {
						  _ktx.assertOpen();
						  if ( RelationshipDelete( relId, false ) )
						  {
								count.increment();
						  }
			  }, _ktx.statementLocks().optimistic(), _ktx.lockTracer());

			  locking.LockAllNodesAndConsumeRelationships( nodeId, _ktx, _ktx.ambientNodeCursor() );
			  _ktx.assertOpen();

			  //we are already holding the lock
			  NodeDelete( nodeId, false );
			  return count.intValue();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long relationshipCreate(long sourceNode, int relationshipType, long targetNode) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 public override long RelationshipCreate( long sourceNode, int relationshipType, long targetNode )
		 {
			  _ktx.assertOpen();

			  SharedSchemaLock( ResourceTypes.RELATIONSHIP_TYPE, relationshipType );
			  LockRelationshipNodes( sourceNode, targetNode );

			  AssertNodeExists( sourceNode );
			  AssertNodeExists( targetNode );

			  long id = _statement.reserveRelationship();
			  _ktx.txState().relationshipDoCreate(id, relationshipType, sourceNode, targetNode);
			  return id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean relationshipDelete(long relationship) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override bool RelationshipDelete( long relationship )
		 {
			  _ktx.assertOpen();
			  return RelationshipDelete( relationship, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean nodeAddLabel(long node, int nodeLabel) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.schema.ConstraintValidationException
		 public override bool NodeAddLabel( long node, int nodeLabel )
		 {
			  SharedSchemaLock( ResourceTypes.LABEL, nodeLabel );
			  AcquireExclusiveNodeLock( node );

			  SingleNode( node );

			  if ( _nodeCursor.hasLabel( nodeLabel ) )
			  {
					//label already there, nothing to do
					return false;
			  }

			  CheckConstraintsAndAddLabelToNode( node, nodeLabel );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkConstraintsAndAddLabelToNode(long node, int nodeLabel) throws org.Neo4Net.kernel.api.exceptions.schema.UniquePropertyValueValidationException, org.Neo4Net.kernel.api.exceptions.schema.UnableToValidateConstraintException
		 private void CheckConstraintsAndAddLabelToNode( long node, int nodeLabel )
		 {
			  // Load the property key id list for this node. We may need it for constraint validation if there are any related constraints,
			  // but regardless we need it for tx state updating
			  int[] existingPropertyKeyIds = LoadSortedPropertyKeyList();

			  //Check so that we are not breaking uniqueness constraints
			  //We do this by checking if there is an existing node in the index that
			  //with the same label and property combination.
			  if ( existingPropertyKeyIds.Length > 0 )
			  {
					foreach ( IndexBackedConstraintDescriptor uniquenessConstraint in _indexingService.getRelatedUniquenessConstraints( new long[]{ nodeLabel }, existingPropertyKeyIds, NODE ) )
					{
						 IndexQuery.ExactPredicate[] propertyValues = GetAllPropertyValues( uniquenessConstraint.Schema(), StatementConstants.NO_SUCH_PROPERTY_KEY, Values.NO_VALUE );
						 if ( propertyValues != null )
						 {
							  ValidateNoExistingNodeWithExactValues( uniquenessConstraint, propertyValues, node );
						 }
					}
			  }

			  //node is there and doesn't already have the label, let's add
			  _ktx.txState().nodeDoAddLabel(nodeLabel, node);
			  _updater.onLabelChange( nodeLabel, existingPropertyKeyIds, _nodeCursor, _propertyCursor, ADDED_LABEL );
		 }

		 private int[] LoadSortedPropertyKeyList()
		 {
			  _nodeCursor.properties( _propertyCursor );
			  if ( !_propertyCursor.next() )
			  {
					return _emptyIntArray;
			  }

			  int[] propertyKeyIds = new int[4]; // just some arbitrary starting point, it grows on demand
			  int cursor = 0;
			  do
			  {
					if ( cursor == propertyKeyIds.Length )
					{
						 propertyKeyIds = Arrays.copyOf( propertyKeyIds, cursor * 2 );
					}
					propertyKeyIds[cursor++] = _propertyCursor.propertyKey();
			  } while ( _propertyCursor.next() );
			  if ( cursor != propertyKeyIds.Length )
			  {
					propertyKeyIds = Arrays.copyOf( propertyKeyIds, cursor );
			  }
			  Arrays.sort( propertyKeyIds );
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean nodeDelete(long node, boolean lock) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 private bool NodeDelete( long node, bool @lock )
		 {
			  _ktx.assertOpen();

			  if ( _ktx.hasTxStateWithChanges() )
			  {
					if ( _ktx.txState().nodeIsAddedInThisTx(node) )
					{
						 _autoIndexing.nodes().entityRemoved(this, node);
						 _ktx.txState().nodeDoDelete(node);
						 return true;
					}
					if ( _ktx.txState().nodeIsDeletedInThisTx(node) )
					{
						 // already deleted
						 return false;
					}
			  }

			  if ( @lock )
			  {
					_ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), ResourceTypes.NODE, node);
			  }

			  _allStoreHolder.singleNode( node, _nodeCursor );
			  if ( _nodeCursor.next() )
			  {
					AcquireSharedNodeLabelLocks();

					_autoIndexing.nodes().entityRemoved(this, node);
					_ktx.txState().nodeDoDelete(node);
					return true;
			  }

			  // tried to delete node that does not exist
			  return false;
		 }

		 /// <summary>
		 /// Assuming that the nodeCursor have been initialized to the node that labels are retrieved from
		 /// </summary>
		 private long[] AcquireSharedNodeLabelLocks()
		 {
			  long[] labels = _nodeCursor.labels().all();
			  _ktx.statementLocks().optimistic().acquireShared(_ktx.lockTracer(), ResourceTypes.LABEL, labels);
			  return labels;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean relationshipDelete(long relationship, boolean lock) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 private bool RelationshipDelete( long relationship, bool @lock )
		 {
			  _allStoreHolder.singleRelationship( relationship, _relationshipCursor ); // tx-state aware

			  if ( _relationshipCursor.next() )
			  {
					if ( @lock )
					{
						 LockRelationshipNodes( _relationshipCursor.sourceNodeReference(), _relationshipCursor.targetNodeReference() );
						 AcquireExclusiveRelationshipLock( relationship );
					}
					if ( !_allStoreHolder.relationshipExists( relationship ) )
					{
						 return false;
					}

					_ktx.assertOpen();

					_autoIndexing.relationships().entityRemoved(this, relationship);

					TransactionState txState = _ktx.txState();
					if ( txState.RelationshipIsAddedInThisTx( relationship ) )
					{
						 txState.RelationshipDoDeleteAddedInThisTx( relationship );
					}
					else
					{
						 txState.RelationshipDoDelete( relationship, _relationshipCursor.type(), _relationshipCursor.sourceNodeReference(), _relationshipCursor.targetNodeReference() );
					}
					return true;
			  }

			  // tried to delete relationship that does not exist
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void singleNode(long node) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 private void SingleNode( long node )
		 {
			  _allStoreHolder.singleNode( node, _nodeCursor );
			  if ( !_nodeCursor.next() )
			  {
					throw new IEntityNotFoundException( NODE, node );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void singleRelationship(long relationship) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 private void SingleRelationship( long relationship )
		 {
			  _allStoreHolder.singleRelationship( relationship, _relationshipCursor );
			  if ( !_relationshipCursor.next() )
			  {
					throw new IEntityNotFoundException( IEntityType.RELATIONSHIP, relationship );
			  }
		 }

		 /// <summary>
		 /// Fetch the property values for all properties in schema for a given node. Return these as an exact predicate
		 /// array.
		 /// </summary>
		 private IndexQuery.ExactPredicate[] GetAllPropertyValues( SchemaDescriptor schema, int changedPropertyKeyId, Value changedValue )
		 {
			  int[] schemaPropertyIds = Schema.PropertyIds;
			  IndexQuery.ExactPredicate[] values = new IndexQuery.ExactPredicate[schemaPropertyIds.Length];

			  int nMatched = 0;
			  _nodeCursor.properties( _propertyCursor );
			  while ( _propertyCursor.next() )
			  {
					int nodePropertyId = _propertyCursor.propertyKey();
					int k = ArrayUtils.IndexOf( schemaPropertyIds, nodePropertyId );
					if ( k >= 0 )
					{
						 if ( nodePropertyId != StatementConstants.NO_SUCH_PROPERTY_KEY )
						 {
							  values[k] = IndexQuery.exact( nodePropertyId, _propertyCursor.propertyValue() );
						 }
						 nMatched++;
					}
			  }

			  //This is true if we are adding a property
			  if ( changedPropertyKeyId != NO_SUCH_PROPERTY_KEY )
			  {
					int k = ArrayUtils.IndexOf( schemaPropertyIds, changedPropertyKeyId );
					if ( k >= 0 )
					{
						 values[k] = IndexQuery.exact( changedPropertyKeyId, changedValue );
						 nMatched++;
					}
			  }

			  if ( nMatched < values.Length )
			  {
					return null;
			  }
			  return values;
		 }

		 /// <summary>
		 /// Check so that there is not an existing node with the exact match of label and property
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNoExistingNodeWithExactValues(org.Neo4Net.kernel.api.schema.constraints.IndexBackedConstraintDescriptor constraint, org.Neo4Net.internal.kernel.api.IndexQuery.ExactPredicate[] propertyValues, long modifiedNode) throws org.Neo4Net.kernel.api.exceptions.schema.UniquePropertyValueValidationException, org.Neo4Net.kernel.api.exceptions.schema.UnableToValidateConstraintException
		 private void ValidateNoExistingNodeWithExactValues( IndexBackedConstraintDescriptor constraint, IndexQuery.ExactPredicate[] propertyValues, long modifiedNode )
		 {
			  IndexDescriptor schemaIndexDescriptor = constraint.OwnedIndexDescriptor();
			  IndexReference indexReference = _allStoreHolder.indexGetCapability( schemaIndexDescriptor );
			  try
			  {
					  using ( DefaultNodeValueIndexCursor valueCursor = _cursors.allocateNodeValueIndexCursor(), IndexReaders indexReaders = new IndexReaders(indexReference, _allStoreHolder) )
					  {
						AssertIndexOnline( schemaIndexDescriptor );
						int labelId = schemaIndexDescriptor.Schema().keyId();
      
						//Take a big fat lock, and check for existing node in index
						_ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), INDEX_ENTRY, indexEntryResourceId(labelId, propertyValues));
      
						_allStoreHolder.nodeIndexSeekWithFreshIndexReader( valueCursor, indexReaders.CreateReader(), propertyValues );
						if ( valueCursor.Next() && valueCursor.NodeReference() != modifiedNode )
						{
							 throw new UniquePropertyValueValidationException( constraint, VALIDATION, new IndexEntryConflictException( valueCursor.NodeReference(), NO_SUCH_NODE, IndexQuery.asValueTuple(propertyValues) ) );
						}
					  }
			  }
			  catch ( Exception e ) when ( e is IndexNotFoundKernelException || e is IndexBrokenKernelException || e is IndexNotApplicableKernelException )
			  {
					throw new UnableToValidateConstraintException( constraint, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexOnline(org.Neo4Net.storageengine.api.schema.IndexDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.schema.IndexBrokenKernelException
		 private void AssertIndexOnline( IndexDescriptor descriptor )
		 {
			  if ( _allStoreHolder.indexGetState( descriptor ) != InternalIndexState.ONLINE )
			  {
					throw new IndexBrokenKernelException( _allStoreHolder.indexGetFailure( descriptor ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean nodeRemoveLabel(long node, int labelId) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 public override bool NodeRemoveLabel( long node, int labelId )
		 {
			  AcquireExclusiveNodeLock( node );
			  _ktx.assertOpen();

			  SingleNode( node );

			  if ( !_nodeCursor.hasLabel( labelId ) )
			  {
					//the label wasn't there, nothing to do
					return false;
			  }

			  SharedSchemaLock( ResourceTypes.LABEL, labelId );
			  _ktx.txState().nodeDoRemoveLabel(labelId, node);
			  if ( _indexingService.hasRelatedSchema( labelId, NODE ) )
			  {
					_updater.onLabelChange( labelId, LoadSortedPropertyKeyList(), _nodeCursor, _propertyCursor, REMOVED_LABEL );
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.storable.Value nodeSetProperty(long node, int propertyKey, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.schema.ConstraintValidationException, org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override Value NodeSetProperty( long node, int propertyKey, Value value )
		 {
			  AcquireExclusiveNodeLock( node );
			  _ktx.assertOpen();

			  SingleNode( node );
			  long[] labels = AcquireSharedNodeLabelLocks();
			  Value existingValue = ReadNodeProperty( propertyKey );
			  int[] existingPropertyKeyIds = null;
			  bool hasRelatedSchema = _indexingService.hasRelatedSchema( labels, propertyKey, NODE );
			  if ( hasRelatedSchema )
			  {
					existingPropertyKeyIds = LoadSortedPropertyKeyList();
			  }

			  if ( hasRelatedSchema && !existingValue.Equals( value ) )
			  {
					// The value changed and there may be relevant constraints to check so let's check those now.
					ICollection<IndexBackedConstraintDescriptor> uniquenessConstraints = _indexingService.getRelatedUniquenessConstraints( labels, propertyKey, NODE );
					NodeSchemaMatcher.OnMatchingSchema(uniquenessConstraints.GetEnumerator(), propertyKey, existingPropertyKeyIds, uniquenessConstraint =>
					{
								ValidateNoExistingNodeWithExactValues( uniquenessConstraint, GetAllPropertyValues( uniquenessConstraint.schema(), propertyKey, value ), node );
					});
			  }

			  if ( existingValue == NO_VALUE )
			  {
					//no existing value, we just add it
					_autoIndexing.nodes().propertyAdded(this, node, propertyKey, value);
					_ktx.txState().nodeDoAddProperty(node, propertyKey, value);
					if ( hasRelatedSchema )
					{
						 _updater.onPropertyAdd( _nodeCursor, _propertyCursor, labels, propertyKey, existingPropertyKeyIds, value );
					}
					return NO_VALUE;
			  }
			  else
			  {
					// We need to auto-index even if not actually changing the value.
					_autoIndexing.nodes().propertyChanged(this, node, propertyKey, existingValue, value);
					if ( PropertyHasChanged( value, existingValue ) )
					{
						 //the value has changed to a new value
						 _ktx.txState().nodeDoChangeProperty(node, propertyKey, value);
						 if ( hasRelatedSchema )
						 {
							  _updater.onPropertyChange( _nodeCursor, _propertyCursor, labels, propertyKey, existingPropertyKeyIds, existingValue, value );
						 }
					}
					return existingValue;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.storable.Value nodeRemoveProperty(long node, int propertyKey) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override Value NodeRemoveProperty( long node, int propertyKey )
		 {
			  AcquireExclusiveNodeLock( node );
			  _ktx.assertOpen();
			  SingleNode( node );
			  Value existingValue = ReadNodeProperty( propertyKey );

			  if ( existingValue != NO_VALUE )
			  {
					long[] labels = AcquireSharedNodeLabelLocks();
					_autoIndexing.nodes().propertyRemoved(this, node, propertyKey);
					_ktx.txState().nodeDoRemoveProperty(node, propertyKey);
					if ( _indexingService.hasRelatedSchema( labels, propertyKey, NODE ) )
					{
						 _updater.onPropertyRemove( _nodeCursor, _propertyCursor, labels, propertyKey, LoadSortedPropertyKeyList(), existingValue );
					}
			  }

			  return existingValue;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.storable.Value relationshipSetProperty(long relationship, int propertyKey, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override Value RelationshipSetProperty( long relationship, int propertyKey, Value value )
		 {
			  AcquireExclusiveRelationshipLock( relationship );
			  _ktx.assertOpen();
			  SingleRelationship( relationship );
			  Value existingValue = ReadRelationshipProperty( propertyKey );
			  if ( existingValue == NO_VALUE )
			  {
					_autoIndexing.relationships().propertyAdded(this, relationship, propertyKey, value);
					_ktx.txState().relationshipDoReplaceProperty(relationship, propertyKey, NO_VALUE, value);
					return NO_VALUE;
			  }
			  else
			  {
					// We need to auto-index even if not actually changing the value.
					_autoIndexing.relationships().propertyChanged(this, relationship, propertyKey, existingValue, value);
					if ( PropertyHasChanged( existingValue, value ) )
					{

						 _ktx.txState().relationshipDoReplaceProperty(relationship, propertyKey, existingValue, value);
					}

					return existingValue;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.storable.Value relationshipRemoveProperty(long relationship, int propertyKey) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override Value RelationshipRemoveProperty( long relationship, int propertyKey )
		 {
			  AcquireExclusiveRelationshipLock( relationship );
			  _ktx.assertOpen();
			  SingleRelationship( relationship );
			  Value existingValue = ReadRelationshipProperty( propertyKey );

			  if ( existingValue != NO_VALUE )
			  {
					_autoIndexing.relationships().propertyRemoved(this, relationship, propertyKey);
					_ktx.txState().relationshipDoRemoveProperty(relationship, propertyKey);
			  }

			  return existingValue;
		 }

		 public override Value GraphSetProperty( int propertyKey, Value value )
		 {
			  _ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), ResourceTypes.GRAPH_PROPS, ResourceTypes.graphPropertyResource());
			  _ktx.assertOpen();

			  Value existingValue = ReadGraphProperty( propertyKey );
			  if ( !existingValue.Equals( value ) )
			  {
					_ktx.txState().graphDoReplaceProperty(propertyKey, existingValue, value);
			  }
			  return existingValue;
		 }

		 public override Value GraphRemoveProperty( int propertyKey )
		 {
			  _ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), ResourceTypes.GRAPH_PROPS, ResourceTypes.graphPropertyResource());
			  _ktx.assertOpen();

			  Value existingValue = ReadGraphProperty( propertyKey );
			  if ( existingValue != Values.NO_VALUE )
			  {
					_ktx.txState().graphDoRemoveProperty(propertyKey);
			  }
			  return existingValue;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeAddToExplicitIndex(String indexName, long node, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeAddToExplicitIndex( string indexName, long node, string key, object value )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().nodeChanges(indexName).addNode(node, key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeRemoveFromExplicitIndex(String indexName, long node) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeRemoveFromExplicitIndex( string indexName, long node )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().nodeChanges(indexName).remove(node);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeRemoveFromExplicitIndex(String indexName, long node, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeRemoveFromExplicitIndex( string indexName, long node, string key, object value )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().nodeChanges(indexName).remove(node, key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeRemoveFromExplicitIndex(String indexName, long node, String key) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeRemoveFromExplicitIndex( string indexName, long node, string key )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().nodeChanges(indexName).remove(node, key);
		 }

		 public override void NodeExplicitIndexCreate( string indexName, IDictionary<string, string> customConfig )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().createIndex(IndexEntityType.Node, indexName, customConfig);
		 }

		 public override void NodeExplicitIndexCreateLazily( string indexName, IDictionary<string, string> customConfig )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.getOrCreateNodeIndexConfig( indexName, customConfig );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeExplicitIndexDrop(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexDrop( string indexName )
		 {
			  _ktx.assertOpen();
			  ExplicitIndexTransactionState txState = _allStoreHolder.explicitIndexTxState();
			  txState.NodeChanges( indexName ).drop();
			  txState.DeleteIndex( IndexEntityType.Node, indexName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String nodeExplicitIndexSetConfiguration(String indexName, String key, String value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override string NodeExplicitIndexSetConfiguration( string indexName, string key, string value )
		 {
			  _ktx.assertOpen();
			  return _allStoreHolder.explicitIndexStore().setNodeIndexConfiguration(indexName, key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String nodeExplicitIndexRemoveConfiguration(String indexName, String key) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override string NodeExplicitIndexRemoveConfiguration( string indexName, string key )
		 {
			  _ktx.assertOpen();
			  return _allStoreHolder.explicitIndexStore().removeNodeIndexConfiguration(indexName, key);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipAddToExplicitIndex(String indexName, long relationship, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException, org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 public override void RelationshipAddToExplicitIndex( string indexName, long relationship, string key, object value )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.singleRelationship( relationship, _relationshipCursor );
			  if ( _relationshipCursor.next() )
			  {
					_allStoreHolder.explicitIndexTxState().relationshipChanges(indexName).addRelationship(relationship, key, value, _relationshipCursor.sourceNodeReference(), _relationshipCursor.targetNodeReference());
			  }
			  else
			  {
					throw new IEntityNotFoundException( IEntityType.RELATIONSHIP, relationship );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipRemoveFromExplicitIndex(String indexName, long relationship, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipRemoveFromExplicitIndex( string indexName, long relationship, string key, object value )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().relationshipChanges(indexName).remove(relationship, key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipRemoveFromExplicitIndex(String indexName, long relationship, String key) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipRemoveFromExplicitIndex( string indexName, long relationship, string key )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().relationshipChanges(indexName).remove(relationship, key);

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipRemoveFromExplicitIndex(String indexName, long relationship) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipRemoveFromExplicitIndex( string indexName, long relationship )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().relationshipChanges(indexName).remove(relationship);
		 }

		 public override void RelationshipExplicitIndexCreate( string indexName, IDictionary<string, string> customConfig )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.explicitIndexTxState().createIndex(IndexEntityType.Relationship, indexName, customConfig);
		 }

		 public override void RelationshipExplicitIndexCreateLazily( string indexName, IDictionary<string, string> customConfig )
		 {
			  _ktx.assertOpen();
			  _allStoreHolder.getOrCreateRelationshipIndexConfig( indexName, customConfig );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexDrop(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexDrop( string indexName )
		 {
			  _ktx.assertOpen();
			  ExplicitIndexTransactionState txState = _allStoreHolder.explicitIndexTxState();
			  txState.RelationshipChanges( indexName ).drop();
			  txState.DeleteIndex( IndexEntityType.Relationship, indexName );
		 }

		 private Value ReadNodeProperty( int propertyKey )
		 {
			  _nodeCursor.properties( _propertyCursor );

			  //Find out if the property had a value
			  Value existingValue = NO_VALUE;
			  while ( _propertyCursor.next() )
			  {
					if ( _propertyCursor.propertyKey() == propertyKey )
					{
						 existingValue = _propertyCursor.propertyValue();
						 break;
					}
			  }
			  return existingValue;
		 }

		 private Value ReadRelationshipProperty( int propertyKey )
		 {
			  _relationshipCursor.properties( _propertyCursor );

			  //Find out if the property had a value
			  Value existingValue = NO_VALUE;
			  while ( _propertyCursor.next() )
			  {
					if ( _propertyCursor.propertyKey() == propertyKey )
					{
						 existingValue = _propertyCursor.propertyValue();
						 break;
					}
			  }
			  return existingValue;
		 }

		 private Value ReadGraphProperty( int propertyKey )
		 {
			  _allStoreHolder.graphProperties( _propertyCursor );

			  //Find out if the property had a value
			  Value existingValue = NO_VALUE;
			  while ( _propertyCursor.next() )
			  {
					if ( _propertyCursor.propertyKey() == propertyKey )
					{
						 existingValue = _propertyCursor.propertyValue();
						 break;
					}
			  }
			  return existingValue;
		 }

		 public virtual CursorFactory Cursors()
		 {
			  return _cursors;
		 }

		 public virtual Procedures Procedures()
		 {
			  return _allStoreHolder;
		 }

		 public virtual void Release()
		 {
			  if ( _nodeCursor != null )
			  {
					_nodeCursor.close();
					_nodeCursor = null;
			  }
			  if ( _propertyCursor != null )
			  {
					_propertyCursor.close();
					_propertyCursor = null;
			  }
			  if ( _relationshipCursor != null )
			  {
					_relationshipCursor.close();
					_relationshipCursor = null;
			  }

			  _cursors.assertClosed();
			  _cursors.release();
		 }

		 public virtual Token Token()
		 {
			  return _token;
		 }

		 public virtual ExplicitIndexRead IndexRead()
		 {
			  return _allStoreHolder;
		 }

		 public virtual SchemaRead SchemaRead()
		 {
			  return _allStoreHolder;
		 }

		 public virtual Read DataRead()
		 {
			  return _allStoreHolder;
		 }

		 public virtual DefaultNodeCursor NodeCursor()
		 {
			  return _nodeCursor;
		 }

		 public virtual DefaultRelationshipScanCursor RelationshipCursor()
		 {
			  return _relationshipCursor;
		 }

		 public virtual DefaultPropertyCursor PropertyCursor()
		 {
			  return _propertyCursor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.IndexReference indexCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override IndexReference IndexCreate( SchemaDescriptor descriptor )
		 {
			  return IndexCreate( descriptor, _config.get( GraphDatabaseSettings.default_schema_provider ), null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.IndexReference indexCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, java.util.Optional<String> indexName) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override IndexReference IndexCreate( SchemaDescriptor descriptor, Optional<string> indexName )
		 {
			  return IndexCreate( descriptor, _config.get( GraphDatabaseSettings.default_schema_provider ), indexName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.IndexReference indexCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, String provider, java.util.Optional<String> name) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override IndexReference IndexCreate( SchemaDescriptor descriptor, string provider, Optional<string> name )
		 {
			  ExclusiveSchemaLock( descriptor );
			  _ktx.assertOpen();
			  AssertValidDescriptor( descriptor, SchemaKernelException.OperationContext.INDEX_CREATION );
			  AssertIndexDoesNotExist( SchemaKernelException.OperationContext.INDEX_CREATION, descriptor, name );

			  IndexProviderDescriptor providerDescriptor = _indexingService.indexProviderByName( provider );
			  IndexDescriptor index = IndexDescriptorFactory.forSchema( descriptor, name, providerDescriptor );
			  index = _indexingService.getBlessedDescriptorFromProvider( index );
			  _ktx.txState().indexDoAdd(index);
			  return index;
		 }

		 // Note: this will be sneakily executed by an internal transaction, so no additional locking is required.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.storageengine.api.schema.IndexDescriptor indexUniqueCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor schema, String provider) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public virtual IndexDescriptor IndexUniqueCreate( SchemaDescriptor schema, string provider )
		 {
			  IndexProviderDescriptor providerDescriptor = _indexingService.indexProviderByName( provider );
			  IndexDescriptor index = IndexDescriptorFactory.uniqueForSchema( schema, null, providerDescriptor );
			  index = _indexingService.getBlessedDescriptorFromProvider( index );
			  _ktx.txState().indexDoAdd(index);
			  return index;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void indexDrop(org.Neo4Net.internal.kernel.api.IndexReference indexReference) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override void IndexDrop( IndexReference indexReference )
		 {
			  AssertValidIndex( indexReference );
			  IndexDescriptor index = ( IndexDescriptor ) indexReference;
			  SchemaDescriptor schema = index.Schema();

			  ExclusiveSchemaLock( schema );
			  _ktx.assertOpen();
			  try
			  {
					IndexDescriptor existingIndex = _allStoreHolder.indexGetForSchema( schema );

					if ( existingIndex == null )
					{
						 throw new NoSuchIndexException( schema );
					}

					if ( existingIndex.Type() == UNIQUE )
					{
						 if ( _allStoreHolder.indexGetOwningUniquenessConstraintId( existingIndex ) != null )
						 {
							  throw new IndexBelongsToConstraintException( schema );
						 }
					}
			  }
			  catch ( Exception e ) when ( e is IndexBelongsToConstraintException || e is NoSuchIndexException )
			  {
					throw new DropIndexFailureException( schema, e );
			  }
			  _ktx.txState().indexDoDrop(index);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor UniquePropertyConstraintCreate( SchemaDescriptor descriptor )
		 {
			  return UniquePropertyConstraintCreate( descriptor, _config.get( GraphDatabaseSettings.default_schema_provider ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, String provider) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor UniquePropertyConstraintCreate( SchemaDescriptor descriptor, string provider )
		 {
			  //Lock
			  ExclusiveSchemaLock( descriptor );
			  _ktx.assertOpen();
			  UniquenessConstraintDescriptor constraint;

			  try
			  {
					//Check data integrity
					AssertValidDescriptor( descriptor, SchemaKernelException.OperationContext.CONSTRAINT_CREATION );
					constraint = ConstraintDescriptorFactory.uniqueForSchema( descriptor );
					AssertConstraintDoesNotExist( constraint );
					// It is not allowed to create uniqueness constraints on indexed label/property pairs
					AssertIndexDoesNotExist( SchemaKernelException.OperationContext.CONSTRAINT_CREATION, descriptor, null );
			  }
			  catch ( SchemaKernelException e )
			  {
					ExclusiveSchemaUnlock( descriptor ); // Try not to hold on to exclusive schema locks when we don't strictly need them.
					throw e;
			  }

			  // Create constraints
			  IndexBackedConstraintCreate( constraint, provider );
			  return constraint;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.Neo4Net.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor NodeKeyConstraintCreate( LabelSchemaDescriptor descriptor )
		 {
			  return NodeKeyConstraintCreate( descriptor, _config.get( GraphDatabaseSettings.default_schema_provider ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.Neo4Net.internal.kernel.api.schema.LabelSchemaDescriptor descriptor, String provider) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor NodeKeyConstraintCreate( LabelSchemaDescriptor descriptor, string provider )
		 {
			  //Lock
			  ExclusiveSchemaLock( descriptor );
			  _ktx.assertOpen();
			  NodeKeyConstraintDescriptor constraint;

			  try
			  {
					//Check data integrity
					AssertValidDescriptor( descriptor, SchemaKernelException.OperationContext.CONSTRAINT_CREATION );
					constraint = ConstraintDescriptorFactory.nodeKeyForSchema( descriptor );
					AssertConstraintDoesNotExist( constraint );
					// It is not allowed to create node key constraints on indexed label/property pairs
					AssertIndexDoesNotExist( SchemaKernelException.OperationContext.CONSTRAINT_CREATION, descriptor, null );
			  }
			  catch ( SchemaKernelException e )
			  {
					ExclusiveSchemaUnlock( descriptor );
					throw e;
			  }

			  //enforce constraints
			  using ( NodeLabelIndexCursor nodes = _cursors.allocateNodeLabelIndexCursor() )
			  {
					_allStoreHolder.nodeLabelScan( descriptor.LabelId, nodes );
					_constraintSemantics.validateNodeKeyConstraint( nodes, _nodeCursor, _propertyCursor, descriptor );
			  }

			  //create constraint
			  IndexBackedConstraintCreate( constraint, provider );
			  return constraint;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor nodePropertyExistenceConstraintCreate(org.Neo4Net.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor NodePropertyExistenceConstraintCreate( LabelSchemaDescriptor descriptor )
		 {
			  ConstraintDescriptor constraint = LockAndValidatePropertyExistenceConstraint( descriptor );

			  //enforce constraints
			  using ( NodeLabelIndexCursor nodes = _cursors.allocateNodeLabelIndexCursor() )
			  {
					_allStoreHolder.nodeLabelScan( descriptor.LabelId, nodes );
					_constraintSemantics.validateNodePropertyExistenceConstraint( nodes, _nodeCursor, _propertyCursor, descriptor );
			  }

			  //create constraint
			  _ktx.txState().constraintDoAdd(constraint);
			  return constraint;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor relationshipPropertyExistenceConstraintCreate(org.Neo4Net.internal.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override ConstraintDescriptor RelationshipPropertyExistenceConstraintCreate( RelationTypeSchemaDescriptor descriptor )
		 {
			  ConstraintDescriptor constraint = LockAndValidatePropertyExistenceConstraint( descriptor );

			  //enforce constraints
			  _allStoreHolder.relationshipTypeScan( descriptor.RelTypeId, _relationshipCursor );
			  _constraintSemantics.validateRelationshipPropertyExistenceConstraint( _relationshipCursor, _propertyCursor, descriptor );

			  //Create
			  _ktx.txState().constraintDoAdd(constraint);
			  return constraint;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor lockAndValidatePropertyExistenceConstraint(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 private ConstraintDescriptor LockAndValidatePropertyExistenceConstraint( SchemaDescriptor descriptor )
		 {
			  // Lock constraint schema.
			  ExclusiveSchemaLock( descriptor );
			  _ktx.assertOpen();

			  try
			  {
					// Verify data integrity.
					AssertValidDescriptor( descriptor, SchemaKernelException.OperationContext.CONSTRAINT_CREATION );
					ConstraintDescriptor constraint = ConstraintDescriptorFactory.existsForSchema( descriptor );
					AssertConstraintDoesNotExist( constraint );
					return constraint;
			  }
			  catch ( SchemaKernelException e )
			  {
					ExclusiveSchemaUnlock( descriptor );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String relationshipExplicitIndexSetConfiguration(String indexName, String key, String value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override string RelationshipExplicitIndexSetConfiguration( string indexName, string key, string value )
		 {
			  _ktx.assertOpen();
			  return _allStoreHolder.explicitIndexStore().setRelationshipIndexConfiguration(indexName, key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String relationshipExplicitIndexRemoveConfiguration(String indexName, String key) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override string RelationshipExplicitIndexRemoveConfiguration( string indexName, string key )
		 {
			  _ktx.assertOpen();
			  return _allStoreHolder.explicitIndexStore().removeRelationshipIndexConfiguration(indexName, key);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void constraintDrop(org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
		 public override void ConstraintDrop( ConstraintDescriptor descriptor )
		 {
			  //Lock
			  SchemaDescriptor schema = descriptor.Schema();
			  ExclusiveOptimisticLock( Schema.keyType(), Schema.keyId() );
			  _ktx.assertOpen();

			  //verify data integrity
			  try
			  {
					AssertConstraintExists( descriptor );
			  }
			  catch ( NoSuchConstraintException e )
			  {
					throw new DropConstraintFailureException( descriptor, e );
			  }

			  //Drop it like it's hot
			  _ktx.txState().constraintDoDrop(descriptor);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexDoesNotExist(org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException.OperationContext context, org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, java.util.Optional<String> name) throws org.Neo4Net.kernel.api.exceptions.schema.AlreadyIndexedException, org.Neo4Net.kernel.api.exceptions.schema.AlreadyConstrainedException
		 private void AssertIndexDoesNotExist( SchemaKernelException.OperationContext context, SchemaDescriptor descriptor, Optional<string> name )
		 {
			  IndexDescriptor existingIndex = _allStoreHolder.indexGetForSchema( descriptor );
			  if ( existingIndex == null && name.Present )
			  {
					IndexReference indexReference = _allStoreHolder.indexGetForName( name.get() );
					if ( indexReference != IndexReference.NO_INDEX )
					{
						 existingIndex = ( IndexDescriptor ) indexReference;
					}
			  }
			  if ( existingIndex != null )
			  {
					// OK so we found a matching constraint index. We check whether or not it has an owner
					// because this may have been a left-over constraint index from a previously failed
					// constraint creation, due to crash or similar, hence the missing owner.
					if ( existingIndex.Type() == UNIQUE )
					{
						 if ( context != CONSTRAINT_CREATION || ConstraintIndexHasOwner( existingIndex ) )
						 {
							  throw new AlreadyConstrainedException( ConstraintDescriptorFactory.uniqueForSchema( descriptor ), context, new SilentTokenNameLookup( _token ) );
						 }
					}
					else
					{
						 throw new AlreadyIndexedException( descriptor, context );
					}
			  }
		 }

		 private void ExclusiveOptimisticLock( ResourceType resource, long resourceId )
		 {
			  _ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), resource, resourceId);
		 }

		 private void AcquireExclusiveNodeLock( long node )
		 {
			  if ( !_ktx.hasTxStateWithChanges() || !_ktx.txState().nodeIsAddedInThisTx(node) )
			  {
					_ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), ResourceTypes.NODE, node);
			  }
		 }

		 private void AcquireExclusiveRelationshipLock( long relationshipId )
		 {
			  if ( !_ktx.hasTxStateWithChanges() || !_ktx.txState().relationshipIsAddedInThisTx(relationshipId) )
			  {
					_ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), ResourceTypes.RELATIONSHIP, relationshipId);
			  }
		 }

		 private void SharedSchemaLock( ResourceType type, int tokenId )
		 {
			  _ktx.statementLocks().optimistic().acquireShared(_ktx.lockTracer(), type, tokenId);
		 }

		 private void ExclusiveSchemaLock( SchemaDescriptor schema )
		 {
			  long[] lockingIds = schemaTokenLockingIds( schema );
			  _ktx.statementLocks().optimistic().acquireExclusive(_ktx.lockTracer(), Schema.keyType(), lockingIds);
		 }

		 private void ExclusiveSchemaUnlock( SchemaDescriptor schema )
		 {
			  long[] lockingIds = schemaTokenLockingIds( schema );
			  _ktx.statementLocks().optimistic().releaseExclusive(Schema.keyType(), lockingIds);
		 }

		 private void LockRelationshipNodes( long startNodeId, long endNodeId )
		 {
			  // Order the locks to lower the risk of deadlocks with other threads creating/deleting rels concurrently
			  AcquireExclusiveNodeLock( min( startNodeId, endNodeId ) );
			  if ( startNodeId != endNodeId )
			  {
					AcquireExclusiveNodeLock( max( startNodeId, endNodeId ) );
			  }
		 }

		 private static bool PropertyHasChanged( Value lhs, Value rhs )
		 {
			  //It is not enough to check equality here since by our equality semantics `int == tofloat(int)` is `true`
			  //so by only checking for equality users cannot change type of property without also "changing" the value.
			  //Hence the extra type check here.
			  return lhs.GetType() != rhs.GetType() || !lhs.Equals(rhs);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeExists(long sourceNode) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
		 private void AssertNodeExists( long sourceNode )
		 {
			  if ( !_allStoreHolder.nodeExists( sourceNode ) )
			  {
					throw new IEntityNotFoundException( NODE, sourceNode );
			  }
		 }

		 private bool ConstraintIndexHasOwner( IndexDescriptor descriptor )
		 {
			  return _allStoreHolder.indexGetOwningUniquenessConstraintId( descriptor ) != null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertConstraintDoesNotExist(org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws org.Neo4Net.kernel.api.exceptions.schema.AlreadyConstrainedException
		 private void AssertConstraintDoesNotExist( ConstraintDescriptor constraint )
		 {
			  if ( _allStoreHolder.constraintExists( constraint ) )
			  {
					throw new AlreadyConstrainedException( constraint, SchemaKernelException.OperationContext.CONSTRAINT_CREATION, new SilentTokenNameLookup( _token ) );
			  }
		 }

		 public virtual Locks Locks()
		 {
			  return _allStoreHolder;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertConstraintExists(org.Neo4Net.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws org.Neo4Net.kernel.api.exceptions.schema.NoSuchConstraintException
		 private void AssertConstraintExists( ConstraintDescriptor constraint )
		 {
			  if ( !_allStoreHolder.constraintExists( constraint ) )
			  {
					throw new NoSuchConstraintException( constraint );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertValidDescriptor(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException.OperationContext context) throws org.Neo4Net.kernel.api.exceptions.schema.RepeatedSchemaComponentException
		 private static void AssertValidDescriptor( SchemaDescriptor descriptor, SchemaKernelException.OperationContext context )
		 {
			  long numUniqueProp = java.util.descriptor.PropertyIds.Distinct().Count();
			  long numUniqueEntityTokens = java.util.descriptor.EntityTokenIds.Distinct().Count();

			  if ( numUniqueProp != descriptor.PropertyIds.Length )
			  {
					throw new RepeatedPropertyInSchemaException( descriptor, context );
			  }
			  if ( numUniqueEntityTokens != descriptor.EntityTokenIds.Length )
			  {
					if ( descriptor.EntityType() == NODE )
					{
						 throw new RepeatedLabelInSchemaException( descriptor, context );
					}
					else
					{
						 throw new RepeatedRelationshipTypeInSchemaException( descriptor, context );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void indexBackedConstraintCreate(org.Neo4Net.kernel.api.schema.constraints.IndexBackedConstraintDescriptor constraint, String provider) throws org.Neo4Net.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		 private void IndexBackedConstraintCreate( IndexBackedConstraintDescriptor constraint, string provider )
		 {
			  SchemaDescriptor descriptor = constraint.Schema();
			  try
			  {
					if ( _ktx.hasTxStateWithChanges() && _ktx.txState().indexDoUnRemove(constraint.OwnedIndexDescriptor()) ) // ..., DROP, *CREATE*
					{ // creation is undoing a drop
						 if ( !_ktx.txState().constraintDoUnRemove(constraint) ) // CREATE, ..., DROP, *CREATE*
						 { // ... the drop we are undoing did itself undo a prior create...
							  _ktx.txState().constraintDoAdd(constraint, _ktx.txState().indexCreatedForConstraint(constraint).Value);
						 }
					}
					else // *CREATE*
					{ // create from scratch
						 IEnumerator<ConstraintDescriptor> it = _allStoreHolder.constraintsGetForSchema( descriptor );
						 while ( it.MoveNext() )
						 {
							  if ( it.Current.Equals( constraint ) )
							  {
									return;
							  }
						 }
						 long indexId = _constraintIndexCreator.createUniquenessConstraintIndex( _ktx, descriptor, provider );
						 if ( !_allStoreHolder.constraintExists( constraint ) )
						 {
							  // This looks weird, but since we release the label lock while awaiting population of the index
							  // backing this constraint there can be someone else getting ahead of us, creating this exact
							  // constraint
							  // before we do, so now getting out here under the lock we must check again and if it exists
							  // we must at this point consider this an idempotent operation because we verified earlier
							  // that it didn't exist and went on to create it.
							  _ktx.txState().constraintDoAdd(constraint, indexId);
						 }
					}
			  }
			  catch ( Exception e ) when ( e is UniquePropertyValueValidationException || e is TransactionFailureException || e is AlreadyConstrainedException )
			  {
					throw new CreateConstraintFailureException( constraint, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertValidIndex(org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.kernel.api.exceptions.schema.NoSuchIndexException
		 private static void AssertValidIndex( IndexReference index )
		 {
			  if ( index == IndexReference.NO_INDEX )
			  {
					throw new NoSuchIndexException( index.Schema() );
			  }
		 }
	}

}
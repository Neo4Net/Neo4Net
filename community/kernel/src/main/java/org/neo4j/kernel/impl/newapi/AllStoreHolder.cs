using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Newapi
{

	using Org.Neo4j.Collection;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using SchemaReadCore = Org.Neo4j.@internal.Kernel.Api.SchemaReadCore;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureHandle = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureHandle;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using UserAggregator = Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionHandle = Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionHandle;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using SchemaRuleNotFoundException = Org.Neo4j.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using BasicContext = Org.Neo4j.Kernel.api.proc.BasicContext;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using TransactionCountingStateVisitor = Org.Neo4j.Kernel.api.txstate.TransactionCountingStateVisitor;
	using TransactionState = Org.Neo4j.Kernel.api.txstate.TransactionState;
	using ClockContext = Org.Neo4j.Kernel.Impl.Api.ClockContext;
	using CountsRecordState = Org.Neo4j.Kernel.Impl.Api.CountsRecordState;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Org.Neo4j.Kernel.Impl.Api.SchemaState;
	using OverriddenAccessMode = Org.Neo4j.Kernel.Impl.Api.security.OverriddenAccessMode;
	using RestrictedAccessMode = Org.Neo4j.Kernel.Impl.Api.security.RestrictedAccessMode;
	using ExplicitIndexStore = Org.Neo4j.Kernel.impl.index.ExplicitIndexStore;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Register = Org.Neo4j.Register.Register;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using StorageSchemaReader = Org.Neo4j.Storageengine.Api.StorageSchemaReader;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Org.Neo4j.Storageengine.Api.txstate;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Org.Neo4j.Values;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.singleOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.register.Registers.newDoubleLongRegister;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.txstate.TxStateVisitor_Fields.EMPTY;

	public class AllStoreHolder : Read
	{
		 private readonly StorageReader _storageReader;
		 private readonly ExplicitIndexStore _explicitIndexStore;
		 private readonly Procedures _procedures;
		 private readonly SchemaState _schemaState;
		 private readonly Dependencies _dataSourceDependencies;

		 public AllStoreHolder( StorageReader storageReader, KernelTransactionImplementation ktx, DefaultCursors cursors, ExplicitIndexStore explicitIndexStore, Procedures procedures, SchemaState schemaState, Dependencies dataSourceDependencies ) : base( cursors, ktx )
		 {
			  this._storageReader = storageReader;
			  this._explicitIndexStore = explicitIndexStore;
			  this._procedures = procedures;
			  this._schemaState = schemaState;
			  this._dataSourceDependencies = dataSourceDependencies;
		 }

		 public override bool NodeExists( long reference )
		 {
			  Ktx.assertOpen();

			  if ( HasTxStateWithChanges() )
			  {
					TransactionState txState = txState();
					if ( txState.NodeIsDeletedInThisTx( reference ) )
					{
						 return false;
					}
					else if ( txState.NodeIsAddedInThisTx( reference ) )
					{
						 return true;
					}
			  }
			  return _storageReader.nodeExists( reference );
		 }

		 public override bool NodeDeletedInTransaction( long node )
		 {
			  Ktx.assertOpen();
			  return HasTxStateWithChanges() && TxState().nodeIsDeletedInThisTx(node);
		 }

		 public override bool RelationshipDeletedInTransaction( long relationship )
		 {
			  Ktx.assertOpen();
			  return HasTxStateWithChanges() && TxState().relationshipIsDeletedInThisTx(relationship);
		 }

		 public override Value NodePropertyChangeInTransactionOrNull( long node, int propertyKeyId )
		 {
			  Ktx.assertOpen();
			  return HasTxStateWithChanges() ? TxState().getNodeState(node).propertyValue(propertyKeyId) : null;
		 }

		 public override long CountsForNode( int labelId )
		 {
			  long count = CountsForNodeWithoutTxState( labelId );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					CountsRecordState counts = new CountsRecordState();
					try
					{
						 TransactionState txState = Ktx.txState();
						 txState.Accept( new TransactionCountingStateVisitor( EMPTY, _storageReader, txState, counts ) );
						 if ( Counts.hasChanges() )
						 {
							  count += Counts.nodeCount( labelId, newDoubleLongRegister() ).readSecond();
						 }
					}
					catch ( Exception e ) when ( e is ConstraintValidationException || e is CreateConstraintFailureException )
					{
						 throw new System.ArgumentException( "Unexpected error: " + e.Message );
					}
			  }
			  return count;
		 }

		 public override long CountsForNodeWithoutTxState( int labelId )
		 {
			  return _storageReader.countsForNode( labelId );
		 }

		 public override long CountsForRelationship( int startLabelId, int typeId, int endLabelId )
		 {
			  long count = CountsForRelationshipWithoutTxState( startLabelId, typeId, endLabelId );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					CountsRecordState counts = new CountsRecordState();
					try
					{
						 TransactionState txState = Ktx.txState();
						 txState.Accept( new TransactionCountingStateVisitor( EMPTY, _storageReader, txState, counts ) );
						 if ( Counts.hasChanges() )
						 {
							  count += Counts.relationshipCount( startLabelId, typeId, endLabelId, newDoubleLongRegister() ).readSecond();
						 }
					}
					catch ( Exception e ) when ( e is ConstraintValidationException || e is CreateConstraintFailureException )
					{
						 throw new System.ArgumentException( "Unexpected error: " + e.Message );
					}
			  }
			  return count;
		 }

		 public override long CountsForRelationshipWithoutTxState( int startLabelId, int typeId, int endLabelId )
		 {
			  return _storageReader.countsForRelationship( startLabelId, typeId, endLabelId );
		 }

		 public override bool RelationshipExists( long reference )
		 {
			  Ktx.assertOpen();

			  if ( HasTxStateWithChanges() )
			  {
					TransactionState txState = txState();
					if ( txState.RelationshipIsDeletedInThisTx( reference ) )
					{
						 return false;
					}
					else if ( txState.RelationshipIsAddedInThisTx( reference ) )
					{
						 return true;
					}
			  }
			  return _storageReader.relationshipExists( reference );
		 }

		 internal override long GraphPropertiesReference()
		 {
			  return _storageReader.GraphPropertyReference;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader indexReader(org.neo4j.internal.kernel.api.IndexReference index, boolean fresh) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexReader IndexReader( IndexReference index, bool fresh )
		 {
			  AssertValidIndex( index );
			  return fresh ? _storageReader.getFreshIndexReader( ( IndexDescriptor ) index ) : _storageReader.getIndexReader( ( IndexDescriptor ) index );
		 }

		 internal override LabelScanReader LabelScanReader()
		 {
			  return _storageReader.LabelScanReader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndex explicitNodeIndex(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override ExplicitIndex ExplicitNodeIndex( string indexName )
		 {
			  Ktx.assertOpen();
			  return ExplicitIndexTxState().nodeChanges(indexName);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndex explicitRelationshipIndex(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override ExplicitIndex ExplicitRelationshipIndex( string indexName )
		 {
			  Ktx.assertOpen();
			  return ExplicitIndexTxState().relationshipChanges(indexName);
		 }

		 public override string[] NodeExplicitIndexesGetAll()
		 {
			  Ktx.assertOpen();
			  return _explicitIndexStore.AllNodeIndexNames;
		 }

		 public override bool NodeExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  Ktx.assertOpen();
			  return ExplicitIndexTxState().checkIndexExistence(IndexEntityType.Node, indexName, customConfiguration);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String,String> nodeExplicitIndexGetConfiguration(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override IDictionary<string, string> NodeExplicitIndexGetConfiguration( string indexName )
		 {
			  Ktx.assertOpen();
			  return _explicitIndexStore.getNodeIndexConfiguration( indexName );
		 }

		 public override string[] RelationshipExplicitIndexesGetAll()
		 {
			  Ktx.assertOpen();
			  return _explicitIndexStore.AllRelationshipIndexNames;
		 }

		 public override bool RelationshipExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  Ktx.assertOpen();
			  return ExplicitIndexTxState().checkIndexExistence(IndexEntityType.Relationship, indexName, customConfiguration);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String,String> relationshipExplicitIndexGetConfiguration(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override IDictionary<string, string> RelationshipExplicitIndexGetConfiguration( string indexName )
		 {
			  Ktx.assertOpen();
			  return _explicitIndexStore.getRelationshipIndexConfiguration( indexName );
		 }

		 public override IndexReference Index( int label, params int[] properties )
		 {
			  Ktx.assertOpen();

			  LabelSchemaDescriptor descriptor;
			  try
			  {
					descriptor = SchemaDescriptorFactory.forLabel( label, properties );
			  }
			  catch ( System.ArgumentException )
			  {
					// This means we have invalid label or property ids.
					return IndexReference.NO_INDEX;
			  }
			  CapableIndexDescriptor indexDescriptor = _storageReader.indexGetForSchema( descriptor );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					DiffSets<IndexDescriptor> diffSets = Ktx.txState().indexDiffSetsByLabel(label);
					if ( indexDescriptor != null )
					{
						 if ( diffSets.IsRemoved( indexDescriptor ) )
						 {
							  return IndexReference.NO_INDEX;
						 }
						 else
						 {
							  return indexDescriptor;
						 }
					}
					else
					{
						 IEnumerator<IndexDescriptor> fromTxState = filter( SchemaDescriptor.equalTo( descriptor ), diffSets.Added.GetEnumerator() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( fromTxState.hasNext() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  return fromTxState.next();
						 }
						 else
						 {
							  return IndexReference.NO_INDEX;
						 }
					}
			  }

			  return indexDescriptor != null ? indexDescriptor : IndexReference.NO_INDEX;
		 }

		 public override IndexReference Index( SchemaDescriptor schema )
		 {
			  Ktx.assertOpen();
			  return IndexGetForSchema( _storageReader, schema );
		 }

		 internal virtual IndexReference IndexGetForSchema( StorageSchemaReader reader, SchemaDescriptor schema )
		 {
			  CapableIndexDescriptor indexDescriptor = reader.IndexGetForSchema( schema );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					DiffSets<IndexDescriptor> diffSets = Ktx.txState().indexDiffSetsBySchema(schema);
					if ( indexDescriptor != null )
					{
						 if ( diffSets.IsRemoved( indexDescriptor ) )
						 {
							  return IndexReference.NO_INDEX;
						 }
						 else
						 {
							  return indexDescriptor;
						 }
					}
					else
					{
						 IEnumerator<IndexDescriptor> fromTxState = filter( SchemaDescriptor.equalTo( schema ), diffSets.Added.GetEnumerator() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( fromTxState.hasNext() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  return fromTxState.next();
						 }
						 else
						 {
							  return IndexReference.NO_INDEX;
						 }
					}
			  }

			  return indexDescriptor != null ? indexDescriptor : IndexReference.NO_INDEX;
		 }

		 public override IndexReference IndexReferenceUnchecked( int label, params int[] properties )
		 {
			  return IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( label, properties ), null, IndexProviderDescriptor.UNDECIDED );
		 }

		 public override IndexReference IndexReferenceUnchecked( SchemaDescriptor schema )
		 {
			  return IndexDescriptorFactory.forSchema( schema, null, IndexProviderDescriptor.UNDECIDED );
		 }

		 public override IEnumerator<IndexReference> IndexesGetForLabel( int labelId )
		 {
			  acquireSharedLock( ResourceTypes.LABEL, labelId );
			  Ktx.assertOpen();
			  return IndexesGetForLabel( _storageReader, labelId );
		 }

		 internal virtual IEnumerator<IndexReference> IndexesGetForLabel( StorageSchemaReader reader, int labelId )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.storageengine.api.schema.IndexDescriptor> iterator = reader.indexesGetForLabel(labelId);
			  IEnumerator<IndexDescriptor> iterator = reader.IndexesGetForLabel( labelId );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					iterator = Ktx.txState().indexDiffSetsByLabel(labelId).apply(iterator);
			  }
			  //noinspection unchecked
			  return ( System.Collections.IEnumerator ) iterator;
		 }

		 public override IEnumerator<IndexReference> IndexesGetForRelationshipType( int relationshipType )
		 {
			  acquireSharedLock( ResourceTypes.RELATIONSHIP_TYPE, relationshipType );
			  Ktx.assertOpen();
			  return IndexesGetForRelationshipType( _storageReader, relationshipType );
		 }

		 internal virtual IEnumerator<IndexReference> IndexesGetForRelationshipType( StorageSchemaReader reader, int relationshipType )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.storageengine.api.schema.IndexDescriptor> iterator = reader.indexesGetForRelationshipType(relationshipType);
			  IEnumerator<IndexDescriptor> iterator = reader.IndexesGetForRelationshipType( relationshipType );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					iterator = Ktx.txState().indexDiffSetsByRelationshipType(relationshipType).apply(iterator);
			  }
			  //noinspection unchecked
			  return ( System.Collections.IEnumerator ) iterator;
		 }

		 public override IndexReference IndexGetForName( string name )
		 {
			  Ktx.assertOpen();

			  IndexDescriptor index = _storageReader.indexGetForName( name );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					System.Predicate<IndexDescriptor> namePredicate = indexDescriptor =>
					{
					 Optional<string> userSuppliedName = indexDescriptor.UserSuppliedName;
					 //noinspection OptionalIsPresent -- the suggested functional style causes allocation we can trivially avoid.
					 if ( userSuppliedName.Present )
					 {
						  return userSuppliedName.get().Equals(name);
					 }
					 //No name cannot match a name.
					 return false;
					};
					IEnumerator<IndexDescriptor> indexes = Ktx.txState().indexChanges().filterAdded(namePredicate).apply(Iterators.iterator(index));
					index = singleOrNull( indexes );
			  }
			  if ( index == null )
			  {
					return IndexReference.NO_INDEX;
			  }
			  AcquireSharedSchemaLock( index.Schema() );
			  return index;
		 }

		 public override IEnumerator<IndexReference> IndexesGetAll()
		 {
			  Ktx.assertOpen();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.storageengine.api.schema.IndexDescriptor> iterator = indexesGetAll(storageReader);
			  IEnumerator<IndexDescriptor> iterator = IndexesGetAll( _storageReader );

			  return Iterators.map(indexDescriptor =>
			  {
				AcquireSharedSchemaLock( indexDescriptor.schema() );
				return indexDescriptor;
			  }, iterator);
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.storageengine.api.schema.IndexDescriptor> indexesGetAll(org.neo4j.storageengine.api.StorageSchemaReader reader)
		 internal virtual IEnumerator<IndexDescriptor> IndexesGetAll( StorageSchemaReader reader )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.storageengine.api.schema.IndexDescriptor> iterator = reader.indexesGetAll();
			  IEnumerator<IndexDescriptor> iterator = reader.IndexesGetAll();
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					iterator = Ktx.txState().indexChanges().apply(iterator);
			  }
			  return iterator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override InternalIndexState IndexGetState( IndexReference index )
		 {
			  AssertValidIndex( index );
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  return IndexGetState( ( IndexDescriptor ) index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override PopulationProgress IndexGetPopulationProgress( IndexReference index )
		 {
			  AssertValidIndex( index );
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  return IndexGetPopulationProgress( _storageReader, index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(org.neo4j.storageengine.api.StorageSchemaReader reader, org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal virtual PopulationProgress IndexGetPopulationProgress( StorageSchemaReader reader, IndexReference index )
		 {
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					if ( CheckIndexState( ( IndexDescriptor ) index, Ktx.txState().indexDiffSetsBySchema(index.Schema()) ) )
					{
						 return Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.None;
					}
			  }

			  return reader.IndexGetPopulationProgress( index.Schema() );
		 }

		 public override long? IndexGetOwningUniquenessConstraintId( IndexReference index )
		 {
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  if ( index is StoreIndexDescriptor )
			  {
					return ( ( StoreIndexDescriptor ) index ).OwningConstraint;
			  }
			  else
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long indexGetCommittedId(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.kernel.api.exceptions.schema.SchemaRuleNotFoundException
		 public override long IndexGetCommittedId( IndexReference index )
		 {
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  if ( index is StoreIndexDescriptor )
			  {
					return ( ( StoreIndexDescriptor ) index ).Id;
			  }
			  else
			  {
					throw new SchemaRuleNotFoundException( Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind.IndexRule, index.Schema() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String indexGetFailure(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override string IndexGetFailure( IndexReference index )
		 {
			  AssertValidIndex( index );
			  return _storageReader.indexGetFailure( index.Schema() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double indexUniqueValuesSelectivity(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override double IndexUniqueValuesSelectivity( IndexReference index )
		 {
			  AssertValidIndex( index );
			  SchemaDescriptor schema = index.Schema();
			  AcquireSharedSchemaLock( schema );
			  Ktx.assertOpen();
			  return _storageReader.indexUniqueValuesPercentage( schema );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long indexSize(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override long IndexSize( IndexReference index )
		 {
			  AssertValidIndex( index );
			  SchemaDescriptor schema = index.Schema();
			  AcquireSharedSchemaLock( schema );
			  Ktx.assertOpen();
			  return _storageReader.indexSize( schema );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long nodesCountIndexed(org.neo4j.internal.kernel.api.IndexReference index, long nodeId, int propertyKeyId, org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override long NodesCountIndexed( IndexReference index, long nodeId, int propertyKeyId, Value value )
		 {
			  Ktx.assertOpen();
			  AssertValidIndex( index );
			  IndexReader reader = _storageReader.getIndexReader( ( IndexDescriptor ) index );
			  return reader.CountIndexedNodes( nodeId, new int[] { propertyKeyId }, value );
		 }

		 public override long NodesGetCount()
		 {
			  Ktx.assertOpen();
			  long @base = _storageReader.nodesGetCount();
			  return Ktx.hasTxStateWithChanges() ? @base + Ktx.txState().addedAndRemovedNodes().delta() : @base;
		 }

		 public override long RelationshipsGetCount()
		 {
			  Ktx.assertOpen();
			  long @base = _storageReader.relationshipsGetCount();
			  return Ktx.hasTxStateWithChanges() ? @base + Ktx.txState().addedAndRemovedRelationships().delta() : @base;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.register.Register_DoubleLongRegister indexUpdatesAndSize(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.register.Register_DoubleLongRegister target) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override Org.Neo4j.Register.Register_DoubleLongRegister IndexUpdatesAndSize( IndexReference index, Org.Neo4j.Register.Register_DoubleLongRegister target )
		 {
			  Ktx.assertOpen();
			  AssertValidIndex( index );
			  return _storageReader.indexUpdatesAndSize( index.Schema(), target );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.register.Register_DoubleLongRegister indexSample(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.register.Register_DoubleLongRegister target) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override Org.Neo4j.Register.Register_DoubleLongRegister IndexSample( IndexReference index, Org.Neo4j.Register.Register_DoubleLongRegister target )
		 {
			  Ktx.assertOpen();
			  AssertValidIndex( index );
			  return _storageReader.indexSample( index.Schema(), target );
		 }

		 internal virtual IndexReference IndexGetCapability( IndexDescriptor schemaIndexDescriptor )
		 {
			  try
			  {
					return _storageReader.indexReference( schemaIndexDescriptor );
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new System.InvalidOperationException( "Could not find capability for index " + schemaIndexDescriptor, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal virtual InternalIndexState IndexGetState( IndexDescriptor descriptor )
		 {
			  return IndexGetState( _storageReader, descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.storageengine.api.StorageSchemaReader reader, org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal virtual InternalIndexState IndexGetState( StorageSchemaReader reader, IndexDescriptor descriptor )
		 {
			  // If index is in our state, then return populating
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					if ( CheckIndexState( descriptor, Ktx.txState().indexDiffSetsBySchema(descriptor.Schema()) ) )
					{
						 return InternalIndexState.POPULATING;
					}
			  }

			  return reader.IndexGetState( descriptor );
		 }

		 internal virtual long? IndexGetOwningUniquenessConstraintId( IndexDescriptor index )
		 {
			  return _storageReader.indexGetOwningUniquenessConstraintId( index );
		 }

		 internal virtual IndexDescriptor IndexGetForSchema( SchemaDescriptor descriptor )
		 {
			  IndexDescriptor indexDescriptor = _storageReader.indexGetForSchema( descriptor );
			  IEnumerator<IndexDescriptor> indexes = iterator( indexDescriptor );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					indexes = filter( SchemaDescriptor.equalTo( descriptor ), Ktx.txState().indexDiffSetsBySchema(descriptor).apply(indexes) );
			  }
			  return singleOrNull( indexes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean checkIndexState(org.neo4j.storageengine.api.schema.IndexDescriptor index, org.neo4j.storageengine.api.txstate.DiffSets<org.neo4j.storageengine.api.schema.IndexDescriptor> diffSet) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private bool CheckIndexState( IndexDescriptor index, DiffSets<IndexDescriptor> diffSet )
		 {
			  if ( diffSet.IsAdded( index ) )
			  {
					return true;
			  }
			  if ( diffSet.IsRemoved( index ) )
			  {
					throw new IndexNotFoundKernelException( format( "Index on %s has been dropped in this transaction.", index.UserDescription( SchemaUtil.idTokenNameLookup ) ) );
			  }
			  return false;
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor )
		 {
			  AcquireSharedSchemaLock( descriptor );
			  Ktx.assertOpen();
			  IEnumerator<ConstraintDescriptor> constraints = _storageReader.constraintsGetForSchema( descriptor );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					return Ktx.txState().constraintsChangesForSchema(descriptor).apply(constraints);
			  }
			  return constraints;
		 }

		 public override bool ConstraintExists( ConstraintDescriptor descriptor )
		 {
			  SchemaDescriptor schema = descriptor.Schema();
			  AcquireSharedSchemaLock( schema );
			  Ktx.assertOpen();
			  bool inStore = _storageReader.constraintExists( descriptor );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					DiffSets<ConstraintDescriptor> diffSet = Ktx.txState().constraintsChangesForSchema(descriptor.Schema());
					return diffSet.IsAdded( descriptor ) || ( inStore && !diffSet.IsRemoved( descriptor ) );
			  }

			  return inStore;
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId )
		 {
			  acquireSharedLock( ResourceTypes.LABEL, labelId );
			  Ktx.assertOpen();
			  return ConstraintsGetForLabel( _storageReader, labelId );
		 }

		 internal virtual IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( StorageSchemaReader reader, int labelId )
		 {
			  IEnumerator<ConstraintDescriptor> constraints = reader.ConstraintsGetForLabel( labelId );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					return Ktx.txState().constraintsChangesForLabel(labelId).apply(constraints);
			  }
			  return constraints;
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetAll()
		 {
			  Ktx.assertOpen();
			  IEnumerator<ConstraintDescriptor> constraints = ConstraintsGetAll( _storageReader );
			  return Iterators.map( this.lockConstraint, constraints );
		 }

		 internal virtual IEnumerator<ConstraintDescriptor> ConstraintsGetAll( StorageSchemaReader reader )
		 {
			  IEnumerator<ConstraintDescriptor> constraints = reader.ConstraintsGetAll();
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					constraints = Ktx.txState().constraintsChanges().apply(constraints);
			  }
			  return constraints;
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId )
		 {
			  acquireSharedLock( ResourceTypes.RELATIONSHIP_TYPE, typeId );
			  Ktx.assertOpen();
			  return ConstraintsGetForRelationshipType( _storageReader, typeId );
		 }

		 internal virtual IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( StorageSchemaReader reader, int typeId )
		 {
			  IEnumerator<ConstraintDescriptor> constraints = reader.ConstraintsGetForRelationshipType( typeId );
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					return Ktx.txState().constraintsChangesForRelationshipType(typeId).apply(constraints);
			  }
			  return constraints;
		 }

		 public override SchemaReadCore Snapshot()
		 {
			  Ktx.assertOpen();
			  StorageSchemaReader snapshot = _storageReader.schemaSnapshot();
			  return new SchemaReadCoreSnapshot( snapshot, Ktx, this );
		 }

		 internal virtual bool NodeExistsInStore( long id )
		 {
			  return _storageReader.nodeExists( id );
		 }

		 internal virtual void GetOrCreateNodeIndexConfig( string indexName, IDictionary<string, string> customConfig )
		 {
			  _explicitIndexStore.getOrCreateNodeIndexConfig( indexName, customConfig );
		 }

		 internal virtual void GetOrCreateRelationshipIndexConfig( string indexName, IDictionary<string, string> customConfig )
		 {
			  _explicitIndexStore.getOrCreateRelationshipIndexConfig( indexName, customConfig );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String indexGetFailure(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal virtual string IndexGetFailure( IndexDescriptor descriptor )
		 {
			  return _storageReader.indexGetFailure( descriptor.Schema() );
		 }

		 public override UserFunctionHandle FunctionGet( QualifiedName name )
		 {
			  Ktx.assertOpen();
			  return _procedures.function( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.ProcedureHandle procedureGet(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override ProcedureHandle ProcedureGet( QualifiedName name )
		 {
			  Ktx.assertOpen();
			  return _procedures.procedure( name );
		 }

		 public override ISet<ProcedureSignature> ProceduresGetAll()
		 {
			  Ktx.assertOpen();
			  return _procedures.AllProcedures;
		 }

		 public override UserFunctionHandle AggregationFunctionGet( QualifiedName name )
		 {
			  Ktx.assertOpen();
			  return _procedures.aggregationFunction( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallRead(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallRead( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsReads() )
			  {
					throw accessMode.OnViolation( format( "Read operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallReadOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallReadOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWrite(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWrite( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsWrites() )
			  {
					throw accessMode.OnViolation( format( "Write operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.TokenWrite ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWriteOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.TokenWrite ), context );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchema(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchema( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsSchemaWrites() )
			  {
					throw accessMode.OnViolation( format( "Schema operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchemaOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallRead(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallRead( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsReads() )
			  {
					throw accessMode.OnViolation( format( "Read operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallReadOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallReadOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWrite(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWrite( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsWrites() )
			  {
					throw accessMode.OnViolation( format( "Write operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.TokenWrite ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWriteOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.TokenWrite ), context );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchema(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchema( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsSchemaWrites() )
			  {
					throw accessMode.OnViolation( format( "Schema operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchemaOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue functionCall(int id, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue FunctionCall( int id, AnyValue[] arguments )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return callFunction( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue functionCall(org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue FunctionCall( QualifiedName name, AnyValue[] arguments )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return callFunction( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue functionCallOverride(int id, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue FunctionCallOverride( int id, AnyValue[] arguments )
		 {
			  return callFunction( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue functionCallOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue FunctionCallOverride( QualifiedName name, AnyValue[] arguments )
		 {
			  return callFunction( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override UserAggregator AggregationFunction( int id )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return aggregationFunction( id, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override UserAggregator AggregationFunction( QualifiedName name )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return aggregationFunction( name, new RestrictedAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunctionOverride(int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override UserAggregator AggregationFunctionOverride( int id )
		 {
			  return aggregationFunction( id, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunctionOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override UserAggregator AggregationFunctionOverride( QualifiedName name )
		 {
			  return aggregationFunction( name, new OverriddenAccessMode( Ktx.securityContext().mode(), Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Read ) );
		 }

		 public override ValueMapper<object> ValueMapper()
		 {
			  return _procedures.valueMapper();
		 }

		 public override V SchemaStateGetOrCreate<K, V>( K key, System.Func<K, V> creator )
		 {
			  return _schemaState.getOrCreate( key, creator );
		 }

		 public override void SchemaStateFlush()
		 {
			  _schemaState.clear();
		 }

		 internal virtual ExplicitIndexStore ExplicitIndexStore()
		 {
			  return _explicitIndexStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(int id, Object[] input, final org.neo4j.internal.kernel.api.security.AccessMode override, org.neo4j.internal.kernel.api.procs.ProcedureCallContext procedureCallContext) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private RawIterator<object[], ProcedureException> CallProcedure( int id, object[] input, AccessMode @override, ProcedureCallContext procedureCallContext )
		 {
			  Ktx.assertOpen();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.security.SecurityContext procedureSecurityContext = ktx.securityContext().withMode(override);
			  SecurityContext procedureSecurityContext = Ktx.securityContext().withMode(@override);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCall;
			  RawIterator<object[], ProcedureException> procedureCall;
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( procedureSecurityContext ), Statement statement = Ktx.acquireStatement() )
			  {
					procedureCall = _procedures.callProcedure( PrepareContext( procedureSecurityContext, procedureCallContext ), id, input, statement );
			  }
			  return CreateIterator( procedureSecurityContext, procedureCall );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] input, final org.neo4j.internal.kernel.api.security.AccessMode override, org.neo4j.internal.kernel.api.procs.ProcedureCallContext procedureCallContext) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private RawIterator<object[], ProcedureException> CallProcedure( QualifiedName name, object[] input, AccessMode @override, ProcedureCallContext procedureCallContext )
		 {
			  Ktx.assertOpen();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.security.SecurityContext procedureSecurityContext = ktx.securityContext().withMode(override);
			  SecurityContext procedureSecurityContext = Ktx.securityContext().withMode(@override);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCall;
			  RawIterator<object[], ProcedureException> procedureCall;
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( procedureSecurityContext ), Statement statement = Ktx.acquireStatement() )
			  {
					procedureCall = _procedures.callProcedure( PrepareContext( procedureSecurityContext, procedureCallContext ), name, input, statement );
			  }
			  return CreateIterator( procedureSecurityContext, procedureCall );
		 }

		 private RawIterator<object[], ProcedureException> CreateIterator( SecurityContext procedureSecurityContext, RawIterator<object[], ProcedureException> procedureCall )
		 {
			  return new RawIteratorAnonymousInnerClass( this, procedureSecurityContext, procedureCall );
		 }

		 private class RawIteratorAnonymousInnerClass : RawIterator<object[], ProcedureException>
		 {
			 private readonly AllStoreHolder _outerInstance;

			 private SecurityContext _procedureSecurityContext;
			 private RawIterator<object[], ProcedureException> _procedureCall;

			 public RawIteratorAnonymousInnerClass( AllStoreHolder outerInstance, SecurityContext procedureSecurityContext, RawIterator<object[], ProcedureException> procedureCall )
			 {
				 this.outerInstance = outerInstance;
				 this._procedureSecurityContext = procedureSecurityContext;
				 this._procedureCall = procedureCall;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasNext() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public bool hasNext()
			 {
				  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = _outerInstance.ktx.overrideWith( _procedureSecurityContext ) )
				  {
						return _procedureCall.hasNext();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object[] next() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public object[] next()
			 {
				  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = _outerInstance.ktx.overrideWith( _procedureSecurityContext ) )
				  {
						return _procedureCall.next();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.values.AnyValue callFunction(int id, org.neo4j.values.AnyValue[] input, final org.neo4j.internal.kernel.api.security.AccessMode mode) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private AnyValue CallFunction( int id, AnyValue[] input, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.callFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), id, input );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.values.AnyValue callFunction(org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] input, final org.neo4j.internal.kernel.api.security.AccessMode mode) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private AnyValue CallFunction( QualifiedName name, AnyValue[] input, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.callFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), name, input );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(int id, final org.neo4j.internal.kernel.api.security.AccessMode mode) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private UserAggregator AggregationFunction( int id, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.createAggregationFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), id );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(org.neo4j.internal.kernel.api.procs.QualifiedName name, final org.neo4j.internal.kernel.api.security.AccessMode mode) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private UserAggregator AggregationFunction( QualifiedName name, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Org.Neo4j.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.createAggregationFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), name );
			  }
		 }

		 private BasicContext PrepareContext( SecurityContext securityContext, ProcedureCallContext procedureCallContext )
		 {
			  BasicContext ctx = new BasicContext();
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.KernelTransaction, Ktx );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.DatabaseApi, _dataSourceDependencies.resolveDependency( typeof( GraphDatabaseAPI ) ) );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.DependencyResolver, _dataSourceDependencies );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.Thread, Thread.CurrentThread );
			  ClockContext clocks = Ktx.clocks();
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.SystemClock, clocks.SystemClock() );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.StatementClock, clocks.StatementClock() );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.TransactionClock, clocks.TransactionClock() );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.SecurityContext, securityContext );
			  ctx.Put( Org.Neo4j.Kernel.api.proc.Context_Fields.ProcedureCallContext, procedureCallContext );
			  return ctx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertValidIndex(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal static void AssertValidIndex( IndexReference index )
		 {
			  if ( index == IndexReference.NO_INDEX )
			  {
					throw new IndexNotFoundKernelException( "No index was found" );
			  }
		 }

		 private ConstraintDescriptor LockConstraint( ConstraintDescriptor constraint )
		 {
			  SchemaDescriptor schema = constraint.Schema();
			  Ktx.statementLocks().pessimistic().acquireShared(Ktx.lockTracer(), Schema.keyType(), Schema.keyId());
			  return constraint;
		 }
	}

}
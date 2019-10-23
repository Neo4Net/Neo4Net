using System;
using System.Collections.Generic;
using System.Threading;

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

	using Neo4Net.Collections;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using SchemaReadCore = Neo4Net.Kernel.Api.Internal.SchemaReadCore;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.CreateConstraintFailureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
	using ProcedureHandle = Neo4Net.Kernel.Api.Internal.procs.ProcedureHandle;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.Kernel.Api.Internal.procs.QualifiedName;
	using UserAggregator = Neo4Net.Kernel.Api.Internal.procs.UserAggregator;
	using UserFunctionHandle = Neo4Net.Kernel.Api.Internal.procs.UserFunctionHandle;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.schema.SchemaUtil;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using AccessMode = Neo4Net.Kernel.Api.Internal.security.AccessMode;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using ExplicitIndex = Neo4Net.Kernel.api.ExplicitIndex;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using TransactionCountingStateVisitor = Neo4Net.Kernel.api.txstate.TransactionCountingStateVisitor;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using CountsRecordState = Neo4Net.Kernel.Impl.Api.CountsRecordState;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using OverriddenAccessMode = Neo4Net.Kernel.Impl.Api.security.OverriddenAccessMode;
	using RestrictedAccessMode = Neo4Net.Kernel.Impl.Api.security.RestrictedAccessMode;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Register = Neo4Net.Register.Register;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using StorageSchemaReader = Neo4Net.Kernel.Api.StorageEngine.StorageSchemaReader;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using LabelScanReader = Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using Neo4Net.Kernel.Api.StorageEngine.TxState;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.singleOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.register.Registers.newDoubleLongRegister;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Fields.EMPTY;

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
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader indexReader(org.Neo4Net.Kernel.Api.Internal.IndexReference index, boolean fresh) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
//ORIGINAL LINE: ExplicitIndex explicitNodeIndex(String indexName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override ExplicitIndex ExplicitNodeIndex( string indexName )
		 {
			  Ktx.assertOpen();
			  return ExplicitIndexTxState().nodeChanges(indexName);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndex explicitRelationshipIndex(String indexName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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
//ORIGINAL LINE: public java.util.Map<String,String> nodeExplicitIndexGetConfiguration(String indexName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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
//ORIGINAL LINE: public java.util.Map<String,String> relationshipExplicitIndexGetConfiguration(String indexName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
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
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> iterator = reader.indexesGetForLabel(labelId);
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
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> iterator = reader.indexesGetForRelationshipType(relationshipType);
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
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> iterator = indexesGetAll(storageReader);
			  IEnumerator<IndexDescriptor> iterator = IndexesGetAll( _storageReader );

			  return Iterators.map(indexDescriptor =>
			  {
				AcquireSharedSchemaLock( indexDescriptor.schema() );
				return indexDescriptor;
			  }, iterator);
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> indexesGetAll(org.Neo4Net.Kernel.Api.StorageEngine.StorageSchemaReader reader)
		 internal virtual IEnumerator<IndexDescriptor> IndexesGetAll( StorageSchemaReader reader )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> iterator = reader.indexesGetAll();
			  IEnumerator<IndexDescriptor> iterator = reader.IndexesGetAll();
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					iterator = Ktx.txState().indexChanges().apply(iterator);
			  }
			  return iterator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.InternalIndexState indexGetState(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override InternalIndexState IndexGetState( IndexReference index )
		 {
			  AssertValidIndex( index );
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  return IndexGetState( ( IndexDescriptor ) index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress indexGetPopulationProgress(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override PopulationProgress IndexGetPopulationProgress( IndexReference index )
		 {
			  AssertValidIndex( index );
			  AcquireSharedSchemaLock( index.Schema() );
			  Ktx.assertOpen();
			  return IndexGetPopulationProgress( _storageReader, index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress indexGetPopulationProgress(org.Neo4Net.Kernel.Api.StorageEngine.StorageSchemaReader reader, org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 internal virtual PopulationProgress IndexGetPopulationProgress( StorageSchemaReader reader, IndexReference index )
		 {
			  if ( Ktx.hasTxStateWithChanges() )
			  {
					if ( CheckIndexState( ( IndexDescriptor ) index, Ktx.txState().indexDiffSetsBySchema(index.Schema()) ) )
					{
						 return Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress_Fields.None;
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
//ORIGINAL LINE: public long indexGetCommittedId(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
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
					throw new SchemaRuleNotFoundException( Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule_Kind.IndexRule, index.Schema() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String indexGetFailure(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override string IndexGetFailure( IndexReference index )
		 {
			  AssertValidIndex( index );
			  return _storageReader.indexGetFailure( index.Schema() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double indexUniqueValuesSelectivity(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override double IndexUniqueValuesSelectivity( IndexReference index )
		 {
			  AssertValidIndex( index );
			  SchemaDescriptor schema = index.Schema();
			  AcquireSharedSchemaLock( schema );
			  Ktx.assertOpen();
			  return _storageReader.indexUniqueValuesPercentage( schema );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long indexSize(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override long IndexSize( IndexReference index )
		 {
			  AssertValidIndex( index );
			  SchemaDescriptor schema = index.Schema();
			  AcquireSharedSchemaLock( schema );
			  Ktx.assertOpen();
			  return _storageReader.indexSize( schema );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long nodesCountIndexed(org.Neo4Net.Kernel.Api.Internal.IndexReference index, long nodeId, int propertyKeyId, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
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
//ORIGINAL LINE: public org.Neo4Net.register.Register_DoubleLongRegister indexUpdatesAndSize(org.Neo4Net.Kernel.Api.Internal.IndexReference index, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize( IndexReference index, Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  Ktx.assertOpen();
			  AssertValidIndex( index );
			  return _storageReader.indexUpdatesAndSize( index.Schema(), target );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.register.Register_DoubleLongRegister indexSample(org.Neo4Net.Kernel.Api.Internal.IndexReference index, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 public override Neo4Net.Register.Register_DoubleLongRegister IndexSample( IndexReference index, Neo4Net.Register.Register_DoubleLongRegister target )
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
//ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.InternalIndexState indexGetState(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 internal virtual InternalIndexState IndexGetState( IndexDescriptor descriptor )
		 {
			  return IndexGetState( _storageReader, descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.InternalIndexState indexGetState(org.Neo4Net.Kernel.Api.StorageEngine.StorageSchemaReader reader, org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
//ORIGINAL LINE: private boolean checkIndexState(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor index, org.Neo4Net.Kernel.Api.StorageEngine.TxState.DiffSets<org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor> diffSet) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
//ORIGINAL LINE: String indexGetFailure(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.ProcedureHandle procedureGet(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
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
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallRead(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallRead( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsReads() )
			  {
					throw accessMode.OnViolation( format( "Read operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallReadOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallReadOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWrite(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWrite( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsWrites() )
			  {
					throw accessMode.OnViolation( format( "Write operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.TokenWrite ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWriteOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.TokenWrite ), context );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchema(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchema( int id, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsSchemaWrites() )
			  {
					throw accessMode.OnViolation( format( "Schema operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchemaOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( int id, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallRead(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallRead( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsReads() )
			  {
					throw accessMode.OnViolation( format( "Read operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallReadOverride(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallReadOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWrite(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWrite( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsWrites() )
			  {
					throw accessMode.OnViolation( format( "Write operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.TokenWrite ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWriteOverride(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.TokenWrite ), context );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchema(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchema( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  AccessMode accessMode = Ktx.securityContext().mode();
			  if ( !accessMode.AllowsSchemaWrites() )
			  {
					throw accessMode.OnViolation( format( "Schema operations are not allowed for %s.", Ktx.securityContext().description() ) );
			  }
			  return callProcedure( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchemaOverride(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( QualifiedName name, object[] arguments, ProcedureCallContext context )
		 {
			  return callProcedure( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Full ), context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue functionCall(int id, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override AnyValue FunctionCall( int id, AnyValue[] arguments )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return callFunction( id, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue functionCall(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override AnyValue FunctionCall( QualifiedName name, AnyValue[] arguments )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return callFunction( name, arguments, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue functionCallOverride(int id, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override AnyValue FunctionCallOverride( int id, AnyValue[] arguments )
		 {
			  return callFunction( id, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue functionCallOverride(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override AnyValue FunctionCallOverride( QualifiedName name, AnyValue[] arguments )
		 {
			  return callFunction( name, arguments, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunction(int id) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override UserAggregator AggregationFunction( int id )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return aggregationFunction( id, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunction(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override UserAggregator AggregationFunction( QualifiedName name )
		 {
			  if ( !Ktx.securityContext().mode().allowsReads() )
			  {
					throw Ktx.securityContext().mode().onViolation(format("Read operations are not allowed for %s.", Ktx.securityContext().description()));
			  }
			  return aggregationFunction( name, new RestrictedAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunctionOverride(int id) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override UserAggregator AggregationFunctionOverride( int id )
		 {
			  return aggregationFunction( id, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunctionOverride(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public override UserAggregator AggregationFunctionOverride( QualifiedName name )
		 {
			  return aggregationFunction( name, new OverriddenAccessMode( Ktx.securityContext().mode(), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Read ) );
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
//ORIGINAL LINE: private org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> callProcedure(int id, Object[] input, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode override, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext procedureCallContext) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private RawIterator<object[], ProcedureException> CallProcedure( int id, object[] input, AccessMode @override, ProcedureCallContext procedureCallContext )
		 {
			  Ktx.assertOpen();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.security.SecurityContext procedureSecurityContext = ktx.securityContext().withMode(override);
			  SecurityContext procedureSecurityContext = Ktx.securityContext().withMode(@override);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCall;
			  RawIterator<object[], ProcedureException> procedureCall;
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( procedureSecurityContext ), Statement statement = Ktx.acquireStatement() )
			  {
					procedureCall = _procedures.callProcedure( PrepareContext( procedureSecurityContext, procedureCallContext ), id, input, statement );
			  }
			  return CreateIterator( procedureSecurityContext, procedureCall );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> callProcedure(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] input, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode override, org.Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext procedureCallContext) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private RawIterator<object[], ProcedureException> CallProcedure( QualifiedName name, object[] input, AccessMode @override, ProcedureCallContext procedureCallContext )
		 {
			  Ktx.assertOpen();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.security.SecurityContext procedureSecurityContext = ktx.securityContext().withMode(override);
			  SecurityContext procedureSecurityContext = Ktx.securityContext().withMode(@override);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCall;
			  RawIterator<object[], ProcedureException> procedureCall;
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( procedureSecurityContext ), Statement statement = Ktx.acquireStatement() )
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
//ORIGINAL LINE: public boolean hasNext() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			 public bool hasNext()
			 {
				  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = _outerInstance.ktx.overrideWith( _procedureSecurityContext ) )
				  {
						return _procedureCall.hasNext();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object[] next() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			 public object[] next()
			 {
				  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = _outerInstance.ktx.overrideWith( _procedureSecurityContext ) )
				  {
						return _procedureCall.next();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.AnyValue callFunction(int id, org.Neo4Net.values.AnyValue[] input, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode mode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private AnyValue CallFunction( int id, AnyValue[] input, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.callFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), id, input );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.values.AnyValue callFunction(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, org.Neo4Net.values.AnyValue[] input, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode mode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private AnyValue CallFunction( QualifiedName name, AnyValue[] input, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.callFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), name, input );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunction(int id, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode mode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private UserAggregator AggregationFunction( int id, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.createAggregationFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), id );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator aggregationFunction(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, final org.Neo4Net.Kernel.Api.Internal.security.AccessMode mode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private UserAggregator AggregationFunction( QualifiedName name, AccessMode mode )
		 {
			  Ktx.assertOpen();

			  SecurityContext securityContext = Ktx.securityContext().withMode(mode);
			  using ( Neo4Net.Kernel.api.KernelTransaction_Revertable ignore = Ktx.overrideWith( securityContext ) )
			  {
					return _procedures.createAggregationFunction( PrepareContext( securityContext, ProcedureCallContext.EMPTY ), name );
			  }
		 }

		 private BasicContext PrepareContext( SecurityContext securityContext, ProcedureCallContext procedureCallContext )
		 {
			  BasicContext ctx = new BasicContext();
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.KernelTransaction, Ktx );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.DatabaseApi, _dataSourceDependencies.resolveDependency( typeof( GraphDatabaseAPI ) ) );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.DependencyResolver, _dataSourceDependencies );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.Thread, Thread.CurrentThread );
			  ClockContext clocks = Ktx.clocks();
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.SystemClock, clocks.SystemClock() );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.StatementClock, clocks.StatementClock() );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.TransactionClock, clocks.TransactionClock() );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.SecurityContext, securityContext );
			  ctx.Put( Neo4Net.Kernel.api.proc.Context_Fields.ProcedureCallContext, procedureCallContext );
			  return ctx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertValidIndex(org.Neo4Net.Kernel.Api.Internal.IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
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
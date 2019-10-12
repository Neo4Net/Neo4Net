using System.Collections.Generic;

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
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using NodeExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeExplicitIndexCursor;
	using NodeLabelIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeLabelIndexCursor;
	using NodeValueIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using RelationshipExplicitIndexCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipExplicitIndexCursor;
	using RelationshipGroupCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipGroupCursor;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using RelationshipTraversalCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipTraversalCursor;
	using Org.Neo4j.@internal.Kernel.Api;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using AssertOpen = Org.Neo4j.Kernel.api.AssertOpen;
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using ExplicitIndexHits = Org.Neo4j.Kernel.api.ExplicitIndexHits;
	using IndexBrokenKernelException = Org.Neo4j.Kernel.Api.Exceptions.schema.IndexBrokenKernelException;
	using ExplicitIndexTransactionState = Org.Neo4j.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Org.Neo4j.Kernel.api.txstate.TransactionState;
	using TxStateHolder = Org.Neo4j.Kernel.api.txstate.TxStateHolder;
	using AuxiliaryTransactionState = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.schema.SchemaDescriptor.schemaTokenLockingIds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.GEOMETRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.NUMBER;

	internal abstract class Read : TxStateHolder, Org.Neo4j.@internal.Kernel.Api.Read, Org.Neo4j.@internal.Kernel.Api.ExplicitIndexRead, Org.Neo4j.@internal.Kernel.Api.SchemaRead, Org.Neo4j.@internal.Kernel.Api.Procedures, Org.Neo4j.@internal.Kernel.Api.Locks, AssertOpen, LockingNodeUniqueIndexSeek.UniqueNodeIndexSeeker<DefaultNodeValueIndexCursor>
	{
		public abstract void NodeIndexSeekWithFreshIndexReader( CURSOR cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] predicates );
		public abstract Org.Neo4j.Values.ValueMapper<object> ValueMapper();
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator AggregationFunctionOverride( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name );
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator AggregationFunctionOverride( int id );
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator AggregationFunction( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name );
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator AggregationFunction( int id );
		public abstract Org.Neo4j.Values.AnyValue FunctionCallOverride( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, Org.Neo4j.Values.AnyValue[] arguments );
		public abstract Org.Neo4j.Values.AnyValue FunctionCallOverride( int id, Org.Neo4j.Values.AnyValue[] arguments );
		public abstract Org.Neo4j.Values.AnyValue FunctionCall( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, Org.Neo4j.Values.AnyValue[] arguments );
		public abstract Org.Neo4j.Values.AnyValue FunctionCall( int id, Org.Neo4j.Values.AnyValue[] arguments );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchemaOverride( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchema( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWriteOverride( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWrite( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallReadOverride( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallRead( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchemaOverride( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchema( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWriteOverride( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWrite( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallReadOverride( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Org.Neo4j.Collection.RawIterator<object[], Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallRead( int id, object[] arguments, Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract ISet<Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature> ProceduresGetAll();
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.ProcedureHandle ProcedureGet( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name );
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionHandle AggregationFunctionGet( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name );
		public abstract Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionHandle FunctionGet( Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName name );
		public abstract IEnumerator<Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetAll();
		public abstract IEnumerator<Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId );
		public abstract IEnumerator<Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForLabel( int labelId );
		public abstract string IndexGetFailure( IndexReference index );
		public abstract Org.Neo4j.Storageengine.Api.schema.PopulationProgress IndexGetPopulationProgress( IndexReference index );
		public abstract InternalIndexState IndexGetState( IndexReference index );
		public abstract IEnumerator<IndexReference> IndexesGetAll();
		public abstract IEnumerator<IndexReference> IndexesGetForRelationshipType( int relationshipType );
		public abstract IEnumerator<IndexReference> IndexesGetForLabel( int labelId );
		public abstract IndexReference Index( SchemaDescriptor schema );
		public abstract void SchemaStateFlush();
		public abstract V SchemaStateGetOrCreate( K key, System.Func<K, V> creator );
		public abstract long? IndexGetOwningUniquenessConstraintId( IndexReference index );
		public abstract SchemaReadCore Snapshot();
		public abstract bool ConstraintExists( Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor descriptor );
		public abstract IEnumerator<Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor );
		public abstract Org.Neo4j.Register.Register_DoubleLongRegister IndexSample( IndexReference index, Org.Neo4j.Register.Register_DoubleLongRegister target );
		public abstract Org.Neo4j.Register.Register_DoubleLongRegister IndexUpdatesAndSize( IndexReference index, Org.Neo4j.Register.Register_DoubleLongRegister target );
		public abstract long NodesCountIndexed( IndexReference index, long nodeId, int propertyKeyId, Value value );
		public abstract long IndexSize( IndexReference index );
		public abstract double IndexUniqueValuesSelectivity( IndexReference index );
		public abstract long IndexGetCommittedId( IndexReference index );
		public abstract IndexReference IndexGetForName( string name );
		public abstract IndexReference IndexReferenceUnchecked( SchemaDescriptor schema );
		public abstract IndexReference IndexReferenceUnchecked( int label, params int[] properties );
		public abstract IDictionary<string, string> RelationshipExplicitIndexGetConfiguration( string indexName );
		public abstract string[] RelationshipExplicitIndexesGetAll();
		public abstract string[] NodeExplicitIndexesGetAll();
		public abstract bool RelationshipExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration );
		public abstract IDictionary<string, string> NodeExplicitIndexGetConfiguration( string indexName );
		public abstract bool NodeExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration );
		public abstract Value NodePropertyChangeInTransactionOrNull( long node, int propertyKeyId );
		public abstract bool RelationshipDeletedInTransaction( long relationship );
		public abstract bool NodeDeletedInTransaction( long node );
		public abstract bool RelationshipExists( long reference );
		public abstract long RelationshipsGetCount();
		public abstract long NodesGetCount();
		public abstract long CountsForRelationshipWithoutTxState( int startLabelId, int typeId, int endLabelId );
		public abstract long CountsForRelationship( int startLabelId, int typeId, int endLabelId );
		public abstract long CountsForNodeWithoutTxState( int labelId );
		public abstract long CountsForNode( int labelId );
		public abstract bool NodeExists( long reference );
		public abstract long LockingNodeUniqueIndexSeek( IndexReference index, params IndexQuery.ExactPredicate[] predicates );
		public abstract void NodeIndexSeek( IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query );
		 private readonly DefaultCursors _cursors;
		 internal readonly KernelTransactionImplementation Ktx;

		 internal Read( DefaultCursors cursors, KernelTransactionImplementation ktx )
		 {
			  this._cursors = cursors;
			  this.Ktx = ktx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeIndexSeek(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.NodeValueIndexCursor cursor, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override void NodeIndexSeek( IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  Ktx.assertOpen();
			  if ( HasForbiddenProperties( index ) )
			  {
					cursor.Close();
					return;
			  }

			  DefaultNodeValueIndexCursor cursorImpl = ( DefaultNodeValueIndexCursor ) cursor;
			  IndexReader reader = IndexReader( index, false );
			  cursorImpl.Read = this;
			  Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient withFullPrecision = InjectFullValuePrecision( cursorImpl, query, reader );
			  reader.Query( withFullPrecision, indexOrder, needsValues, query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeIndexDistinctValues(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.NodeValueIndexCursor cursor, boolean needsValues) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override void NodeIndexDistinctValues( IndexReference index, NodeValueIndexCursor cursor, bool needsValues )
		 {
			  Ktx.assertOpen();
			  DefaultNodeValueIndexCursor cursorImpl = ( DefaultNodeValueIndexCursor ) cursor;
			  IndexReader reader = IndexReader( index, true );
			  cursorImpl.Read = this;
			  using ( CursorPropertyAccessor accessor = new CursorPropertyAccessor( _cursors.allocateNodeCursor(), _cursors.allocatePropertyCursor(), this ) )
			  {
					reader.DistinctValues( cursorImpl, accessor, needsValues );
			  }
		 }

		 private Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient InjectFullValuePrecision( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexQuery[] query, IndexReader reader )
		 {
			  Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient target = cursor;
			  if ( !reader.HasFullValuePrecision( query ) )
			  {
					IndexQuery[] filters = new IndexQuery[query.Length];
					int count = 0;
					for ( int i = 0; i < query.Length; i++ )
					{
						 IndexQuery q = query[i];
						 switch ( q.Type() )
						 {
						 case range:
							  ValueGroup valueGroup = q.ValueGroup();
							  if ( ( valueGroup == NUMBER || valueGroup == GEOMETRY ) && !reader.HasFullValuePrecision( q ) )
							  {
									filters[i] = q;
									count++;
							  }
							  break;
						 case exact:
							  Value value = ( ( IndexQuery.ExactPredicate ) q ).value();
							  if ( value.ValueGroup() == ValueGroup.NUMBER || Values.isArrayValue(value) || value.ValueGroup() == ValueGroup.GEOMETRY )
							  {
									if ( !reader.HasFullValuePrecision( q ) )
									{
										 filters[i] = q;
										 count++;
									}
							  }
							  break;
						 default:
							  break;
						 }
					}
					if ( count > 0 )
					{
						 // filters[] can contain null elements. The non-null elements are the filters and each sit in the designated slot
						 // matching the values from the index.
						 target = new NodeValueClientFilter( target, _cursors.allocateNodeCursor(), _cursors.allocatePropertyCursor(), this, filters );
					}
			  }
			  return target;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long lockingNodeUniqueIndexSeek(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.kernel.api.exceptions.schema.IndexBrokenKernelException
		 public override long LockingNodeUniqueIndexSeek( IndexReference index, params IndexQuery.ExactPredicate[] predicates )
		 {
			  AssertIndexOnline( index );
			  AssertPredicatesMatchSchema( index, predicates );

			  Org.Neo4j.Kernel.impl.locking.Locks_Client locks = Ktx.statementLocks().optimistic();
			  LockTracer lockTracer = Ktx.lockTracer();

			  return LockingNodeUniqueIndexSeek.Apply( locks, lockTracer, _cursors.allocateNodeValueIndexCursor, this, this, index, predicates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeIndexSeekWithFreshIndexReader(DefaultNodeValueIndexCursor cursor, org.neo4j.storageengine.api.schema.IndexReader indexReader, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void NodeIndexSeekWithFreshIndexReader( DefaultNodeValueIndexCursor cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] query )
		 {
			  cursor.Read = this;
			  Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient target = InjectFullValuePrecision( cursor, query, indexReader );
			  // we never need values for exact predicates
			  indexReader.Query( target, IndexOrder.NONE, false, query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeIndexScan(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.NodeValueIndexCursor cursor, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void NodeIndexScan( IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues )
		 {
			  Ktx.assertOpen();
			  if ( HasForbiddenProperties( index ) )
			  {
					cursor.Close();
					return;
			  }

			  // for a scan, we simply query for existence of the first property, which covers all entries in an index
			  int firstProperty = index.Properties()[0];

			  DefaultNodeValueIndexCursor cursorImpl = ( DefaultNodeValueIndexCursor ) cursor;
			  cursorImpl.Read = this;
			  IndexReader( index, false ).query( cursorImpl, indexOrder, needsValues, IndexQuery.exists( firstProperty ) );
		 }

		 private bool HasForbiddenProperties( IndexReference index )
		 {
			  AccessMode mode = Ktx.securityContext().mode();
			  foreach ( int prop in index.Properties() )
			  {
					if ( !mode.AllowsPropertyReads( prop ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void NodeLabelScan( int label, NodeLabelIndexCursor cursor )
		 {
			  Ktx.assertOpen();

			  DefaultNodeLabelIndexCursor indexCursor = ( DefaultNodeLabelIndexCursor ) cursor;
			  indexCursor.Read = this;
			  LabelScanReader().nodesWithLabel(indexCursor, label);
		 }

		 public override void NodeLabelUnionScan( NodeLabelIndexCursor cursor, params int[] labels )
		 {
			  Ktx.assertOpen();

			  DefaultNodeLabelIndexCursor client = ( DefaultNodeLabelIndexCursor ) cursor;
			  client.Read = this;
			  client.UnionScan( new NodeLabelIndexProgressor( LabelScanReader().nodesWithAnyOfLabels(labels), client ), false, labels );
		 }

		 public override void NodeLabelIntersectionScan( NodeLabelIndexCursor cursor, params int[] labels )
		 {
			  Ktx.assertOpen();

			  DefaultNodeLabelIndexCursor client = ( DefaultNodeLabelIndexCursor ) cursor;
			  client.Read = this;
			  client.IntersectionScan( new NodeLabelIndexProgressor( LabelScanReader().nodesWithAllLabels(labels), client ), false, labels );
		 }

		 public override Scan<NodeLabelIndexCursor> NodeLabelScan( int label )
		 {
			  Ktx.assertOpen();
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void AllNodesScan( NodeCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeCursor ) cursor ).Scan( this );
		 }

		 public override Scan<NodeCursor> AllNodesScan()
		 {
			  Ktx.assertOpen();
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void SingleNode( long reference, NodeCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeCursor ) cursor ).Single( reference, this );
		 }

		 public override void SingleRelationship( long reference, RelationshipScanCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipScanCursor ) cursor ).Single( reference, this );
		 }

		 public override void AllRelationshipsScan( RelationshipScanCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipScanCursor ) cursor ).Scan( -1, this );
		 }

		 public override Scan<RelationshipScanCursor> AllRelationshipsScan()
		 {
			  Ktx.assertOpen();
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void RelationshipTypeScan( int type, RelationshipScanCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipScanCursor ) cursor ).Scan( type, this );
		 }

		 public override Scan<RelationshipScanCursor> RelationshipTypeScan( int type )
		 {
			  Ktx.assertOpen();
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void RelationshipGroups( long nodeReference, long reference, RelationshipGroupCursor cursor )
		 {
			  ( ( DefaultRelationshipGroupCursor ) cursor ).Init( nodeReference, reference, this );
		 }

		 public override void Relationships( long nodeReference, long reference, RelationshipTraversalCursor cursor )
		 {
			  ( ( DefaultRelationshipTraversalCursor ) cursor ).Init( nodeReference, reference, this );
		 }

		 public override void NodeProperties( long nodeReference, long reference, PropertyCursor cursor )
		 {
			  ( ( DefaultPropertyCursor ) cursor ).InitNode( nodeReference, reference, this, Ktx );
		 }

		 public override void RelationshipProperties( long relationshipReference, long reference, PropertyCursor cursor )
		 {
			  ( ( DefaultPropertyCursor ) cursor ).InitRelationship( relationshipReference, reference, this, Ktx );
		 }

		 public override void GraphProperties( PropertyCursor cursor )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultPropertyCursor ) cursor ).InitGraph( GraphPropertiesReference(), this, Ktx );
		 }

		 internal abstract long GraphPropertiesReference();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeExplicitIndexLookup(org.neo4j.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexLookup( NodeExplicitIndexCursor cursor, string index, string key, object value )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).get( key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeExplicitIndexQuery(org.neo4j.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, Object query) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, object query )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).query( query is Value ? ( ( Value ) query ).asObject() : query ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeExplicitIndexQuery(org.neo4j.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, String key, Object query) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, string key, object query )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).query( key, query is Value ? ( ( Value ) query ).asObject() : query ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexLookup(org.neo4j.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, String key, Object value, long source, long target) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexLookup( RelationshipExplicitIndexCursor cursor, string index, string key, object value, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).get( key, value, source, target ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexQuery(org.neo4j.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, Object query, long source, long target) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, object query, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).query( query is Value ? ( ( Value ) query ).asObject() : query, source, target ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexQuery(org.neo4j.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, String key, Object query, long source, long target) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, string key, object query, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).query( key, query is Value ? ( ( Value ) query ).asObject() : query, source, target ) );
		 }

		 private static void ExplicitIndex( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_ExplicitClient client, ExplicitIndexHits hits )
		 {
			  client.Initialize( new ExplicitIndexProgressor( hits, client ), hits.Size() );
		 }

		 public override void FutureNodeReferenceRead( long reference )
		 {
			  Ktx.assertOpen();
		 }

		 public override void FutureRelationshipsReferenceRead( long reference )
		 {
			  Ktx.assertOpen();
		 }

		 public override void FutureNodePropertyReferenceRead( long reference )
		 {
			  Ktx.assertOpen();
		 }

		 public override void FutureRelationshipPropertyReferenceRead( long reference )
		 {
			  Ktx.assertOpen();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.storageengine.api.schema.IndexReader indexReader(org.neo4j.internal.kernel.api.IndexReference index, boolean fresh) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 public abstract IndexReader IndexReader( IndexReference index, bool fresh );

		 internal abstract LabelScanReader LabelScanReader();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.kernel.api.ExplicitIndex explicitNodeIndex(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 internal abstract ExplicitIndex ExplicitNodeIndex( string indexName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.kernel.api.ExplicitIndex explicitRelationshipIndex(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 internal abstract ExplicitIndex ExplicitRelationshipIndex( string indexName );

		 public override abstract IndexReference Index( int label, params int[] properties );

		 public override TransactionState TxState()
		 {
			  return Ktx.txState();
		 }

		 public override AuxiliaryTransactionState AuxiliaryTxState( object providerIdentityKey )
		 {
			  return Ktx.auxiliaryTxState( providerIdentityKey );
		 }

		 public override ExplicitIndexTransactionState ExplicitIndexTxState()
		 {
			  return Ktx.explicitIndexTxState();
		 }

		 public override bool HasTxStateWithChanges()
		 {
			  return Ktx.hasTxStateWithChanges();
		 }

		 public override void AcquireExclusiveNodeLock( params long[] ids )
		 {
			  AcquireExclusiveLock( ResourceTypes.NODE, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireExclusiveRelationshipLock( params long[] ids )
		 {
			  AcquireExclusiveLock( ResourceTypes.RELATIONSHIP, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireExclusiveExplicitIndexLock( params long[] ids )
		 {
			  AcquireExclusiveLock( ResourceTypes.EXPLICIT_INDEX, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireExclusiveLabelLock( params long[] ids )
		 {
			  AcquireExclusiveLock( ResourceTypes.LABEL, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseExclusiveNodeLock( params long[] ids )
		 {
			  ReleaseExclusiveLock( ResourceTypes.NODE, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseExclusiveRelationshipLock( params long[] ids )
		 {
			  ReleaseExclusiveLock( ResourceTypes.RELATIONSHIP, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseExclusiveExplicitIndexLock( params long[] ids )
		 {
			  ReleaseExclusiveLock( ResourceTypes.EXPLICIT_INDEX, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseExclusiveLabelLock( params long[] ids )
		 {
			  ReleaseExclusiveLock( ResourceTypes.LABEL, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireSharedNodeLock( params long[] ids )
		 {
			  acquireSharedLock( ResourceTypes.NODE, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireSharedRelationshipLock( params long[] ids )
		 {
			  acquireSharedLock( ResourceTypes.RELATIONSHIP, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireSharedExplicitIndexLock( params long[] ids )
		 {
			  acquireSharedLock( ResourceTypes.EXPLICIT_INDEX, ids );
			  Ktx.assertOpen();
		 }

		 public override void AcquireSharedLabelLock( params long[] ids )
		 {
			  acquireSharedLock( ResourceTypes.LABEL, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseSharedNodeLock( params long[] ids )
		 {
			  ReleaseSharedLock( ResourceTypes.NODE, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseSharedRelationshipLock( params long[] ids )
		 {
			  ReleaseSharedLock( ResourceTypes.RELATIONSHIP, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseSharedExplicitIndexLock( params long[] ids )
		 {
			  ReleaseSharedLock( ResourceTypes.EXPLICIT_INDEX, ids );
			  Ktx.assertOpen();
		 }

		 public override void ReleaseSharedLabelLock( params long[] ids )
		 {
			  ReleaseSharedLock( ResourceTypes.LABEL, ids );
			  Ktx.assertOpen();
		 }

		 internal virtual void AcquireSharedSchemaLock( SchemaDescriptor schema )
		 {
			  long[] lockingIds = schemaTokenLockingIds( schema );
			  Ktx.statementLocks().optimistic().acquireShared(Ktx.lockTracer(), Schema.keyType(), lockingIds);
		 }

		 internal virtual void AcquireSharedLock( ResourceType resource, long resourceId )
		 {
			  Ktx.statementLocks().optimistic().acquireShared(Ktx.lockTracer(), resource, resourceId);
		 }

		 private void AcquireExclusiveLock( ResourceTypes types, params long[] ids )
		 {
			  Ktx.statementLocks().pessimistic().acquireExclusive(Ktx.lockTracer(), types, ids);
		 }

		 private void ReleaseExclusiveLock( ResourceTypes types, params long[] ids )
		 {
			  Ktx.statementLocks().pessimistic().releaseExclusive(types, ids);
		 }

		 private void AcquireSharedLock( ResourceTypes types, params long[] ids )
		 {
			  Ktx.statementLocks().pessimistic().acquireShared(Ktx.lockTracer(), types, ids);
		 }

		 private void ReleaseSharedLock( ResourceTypes types, params long[] ids )
		 {
			  Ktx.statementLocks().pessimistic().releaseShared(types, ids);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexOnline(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.kernel.api.exceptions.schema.IndexBrokenKernelException
		 private void AssertIndexOnline( IndexReference index )
		 {
			  switch ( IndexGetState( index ) )
			  {
			  case ONLINE:
					return;
			  default:
					throw new IndexBrokenKernelException( IndexGetFailure( index ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertPredicatesMatchSchema(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate[] predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 private static void AssertPredicatesMatchSchema( IndexReference index, IndexQuery.ExactPredicate[] predicates )
		 {
			  int[] propertyIds = index.Properties();
			  if ( propertyIds.Length != predicates.Length )
			  {
					throw new IndexNotApplicableKernelException( format( "The index specifies %d properties, but only %d lookup predicates were given.", propertyIds.Length, predicates.Length ) );
			  }
			  for ( int i = 0; i < predicates.Length; i++ )
			  {
					if ( predicates[i].PropertyKeyId() != propertyIds[i] )
					{
						 throw new IndexNotApplicableKernelException( format( "The index has the property id %d in position %d, but the lookup property id was %d.", propertyIds[i], i, predicates[i].PropertyKeyId() ) );
					}
			  }
		 }

		 public override void AssertOpen()
		 {
			  Ktx.assertOpen();
		 }
	}

}
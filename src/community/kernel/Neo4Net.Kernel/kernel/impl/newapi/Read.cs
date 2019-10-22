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
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using NodeExplicitIndexCursor = Neo4Net.Internal.Kernel.Api.NodeExplicitIndexCursor;
	using NodeLabelIndexCursor = Neo4Net.Internal.Kernel.Api.NodeLabelIndexCursor;
	using NodeValueIndexCursor = Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using RelationshipExplicitIndexCursor = Neo4Net.Internal.Kernel.Api.RelationshipExplicitIndexCursor;
	using RelationshipGroupCursor = Neo4Net.Internal.Kernel.Api.RelationshipGroupCursor;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using RelationshipTraversalCursor = Neo4Net.Internal.Kernel.Api.RelationshipTraversalCursor;
	using Neo4Net.Internal.Kernel.Api;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using ExplicitIndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using IndexNotApplicableKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using AccessMode = Neo4Net.Internal.Kernel.Api.security.AccessMode;
	using AssertOpen = Neo4Net.Kernel.api.AssertOpen;
	using ExplicitIndex = Neo4Net.Kernel.api.ExplicitIndex;
	using ExplicitIndexHits = Neo4Net.Kernel.api.ExplicitIndexHits;
	using IndexBrokenKernelException = Neo4Net.Kernel.Api.Exceptions.schema.IndexBrokenKernelException;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.SchemaDescriptor.schemaTokenLockingIds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ValueGroup.GEOMETRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ValueGroup.NUMBER;

	internal abstract class Read : TxStateHolder, Neo4Net.Internal.Kernel.Api.Read, Neo4Net.Internal.Kernel.Api.ExplicitIndexRead, Neo4Net.Internal.Kernel.Api.SchemaRead, Neo4Net.Internal.Kernel.Api.Procedures, Neo4Net.Internal.Kernel.Api.Locks, AssertOpen, LockingNodeUniqueIndexSeek.UniqueNodeIndexSeeker<DefaultNodeValueIndexCursor>
	{
		public abstract void NodeIndexSeekWithFreshIndexReader( CURSOR cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] predicates );
		public abstract Neo4Net.Values.ValueMapper<object> ValueMapper();
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserAggregator AggregationFunctionOverride( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name );
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserAggregator AggregationFunctionOverride( int id );
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserAggregator AggregationFunction( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name );
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserAggregator AggregationFunction( int id );
		public abstract Neo4Net.Values.AnyValue FunctionCallOverride( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, Neo4Net.Values.AnyValue[] arguments );
		public abstract Neo4Net.Values.AnyValue FunctionCallOverride( int id, Neo4Net.Values.AnyValue[] arguments );
		public abstract Neo4Net.Values.AnyValue FunctionCall( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, Neo4Net.Values.AnyValue[] arguments );
		public abstract Neo4Net.Values.AnyValue FunctionCall( int id, Neo4Net.Values.AnyValue[] arguments );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchemaOverride( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchema( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWriteOverride( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWrite( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallReadOverride( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallRead( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchemaOverride( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallSchema( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWriteOverride( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallWrite( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallReadOverride( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract Neo4Net.Collections.RawIterator<object[], Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException> ProcedureCallRead( int id, object[] arguments, Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext context );
		public abstract ISet<Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature> ProceduresGetAll();
		public abstract Neo4Net.Internal.Kernel.Api.procs.ProcedureHandle ProcedureGet( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name );
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserFunctionHandle AggregationFunctionGet( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name );
		public abstract Neo4Net.Internal.Kernel.Api.procs.UserFunctionHandle FunctionGet( Neo4Net.Internal.Kernel.Api.procs.QualifiedName name );
		public abstract IEnumerator<Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetAll();
		public abstract IEnumerator<Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId );
		public abstract IEnumerator<Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForLabel( int labelId );
		public abstract string IndexGetFailure( IndexReference index );
		public abstract Neo4Net.Storageengine.Api.schema.PopulationProgress IndexGetPopulationProgress( IndexReference index );
		public abstract InternalIndexState IndexGetState( IndexReference index );
		public abstract IEnumerator<IndexReference> IndexesGetAll();
		public abstract IEnumerator<IndexReference> IndexesGetForRelationshipType( int relationshipType );
		public abstract IEnumerator<IndexReference> IndexesGetForLabel( int labelId );
		public abstract IndexReference Index( SchemaDescriptor schema );
		public abstract void SchemaStateFlush();
		public abstract V SchemaStateGetOrCreate( K key, System.Func<K, V> creator );
		public abstract long? IndexGetOwningUniquenessConstraintId( IndexReference index );
		public abstract SchemaReadCore Snapshot();
		public abstract bool ConstraintExists( Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor descriptor );
		public abstract IEnumerator<Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor );
		public abstract Neo4Net.Register.Register_DoubleLongRegister IndexSample( IndexReference index, Neo4Net.Register.Register_DoubleLongRegister target );
		public abstract Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize( IndexReference index, Neo4Net.Register.Register_DoubleLongRegister target );
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
//ORIGINAL LINE: public final void nodeIndexSeek(org.Neo4Net.internal.kernel.api.IndexReference index, org.Neo4Net.internal.kernel.api.NodeValueIndexCursor cursor, org.Neo4Net.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.Neo4Net.internal.kernel.api.IndexQuery... query) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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
			  Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient withFullPrecision = InjectFullValuePrecision( cursorImpl, query, reader );
			  reader.Query( withFullPrecision, indexOrder, needsValues, query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeIndexDistinctValues(org.Neo4Net.internal.kernel.api.IndexReference index, org.Neo4Net.internal.kernel.api.NodeValueIndexCursor cursor, boolean needsValues) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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

		 private Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient InjectFullValuePrecision( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexQuery[] query, IndexReader reader )
		 {
			  Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient target = cursor;
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
//ORIGINAL LINE: public long lockingNodeUniqueIndexSeek(org.Neo4Net.internal.kernel.api.IndexReference index, org.Neo4Net.internal.kernel.api.IndexQuery.ExactPredicate... predicates) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.schema.IndexBrokenKernelException
		 public override long LockingNodeUniqueIndexSeek( IndexReference index, params IndexQuery.ExactPredicate[] predicates )
		 {
			  AssertIndexOnline( index );
			  AssertPredicatesMatchSchema( index, predicates );

			  Neo4Net.Kernel.impl.locking.Locks_Client locks = Ktx.statementLocks().optimistic();
			  LockTracer lockTracer = Ktx.lockTracer();

			  return LockingNodeUniqueIndexSeek.Apply( locks, lockTracer, _cursors.allocateNodeValueIndexCursor, this, this, index, predicates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nodeIndexSeekWithFreshIndexReader(DefaultNodeValueIndexCursor cursor, org.Neo4Net.storageengine.api.schema.IndexReader indexReader, org.Neo4Net.internal.kernel.api.IndexQuery.ExactPredicate... query) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void NodeIndexSeekWithFreshIndexReader( DefaultNodeValueIndexCursor cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] query )
		 {
			  cursor.Read = this;
			  Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient target = InjectFullValuePrecision( cursor, query, indexReader );
			  // we never need values for exact predicates
			  indexReader.Query( target, IndexOrder.NONE, false, query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeIndexScan(org.Neo4Net.internal.kernel.api.IndexReference index, org.Neo4Net.internal.kernel.api.NodeValueIndexCursor cursor, org.Neo4Net.internal.kernel.api.IndexOrder indexOrder, boolean needsValues) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
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
//ORIGINAL LINE: public final void nodeExplicitIndexLookup(org.Neo4Net.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexLookup( NodeExplicitIndexCursor cursor, string index, string key, object value )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).get( key, value ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeExplicitIndexQuery(org.Neo4Net.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, Object query) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, object query )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).query( query is Value ? ( ( Value ) query ).asObject() : query ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void nodeExplicitIndexQuery(org.Neo4Net.internal.kernel.api.NodeExplicitIndexCursor cursor, String index, String key, Object query) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, string key, object query )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultNodeExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultNodeExplicitIndexCursor ) cursor, ExplicitNodeIndex( index ).query( key, query is Value ? ( ( Value ) query ).asObject() : query ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexLookup(org.Neo4Net.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, String key, Object value, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexLookup( RelationshipExplicitIndexCursor cursor, string index, string key, object value, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).get( key, value, source, target ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexQuery(org.Neo4Net.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, Object query, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, object query, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).query( query is Value ? ( ( Value ) query ).asObject() : query, source, target ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipExplicitIndexQuery(org.Neo4Net.internal.kernel.api.RelationshipExplicitIndexCursor cursor, String index, String key, Object query, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public override void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, string key, object query, long source, long target )
		 {
			  Ktx.assertOpen();
			  ( ( DefaultRelationshipExplicitIndexCursor ) cursor ).Read = this;
			  ExplicitIndex( ( DefaultRelationshipExplicitIndexCursor ) cursor, ExplicitRelationshipIndex( index ).query( key, query is Value ? ( ( Value ) query ).asObject() : query, source, target ) );
		 }

		 private static void ExplicitIndex( Neo4Net.Storageengine.Api.schema.IndexProgressor_ExplicitClient client, ExplicitIndexHits hits )
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
//ORIGINAL LINE: public abstract org.Neo4Net.storageengine.api.schema.IndexReader indexReader(org.Neo4Net.internal.kernel.api.IndexReference index, boolean fresh) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 public abstract IndexReader IndexReader( IndexReference index, bool fresh );

		 internal abstract LabelScanReader LabelScanReader();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.Neo4Net.kernel.api.ExplicitIndex explicitNodeIndex(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 internal abstract ExplicitIndex ExplicitNodeIndex( string indexName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.Neo4Net.kernel.api.ExplicitIndex explicitRelationshipIndex(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
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
//ORIGINAL LINE: private void assertIndexOnline(org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.schema.IndexBrokenKernelException
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
//ORIGINAL LINE: private static void assertPredicatesMatchSchema(org.Neo4Net.internal.kernel.api.IndexReference index, org.Neo4Net.internal.kernel.api.IndexQuery.ExactPredicate[] predicates) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
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
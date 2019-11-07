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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using IntIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntIntHashMap;


	using LabelSet = Neo4Net.Kernel.Api.Internal.LabelSet;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using AllStoreHolder = Neo4Net.Kernel.Impl.Newapi.AllStoreHolder;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using StorageProperty = Neo4Net.Kernel.Api.StorageEngine.StorageProperty;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.LuceneFulltextDocumentStructure.documentRepresentingProperties;

	/// <summary>
	/// A <seealso cref="TxStateVisitor"/> that adds all entities to a <seealso cref="TransactionStateLuceneIndexWriter"/>, that matches the index according to the
	/// <seealso cref="FulltextIndexDescriptor"/>.
	/// </summary>
	internal class FulltextIndexTransactionStateVisitor : Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Adapter
	{
		 private readonly FulltextIndexDescriptor _descriptor;
		 private readonly SchemaDescriptor _schema;
		 private readonly bool _visitingNodes;
		 private readonly int[] _entityTokenIds;
		 private readonly Value[] _propertyValues;
		 private readonly IntIntHashMap _propKeyToIndex;
		 private readonly MutableLongSet _modifiedEntityIdsInThisTransaction;
		 private readonly TransactionStateLuceneIndexWriter _writer;
		 private AllStoreHolder _read;
		 private NodeCursor _nodeCursor;
		 private PropertyCursor _propertyCursor;
		 private RelationshipScanCursor _relationshipCursor;

		 internal FulltextIndexTransactionStateVisitor( FulltextIndexDescriptor descriptor, MutableLongSet modifiedEntityIdsInThisTransaction, TransactionStateLuceneIndexWriter writer )
		 {
			  this._descriptor = descriptor;
			  this._schema = descriptor.Schema();
			  this._modifiedEntityIdsInThisTransaction = modifiedEntityIdsInThisTransaction;
			  this._writer = writer;
			  this._visitingNodes = _schema.entityType() == EntityType.NODE;
			  _entityTokenIds = _schema.EntityTokenIds;
			  int[] propertyIds = _schema.PropertyIds;
			  _propertyValues = new Value[propertyIds.Length];
			  _propKeyToIndex = new IntIntHashMap();
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					_propKeyToIndex.put( propertyIds[i], i );
			  }
		 }

		 internal virtual FulltextIndexTransactionStateVisitor Init( AllStoreHolder read, NodeCursor nodeCursor, RelationshipScanCursor relationshipCursor, PropertyCursor propertyCursor )
		 {
			  this._read = read;
			  this._nodeCursor = nodeCursor;
			  this._relationshipCursor = relationshipCursor;
			  this._propertyCursor = propertyCursor;
			  return this;
		 }

		 public override void VisitCreatedNode( long id )
		 {
			  IndexNode( id );
		 }

		 public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
		 {
			  IndexRelationship( id );
		 }

		 public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		 {
			  IndexNode( id );
		 }

		 public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		 {
			  IndexRelationship( id );
		 }

		 public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
		 {
			  IndexNode( id );
			  if ( _visitingNodes )
			  {
					// Nodes that have had their indexed labels removed will not have their properties indexed, so 'indexNode' would skip them.
					// However, we still need to make sure that they are not included in the result from the base index reader.
					foreach ( int IEntityTokenId in _entityTokenIds )
					{
						 if ( removed.contains( IEntityTokenId ) )
						 {
							  _modifiedEntityIdsInThisTransaction.add( id );
							  break;
						 }
					}
			  }
		 }

		 private void IndexNode( long id )
		 {
			  if ( _visitingNodes )
			  {
					_read.singleNode( id, _nodeCursor );
					if ( _nodeCursor.next() )
					{
						 LabelSet labels = _nodeCursor.labels();
						 if ( _schema.isAffected( labels.All() ) )
						 {
							  _nodeCursor.properties( _propertyCursor );
							  IndexProperties( id );
						 }
					}
			  }
		 }

		 private void IndexRelationship( long id )
		 {
			  if ( !_visitingNodes )
			  {
					_read.singleRelationship( id, _relationshipCursor );
					if ( _relationshipCursor.next() && _schema.isAffected(new long[]{ _relationshipCursor.type() }) )
					{
						 _relationshipCursor.properties( _propertyCursor );
						 IndexProperties( id );
					}
			  }
		 }

		 private void IndexProperties( long id )
		 {
			  while ( _propertyCursor.next() )
			  {
					int propertyKey = _propertyCursor.propertyKey();
					int index = _propKeyToIndex.getIfAbsent( propertyKey, -1 );
					if ( index != -1 )
					{
						 _propertyValues[index] = _propertyCursor.propertyValue();
					}
			  }
			  if ( _modifiedEntityIdsInThisTransaction.add( id ) )
			  {
					try
					{
						 _writer.addDocument( documentRepresentingProperties( id, _descriptor.propertyNames(), _propertyValues ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
			  Arrays.fill( _propertyValues, null );
		 }
	}

}
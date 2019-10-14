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
namespace Neo4Net.Kernel.impl.core
{

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using LabelSet = Neo4Net.Internal.Kernel.Api.LabelSet;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using RelationshipGroupCursor = Neo4Net.Internal.Kernel.Api.RelationshipGroupCursor;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using LabelNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.LabelNotFoundKernelException;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using AutoIndexingKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using IllegalTokenNameException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Neo4Net.Internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using Nodes = Neo4Net.Internal.Kernel.Api.helpers.Nodes;
	using Neo4Net.Internal.Kernel.Api.helpers;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.TokenRead_Fields.NO_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.RelationshipSelections.allIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.RelationshipSelections.incomingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.RelationshipSelections.outgoingIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_RELATIONSHIP_TYPE;

	public class NodeProxy : Node, RelationshipFactory<Relationship>
	{
		 private readonly EmbeddedProxySPI _spi;
		 private readonly long _nodeId;

		 public NodeProxy( EmbeddedProxySPI spi, long nodeId )
		 {
			  this._nodeId = nodeId;
			  this._spi = spi;
		 }

		 public static bool IsDeletedInCurrentTransaction( Node node )
		 {
			  if ( node is NodeProxy )
			  {
					NodeProxy proxy = ( NodeProxy ) node;
					KernelTransaction ktx = proxy._spi.kernelTransaction();
					using ( Statement ignore = ktx.AcquireStatement() )
					{
						 return ktx.DataRead().nodeDeletedInTransaction(proxy._nodeId);
					}
			  }
			  return false;
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return _nodeId;
			 }
		 }

		 public virtual GraphDatabaseService GraphDatabase
		 {
			 get
			 {
				  return _spi.GraphDatabase;
			 }
		 }

		 public override void Delete()
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  try
			  {
					bool deleted = transaction.DataWrite().nodeDelete(Id);
					if ( !deleted )
					{
						 throw new NotFoundException( "Unable to delete Node[" + _nodeId + "] since it has already been deleted." );
					}
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while deleting the node: " + e.Message, e );
			  }
		 }

		 public virtual ResourceIterable<Relationship> Relationships
		 {
			 get
			 {
				  return GetRelationships( Direction.BOTH );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterable<org.neo4j.graphdb.Relationship> getRelationships(final org.neo4j.graphdb.Direction direction)
		 public virtual ResourceIterable<Relationship> getRelationships( Direction direction )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  return InnerGetRelationships( transaction, direction, null );
		 }

		 public virtual ResourceIterable<Relationship> getRelationships( params RelationshipType[] types )
		 {
			  return GetRelationships( Direction.BOTH, types );
		 }

		 public virtual ResourceIterable<Relationship> getRelationships( RelationshipType type, Direction dir )
		 {
			  return GetRelationships( dir, type );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterable<org.neo4j.graphdb.Relationship> getRelationships(final org.neo4j.graphdb.Direction direction, org.neo4j.graphdb.RelationshipType... types)
		 public virtual ResourceIterable<Relationship> getRelationships( Direction direction, params RelationshipType[] types )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int[] typeIds = RelTypeIds( types, transaction.TokenRead() );
			  return InnerGetRelationships( transaction, direction, typeIds );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.ResourceIterable<org.neo4j.graphdb.Relationship> innerGetRelationships(org.neo4j.kernel.api.KernelTransaction transaction, final org.neo4j.graphdb.Direction direction, int[] typeIds)
		 private ResourceIterable<Relationship> InnerGetRelationships( KernelTransaction transaction, Direction direction, int[] typeIds )
		 {
			  return () => GetRelationshipSelectionIterator(transaction, direction, typeIds);
		 }

		 public override bool HasRelationship()
		 {
			  return HasRelationship( Direction.BOTH );
		 }

		 public override bool HasRelationship( Direction direction )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  return InnerHasRelationships( transaction, direction, null );
		 }

		 public override bool HasRelationship( params RelationshipType[] types )
		 {
			  return HasRelationship( Direction.BOTH, types );
		 }

		 public override bool HasRelationship( Direction direction, params RelationshipType[] types )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int[] typeIds = RelTypeIds( types, transaction.TokenRead() );
			  return InnerHasRelationships( transaction, direction, typeIds );
		 }

		 public override bool HasRelationship( RelationshipType type, Direction dir )
		 {
			  return HasRelationship( dir, type );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private boolean innerHasRelationships(final org.neo4j.kernel.api.KernelTransaction transaction, final org.neo4j.graphdb.Direction direction, int[] typeIds)
		 private bool InnerHasRelationships( KernelTransaction transaction, Direction direction, int[] typeIds )
		 {
			  using ( ResourceIterator<Relationship> iterator = GetRelationshipSelectionIterator( transaction, direction, typeIds ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return iterator.hasNext();
			  }
		 }

		 public override Relationship GetSingleRelationship( RelationshipType type, Direction dir )
		 {
			  using ( ResourceIterator<Relationship> rels = GetRelationships( dir, type ).GetEnumerator() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !rels.hasNext() )
					{
						 return null;
					}

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Relationship rel = rels.next();
					while ( rels.MoveNext() )
					{
						 Relationship other = rels.Current;
						 if ( !other.Equals( rel ) )
						 {
							  throw new NotFoundException( "More than one relationship[" + type + ", " + dir + "] found for " + this );
						 }
					}
					return rel;
			  }
		 }

		 public override void SetProperty( string key, object value )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  int propertyKeyId;
			  try
			  {
					propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new System.ArgumentException( format( "Invalid property key '%s'.", key ), e );
			  }

			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						transaction.DataWrite().nodeSetProperty(_nodeId, propertyKeyId, Values.of(value, false));
					  }
			  }
			  catch ( ConstraintValidationException e )
			  {
					throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
			  }
			  catch ( System.ArgumentException e )
			  {
					// Trying to set an illegal value is a critical error - fail this transaction
					_spi.failTransaction();
					throw e;
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while setting property: " + e.Message, e );
			  }
			  catch ( KernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object removeProperty(String key) throws org.neo4j.graphdb.NotFoundException
		 public override object RemoveProperty( string key )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
						return transaction.DataWrite().nodeRemoveProperty(_nodeId, propertyKeyId).asObjectCopy();
					  }
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( e );
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new System.ArgumentException( format( "Invalid property key '%s'.", key ), e );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
			  catch ( AutoIndexingKernelException e )
			  {
					throw new System.InvalidOperationException( "Auto indexing encountered a failure while removing property: " + e.Message, e );
			  }
		 }

		 public override object GetProperty( string key, object defaultValue )
		 {
			  if ( null == key )
			  {
					throw new System.ArgumentException( "(null) property key is not allowed" );
			  }
			  KernelTransaction transaction = SafeAcquireTransaction();
			  NodeCursor nodes = transaction.AmbientNodeCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					return defaultValue;
			  }
			  SingleNode( transaction, nodes );
			  nodes.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 Value value = properties.PropertyValue();
						 return value == Values.NO_VALUE ? defaultValue : value.AsObjectCopy();
					}
			  }
			  return defaultValue;
		 }

		 public virtual IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
				  IList<string> keys = new List<string>();
				  try
				  {
						NodeCursor nodes = transaction.AmbientNodeCursor();
						PropertyCursor properties = transaction.AmbientPropertyCursor();
						SingleNode( transaction, nodes );
						TokenRead token = transaction.TokenRead();
						nodes.Properties( properties );
						while ( properties.Next() )
						{
							 keys.Add( token.PropertyKeyName( properties.PropertyKey() ) );
						}
				  }
				  catch ( PropertyKeyIdNotFoundKernelException e )
				  {
						throw new System.InvalidOperationException( "Property key retrieved through kernel API should exist.", e );
				  }
				  return keys;
			 }
		 }

		 public override IDictionary<string, object> GetProperties( params string[] keys )
		 {
			  Objects.requireNonNull( keys, "Properties keys should be not null array." );

			  if ( keys.Length == 0 )
			  {
					return Collections.emptyMap();
			  }

			  KernelTransaction transaction = SafeAcquireTransaction();

			  int itemsToReturn = keys.Length;
			  IDictionary<string, object> properties = new Dictionary<string, object>( itemsToReturn );
			  TokenRead token = transaction.TokenRead();

			  //Find ids, note we are betting on that the number of keys
			  //is small enough not to use a set here.
			  int[] propertyIds = new int[itemsToReturn];
			  for ( int i = 0; i < itemsToReturn; i++ )
			  {
					string key = keys[i];
					if ( string.ReferenceEquals( key, null ) )
					{
						 throw new System.NullReferenceException( string.Format( "Key {0:D} was null", i ) );
					}
					propertyIds[i] = token.PropertyKey( key );
			  }

			  NodeCursor nodes = transaction.AmbientNodeCursor();
			  PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
			  SingleNode( transaction, nodes );
			  nodes.Properties( propertyCursor );
			  int propertiesToFind = itemsToReturn;
			  while ( propertiesToFind > 0 && propertyCursor.Next() )
			  {
					//Do a linear check if this is a property we are interested in.
					int currentKey = propertyCursor.PropertyKey();
					for ( int i = 0; i < itemsToReturn; i++ )
					{
						 if ( propertyIds[i] == currentKey )
						 {
							  properties[keys[i]] = propertyCursor.PropertyValue().asObjectCopy();
							  propertiesToFind--;
							  break;
						 }
					}
			  }
			  return properties;
		 }

		 public virtual IDictionary<string, object> AllProperties
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
				  IDictionary<string, object> properties = new Dictionary<string, object>();
   
				  try
				  {
						NodeCursor nodes = transaction.AmbientNodeCursor();
						PropertyCursor propertyCursor = transaction.AmbientPropertyCursor();
						TokenRead token = transaction.TokenRead();
						SingleNode( transaction, nodes );
						nodes.Properties( propertyCursor );
						while ( propertyCursor.Next() )
						{
							 properties[token.PropertyKeyName( propertyCursor.PropertyKey() )] = propertyCursor.PropertyValue().asObjectCopy();
						}
				  }
				  catch ( PropertyKeyIdNotFoundKernelException e )
				  {
						throw new System.InvalidOperationException( "Property key retrieved through kernel API should exist.", e );
				  }
				  return properties;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object getProperty(String key) throws org.neo4j.graphdb.NotFoundException
		 public override object GetProperty( string key )
		 {
			  if ( null == key )
			  {
					throw new System.ArgumentException( "(null) property key is not allowed" );
			  }
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					throw new NotFoundException( format( "No such property, '%s'.", key ) );
			  }

			  NodeCursor nodes = transaction.AmbientNodeCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  SingleNode( transaction, nodes );
			  nodes.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 Value value = properties.PropertyValue();
						 if ( value == Values.NO_VALUE )
						 {
							  throw new NotFoundException( format( "No such property, '%s'.", key ) );
						 }
						 return value.AsObjectCopy();
					}
			  }
			  throw new NotFoundException( format( "No such property, '%s'.", key ) );
		 }

		 public override bool HasProperty( string key )
		 {
			  if ( null == key )
			  {
					return false;
			  }

			  KernelTransaction transaction = SafeAcquireTransaction();
			  int propertyKey = transaction.TokenRead().propertyKey(key);
			  if ( propertyKey == Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					return false;
			  }

			  NodeCursor nodes = transaction.AmbientNodeCursor();
			  PropertyCursor properties = transaction.AmbientPropertyCursor();
			  SingleNode( transaction, nodes );
			  nodes.Properties( properties );
			  while ( properties.Next() )
			  {
					if ( propertyKey == properties.PropertyKey() )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private KernelTransaction SafeAcquireTransaction()
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  if ( transaction.Terminated )
			  {
					Status terminationReason = transaction.ReasonIfTerminated.orElse( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
					throw new TransactionTerminatedException( terminationReason );
			  }
			  return transaction;
		 }

		 public virtual int CompareTo( object node )
		 {
			  Node n = ( Node ) node;
			  return Long.compare( this.Id, n.Id );
		 }

		 public override bool Equals( object o )
		 {
			  return o is Node && this.Id == ( ( Node ) o ).Id;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( ( ( long )( ( ulong )_nodeId >> 32 ) ) ^ _nodeId );
		 }

		 public override string ToString()
		 {
			  return "Node[" + this.Id + "]";
		 }

		 public override Relationship CreateRelationshipTo( Node otherNode, RelationshipType type )
		 {
			  if ( otherNode == null )
			  {
					throw new System.ArgumentException( "Other node is null." );
			  }
			  // TODO: This is the checks we would like to do, but we have tests that expect to mix nodes...
			  //if ( !(otherNode instanceof NodeProxy) || (((NodeProxy) otherNode).actions != actions) )
			  //{
			  //    throw new IllegalArgumentException( "Nodes do not belong to same graph database." );
			  //}

			  KernelTransaction transaction = SafeAcquireTransaction();
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						int relationshipTypeId = transaction.TokenWrite().relationshipTypeGetOrCreateForName(type.Name());
						long relationshipId = transaction.DataWrite().relationshipCreate(_nodeId, relationshipTypeId, otherNode.Id);
						return _spi.newRelationshipProxy( relationshipId, _nodeId, relationshipTypeId, otherNode.Id );
					  }
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new System.ArgumentException( e );
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( "Node[" + e.entityId() + "] is deleted and cannot be used to create a relationship" );
			  }
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override void AddLabel( Label label )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						transaction.DataWrite().nodeAddLabel(Id, transaction.TokenWrite().labelGetOrCreateForName(label.Name()));
					  }
			  }
			  catch ( ConstraintValidationException e )
			  {
					throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
			  }
			  catch ( IllegalTokenNameException e )
			  {
					throw new ConstraintViolationException( format( "Invalid label name '%s'.", label.Name() ), e );
			  }
			  catch ( TooManyLabelsException e )
			  {
					throw new ConstraintViolationException( "Unable to add label.", e );
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( "No node with id " + Id + " found.", e );
			  }
			  catch ( KernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override void RemoveLabel( Label label )
		 {
			  KernelTransaction transaction = _spi.kernelTransaction();
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						int labelId = transaction.TokenRead().nodeLabel(label.Name());
						if ( labelId != Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
						{
							 transaction.DataWrite().nodeRemoveLabel(Id, labelId);
						}
					  }
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new NotFoundException( "No node with id " + Id + " found.", e );
			  }
			  catch ( KernelException e )
			  {
					throw new ConstraintViolationException( e.Message, e );
			  }
		 }

		 public override bool HasLabel( Label label )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  NodeCursor nodes = transaction.AmbientNodeCursor();
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					int labelId = transaction.TokenRead().nodeLabel(label.Name());
					if ( labelId == NO_SUCH_LABEL )
					{
						 return false;
					}
					transaction.DataRead().singleNode(_nodeId, nodes);
					return nodes.Next() && nodes.HasLabel(labelId);
			  }
		 }

		 public virtual IEnumerable<Label> Labels
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
				  NodeCursor nodes = transaction.AmbientNodeCursor();
				  try
				  {
						  using ( Statement ignore = _spi.statement() )
						  {
							SingleNode( transaction, nodes );
							LabelSet labelSet = nodes.Labels();
							TokenRead tokenRead = transaction.TokenRead();
							List<Label> list = new List<Label>( labelSet.NumberOfLabels() );
							for ( int i = 0; i < labelSet.NumberOfLabels(); i++ )
							{
								 list.Add( label( tokenRead.NodeLabelName( labelSet.Label( i ) ) ) );
							}
							return list;
						  }
				  }
				  catch ( LabelNotFoundKernelException e )
				  {
						throw new System.InvalidOperationException( "Label retrieved through kernel API should exist.", e );
				  }
			 }
		 }

		 public virtual int Degree
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
   
				  using ( Statement ignore = transaction.AcquireStatement() )
				  {
						NodeCursor nodes = transaction.AmbientNodeCursor();
						SingleNode( transaction, nodes );
   
						return Nodes.countAll( nodes, transaction.Cursors() );
				  }
			 }
		 }

		 public virtual int getDegree( RelationshipType type )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int typeId = transaction.TokenRead().relationshipType(type.Name());
			  if ( typeId == NO_TOKEN )
			  { // This type doesn't even exist. Return 0
					return 0;
			  }

			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					NodeCursor nodes = transaction.AmbientNodeCursor();
					SingleNode( transaction, nodes );

					return Nodes.countAll( nodes, transaction.Cursors(), typeId );
			  }
		 }

		 public virtual int getDegree( Direction direction )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					NodeCursor nodes = transaction.AmbientNodeCursor();
					SingleNode( transaction, nodes );

					switch ( direction.innerEnumValue )
					{
					case Direction.InnerEnum.OUTGOING:
						 return Nodes.countOutgoing( nodes, transaction.Cursors() );
					case Direction.InnerEnum.INCOMING:
						 return Nodes.countIncoming( nodes, transaction.Cursors() );
					case Direction.InnerEnum.BOTH:
						 return Nodes.countAll( nodes, transaction.Cursors() );
					default:
						 throw new System.InvalidOperationException( "Unknown direction " + direction );
					}
			  }
		 }

		 public virtual int getDegree( RelationshipType type, Direction direction )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction();
			  int typeId = transaction.TokenRead().relationshipType(type.Name());
			  if ( typeId == NO_TOKEN )
			  { // This type doesn't even exist. Return 0
					return 0;
			  }

			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					NodeCursor nodes = transaction.AmbientNodeCursor();
					SingleNode( transaction, nodes );
					switch ( direction.innerEnumValue )
					{
					case Direction.InnerEnum.OUTGOING:
						 return Nodes.countOutgoing( nodes, transaction.Cursors(), typeId );
					case Direction.InnerEnum.INCOMING:
						 return Nodes.countIncoming( nodes, transaction.Cursors(), typeId );
					case Direction.InnerEnum.BOTH:
						 return Nodes.countAll( nodes, transaction.Cursors(), typeId );
					default:
						 throw new System.InvalidOperationException( "Unknown direction " + direction );
					}
			  }
		 }

		 public virtual IEnumerable<RelationshipType> RelationshipTypes
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction();
				  try
				  {
						  using ( RelationshipGroupCursor relationships = transaction.Cursors().allocateRelationshipGroupCursor(), Statement ignore = transaction.AcquireStatement() )
						  {
							NodeCursor nodes = transaction.AmbientNodeCursor();
							TokenRead tokenRead = transaction.TokenRead();
							SingleNode( transaction, nodes );
							nodes.Relationships( relationships );
							IList<RelationshipType> types = new List<RelationshipType>();
							while ( relationships.next() )
							{
								 // only include this type if there are any relationships with this type
								 int type = relationships.Type();
								 if ( relationships.TotalCount() > 0 )
								 {
									  types.Add( RelationshipType.withName( tokenRead.RelationshipTypeName( relationships.Type() ) ) );
								 }
							}
         
							return types;
						  }
				  }
				  catch ( KernelException e )
				  {
						throw new NotFoundException( "Relationship name not found.", e );
				  }
			 }
		 }

		 private ResourceIterator<Relationship> GetRelationshipSelectionIterator( KernelTransaction transaction, Direction direction, int[] typeIds )
		 {
			  NodeCursor node = transaction.AmbientNodeCursor();
			  transaction.DataRead().singleNode(Id, node);
			  if ( !node.Next() )
			  {
					throw new NotFoundException( format( "Node %d not found", _nodeId ) );
			  }

			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					return outgoingIterator( transaction.Cursors(), node, typeIds, this );
			  case Direction.InnerEnum.INCOMING:
					return incomingIterator( transaction.Cursors(), node, typeIds, this );
			  case Direction.InnerEnum.BOTH:
					return allIterator( transaction.Cursors(), node, typeIds, this );
			  default:
					throw new System.InvalidOperationException( "Unknown direction " + direction );
			  }
		 }

		 private int[] RelTypeIds( RelationshipType[] types, TokenRead token )
		 {
			  int[] ids = new int[types.Length];
			  int outIndex = 0;
			  foreach ( RelationshipType type in types )
			  {
					int id = token.RelationshipType( type.Name() );
					if ( id != NO_SUCH_RELATIONSHIP_TYPE )
					{
						 ids[outIndex++] = id;
					}
			  }

			  if ( outIndex != ids.Length )
			  {
					// One or more relationship types do not exist, so we can exclude them right away.
					ids = Arrays.copyOf( ids, outIndex );
			  }
			  return ids;
		 }

		 private void SingleNode( KernelTransaction transaction, NodeCursor nodes )
		 {
			  transaction.DataRead().singleNode(_nodeId, nodes);
			  if ( !nodes.Next() )
			  {
					throw new NotFoundException( new EntityNotFoundException( EntityType.NODE, _nodeId ) );
			  }
		 }

		 public override Relationship Relationship( long id, long startNodeId, int typeId, long endNodeId )
		 {
			  return _spi.newRelationshipProxy( id, startNodeId, typeId, endNodeId );
		 }
	}

}
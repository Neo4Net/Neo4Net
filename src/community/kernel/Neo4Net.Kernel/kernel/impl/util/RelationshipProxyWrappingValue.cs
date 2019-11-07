using System;

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
namespace Neo4Net.Kernel.impl.util
{
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;
	using Neo4Net.Values;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public class RelationshipProxyWrappingValue : RelationshipValue
	{
		 private readonly Relationship _relationship;
		 private volatile TextValue _type;
		 private volatile MapValue _properties;
		 private volatile NodeValue _startNode;
		 private volatile NodeValue _endNode;

		 internal RelationshipProxyWrappingValue( Relationship relationship ) : base( relationship.Id )
		 {
			  this._relationship = relationship;
		 }

		 public virtual Relationship RelationshipProxy()
		 {
			  return _relationship;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void WriteTo(Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  if ( _relationship is RelationshipProxy )
			  {
					RelationshipProxy proxy = ( RelationshipProxy ) _relationship;
					if ( !proxy.InitializeData() )
					{
						 // If the relationship has been deleted since it was found by the query, then we'll have to tell the client that their transaction conflicted,
						 // and that they need to retry it.
						 throw new ReadAndDeleteTransactionConflictException( RelationshipProxy.isDeletedInCurrentTransaction( _relationship ) );
					}
			  }

			  MapValue p;
			  try
			  {
					p = Properties();
			  }
			  catch ( NotFoundException )
			  {
					p = VirtualValues.EMPTY_MAP;
			  }
			  catch ( System.InvalidOperationException e )
			  {
					throw new ReadAndDeleteTransactionConflictException( RelationshipProxy.isDeletedInCurrentTransaction( _relationship ), e );
			  }

			  if ( Id() < 0 )
			  {
					writer.WriteVirtualRelationshipHack( _relationship );
			  }

			  writer.WriteRelationship( Id(), StartNode().id(), EndNode().id(), Type(), p );
		 }

		 public override NodeValue StartNode()
		 {
			  NodeValue start = _startNode;
			  if ( start == null )
			  {
					lock ( this )
					{
						 start = _startNode;
						 if ( start == null )
						 {
							  start = _startNode = ValueUtils.FromNodeProxy( _relationship.StartNode );
						 }
					}
			  }
			  return start;
		 }

		 public override NodeValue EndNode()
		 {
			  NodeValue end = _endNode;
			  if ( end == null )
			  {
					lock ( this )
					{
						 end = _endNode;
						 if ( end == null )
						 {
							  end = _endNode = ValueUtils.FromNodeProxy( _relationship.EndNode );
						 }
					}
			  }
			  return end;
		 }

		 public override NodeValue OtherNode( VirtualNodeValue node )
		 {
			  if ( node is NodeProxyWrappingNodeValue )
			  {
					Node proxy = ( ( NodeProxyWrappingNodeValue ) node ).NodeProxy();
					return ValueUtils.FromNodeProxy( _relationship.getOtherNode( proxy ) );
			  }
			  else
			  {
				  return base.OtherNode( node );
			  }
		 }

		 public override long OtherNodeId( long node )
		 {
			  return _relationship.getOtherNodeId( node );
		 }

		 public override TextValue Type()
		 {
			  TextValue t = _type;
			  if ( t == null )
			  {
					lock ( this )
					{
						 t = _type;
						 if ( t == null )
						 {
							  t = _type = Values.stringValue( _relationship.Type.name() );
						 }
					}
			  }
			  return t;
		 }

		 public override MapValue Properties()
		 {
			  MapValue m = _properties;
			  if ( m == null )
			  {
					lock ( this )
					{
						 m = _properties;
						 if ( m == null )
						 {
							  m = _properties = ValueUtils.AsMapValue( _relationship.AllProperties );
						 }
					}
			  }
			  return m;
		 }
	}


}
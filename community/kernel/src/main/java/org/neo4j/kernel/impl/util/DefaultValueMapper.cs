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
namespace Org.Neo4j.Kernel.impl.util
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Paths = Org.Neo4j.Graphdb.traversal.Paths;
	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxy = Org.Neo4j.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Org.Neo4j.Kernel.impl.core.RelationshipProxy;
	using Org.Neo4j.Values;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iteratorsEqual;

	public class DefaultValueMapper : Org.Neo4j.Values.ValueMapper_JavaMapper
	{
		 private readonly EmbeddedProxySPI _proxySPI;

		 public DefaultValueMapper( EmbeddedProxySPI proxySPI )
		 {
			  this._proxySPI = proxySPI;
		 }

		 public override Node MapNode( VirtualNodeValue value )
		 {
			  if ( value is NodeProxyWrappingNodeValue )
			  { // this is the back door through which "virtual nodes" slip
					return ( ( NodeProxyWrappingNodeValue ) value ).NodeProxy();
			  }
			  return new NodeProxy( _proxySPI, value.Id() );
		 }

		 public override Relationship MapRelationship( VirtualRelationshipValue value )
		 {
			  if ( value is RelationshipProxyWrappingValue )
			  { // this is the back door through which "virtual relationships" slip
					return ( ( RelationshipProxyWrappingValue ) value ).RelationshipProxy();
			  }
			  return new RelationshipProxy( _proxySPI, value.Id() );
		 }

		 public override Path MapPath( PathValue value )
		 {
			  if ( value is PathWrappingPathValue )
			  {
					return ( ( PathWrappingPathValue ) value ).Path();
			  }
			  return new CoreAPIPath( this, value );
		 }

		 private IEnumerable<V> AsList<U, V>( U[] values, System.Func<U, V> mapper )
		 {
			  return () => new IteratorAnonymousInnerClass(this, values, mapper);
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<V>
		 {
			 private readonly DefaultValueMapper _outerInstance;

			 private U[] _values;
			 private System.Func<U, V> _mapper;

			 public IteratorAnonymousInnerClass( DefaultValueMapper outerInstance, U[] values, System.Func<U, V> mapper )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
				 this._mapper = mapper;
			 }

			 private int index;

			 public bool hasNext()
			 {
				  return index < _values.Length;
			 }

			 public V next()
			 {
				  return _mapper( _values[index++] );
			 }
		 }

		 private IEnumerable<V> AsReverseList<U, V>( U[] values, System.Func<U, V> mapper )
		 {
			  return () => new IteratorAnonymousInnerClass2(this, values, mapper);
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<V>
		 {
			 private readonly DefaultValueMapper _outerInstance;

			 private U[] _values;
			 private System.Func<U, V> _mapper;

			 public IteratorAnonymousInnerClass2( DefaultValueMapper outerInstance, U[] values, System.Func<U, V> mapper )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
				 this._mapper = mapper;
				 index = values.Length - 1;
			 }

			 private int index;

			 public bool hasNext()
			 {
				  return index >= 0;
			 }

			 public V next()
			 {
				  return _mapper( _values[index--] );
			 }
		 }

		 private class CoreAPIPath : Path
		 {
			 private readonly DefaultValueMapper _outerInstance;

			  internal readonly PathValue Value;

			  internal CoreAPIPath( DefaultValueMapper outerInstance, PathValue value )
			  {
				  this._outerInstance = outerInstance;
					this.Value = value;
			  }

			  public override string ToString()
			  {
					return Paths.defaultPathToStringWithNotInTransactionFallback( this );
			  }

			  public override int GetHashCode()
			  {
					return Value.GetHashCode();
			  }

			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj is CoreAPIPath )
					{
						 return Value.Equals( ( ( CoreAPIPath ) obj ).Value );
					}
					else if ( obj is Path )
					{
						 Path other = ( Path ) obj;
						 if ( Value.nodes()[0].id() != other.StartNode().Id )
						 {
							  return false;
						 }
						 return iteratorsEqual( this.Relationships().GetEnumerator(), other.Relationships().GetEnumerator() );
					}
					else
					{
						 return false;
					}
			  }

			  public override Node StartNode()
			  {
					return outerInstance.MapNode( Value.startNode() );
			  }

			  public override Node EndNode()
			  {
					return outerInstance.MapNode( Value.endNode() );
			  }

			  public override Relationship LastRelationship()
			  {
					if ( Value.size() == 0 )
					{
						 return null;
					}
					else
					{
						 return outerInstance.MapRelationship( Value.lastRelationship() );
					}
			  }

			  public override IEnumerable<Relationship> Relationships()
			  {
					return _outerInstance.asList( Value.relationships(), _outerInstance.mapRelationship );
			  }

			  public override IEnumerable<Relationship> ReverseRelationships()
			  {
					return _outerInstance.asReverseList( Value.relationships(), _outerInstance.mapRelationship );
			  }

			  public override IEnumerable<Node> Nodes()
			  {
					return _outerInstance.asList( Value.nodes(), _outerInstance.mapNode );
			  }

			  public override IEnumerable<Node> ReverseNodes()
			  {
					return _outerInstance.asReverseList( Value.nodes(), _outerInstance.mapNode );
			  }

			  public override int Length()
			  {
					return Value.size();
			  }

			  public override IEnumerator<PropertyContainer> Iterator()
			  {
					return new IteratorAnonymousInnerClass3( this );
			  }

			  private class IteratorAnonymousInnerClass3 : IEnumerator<PropertyContainer>
			  {
				  private readonly CoreAPIPath _outerInstance;

				  public IteratorAnonymousInnerClass3( CoreAPIPath outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  size = 2 * outerInstance.Value.size() + 1;
					  nodes = outerInstance.Value.nodes();
					  relationships = outerInstance.Value.relationships();
				  }

				  private readonly int size;
				  private int index;
				  private readonly NodeValue[] nodes;
				  private readonly RelationshipValue[] relationships;

				  public bool hasNext()
				  {
						return index < size;
				  }

				  public PropertyContainer next()
				  {
						PropertyContainer propertyContainer;
						if ( ( index & 1 ) == 0 )
						{
							 propertyContainer = outerInstance.outerInstance.MapNode( nodes[index >> 1] );
						}
						else
						{
							 propertyContainer = outerInstance.outerInstance.MapRelationship( relationships[index >> 1] );
						}
						index++;
						return propertyContainer;
				  }
			  }
		 }
	}


}
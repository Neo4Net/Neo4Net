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
namespace Neo4Net.Cypher.Internal.codegen
{

	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Helpers.Collections;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using Neo4Net.Values;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iteratorsEqual;

	/// <summary>
	/// Used for turning parameters into appropriate types in the compiled runtime
	/// </summary>
	internal class ParameterConverter : AnyValueWriter<Exception>
	{
		 private readonly Deque<Writer> _stack = new LinkedList<Writer>();
		 private readonly EmbeddedProxySPI _proxySpi;

		 internal ParameterConverter( EmbeddedProxySPI proxySpi )
		 {
			  this._proxySpi = proxySpi;
			  _stack.push( new ObjectWriter() );
		 }

		 public virtual object Value()
		 {
			  Debug.Assert( _stack.size() == 1 );
			  return _stack.Last.value();
		 }

		 private void WriteValue( object value )
		 {
			  Debug.Assert( !_stack.Empty );
			  Writer head = _stack.peek();
			  head.Write( value );
		 }

		 public override void WriteNodeReference( long nodeId )
		 {
			  WriteValue( VirtualValues.node( nodeId ) );
		 }

		 public override void WriteNode( long nodeId, TextArray ignore, MapValue properties )
		 {
			  WriteValue( VirtualValues.node( nodeId ) );
		 }

		 public override void WriteRelationshipReference( long relId )
		 {
			  WriteValue( VirtualValues.relationship( relId ) );
		 }

		 public override void WriteRelationship( long relId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
		 {
			  WriteValue( VirtualValues.relationship( relId ) );
		 }

		 public override void BeginMap( int size )
		 {
			  _stack.push( new MapWriter( size ) );
		 }

		 public override void EndMap()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

		 public override void BeginList( int size )
		 {
			  _stack.push( new ListWriter( size ) );
		 }

		 public override void EndList()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

		 public override void WritePath( NodeValue[] nodes, RelationshipValue[] relationships )
		 {
			  Debug.Assert( nodes != null );
			  Debug.Assert( nodes.Length > 0 );
			  Debug.Assert( relationships != null );
			  Debug.Assert( nodes.Length == relationships.Length + 1 );

			  Node[] nodeProxies = new Node[nodes.Length];
			  for ( int i = 0; i < nodes.Length; i++ )
			  {
					nodeProxies[i] = _proxySpi.newNodeProxy( nodes[i].Id() );
			  }
			  Relationship[] relationship = new Relationship[relationships.Length];
			  for ( int i = 0; i < relationships.Length; i++ )
			  {
					relationship[i] = _proxySpi.newRelationshipProxy( relationships[i].Id() );
			  }
			  WriteValue( new PathAnonymousInnerClass( this, nodeProxies, relationship ) );
		 }

		 private class PathAnonymousInnerClass : Path
		 {
			 private readonly ParameterConverter _outerInstance;

			 private Node[] _nodeProxies;
			 private Relationship[] _relationship;

			 public PathAnonymousInnerClass( ParameterConverter outerInstance, Node[] nodeProxies, Relationship[] relationship )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeProxies = nodeProxies;
				 this._relationship = relationship;
			 }

			 public Node startNode()
			 {
				  return _nodeProxies[0];
			 }

			 public Node endNode()
			 {
				  return _nodeProxies[_nodeProxies.Length - 1];
			 }

			 public Relationship lastRelationship()
			 {
				  return _relationship[_relationship.Length - 1];
			 }

			 public IEnumerable<Relationship> relationships()
			 {
				  return Arrays.asList( _relationship );
			 }

			 public IEnumerable<Relationship> reverseRelationships()
			 {
				  return () => new ReverseArrayIterator<Relationship>(_relationship);
			 }

			 public IEnumerable<Node> nodes()
			 {
				  return Arrays.asList( _nodeProxies );
			 }

			 public IEnumerable<Node> reverseNodes()
			 {
				  return () => new ReverseArrayIterator<Node>(_nodeProxies);
			 }

			 public int length()
			 {
				  return _relationship.Length;
			 }

			 public override int GetHashCode()
			 {
				  if ( _relationship.Length == 0 )
				  {
						return startNode().GetHashCode();
				  }
				  else
				  {
						return Arrays.GetHashCode( _relationship );
				  }
			 }

			 public override bool Equals( object obj )
			 {
				  if ( this == obj )
				  {
						return true;
				  }
				  else if ( obj is Path )
				  {
						Path other = ( Path ) obj;
						return startNode().Equals(other.StartNode()) && iteratorsEqual(this.relationships().GetEnumerator(), other.Relationships().GetEnumerator());

				  }
				  else
				  {
						return false;
				  }
			 }

			 public IEnumerator<PropertyContainer> iterator()
			 {
				  return new IteratorAnonymousInnerClass( this );
			 }

			 private class IteratorAnonymousInnerClass : IEnumerator<PropertyContainer>
			 {
				 private readonly PathAnonymousInnerClass _outerInstance;

				 public IteratorAnonymousInnerClass( PathAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
					 current = nodes().GetEnumerator();
					 next = relationships().GetEnumerator();
				 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.graphdb.PropertyContainer> current;
				 internal IEnumerator<PropertyContainer> current;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.graphdb.PropertyContainer> next;
				 internal IEnumerator<PropertyContainer> next;

				 public bool hasNext()
				 {
					  return current.hasNext();
				 }

				 public PropertyContainer next()
				 {
					  try
					  {
							return current.next();
					  }
					  finally
					  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.neo4j.graphdb.PropertyContainer> temp = current;
							IEnumerator<PropertyContainer> temp = current;
							current = next;
							next = temp;
					  }
				 }

				 public void remove()
				 {
					  next.remove();
				 }
			 }
		 }

		 public override void WriteNull()
		 {
			  WriteValue( null );
		 }

		 public override void WriteBoolean( bool value )
		 {
			  WriteValue( value );
		 }

		 public override void WriteInteger( sbyte value )
		 {
			  WriteValue( ( long ) value );
		 }

		 public override void WriteInteger( short value )
		 {
			  WriteValue( ( long ) value );
		 }

		 public override void WriteInteger( int value )
		 {
			  WriteValue( ( long ) value );
		 }

		 public override void WriteInteger( long value )
		 {
			  WriteValue( value );
		 }

		 public override void WriteFloatingPoint( float value )
		 {
			  WriteValue( ( double ) value );
		 }

		 public override void WriteFloatingPoint( double value )
		 {
			  WriteValue( value );
		 }

		 public override void WriteString( string value )
		 {
			  WriteValue( value );
		 }

		 public override void WriteString( char value )
		 {
			  WriteValue( value );
		 }

		 public override void BeginArray( int size, ArrayType arrayType )
		 {
			  _stack.push( new ArrayWriter( size, arrayType ) );
		 }

		 public override void EndArray()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

		 public override void WriteByteArray( sbyte[] value )
		 {
			  WriteValue( value );
		 }

		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  WriteValue( Values.pointValue( crs, coordinate ) );
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  WriteValue( DurationValue.duration( months, days, seconds, nanos ) );
		 }

		 public override void WriteDate( LocalDate localDate )
		 {
			  WriteValue( localDate );
		 }

		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  WriteValue( localTime );
		 }

		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  WriteValue( offsetTime );
		 }

		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  WriteValue( localDateTime );
		 }

		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  WriteValue( zonedDateTime );
		 }

		 private interface Writer
		 {
			  void Write( object value );

			  object Value();
		 }

		 private class ObjectWriter : Writer
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal object ValueConflict;

			  public override void Write( object value )
			  {
					this.ValueConflict = value;
			  }

			  public override object Value()
			  {
					return ValueConflict;
			  }
		 }

		 private class MapWriter : Writer
		 {
			  internal string Key;
			  internal bool IsKey = true;
			  internal readonly Dictionary<string, object> Map;

			  internal MapWriter( int size )
			  {
					this.Map = new Dictionary<string, object>( size );
			  }

			  public override void Write( object value )
			  {
					if ( IsKey )
					{
						 Key = ( string ) value;
						 IsKey = false;
					}
					else
					{
						 Map[Key] = value;
						 IsKey = true;
					}
			  }

			  public override object Value()
			  {
					return Map;
			  }
		 }

		 private class ArrayWriter : Writer
		 {
			  protected internal readonly object Array;
			  internal int Index;

			  internal ArrayWriter( int size, ArrayType arrayType )
			  {
					switch ( arrayType )
					{
					case SHORT:
						 this.Array = Array.CreateInstance( typeof( short ), size );
						 break;
					case INT:
						 this.Array = Array.CreateInstance( typeof( int ), size );
						 break;
					case BYTE:
						 this.Array = Array.CreateInstance( typeof( sbyte ), size );
						 break;
					case LONG:
						 this.Array = Array.CreateInstance( typeof( long ), size );
						 break;
					case FLOAT:
						 this.Array = Array.CreateInstance( typeof( float ), size );
						 break;
					case DOUBLE:
						 this.Array = Array.CreateInstance( typeof( double ), size );
						 break;
					case BOOLEAN:
						 this.Array = Array.CreateInstance( typeof( bool ), size );
						 break;
					case STRING:
						 this.Array = Array.CreateInstance( typeof( string ), size );
						 break;
					case CHAR:
						 this.Array = Array.CreateInstance( typeof( char ), size );
						 break;
					default:
						 this.Array = new object[size];
					 break;
					}
			  }

			  public override void Write( object value )
			  {
					( ( Array )Array ).SetValue( value, Index );
					Index++;
			  }

			  public override object Value()
			  {
					return Array;
			  }
		 }

		 private class ListWriter : Writer
		 {
			  internal readonly IList<object> List;

			  internal ListWriter( int size )
			  {
					this.List = new List<object>( size );
			  }

			  public override void Write( object value )
			  {
					List.Add( value );
			  }

			  public override object Value()
			  {
					return List;
			  }
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.util
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Paths = Neo4Net.GraphDb.Traversal.Paths;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Values;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iteratorsEqual;

	/// <summary>
	/// Base class for converting AnyValue to normal java objects.
	/// <para>
	/// This base class takes care of converting all "normal" java types such as
	/// number types, booleans, strings, arrays and lists. It leaves to the extending
	/// class to handle Neo4Net specific types such as nodes, edges and points.
	/// 
	/// </para>
	/// </summary>
	/// @param <E> the exception thrown on error. </param>
	public abstract class BaseToObjectValueWriter<E> : AnyValueWriter<E> where E : Exception
	{
		 private readonly Deque<Writer> _stack = new LinkedList<Writer>();

		 public BaseToObjectValueWriter()
		 {
			  _stack.push( new ObjectWriter() );
		 }

		 protected internal abstract Node NewNodeProxyById( long id );

		 protected internal abstract Relationship NewRelationshipProxyById( long id );

		 protected internal abstract Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate );

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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNodeReference(long nodeId) throws RuntimeException
		 public override void WriteNodeReference( long nodeId )
		 {
			  throw new System.NotSupportedException( "Cannot write a raw node reference" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNode(long nodeId, org.Neo4Net.values.storable.TextArray ignore, org.Neo4Net.values.virtual.MapValue properties) throws RuntimeException
		 public override void WriteNode( long nodeId, TextArray ignore, MapValue properties )
		 {
			  if ( nodeId >= 0 )
			  {
					WriteValue( NewNodeProxyById( nodeId ) );
			  }
		 }

		 public override void WriteVirtualNodeHack( object node )
		 {
			  WriteValue( node );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRelationshipReference(long relId) throws RuntimeException
		 public override void WriteRelationshipReference( long relId )
		 {
			  throw new System.NotSupportedException( "Cannot write a raw edge reference" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRelationship(long relId, long startNodeId, long endNodeId, org.Neo4Net.values.storable.TextValue type, org.Neo4Net.values.virtual.MapValue properties) throws RuntimeException
		 public override void WriteRelationship( long relId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
		 {
			  if ( relId >= 0 )
			  {
					WriteValue( NewRelationshipProxyById( relId ) );
			  }
		 }

		 public override void WriteVirtualRelationshipHack( object relationship )
		 {
			  WriteValue( relationship );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginMap(int size) throws RuntimeException
		 public override void BeginMap( int size )
		 {
			  _stack.push( new MapWriter( size ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endMap() throws RuntimeException
		 public override void EndMap()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginList(int size) throws RuntimeException
		 public override void BeginList( int size )
		 {
			  _stack.push( new ListWriter( size ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endList() throws RuntimeException
		 public override void EndList()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePath(org.Neo4Net.values.virtual.NodeValue[] nodes, org.Neo4Net.values.virtual.RelationshipValue[] relationships) throws RuntimeException
		 public override void WritePath( NodeValue[] nodes, RelationshipValue[] relationships )
		 {
			  Debug.Assert( nodes != null );
			  Debug.Assert( nodes.Length > 0 );
			  Debug.Assert( relationships != null );
			  Debug.Assert( nodes.Length == relationships.Length + 1 );

			  Node[] nodeProxies = new Node[nodes.Length];
			  for ( int i = 0; i < nodes.Length; i++ )
			  {
					nodeProxies[i] = NewNodeProxyById( nodes[i].Id() );
			  }
			  Relationship[] relationship = new Relationship[relationships.Length];
			  for ( int i = 0; i < relationships.Length; i++ )
			  {
					relationship[i] = NewRelationshipProxyById( relationships[i].Id() );
			  }
			  WriteValue( new PathAnonymousInnerClass( this, nodeProxies, relationship ) );
		 }

		 private class PathAnonymousInnerClass : Path
		 {
			 private readonly BaseToObjectValueWriter<E> _outerInstance;

			 private Node[] _nodeProxies;
			 private Relationship[] _relationship;

			 public PathAnonymousInnerClass( BaseToObjectValueWriter<E> outerInstance, Node[] nodeProxies, Relationship[] relationship )
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
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> current;
				 internal IEnumerator<PropertyContainer> current;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> next;
				 internal IEnumerator<PropertyContainer> next;

				 public bool hasNext()
				 {
					  return current.hasNext();
				 }

				 public IPropertyContainer next()
				 {
					  try
					  {
							return current.next();
					  }
					  finally
					  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> temp = current;
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

			 public override string ToString()
			 {
				  return Paths.defaultPathToStringWithNotInTransactionFallback( this );
			 }
		 }

		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  WriteValue( NewPoint( crs, coordinate ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNull() throws RuntimeException
		 public override void WriteNull()
		 {
			  WriteValue( null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolean(boolean value) throws RuntimeException
		 public override void WriteBoolean( bool value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(byte value) throws RuntimeException
		 public override void WriteInteger( sbyte value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(short value) throws RuntimeException
		 public override void WriteInteger( short value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(int value) throws RuntimeException
		 public override void WriteInteger( int value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(long value) throws RuntimeException
		 public override void WriteInteger( long value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(float value) throws RuntimeException
		 public override void WriteFloatingPoint( float value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(double value) throws RuntimeException
		 public override void WriteFloatingPoint( double value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(String value) throws RuntimeException
		 public override void WriteString( string value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(char value) throws RuntimeException
		 public override void WriteString( char value )
		 {
			  WriteValue( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginArray(int size, ArrayType arrayType) throws RuntimeException
		 public override void BeginArray( int size, ArrayType arrayType )
		 {
			  _stack.push( new ArrayWriter( size, arrayType ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endArray() throws RuntimeException
		 public override void EndArray()
		 {
			  Debug.Assert( !_stack.Empty );
			  WriteValue( _stack.pop().value() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws RuntimeException
		 public override void WriteByteArray( sbyte[] value )
		 {
			  WriteValue( value );
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  WriteValue( DurationValue.duration( months, days, seconds, nanos ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws RuntimeException
		 public override void WriteDate( LocalDate localDate )
		 {
			  WriteValue( localDate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws RuntimeException
		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  WriteValue( localTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws RuntimeException
		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  WriteValue( offsetTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws RuntimeException
		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  WriteValue( localDateTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws RuntimeException
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
					( ( Array )Array ).SetValue( value, Index++ );
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.utils
{

	using Neo4Net.Values;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

	/// <summary>
	/// Pretty printer for AnyValues.
	/// <para>
	/// Used to format AnyValue as a json-like string, as following:
	/// <ul>
	/// <li>nodes: <code>(id=42 :LABEL {prop1: ["a", 13]})</code></li>
	/// <li>edges: <code>-[id=42 :TYPE {prop1: ["a", 13]}]-</code></li>
	/// <li>paths: <code>(id=1 :L)-[id=42 :T {k: "v"}]->(id=2)-...</code></li>
	/// <li>points are serialized to geojson</li>
	/// <li>maps: <code>{foo: 42, bar: "baz"}</code></li>
	/// <li>lists and arrays: <code>["aa", "bb", "cc"]</code></li>
	/// <li>Numbers: <code>2.7182818285</code></li>
	/// <li>Strings: <code>"this is a string"</code></li>
	/// </ul>
	/// </para>
	/// </summary>
	public class PrettyPrinter : AnyValueWriter<Exception>
	{
		 private readonly Deque<Writer> _stack = new LinkedList<Writer>();
		 private readonly string _quoteMark;

		 public PrettyPrinter() : this("\"")
		 {
		 }

		 public PrettyPrinter( string quoteMark )
		 {
			  this._quoteMark = quoteMark;
			  _stack.push( new ValueWriter( this ) );
		 }

		 public override void WriteNodeReference( long nodeId )
		 {
			  Append( format( "(id=%d)", nodeId ) );
		 }

		 public override void WriteNode( long nodeId, TextArray labels, MapValue properties )
		 {
			  Append( format( "(id=%d", nodeId ) );
			  string sep = " ";
			  for ( int i = 0; i < labels.Length(); i++ )
			  {
					Append( sep );
					Append( ":" + labels.StringValue( i ) );
					sep = "";
			  }
			  if ( properties.Size() > 0 )
			  {
					Append( " " );
					properties.WriteTo( this );
			  }

			  Append( ")" );
		 }

		 public override void WriteRelationshipReference( long relId )
		 {
			  Append( format( "-[id=%d]-", relId ) );
		 }

		 public override void WriteRelationship( long relId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
		 {
			  Append( format( "-[id=%d :%s", relId, type.StringValue() ) );
			  if ( properties.Size() > 0 )
			  {
					Append( " " );
					properties.WriteTo( this );
			  }
			  Append( "]-" );
		 }

		 public override void BeginMap( int size )
		 {
			  _stack.push( new MapWriter( this ) );
		 }

		 public override void EndMap()
		 {
			  Debug.Assert( !_stack.Empty );
			  Append( _stack.pop().done() );
		 }

		 public override void BeginList( int size )
		 {
			  _stack.push( new ListWriter( this ) );
		 }

		 public override void EndList()
		 {
			  Debug.Assert( !_stack.Empty );
			  Append( _stack.pop().done() );
		 }

		 public override void WritePath( NodeValue[] nodes, RelationshipValue[] relationships )
		 {
			  if ( nodes.Length == 0 )
			  {
					return;
			  }
			  //Path guarantees that nodes.length = edges.length = 1
			  nodes[0].WriteTo( this );
			  for ( int i = 0; i < relationships.Length; i++ )
			  {
					relationships[i].WriteTo( this );
					Append( ">" );
					nodes[i + 1].WriteTo( this );
			  }

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(org.Neo4Net.values.storable.CoordinateReferenceSystem crs, double[] coordinate) throws RuntimeException
		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  Append( "{geometry: {type: \"Point\", coordinates: " );
			  Append( Arrays.ToString( coordinate ) );
			  Append( ", crs: {type: link, properties: {href: \"" );
			  Append( crs.Href );
			  Append( "\", code: " );
			  Append( Convert.ToString( crs.Code ) );
			  Append( "}}}}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws RuntimeException
		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  Append( "{duration: {months: " );
			  Append( Convert.ToString( months ) );
			  Append( ", days: " );
			  Append( Convert.ToString( days ) );
			  Append( ", seconds: " );
			  Append( Convert.ToString( seconds ) );
			  Append( ", nanos: " );
			  Append( Convert.ToString( nanos ) );
			  Append( "}}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws RuntimeException
		 public override void WriteDate( LocalDate localDate )
		 {
			  Append( "{date: " );
			  Append( Quote( localDate.ToString() ) );
			  Append( "}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws RuntimeException
		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  Append( "{localTime: " );
			  Append( Quote( localTime.ToString() ) );
			  Append( "}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws RuntimeException
		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  Append( "{time: " );
			  Append( Quote( offsetTime.ToString() ) );
			  Append( "}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws RuntimeException
		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  Append( "{localDateTime: " );
			  Append( Quote( localDateTime.ToString() ) );
			  Append( "}" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws RuntimeException
		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  Append( "{datetime: " );
			  Append( Quote( zonedDateTime.ToString() ) );
			  Append( "}" );
		 }

		 public override void WriteNull()
		 {
			  Append( "<null>" );
		 }

		 public override void WriteBoolean( bool value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteInteger( sbyte value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteInteger( short value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteInteger( int value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteInteger( long value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteFloatingPoint( float value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteFloatingPoint( double value )
		 {
			  Append( Convert.ToString( value ) );
		 }

		 public override void WriteString( string value )
		 {
			  Append( Quote( value ) );
		 }

		 public override void WriteString( char value )
		 {
			  WriteString( Convert.ToString( value ) );
		 }

		 public override void BeginArray( int size, ArrayType arrayType )
		 {
			  _stack.push( new ListWriter( this ) );
		 }

		 public override void EndArray()
		 {
			  Debug.Assert( !_stack.Empty );
			  Append( _stack.pop().done() );
		 }

		 public override void WriteByteArray( sbyte[] value )
		 {
			  string sep = "";
			  Append( "[" );
			  foreach ( sbyte b in value )
			  {
					Append( sep );
					Append( Convert.ToString( b ) );
					sep = ", ";
			  }
			  Append( "]" );
		 }

		 public virtual string Value()
		 {
			  Debug.Assert( _stack.size() == 1 );
			  return _stack.Last.done();
		 }

		 private void Append( string value )
		 {
			  Debug.Assert( !_stack.Empty );
			  Writer head = _stack.peek();
			  head.Append( value );
		 }

		 private string Quote( string value )
		 {
			  Debug.Assert( !_stack.Empty );
			  Writer head = _stack.peek();
			  return head.Quote( value );
		 }

		 private interface Writer
		 {
			  void Append( string value );

			  string Done();

			  string Quote( string @in );
		 }

		 private abstract class BaseWriter : Writer
		 {
			 public abstract void Append( string value );
			 private readonly PrettyPrinter _outerInstance;

			 public BaseWriter( PrettyPrinter outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  protected internal readonly StringBuilder Builder = new StringBuilder();

			  public override string Done()
			  {
					return Builder.ToString();
			  }

			  public override string Quote( string @in )
			  {
					return outerInstance.quoteMark + @in + outerInstance.quoteMark;
			  }
		 }

		 private class ValueWriter : BaseWriter
		 {
			 private readonly PrettyPrinter _outerInstance;

			 public ValueWriter( PrettyPrinter outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Append( string value )
			  {
					Builder.Append( value );
			  }
		 }

		 private class MapWriter : BaseWriter
		 {
			 private readonly PrettyPrinter _outerInstance;

			  internal bool WriteKey = true;
			  internal string Sep = "";

			  internal MapWriter( PrettyPrinter outerInstance ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Builder.Append( "{" );
			  }

			  public override void Append( string value )
			  {
					if ( WriteKey )
					{
						 Builder.Append( Sep ).Append( value ).Append( ": " );
					}
					else
					{
						 Builder.Append( value );
					}
					WriteKey = !WriteKey;
					Sep = ", ";
			  }

			  public override string Done()
			  {
					return Builder.Append( "}" ).ToString();
			  }

			  public override string Quote( string @in )
			  {
					return WriteKey ? @in : base.Quote( @in );
			  }
		 }

		 private class ListWriter : BaseWriter
		 {
			 private readonly PrettyPrinter _outerInstance;

			  internal string Sep = "";

			  internal ListWriter( PrettyPrinter outerInstance ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Builder.Append( "[" );
			  }

			  public override void Append( string value )
			  {
					Builder.Append( Sep ).Append( value );
					Sep = ", ";
			  }

			  public override string Done()
			  {
					return Builder.Append( "]" ).ToString();
			  }
		 }
	}

}
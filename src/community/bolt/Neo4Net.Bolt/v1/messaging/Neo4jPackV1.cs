using System;
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
namespace Neo4Net.Bolt.v1.messaging
{

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using StructType = Neo4Net.Bolt.messaging.StructType;
	using PackInput = Neo4Net.Bolt.v1.packstream.PackInput;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;
	using PackType = Neo4Net.Bolt.v1.packstream.PackType;
	using PrimitiveLongIntKeyValueArray = Neo4Net.Collections.primitive.PrimitiveLongIntKeyValueArray;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.packstream.PackStream.UNKNOWN_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.byteArray;

	/// <summary>
	/// Extended PackStream packer and unpacker classes for working
	/// with Neo4Net-specific data types, represented as structures.
	/// </summary>
	public class Neo4NetPackV1 : Neo4NetPack
	{
		 public const long VERSION = 1;

		 public const sbyte NODE = ( sbyte )'N';
		 public const int NODE_SIZE = 3;

		 public const sbyte RELATIONSHIP = ( sbyte )'R';
		 public const int RELATIONSHIP_SIZE = 5;

		 public const sbyte UNBOUND_RELATIONSHIP = ( sbyte )'r';
		 public const int UNBOUND_RELATIONSHIP_SIZE = 3;

		 public const sbyte PATH = ( sbyte )'P';
		 public const int PATH_SIZE = 3;

		 public override Neo4Net.Bolt.messaging.Neo4NetPack_Packer NewPacker( PackOutput output )
		 {
			  return new PackerV1( output );
		 }

		 public override Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker NewUnpacker( PackInput input )
		 {
			  return new UnpackerV1( input );
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }

		 protected internal class PackerV1 : PackStream.Packer, AnyValueWriter<IOException>, Neo4Net.Bolt.messaging.Neo4NetPack_Packer
		 {
			  internal const int INITIAL_PATH_CAPACITY = 500;
			  internal const int NO_SUCH_ID = -1;
			  internal readonly PrimitiveLongIntKeyValueArray NodeIndexes = new PrimitiveLongIntKeyValueArray( INITIAL_PATH_CAPACITY + 1 );
			  internal readonly PrimitiveLongIntKeyValueArray RelationshipIndexes = new PrimitiveLongIntKeyValueArray( INITIAL_PATH_CAPACITY );

			  protected internal PackerV1( PackOutput output ) : base( output )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(org.Neo4Net.values.AnyValue value) throws java.io.IOException
			  public override void Pack( AnyValue value )
			  {
					value.WriteTo( this );
			  }

			  public override void WriteNodeReference( long nodeId )
			  {
					throw new System.NotSupportedException( "Cannot write a raw node reference" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNode(long nodeId, org.Neo4Net.values.storable.TextArray labels, org.Neo4Net.values.virtual.MapValue properties) throws java.io.IOException
			  public override void WriteNode( long nodeId, TextArray labels, MapValue properties )
			  {
					PackStructHeader( NODE_SIZE, NODE );
					Pack( nodeId );
					PackListHeader( labels.Length() );
					for ( int i = 0; i < labels.Length(); i++ )
					{
						 labels.Value( i ).writeTo( this );
					}
					properties.WriteTo( this );
			  }

			  public override void WriteRelationshipReference( long relationshipId )
			  {
					throw new System.NotSupportedException( "Cannot write a raw relationship reference" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRelationship(long relationshipId, long startNodeId, long endNodeId, org.Neo4Net.values.storable.TextValue type, org.Neo4Net.values.virtual.MapValue properties) throws java.io.IOException
			  public override void WriteRelationship( long relationshipId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
			  {
					PackStructHeader( RELATIONSHIP_SIZE, RELATIONSHIP );
					Pack( relationshipId );
					Pack( startNodeId );
					Pack( endNodeId );
					type.WriteTo( this );
					properties.WriteTo( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginMap(int size) throws java.io.IOException
			  public override void BeginMap( int size )
			  {
					PackMapHeader( size );
			  }

			  public override void EndMap()
			  {
					//do nothing
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginList(int size) throws java.io.IOException
			  public override void BeginList( int size )
			  {
					PackListHeader( size );
			  }

			  public override void EndList()
			  {
					//do nothing
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePath(org.Neo4Net.values.virtual.NodeValue[] nodes, org.Neo4Net.values.virtual.RelationshipValue[] relationships) throws java.io.IOException
			  public override void WritePath( NodeValue[] nodes, RelationshipValue[] relationships )
			  {
					//A path is serialized in the following form
					// Given path: (a {id: 42})-[r1 {id: 10}]->(b {id: 43})<-[r1 {id: 11}]-(c {id: 44})
					//The serialization will look like:
					//
					// {
					//    [a, b, c]
					//    [r1, r2]
					//    [1, 1, -2, 2]
					// }
					// The first list contains all nodes where the first node (a) is guaranteed to be the start node of
					// the path
					// The second list contains all edges of the path
					// The third list defines the path order, where every other item specifies the offset into the
					// relationship and node list respectively. Since all paths is guaranteed to start with a 0, meaning
					// that
					// a is the start node in this case, those are excluded. So the first integer in the array refers to the
					// position
					// in the relationship array (1 indexed where sign denotes direction) and the second one refers to
					// the offset
					// into the
					// node list (zero indexed) and so on.
					PackStructHeader( PATH_SIZE, PATH );

					WriteNodesForPath( nodes );
					WriteRelationshipsForPath( relationships );

					PackListHeader( 2 * relationships.Length );
					if ( relationships.Length == 0 )
					{
						 return;
					}

					NodeValue node = nodes[0];
					for ( int i = 1; i <= 2 * relationships.Length; i++ )
					{
						 if ( i % 2 == 0 )
						 {
							  node = nodes[i / 2];
							  int index = NodeIndexes.getOrDefault( node.Id(), NO_SUCH_ID );
							  Pack( index );
						 }
						 else
						 {
							  RelationshipValue r = relationships[i / 2];
							  int index = RelationshipIndexes.getOrDefault( r.Id(), NO_SUCH_ID );

							  if ( node.Id() == r.StartNode().id() )
							  {
									Pack( index );
							  }
							  else
							  {
									Pack( -index );
							  }
						 }

					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNodesForPath(org.Neo4Net.values.virtual.NodeValue[] nodes) throws java.io.IOException
			  internal virtual void WriteNodesForPath( NodeValue[] nodes )
			  {
					NodeIndexes.reset( nodes.Length );
					foreach ( NodeValue node in nodes )
					{
						 NodeIndexes.putIfAbsent( node.Id(), NodeIndexes.size() );
					}

					int size = NodeIndexes.size();
					PackListHeader( size );
					if ( size > 0 )
					{
						 NodeValue node = nodes[0];
						 foreach ( long id in NodeIndexes.keys() )
						 {
							  int i = 1;
							  while ( node.Id() != id )
							  {
									node = nodes[i++];
							  }
							  node.WriteTo( this );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRelationshipsForPath(org.Neo4Net.values.virtual.RelationshipValue[] relationships) throws java.io.IOException
			  internal virtual void WriteRelationshipsForPath( RelationshipValue[] relationships )
			  {
					RelationshipIndexes.reset( relationships.Length );
					foreach ( RelationshipValue node in relationships )
					{
						 // relationship indexes are one-based
						 RelationshipIndexes.putIfAbsent( node.Id(), RelationshipIndexes.size() + 1 );
					}

					int size = RelationshipIndexes.size();
					PackListHeader( size );
					if ( size > 0 )
					{
						 {
							  RelationshipValue edge = relationships[0];
							  foreach ( long id in RelationshipIndexes.keys() )
							  {
									int i = 1;
									while ( edge.Id() != id )
									{
										 edge = relationships[i++];
									}
									//Note that we are not doing relationship.writeTo(this) here since the serialization protocol
									//requires these to be _unbound relationships_, thus relationships without any start node nor
									// end node.
									PackStructHeader( UNBOUND_RELATIONSHIP_SIZE, UNBOUND_RELATIONSHIP );
									Pack( edge.Id() );
									edge.Type().writeTo(this);
									edge.Properties().writeTo(this);
							  }
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePoint(org.Neo4Net.values.storable.CoordinateReferenceSystem crs, double[] coordinate) throws java.io.IOException
			  public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					ThrowUnsupportedTypeError( "Point" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDuration(long months, long days, long seconds, int nanos) throws java.io.IOException
			  public override void WriteDuration( long months, long days, long seconds, int nanos )
			  {
					ThrowUnsupportedTypeError( "Duration" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDate(java.time.LocalDate localDate) throws java.io.IOException
			  public override void WriteDate( LocalDate localDate )
			  {
					ThrowUnsupportedTypeError( "Date" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalTime(java.time.LocalTime localTime) throws java.io.IOException
			  public override void WriteLocalTime( LocalTime localTime )
			  {
					ThrowUnsupportedTypeError( "LocalTime" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTime(java.time.OffsetTime offsetTime) throws java.io.IOException
			  public override void WriteTime( OffsetTime offsetTime )
			  {
					ThrowUnsupportedTypeError( "Time" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws java.io.IOException
			  public override void WriteLocalDateTime( DateTime localDateTime )
			  {
					ThrowUnsupportedTypeError( "LocalDateTime" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws java.io.IOException
			  public override void WriteDateTime( ZonedDateTime zonedDateTime )
			  {
					ThrowUnsupportedTypeError( "DateTime" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNull() throws java.io.IOException
			  public override void WriteNull()
			  {
					PackNull();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolean(boolean value) throws java.io.IOException
			  public override void WriteBoolean( bool value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(byte value) throws java.io.IOException
			  public override void WriteInteger( sbyte value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(short value) throws java.io.IOException
			  public override void WriteInteger( short value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(int value) throws java.io.IOException
			  public override void WriteInteger( int value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInteger(long value) throws java.io.IOException
			  public override void WriteInteger( long value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(float value) throws java.io.IOException
			  public override void WriteFloatingPoint( float value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeFloatingPoint(double value) throws java.io.IOException
			  public override void WriteFloatingPoint( double value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeUTF8(byte[] bytes, int offset, int length) throws java.io.IOException
			  public override void WriteUTF8( sbyte[] bytes, int offset, int length )
			  {
					PackUTF8( bytes, offset, length );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(String value) throws java.io.IOException
			  public override void WriteString( string value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeString(char value) throws java.io.IOException
			  public override void WriteString( char value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginArray(int size, ArrayType arrayType) throws java.io.IOException
			  public override void BeginArray( int size, ArrayType arrayType )
			  {
					switch ( arrayType )
					{
					case BYTE:
						 PackBytesHeader( size );
						 break;
					default:
						 PackListHeader( size );
					 break;
					}

			  }

			  public override void EndArray()
			  {
					//Do nothing
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeByteArray(byte[] value) throws java.io.IOException
			  public override void WriteByteArray( sbyte[] value )
			  {
					Pack( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void throwUnsupportedTypeError(String type) throws org.Neo4Net.bolt.messaging.BoltIOException
			  internal virtual void ThrowUnsupportedTypeError( string type )
			  {
					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, type + " is not supported as a return type in Bolt protocol version 1. " + "Please make sure driver supports at least protocol version 2. " + "Driver upgrade is most likely required." );
			  }
		 }

		 protected internal class UnpackerV1 : PackStream.Unpacker, Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker
		 {
			  protected internal UnpackerV1( PackInput input ) : base( input )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue unpack() throws java.io.IOException
			  public override AnyValue Unpack()
			  {
					PackType valType = PeekNextType();
					switch ( valType )
					{
					case PackType.BYTES:
						 return byteArray( UnpackBytes() );
					case PackType.STRING:
						 return Values.utf8Value( UnpackUTF8() );
					case PackType.INTEGER:
						 return Values.longValue( UnpackLong() );
					case PackType.FLOAT:
						 return Values.doubleValue( UnpackDouble() );
					case PackType.BOOLEAN:
						 return Values.booleanValue( UnpackBoolean() );
					case PackType.NULL:
						 // still need to move past the null value
						 UnpackNull();
						 return Values.NO_VALUE;
					case PackType.LIST:
					{
						 return UnpackList();
					}
					case PackType.MAP:
					{
						 return UnpackMap();
					}
					case PackType.STRUCT:
					{
						 long size = UnpackStructHeader();
						 char signature = UnpackStructSignature();
						 return UnpackStruct( signature, size );
					}
					case PackType.END_OF_STREAM:
					{
						 UnpackEndOfStream();
						 return null;
					}
					default:
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, "Unknown value type: " + valType );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.values.virtual.ListValue unpackList() throws java.io.IOException
			  internal virtual ListValue UnpackList()
			  {
					int size = ( int ) UnpackListHeader();
					if ( size == 0 )
					{
						 return VirtualValues.EMPTY_LIST;
					}
					else if ( size == UNKNOWN_SIZE )
					{
						 IList<AnyValue> list = new List<AnyValue>();
						 bool more = true;
						 while ( more )
						 {
							  PackType keyType = PeekNextType();
							  switch ( keyType )
							  {
							  case PackType.END_OF_STREAM:
									Unpack();
									more = false;
									break;
							  default:
									list.Add( Unpack() );
								break;
							  }
						 }
						 return VirtualValues.list( list.ToArray() );
					}
					else
					{
						 AnyValue[] values = new AnyValue[size];
						 for ( int i = 0; i < size; i++ )
						 {
							  values[i] = Unpack();
						 }
						 return VirtualValues.list( values );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.values.AnyValue unpackStruct(char signature, long size) throws java.io.IOException
			  protected internal virtual AnyValue UnpackStruct( char signature, long size )
			  {
					StructType structType = StructType.valueOf( signature );
					if ( structType == null )
					{
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Struct types of 0x{0} are not recognized.", signature.ToString( "x" ) ) );
					}

					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError, string.Format( "{0} values cannot be unpacked with this version of bolt.", structType.description() ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.virtual.MapValue unpackMap() throws java.io.IOException
			  public override MapValue UnpackMap()
			  {
					int size = ( int ) UnpackMapHeader();
					if ( size == 0 )
					{
						 return VirtualValues.EMPTY_MAP;
					}
					MapValueBuilder map;
					if ( size == UNKNOWN_SIZE )
					{
						 map = new MapValueBuilder();
						 bool more = true;
						 while ( more )
						 {
							  PackType keyType = PeekNextType();
							  string key;
							  AnyValue val;
							  switch ( keyType )
							  {
							  case PackType.END_OF_STREAM:
									Unpack();
									more = false;
									break;
							  case PackType.STRING:
									key = UnpackString();
									val = Unpack();
									if ( map.Add( key, val ) != null )
									{
										 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `" + key + "`." );
									}
									break;
							  case PackType.NULL:
									throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." );
							  default:
									throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, "Bad key type: " + keyType );
							  }
						 }
					}
					else
					{
						 map = new MapValueBuilder( size );
						 for ( int i = 0; i < size; i++ )
						 {
							  PackType keyType = PeekNextType();
							  string key;
							  switch ( keyType )
							  {
							  case PackType.NULL:
									throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." );
							  case PackType.STRING:
									key = UnpackString();
									break;
							  default:
									throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, "Bad key type: " + keyType );
							  }

							  AnyValue val = Unpack();
							  if ( map.Add( key, val ) != null )
							  {
									throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `" + key + "`." );
							  }
						 }
					}
					return map.Build();
			  }
		 }
	}

}
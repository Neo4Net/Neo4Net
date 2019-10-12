using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Bolt.v1.messaging
{
	using Test = org.junit.Test;


	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using PackedInputArray = Org.Neo4j.Bolt.v1.packstream.PackedInputArray;
	using PackedOutputArray = Org.Neo4j.Bolt.v1.packstream.PackedOutputArray;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using TextArray = Org.Neo4j.Values.Storable.TextArray;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using UTF8StringValue = Org.Neo4j.Values.Storable.UTF8StringValue;
	using ListValue = Org.Neo4j.Values.@virtual.ListValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.Neo4jPackV1.UNBOUND_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.ALICE_KNOWS_BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.ALICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.ALL_PATHS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.utf8Value;

	public class Neo4jPackV1Test
	{
		 private readonly Neo4jPackV1 _neo4jPack = new Neo4jPackV1();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] packed(org.neo4j.values.AnyValue object) throws java.io.IOException
		 private sbyte[] Packed( AnyValue @object )
		 {
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.Pack( @object );
			  return output.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.values.AnyValue unpacked(byte[] bytes) throws java.io.IOException
		 private AnyValue Unpacked( sbyte[] bytes )
		 {
			  PackedInputArray input = new PackedInputArray( bytes );
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker = _neo4jPack.newUnpacker( input );
			  return unpacker.Unpack();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPackAndUnpackList() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToPackAndUnpackList()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.PackListHeader( ALICE.labels().length() );
			  IList<string> expected = new List<string>();
			  TextArray labels = ALICE.labels();
			  for ( int i = 0; i < labels.Length(); i++ )
			  {
					string labelName = labels.StringValue( i );
					packer.Pack( labelName );
					expected.Add( labelName );
			  }
			  AnyValue unpacked = unpacked( output.Bytes() );

			  // Then
			  assertThat( unpacked, instanceOf( typeof( ListValue ) ) );
			  ListValue unpackedList = ( ListValue ) unpacked;
			  assertThat( unpackedList, equalTo( ValueUtils.asListValue( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPackAndUnpackMap() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToPackAndUnpackMap()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.PackMapHeader( ALICE.properties().size() );
			  ALICE.properties().@foreach((s, value) =>
			  {
				try
				{
					 packer.pack( s );
					 packer.pack( value );
				}
				catch ( IOException e )
				{
					 throw new UncheckedIOException( e );
				}
			  });
			  AnyValue unpacked = unpacked( output.Bytes() );

			  // Then
			  assertThat( unpacked, instanceOf( typeof( MapValue ) ) );
			  MapValue unpackedMap = ( MapValue ) unpacked;
			  assertThat( unpackedMap, equalTo( ALICE.properties() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenTryingToPackAndUnpackMapContainingNullKeys() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenTryingToPackAndUnpackMapContainingNullKeys()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );

			  IDictionary<string, AnyValue> map = new Dictionary<string, AnyValue>();
			  map[null] = longValue( 42L );
			  map["foo"] = longValue( 1337L );
			  packer.PackMapHeader( map.Count );
			  foreach ( KeyValuePair<string, AnyValue> entry in map.SetOfKeyValuePairs() )
			  {
					packer.pack( entry.Key );
					packer.pack( entry.Value );
			  }

			  // When
			  try
			  {
					PackedInputArray input = new PackedInputArray( output.Bytes() );
					Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker = _neo4jPack.newUnpacker( input );
					unpacker.Unpack();

					fail( "exception expected" );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnUnpackingMapWithDuplicateKeys() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnUnpackingMapWithDuplicateKeys()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.PackMapHeader( 2 );
			  packer.Pack( "key" );
			  packer.pack( intValue( 1 ) );
			  packer.Pack( "key" );
			  packer.pack( intValue( 2 ) );

			  // When
			  try
			  {
					PackedInputArray input = new PackedInputArray( output.Bytes() );
					Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker = _neo4jPack.newUnpacker( input );
					unpacker.Unpack();

					fail( "exception expected" );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `key`." ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnUnpackingMapWithUnsupportedKeyType() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnUnpackingMapWithUnsupportedKeyType()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.PackMapHeader( 2 );
			  packer.Pack( ValueUtils.of( 1L ) );
			  packer.pack( intValue( 1 ) );

			  // When
			  try
			  {
					PackedInputArray input = new PackedInputArray( output.Bytes() );
					Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker = _neo4jPack.newUnpacker( input );
					unpacker.Unpack();

					fail( "exception expected" );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, "Bad key type: INTEGER" ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUnpackNode() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToUnpackNode()
		 {
			  try
			  {
					// When
					Unpacked( Packed( ALICE ) );
					fail( "exception expected." );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, "Node values cannot be unpacked with this version of bolt." ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUnpackRelationship() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToUnpackRelationship()
		 {
			  try
			  {
					// When
					Unpacked( Packed( ALICE_KNOWS_BOB ) );
					fail( "exception expected." );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, "Relationship values cannot be unpacked with this version of bolt." ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUnpackUnboundRelationship() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToUnpackUnboundRelationship()
		 {
			  // Given
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4jPackV1.Packer packer = _neo4jPack.newPacker( @out );

			  packer.packStructHeader( 3, UNBOUND_RELATIONSHIP );
			  packer.pack( ValueUtils.of( 1L ) );
			  packer.pack( ValueUtils.of( "RELATES_TO" ) );
			  packer.pack( ValueUtils.asMapValue( MapUtil.map( "a", 1L, "b", "x" ) ) );

			  try
			  {
					// When
					Unpacked( @out.Bytes() );
					fail( "exception expected." );
			  }
			  catch ( BoltIOException ex )
			  {
					assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, "Relationship values cannot be unpacked with this version of bolt." ), Neo4jError.from( ex ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUnpackPaths() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToUnpackPaths()
		 {
			  foreach ( PathValue path in ALL_PATHS )
			  {
					try
					{
						 // When
						 Unpacked( Packed( path ) );
						 fail( "exception expected." );
					}
					catch ( BoltIOException ex )
					{
						 assertEquals( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, "Path values cannot be unpacked with this version of bolt." ), Neo4jError.from( ex ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatSingleCharAsSingleCharacterString() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTreatSingleCharAsSingleCharacterString()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.pack( charValue( 'C' ) );
			  AnyValue unpacked = unpacked( output.Bytes() );

			  // Then
			  assertThat( unpacked, instanceOf( typeof( TextValue ) ) );
			  assertThat( unpacked, equalTo( stringValue( "C" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatCharArrayAsListOfStrings() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTreatCharArrayAsListOfStrings()
		 {
			  // Given
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.pack( charArray( new char[]{ 'W', 'H', 'Y' } ) );
			  object unpacked = unpacked( output.Bytes() );

			  // Then
			  assertThat( unpacked, instanceOf( typeof( ListValue ) ) );
			  assertThat( unpacked, equalTo( VirtualValues.list( stringValue( "W" ), stringValue( "H" ), stringValue( "Y" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackUtf8() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPackUtf8()
		 {
			  // Given
			  string value = "\uD83D\uDE31";
			  sbyte[] bytes = value.GetBytes( Encoding.UTF8 );
			  TextValue textValue = utf8Value( bytes, 0, bytes.Length );
			  PackedOutputArray output = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( output );
			  packer.pack( textValue );

			  // When
			  AnyValue unpacked = unpacked( output.Bytes() );
			  assertThat( unpacked, @is( instanceOf( typeof( UTF8StringValue ) ) ) );

			  // Then
			  assertThat( unpacked, equalTo( textValue ) );
		 }
	}

}
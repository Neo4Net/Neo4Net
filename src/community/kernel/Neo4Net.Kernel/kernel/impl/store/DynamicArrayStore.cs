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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.Collections.Helpers;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Capability = Neo4Net.Kernel.impl.store.format.Capability;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using UnsupportedFormatCapabilityException = Neo4Net.Kernel.impl.store.format.UnsupportedFormatCapabilityException;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CRSTable = Neo4Net.Values.Storable.CRSTable;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.arraycopy;

	/// <summary>
	/// Dynamic store that stores arrays.
	/// 
	/// Arrays are uniform collections of the same type. They can contain primitives, strings or Geometries.
	/// <ul>
	///     <li>
	///         Primitive arrays are stored using a 3 byte header followed by a byte[] of bit-compacted data. The header defines the format of the byte[]:
	///         <ul>
	///             <li>Byte 0: The type of the primitive being stored. See <seealso cref="PropertyType"/></li>
	///             <li>Byte 1: The number of bits used in the last byte</li>
	///             <li>Byte 2: The number of bits required for each element of the data array (after compaction)</li>
	///         </ul>
	///         The total number of elements can be calculated by combining the information about the individual element size
	///         (bits required - 3rd byte) with the length of the data specified in the DynamicRecordFormat.
	///     </li>
	///     <li>
	///         Arrays of strings are stored using a 5 byte header:
	///         <ul>
	///             <li>Byte 0: PropertyType.STRING</li>
	///             <li>Bytes 1 to 4: 32bit Int length of string array</li>
	///         </ul>
	///         This is followed by a byte[] composed of a 4 byte header containing the length of the byte[] representstion of the string, and then those bytes.
	///     </li>
	///     <li>
	///         Arrays of Geometries starting with a 6 byte header:
	///         <ul>
	///             <li>Byte 0: PropertyType.GEOMETRY</li>
	///             <li>Byte 1: GeometryType, currently only POINT is supported</li>
	///             <li>Byte 2: The dimension of the geometry (currently only 2 or 3 dimensions are supported)</li>
	///             <li>Byte 3: Coordinate Reference System Table id: <seealso cref="CRSTable"/></li>
	///             <li>Bytes 4-5: 16bit short Coordinate Reference System code: <seealso cref="CoordinateReferenceSystem"/></li>
	///         </ul>
	///         The format of the body is specific to the type of Geometry being stored:
	///         <ul>
	///             <li>Points: Stored as double[] using the same format as primitive arrays above, starting with the 3 byte header (see above)</li>
	///         </ul>
	///     </li>
	/// </ul>
	/// </summary>
	public class DynamicArrayStore : AbstractDynamicStore
	{
		 public const int NUMBER_HEADER_SIZE = 3;
		 public const int STRING_HEADER_SIZE = 5;
		 public const int GEOMETRY_HEADER_SIZE = 6; // This should match contents of GeometryType.GeometryHeader
		 public const int TEMPORAL_HEADER_SIZE = 2;

		 // store version, each store ends with this string (byte encoded)
		 public const string TYPE_DESCRIPTOR = "ArrayPropertyStore";
		 private readonly bool _allowStorePointsAndTemporal;

		 public DynamicArrayStore( File file, File idFile, Config configuration, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, int dataSizeFromConfiguration, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, configuration, idType, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, dataSizeFromConfiguration, recordFormats.Dynamic(), recordFormats.StoreVersion(), openOptions )
		 {
			  _allowStorePointsAndTemporal = recordFormats.HasCapability( Capability.POINT_PROPERTIES ) && recordFormats.HasCapability( Capability.TEMPORAL_PROPERTIES );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.Neo4Net.kernel.impl.store.record.DynamicRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, DynamicRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessArray( this, record );
		 }

		 public static sbyte[] EncodeFromNumbers( object array, int offsetBytes )
		 {
			  ShortArray type = ShortArray.typeOf( array );
			  if ( type == null )
			  {
					throw new System.ArgumentException( array + " not a valid array type." );
			  }

			  if ( type == ShortArray.Double || type == ShortArray.Float )
			  {
					// Skip array compaction for floating point numbers where compaction makes very little difference
					return CreateUncompactedArray( type, array, offsetBytes );
			  }
			  else
			  {
					return CreateBitCompactedArray( type, array, offsetBytes );
			  }
		 }

		 private static sbyte[] CreateBitCompactedArray( ShortArray type, object array, int offsetBytes )
		 {
			  Type componentType = array.GetType().GetElementType();
			  bool isPrimitiveByteArray = componentType.Equals( Byte.TYPE );
			  bool isByteArray = componentType.Equals( typeof( Byte ) ) || isPrimitiveByteArray;
			  int arrayLength = Array.getLength( array );
			  int requiredBits = isByteArray ? ( sizeof( sbyte ) * 8 ) : type.calculateRequiredBitsForArray( array, arrayLength );
			  int totalBits = requiredBits * arrayLength;
			  int bitsUsedInLastByte = totalBits % 8;
			  bitsUsedInLastByte = bitsUsedInLastByte == 0 ? 8 : bitsUsedInLastByte;
			  if ( isByteArray )
			  {
					return CreateBitCompactedByteArray( type, isPrimitiveByteArray, array, bitsUsedInLastByte, requiredBits, offsetBytes );
			  }
			  else
			  {
					int numberOfBytes = ( totalBits - 1 ) / 8 + 1;
					numberOfBytes += NUMBER_HEADER_SIZE; // type + rest + requiredBits header. TODO no need to use full bytes
					Bits bits = Bits.bits( numberOfBytes );
					bits.Put( ( sbyte ) type.intValue() );
					bits.Put( ( sbyte ) bitsUsedInLastByte );
					bits.Put( ( sbyte ) requiredBits );
					type.writeAll( array, arrayLength, requiredBits, bits );
					return bits.AsBytes( offsetBytes );
			  }
		 }

		 private static sbyte[] CreateBitCompactedByteArray( ShortArray type, bool isPrimitiveByteArray, object array, int bitsUsedInLastByte, int requiredBits, int offsetBytes )
		 {
			  int arrayLength = Array.getLength( array );
			  sbyte[] bytes = new sbyte[NUMBER_HEADER_SIZE + arrayLength + offsetBytes];
			  bytes[offsetBytes + 0] = ( sbyte ) type.intValue();
			  bytes[offsetBytes + 1] = ( sbyte ) bitsUsedInLastByte;
			  bytes[offsetBytes + 2] = ( sbyte ) requiredBits;
			  if ( isPrimitiveByteArray )
			  {
					arraycopy( array, 0, bytes, NUMBER_HEADER_SIZE + offsetBytes, arrayLength );
			  }
			  else
			  {
					sbyte?[] source = ( sbyte?[] ) array;
					for ( int i = 0; i < source.Length; i++ )
					{
						 bytes[NUMBER_HEADER_SIZE + offsetBytes + i] = source[i].Value;
					}
			  }
			  return bytes;
		 }

		 private static sbyte[] CreateUncompactedArray( ShortArray type, object array, int offsetBytes )
		 {
			  int arrayLength = Array.getLength( array );
			  int bytesPerElement = type.maxBits / 8;
			  sbyte[] bytes = new sbyte[NUMBER_HEADER_SIZE + bytesPerElement * arrayLength + offsetBytes];
			  bytes[offsetBytes + 0] = ( sbyte ) type.intValue();
			  bytes[offsetBytes + 1] = ( sbyte ) 8;
			  bytes[offsetBytes + 2] = ( sbyte ) type.maxBits;
			  type.writeAll( array, bytes, NUMBER_HEADER_SIZE + offsetBytes );
			  return bytes;
		 }

		 public static void AllocateFromNumbers( ICollection<DynamicRecord> target, object array, DynamicRecordAllocator recordAllocator )
		 {
			  sbyte[] bytes = EncodeFromNumbers( array, 0 );
			  AllocateRecordsFromBytes( target, bytes, recordAllocator );
		 }

		 private static void AllocateFromCompositeType( ICollection<DynamicRecord> target, sbyte[] bytes, DynamicRecordAllocator recordAllocator, bool allowsStorage, Capability storageCapability )
		 {
			  if ( allowsStorage )
			  {
					AllocateRecordsFromBytes( target, bytes, recordAllocator );
			  }
			  else
			  {
					throw new UnsupportedFormatCapabilityException( storageCapability );
			  }
		 }

		 private static void AllocateFromString( ICollection<DynamicRecord> target, string[] array, DynamicRecordAllocator recordAllocator )
		 {
			  sbyte[][] stringsAsBytes = new sbyte[array.Length][];
			  int totalBytesRequired = STRING_HEADER_SIZE; // 1b type + 4b array length
			  for ( int i = 0; i < array.Length; i++ )
			  {
					string @string = array[i];
					sbyte[] bytes = PropertyStore.EncodeString( @string );
					stringsAsBytes[i] = bytes;
					totalBytesRequired += 4 + bytes.Length;
			  }

			  ByteBuffer buf = ByteBuffer.allocate( totalBytesRequired );
			  buf.put( PropertyType.String.byteValue() );
			  buf.putInt( array.Length );
			  foreach ( sbyte[] stringAsBytes in stringsAsBytes )
			  {
					buf.putInt( stringAsBytes.Length );
					buf.put( stringAsBytes );
			  }
			  AllocateRecordsFromBytes( target, buf.array(), recordAllocator );
		 }

		 public virtual void AllocateRecords( ICollection<DynamicRecord> target, object array )
		 {
			  AllocateRecords( target, array, this, _allowStorePointsAndTemporal );
		 }

		 public static void AllocateRecords( ICollection<DynamicRecord> target, object array, DynamicRecordAllocator recordAllocator, bool allowStorePointsAndTemporal )
		 {
			  if ( !array.GetType().IsArray )
			  {
					throw new System.ArgumentException( array + " not an array" );
			  }

			  Type type = array.GetType().GetElementType();
			  if ( type.Equals( typeof( string ) ) )
			  {
					AllocateFromString( target, ( string[] ) array, recordAllocator );
			  }
			  else if ( type.Equals( typeof( PointValue ) ) )
			  {
					AllocateFromCompositeType( target,GeometryType.encodePointArray( ( PointValue[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.POINT_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( LocalDate ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeDateArray( ( LocalDate[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( LocalTime ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeLocalTimeArray( ( LocalTime[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( DateTime ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeLocalDateTimeArray( ( DateTime[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( OffsetTime ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeTimeArray( ( OffsetTime[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( ZonedDateTime ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeDateTimeArray( ( ZonedDateTime[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else if ( type.Equals( typeof( DurationValue ) ) )
			  {
					AllocateFromCompositeType( target, TemporalType.encodeDurationArray( ( DurationValue[] ) array ), recordAllocator, allowStorePointsAndTemporal, Capability.TEMPORAL_PROPERTIES );
			  }
			  else
			  {
					AllocateFromNumbers( target, array, recordAllocator );
			  }
		 }

		 public static Value GetRightArray( Pair<sbyte[], sbyte[]> data )
		 {
			  sbyte[] header = data.First();
			  sbyte[] bArray = data.Other();
			  sbyte typeId = header[0];
			  if ( typeId == PropertyType.String.intValue() )
			  {
					ByteBuffer headerBuffer = ByteBuffer.wrap( header, 1, header.Length - 1 );
					int arrayLength = headerBuffer.Int;
					string[] result = new string[arrayLength];

					ByteBuffer dataBuffer = ByteBuffer.wrap( bArray );
					for ( int i = 0; i < arrayLength; i++ )
					{
						 int byteLength = dataBuffer.Int;
						 sbyte[] stringByteArray = new sbyte[byteLength];
						 dataBuffer.get( stringByteArray );
						 result[i] = PropertyStore.DecodeString( stringByteArray );
					}
					return Values.stringArray( result );
			  }
			  else if ( typeId == PropertyType.Geometry.intValue() )
			  {
					GeometryType.GeometryHeader geometryHeader = GeometryType.GeometryHeader.fromArrayHeaderBytes( header );
					return GeometryType.decodeGeometryArray( geometryHeader, bArray );
			  }
			  else if ( typeId == PropertyType.Temporal.intValue() )
			  {
					TemporalType.TemporalHeader temporalHeader = TemporalType.TemporalHeader.fromArrayHeaderBytes( header );
					return TemporalType.decodeTemporalArray( temporalHeader, bArray );
			  }
			  else
			  {
					ShortArray type = ShortArray.typeOf( typeId );
					int bitsUsedInLastByte = header[1];
					int requiredBits = header[2];
					if ( requiredBits == 0 )
					{
						 return type.createEmptyArray();
					}
					if ( type == ShortArray.Byte && requiredBits == ( sizeof( sbyte ) * 8 ) )
					{ // Optimization for byte arrays (probably large ones)
						 return Values.byteArray( bArray );
					}
					else
					{ // Fallback to the generic approach, which is a slower
						 Bits bits = Bits.bitsFromBytes( bArray );
						 int length = ( bArray.Length * 8 - ( 8 - bitsUsedInLastByte ) ) / requiredBits;
						 return type.createArray( length, bits, requiredBits );
					}
			  }
		 }

		 public virtual object GetArrayFor( IEnumerable<DynamicRecord> records )
		 {
			  return GetRightArray( ReadFullByteArray( records, PropertyType.Array ) ).asObject();
		 }
	}

}
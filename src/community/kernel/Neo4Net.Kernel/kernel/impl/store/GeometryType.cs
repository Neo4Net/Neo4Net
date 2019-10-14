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

	using Neo4Net.Helpers.Collections;
	using StandardFormatSettings = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using FloatingPointArray = Neo4Net.Values.Storable.FloatingPointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// For the PropertyStore format, check <seealso cref="PropertyStore"/>.
	/// For the array format, check <seealso cref="DynamicArrayStore"/>.
	/// </summary>
	public abstract class GeometryType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GEOMETRY_INVALID(0, "Invalid") { public org.neo4j.values.storable.Value decode(org.neo4j.values.storable.CoordinateReferenceSystem crs, int dimension, long[] valueBlocks, int offset) { throw new UnsupportedOperationException("Cannot decode invalid geometry"); } public int calculateNumberOfBlocksUsedForGeometry(long firstBlock) { return PropertyType.BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING; } public org.neo4j.values.storable.ArrayValue decodeArray(GeometryHeader header, byte[] data) { throw new UnsupportedOperationException("Cannot decode invalid geometry array"); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GEOMETRY_POINT(1, "Point") { public org.neo4j.values.storable.Value decode(org.neo4j.values.storable.CoordinateReferenceSystem crs, int dimension, long[] valueBlocks, int offset) { double[] coordinate = new double[dimension]; for(int i = 0; i < dimension; i++) { coordinate[i] = Double.longBitsToDouble(valueBlocks[i + 1 + offset]); } return org.neo4j.values.storable.Values.pointValue(crs, coordinate); } public int calculateNumberOfBlocksUsedForGeometry(long firstBlock) { int dimension = getDimension(firstBlock); if(dimension > GeometryType.getMaxSupportedDimensions()) { return PropertyType.BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING; } return 1 + dimension; } public org.neo4j.values.storable.ArrayValue decodeArray(GeometryHeader header, byte[] data) { byte[] dataHeader = PropertyType.ARRAY.readDynamicRecordHeader(data); byte[] dataBody = new byte[data.length - dataHeader.length]; System.arraycopy(data, dataHeader.length, dataBody, 0, dataBody.length); org.neo4j.values.storable.Value dataValue = DynamicArrayStore.getRightArray(org.neo4j.helpers.collection.Pair.of(dataHeader, dataBody)); if(dataValue instanceof org.neo4j.values.storable.FloatingPointArray) { org.neo4j.values.storable.FloatingPointArray numbers = (org.neo4j.values.storable.FloatingPointArray) dataValue; PointValue[] points = new org.neo4j.values.storable.PointValue[numbers.length() / header.dimension]; for(int i = 0; i < points.length; i++) { double[] coords = new double[header.dimension]; for(int d = 0; d < header.dimension; d++) { coords[d] = numbers.doubleValue(i * header.dimension + d); } points[i] = org.neo4j.values.storable.Values.pointValue(header.crs, coords); } return org.neo4j.values.storable.Values.pointArray(points); } else { throw new InvalidRecordException("Point array with unexpected type. Actual:" + dataValue.getClass().getSimpleName() + ". Expected: FloatingPointArray."); } } };

		 private static readonly IList<GeometryType> valueList = new List<GeometryType>();

		 static GeometryType()
		 {
			 valueList.Add( GEOMETRY_INVALID );
			 valueList.Add( GEOMETRY_POINT );
		 }

		 public enum InnerEnum
		 {
			 GEOMETRY_INVALID,
			 GEOMETRY_POINT
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private GeometryType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Handler for header information for Geometry objects and arrays of Geometry objects
		 /// </summary>
		 public static final GeometryType public static class GeometryHeader
		 {
			 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.Get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( sbyte[] bytes ) { bytes[0] = ( sbyte ) PropertyType.Geometry.intValue(); bytes[1] = (sbyte) geometryType; bytes[2] = (sbyte) dimension; bytes[3] = (sbyte) crs.Table.TableId; bytes[4] = unchecked((sbyte)(crs.Code >> 8 & 0xFFL)); bytes[5] = unchecked((sbyte)(crs.Code & 0xFFL)); } static GeometryHeader fromArrayHeaderBytes(sbyte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
		 }
		 private static final GeometryType[] TYPES = GeometryType.values = new GeometryType("public static class GeometryHeader { private final int geometryType; private final int dimension; private final org.neo4j.values.storable.CoordinateReferenceSystem crs; private GeometryHeader(int geometryType, int dimension, org.neo4j.values.storable.CoordinateReferenceSystem crs) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader(int geometryType, int dimension, int crsTableId, int crsCode) { this(geometryType, dimension, org.neo4j.values.storable.CoordinateReferenceSystem.get(crsTableId, crsCode)); } private void writeArrayHeaderTo(byte[] bytes) { bytes[0] = (byte) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } } private static final GeometryType[] TYPES = GeometryType.values", InnerEnum.public static class GeometryHeader
		 {
			 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.Get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( sbyte[] bytes ) { bytes[0] = ( sbyte ) PropertyType.Geometry.intValue(); bytes[1] = (sbyte) geometryType; bytes[2] = (sbyte) dimension; bytes[3] = (sbyte) crs.Table.TableId; bytes[4] = unchecked((sbyte)(crs.Code >> 8 & 0xFFL)); bytes[5] = unchecked((sbyte)(crs.Code & 0xFFL)); } static GeometryHeader fromArrayHeaderBytes(sbyte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
		 }
		 private static final GeometryType[] TYPES = GeometryType.values, );

		 private static readonly IList<GeometryType> valueList = new List<GeometryType>();

		 static GeometryType()
		 {
			 valueList.Add( GEOMETRY_INVALID );
			 valueList.Add( GEOMETRY_POINT );
			 valueList.Add(public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values);
		 }

		 public enum InnerEnum
		 {
			 GEOMETRY_INVALID,
			 GEOMETRY_POINT,
			 public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private static readonly IDictionary<string, GeometryType> all = new Dictionary<string, GeometryType>( TYPES.length );

		 public static readonly GeometryType static
		 {
			 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
		 }
		 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L = new GeometryType("static { for(GeometryType geometryType : TYPES) { all.put(geometryType.name, geometryType); } } private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L", InnerEnum.static
		 {
			 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
		 }
		 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L);

		 private static readonly IList<GeometryType> valueList = new List<GeometryType>();

		 static GeometryType()
		 {
			 valueList.Add( GEOMETRY_INVALID );
			 valueList.Add( GEOMETRY_POINT );
			 valueList.Add(public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values);
			 valueList.Add(static
			 {
				 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
			 }
			 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L);
		 }

		 public enum InnerEnum
		 {
			 GEOMETRY_INVALID,
			 GEOMETRY_POINT,
			 public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values,
			 static
			 {
				 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
			 }
			 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private const long DIMENSION_MASK = 0x0000000F00000000L;
		 private const long CRS_TABLE_MASK = 0x000000F000000000L;
		 private const long CRS_CODE_MASK = 0x00FFFF0000000000L;
		 private const long PRECISION_MASK = 0x0100000000000000L;

		 public static readonly GeometryType private static int getGeometryType( long firstBlock ) { return( int )( ( firstBlock & GEOMETRY_TYPE_MASK ) >> 28 ); } private static int getDimension( long firstBlock ) { return( int )( ( firstBlock & DIMENSION_MASK ) >> 32 ); } private static int getCRSTable( long firstBlock ) { return( int )( ( firstBlock & CRS_TABLE_MASK ) >> 36 ); } private static int getCRSCode( long firstBlock ) { return( int )( ( firstBlock & CRS_CODE_MASK ) >> 40 ); } private static boolean isFloatPrecision( long firstBlock ) { return( ( firstBlock & PRECISION_MASK ) >> 56 ) == 1; } private static int getMaxSupportedDimensions() { return PropertyType.getPayloadSizeLongs() - 1; } public static int calculateNumberOfBlocksUsed(long firstBlock) { GeometryType geometryType = find(getGeometryType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForGeometry(firstBlock); } private static GeometryType find(int gtype)
		 {
			 if ( gtype < TYPES.length && gtype >= 0 ) { return TYPES[gtype]; } else { return GEOMETRY_INVALID; }
		 }
		 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.Impl.Store.Records.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset)
		 {
			 long firstBlock = valueBlocks[offset]; int gtype = getGeometryType( firstBlock ); int dimension = getDimension( firstBlock ); if ( isFloatPrecision( firstBlock ) ) { throw new UnsupportedOperationException( "Float precision is unsupported in Geometry properties" ); } if ( dimension > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } Neo4Net.Values.Storable.CoordinateReferenceSystem crs = Neo4Net.Values.Storable.CoordinateReferenceSystem.get(getCRSTable(firstBlock), getCRSCode(firstBlock)); return find(gtype).decode(crs, dimension, valueBlocks, offset);
		 }
		 public static long[] encodePoint( int keyId, Neo4Net.Values.Storable.CoordinateReferenceSystem crs, double[] coordinate )
		 {
			 if ( coordinate.length > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.GEOMETRY.intValue()) << idBits)); long gtypeBits = GeometryType.GEOMETRY_POINT.gtype << (idBits + 4); long dimensionBits = ((long) coordinate.length) << (idBits + 8); long crsTableIdBits = ((long) crs.getTable().getTableId()) << (idBits + 12); long crsCodeBits = ((long) crs.getCode()) << (idBits + 16); long[] data = new long[1 + coordinate.length]; data[0] = keyAndType | gtypeBits | dimensionBits | crsTableIdBits | crsCodeBits; for (int i = 0; i < coordinate.length; i++) { data[1 + i] = Double.doubleToLongBits(coordinate[i]); } return data;
		 }
		 public static byte[] encodePointArray( Neo4Net.Values.Storable.PointValue[] points )
		 {
			 int dimension = points[0].coordinate().length; Neo4Net.Values.Storable.CoordinateReferenceSystem crs = points[0].getCoordinateReferenceSystem(); for (int i = 1; i < points.length; i++)
			 {
				 if ( dimension != points[i].coordinate().length ) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent dimension. Point " + i + " has a different dimension."); } if (!crs.equals(points[i].getCoordinateReferenceSystem())) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent CRS. Point " + i + " has a different CRS."); }
			 }
			 double[] data = new double[points.length * dimension]; for ( int i = 0; i < data.length; i++ ) { data[i] = points[i / dimension].coordinate()[i % dimension]; } GeometryHeader geometryHeader = new GeometryHeader(GeometryType.GEOMETRY_POINT.gtype, dimension, crs); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.GEOMETRY_HEADER_SIZE); geometryHeader.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static Neo4Net.Values.Storable.ArrayValue decodeGeometryArray( GeometryHeader header, byte[] data ) { return find( header.geometryType ).decodeArray( header, data ); } private final int gtype = new GeometryType("private static int getGeometryType(long firstBlock) { return(int)((firstBlock & GEOMETRY_TYPE_MASK) >> 28); } private static int getDimension(long firstBlock) { return(int)((firstBlock & DIMENSION_MASK) >> 32); } private static int getCRSTable(long firstBlock) { return(int)((firstBlock & CRS_TABLE_MASK) >> 36); } private static int getCRSCode(long firstBlock) { return(int)((firstBlock & CRS_CODE_MASK) >> 40); } private static boolean isFloatPrecision(long firstBlock) { return((firstBlock & PRECISION_MASK) >> 56) == 1; } private static int getMaxSupportedDimensions() { return PropertyType.getPayloadSizeLongs() - 1; } public static int calculateNumberOfBlocksUsed(long firstBlock) { GeometryType geometryType = find(getGeometryType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForGeometry(firstBlock); } private static GeometryType find(int gtype) { if(gtype < TYPES.length && gtype >= 0) { return TYPES[gtype]; } else { return GEOMETRY_INVALID; } } public static org.neo4j.values.storable.Value decode(org.neo4j.kernel.impl.store.record.PropertyBlock block) { return decode(block.getValueBlocks(), 0); } public static org.neo4j.values.storable.Value decode(long[] valueBlocks, int offset) { long firstBlock = valueBlocks[offset]; int gtype = getGeometryType(firstBlock); int dimension = getDimension(firstBlock); if(isFloatPrecision(firstBlock)) { throw new UnsupportedOperationException("Float precision is unsupported in Geometry properties"); } if(dimension > GeometryType.getMaxSupportedDimensions()) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } org.neo4j.values.storable.CoordinateReferenceSystem crs = org.neo4j.values.storable.CoordinateReferenceSystem.get(getCRSTable(firstBlock), getCRSCode(firstBlock)); return find(gtype).decode(crs, dimension, valueBlocks, offset); } public static long[] encodePoint(int keyId, org.neo4j.values.storable.CoordinateReferenceSystem crs, double[] coordinate) { if(coordinate.length > GeometryType.getMaxSupportedDimensions()) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } int idBits = org.neo4j.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.GEOMETRY.intValue()) << idBits)); long gtypeBits = GeometryType.GEOMETRY_POINT.gtype << (idBits + 4); long dimensionBits = ((long) coordinate.length) << (idBits + 8); long crsTableIdBits = ((long) crs.getTable().getTableId()) << (idBits + 12); long crsCodeBits = ((long) crs.getCode()) << (idBits + 16); long[] data = new long[1 + coordinate.length]; data[0] = keyAndType | gtypeBits | dimensionBits | crsTableIdBits | crsCodeBits; for(int i = 0; i < coordinate.length; i++) { data[1 + i] = Double.doubleToLongBits(coordinate[i]); } return data; } public static byte[] encodePointArray(org.neo4j.values.storable.PointValue[] points) { int dimension = points[0].coordinate().length; org.neo4j.values.storable.CoordinateReferenceSystem crs = points[0].getCoordinateReferenceSystem(); for(int i = 1; i < points.length; i++) { if(dimension != points[i].coordinate().length) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent dimension. Point " + i + " has a different dimension."); } if(!crs.equals(points[i].getCoordinateReferenceSystem())) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent CRS. Point " + i + " has a different CRS."); } } double[] data = new double[points.length * dimension]; for(int i = 0; i < data.length; i++) { data[i] = points[i / dimension].coordinate()[i % dimension]; } GeometryHeader geometryHeader = new GeometryHeader(GeometryType.GEOMETRY_POINT.gtype, dimension, crs); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.GEOMETRY_HEADER_SIZE); geometryHeader.writeArrayHeaderTo(bytes); return bytes; } public static org.neo4j.values.storable.ArrayValue decodeGeometryArray(GeometryHeader header, byte[] data) { return find(header.geometryType).decodeArray(header, data); } private final int gtype", InnerEnum.private static int getGeometryType(long firstBlock) { return(int)((firstBlock & GEOMETRY_TYPE_MASK) >> 28); } private static int getDimension(long firstBlock) { return(int)((firstBlock & DIMENSION_MASK) >> 32); } private static int getCRSTable(long firstBlock) { return(int)((firstBlock & CRS_TABLE_MASK) >> 36); } private static int getCRSCode(long firstBlock) { return(int)((firstBlock & CRS_CODE_MASK) >> 40); } private static boolean isFloatPrecision(long firstBlock) { return((firstBlock & PRECISION_MASK) >> 56) == 1; } private static int getMaxSupportedDimensions() { return PropertyType.getPayloadSizeLongs() - 1; } public static int calculateNumberOfBlocksUsed(long firstBlock) { GeometryType geometryType = find(getGeometryType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForGeometry(firstBlock); } private static GeometryType find(int gtype)
		 {
			 if ( gtype < TYPES.length && gtype >= 0 ) { return TYPES[gtype]; } else { return GEOMETRY_INVALID; }
		 }
		 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.Impl.Store.Records.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset)
		 {
			 long firstBlock = valueBlocks[offset]; int gtype = getGeometryType( firstBlock ); int dimension = getDimension( firstBlock ); if ( isFloatPrecision( firstBlock ) ) { throw new UnsupportedOperationException( "Float precision is unsupported in Geometry properties" ); } if ( dimension > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } Neo4Net.Values.Storable.CoordinateReferenceSystem crs = Neo4Net.Values.Storable.CoordinateReferenceSystem.get(getCRSTable(firstBlock), getCRSCode(firstBlock)); return find(gtype).decode(crs, dimension, valueBlocks, offset);
		 }
		 public static long[] encodePoint( int keyId, Neo4Net.Values.Storable.CoordinateReferenceSystem crs, double[] coordinate )
		 {
			 if ( coordinate.length > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.GEOMETRY.intValue()) << idBits)); long gtypeBits = GeometryType.GEOMETRY_POINT.gtype << (idBits + 4); long dimensionBits = ((long) coordinate.length) << (idBits + 8); long crsTableIdBits = ((long) crs.getTable().getTableId()) << (idBits + 12); long crsCodeBits = ((long) crs.getCode()) << (idBits + 16); long[] data = new long[1 + coordinate.length]; data[0] = keyAndType | gtypeBits | dimensionBits | crsTableIdBits | crsCodeBits; for (int i = 0; i < coordinate.length; i++) { data[1 + i] = Double.doubleToLongBits(coordinate[i]); } return data;
		 }
		 public static byte[] encodePointArray( Neo4Net.Values.Storable.PointValue[] points )
		 {
			 int dimension = points[0].coordinate().length; Neo4Net.Values.Storable.CoordinateReferenceSystem crs = points[0].getCoordinateReferenceSystem(); for (int i = 1; i < points.length; i++)
			 {
				 if ( dimension != points[i].coordinate().length ) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent dimension. Point " + i + " has a different dimension."); } if (!crs.equals(points[i].getCoordinateReferenceSystem())) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent CRS. Point " + i + " has a different CRS."); }
			 }
			 double[] data = new double[points.length * dimension]; for ( int i = 0; i < data.length; i++ ) { data[i] = points[i / dimension].coordinate()[i % dimension]; } GeometryHeader geometryHeader = new GeometryHeader(GeometryType.GEOMETRY_POINT.gtype, dimension, crs); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.GEOMETRY_HEADER_SIZE); geometryHeader.writeArrayHeaderTo(bytes); return bytes;
		 }
		 public static Neo4Net.Values.Storable.ArrayValue decodeGeometryArray( GeometryHeader header, byte[] data ) { return find( header.geometryType ).decodeArray( header, data ); } private final int gtype);

		 private static readonly IList<GeometryType> valueList = new List<GeometryType>();

		 static GeometryType()
		 {
			 valueList.Add( GEOMETRY_INVALID );
			 valueList.Add( GEOMETRY_POINT );
			 valueList.Add(public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values);
			 valueList.Add(static
			 {
				 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
			 }
			 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L);
			 valueList.Add(private static int getGeometryType(long firstBlock) { return(int)((firstBlock & GEOMETRY_TYPE_MASK) >> 28); } private static int getDimension(long firstBlock) { return(int)((firstBlock & DIMENSION_MASK) >> 32); } private static int getCRSTable(long firstBlock) { return(int)((firstBlock & CRS_TABLE_MASK) >> 36); } private static int getCRSCode(long firstBlock) { return(int)((firstBlock & CRS_CODE_MASK) >> 40); } private static boolean isFloatPrecision(long firstBlock) { return((firstBlock & PRECISION_MASK) >> 56) == 1; } private static int getMaxSupportedDimensions() { return PropertyType.getPayloadSizeLongs() - 1; } public static int calculateNumberOfBlocksUsed(long firstBlock) { GeometryType geometryType = find(getGeometryType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForGeometry(firstBlock); } private static GeometryType find(int gtype)
			 {
				 if ( gtype < TYPES.length && gtype >= 0 ) { return TYPES[gtype]; } else { return GEOMETRY_INVALID; }
			 }
			 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.Impl.Store.Records.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset)
			 {
				 long firstBlock = valueBlocks[offset]; int gtype = getGeometryType( firstBlock ); int dimension = getDimension( firstBlock ); if ( isFloatPrecision( firstBlock ) ) { throw new UnsupportedOperationException( "Float precision is unsupported in Geometry properties" ); } if ( dimension > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } Neo4Net.Values.Storable.CoordinateReferenceSystem crs = Neo4Net.Values.Storable.CoordinateReferenceSystem.get(getCRSTable(firstBlock), getCRSCode(firstBlock)); return find(gtype).decode(crs, dimension, valueBlocks, offset);
			 }
			 public static long[] encodePoint( int keyId, Neo4Net.Values.Storable.CoordinateReferenceSystem crs, double[] coordinate )
			 {
				 if ( coordinate.length > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.GEOMETRY.intValue()) << idBits)); long gtypeBits = GeometryType.GEOMETRY_POINT.gtype << (idBits + 4); long dimensionBits = ((long) coordinate.length) << (idBits + 8); long crsTableIdBits = ((long) crs.getTable().getTableId()) << (idBits + 12); long crsCodeBits = ((long) crs.getCode()) << (idBits + 16); long[] data = new long[1 + coordinate.length]; data[0] = keyAndType | gtypeBits | dimensionBits | crsTableIdBits | crsCodeBits; for (int i = 0; i < coordinate.length; i++) { data[1 + i] = Double.doubleToLongBits(coordinate[i]); } return data;
			 }
			 public static byte[] encodePointArray( Neo4Net.Values.Storable.PointValue[] points )
			 {
				 int dimension = points[0].coordinate().length; Neo4Net.Values.Storable.CoordinateReferenceSystem crs = points[0].getCoordinateReferenceSystem(); for (int i = 1; i < points.length; i++)
				 {
					 if ( dimension != points[i].coordinate().length ) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent dimension. Point " + i + " has a different dimension."); } if (!crs.equals(points[i].getCoordinateReferenceSystem())) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent CRS. Point " + i + " has a different CRS."); }
				 }
				 double[] data = new double[points.length * dimension]; for ( int i = 0; i < data.length; i++ ) { data[i] = points[i / dimension].coordinate()[i % dimension]; } GeometryHeader geometryHeader = new GeometryHeader(GeometryType.GEOMETRY_POINT.gtype, dimension, crs); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.GEOMETRY_HEADER_SIZE); geometryHeader.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static Neo4Net.Values.Storable.ArrayValue decodeGeometryArray( GeometryHeader header, byte[] data ) { return find( header.geometryType ).decodeArray( header, data ); } private final int gtype);
		 }

		 public enum InnerEnum
		 {
			 GEOMETRY_INVALID,
			 GEOMETRY_POINT,
			 public static class GeometryHeader
			 {
				 private final int geometryType; private final int dimension; private final Neo4Net.Values.Storable.CoordinateReferenceSystem crs; private GeometryHeader( int geometryType, int dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem crs ) { this.geometryType = geometryType; this.dimension = dimension; this.crs = crs; } private GeometryHeader( int geometryType, int dimension, int crsTableId, int crsCode ) { this( geometryType, dimension, Neo4Net.Values.Storable.CoordinateReferenceSystem.get( crsTableId, crsCode ) ); } private void writeArrayHeaderTo( byte[] bytes ) { bytes[0] = ( byte ) PropertyType.GEOMETRY.intValue(); bytes[1] = (byte) geometryType; bytes[2] = (byte) dimension; bytes[3] = (byte) crs.getTable().getTableId(); bytes[4] = (byte)(crs.getCode() >> 8 & 0xFFL); bytes[5] = (byte)(crs.getCode() & 0xFFL); } static GeometryHeader fromArrayHeaderBytes(byte[] header) { int geometryType = Byte.toUnsignedInt(header[1]); int dimension = Byte.toUnsignedInt(header[2]); int crsTableId = Byte.toUnsignedInt(header[3]); int crsCode = (Byte.toUnsignedInt(header[4]) << 8) + Byte.toUnsignedInt(header[5]); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); } public static GeometryHeader fromArrayHeaderByteBuffer(ByteBuffer buffer) { int geometryType = Byte.toUnsignedInt(buffer.get()); int dimension = Byte.toUnsignedInt(buffer.get()); int crsTableId = Byte.toUnsignedInt(buffer.get()); int crsCode = (Byte.toUnsignedInt(buffer.get()) << 8) + Byte.toUnsignedInt(buffer.get()); return new GeometryHeader(geometryType, dimension, crsTableId, crsCode); }
			 }
			 private static final GeometryType[] TYPES = GeometryType.values,
			 static
			 {
				 for ( GeometryType geometryType : TYPES ) { all.put( geometryType.name, geometryType ); }
			 }
			 private static final long GEOMETRY_TYPE_MASK = 0x00000000F0000000L,
			 private static int getGeometryType( long firstBlock ) { return( int )( ( firstBlock & GEOMETRY_TYPE_MASK ) >> 28 ); } private static int getDimension( long firstBlock ) { return( int )( ( firstBlock & DIMENSION_MASK ) >> 32 ); } private static int getCRSTable( long firstBlock ) { return( int )( ( firstBlock & CRS_TABLE_MASK ) >> 36 ); } private static int getCRSCode( long firstBlock ) { return( int )( ( firstBlock & CRS_CODE_MASK ) >> 40 ); } private static boolean isFloatPrecision( long firstBlock ) { return( ( firstBlock & PRECISION_MASK ) >> 56 ) == 1; } private static int getMaxSupportedDimensions() { return PropertyType.getPayloadSizeLongs() - 1; } public static int calculateNumberOfBlocksUsed(long firstBlock) { GeometryType geometryType = find(getGeometryType(firstBlock)); return geometryType.calculateNumberOfBlocksUsedForGeometry(firstBlock); } private static GeometryType find(int gtype)
			 {
				 if ( gtype < TYPES.length && gtype >= 0 ) { return TYPES[gtype]; } else { return GEOMETRY_INVALID; }
			 }
			 public static Neo4Net.Values.Storable.Value decode( Neo4Net.Kernel.Impl.Store.Records.PropertyBlock block ) { return decode( block.getValueBlocks(), 0 ); } public static Neo4Net.Values.Storable.Value decode(long[] valueBlocks, int offset)
			 {
				 long firstBlock = valueBlocks[offset]; int gtype = getGeometryType( firstBlock ); int dimension = getDimension( firstBlock ); if ( isFloatPrecision( firstBlock ) ) { throw new UnsupportedOperationException( "Float precision is unsupported in Geometry properties" ); } if ( dimension > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } Neo4Net.Values.Storable.CoordinateReferenceSystem crs = Neo4Net.Values.Storable.CoordinateReferenceSystem.get(getCRSTable(firstBlock), getCRSCode(firstBlock)); return find(gtype).decode(crs, dimension, valueBlocks, offset);
			 }
			 public static long[] encodePoint( int keyId, Neo4Net.Values.Storable.CoordinateReferenceSystem crs, double[] coordinate )
			 {
				 if ( coordinate.length > GeometryType.getMaxSupportedDimensions() ) { throw new UnsupportedOperationException("Points with more than " + GeometryType.getMaxSupportedDimensions() + " dimensions are not supported"); } int idBits = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS; long keyAndType = keyId | (((long)(PropertyType.GEOMETRY.intValue()) << idBits)); long gtypeBits = GeometryType.GEOMETRY_POINT.gtype << (idBits + 4); long dimensionBits = ((long) coordinate.length) << (idBits + 8); long crsTableIdBits = ((long) crs.getTable().getTableId()) << (idBits + 12); long crsCodeBits = ((long) crs.getCode()) << (idBits + 16); long[] data = new long[1 + coordinate.length]; data[0] = keyAndType | gtypeBits | dimensionBits | crsTableIdBits | crsCodeBits; for (int i = 0; i < coordinate.length; i++) { data[1 + i] = Double.doubleToLongBits(coordinate[i]); } return data;
			 }
			 public static byte[] encodePointArray( Neo4Net.Values.Storable.PointValue[] points )
			 {
				 int dimension = points[0].coordinate().length; Neo4Net.Values.Storable.CoordinateReferenceSystem crs = points[0].getCoordinateReferenceSystem(); for (int i = 1; i < points.length; i++)
				 {
					 if ( dimension != points[i].coordinate().length ) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent dimension. Point " + i + " has a different dimension."); } if (!crs.equals(points[i].getCoordinateReferenceSystem())) { throw new IllegalArgumentException("Attempting to store array of points with inconsistent CRS. Point " + i + " has a different CRS."); }
				 }
				 double[] data = new double[points.length * dimension]; for ( int i = 0; i < data.length; i++ ) { data[i] = points[i / dimension].coordinate()[i % dimension]; } GeometryHeader geometryHeader = new GeometryHeader(GeometryType.GEOMETRY_POINT.gtype, dimension, crs); byte[] bytes = DynamicArrayStore.encodeFromNumbers(data, DynamicArrayStore.GEOMETRY_HEADER_SIZE); geometryHeader.writeArrayHeaderTo(bytes); return bytes;
			 }
			 public static Neo4Net.Values.Storable.ArrayValue decodeGeometryArray( GeometryHeader header, byte[] data ) { return find( header.geometryType ).decodeArray( header, data ); } private final int gtype
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private readonly string name;

		 GeometryType( int gtype, string name ) { this.gtype = gtype; this.name = name; } public abstract Neo4Net.Values.Storable.Value decode( Neo4Net.Values.Storable.CoordinateReferenceSystem crs, int dimension, long[] valueBlocks, int offset );

		 public abstract int calculateNumberOfBlocksUsedForGeometry( long firstBlock );

		 public abstract Neo4Net.Values.Storable.ArrayValue decodeArray( GeometryHeader header, sbyte[] data );

		 public static readonly GeometryType public int getGtype() { return gtype; } public String getName() { return name; } = new GeometryType("public int getGtype() { return gtype; } public String getName() { return name; }", InnerEnum.public int getGtype() { return gtype; } public String getName() { return name; });

		public static IList<GeometryType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static GeometryType valueOf( string name )
		{
			foreach ( GeometryType enumInstance in GeometryType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}
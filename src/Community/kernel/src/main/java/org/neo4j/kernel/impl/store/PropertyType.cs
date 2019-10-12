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
namespace Neo4Net.Kernel.impl.store
{

	using PropertyRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyRecordFormat;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Defines valid property types.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnnecessaryBoxing") public enum PropertyType
	public abstract class PropertyType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BOOL(1) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.booleanValue(getValue(block.getSingleValueLong())); } private boolean getValue(long propBlock) { return(propBlock & 0x1) == 1; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BYTE(2) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.byteValue(block.getSingleValueByte()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SHORT(3) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.shortValue(block.getSingleValueShort()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CHAR(4) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.charValue((char) block.getSingleValueShort()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INT(5) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.intValue(block.getSingleValueInt()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LONG(6) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { long firstBlock = block.getSingleValueBlock(); long value = valueIsInlined(firstBlock) ? (block.getSingleValueLong() >>> 1) : block.getValueBlocks()[1]; return org.neo4j.values.storable.Values.longValue(value); } private boolean valueIsInlined(long firstBlock) { return(firstBlock & 0x10000000L) > 0; } public int calculateNumberOfBlocksUsed(long firstBlock) { return valueIsInlined(firstBlock) ? 1 : 2; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FLOAT(7) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.floatValue(Float.intBitsToFloat(block.getSingleValueInt())); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DOUBLE(8) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.doubleValue(Double.longBitsToDouble(block.getValueBlocks()[1])); } public int calculateNumberOfBlocksUsed(long firstBlock) { return 2; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       STRING(9) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return org.neo4j.values.storable.Values.stringValue(store.getStringFor(block)); } public byte[] readDynamicRecordHeader(byte[] recordBytes) { return EMPTY_BYTE_ARRAY; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ARRAY(10) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return store.getArrayFor(block); } public byte[] readDynamicRecordHeader(byte[] recordBytes) { byte itemType = recordBytes[0]; if(itemType == STRING.byteValue()) { return headOf(recordBytes, DynamicArrayStore.STRING_HEADER_SIZE); } else if(itemType <= DOUBLE.byteValue()) { return headOf(recordBytes, DynamicArrayStore.NUMBER_HEADER_SIZE); } else if(itemType == GEOMETRY.byteValue()) { return headOf(recordBytes, DynamicArrayStore.GEOMETRY_HEADER_SIZE); } else if(itemType == TEMPORAL.byteValue()) { return headOf(recordBytes, DynamicArrayStore.TEMPORAL_HEADER_SIZE); } throw new IllegalArgumentException("Unknown array type " + itemType); } private byte[] headOf(byte[] bytes, int length) { return java.util.Arrays.copyOf(bytes, length); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SHORT_STRING(11) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return LongerShortString.decode(block); } public int calculateNumberOfBlocksUsed(long firstBlock) { return LongerShortString.calculateNumberOfBlocksUsed(firstBlock); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SHORT_ARRAY(12) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return ShortArray.decode(block); } public int calculateNumberOfBlocksUsed(long firstBlock) { return ShortArray.calculateNumberOfBlocksUsed(firstBlock); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GEOMETRY(13) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return GeometryType.decode(block); } public int calculateNumberOfBlocksUsed(long firstBlock) { return GeometryType.calculateNumberOfBlocksUsed(firstBlock); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TEMPORAL(14) { public org.neo4j.values.storable.Value value(org.neo4j.kernel.impl.store.record.PropertyBlock block, PropertyStore store) { return TemporalType.decode(block); } public int calculateNumberOfBlocksUsed(long firstBlock) { return TemporalType.calculateNumberOfBlocksUsed(firstBlock); } };

		 private static readonly IList<PropertyType> valueList = new List<PropertyType>();

		 static PropertyType()
		 {
			 valueList.Add( BOOL );
			 valueList.Add( BYTE );
			 valueList.Add( SHORT );
			 valueList.Add( CHAR );
			 valueList.Add( INT );
			 valueList.Add( LONG );
			 valueList.Add( FLOAT );
			 valueList.Add( DOUBLE );
			 valueList.Add( STRING );
			 valueList.Add( ARRAY );
			 valueList.Add( SHORT_STRING );
			 valueList.Add( SHORT_ARRAY );
			 valueList.Add( GEOMETRY );
			 valueList.Add( TEMPORAL );
		 }

		 public enum InnerEnum
		 {
			 BOOL,
			 BYTE,
			 SHORT,
			 CHAR,
			 INT,
			 LONG,
			 FLOAT,
			 DOUBLE,
			 STRING,
			 ARRAY,
			 SHORT_STRING,
			 SHORT_ARRAY,
			 GEOMETRY,
			 TEMPORAL
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private PropertyType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static readonly sbyte[] EMPTY_BYTE_ARRAY = new sbyte[0];
		 public const int BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING = -1;

		 // TODO In wait of a better place
		 private static int payloadSize = Neo4Net.Kernel.impl.store.format.standard.PropertyRecordFormat.DEFAULT_PAYLOAD_SIZE;

		 private readonly int type;

		 PropertyType( int type ) { this.type = type; } public int intValue() { return type; } public sbyte byteValue() { return(sbyte) type; } public abstract Neo4Net.Values.Storable.Value value(Neo4Net.Kernel.impl.store.record.PropertyBlock block, PropertyStore store);

		 public static readonly PropertyType public static PropertyType getPropertyTypeOrNull( long propBlock )
		 {
			 int type = typeIdentifier( propBlock ); switch ( type ) { case 1: return BOOL; case 2: return BYTE; case 3: return SHORT; case 4: return CHAR; case 5: return INT; case 6: return LONG; case 7: return FLOAT; case 8: return DOUBLE; case 9: return STRING; case 10: return ARRAY; case 11: return SHORT_STRING; case 12: return SHORT_ARRAY; case 13: return GEOMETRY; case 14: return TEMPORAL; default: return null; }
		 }
		 private static int typeIdentifier( long propBlock ) { return( int )( ( propBlock & 0x000000000F000000L ) >> 24 ); } public static PropertyType getPropertyTypeOrThrow( long propBlock )
		 {
			 PropertyType type = getPropertyTypeOrNull( propBlock ); if ( type == null ) { throw new InvalidRecordException( "Unknown property type for type " + typeIdentifier( propBlock ) ); } return type;
		 }
		 public static int getPayloadSize() { return payloadSize; } public static int getPayloadSizeLongs() { return payloadSize >> > 3; } public int calculateNumberOfBlocksUsed(long firstBlock) { return 1; } public byte[] readDynamicRecordHeader(byte[] recordBytes) { throw new UnsupportedOperationException(); } = new PropertyType("public static PropertyType getPropertyTypeOrNull(long propBlock) { int type = typeIdentifier(propBlock); switch(type) { case 1: return BOOL; case 2: return BYTE; case 3: return SHORT; case 4: return CHAR; case 5: return INT; case 6: return LONG; case 7: return FLOAT; case 8: return DOUBLE; case 9: return STRING; case 10: return ARRAY; case 11: return SHORT_STRING; case 12: return SHORT_ARRAY; case 13: return GEOMETRY; case 14: return TEMPORAL; default: return null; } } private static int typeIdentifier(long propBlock) { return(int)((propBlock & 0x000000000F000000L) >> 24); } public static PropertyType getPropertyTypeOrThrow(long propBlock) { PropertyType type = getPropertyTypeOrNull(propBlock); if(type == null) { throw new InvalidRecordException("Unknown property type for type " + typeIdentifier(propBlock)); } return type; } public static int getPayloadSize() { return payloadSize; } public static int getPayloadSizeLongs() { return payloadSize >>> 3; } public int calculateNumberOfBlocksUsed(long firstBlock) { return 1; } public byte[] readDynamicRecordHeader(byte[] recordBytes) { throw new UnsupportedOperationException(); }", InnerEnum.public static PropertyType getPropertyTypeOrNull(long propBlock)
		 {
			 int type = typeIdentifier( propBlock ); switch ( type ) { case 1: return BOOL; case 2: return BYTE; case 3: return SHORT; case 4: return CHAR; case 5: return INT; case 6: return LONG; case 7: return FLOAT; case 8: return DOUBLE; case 9: return STRING; case 10: return ARRAY; case 11: return SHORT_STRING; case 12: return SHORT_ARRAY; case 13: return GEOMETRY; case 14: return TEMPORAL; default: return null; }
		 }
		 private static int typeIdentifier( long propBlock ) { return( int )( ( propBlock & 0x000000000F000000L ) >> 24 ); } public static PropertyType getPropertyTypeOrThrow( long propBlock )
		 {
			 PropertyType type = getPropertyTypeOrNull( propBlock ); if ( type == null ) { throw new InvalidRecordException( "Unknown property type for type " + typeIdentifier( propBlock ) ); } return type;
		 }
		 public static int getPayloadSize() { return payloadSize; } public static int getPayloadSizeLongs() { return payloadSize >> > 3; } public int calculateNumberOfBlocksUsed(long firstBlock) { return 1; } public byte[] readDynamicRecordHeader(byte[] recordBytes) { throw new UnsupportedOperationException(); });

		public static IList<PropertyType> values()
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

		public static PropertyType valueOf( string name )
		{
			foreach ( PropertyType enumInstance in PropertyType.valueList )
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
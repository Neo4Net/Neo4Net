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
namespace Neo4Net.Kernel.Impl.Store.Records
{

	using PropertyKeyValue = Neo4Net.Kernel.api.properties.PropertyKeyValue;
	using Value = Neo4Net.Values.Storable.Value;

	public class PropertyBlock : ICloneable
	{
		 /// <summary>
		 /// Size of one property block in a property record. One property may be composed by one or more property blocks
		 /// and one property record contains several property blocks.
		 /// </summary>
		 public static readonly int PropertyBlockSize = Long.BYTES;

		 private const long KEY_BITMASK = 0xFFFFFFL;

		 private const int MAX_ARRAY_TOSTRING_SIZE = 4;
		 private IList<DynamicRecord> _valueRecords;
		 private long[] _valueBlocks;

		 public virtual PropertyType Type
		 {
			 get
			 {
				  return GetType( false );
			 }
		 }

		 public virtual PropertyType ForceGetType()
		 {
			  return GetType( true );
		 }

		 private PropertyType getType( bool force )
		 {
			  return _valueBlocks == null ? null : force ? PropertyType.getPropertyTypeOrNull( _valueBlocks[0] ) : PropertyType.getPropertyTypeOrThrow( _valueBlocks[0] );
		 }

		 public virtual int KeyIndexId
		 {
			 get
			 {
				  return KeyIndexId( _valueBlocks[0] );
			 }
			 set
			 {
				  _valueBlocks[0] &= ~KEY_BITMASK;
				  _valueBlocks[0] |= value;
			 }
		 }


		 public virtual long SingleBlock
		 {
			 set
			 {
				  _valueBlocks = new long[1];
				  _valueBlocks[0] = value;
				  if ( _valueRecords != null )
				  {
						_valueRecords.Clear();
				  }
			 }
		 }

		 public virtual void AddValueRecord( DynamicRecord record )
		 {
			  if ( _valueRecords == null )
			  {
					_valueRecords = new LinkedList<DynamicRecord>();
			  }
			  _valueRecords.Add( record );
		 }

		 public virtual IList<DynamicRecord> ValueRecords
		 {
			 set
			 {
				  Debug.Assert( this._valueRecords == null || this._valueRecords.Count == 0, this._valueRecords.ToString() );
				  this._valueRecords = value;
			 }
			 get
			 {
				  return _valueRecords != null ? _valueRecords : Collections.emptyList();
			 }
		 }


		 public virtual long SingleValueBlock
		 {
			 get
			 {
				  return _valueBlocks[0];
			 }
		 }

		 /// <summary>
		 /// use this for references to the dynamic stores
		 /// </summary>
		 public virtual long SingleValueLong
		 {
			 get
			 {
				  return FetchLong( _valueBlocks[0] );
			 }
		 }

		 public virtual int SingleValueInt
		 {
			 get
			 {
				  return FetchInt( _valueBlocks[0] );
			 }
		 }

		 public virtual short SingleValueShort
		 {
			 get
			 {
				  return FetchShort( _valueBlocks[0] );
			 }
		 }

		 public virtual sbyte SingleValueByte
		 {
			 get
			 {
				  return FetchByte( _valueBlocks[0] );
			 }
		 }

		 public virtual long[] ValueBlocks
		 {
			 get
			 {
				  return _valueBlocks;
			 }
			 set
			 {
				  int expectedPayloadSize = PropertyType.PayloadSizeLongs;
				  Debug.Assert( value == null || value.Length <= expectedPayloadSize, "I was given an array of size " + value.Length + ", but I wanted it to be " + expectedPayloadSize );
				  this._valueBlocks = value;
				  if ( _valueRecords != null )
				  {
						_valueRecords.Clear();
				  }
			 }
		 }

		 public virtual bool Light
		 {
			 get
			 {
				  return _valueRecords == null || _valueRecords.Count == 0;
			 }
		 }


		 /// <summary>
		 /// A property block can take a variable size of bytes in a property record.
		 /// This method returns the size of this block in bytes, including the header
		 /// size. This does not include dynamic records.
		 /// </summary>
		 /// <returns> The size of this block in bytes, including the header. </returns>
		 public virtual int Size
		 {
			 get
			 {
				  // Currently each block is a multiple of 8 in size
				  return _valueBlocks == null ? 0 : _valueBlocks.Length * PropertyBlockSize;
			 }
		 }

		 public override string ToString()
		 {
			  StringBuilder result = new StringBuilder( "PropertyBlock[" );
			  PropertyType type = Type;
			  if ( _valueBlocks != null )
			  {
					result.Append( "blocks=" ).Append( _valueBlocks.Length ).Append( ',' );
			  }
			  result.Append( type == null ? "<unknown type>" : type.name() ).Append(',');
			  result.Append( "key=" ).Append( _valueBlocks == null ? "?" : Convert.ToString( KeyIndexId ) );
			  if ( type != null )
			  {
					switch ( type.innerEnumValue )
					{
					case PropertyType.InnerEnum.STRING:
					case PropertyType.InnerEnum.ARRAY:
						 result.Append( ",firstDynamic=" ).Append( SingleValueLong );
						 break;
					default:
						 object value = type.value( this, null ).asObject();
						 if ( value != null && value.GetType().IsArray )
						 {
							  int length = Array.getLength( value );
							  StringBuilder buf = ( new StringBuilder( value.GetType().GetElementType().SimpleName ) ).Append("[");
							  for ( int i = 0; i < length && i <= MAX_ARRAY_TOSTRING_SIZE; i++ )
							  {
									if ( i != 0 )
									{
										 buf.Append( ',' );
									}
									buf.Append( Array.get( value, i ) );
							  }
							  if ( length > MAX_ARRAY_TOSTRING_SIZE )
							  {
									buf.Append( ",..." );
							  }
							  value = buf.Append( ']' );
						 }
						 result.Append( ",value=" ).Append( value );
						 break;
					}
			  }
			  if ( !Light )
			  {
					result.Append( ",ValueRecords[" );
					IEnumerator<DynamicRecord> recIt = _valueRecords.GetEnumerator();
					while ( recIt.MoveNext() )
					{
						 result.Append( recIt.Current );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( recIt.hasNext() )
						 {
							  result.Append( ',' );
						 }
					}
					result.Append( ']' );
			  }
			  result.Append( ']' );
			  return result.ToString();
		 }

		 public override PropertyBlock Clone()
		 {
			  PropertyBlock result = new PropertyBlock();
			  if ( _valueBlocks != null )
			  {
					result._valueBlocks = _valueBlocks.Clone();
			  }
			  if ( _valueRecords != null )
			  {
					foreach ( DynamicRecord valueRecord in _valueRecords )
					{
						 result.AddValueRecord( valueRecord.Clone() );
					}
			  }
			  return result;
		 }

		 public virtual bool HasSameContentsAs( PropertyBlock other )
		 {
			  // Assumption (which happens to be true) that if a heavy (long string/array) property
			  // changes it will get another id, making the valueBlocks values differ.
			  return Arrays.Equals( _valueBlocks, other._valueBlocks );
		 }

		 public virtual Value NewPropertyValue( PropertyStore propertyStore )
		 {
			  return Type.value( this, propertyStore );
		 }

		 public virtual PropertyKeyValue NewPropertyKeyValue( PropertyStore propertyStore )
		 {
			  int propertyKeyId = KeyIndexId;
			  return new PropertyKeyValue( propertyKeyId, Type.value( this, propertyStore ) );
		 }

		 public static int KeyIndexId( long valueBlock )
		 {
			  // [][][][][][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			  return ( int )( valueBlock & KEY_BITMASK );
		 }

		 public static long FetchLong( long valueBlock )
		 {
			  return ( int )( ( uint )( valueBlock & 0xFFFFFFFFF0000000L ) >> 28 );
		 }

		 public static int FetchInt( long valueBlock )
		 {
			  return ( int )( ( long )( ( ulong )( valueBlock & 0x0FFFFFFFF0000000L ) >> 28 ) );
		 }

		 public static short FetchShort( long valueBlock )
		 {
			  return ( short )( ( long )( ( ulong )( valueBlock & 0x00000FFFF0000000L ) >> 28 ) );
		 }

		 public static sbyte FetchByte( long valueBlock )
		 {
			  return ( sbyte )( ( long )( ( ulong )( valueBlock & 0x0000000FF0000000L ) >> 28 ) );
		 }

		 public static bool ValueIsInlined( long valueBlock )
		 {
			  // [][][][][   i,tttt][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			  return ( valueBlock & 0x10000000L ) > 0;
		 }
	}

}
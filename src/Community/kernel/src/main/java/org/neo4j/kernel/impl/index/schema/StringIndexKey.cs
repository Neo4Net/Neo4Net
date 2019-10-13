using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.@internal.gbptree;
	using UTF8 = Neo4Net.@string.UTF8;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="string"/>,
	/// or rather any string that <seealso cref="GBPTree"/> can handle.
	/// </summary>
	internal class StringIndexKey : NativeIndexSingleValueKey<StringIndexKey>
	{
		 private bool _ignoreLength;

		 // UTF-8 bytes, grows on demand. Actual length is dictated by bytesLength field.
		 internal sbyte[] Bytes;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal int BytesLengthConflict;
		 // Set to true when the internal byte[] have been handed out to an UTF8Value, so that the next call to setBytesLength
		 // will be forced to allocate a new array. The byte[] isn't cleared with null since this key still logically contains those bytes.
		 private bool _bytesDereferenced;

		 internal virtual int Size()
		 {
			  return ENTITY_ID_SIZE + BytesLengthConflict;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !Values.isTextValue( value ) )
			  {
					throw new System.ArgumentException( "Key layout does only support strings, tried to create key from " + value );
			  }
			  return value;
		 }

		 internal override void Initialize( long entityId )
		 {
			  base.Initialize( entityId );
			  _ignoreLength = false;
		 }

		 public override Value AsValue()
		 {
			  if ( Bytes == null )
			  {
					return Values.NO_VALUE;
			  }

			  // Dereference our bytes so that we won't overwrite it on next read
			  _bytesDereferenced = true;
			  return Values.utf8Value( Bytes, 0, BytesLengthConflict );
		 }

		 internal override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  Bytes = null;
		 }

		 internal override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  Bytes = null;
		 }

		 internal virtual void InitAsPrefixLow( TextValue prefix )
		 {
			  prefix.WriteTo( this );
			  Initialize( long.MinValue );
			  // Don't set ignoreLength = true here since the "low" a.k.a. left side of the range should care about length.
			  // This will make the prefix lower than those that matches the prefix (their length is >= that of the prefix)
		 }

		 internal virtual void InitAsPrefixHigh( TextValue prefix )
		 {
			  prefix.WriteTo( this );
			  Initialize( long.MaxValue );
			  _ignoreLength = true;
		 }

		 private bool Highest
		 {
			 get
			 {
				  return CompareId && EntityId == long.MaxValue && Bytes == null;
			 }
		 }

		 /// <summary>
		 /// Compares the value of this key to that of another key.
		 /// This method is expected to be called in scenarios where inconsistent reads may happen (and later retried).
		 /// </summary>
		 /// <param name="other"> the <seealso cref="StringIndexKey"/> to compare to. </param>
		 /// <returns> comparison against the {@code other} <seealso cref="StringIndexKey"/>. </returns>
		 internal override int CompareValueTo( StringIndexKey other )
		 {
			  if ( Bytes != other.Bytes )
			  {
					if ( Bytes == null )
					{
						 return Highest ? 1 : -1;
					}
					if ( other.Bytes == null )
					{
						 return other.Highest ? -1 : 1;
					}
			  }
			  else
			  {
					return 0;
			  }

			  return LexicographicalUnsignedByteArrayCompare( Bytes, BytesLengthConflict, other.Bytes, other.BytesLengthConflict, _ignoreLength | other._ignoreLength );
		 }

		 /// <summary>
		 /// Compare arrays byte by byte, first byte is most significant.
		 /// If arrays have different length and the longer array share all bytes with the shorter array, then the longer one is larger,
		 /// unless ignoreLength is set to true in which case they are considered equal.
		 /// </summary>
		 internal static int LexicographicalUnsignedByteArrayCompare( sbyte[] a, int aLength, sbyte[] b, int bLength, bool ignoreLength )
		 {
			  Debug.Assert( a != null && b != null, "Null arrays not supported." );

			  if ( a == b && aLength == bLength )
			  {
					return 0;
			  }

			  int length = Math.Min( aLength, bLength );
			  for ( int i = 0; i < length; i++ )
			  {
					int compare = Short.compare( ( short )( a[i] & 0xFF ), ( short )( b[i] & 0xFF ) );
					if ( compare != 0 )
					{
						 return compare;
					}
			  }

			  return ignoreLength ? 0 : Integer.compare( aLength, bLength );
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,bytes=%s", AsValue(), EntityId, Bytes == null ? "null" : Arrays.ToString(Arrays.copyOf(Bytes, BytesLengthConflict)) );
		 }

		 public override void WriteString( string value )
		 {
			  Bytes = UTF8.encode( value );
			  BytesLengthConflict = Bytes.Length;
			  _bytesDereferenced = false;
		 }

		 public override void WriteString( char value )
		 {
			  WriteString( value.ToString() );
		 }

		 public override void WriteUTF8( sbyte[] bytes, int offset, int length )
		 {
			  this.Bytes = bytes;
			  BytesLengthConflict = length;
			  _bytesDereferenced = true;
		 }

		 internal virtual void CopyFrom( StringIndexKey key )
		 {
			  EntityId = key.EntityId;
			  CompareId = key.CompareId;
			  CopyValueFrom( key, key.BytesLengthConflict );
		 }

		 internal virtual void CopyValueFrom( StringIndexKey key, int targetLength )
		 {
			  BytesLength = targetLength;
			  Array.Copy( key.Bytes, 0, Bytes, 0, targetLength );
		 }

		 /// <summary>
		 /// Ensures that the internal byte[] is long enough, or longer than the given {@code length}.
		 /// Also sets the internal {@code bytesLength} field to the given {@code length} so that interactions with the byte[]
		 /// from this point on will use that for length, instead of the length of the byte[].
		 /// </summary>
		 /// <param name="length"> minimum length that the internal byte[] needs to be. </param>
		 internal virtual int BytesLength
		 {
			 set
			 {
				  if ( _bytesDereferenced || Bytes == null || Bytes.Length < value )
				  {
						_bytesDereferenced = false;
   
						// allocate a bit more than required so that there's a higher chance that this byte[] instance
						// can be used for more keys than just this one
						Bytes = new sbyte[value + value / 2];
				  }
				  BytesLengthConflict = value;
			 }
		 }
	}

}
using System;

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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using UTF8StringValue = Neo4Net.Values.Storable.UTF8StringValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.SIZE_STRING_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.setCursorException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.toNonNegativeShortExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.StringIndexKey.lexicographicalUnsignedByteArrayCompare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	internal class TextType : Type
	{
		 // in-memory marker in long2 for TEXT value type, i.e. 1:CHAR, 0:STRING
		 internal const long CHAR_TYPE_STATE_MARKER = 0x2;
		 // persistent marker in 2B length, 1:CHAR, 0:STRING
		 internal const int CHAR_TYPE_LENGTH_MARKER = 0x8000;

		 // Affected key state:
		 // long0 (length)
		 // long1 (bytesDereferenced)
		 // long2 (ignoreLength|charValueType)
		 // long3 (isHighest)
		 // byteArray

		 internal TextType( sbyte typeId ) : base( ValueGroup.TEXT, typeId, Values.of( "" ), Values.of( "" ) )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return TextKeySize( state.Long0 );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  // don't copy long1 since it's instance-local (bytesDereferenced)
			  to.Long2 = from.Long2;
			  to.Long3 = from.Long3;
			  SetBytesLength( to, ( int ) from.Long0 );
			  Array.Copy( from.ByteArray, 0, to.ByteArray, 0, ( int ) from.Long0 );
		 }

		 internal override void MinimalSplitter( GenericKey left, GenericKey right, GenericKey into )
		 {
			  int length = 0;
			  if ( left.TypeConflict == Types.Text )
			  {
					length = StringLayout.MinimalLengthFromRightNeededToDifferentiateFromLeft( left.ByteArray, ( int ) left.Long0, right.ByteArray, ( int ) right.Long0 );
			  }
			  into.WriteUTF8( right.ByteArray, 0, length );
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  // There's a difference between composing a single text value and a array text values
			  // and there's therefore no common "raw" variant of it
			  if ( state.ByteArray == null )
			  {
					return NO_VALUE;
			  }

			  if ( IsCharValueType( state.Long2 ) )
			  {
					// This is a char value.
					return Values.charValue( TextAsChar( state.ByteArray ) );
			  }

			  // This is a string value
			  state.Long1 = TRUE;
			  return Values.utf8Value( state.ByteArray, 0, ( int ) state.Long0 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.ByteArray, left.Long0, left.Long2, left.Long3, right.ByteArray, right.Long0, right.Long2, right.Long3 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  Put( cursor, state.ByteArray, state.Long0, state.Long2 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return Read( cursor, size, into );
		 }

		 internal static int TextKeySize( long long0 )
		 {
			  return SIZE_STRING_LENGTH + ( int ) long0; // bytesLength
		 }

		 internal static int Compare( sbyte[] thisByteArray, long thisLong0, long thisLong2, long thisLong3, sbyte[] thatByteArray, long thatLong0, long thatLong2, long thatLong3 )
		 {
			  if ( thisByteArray != thatByteArray )
			  {
					if ( IsHighestText( thisLong3 ) || IsHighestText( thatLong3 ) )
					{
						 return Boolean.compare( IsHighestText( thisLong3 ), IsHighestText( thatLong3 ) );
					}
					if ( thisByteArray == null )
					{
						 return -1;
					}
					if ( thatByteArray == null )
					{
						 return 1;
					}
			  }
			  else
			  {
					return 0;
			  }

			  return lexicographicalUnsignedByteArrayCompare( thisByteArray, ( int ) thisLong0, thatByteArray, ( int ) thatLong0, BooleanOf( thisLong2 ) | BooleanOf( thatLong2 ) );
		 }

		 internal static void Put( PageCursor cursor, sbyte[] byteArray, long long0, long long2 )
		 {
			  // There are two variants of a text value, one is string, the other is char. Both are the same ValueGroup, i.e. TEXT
			  // and should be treated the same, it's just that we need to know if it's a char so that we can materialize a CharValue for chars.
			  // We put a special marker for char values, knowing that a char is exactly 2 bytes in storage.
			  // This can be picked up by reader and set the right flag in state so that a CharValue can be materialized.
			  short length = toNonNegativeShortExact( long0 );
			  cursor.PutShort( IsCharValueType( long2 ) ? ( short )( length | CHAR_TYPE_LENGTH_MARKER ) : length );
			  cursor.PutBytes( byteArray, 0, length );
		 }

		 internal static bool Read( PageCursor cursor, int maxSize, GenericKey into )
		 {
			  // For performance reasons cannot be redirected to writeString, due to byte[] reuse
			  short rawLength = cursor.Short;
			  bool isCharType = ( rawLength & CHAR_TYPE_LENGTH_MARKER ) != 0;
			  short bytesLength = ( short )( rawLength & ~CHAR_TYPE_LENGTH_MARKER );
			  if ( bytesLength < 0 || bytesLength > maxSize )
			  {
					setCursorException( cursor, "non-valid bytes length for text, " + bytesLength );
					return false;
			  }

			  // Remember this fact, i.e. set the flag in this state
			  SetCharType( into, isCharType );
			  SetBytesLength( into, bytesLength );
			  cursor.GetBytes( into.ByteArray, 0, bytesLength );
			  return true;
		 }

		 internal static void SetCharType( GenericKey into, bool isCharType )
		 {
			  if ( isCharType )
			  {
					into.Long2 |= CHAR_TYPE_STATE_MARKER;
			  }
			  else
			  {
					into.Long2 &= ~CHAR_TYPE_STATE_MARKER;
			  }
		 }

		 private static bool IsHighestText( long long3 )
		 {
			  return long3 == TRUE;
		 }

		 internal static bool IsCharValueType( long long2 )
		 {
			  return BooleanOf( long2 >> 1 );
		 }

		 internal virtual void Write( GenericKey state, sbyte[] bytes, bool isCharType )
		 {
			  state.ByteArray = bytes;
			  state.Long0 = bytes.Length;
			  SetCharType( state, isCharType );
		 }

		 internal override void InitializeAsHighest( GenericKey state )
		 {
			  base.InitializeAsHighest( state );
			  state.Long3 = TRUE;
		 }

		 internal static char TextAsChar( sbyte[] byteArray )
		 {
			  long codePoint = ( new UTF8StringValue.CodePointCursor( byteArray, 0 ) ).nextCodePoint();
			  if ( ( codePoint & ~0xFFFF ) != 0 )
			  {
					throw new System.InvalidOperationException( "Char value seems to be bigger than what a char can hold " + codePoint );
			  }
			  return ( char ) codePoint;
		 }

		 private static void SetBytesLength( GenericKey state, int length )
		 {
			  if ( BooleanOf( state.Long1 ) || state.ByteArray == null || state.ByteArray.Length < length )
			  {
					state.Long1 = FALSE;

					// allocate a bit more than required so that there's a higher chance that this byte[] instance
					// can be used for more keys than just this one
					state.ByteArray = new sbyte[length + length / 2];
			  }
			  state.Long0 = length;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long2=" + state.Long2 );
			  joiner.add( "long3=" + state.Long3 );
			  joiner.add( "byteArray=" + Arrays.ToString( state.ByteArray ) );
		 }
	}

}
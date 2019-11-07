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
	using UTF8 = Neo4Net.Strings.UTF8;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericKey.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericKey.SIZE_ARRAY_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericKey.setCursorException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericKey.toNonNegativeShortExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.TextType.CHAR_TYPE_LENGTH_MARKER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.TextType.isCharValueType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.TextType.setCharType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.TextType.textAsChar;

	internal class TextArrayType : AbstractArrayType<string>
	{
		 // Affected key state:
		 // long0Array (length)
		 // long1 (bytesDereferenced)
		 // long2 (ignoreLength|charValueType)
		 // long3 (isHighest)
		 // byteArrayArray

		 internal TextArrayType( sbyte typeId ) : base( ValueGroup.TEXT_ARRAY, typeId, ( o1, o2, i ) -> TextType.Compare( o1.byteArrayArray[i], o1.long0Array[i], o1.long2, o1.long3, o2.byteArrayArray[i], o2.long0Array[i], o2.long2, o2.long3 ), ( k, i ) -> AsValueRaw( k.byteArrayArray[i], k.long0Array[i] ), ( c, k, i ) -> TextType.put( c, k.byteArrayArray[i], k.long0Array[i], 0 ), null, string[]::new, Neo4Net.values.storable.ValueWriter_ArrayType.String )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  int stringArraySize = 0;
			  for ( int i = 0; i < state.ArrayLength; i++ )
			  {
					stringArraySize += TextType.TextKeySize( state.Long0Array[i] );
			  }
			  return SIZE_ARRAY_LENGTH + stringArraySize;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  to.Long1 = FALSE;
			  to.Long2 = from.Long2;
			  to.Long3 = from.Long3;
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
			  for ( int i = 0; i < length; i++ )
			  {
					short targetLength = ( short ) from.Long0Array[i];
					to.ByteArrayArray[i] = EnsureBigEnough( to.ByteArrayArray[i], targetLength );
					Array.Copy( from.ByteArrayArray[i], 0, to.ByteArrayArray[i], 0, targetLength );
			  }
		 }

		 internal override void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );
			  key.ByteArrayArray = EnsureBigEnough( key.ByteArrayArray, length );
			  // long1 (bytesDereferenced) - Not needed because we never leak bytes from string array
			  // long2 (ignoreLength) - Not needed because kept on 'global' level for full array
			  // long3 (isHighest) - Not needed because kept on 'global' level for full array
			  setCharType( key, arrayType == Neo4Net.Values.Storable.ValueWriter_ArrayType.Char );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  short typeMarker = ( short )( isCharValueType( state.Long2 ) ? CHAR_TYPE_LENGTH_MARKER : 0 );
			  PutArrayHeader( cursor, ( short )( toNonNegativeShortExact( state.ArrayLength ) | typeMarker ) );
			  PutArrayItems( cursor, state, ArrayElementWriter );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  short rawLength = cursor.Short;
			  bool isCharType = ( rawLength & CHAR_TYPE_LENGTH_MARKER ) != 0;
			  short length = ( short )( rawLength & ~CHAR_TYPE_LENGTH_MARKER );
			  if ( !SetArrayLengthWhenReading( into, cursor, length ) )
			  {
					return false;
			  }
			  into.BeginArray( into.ArrayLength, isCharType ? Neo4Net.Values.Storable.ValueWriter_ArrayType.Char : Neo4Net.Values.Storable.ValueWriter_ArrayType.String );
			  for ( int i = 0; i < into.ArrayLength; i++ )
			  {
					short bytesLength = cursor.Short;
					if ( bytesLength < 0 || bytesLength > size )
					{
						 setCursorException( cursor, "non-valid bytes length, " + bytesLength );
						 return false;
					}

					into.ByteArrayArray[i] = EnsureBigEnough( into.ByteArrayArray[i], bytesLength );
					into.Long0Array[i] = bytesLength;
					cursor.GetBytes( into.ByteArrayArray[i], 0, bytesLength );
			  }
			  into.EndArray();
			  return true;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  // no need to set bytes dereferenced because byte[][] owned by this class will be deserialized into String objects.
			  if ( isCharValueType( state.Long2 ) )
			  {
					// this is a char[]
					return CharArrayAsValue( state );
			  }
			  // this is a String[]
			  return base.AsValue( state );
		 }

		 private Value CharArrayAsValue( GenericKey state )
		 {
			  char[] chars = new char[state.ArrayLength];
			  for ( int i = 0; i < state.ArrayLength; i++ )
			  {
					chars[i] = textAsChar( state.ByteArrayArray[i] );
			  }
			  return Values.charArray( chars );
		 }

		 internal static string AsValueRaw( sbyte[] byteArray, long long0 )
		 {
			  return byteArray == null ? null : UTF8.decode( byteArray, 0, ( int ) long0 );
		 }

		 internal virtual void Write( GenericKey state, int offset, sbyte[] bytes )
		 {
			  state.ByteArrayArray[offset] = bytes;
			  state.Long0Array[offset] = bytes.Length;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long2=" + state.Long2 );
			  joiner.add( "long3=" + state.Long3 );
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  joiner.add( "byteArrayArray=" + Arrays.deepToString( state.ByteArrayArray ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}
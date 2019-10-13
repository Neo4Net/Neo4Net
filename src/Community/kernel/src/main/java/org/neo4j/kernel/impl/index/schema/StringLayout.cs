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
	using Neo4Net.Index.@internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.StringIndexKey.ENTITY_ID_SIZE;

	/// <summary>
	/// <seealso cref="Layout"/> for strings.
	/// </summary>
	internal class StringLayout : IndexLayout<StringIndexKey, NativeIndexValue>
	{
		 private const int NO_ENTITY_ID = -1;

		 internal StringLayout() : base("USI", 0, 1)
		 {
		 }

		 public override StringIndexKey NewKey()
		 {
			  return new StringIndexKey();
		 }

		 public override StringIndexKey CopyKey( StringIndexKey key, StringIndexKey into )
		 {
			  into.CopyFrom( key );
			  return into;
		 }

		 public override int KeySize( StringIndexKey key )
		 {
			  return key.Size();
		 }

		 public override void WriteKey( PageCursor cursor, StringIndexKey key )
		 {
			  cursor.PutLong( key.EntityId );
			  cursor.PutBytes( key.Bytes, 0, key.BytesLengthConflict );
		 }

		 public override void ReadKey( PageCursor cursor, StringIndexKey into, int keySize )
		 {
			  if ( keySize < ENTITY_ID_SIZE )
			  {
					into.EntityId = long.MinValue;
					into.BytesLength = 0;
					cursor.CursorException = format( "Reading string index key with an unexpected keySize:%d", keySize );
					return;
			  }
			  into.EntityId = cursor.Long;
			  int bytesLength = keySize - ENTITY_ID_SIZE;
			  into.BytesLength = bytesLength;
			  cursor.GetBytes( into.Bytes, 0, bytesLength );
		 }

		 public override bool FixedSize()
		 {
			  return false;
		 }

		 public override void MinimalSplitter( StringIndexKey left, StringIndexKey right, StringIndexKey into )
		 {
			  into.CompareId = right.CompareId;
			  if ( CompareValue( left, right ) != 0 )
			  {
					into.EntityId = NO_ENTITY_ID;
			  }
			  else
			  {
					into.EntityId = right.EntityId;
			  }
			  int targetLength = MinimalLengthFromRightNeededToDifferentiateFromLeft( left.Bytes, left.BytesLengthConflict, right.Bytes, right.BytesLengthConflict );
			  into.CopyValueFrom( right, targetLength );
		 }

		 internal static int MinimalLengthFromRightNeededToDifferentiateFromLeft( sbyte[] leftBytes, int leftLength, sbyte[] rightBytes, int rightLength )
		 {
			  int lastEqualIndex = -1;
			  int maxLength = min( leftLength, rightLength );
			  for ( int index = 0; index < maxLength; index++ )
			  {
					if ( leftBytes[index] != rightBytes[index] )
					{
						 break;
					}
					lastEqualIndex++;
			  }
			  // Convert from last equal index to first index to differ +1
			  // Convert from index to length +1
			  // Total +2
			  return Math.Min( rightLength, lastEqualIndex + 2 );
		 }

		 public override string ToString()
		 {
			  return format( "%s[version:%d.%d, identifier:%d]", this.GetType().Name, MajorVersion(), MinorVersion(), Identifier() );
		 }

		 internal override int CompareValue( StringIndexKey o1, StringIndexKey o2 )
		 {
			  return o1.CompareValueTo( o2 );
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for <seealso cref="GBPTree"/> used by <seealso cref="NativeLabelScanStore"/>.
	/// 
	/// <ul>
	/// <li>
	/// Each keys is a combination of {@code labelId} and {@code nodeIdRange} ({@code nodeId/64}).
	/// </li>
	/// <li>
	/// Each value is a 64-bit bit set (a primitive {@code long}) where each set bit in it represents
	/// a node with that label, such that {@code nodeId = nodeIdRange+bitOffset}. Range size is 64 bits.
	/// </li>
	/// </ul>
	/// </summary>
	public class LabelScanLayout : Neo4Net.Index.@internal.gbptree.Layout_Adapter<LabelScanKey, LabelScanValue>
	{
		 /// <summary>
		 /// Name part of the <seealso cref="identifier()"/> value.
		 /// </summary>
		 private const string IDENTIFIER_NAME = "LSL";

		 /// <summary>
		 /// Size of each <seealso cref="LabelScanKey"/>.
		 /// </summary>
		 private static readonly int _keySize = Integer.BYTES + 6;

		 /// <summary>
		 /// Compares <seealso cref="LabelScanKey"/>, giving ascending order of {@code labelId} then {@code nodeIdRange}.
		 /// </summary>
		 public override int Compare( LabelScanKey o1, LabelScanKey o2 )
		 {
			  int labelComparison = Integer.compare( o1.LabelId, o2.LabelId );
			  return labelComparison != 0 ? labelComparison : Long.compare( o1.IdRange, o2.IdRange );
		 }

		 public override LabelScanKey NewKey()
		 {
			  return new LabelScanKey();
		 }

		 public override LabelScanKey CopyKey( LabelScanKey key, LabelScanKey into )
		 {
			  into.LabelId = key.LabelId;
			  into.IdRange = key.IdRange;
			  return into;
		 }

		 public override LabelScanValue NewValue()
		 {
			  return new LabelScanValue();
		 }

		 public override int KeySize( LabelScanKey key )
		 {
			  return _keySize;
		 }

		 public override int ValueSize( LabelScanValue value )
		 {
			  return LabelScanValue.RangeSizeBytes;
		 }

		 public override void WriteKey( PageCursor cursor, LabelScanKey key )
		 {
			  cursor.PutInt( key.LabelId );
			  Put6ByteLong( cursor, key.IdRange );
		 }

		 private static void Put6ByteLong( PageCursor cursor, long value )
		 {
			  cursor.PutInt( ( int ) value );
			  cursor.PutShort( ( short )( ( long )( ( ulong )value >> ( sizeof( int ) * 8 ) ) ) );
		 }

		 public override void WriteValue( PageCursor cursor, LabelScanValue value )
		 {
			  cursor.PutLong( value.Bits );
		 }

		 public override void ReadKey( PageCursor cursor, LabelScanKey into, int keySize )
		 {
			  into.LabelId = cursor.Int;
			  into.IdRange = Get6ByteLong( cursor );
		 }

		 private static long Get6ByteLong( PageCursor cursor )
		 {
			  long low4b = cursor.Int & 0xFFFFFFFFL;
			  long high2b = cursor.Short;
			  return low4b | ( high2b << ( sizeof( int ) * 8 ) );
		 }

		 public override void ReadValue( PageCursor cursor, LabelScanValue into, int valueSize )
		 {
			  into.Bits = cursor.Long;
		 }

		 public override bool FixedSize()
		 {
			  return true;
		 }

		 public override long Identifier()
		 {
			  return Layout.namedIdentifier( IDENTIFIER_NAME, LabelScanValue.RangeSize );
		 }

		 public override int MajorVersion()
		 {
			  return 0;
		 }

		 public override int MinorVersion()
		 {
			  return 1;
		 }
	}

}
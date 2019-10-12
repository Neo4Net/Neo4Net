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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="Layout"/> for absolute date times.
	/// </summary>
	internal class ZonedDateTimeLayout : IndexLayout<ZonedDateTimeIndexKey, NativeIndexValue>
	{
		 // A 1 signals a named time zone is stored, a 0 that an offset is stored
		 internal const int ZONE_ID_FLAG = 0x0100_0000;
		 // Mask for offsets to remove to not collide with the flag for negative numbers
		 // It is 24 bits which allows to store all possible minute offsets
		 internal const int ZONE_ID_MASK = 0x00FF_FFFF;
		 // This is used to determine if the value is negative (after applying the bitmask)
		 internal const int ZONE_ID_HIGH = 0x0080_0000;
		 // This is ised to restore masked negative offsets to their real value
		 internal const int ZONE_ID_EXT = unchecked( ( int )0xFF00_0000 );

		 internal ZonedDateTimeLayout() : base("Tdt", 0, 1)
		 {
		 }

		 public override ZonedDateTimeIndexKey NewKey()
		 {
			  return new ZonedDateTimeIndexKey();
		 }

		 public override ZonedDateTimeIndexKey CopyKey( ZonedDateTimeIndexKey key, ZonedDateTimeIndexKey into )
		 {
			  into.EpochSecondUTC = key.EpochSecondUTC;
			  into.NanoOfSecond = key.NanoOfSecond;
			  into.ZoneId = key.ZoneId;
			  into.ZoneOffsetSeconds = key.ZoneOffsetSeconds;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  return into;
		 }

		 public override int KeySize( ZonedDateTimeIndexKey key )
		 {
			  return ZonedDateTimeIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, ZonedDateTimeIndexKey key )
		 {
			  cursor.PutLong( key.EpochSecondUTC );
			  cursor.PutInt( key.NanoOfSecond );
			  if ( key.ZoneId >= 0 )
			  {
					cursor.PutInt( key.ZoneId | ZONE_ID_FLAG );
			  }
			  else
			  {
					cursor.PutInt( key.ZoneOffsetSeconds & ZONE_ID_MASK );
			  }
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, ZonedDateTimeIndexKey into, int keySize )
		 {
			  into.EpochSecondUTC = cursor.Long;
			  into.NanoOfSecond = cursor.Int;
			  int encodedZone = cursor.Int;
			  if ( IsZoneId( encodedZone ) )
			  {
					into.ZoneId = AsZoneId( encodedZone );
					into.ZoneOffsetSeconds = 0;
			  }
			  else
			  {
					into.ZoneId = -1;
					into.ZoneOffsetSeconds = AsZoneOffset( encodedZone );
			  }
			  into.EntityId = cursor.Long;
		 }

		 internal static int AsZoneOffset( int encodedZone )
		 {
			  if ( ( ZONE_ID_HIGH & encodedZone ) == ZONE_ID_HIGH )
			  {
					return ZONE_ID_EXT | encodedZone;
			  }
			  else
			  {
					return encodedZone;
			  }
		 }

		 internal static short AsZoneId( int encodedZone )
		 {
			  return ( short )( encodedZone & ZONE_ID_MASK );
		 }

		 internal static bool IsZoneId( int encodedZone )
		 {
			  return ( encodedZone & ZONE_ID_FLAG ) != 0;
		 }
	}

}
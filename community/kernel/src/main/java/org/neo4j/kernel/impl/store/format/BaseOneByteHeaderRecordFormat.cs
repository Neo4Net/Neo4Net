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
namespace Org.Neo4j.Kernel.impl.store.format
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	/// <summary>
	/// Implementation of a very common type of format where the first byte, at least one bit in it,
	/// say whether or not the record is in use. That can be used to let sub classes have simpler
	/// read/write implementations. The rest of the 7 bits in that header byte are free to use by subclasses.
	/// </summary>
	/// @param <RECORD> type of record. </param>
	public abstract class BaseOneByteHeaderRecordFormat<RECORD> : BaseRecordFormat<RECORD> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 protected internal const int HEADER_SIZE = 1;
		 private readonly int _inUseBitMaskForFirstByte;

		 protected internal BaseOneByteHeaderRecordFormat( System.Func<StoreHeader, int> recordSize, int recordHeaderSize, int inUseBitMaskForFirstByte, int idBits ) : base( recordSize, recordHeaderSize, idBits )
		 {
			  this._inUseBitMaskForFirstByte = inUseBitMaskForFirstByte;
		 }

		 protected internal virtual void MarkAsUnused( PageCursor cursor )
		 {
			  sbyte inUseByte = cursor.GetByte( cursor.Offset );
			  inUseByte &= ( sbyte )( ~_inUseBitMaskForFirstByte );
			  cursor.PutByte( inUseByte );
		 }

		 public override bool IsInUse( PageCursor cursor )
		 {
			  return IsInUse( cursor.GetByte( cursor.Offset ) );
		 }

		 /// <summary>
		 /// Given a record with a header byte this method checks the specific bit which this record format was
		 /// configured to interpret as inUse.
		 /// </summary>
		 /// <param name="headerByte"> header byte of a record (the first byte) which contains the inUse bit we're interested in. </param>
		 /// <returns> whether or not this header byte has the specific bit saying that it's in use. </returns>
		 protected internal virtual bool IsInUse( sbyte headerByte )
		 {
			  return Has( headerByte, _inUseBitMaskForFirstByte );
		 }

		 /// <summary>
		 /// Checks whether or not a specific bit in a byte is set.
		 /// </summary>
		 /// <param name="headerByte"> the header byte to check, here represented as a {@code long} for convenience
		 /// due to many callers keeping this header as long as to remove common problems of forgetting to
		 /// cast to long before shifting. </param>
		 /// <param name="bitMask"> mask for the bit to check, such as 0x1, 0x2 and 0x4. </param>
		 /// <returns> whether or not that bit is set. </returns>
		 protected internal static bool Has( long headerByte, int bitMask )
		 {
			  return ( headerByte & bitMask ) != 0;
		 }

		 /// <summary>
		 /// Sets or clears bits specified by the {@code bitMask} in the header byte.
		 /// </summary>
		 /// <param name="headerByte"> byte to set bits in. </param>
		 /// <param name="bitMask"> mask specifying which bits to change. </param>
		 /// <param name="value"> {@code true} means setting the bits specified by the bit mask, {@code false} means clearing. </param>
		 /// <returns> the {@code headerByte} with the changes incorporated. </returns>
		 protected internal static sbyte Set( sbyte headerByte, int bitMask, bool value )
		 {
			  return ( sbyte )( value ? headerByte | bitMask : headerByte );
		 }
	}

}
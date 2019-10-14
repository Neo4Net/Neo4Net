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
namespace Neo4Net.Index.@internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// Defines interfaces and common implementations of header reader/writer for <seealso cref="GBPTree"/>.
	/// </summary>
	public class Header
	{
		 /// <summary>
		 /// The total overhead of other things written into the page that the additional header is written into.
		 /// Therefore the max size of an additional header cannot exceed page size minus this overhead.
		 /// </summary>
		 public static readonly int Overhead = TreeState.Size + Integer.BYTES; // size of the field storing the length of the additional header data

		 /// <summary>
		 /// Writes a header into a <seealso cref="GBPTree"/> state page during
		 /// <seealso cref="GBPTree.checkpoint(org.neo4j.io.pagecache.IOLimiter)"/>.
		 /// </summary>
		 public interface Writer
		 {
			  /// <summary>
			  /// Writes header data into {@code to} with previous valid header data found in {@code from} of {@code length}
			  /// bytes in size. </summary>
			  /// <param name="from"> <seealso cref="PageCursor"/> positioned at the header data written in the previous check point. </param>
			  /// <param name="length"> size in bytes of the previous header data. </param>
			  /// <param name="to"> <seealso cref="PageCursor"/> to write new header into. </param>
			  void Write( PageCursor from, int length, PageCursor to );
		 }

		 private Header()
		 {
		 }

		 internal static readonly Writer CarryOverPreviousHeader = ( from, length, to ) =>
		 {
		  int toOffset = to.Offset;
		  from.copyTo( from.Offset, to, toOffset, length );
		  to.Offset = toOffset + length;
		 };

		 internal static Writer Replace( System.Action<PageCursor> writer )
		 {
			  // Discard the previous state, just write the new
			  return ( from, length, to ) => writer( to );
		 }

		 /// <summary>
		 /// Reads a header from a <seealso cref="GBPTree"/> state page during opening it.
		 /// </summary>
		 public interface Reader
		 {
			  /// <summary>
			  /// Called when it's time to read header data from the most up to date and valid state page.
			  /// The data that can be accessed from the {@code headerBytes} buffer have been consistently
			  /// read from a <seealso cref="PageCursor"/>.
			  /// </summary>
			  /// <param name="headerBytes"> <seealso cref="ByteBuffer"/> containing the header data. </param>
			  void Read( ByteBuffer headerBytes );
		 }
	}

}
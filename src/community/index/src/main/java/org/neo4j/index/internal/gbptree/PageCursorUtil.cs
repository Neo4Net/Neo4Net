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
namespace Neo4Net.Index.Internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	/// <summary>
	/// <seealso cref="PageCursor"/> functionality commonly used around the <seealso cref="GBPTree"/> and supporting code.
	/// </summary>
	internal class PageCursorUtil
	{
		 internal const int _2_B_MASK = 0xFFFF;
		 internal const long _4_B_MASK = 0xFFFFFFFFL;
		 internal const long _6_B_MASK = 0xFFFF_FFFFFFFFL;

		 private PageCursorUtil()
		 {
		 }

		 /// <summary>
		 /// Puts the low 6 bytes of the {@code value} into {@code cursor} at current offset.
		 /// Puts <seealso cref="PageCursor.putInt(int) int"/> followed by <seealso cref="PageCursor.putShort(short) short"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to put into, at the current offset. </param>
		 /// <param name="value"> the value to put. </param>
		 internal static void Put6BLong( PageCursor cursor, long value )
		 {
			  if ( ( value & ~_6_B_MASK ) != 0 )
			  {
					throw new System.ArgumentException( "Illegal 6B value " + value );
			  }

			  int lsb = ( int ) value;
			  short msb = ( short )( ( long )( ( ulong )value >> ( sizeof( int ) * 8 ) ) );
			  cursor.PutInt( lsb );
			  cursor.PutShort( msb );
		 }

		 /// <summary>
		 /// Gets 6 bytes from {@code cursor} at current offset and returns that a as a {@code long}.
		 /// Reads <seealso cref="PageCursor.getInt()"/> followed by <seealso cref="PageCursor.getShort()"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to get from, at the current offset. </param>
		 /// <returns> the 6 bytes as a {@code long}. </returns>
		 internal static long Get6BLong( PageCursor cursor )
		 {
			  long lsb = GetUnsignedInt( cursor );
			  long msb = GetUnsignedShort( cursor );
			  return lsb | ( msb << ( sizeof( int ) * 8 ) );
		 }

		 /// <summary>
		 ///  Puts the low 2 bytes of the {@code value} into cursor at current offset.
		 ///  Puts <seealso cref="PageCursor.putShort(short)"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to put into, at the current offset. </param>
		 /// <param name="value"> the value to put. </param>
		 internal static void PutUnsignedShort( PageCursor cursor, int value )
		 {
			  if ( ( value & ~_2_B_MASK ) != 0 )
			  {
					throw new System.ArgumentException( "Illegal 2B value " + value );
			  }

			  cursor.PutShort( ( short ) value );
		 }

		 /// <summary>
		 ///  Puts the low 2 bytes of the {@code value} into cursor at given offset.
		 ///  Puts <seealso cref="PageCursor.putShort(short)"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to put into. </param>
		 /// <param name="offset"> offset into page where to write. </param>
		 /// <param name="value"> the value to put. </param>
		 internal static void PutUnsignedShort( PageCursor cursor, int offset, int value )
		 {
			  if ( ( value & ~_2_B_MASK ) != 0 )
			  {
					throw new System.ArgumentException( "Illegal 2B value " + value );
			  }

			  cursor.PutShort( offset, ( short ) value );
		 }

		 /// <summary>
		 /// Gets 2 bytes and returns that value as an {@code int}, ignoring its sign.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to get from, at the current offset. </param>
		 /// <returns> {@code int} containing the value of the unsigned {@code short}. </returns>
		 internal static int GetUnsignedShort( PageCursor cursor )
		 {
			  return cursor.Short & _2_B_MASK;
		 }

		 /// <summary>
		 /// Gets 2 bytes and returns that value as an {@code int}, ignoring its sign.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to get from. </param>
		 /// <param name="offset"> offset into page from where to read. </param>
		 /// <returns> {@code int} containing the value of the unsigned {@code short}. </returns>
		 internal static int GetUnsignedShort( PageCursor cursor, int offset )
		 {
			  return cursor.GetShort( offset ) & _2_B_MASK;
		 }

		 /// <summary>
		 /// Gets 4 bytes and returns that value as an {@code long}, ignoring its sign.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to get from, at the current offset. </param>
		 /// <returns> {@code long} containing the value of the unsigned {@code int}. </returns>
		 internal static long GetUnsignedInt( PageCursor cursor )
		 {
			  return cursor.Int & _4_B_MASK;
		 }

		 /// <summary>
		 /// Calls <seealso cref="PageCursor.checkAndClearBoundsFlag()"/> and if {@code true} throws <seealso cref="TreeInconsistencyException"/>.
		 /// Should be called whenever leaving a <seealso cref="PageCursor.shouldRetry() shouldRetry-loop"/> successfully.
		 /// Purpose of this method is to unify <seealso cref="PageCursor"/> read behavior and exception handling.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to check for out-of-bounds. </param>
		 internal static void CheckOutOfBounds( PageCursor cursor )
		 {
			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					throw new TreeInconsistencyException( "Some internal problem causing out of bounds: pageId:" + cursor.CurrentPageId );
			  }
		 }

		 /// <summary>
		 /// Calls <seealso cref="PageCursor.next(long)"/> with the {@code pageId} and throws <seealso cref="System.InvalidOperationException"/>
		 /// if that call returns {@code false}.
		 /// Purpose of this method is to unify exception handling when moving between pages.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to call <seealso cref="PageCursor.next(long)"/> on. </param>
		 /// <param name="messageOnError"> additional error message to include in exception if <seealso cref="PageCursor.next(long)"/>
		 /// returned {@code false}, providing more context to the exception message. </param>
		 /// <param name="pageId"> page id to move to. </param>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor.next(long)"/> exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void goTo(org.neo4j.io.pagecache.PageCursor cursor, String messageOnError, long pageId) throws java.io.IOException
		 internal static void GoTo( PageCursor cursor, string messageOnError, long pageId )
		 {
			  if ( !cursor.Next( pageId ) )
			  {
					throw new System.InvalidOperationException( "Could not go to page:" + pageId + " [" + messageOnError + "]" );
			  }
		 }
	}

}
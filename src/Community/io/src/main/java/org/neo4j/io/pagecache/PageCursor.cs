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
namespace Neo4Net.Io.pagecache
{

	/// <summary>
	/// A PageCursor is returned from <seealso cref="org.neo4j.io.pagecache.PagedFile.io(long, int)"/>,
	/// and is used to scan through pages and process them in a consistent and safe fashion.
	/// <para>
	/// A page must be processed in the following manner:
	/// <pre><code>
	///     try ( PageCursor cursor = pagedFile.io( pageId, pf_flags ) )
	///     {
	///         // Use 'if' for processing a single page,
	///         // use 'while' for scanning through pages:
	///         if ( cursor.next() )
	///         {
	///             do
	///             {
	///                 processPage( cursor );
	///             } while ( cursor.shouldRetry() );
	///             // Any finalising, non-repeatable post-processing
	///             // goes here.
	///         }
	///     }
	///     catch ( IOException e )
	///     {
	///         // handle the error, somehow
	///     }
	/// </code></pre>
	/// </para>
	/// <para>There are a couple of things to this pattern that are worth noting:
	/// <ul>
	/// <li>We use a try-with-resources clause to make sure that the resources associated with the PageCursor are always
	/// released properly.
	/// </li>
	/// <li>We use an if-clause for the next() call if we are only processing a single page, to make sure that the page
	/// exist and is accessible to us.
	/// </li>
	/// <li>We use a while-clause for next() if we are scanning through pages.
	/// </li>
	/// <li>We do our processing of the page in a do-while-retry loop, to make sure that we processed a page that was in a
	/// consistent state.
	/// </li>
	/// </ul>
	/// You can alternatively use the <seealso cref="next(long)"/> method, to navigate the
	/// pages you need in a non-linear fashion.
	/// </para>
	/// </summary>
	public abstract class PageCursor : AutoCloseable
	{
		 public const long UNBOUND_PAGE_ID = -1;
		 public const int UNBOUND_PAGE_SIZE = -1;

		 /// <summary>
		 /// Get the signed byte at the current page offset, and then increment the offset by one.
		 /// </summary>
		 public abstract sbyte Byte { get; }

		 /// <summary>
		 /// Get the signed byte at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract sbyte GetByte( int offset );

		 /// <summary>
		 /// Set the signed byte at the current offset into the page, and then increment the offset by one.
		 /// </summary>
		 public abstract void PutByte( sbyte value );

		 /// <summary>
		 /// Set the signed byte at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract void PutByte( int offset, sbyte value );

		 /// <summary>
		 /// Get the signed long at the current page offset, and then increment the offset by one.
		 /// </summary>
		 public abstract long Long { get; }

		 /// <summary>
		 /// Get the signed long at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract long GetLong( int offset );

		 /// <summary>
		 /// Set the signed long at the current offset into the page, and then increment the offset by one.
		 /// </summary>
		 public abstract void PutLong( long value );

		 /// <summary>
		 /// Set the signed long at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract void PutLong( int offset, long value );

		 /// <summary>
		 /// Get the signed int at the current page offset, and then increment the offset by one.
		 /// </summary>
		 public abstract int Int { get; }

		 /// <summary>
		 /// Get the signed int at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract int GetInt( int offset );

		 /// <summary>
		 /// Set the signed int at the current offset into the page, and then increment the offset by one.
		 /// </summary>
		 public abstract void PutInt( int value );

		 /// <summary>
		 /// Set the signed int at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract void PutInt( int offset, int value );

		 /// <summary>
		 /// Fill the given array with bytes from the page, beginning at the current offset into the page,
		 /// and then increment the current offset by the length of the array.
		 /// </summary>
		 public abstract void GetBytes( sbyte[] data );

		 /// <summary>
		 /// Read the given length of bytes from the page into the given array, starting from the current offset into the
		 /// page, and writing from the given array offset, and then increment the current offset by the length argument.
		 /// </summary>
		 public abstract void GetBytes( sbyte[] data, int arrayOffset, int length );

		 /// <summary>
		 /// Write out all the bytes of the given array into the page, beginning at the current offset into the page,
		 /// and then increment the current offset by the length of the array.
		 /// </summary>
		 public abstract void PutBytes( sbyte[] data );

		 /// <summary>
		 /// Write out the given length of bytes from the given offset into the the given array of bytes, into the page,
		 /// beginning at the current offset into the page, and then increment the current offset by the length argument.
		 /// </summary>
		 public abstract void PutBytes( sbyte[] data, int arrayOffset, int length );

		 /// <summary>
		 /// Set the given number of bytes to the given value, beginning at current offset into the page.
		 /// </summary>
		 public abstract void PutBytes( int bytes, sbyte value );

		 /// <summary>
		 /// Get the signed short at the current page offset, and then increment the offset by one.
		 /// </summary>
		 public abstract short Short { get; }

		 /// <summary>
		 /// Get the signed short at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract short GetShort( int offset );

		 /// <summary>
		 /// Set the signed short at the current offset into the page, and then increment the offset by one.
		 /// </summary>
		 public abstract void PutShort( short value );

		 /// <summary>
		 /// Set the signed short at the given offset into the page.
		 /// Leaves the current page offset unchanged.
		 /// </summary>
		 public abstract void PutShort( int offset, short value );

		 /// <summary>
		 /// Set the current offset into the page, for interacting with the page through the read and write methods that do
		 /// not take a specific offset as an argument.
		 /// </summary>
		 public abstract int Offset { set;get; }


		 /// <summary>
		 /// Mark the current offset. Only one offset can be marked at any time.
		 /// </summary>
		 public abstract void Mark();

		 /// <summary>
		 /// Set the offset to the marked offset. This does not modify the value of the mark.
		 /// </summary>
		 public abstract void SetOffsetToMark();

		 /// <summary>
		 /// Get the file page id that the cursor is currently positioned at, or
		 /// UNBOUND_PAGE_ID if next() has not yet been called on this cursor, or returned false.
		 /// A call to rewind() will make the current page id unbound as well, until
		 /// next() is called.
		 /// </summary>
		 public abstract long CurrentPageId { get; }

		 /// <summary>
		 /// Get the file page size of the page that the cursor is currently positioned at,
		 /// or UNBOUND_PAGE_SIZE if next() has not yet been called on this cursor, or returned false.
		 /// A call to rewind() will make the current page unbound as well, until next() is called.
		 /// </summary>
		 public abstract int CurrentPageSize { get; }

		 /// <summary>
		 /// Get the file the cursor is currently bound to, or {@code null} if next() has not yet been called on this
		 /// cursor, or returned false.
		 /// A call to rewind() will make the cursor unbound as well, until next() is called.
		 /// </summary>
		 public abstract File CurrentFile { get; }

		 /// <summary>
		 /// Rewinds the cursor to its initial condition, as if freshly returned from
		 /// an equivalent io() call. In other words, the next call to next() will
		 /// move the cursor to the starting page that was specified in the io() that
		 /// produced the cursor.
		 /// </summary>
		 public abstract void Rewind();

		 /// <summary>
		 /// Moves the cursor to the next page, if any, and returns true when it is
		 /// ready to be processed. Returns false if there are no more pages to be
		 /// processed. For instance, if the cursor was requested with PF_NO_GROW
		 /// and the page most recently processed was the last page in the file.
		 /// <para>
		 /// <strong>NOTE: When using read locks, read operations can be inconsistent
		 /// and may return completely random data. The data returned from a
		 /// read-locked page cursor should not be interpreted until after
		 /// <seealso cref="shouldRetry()"/> has returned {@code false}.</strong>
		 /// Not interpreting the data also implies that you cannot throw exceptions
		 /// from data validation errors until after <seealso cref="shouldRetry()"/> has told
		 /// you that your read was consistent.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract boolean next() throws java.io.IOException;
		 public abstract bool Next();

		 /// <summary>
		 /// Moves the cursor to the page specified by the given pageId, if any,
		 /// and returns true when it is ready to be processed. Returns false if
		 /// for instance, the cursor was requested with PF_NO_GROW and the page
		 /// most recently processed was the last page in the file.
		 /// <para>
		 /// <strong>NOTE: When using read locks, read operations can be inconsistent
		 /// and may return completely random data. The data returned from a
		 /// read-locked page cursor should not be interpreted until after
		 /// <seealso cref="shouldRetry()"/> has returned {@code false}.</strong>
		 /// Not interpreting the data also implies that you cannot throw exceptions
		 /// from data validation errors until after <seealso cref="shouldRetry()"/> has told
		 /// you that your read was consistent.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract boolean next(long pageId) throws java.io.IOException;
		 public abstract bool Next( long pageId );

		 /// <summary>
		 /// Relinquishes all resources associated with this cursor, including the
		 /// cursor itself, and any linked cursors opened through it. The cursor cannot be used after this call.
		 /// </summary>
		 /// <seealso cref= AutoCloseable#close() </seealso>
		 public override abstract void Close();

		 /// <summary>
		 /// Returns true if the page has entered an inconsistent state since the last call to next() or shouldRetry().
		 /// <para>
		 /// If this method returns true, the in-page offset of the cursor will be reset to zero.
		 /// </para>
		 /// <para>
		 /// Note that <seealso cref="PagedFile.PF_SHARED_WRITE_LOCK write locked"/> cursors never conflict with each other, nor with
		 /// eviction, and thus technically don't require a {@code shouldRetry} check. This method always returns
		 /// {@code false} for write-locking cursors.
		 /// </para>
		 /// <para>
		 /// Cursors that are <seealso cref="PagedFile.PF_SHARED_READ_LOCK read locked"/> must <em>always</em> perform their reads in a
		 /// {@code shouldRetry} loop, and avoid interpreting the data they read until {@code shouldRetry} has confirmed that
		 /// the reads were consistent.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> If the page was evicted while doing IO, the cursor will have
		 /// to do a page fault to get the page back.
		 /// This may throw an IOException. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract boolean shouldRetry() throws java.io.IOException;
		 public abstract bool ShouldRetry();

		 /// <summary>
		 /// Copy the specified number of bytes from the given offset of this page, to the given offset of the target page.
		 /// <para>
		 /// If the length reaches beyond the end of either cursor, then only as many bytes as are available in this cursor,
		 /// or can fit in the target cursor, are actually copied.
		 /// </para>
		 /// <para>
		 /// <strong>Note</strong> that {@code copyTo} is only guaranteed to work when both target and source cursor are from
		 /// the <em>same</em> page cache implementation. Using wrappers, delegates or mixing cursor implementations may
		 /// produce unspecified errors.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="sourceOffset"> The offset into this page to copy from. </param>
		 /// <param name="targetCursor"> The cursor the data will be copied to. </param>
		 /// <param name="targetOffset"> The offset into the target cursor to copy to. </param>
		 /// <param name="lengthInBytes"> The number of bytes to copy. </param>
		 /// <returns> The number of bytes actually copied. </returns>
		 public abstract int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes );

		 /// <summary>
		 /// Copy bytes from the specified offset in this page, into the given buffer, until either the limit of the buffer
		 /// is reached, or the end of the page is reached. The actual number of bytes copied is returned.
		 /// </summary>
		 /// <param name="sourceOffset"> The offset into this page to copy from. </param>
		 /// <param name="targetBuffer"> The buffer the data will be copied to. </param>
		 /// <returns> The number of bytes actually copied. </returns>
		 public abstract int CopyTo( int sourceOffset, ByteBuffer targetBuffer );

		 /// <summary>
		 /// Shift the specified number of bytes starting from given offset the specified number of bytes to the left or
		 /// right. The area
		 /// left behind after the shift is not padded and thus is left with garbage.
		 /// <para>
		 /// Out of bounds flag is raised if either start or end of either source range or target range fall outside end of
		 /// this cursor
		 /// or if length is negative.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="sourceOffset"> The offset into this page to start moving from. </param>
		 /// <param name="length"> The number of bytes to move. </param>
		 /// <param name="shift"> How many steps, in terms of number of bytes, to shift. Can be both positive and negative. </param>
		 public abstract void ShiftBytes( int sourceOffset, int length, int shift );

		 /// <summary>
		 /// Discern whether an out-of-bounds access has occurred since the last call to <seealso cref="next()"/> or
		 /// <seealso cref="next(long)"/>, or since the last call to <seealso cref="shouldRetry()"/> that returned {@code true}, or since the
		 /// last call to this method.
		 /// </summary>
		 /// <returns> {@code true} if an access was out of bounds, or the <seealso cref="raiseOutOfBounds()"/> method has been called. </returns>
		 public abstract bool CheckAndClearBoundsFlag();

		 /// <summary>
		 /// Check if a cursor error has been set on this or any linked cursor, and if so, remove it from the cursor
		 /// and throw it as a <seealso cref="CursorException"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void checkAndClearCursorException() throws CursorException;
		 public abstract void CheckAndClearCursorException();

		 /// <summary>
		 /// Explicitly raise the out-of-bounds flag.
		 /// </summary>
		 /// <seealso cref= #checkAndClearBoundsFlag() </seealso>
		 public abstract void RaiseOutOfBounds();

		 /// <summary>
		 /// Set an error condition on the cursor with the given message.
		 /// <para>
		 /// This will make calls to <seealso cref="checkAndClearCursorException()"/> throw a <seealso cref="CursorException"/> with the given
		 /// message, unless the error has gotten cleared by a <seealso cref="shouldRetry()"/> call that returned {@code true},
		 /// a call to <seealso cref="next()"/> or <seealso cref="next(long)"/>, or the cursor is closed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="message"> The message of the <seealso cref="CursorException"/> that <seealso cref="checkAndClearCursorException()"/> will
		 /// throw. </param>
		 public abstract string CursorException { set; }

		 /// <summary>
		 /// Unconditionally clear any error condition that has been set on this or any linked cursor, without throwing an
		 /// exception.
		 /// </summary>
		 public abstract void ClearCursorException();

		 /// <summary>
		 /// Open a new page cursor with the same pf_flags as this cursor, as if calling the <seealso cref="PagedFile.io(long, int)"/>
		 /// on the relevant paged file. This cursor will then also delegate to the linked cursor when checking
		 /// <seealso cref="shouldRetry()"/> and <seealso cref="checkAndClearBoundsFlag()"/>.
		 /// <para>
		 /// Opening a linked cursor on a cursor that already has a linked cursor, will close the older linked cursor.
		 /// Closing a cursor also closes any linked cursor.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pageId"> The page id that the linked cursor will be placed at after its first call to <seealso cref="next()"/>. </param>
		 /// <returns> A cursor that is linked with this cursor. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract PageCursor openLinkedCursor(long pageId) throws java.io.IOException;
		 public abstract PageCursor OpenLinkedCursor( long pageId );

		 /// <summary>
		 /// Sets all bytes in this page to zero, as if this page was newly allocated at the end of the file.
		 /// </summary>
		 public abstract void ZapPage();

		 /// <returns> {@code true} if this page cursor was opened with <seealso cref="PagedFile.PF_SHARED_WRITE_LOCK"/>,
		 /// {@code false} otherwise. </returns>
		 public abstract bool WriteLocked { get; }
	}

}
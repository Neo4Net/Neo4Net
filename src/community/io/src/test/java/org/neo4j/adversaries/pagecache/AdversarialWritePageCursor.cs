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
namespace Neo4Net.Adversaries.pagecache
{

	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using DelegatingPageCursor = Neo4Net.Io.pagecache.impl.DelegatingPageCursor;

	/// <summary>
	/// A write <seealso cref="PageCursor page cursor"/> that wraps another page cursor and an <seealso cref="Adversary adversary"/>
	/// to provide a misbehaving page cursor implementation for testing.
	/// <para>
	/// Depending on the adversary each read and write operation can throw either <seealso cref="System.Exception"/> like
	/// <seealso cref="SecurityException"/> or <seealso cref="IOException"/> like <seealso cref="FileNotFoundException"/>.
	/// </para>
	/// <para>
	/// Read operations will always return a consistent value because the underlying page is write locked.
	/// See <seealso cref="org.Neo4Net.io.pagecache.PagedFile.PF_SHARED_WRITE_LOCK"/> flag.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class AdversarialWritePageCursor extends org.Neo4Net.io.pagecache.impl.DelegatingPageCursor
	internal class AdversarialWritePageCursor : DelegatingPageCursor
	{
		 private readonly Adversary _adversary;
		 private AdversarialWritePageCursor _linkedCursor;

		 internal AdversarialWritePageCursor( PageCursor @delegate, Adversary adversary ) : base( @delegate )
		 {
			  this._adversary = Objects.requireNonNull( adversary );
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
				  return Delegate.Byte;
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  return Delegate.getByte( offset );
		 }

		 public override void PutByte( sbyte value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putByte( value );
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putByte( offset, value );
		 }

		 public override long Long
		 {
			 get
			 {
				  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
				  return Delegate.Long;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  return Delegate.getLong( offset );
		 }

		 public override void PutLong( long value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putLong( value );
		 }

		 public override void PutLong( int offset, long value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putLong( offset, value );
		 }

		 public override int Int
		 {
			 get
			 {
				  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
				  return Delegate.Int;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  return Delegate.getInt( offset );
		 }

		 public override void PutInt( int value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putInt( value );
		 }

		 public override void PutInt( int offset, int value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putInt( offset, value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.getBytes( data );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.getBytes( data, arrayOffset, length );
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putBytes( data );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putBytes( data, arrayOffset, length );
		 }

		 public override short Short
		 {
			 get
			 {
				  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
				  return Delegate.Short;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  return Delegate.getShort( offset );
		 }

		 public override void PutShort( short value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putShort( value );
		 }

		 public override void PutShort( int offset, short value )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  Delegate.putShort( offset, value );
		 }

		 public override int Offset
		 {
			 set
			 {
				  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
				  Delegate.Offset = value;
			 }
			 get
			 {
				  return Delegate.Offset;
			 }
		 }


		 public override long CurrentPageId
		 {
			 get
			 {
				  return Delegate.CurrentPageId;
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  return Delegate.CurrentPageSize;
			 }
		 }

		 public override File CurrentFile
		 {
			 get
			 {
				  return Delegate.CurrentFile;
			 }
		 }

		 public override void Rewind()
		 {
			  Delegate.rewind();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ), typeof( System.InvalidOperationException ) );
			  return Delegate.next();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ), typeof( System.InvalidOperationException ) );
			  return Delegate.next( pageId );
		 }

		 public override void Close()
		 {
			  Delegate.close();
			  _linkedCursor = null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ), typeof( System.InvalidOperationException ) );
			  bool retry = Delegate.shouldRetry();
			  return retry || ( _linkedCursor != null && _linkedCursor.shouldRetry() );
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  _adversary.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  while ( targetCursor is DelegatingPageCursor )
			  {
					targetCursor = ( ( DelegatingPageCursor ) targetCursor ).unwrap();
			  }
			  return Delegate.copyTo( sourceOffset, targetCursor, targetOffset, lengthInBytes );
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  return Delegate.checkAndClearBoundsFlag() || (_linkedCursor != null && _linkedCursor.checkAndClearBoundsFlag());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.Neo4Net.io.pagecache.CursorException
		 public override void CheckAndClearCursorException()
		 {
			  Delegate.checkAndClearCursorException();
		 }

		 public override void RaiseOutOfBounds()
		 {
			  Delegate.raiseOutOfBounds();
		 }

		 public override string CursorException
		 {
			 set
			 {
				  Delegate.CursorException = value;
			 }
		 }

		 public override void ClearCursorException()
		 {
			  Delegate.clearCursorException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.pagecache.PageCursor openLinkedCursor(long pageId) throws java.io.IOException
		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  return _linkedCursor = new AdversarialWritePageCursor( Delegate.openLinkedCursor( pageId ), _adversary );
		 }

		 public override void ZapPage()
		 {
			  Delegate.zapPage();
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return true;
			 }
		 }
	}

}
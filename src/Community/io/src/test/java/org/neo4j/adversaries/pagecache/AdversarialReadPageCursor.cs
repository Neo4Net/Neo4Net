using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Adversaries.pagecache
{

	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using DelegatingPageCursor = Neo4Net.Io.pagecache.impl.DelegatingPageCursor;
	using FeatureToggles = Neo4Net.Util.FeatureToggles;

	/// <summary>
	/// A read <seealso cref="PageCursor page cursor"/> that wraps another page cursor and an <seealso cref="Adversary adversary"/>
	/// to provide a misbehaving page cursor implementation for testing.
	/// <para>
	/// Depending on the adversary each read operation can throw either <seealso cref="System.Exception"/> like
	/// <seealso cref="SecurityException"/> or <seealso cref="IOException"/> like <seealso cref="FileNotFoundException"/>.
	/// </para>
	/// <para>
	/// Depending on the adversary each read operation can produce an inconsistent read and require caller to retry using
	/// while loop with <seealso cref="PageCursor.shouldRetry()"/> as a condition.
	/// </para>
	/// <para>
	/// Inconsistent reads are injected by first having a retry-round (the set of operations on the cursor up until the
	/// <seealso cref="shouldRetry()"/> call) that counts the number of operations performed on the cursor, and otherwise delegates
	/// the read operations to the real page cursor without corrupting them. Then the {@code shouldRetry} will choose a
	/// random operation, and from that point on in the next retry-round, all read operations will return random data. The
	/// {@code shouldRetry} method returns {@code true} for "yes, you should retry" and the round with the actual read
	/// inconsistencies begins. After that round, the client will be told to retry again, and in this third round there will
	/// be no inconsistencies, and there will be no need to retry unless the real page cursor says so.
	/// </para>
	/// <para>
	/// Write operations will always throw an <seealso cref="System.InvalidOperationException"/> because this is a read cursor.
	/// See <seealso cref="org.neo4j.io.pagecache.PagedFile.PF_SHARED_READ_LOCK"/> flag.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class AdversarialReadPageCursor extends org.neo4j.io.pagecache.impl.DelegatingPageCursor
	internal class AdversarialReadPageCursor : DelegatingPageCursor
	{
		 private static readonly bool _enableInconsistencyTracing = FeatureToggles.flag( typeof( AdversarialReadPageCursor ), "enableInconsistencyTracing", false );

		 private class State : Adversary
		 {
			  internal readonly Adversary Adversary;

			  internal bool CurrentReadIsPreparingInconsistent;
			  internal bool CurrentReadIsInconsistent;
			  internal int CallCounter;

			  // This field for meant to be inspected with the debugger.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("MismatchedQueryAndUpdateOfCollection") private java.util.List<Object> inconsistentReadHistory;
			  internal IList<object> InconsistentReadHistory;

			  internal State( Adversary adversary )
			  {
					this.Adversary = adversary;
					InconsistentReadHistory = new List<object>( 32 );
			  }

			  internal virtual Number Inconsistently<T>( T value, PageCursor @delegate ) where T : Number
			  {
					if ( CurrentReadIsPreparingInconsistent )
					{
						 CallCounter++;
						 return value;
					}
					if ( CurrentReadIsInconsistent && ( --CallCounter ) <= 0 )
					{
						 ThreadLocalRandom rng = ThreadLocalRandom.current();
						 long x = value.longValue();
						 if ( x != 0 & rng.nextBoolean() )
						 {
							  x = ~x;
						 }
						 else
						 {
							  x = rng.nextLong();
						 }
						 InconsistentReadHistory.Add( new NumberValue( value.GetType(), x, @delegate.Offset, value ) );
						 return x;
					}
					return value;
			  }

			  internal virtual void Inconsistently( sbyte[] data, int arrayOffset, int length )
			  {
					if ( CurrentReadIsPreparingInconsistent )
					{
						 CallCounter++;
					}
					else if ( CurrentReadIsInconsistent )
					{
						 sbyte[] gunk = new sbyte[length];
						 ThreadLocalRandom.current().NextBytes(gunk);
						 Array.Copy( gunk, 0, data, arrayOffset, length );
						 InconsistentReadHistory.Add( Arrays.copyOf( data, data.Length ) );
					}
			  }

			  internal virtual void Reset( bool currentReadIsPreparingInconsistent )
			  {
					CallCounter = 0;
					this.CurrentReadIsPreparingInconsistent = currentReadIsPreparingInconsistent;
			  }

			  public override void InjectFailure( params Type[] failureTypes )
			  {
					Adversary.injectFailure( failureTypes );
			  }

			  public override bool InjectFailureOrMischief( params Type[] failureTypes )
			  {
					return Adversary.injectFailureOrMischief( failureTypes );
			  }

			  internal virtual bool HasPreparedInconsistentRead()
			  {
					if ( CurrentReadIsPreparingInconsistent )
					{
						 CurrentReadIsPreparingInconsistent = false;
						 CurrentReadIsInconsistent = true;
						 CallCounter = ThreadLocalRandom.current().Next(CallCounter + 1);
						 InconsistentReadHistory = new List<object>();
						 return true;
					}
					return false;
			  }

			  internal virtual bool HasInconsistentRead()
			  {
					if ( CurrentReadIsInconsistent )
					{
						 CurrentReadIsInconsistent = false;
						 return true;
					}
					return false;
			  }

			  public virtual bool Inconsistent
			  {
				  get
				  {
						if ( CurrentReadIsPreparingInconsistent )
						{
							 CallCounter++;
						}
						return CurrentReadIsInconsistent;
				  }
			  }
		 }

		 private AdversarialReadPageCursor _linkedCursor;
		 private readonly State _state;

		 internal AdversarialReadPageCursor( PageCursor @delegate, Adversary adversary ) : base( @delegate )
		 {
			  this._state = new State( Objects.requireNonNull( adversary ) );
		 }

		 private AdversarialReadPageCursor( PageCursor @delegate, State state ) : base( @delegate )
		 {
			  this._state = state;
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  return Inconsistently( Delegate.Byte ).byteValue();
			 }
		 }

		 private Number Inconsistently<T>( T value ) where T : Number
		 {
			  return _state.inconsistently( value, Delegate );
		 }

		 private void Inconsistently( sbyte[] data, int arrayOffset, int length )
		 {
			  _state.inconsistently( data, arrayOffset, length );
		 }

		 public override sbyte getByte( int offset )
		 {
			  return Inconsistently( Delegate.getByte( offset ) ).byteValue();
		 }

		 public override void PutByte( sbyte value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override long Long
		 {
			 get
			 {
				  return Inconsistently( Delegate.Long ).longValue();
			 }
		 }

		 public override long getLong( int offset )
		 {
			  return Inconsistently( Delegate.getLong( offset ) ).longValue();
		 }

		 public override void PutLong( long value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void PutLong( int offset, long value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override int Int
		 {
			 get
			 {
				  return Inconsistently( Delegate.Int ).intValue();
			 }
		 }

		 public override int getInt( int offset )
		 {
			  return Inconsistently( Delegate.getInt( offset ) ).intValue();
		 }

		 public override void PutInt( int value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void PutInt( int offset, int value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  Delegate.getBytes( data );
			  Inconsistently( data, 0, data.Length );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  Delegate.getBytes( data, arrayOffset, length );
			  Inconsistently( data, arrayOffset, length );
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override short Short
		 {
			 get
			 {
				  return Inconsistently( Delegate.Short ).shortValue();
			 }
		 }

		 public override short getShort( int offset )
		 {
			  return Inconsistently( Delegate.getShort( offset ) ).shortValue();
		 }

		 public override void PutShort( short value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override void PutShort( int offset, short value )
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override int Offset
		 {
			 set
			 {
				  _state.injectFailure( typeof( System.IndexOutOfRangeException ) );
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
			  PrepareNext();
			  return Delegate.next();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  PrepareNext();
			  return Delegate.next( pageId );
		 }

		 private void PrepareNext()
		 {
			  bool currentReadIsPreparingInconsistent = _state.injectFailureOrMischief( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ), typeof( System.InvalidOperationException ) );
			  _state.reset( currentReadIsPreparingInconsistent );
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
			  _state.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ), typeof( System.InvalidOperationException ) );
			  if ( _state.hasPreparedInconsistentRead() )
			  {
					ResetDelegate();
					return true;
			  }
			  if ( _state.hasInconsistentRead() )
			  {
					ResetDelegate();
					return true;
			  }
			  bool retry = Delegate.shouldRetry();
			  return retry || ( _linkedCursor != null && _linkedCursor.shouldRetry() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void resetDelegate() throws java.io.IOException
		 private void ResetDelegate()
		 {
			  Delegate.shouldRetry();
			  Delegate.Offset = 0;
			  Delegate.checkAndClearBoundsFlag();
			  Delegate.clearCursorException();
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  _state.injectFailure( typeof( System.IndexOutOfRangeException ) );
			  if ( !_state.Inconsistent )
			  {
					while ( targetCursor is DelegatingPageCursor )
					{
						 targetCursor = ( ( DelegatingPageCursor ) targetCursor ).unwrap();
					}
					return Delegate.copyTo( sourceOffset, targetCursor, targetOffset, lengthInBytes );
			  }
			  return lengthInBytes;
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  return Delegate.checkAndClearBoundsFlag() || (_linkedCursor != null && _linkedCursor.checkAndClearBoundsFlag());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.neo4j.io.pagecache.CursorException
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
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor openLinkedCursor(long pageId) throws java.io.IOException
		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  return _linkedCursor = new AdversarialReadPageCursor( Delegate.openLinkedCursor( pageId ), _state );
		 }

		 public override void ZapPage()
		 {
			  throw new System.InvalidOperationException( "Cannot write using read cursor" );
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return Delegate.WriteLocked;
			 }
		 }

		 public override string ToString()
		 {
			  State s = this._state;
			  StringBuilder sb = new StringBuilder();
			  foreach ( object o in s.InconsistentReadHistory )
			  {
					sb.Append( o.ToString() ).Append('\n');
					if ( o is NumberValue )
					{
						 NumberValue v = ( NumberValue ) o;
						 v.PrintStackTrace( sb );
					}
			  }
			  return sb.ToString();
		 }

		 private class NumberValue
		 {
			  internal readonly Type Type;
			  internal readonly long Value;
			  internal readonly int Offset;
			  internal readonly Number InsteadOf;
			  internal Exception Trace;

			  internal NumberValue( Type type, long value, int offset, Number insteadOf )
			  {
					this.Type = type;
					this.Value = value;
					this.Offset = offset;
					this.InsteadOf = insteadOf;
					if ( _enableInconsistencyTracing )
					{
						 Trace = new ExceptionAnonymousInnerClass( this );
					}
			  }

			  private class ExceptionAnonymousInnerClass : Exception
			  {
				  private readonly NumberValue _outerInstance;

				  public ExceptionAnonymousInnerClass( NumberValue outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override string Message
				  {
					  get
					  {
							return _outerInstance.ToString();
					  }
				  }
			  }

			  public override string ToString()
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					string typeName = Type.FullName;
					switch ( typeName )
					{
					case "java.lang.Byte":
						 return "(byte)" + Value + " at offset " + Offset + " (instead of " + InsteadOf + ")";
					case "java.lang.Short":
						 return "(short)" + Value + " at offset " + Offset + " (instead of " + InsteadOf + ")";
					case "java.lang.Integer":
						 return "(int)" + Value + " at offset " + Offset + " (instead of " + InsteadOf + ")";
					case "java.lang.Long":
						 return "(long)" + Value + " at offset " + Offset + " (instead of " + InsteadOf + ")";
					default:
						 return "(" + typeName + ")" + Value + " at offset " + Offset + " (instead of " + InsteadOf + ")";
					}
			  }

			  public virtual void PrintStackTrace( StringBuilder sb )
			  {
					StringWriter w = new StringWriter();
					PrintWriter pw = new PrintWriter( w );
					if ( Trace != null )
					{
						 Trace.printStackTrace( pw );
					}
					pw.flush();
					sb.Append( w );
					sb.Append( '\n' );
			  }
		 }
	}

}
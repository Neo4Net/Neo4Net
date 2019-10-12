using System;
using System.Threading;

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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// Base functionality for having a companion thread reading ahead, prefetching.
	/// </summary>
	public abstract class ThreadAhead : Thread, System.IDisposable
	{
		 // A "long" time to wait is OK since these two threads: the owner and the read-ahead thread
		 // notifies/unparks each other when it's time to continue on anyways
		 private static readonly long _parkTime = MILLISECONDS.toNanos( 100 );

		 private readonly Thread _owner;
		 private volatile bool _hasReadAhead;
		 private volatile bool _closed;
		 private volatile bool _eof;
		 private volatile IOException _ioException;
		 private readonly System.IDisposable _actual;

		 protected internal ThreadAhead( System.IDisposable actual )
		 {
			  this._actual = actual;
			  Name = this.GetType().Name + " for " + actual;
			  this._owner = Thread.CurrentThread;
			  Daemon = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _closed = true;
			  try
			  {
					join();
			  }
			  catch ( InterruptedException e )
			  {
					throw new IOException( e );
			  }
			  finally
			  {
					_actual.Dispose();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void waitUntilReadAhead() throws java.io.IOException
		 protected internal virtual void WaitUntilReadAhead()
		 {
			  AssertHealthy();
			  while ( !_hasReadAhead )
			  {
					ParkAWhile();
					AssertHealthy();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertHealthy() throws java.io.IOException
		 protected internal virtual void AssertHealthy()
		 {
			  if ( _ioException != null )
			  {
					throw new IOException( "Error occured in read-ahead thread", _ioException );
			  }
		 }

		 protected internal virtual void ParkAWhile()
		 {
			  LockSupport.parkNanos( _parkTime );
		 }

		 public override void Run()
		 {
			  while ( !_closed )
			  {
					if ( _hasReadAhead || _eof )
					{ // We have already read ahead, sleep a little
						 ParkAWhile();
					}
					else
					{ // We haven't read ahead, or the data we read ahead have been consumed
						 try
						 {
							  if ( !ReadAhead() )
							  {
									_eof = true;
							  }
							  _hasReadAhead = true;
							  LockSupport.unpark( _owner );
						 }
						 catch ( IOException e )
						 {
							  _ioException = e;
							  _closed = true;
						 }
						 catch ( Exception e )
						 {
							  _ioException = new IOException( e );
							  _closed = true;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract boolean readAhead() throws java.io.IOException;
		 protected internal abstract bool ReadAhead();

		 protected internal virtual void PokeReader()
		 {
			  // wake up the reader... there's stuff to do, data to read
			  _hasReadAhead = false;
			  LockSupport.unpark( this );
		 }
	}

}
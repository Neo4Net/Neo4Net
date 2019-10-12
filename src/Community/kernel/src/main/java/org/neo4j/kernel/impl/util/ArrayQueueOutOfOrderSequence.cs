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
namespace Neo4Net.Kernel.impl.util
{

	/// <summary>
	/// A crude, synchronized implementation of OutOfOrderSequence. Please implement a faster one if need be.
	/// </summary>
	public class ArrayQueueOutOfOrderSequence : OutOfOrderSequence
	{
		 // odd means updating, even means no one is updating
		 private volatile int _version;
		 private volatile long _highestGapFreeNumber;
		 private volatile long[] _highestGapFreeMeta;
		 private readonly SequenceArray _outOfOrderQueue;
		 private long[] _metaArray;
		 private volatile long _highestEverSeen;

		 public ArrayQueueOutOfOrderSequence( long startingNumber, int initialArraySize, long[] initialMeta )
		 {
			  this._highestGapFreeNumber = startingNumber;
			  this._highestEverSeen = startingNumber;
			  this._highestGapFreeMeta = initialMeta.Clone();
			  this._metaArray = initialMeta.Clone();
			  this._outOfOrderQueue = new SequenceArray( initialMeta.Length + 1, initialArraySize );
		 }

		 public override bool Offer( long number, long[] meta )
		 {
			 lock ( this )
			 {
				  _highestEverSeen = Math.Max( _highestEverSeen, number );
				  if ( _highestGapFreeNumber + 1 == number )
				  {
						_version++;
						_highestGapFreeNumber = _outOfOrderQueue.pollHighestGapFree( number, _metaArray );
						_highestGapFreeMeta = _highestGapFreeNumber == number ? meta : _metaArray.Clone();
						_version++;
						Monitor.PulseAll( this );
						return true;
				  }
      
				  _outOfOrderQueue.offer( _highestGapFreeNumber, number, Pack( meta ) );
				  return false;
			 }
		 }

		 public override long HighestEverSeen()
		 {
			  return this._highestEverSeen;
		 }

		 private long[] Pack( long[] meta )
		 {
			  _metaArray = meta;
			  return _metaArray;
		 }

		 public override long[] Get()
		 {
			  long number;
			  long[] meta;
			  while ( true )
			  {
					int versionBefore = _version;
					if ( ( versionBefore & 1 ) == 1 )
					{ // Someone else is updating those values as we speak, go another round
						 continue;
					}

					number = _highestGapFreeNumber;
					meta = _highestGapFreeMeta;
					if ( _version == versionBefore )
					{ // We read a consistent version of these two values
						 break;
					}
			  }

			  return CreateResult( number, meta );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void await(long awaitedNumber, long timeoutMillis) throws java.util.concurrent.TimeoutException, InterruptedException
		 public override void Await( long awaitedNumber, long timeoutMillis )
		 {
			 lock ( this )
			 {
				  long endTime = DateTimeHelper.CurrentUnixTimeMillis() + timeoutMillis;
				  while ( awaitedNumber > _highestGapFreeNumber )
				  {
						long timeLeft = endTime - DateTimeHelper.CurrentUnixTimeMillis();
						if ( timeLeft > 0 )
						{
							 Monitor.Wait( this, TimeSpan.FromMilliseconds( timeLeft ) );
						}
						else
						{
							 throw new TimeoutException( "Awaited number was not reached" );
						}
				  }
			 }
		 }

		 private long[] CreateResult( long number, long[] meta )
		 {
			  long[] result = new long[meta.Length + 1];
			  result[0] = number;
			  Array.Copy( meta, 0, result, 1, meta.Length );
			  return result;
		 }

		 public virtual long HighestGapFreeNumber
		 {
			 get
			 {
				  return _highestGapFreeNumber;
			 }
		 }

		 public override void Set( long number, long[] meta )
		 {
			 lock ( this )
			 {
				  _highestEverSeen = number;
				  _highestGapFreeNumber = number;
				  _highestGapFreeMeta = meta;
				  _outOfOrderQueue.clear();
			 }
		 }

		 public override string ToString()
		 {
			 lock ( this )
			 {
				  return format( "out-of-order-sequence:%d %d [%s]", _highestEverSeen, _highestGapFreeNumber, _outOfOrderQueue );
			 }
		 }
	}

}
using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{

	/// <summary>
	/// For synchronising using wait/notify on a volatile value.
	/// </summary>
	public class VolatileFuture<T>
	{
		 private volatile T _value;

		 public VolatileFuture( T initialValue )
		 {
			  this._value = initialValue;
		 }

		 public virtual void Set( T value )
		 {
			 lock ( this )
			 {
				  if ( this._value != value )
				  {
						this._value = value;
						Monitor.PulseAll( this );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T get(long timeoutMillis, System.Predicate<T> predicate) throws java.util.concurrent.TimeoutException, InterruptedException
		 public virtual T Get( long timeoutMillis, System.Predicate<T> predicate )
		 {
			  T alias = _value;
			  if ( predicate( alias ) )
			  {
					return alias;
			  }

			  if ( timeoutMillis == 0 )
			  {
					throw new TimeoutException();
			  }

			  return WaitForValue( timeoutMillis + DateTimeHelper.CurrentUnixTimeMillis(), predicate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized T waitForValue(long endTimeMillis, System.Predicate<T> predicate) throws InterruptedException, java.util.concurrent.TimeoutException
		 private T WaitForValue( long endTimeMillis, System.Predicate<T> predicate )
		 {
			 lock ( this )
			 {
				  T alias;
				  while ( !predicate( alias = _value ) )
				  {
						long timeLeft = endTimeMillis - DateTimeHelper.CurrentUnixTimeMillis();
						if ( timeLeft > 0 )
						{
							 Monitor.Wait( this, TimeSpan.FromMilliseconds( timeLeft ) );
						}
						else
						{
							 throw new TimeoutException();
						}
				  }
      
				  return alias;
			 }
		 }
	}

}
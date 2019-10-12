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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	/// <summary>
	/// In a moving average calculation, only the last N values are considered.
	/// </summary>
	public class MovingAverage
	{
		 private readonly AtomicLongArray _values;
		 private readonly AtomicLong _total = new AtomicLong();
		 private readonly AtomicLong _valueCursor = new AtomicLong();

		 public MovingAverage( int numberOfTrackedValues )
		 {
			  this._values = new AtomicLongArray( numberOfTrackedValues );
		 }

		 public virtual void Add( long value )
		 {
			  long cursor = _valueCursor.AndIncrement;
			  long prevValue = _values.getAndSet( ( int )( cursor % _values.length() ), value );
			  _total.addAndGet( value - prevValue );
		 }

		 private int NumberOfCurrentlyTrackedValues()
		 {
			  return ( int ) min( _valueCursor.get(), _values.length() );
		 }

		 public virtual long Total()
		 {
			  return _total.get();
		 }

		 public virtual long Average()
		 {
			  int trackedValues = NumberOfCurrentlyTrackedValues();
			  return trackedValues > 0 ? _total.get() / trackedValues : 0;
		 }

		 public virtual void Reset()
		 {
			  for ( int i = 0; i < _values.length(); i++ )
			  {
					_values.set( i, 0 );
			  }
			  _total.set( 0 );
			  _valueCursor.set( 0 );
		 }
	}

}
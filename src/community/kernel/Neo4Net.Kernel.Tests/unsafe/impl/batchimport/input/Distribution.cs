using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using RandomValues = Neo4Net.Values.Storable.RandomValues;

	/// <summary>
	/// Distributes the given items so that item[0] converges towards being returned 1/2 of the times,
	/// the next item, item[1] 1/4 of the times, item[2] 1/8 and so on.
	/// </summary>
	public class Distribution<T>
	{
		 private readonly T[] _items;

		 public Distribution( T[] items )
		 {
			  this._items = items;
		 }

		 public virtual int Length()
		 {
			  return _items.Length;
		 }

		 public virtual T Random( Random random )
		 {
			  float value = random.nextFloat();
			  float comparison = 0.5f;
			  foreach ( T item in _items )
			  {
					if ( value >= comparison )
					{
						 return item;
					}
					comparison /= 2f;
			  }
			  return _items[_items.Length - 1];
		 }

		 public virtual T Random( RandomValues random )
		 {
			  float value = random.NextFloat();
			  float comparison = 0.5f;
			  foreach ( T item in _items )
			  {
					if ( value >= comparison )
					{
						 return item;
					}
					comparison /= 2f;
			  }
			  return _items[_items.Length - 1];
		 }
	}

}
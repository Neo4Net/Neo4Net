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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Org.Neo4j.@unsafe.Impl.Batchimport.cache;

	/// <summary>
	/// Base implementation of <seealso cref="Tracker"/> over a <seealso cref="NumberArray"/>.
	/// </summary>
	/// @param <ARRAY> type of <seealso cref="NumberArray"/> in this implementation. </param>
	internal abstract class AbstractTracker<ARRAY> : Tracker where ARRAY : Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArray
	{
		public abstract bool IsMarkedAsDuplicate( long index );
		public abstract void MarkAsDuplicate( long index );
		public abstract void Set( long index, long value );
		public abstract long Get( long index );
		 protected internal ARRAY Array;

		 protected internal AbstractTracker( ARRAY array )
		 {
			  this.Array = array;
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  Array.acceptMemoryStatsVisitor( visitor );
		 }

		 public override void Swap( long fromIndex, long toIndex )
		 {
			  Array.swap( fromIndex, toIndex );
		 }

		 public override void Close()
		 {
			  Array.close();
		 }
	}

}
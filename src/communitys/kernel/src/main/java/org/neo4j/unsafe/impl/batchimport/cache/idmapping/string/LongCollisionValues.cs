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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

	/// <summary>
	/// Stores longs in a <seealso cref="LongArray"/> provided by <seealso cref="NumberArrayFactory"/>.
	/// </summary>
	public class LongCollisionValues : CollisionValues
	{
		 private readonly LongArray _cache;
		 private long _nextOffset;

		 public LongCollisionValues( NumberArrayFactory factory, long length )
		 {
			  _cache = factory.NewLongArray( length, 0 );
		 }

		 public override long Add( object id )
		 {
			  long collisionIndex = _nextOffset++;
			  _cache.set( collisionIndex, ( ( Number )id ).longValue() );
			  return collisionIndex;
		 }

		 public override object Get( long offset )
		 {
			  return _cache.get( offset );
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  _cache.acceptMemoryStatsVisitor( visitor );
		 }

		 public override void Close()
		 {
			  _cache.close();
		 }
	}

}
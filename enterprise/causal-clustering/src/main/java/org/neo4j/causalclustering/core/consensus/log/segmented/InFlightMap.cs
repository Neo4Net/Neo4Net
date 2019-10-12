using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{

	public class InFlightMap<V>
	{
		 private readonly SortedDictionary<long, V> _map = new ConcurrentSkipListMap<long, V>();
		 private volatile bool _enabled;

		 public InFlightMap() : this(false)
		 {
		 }

		 public InFlightMap( bool enabled )
		 {
			  this._enabled = enabled;
		 }

		 public virtual void Enable()
		 {
			  this._enabled = true;
		 }

		 /// <summary>
		 /// Adds a new mapping.
		 /// </summary>
		 /// <param name="key"> The key of the mapping </param>
		 /// <param name="value"> The value corresponding to the key provided. </param>
		 /// <exception cref="IllegalArgumentException"> if a mapping for the key already exists </exception>
		 public virtual void Put( long? key, V value )
		 {
			  if ( !_enabled )
			  {
					return;
			  }

			  V previousValue = _map.putIfAbsent( key, value );

			  if ( previousValue != default( V ) )
			  {
					throw new System.ArgumentException( format( "Attempted to register an already seen value to the log entry cache. " + "Key: %s; New Value: %s; Previous Value: %s", key, value, previousValue ) );
			  }
		 }

		 /// <summary>
		 /// Returns the mapped value for this key or null if the key has not been registered. </summary>
		 /// <param name="key"> The key to use for retrieving the value from the map </param>
		 /// <returns> the value for this key, otherwise null. </returns>
		 public virtual V Get( long? key )
		 {
			  return _map[key];
		 }

		 /// <summary>
		 /// Attempts to remove this object from the map.
		 /// </summary>
		 /// <param name="key"> The object to attempt unregistering. </param>
		 /// <returns> true if the attempt to unregister was successful, otherwise false if this object was not found. </returns>
		 public virtual bool Remove( long? key )
		 {
			  return _map.Remove( key ) != null;
		 }

		 /// <summary>
		 /// Attempts to remove all objects at this key or higher from the map.
		 /// </summary>
		 /// <param name="key"> The object to attempt unregistering. </param>
		 public virtual void Truncate( long? key )
		 {
			  _map.tailMap( key ).Keys.forEach( _map.remove );
		 }

		 public override string ToString()
		 {
			  return string.Format( "InFlightMap{{map={0}}}", _map );
		 }
	}

}
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
namespace Neo4Net.Index.lucene
{
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.index;

	/// <summary>
	/// A utility for ordering nodes or relationships in a timeline. Entities are added
	/// to the timeline and then queried given a time period, w/ or w/o lower/upper
	/// bounds, for example "Give me all entities before this given timestamp" or
	/// "Give me all nodes between these two timestamps".
	/// 
	/// Please note that the timestamps don't need to represent actual points in
	/// time, any <code>long</code> that identifies the indexed <seealso cref="Node"/> or
	/// <seealso cref="Relationship"/> and defines its global order is fine.
	/// </summary>
	/// @deprecated This will be removed in the next major release. Please consider using schema indexes on date/time valued properties instead. 
	[Obsolete("This will be removed in the next major release. Please consider using schema indexes on date/time valued properties instead.")]
	public interface TimelineIndex<T> where T : Neo4Net.GraphDb.PropertyContainer
	{
		 /// <returns> the last IEntity in the timeline, that is the IEntity with the highest
		 /// timestamp or {@code null} if the timeline is empty. </returns>
		 [Obsolete]
		 T Last { get; }

		 /// <returns> the first IEntity in the timeline, that is the IEntity with the lowest
		 /// timestamp or {@code null} if the timeline is empty. </returns>
		 [Obsolete]
		 T First { get; }

		 /// <summary>
		 /// Removes an IEntity from the timeline. The timestamp should be the same
		 /// as when it was added.
		 /// </summary>
		 /// <param name="entity"> the IEntity to remove from this timeline. </param>
		 /// <param name="timestamp"> the timestamp this IEntity was added with. </param>
		 [Obsolete]
		 void Remove( T IEntity, long timestamp );

		 /// <summary>
		 /// Adds an IEntity to this timeline with the given {@code timestamp}.
		 /// </summary>
		 /// <param name="entity"> the IEntity to add to this timeline. </param>
		 /// <param name="timestamp"> the timestamp to use. </param>
		 [Obsolete]
		 void Add( T IEntity, long timestamp );

		 /// <summary>
		 /// Query the timeline with optional lower/upper bounds and get back
		 /// entities within that range, ordered by date. If {@code reversed} is
		 /// {@code true} the order of the result is reversed.
		 /// </summary>
		 /// <param name="startTimestampOrNull"> the start timestamp, entities with greater
		 /// timestamp value will be returned (exclusive). Will be ignored if {@code null}. </param>
		 /// <param name="endTimestampOrNull"> the end timestamp, entities with lesser timestamp </param>
		 /// <param name="reversed"> reverses the result order if {@code true}.
		 /// value will be returned (exclude). Will be ignored if {@code null}. </param>
		 /// <returns> all entities within the given boundaries in this timeline, ordered
		 /// by timestamp. </returns>
		 [Obsolete]
		 IndexHits<T> GetBetween( long? startTimestampOrNull, long? endTimestampOrNull, bool reversed );

		 /// <summary>
		 /// Query the timeline with optional lower/upper bounds and get back
		 /// entities within that range, ordered by date with lowest first.
		 /// </summary>
		 /// <param name="startTimestampOrNull"> the start timestamp, entities with greater
		 /// timestamp value will be returned (exclusive). Will be ignored if {@code null}. </param>
		 /// <param name="endTimestampOrNull"> the end timestamp, entities with lesser timestamp
		 /// value will be returned (exclude). Will be ignored if {@code null}. </param>
		 /// <returns> all entities within the given boundaries in this timeline, ordered
		 /// by timestamp. </returns>
		 [Obsolete]
		 IndexHits<T> GetBetween( long? startTimestampOrNull, long? endTimestampOrNull );
	}

}
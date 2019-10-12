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
namespace Neo4Net.Index.@internal.gbptree
{
	/// <summary>
	/// Decides what to do when inserting key which already exists in index. Different implementations of
	/// <seealso cref="ValueMerger"/> can result in unique/non-unique indexes for example.
	/// </summary>
	/// @param <KEY> type of keys to merge. </param>
	/// @param <VALUE> type of values to merge. </param>
	public interface ValueMerger<KEY, VALUE>
	{
		 /// <summary>
		 /// Merge an existing value with a new value, returning potentially a combination of the two, or {@code null}
		 /// if no merge was done effectively meaning that nothing should be written.
		 /// </summary>
		 /// <param name="existingKey"> existing key </param>
		 /// <param name="newKey"> new key </param>
		 /// <param name="existingValue"> existing value </param>
		 /// <param name="newValue"> new value </param>
		 /// <returns> {@code newValue}, now merged with {@code existingValue}, or {@code null} if no merge was done. </returns>
		 VALUE Merge( KEY existingKey, KEY newKey, VALUE existingValue, VALUE newValue );
	}

}
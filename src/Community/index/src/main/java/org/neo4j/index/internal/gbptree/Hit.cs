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
	/// Represents a hit during an <seealso cref="GBPTree.seek(object, object)"/>. There's no guarantee about whether or
	/// not the instances returned from <seealso cref="key()"/> and <seealso cref="value()"/> are immutable, so if multiple keys/values
	/// are stored temporarily during the seek then it's recommended to take copies of them.
	/// </summary>
	/// @param <KEY> type of keys </param>
	/// @param <VALUE> type of values </param>
	public interface Hit<KEY, VALUE>
	{
		 /// <returns> key of this hit. This returned key instance shouldn't be held on to, rather its contents,
		 /// because key instances can be mutable and change within a result set to be overwritten with the next hit. </returns>
		 KEY Key();

		 /// <returns> value of this hit. This returned value instance shouldn't be held on to, rather its contents,
		 /// because value instances can be mutable and change within a result set to be overwritten with the next hit. </returns>
		 VALUE Value();
	}

}
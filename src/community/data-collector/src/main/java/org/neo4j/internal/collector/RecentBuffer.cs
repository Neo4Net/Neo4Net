﻿/*
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
namespace Neo4Net.Internal.Collector
{

	/// <summary>
	/// Bounded buffer which holds the last n elements. When the buffer is full, each
	/// produce will replace the elements in the buffer that was added first.
	/// 
	/// This collection thread-safely allows
	///  - multiple threads concurrently calling `produce`
	///  - serialized calling of `clear` and `foreach`
	/// </summary>
	/// @param <T> type of elements in this buffer. </param>
	public interface RecentBuffer<T>
	{
		 /// <summary>
		 /// Produce element into the buffer.
		 /// </summary>
		 /// <param name="t"> element to produce </param>
		 void Produce( T t );

		 /// <summary>
		 /// Clear all elements from the buffer.
		 /// </summary>
		 void Clear();

		 /// <summary>
		 /// Iterate over all elements in the buffer. No elements are removed from the buffer.
		 /// </summary>
		 /// <param name="consumer"> consumer to apply on each element </param>
		 void Foreach( System.Action<T> consumer );
	}

}
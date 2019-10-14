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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

	/// <summary>
	/// Generic processor of <seealso cref="AbstractBaseRecord"/> from a store.
	/// </summary>
	/// @param <T> </param>
	public interface RecordProcessor<T> : AutoCloseable where T : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 /// <summary>
		 /// Processes an item.
		 /// </summary>
		 /// <returns> {@code true} if processing this item resulted in changes that should be updated back to the source. </returns>
		 bool Process( T item );

		 void Done();

		 void Close();
	}

}
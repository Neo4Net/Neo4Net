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
namespace Neo4Net.Kernel.impl.store
{
	/// <summary>
	/// Calculates page ids and offset based on record ids.
	/// </summary>
	public class RecordPageLocationCalculator
	{
		 private RecordPageLocationCalculator()
		 {
		 }

		 /// <summary>
		 /// Calculates which page a record with the given {@code id} should go into.
		 /// </summary>
		 /// <param name="id"> record id </param>
		 /// <param name="pageSize"> size of each page </param>
		 /// <param name="recordSize"> size of each record </param>
		 /// <returns> which page the record with the given {@code id} should go into, given the
		 /// {@code pageSize} and {@code recordSize}. </returns>
		 public static long PageIdForRecord( long id, int pageSize, int recordSize )
		 {
			  return id * recordSize / pageSize;
		 }

		 /// <summary>
		 /// Calculates which offset into the right page (had by <seealso cref="pageIdForRecord(long, int, int)"/>)
		 /// the given {@code id} lives at.
		 /// </summary>
		 /// <param name="id"> record id </param>
		 /// <param name="pageSize"> size of each page </param>
		 /// <param name="recordSize"> size of each record </param>
		 /// <returns> which offset into the right page the given {@code id} lives at, given the
		 /// {@code pageSize} and {@code recordSize}. </returns>
		 public static int OffsetForId( long id, int pageSize, int recordSize )
		 {
			  return ( int )( ( id * recordSize ) % pageSize );
		 }
	}

}
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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	/// <summary>
	/// Manages deserialization of one or more entry values, together forming some sort of entity.
	/// It has mutable state and the usage pattern should be:
	/// 
	/// <ol>
	/// <li>One or more calls to <seealso cref="handle(org.neo4j.unsafe.impl.batchimport.input.csv.Header.Entry, object)"/></li>
	/// <li><seealso cref="materialize()"/> to materialize the entity from the handled values</li>
	/// <li><seealso cref="clear()"/> to prepare for the next entity</li>
	/// </ol>
	/// </summary>
	public interface Deserialization<ENTITY>
	{

		 /// <summary>
		 /// Handles one value of a type described by the {@code entry}. One or more values will be able to
		 /// <seealso cref="materialize()"/> into an entity later on.
		 /// </summary>
		 void Handle( Header.Entry entry, object value );

		 /// <summary>
		 /// Takes values received in <seealso cref="handle(org.neo4j.unsafe.impl.batchimport.input.csv.Header.Entry, object)"/>
		 /// and materializes an entity from them.
		 /// </summary>
		 ENTITY Materialize();

		 /// <summary>
		 /// Clears the mutable state, preparing for the next entity.
		 /// </summary>
		 void Clear();
	}

}
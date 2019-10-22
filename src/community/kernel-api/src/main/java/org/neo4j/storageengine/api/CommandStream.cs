using System.Collections.Generic;

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
namespace Neo4Net.Storageengine.Api
{

	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// A stream of commands from one or more transactions, that can be serialized to a transaction log or applied to a
	/// store.
	/// </summary>
	public interface CommandStream : IEnumerable<StorageCommand>
	{
		 /// <summary>
		 /// Accepts a visitor into the commands making up this transaction. </summary>
		 /// <param name="visitor"> <seealso cref="Visitor"/> which will see the commands. </param>
		 /// <returns> {@code true} if any <seealso cref="StorageCommand"/> visited returned {@code true}, otherwise {@code false}. </returns>
		 /// <exception cref="IOException"> if there were any problem reading the commands. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean accept(org.Neo4Net.helpers.collection.Visitor<StorageCommand,java.io.IOException> visitor) throws java.io.IOException;
		 bool Accept( Visitor<StorageCommand, IOException> visitor );
	}

}
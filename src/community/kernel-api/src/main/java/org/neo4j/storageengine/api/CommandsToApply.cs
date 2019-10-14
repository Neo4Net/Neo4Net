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
	/// <summary>
	/// Group of commands to apply onto <seealso cref="StorageEngine"/>, as well as reference to <seealso cref="next()"/> group of commands.
	/// The linked list will form a batch.
	/// </summary>
	public interface CommandsToApply : CommandStream
	{
		 /// <returns> transaction id representing this group of commands. </returns>
		 long TransactionId();

		 /// <returns> next group of commands in this batch. </returns>
		 CommandsToApply Next();

		 /// <returns> {@code true} if applying this group of commands requires that any group chronologically
		 /// before it also needing ordering have been fully applied. This is a way to force serial application
		 /// of some groups and in extension their whole batches. </returns>
		 bool RequiresApplicationOrdering();

		 /// <returns> A string describing the contents of this batch of commands. </returns>
		 String ();
	}

}
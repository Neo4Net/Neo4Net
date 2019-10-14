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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;

	/// <summary>
	/// Maps input node ids as specified by data read into <seealso cref="InputEntityVisitor"/> into actual node ids.
	/// </summary>
	public interface IdMapper : Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable, AutoCloseable
	{

		 /// <summary>
		 /// Maps an {@code inputId} to an actual node id. </summary>
		 /// <param name="inputId"> an id of an unknown type, coming from input. </param>
		 /// <param name="actualId"> the actual node id that the inputId will represent. </param>
		 /// <param name="group"> <seealso cref="Group"/> this input id will be added to. Used for handling input ids collisions
		 /// where multiple equal input ids might be added, as long as all input ids within a single group is unique.
		 /// Group ids are also passed into <seealso cref="get(object, Group)"/>.
		 /// It is required that all input ids belonging to a specific group are put in sequence before putting any
		 /// input ids for another group. </param>
		 void Put( object inputId, long actualId, Group group );

		 /// <returns> whether or not a call to <seealso cref="prepare(LongFunction, Collector, ProgressListener)"/> needs to commence after all calls to
		 /// <seealso cref="put(object, long, Group)"/> and before any call to <seealso cref="get(object, Group)"/>. I.e. whether or not all ids
		 /// needs to be put before making any call to <seealso cref="get(object, Group)"/>. </returns>
		 bool NeedsPreparation();

		 /// <summary>
		 /// After all mappings have been <seealso cref="put(object, long, Group)"/> call this method to prepare for
		 /// <seealso cref="get(object, Group)"/>.
		 /// </summary>
		 /// <param name="inputIdLookup"> can return input id of supplied node id. Used in the event of difficult collisions
		 /// so that more information have to be read from the input data again, data that normally isn't necessary
		 /// and hence discarded. </param>
		 /// <param name="collector"> <seealso cref="Collector"/> for bad entries, such as duplicate node ids. </param>
		 /// <param name="progress"> reports preparation progress. </param>
		 void Prepare( System.Func<long, object> inputIdLookup, Collector collector, ProgressListener progress );

		 /// <summary>
		 /// Returns an actual node id representing {@code inputId}.
		 /// For this call to work <seealso cref="prepare(LongFunction, Collector, ProgressListener)"/> must have
		 /// been called after all calls to <seealso cref="put(object, long, Group)"/> have been made,
		 /// iff <seealso cref="needsPreparation()"/> returns {@code true}. Otherwise ids can be retrieved right after
		 /// <seealso cref="put(object, long, Group) being put"/>
		 /// </summary>
		 /// <param name="inputId"> the input id to get the actual node id for. </param>
		 /// <param name="group"> <seealso cref="Group"/> the given {@code inputId} must exist in, i.e. have been put with. </param>
		 /// <returns> the actual node id previously specified by <seealso cref="put(object, long, Group)"/>, or {@code -1} if not found. </returns>
		 long Get( object inputId, Group group );

		 /// <summary>
		 /// Releases all resources used by this <seealso cref="IdMapper"/>.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Returns instance capable of returning memory usage estimation of given {@code numberOfNodes}.
		 /// </summary>
		 /// <param name="numberOfNodes"> number of nodes to calculate memory for. </param>
		 /// <returns> instance capable of calculating memory usage for the given number of nodes. </returns>
		 Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable MemoryEstimation( long numberOfNodes );

		 LongIterator LeftOverDuplicateNodesIds();
	}

	public static class IdMapper_Fields
	{
		 public const long ID_NOT_FOUND = -1;
	}

}
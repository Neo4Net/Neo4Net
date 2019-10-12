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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;

	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using EncodingIdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.EncodingIdMapper;
	using LongCollisionValues = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.LongCollisionValues;
	using LongEncoder = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.LongEncoder;
	using Radix = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.Radix;
	using StringCollisionValues = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.StringCollisionValues;
	using StringEncoder = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.StringEncoder;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using Groups = Neo4Net.@unsafe.Impl.Batchimport.input.Groups;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.@string.EncodingIdMapper.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.@string.TrackerFactories.dynamic;

	/// <summary>
	/// Place to instantiate common <seealso cref="IdMapper"/> implementations.
	/// </summary>
	public class IdMappers
	{
		 private class ActualIdMapper : IdMapper
		 {
			  public override void Put( object inputId, long actualId, Group group )
			  { // No need to remember anything
			  }

			  public override bool NeedsPreparation()
			  {
					return false;
			  }

			  public override void Prepare( System.Func<long, object> inputIdLookup, Collector collector, ProgressListener progress )
			  { // No need to prepare anything
			  }

			  public override long Get( object inputId, Group group )
			  {
					return ( long? ) inputId.Value;
			  }

			  public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
			  { // No memory usage
			  }

			  public override string ToString()
			  {
					return this.GetType().Name;
			  }

			  public override void Close()
			  { // Nothing to close
			  }

			  public override Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable MemoryEstimation( long numberOfNodes )
			  {
					return Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Fields.None;
			  }

			  public override LongIterator LeftOverDuplicateNodesIds()
			  {
					return ImmutableEmptyLongIterator.INSTANCE;
			  }
		 }

		 private IdMappers()
		 {
		 }

		 /// <summary>
		 /// An <seealso cref="IdMapper"/> that doesn't touch the input ids, but just asserts that node ids arrive in ascending order.
		 /// This is for advanced usage and puts constraints on the input in that all node ids given as input
		 /// must be valid. There will not be further checks, other than that for order of the ids.
		 /// </summary>
		 public static IdMapper Actual()
		 {
			  return new ActualIdMapper();
		 }

		 /// <summary>
		 /// An <seealso cref="IdMapper"/> capable of mapping <seealso cref="string strings"/> to long ids.
		 /// </summary>
		 /// <param name="cacheFactory"> <seealso cref="NumberArrayFactory"/> for allocating memory for the cache used by this index. </param>
		 /// <param name="groups"> <seealso cref="Groups"/> containing all id groups. </param>
		 /// <returns> <seealso cref="IdMapper"/> for when input ids are strings. </returns>
		 public static IdMapper Strings( NumberArrayFactory cacheFactory, Groups groups )
		 {
			  return new EncodingIdMapper( cacheFactory, new StringEncoder(), Radix.STRING, NO_MONITOR, dynamic(), groups, numberOfCollisions => new StringCollisionValues(cacheFactory, numberOfCollisions) );
		 }

		 /// <summary>
		 /// An <seealso cref="IdMapper"/> capable of mapping <seealso cref="Long arbitrary longs"/> to long ids.
		 /// </summary>
		 /// <param name="cacheFactory"> <seealso cref="NumberArrayFactory"/> for allocating memory for the cache used by this index. </param>
		 /// <param name="groups"> <seealso cref="Groups"/> containing all id groups. </param>
		 /// <returns> <seealso cref="IdMapper"/> for when input ids are numbers. </returns>
		 public static IdMapper Longs( NumberArrayFactory cacheFactory, Groups groups )
		 {
			  return new EncodingIdMapper( cacheFactory, new LongEncoder(), Radix.LONG, NO_MONITOR, dynamic(), groups, numberOfCollisions => new LongCollisionValues(cacheFactory, numberOfCollisions) );
		 }
	}

}
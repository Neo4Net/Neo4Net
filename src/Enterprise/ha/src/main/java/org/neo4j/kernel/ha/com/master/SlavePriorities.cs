using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.ha.com.master
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Neo4Net.Helpers.Collection;

	/// <summary>
	/// Factory for common <seealso cref="SlavePriority"/> implementations.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public abstract class SlavePriorities
	{
		 // Purely a factory.
		 private SlavePriorities()
		 {
		 }

		 /// <returns> <seealso cref="SlavePriority"/> which returns the slaves in the order that
		 /// they are given in the {@code slaves} array. </returns>
		 public static SlavePriority GivenOrder()
		 {
			  return slaves => slaves;
		 }

		 /// <returns> <seealso cref="SlavePriority"/> which returns the slaves in a round robin
		 /// fashion, more precisely the start index in the array increments with
		 /// each <seealso cref="SlavePriority.prioritize(System.Collections.IEnumerable) prioritization"/>, ordered
		 /// by server id in ascending. </returns>
		 public static SlavePriority RoundRobin()
		 {
			  return new SlavePriorityAnonymousInnerClass();
		 }

		 private class SlavePriorityAnonymousInnerClass : SlavePriority
		 {
			 internal readonly AtomicInteger index = new AtomicInteger();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<Slave> prioritize(final Iterable<Slave> slaves)
			 public IEnumerable<Slave> prioritize( IEnumerable<Slave> slaves )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Slave> slaveList = sortSlaves(slaves, true);
				  IList<Slave> slaveList = SortSlaves( slaves, true );
				  if ( slaveList.Count == 0 )
				  {
						return Iterables.empty();
				  }
				  return () => new PrefetchingIteratorAnonymousInnerClass(this, slaveList);
			 }

			 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Slave>
			 {
				 private readonly SlavePriorityAnonymousInnerClass _outerInstance;

				 private IList<Slave> _slaveList;

				 public PrefetchingIteratorAnonymousInnerClass( SlavePriorityAnonymousInnerClass outerInstance, IList<Slave> slaveList )
				 {
					 this.outerInstance = outerInstance;
					 this._slaveList = slaveList;
					 start = index.AndIncrement % slaveList.Count;
				 }

				 private int start;
				 private int count;

				 protected internal override Slave fetchNextOrNull()
				 {
					  int id = count++;
					  return id <= _slaveList.Count ? _slaveList[( start + id ) % _slaveList.Count] : null;
				 }
			 }
		 }

		 /// <returns> <seealso cref="SlavePriority"/> which returns the slaves in the same fixed order
		 /// sorted by server id in descending order. </returns>
		 public static SlavePriority FixedDescending()
		 {
			  return slaves => SortSlaves( slaves, false );
		 }

		 /// <returns> <seealso cref="SlavePriority"/> which returns the slaves in the same fixed order
		 /// sorted by server id in ascending order. This is generally preferable to <seealso cref="fixedDescending()"/>,
		 /// because this aligns with the tie-breaker aspect of the lowest server id becoming master.
		 /// <para>
		 /// Eg. if you want to ensure that failover most likely will happen in a specific datacenter,
		 /// you would place low-id instances in that datacenter and choose this strategy. That way,
		 /// most of the time the most up-to-date instance will be in this data center, and if there is
		 /// a tie, the tie-break will also end up electing a master in the same data center.
		 /// </para>
		 /// <para>
		 /// This is in contrast to <seealso cref="fixedDescending()"/>, where a high-id server is likely to be most
		 /// up-to-date if the master fails, but a low-id server is likely to be elected if there is a tie.
		 /// This makes the two scenarios consistently choose a low-id server as the new master. </returns>
		 public static SlavePriority FixedAscending()
		 {
			  return slaves => SortSlaves( slaves, true );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static java.util.List<Slave> sortSlaves(final Iterable<Slave> slaves, boolean asc)
		 private static IList<Slave> SortSlaves( IEnumerable<Slave> slaves, bool asc )
		 {
			  List<Slave> slaveList = Iterables.addAll( new List<Slave>(), slaves );
			  slaveList.sort( asc ? _serverIdComparator : _reverseServerIdComparator );
			  return slaveList;
		 }

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IComparer<Slave> _serverIdComparator = System.Collections.IComparer.comparingInt( Slave::getServerId );

		 private static readonly IComparer<Slave> _reverseServerIdComparator = reverseOrder( _serverIdComparator );
	}

}
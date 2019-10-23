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
namespace Neo4Net.Kernel.Api.StorageEngine.TxState
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;

	/// <summary>
	/// Read only variant of specialised primitive longs collection that with given a sequence of add
	/// and removal operations, tracks which elements need to actually be added and removed at minimum from some
	/// target collection such that the result is equivalent to just
	/// executing the sequence of additions and removals in order.
	/// </summary>
	public interface LongDiffSets
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LongDiffSets EMPTY = new LongDiffSets()
	//	 {
	//		  @@Override public boolean isAdded(long element)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public boolean isRemoved(long element)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public LongSet getAdded()
	//		  {
	//				return LongSets.immutable.empty();
	//		  }
	//
	//		  @@Override public LongSet getRemoved()
	//		  {
	//				return LongSets.immutable.empty();
	//		  }
	//
	//		  @@Override public boolean isEmpty()
	//		  {
	//				return true;
	//		  }
	//
	//		  @@Override public int delta()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public LongIterator augment(LongIterator elements)
	//		  {
	//				return elements;
	//		  }
	//
	//		  @@Override public PrimitiveLongResourceIterator augment(PrimitiveLongResourceIterator elements)
	//		  {
	//				return elements;
	//		  }
	//	 };

		 /// <summary>
		 /// Check if provided element added in this collection </summary>
		 /// <param name="element"> element to check </param>
		 /// <returns> true if added, false otherwise </returns>
		 bool IsAdded( long element );

		 /// <summary>
		 /// Check if provided element is removed in this collection </summary>
		 /// <param name="element"> element to check </param>
		 /// <returns> true if removed, false otherwise </returns>
		 bool IsRemoved( long element );

		 /// <summary>
		 /// All elements that added into this collection </summary>
		 /// <returns> all added elements </returns>
		 LongSet Added { get; }

		 /// <summary>
		 /// All elements that are removed according to underlying collection </summary>
		 /// <returns> all removed elements </returns>
		 LongSet Removed { get; }

		 /// <summary>
		 /// Check if underlying diff set is empty </summary>
		 /// <returns> true if there is no added and removed elements, false otherwise </returns>
		 bool Empty { get; }

		 /// <summary>
		 /// Difference between number of added and removed elements </summary>
		 /// <returns> difference between number of added and removed elements </returns>
		 int Delta();

		 /// <summary>
		 /// Augment current diff sets with elements. Provided element will be augmented if diffset
		 /// does not remove and add that specific element. </summary>
		 /// <param name="elements"> elements to augment with </param>
		 /// <returns> iterator that will iterate over augmented elements as well as over diff set </returns>
		 LongIterator Augment( LongIterator elements );

		 /// <summary>
		 /// Augment current diff sets with elements. Provided element will be augmented if diffset
		 /// does not remove and add that specific element.
		 /// </summary>
		 /// <param name="elements"> elements to augment with </param>
		 /// <returns> iterator that will iterate over augmented elements as well as over diff set </returns>
		 PrimitiveLongResourceIterator Augment( PrimitiveLongResourceIterator elements );
	}

}
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
namespace Neo4Net.Kernel.impl.util
{

	/// <summary>
	/// The thinking behind an out-of-order sequence is that, to the outside, there's one "last number"
	/// which will never be decremented between times of looking at it. It can move in bigger strides
	/// than 1 though. That is because multiple threads can <seealso cref="offer(long, long[]) tell"/> it that a certain number is
	/// "done",
	/// a number that not necessarily is the previously last one plus one. So if a gap is observed then the number
	/// that is the logical next one, whenever that arrives, will move the externally visible number to
	/// the highest gap-free number set.
	/// </summary>
	public interface OutOfOrderSequence
	{
		 /// <summary>
		 /// Offers a number to this sequence.
		 /// </summary>
		 /// <param name="number"> number to offer this sequence </param>
		 /// <param name="meta"> meta data about the number </param>
		 /// <returns> {@code true} if highest gap-free number changed as part of this call, otherwise {@code false}. </returns>
		 bool Offer( long number, long[] meta );

		 /// <returns> the highest number, without its meta data. </returns>
		 long HighestEverSeen();

		 /// <returns> {@code long[]} with the highest offered gap-free number and its meta data. </returns>
		 long[] Get();

		 /// <summary>
		 /// Waits for the specified number (gap-free).
		 /// </summary>
		 /// <param name="awaitedNumber"> the awaited number. </param>
		 /// <param name="timeoutMillis"> the maximum time to wait in milliseconds. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void await(long awaitedNumber, long timeoutMillis) throws java.util.concurrent.TimeoutException, InterruptedException;
		 void Await( long awaitedNumber, long timeoutMillis );

		 /// <returns> the highest gap-free number, without its meta data. </returns>
		 long HighestGapFreeNumber { get; }

		 /// <summary>
		 /// Used in recovery. I don't like the visibility of this method at all.
		 /// </summary>
		 void Set( long number, long[] meta );
	}

}
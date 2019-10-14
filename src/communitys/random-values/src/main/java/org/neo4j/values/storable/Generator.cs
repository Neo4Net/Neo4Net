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
namespace Neo4Net.Values.Storable
{
	/// <summary>
	/// This class is meant as a gap so that we don't need a direct dependency on a random number generator.
	/// <para>
	/// For example by wrapping in a generator we can support both {@code java.util.Random} and {@code java.util
	/// .SplittableRandom}
	/// </para>
	/// </summary>
	public interface Generator
	{
		 /// <summary>
		 /// Return a pseudorandom normally distributed long </summary>
		 /// <returns> a pseudorandom normally distributed long </returns>
		 long NextLong();

		 /// <summary>
		 /// Return a pseudorandom normally distributed boolean </summary>
		 /// <returns> a pseudorandom normally distributed boolean </returns>
		 bool NextBoolean();

		 /// <summary>
		 /// Return a pseudorandom normally distributed int </summary>
		 /// <returns> a pseudorandom normally distributed int </returns>
		 int NextInt();

		 /// <summary>
		 /// Return a pseudorandom normally distributed long between 0 (inclusive) and the given bound(exlusive) </summary>
		 /// <param name="bound"> the exclusive upper bound for the number generation </param>
		 /// <returns> a pseudorandom normally distributed int </returns>
		 int NextInt( int bound );

		 /// <summary>
		 /// Return a pseudorandom normally distributed float from {@code 0.0f} (inclusive) to {@code 1.0f} (exclusive) </summary>
		 /// <returns> a pseudorandom normally distributed from {@code 0.0f} (inclusive) to {@code 1.0f} (exclusive) </returns>
		 float NextFloat();

		 /// <summary>
		 /// Return a pseudorandom normally distributed double from {@code 0.0} (inclusive) to {@code 1.0} (exclusive) </summary>
		 /// <returns> a pseudorandom normally distributed double from {@code 0.0} (inclusive) to {@code 1.0} (exclusive) </returns>
		 double NextDouble();
	}

}
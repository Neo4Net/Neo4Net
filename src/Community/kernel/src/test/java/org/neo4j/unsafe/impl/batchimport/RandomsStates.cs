using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using RandomValues = Neo4Net.Values.Storable.RandomValues;

	/// <summary>
	/// Utility for generating deterministically randomized data, even though chunks may be reordered
	/// during actual import.
	/// </summary>
	public class RandomsStates : System.Func<long, RandomValues>
	{
		 private readonly long _initialSeed;

		 public RandomsStates( long initialSeed )
		 {
			  this._initialSeed = initialSeed;
		 }

		 public override RandomValues Apply( long batch )
		 {
			  return RandomValues.create( new Random( _initialSeed + batch ) );
		 }
	}

}
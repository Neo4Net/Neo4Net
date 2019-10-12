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
namespace Org.Neo4j.Values.Storable
{

	public class SplittableRandomGenerator : Generator
	{
		 private readonly SplittableRandom _random;

		 internal SplittableRandomGenerator( SplittableRandom random )
		 {
			  this._random = random;
		 }

		 public override long NextLong()
		 {
			  return _random.nextLong();
		 }

		 public override bool NextBoolean()
		 {
			  return _random.nextBoolean();
		 }

		 public override int NextInt()
		 {
			  return _random.Next();
		 }

		 public override int NextInt( int bound )
		 {
			  return _random.Next( bound );
		 }

		 public override float NextFloat()
		 {
			  //this is a safe cast since nextDouble returns values in [0,1.0)
			  return ( float ) _random.NextDouble();
		 }

		 public override double NextDouble()
		 {
			  return _random.NextDouble();
		 }
	}

}
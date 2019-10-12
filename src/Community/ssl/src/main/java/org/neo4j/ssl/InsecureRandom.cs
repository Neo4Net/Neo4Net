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
namespace Neo4Net.Ssl
{

	public class InsecureRandom : java.security.SecureRandom
	{
		 private static Random Random()
		 {
			  return ThreadLocalRandom.current();
		 }

		 public override string Algorithm
		 {
			 get
			 {
				  return "insecure";
			 }
		 }

		 public override sbyte[] Seed
		 {
			 set
			 {
			 }
		 }

		 public override long Seed
		 {
			 set
			 {
			 }
		 }

		 public override void NextBytes( sbyte[] bytes )
		 {
			  Random().NextBytes(bytes);
		 }

		 public override sbyte[] GenerateSeed( int numBytes )
		 {
			  sbyte[] seed = new sbyte[numBytes];
			  Random().NextBytes(seed);
			  return seed;
		 }

		 public override int NextInt()
		 {
			  return Random().Next();
		 }

		 public override int NextInt( int n )
		 {
			  return Random().Next(n);
		 }

		 public override bool NextBoolean()
		 {
			  return Random().nextBoolean();
		 }

		 public override long NextLong()
		 {
			  return Random().nextLong();
		 }

		 public override float NextFloat()
		 {
			  return Random().nextFloat();
		 }

		 public override double NextDouble()
		 {
			  return Random().NextDouble();
		 }

		 public override double NextGaussian()
		 {
			  return Random().nextGaussian();
		 }
	}

}
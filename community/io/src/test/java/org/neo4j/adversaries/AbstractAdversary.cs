﻿using System;

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
namespace Org.Neo4j.Adversaries
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public abstract class AbstractAdversary implements Adversary
	public abstract class AbstractAdversary : Adversary
	{
		public abstract bool InjectFailureOrMischief( params Type[] failureTypes );
		public abstract void InjectFailure( params Type[] failureTypes );
		 protected internal readonly Random Rng;

		 public AbstractAdversary()
		 {
			  Rng = new Random();
		 }

		 public virtual long Seed
		 {
			 set
			 {
				  Rng.Seed = value;
			 }
		 }

		 protected internal virtual void ThrowOneOf( params Type[] types )
		 {
			  int choice = Rng.Next( types.Length );
			  Type type = types[choice];
			  Exception throwable;
			  try
			  {
					throwable = System.Activator.CreateInstance( type );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( new Exception( "Failed to instantiate failure", e ) );
			  }
			  SneakyThrow( throwable );
		 }

		 public static void SneakyThrow( Exception throwable )
		 {
			  _sneakyThrow( throwable );
		 }

		 // http://youtu.be/7qXXWHfJha4
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T extends Throwable> void _sneakyThrow(Throwable throwable) throws T
		 private static void _sneakyThrow<T>( Exception throwable ) where T : Exception
		 {
			  throw ( T ) throwable;
		 }
	}

}
using System;
using System.Threading;

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
namespace Neo4Net.Util.concurrent
{
	using Exceptions = Neo4Net.Helpers.Exceptions;

	public class Runnables
	{
		 public static readonly ThreadStart EmptyRunnable = () =>
		 {
		  // empty
		 };

		 /// <summary>
		 /// Run all runnables, chaining exceptions, if any, into a single <seealso cref="System.Exception"/> with provided message as message. </summary>
		 /// <param name="message"> passed to resulting <seealso cref="System.Exception"/> if any runnable throw. </param>
		 /// <param name="runnables"> to run. </param>
		 public static void RunAll( string message, params ThreadStart[] runnables )
		 {
			  Exception exceptions = null;
			  foreach ( ThreadStart runnable in runnables )
			  {
					try
					{
						 runnable.run();
					}
					catch ( Exception t )
					{
						 exceptions = Exceptions.chain( exceptions, t );
					}
			  }
			  if ( exceptions != null )
			  {
					throw new Exception( message, exceptions );
			  }
		 }
	}

}
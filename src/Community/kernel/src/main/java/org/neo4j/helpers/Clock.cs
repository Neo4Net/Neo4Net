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
namespace Neo4Net.Helpers
{
	/// @deprecated please use <seealso cref="java.time.Clock"/> instead 
	/// <seealso cref= org.neo4j.time.Clocks </seealso>
	[Obsolete("please use <seealso cref=\"java.time.Clock\"/> instead")]
	public interface Clock
	{
		 /// @deprecated please use <seealso cref="java.time.Clock.systemUTC()"/> instead 
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 @@Deprecated("please use <seealso cref=\"java.time.Clock.systemUTC()\"/> instead") Clock SYSTEM_CLOCK = new Clock()
	//	 {
	//		  @@Override public long currentTimeMillis()
	//		  {
	//				return System.currentTimeMillis();
	//		  }
	//
	//		  @@Override public long nanoTime()
	//		  {
	//				return System.nanoTime();
	//		  }
	//	 };

		 long CurrentTimeMillis();

		 long NanoTime();
	}

}
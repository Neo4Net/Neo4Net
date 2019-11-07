using System;

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
namespace Neo4Net.Helpers
{
	/// @deprecated please use <seealso cref="java.time.Clock"/> instead 
	/// <seealso cref= Neo4Net.time.Clocks </seealso>
	[Obsolete("please use <seealso cref=\"java.time.Clock\"/> instead")]
	public interface IClock
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
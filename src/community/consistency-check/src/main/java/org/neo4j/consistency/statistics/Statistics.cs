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
namespace Neo4Net.Consistency.statistics
{
	/// <summary>
	/// Top level interface for managing statistics. The statistics are for human eyes, and so there's basically
	/// only a <seealso cref="print(string)"/> method.
	/// </summary>
	public interface IStatistics
	{
		 void Print( string name );

		 void Reset();

		 Counts Counts { get; }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Statistics NONE = new Statistics()
	//	 {
	//		  @@Override public void reset()
	//		  {
	//		  }
	//
	//		  @@Override public void print(String name)
	//		  {
	//		  }
	//
	//		  @@Override public Counts getCounts()
	//		  {
	//				return Counts.NONE;
	//		  }
	//	 };
	}

}
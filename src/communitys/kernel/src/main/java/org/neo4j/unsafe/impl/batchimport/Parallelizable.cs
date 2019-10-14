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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	/// <summary>
	/// Represents something that can be parallelizable, in this case that means the ability to dynamically change
	/// the number of processors executing tasks.
	/// </summary>
	public interface Parallelizable
	{
		 /// <summary>
		 /// Change number of processors assigned to this <seealso cref="Parallelizable"/>. Accepts a {@code delta},
		 /// which may specify positive or negative value, even zero. This instances may have internal constraints
		 /// in the number of processors, min or max, which may be assigned and so potentially the change will
		 /// only be partially accepted or not at all. This is why this call returns the total number of processors
		 /// this instance now has accepted after any effect of this call.
		 /// 
		 /// <seealso cref="Parallelizable"/> is used in many call stacks where call delegation is predominant and so
		 /// reducing number of methods to delegate is favored. This is why this method looks and functions
		 /// like this, it can cater for incrementing, decrementing and even getting number of processors.
		 /// </summary>
		 /// <param name="delta"> number of processors to add or remove, i.e. negative or positive value. A value of
		 /// zero will result in merely the current number of assigned processors to be returned. </param>
		 /// <returns> the number of assigned processors as a result this call. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int processors(int delta)
	//	 {
	//		  return 1;
	//	 }
	}

}
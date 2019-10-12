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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{
	public interface LogPruning
	{
		 /// <summary>
		 /// Prunes logs that have version less than {@code currentVersion}. This is a best effort service and there is no
		 /// guarantee that any logs will be removed.
		 /// </summary>
		 /// <param name="currentVersion"> The lowest version expected to remain after pruning completes. </param>
		 void PruneLogs( long currentVersion );

		 /// <summary>
		 /// Check if there might be a desire to prune logs. This could be used as a hint to schedule some log pruning soon,
		 /// and/or increase the check pointing frequency.
		 /// </summary>
		 /// <returns> {@code true} if calling <seealso cref="pruneLogs(long)"/> now <em>might</em> cause log files to be deleted.
		 /// Otherwise {@code false} if we are pretty sure that we don't need to prune any logs right now. </returns>
		 bool MightHaveLogsToPrune();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LogPruning NO_PRUNING = new LogPruning()
	//	 {
	//		  @@Override public void pruneLogs(long currentVersion)
	//		  {
	//		  }
	//
	//		  @@Override public boolean mightHaveLogsToPrune()
	//		  {
	//				return false;
	//		  }
	//	 };
	}

}
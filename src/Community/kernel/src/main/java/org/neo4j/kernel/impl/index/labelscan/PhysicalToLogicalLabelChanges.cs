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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;

	internal class PhysicalToLogicalLabelChanges
	{
		 private PhysicalToLogicalLabelChanges()
		 {
		 }

		 /// <summary>
		 /// Converts physical before/after state to logical remove/add state. This conversion reuses the existing
		 /// long[] arrays in <seealso cref="NodeLabelUpdate"/>, 'before' is used for removals and 'after' is used for adds,
		 /// by shuffling numbers around and possible terminates them with -1 because the logical change set will be
		 /// equally big or smaller than the physical change set.
		 /// </summary>
		 /// <param name="update"> <seealso cref="NodeLabelUpdate"/> containing physical before/after state. </param>
		 internal static void ConvertToAdditionsAndRemovals( NodeLabelUpdate update )
		 {
			  int beforeLength = update.LabelsBefore.Length;
			  int afterLength = update.LabelsAfter.Length;

			  int bc = 0;
			  int ac = 0;
			  long[] before = update.LabelsBefore;
			  long[] after = update.LabelsAfter;
			  for ( int bi = 0, ai = 0; bi < beforeLength || ai < afterLength; )
			  {
					long beforeId = bi < beforeLength ? before[bi] : -1;
					long afterId = ai < afterLength ? after[ai] : -1;
					if ( beforeId == afterId )
					{ // no change
						 bi++;
						 ai++;
						 continue;
					}

					if ( Smaller( beforeId, afterId ) )
					{
						 while ( Smaller( beforeId, afterId ) && bi < beforeLength )
						 {
							  // looks like there's an id in before which isn't in after ==> REMOVE
							  update.LabelsBefore[bc++] = beforeId;
							  bi++;
							  beforeId = bi < beforeLength ? before[bi] : -1;
						 }
					}
					else if ( Smaller( afterId, beforeId ) )
					{
						 while ( Smaller( afterId, beforeId ) && ai < afterLength )
						 {
							  // looks like there's an id in after which isn't in before ==> ADD
							  update.LabelsAfter[ac++] = afterId;
							  ai++;
							  afterId = ai < afterLength ? after[ai] : -1;
						 }
					}
			  }

			  TerminateWithMinusOneIfNeeded( update.LabelsBefore, bc );
			  TerminateWithMinusOneIfNeeded( update.LabelsAfter, ac );
		 }

		 private static bool Smaller( long id, long otherId )
		 {
			  return id != -1 && ( otherId == -1 || id < otherId );
		 }

		 private static void TerminateWithMinusOneIfNeeded( long[] labelIds, int actualLength )
		 {
			  if ( actualLength < labelIds.Length )
			  {
					labelIds[actualLength] = -1;
			  }
		 }
	}

}
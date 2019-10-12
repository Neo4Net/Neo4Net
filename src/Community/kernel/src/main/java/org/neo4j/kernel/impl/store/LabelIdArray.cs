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
namespace Neo4Net.Kernel.impl.store
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.arraycopy;

	public class LabelIdArray
	{
		 private LabelIdArray()
		 {
		 }

		 internal static long[] ConcatAndSort( long[] existing, long additional )
		 {
			  AssertNotContains( existing, additional );

			  long[] result = new long[existing.Length + 1];
			  arraycopy( existing, 0, result, 0, existing.Length );
			  result[existing.Length] = additional;
			  Arrays.sort( result );
			  return result;
		 }

		 private static void AssertNotContains( long[] existingLabels, long labelId )
		 {
			  if ( Arrays.binarySearch( existingLabels, labelId ) >= 0 )
			  {
					throw new System.InvalidOperationException( "Label " + labelId + " already exists." );
			  }
		 }

		 internal static long[] Filter( long[] ids, long excludeId )
		 {
			  bool found = false;
			  foreach ( long id in ids )
			  {
					if ( id == excludeId )
					{
						 found = true;
						 break;
					}
			  }
			  if ( !found )
			  {
					throw new System.InvalidOperationException( "Label " + excludeId + " not found." );
			  }

			  long[] result = new long[ids.Length - 1];
			  int writerIndex = 0;
			  foreach ( long id in ids )
			  {
					if ( id != excludeId )
					{
						 result[writerIndex++] = id;
					}
			  }
			  return result;
		 }

		 public static long[] PrependNodeId( long nodeId, long[] labelIds )
		 {
			  long[] result = new long[labelIds.Length + 1];
			  arraycopy( labelIds, 0, result, 1, labelIds.Length );
			  result[0] = nodeId;
			  return result;
		 }

		 public static long[] StripNodeId( long[] storedLongs )
		 {
			  return Arrays.copyOfRange( storedLongs, 1, storedLongs.Length );
		 }
	}

}
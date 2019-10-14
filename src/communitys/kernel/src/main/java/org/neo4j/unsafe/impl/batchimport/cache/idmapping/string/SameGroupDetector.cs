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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.IdMapper_Fields.ID_NOT_FOUND;

	/// <summary>
	/// Used by <seealso cref="EncodingIdMapper"/> to help detect collisions of encoded values within the same group.
	/// Same values for different groups are not considered collisions.
	/// </summary>
	internal class SameGroupDetector
	{
		 // Alternating data index, group id
		 private long[] _seen = new long[100]; // grows on demand
		 private int _cursor;

		 /// <returns> -1 if no collision within the same group, or an actual data index which collides with the
		 /// supplied data index and group id. In the case of <strong>not</strong> {@code -1} both {@code dataIndexB}
		 /// and the returned data index should be marked as collisions. </returns>
		 internal virtual long CollisionWithinSameGroup( long dataIndexA, int groupIdA, long dataIndexB, int groupIdB )
		 {
			  // The first call, add both the entries. For consecutive calls for this same collision stretch
			  // only add and compare the second. The reason it's done in here instead of having a method signature
			  // of one data index and group id and having the caller call two times is that we're better suited
			  // to decide if this is the first or consecutive call for this collision stretch.
			  if ( _cursor == 0 )
			  {
					Add( dataIndexA, groupIdA );
			  }

			  long collision = ID_NOT_FOUND;
			  for ( int i = 0; i < _cursor; i++ )
			  {
					long dataIndexAtCursor = _seen[i++];
					long groupIdAtCursor = _seen[i];
					if ( groupIdAtCursor == groupIdB )
					{
						 collision = dataIndexAtCursor;
						 break;
					}
			  }

			  Add( dataIndexB, groupIdB );

			  return collision;
		 }

		 private void Add( long dataIndex, int groupId )
		 {
			  if ( _cursor == _seen.Length )
			  {
					_seen = Arrays.copyOf( _seen, _seen.Length * 2 );
			  }
			  _seen[_cursor++] = dataIndex;
			  _seen[_cursor++] = groupId;
		 }

		 internal virtual void Reset()
		 {
			  _cursor = 0;
		 }
	}

}
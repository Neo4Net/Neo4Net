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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using Groups = Neo4Net.@unsafe.Impl.Batchimport.input.Groups;

	/// <summary>
	/// A little utility for randomizing dividing up nodes into <seealso cref="Group id spaces"/>.
	/// Supplied with number of nodes to divide up and number of groups
	/// to divide into, the group sizes are randomized and together they will contain all nodes.
	/// </summary>
	public class IdGroupDistribution
	{
		 private readonly long[] _groupCounts;
		 private readonly Groups _groups;

		 public IdGroupDistribution( long nodeCount, int numberOfGroups, Random random, Groups groups )
		 {
			  this._groups = groups;
			  _groupCounts = new long[numberOfGroups];

			  // Assign all except the last one
			  long total = 0;
			  long partSize = nodeCount / numberOfGroups;
			  float debt = 1f;
			  for ( int i = 0; i < numberOfGroups - 1; i++ )
			  {
					float part = random.nextFloat() * debt;
					AssignGroup( i, ( long )( partSize * part ) );
					total += _groupCounts[i];
					debt = debt + 1.0f - part;
			  }

			  // Assign the rest to the last one
			  AssignGroup( numberOfGroups - 1, nodeCount - total );
		 }

		 private void AssignGroup( int i, long count )
		 {
			  _groupCounts[i] = count;
			  _groups.getOrCreate( "Group" + i );
		 }

		 public virtual Group GroupOf( long nodeInOrder )
		 {
			  long at = 0;
			  for ( int i = 0; i < _groupCounts.Length; i++ )
			  {
					at += _groupCounts[i];
					if ( nodeInOrder < at )
					{
						 return _groups.get( 1 + i );
					}
			  }
			  throw new System.ArgumentException( "Strange, couldn't find group for node (import order) " + nodeInOrder + ", counted to " + at + " as total number of " + at );
		 }
	}

}
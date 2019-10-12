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
namespace Neo4Net.Storageengine.Api.txstate
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	/// <summary>
	/// Represents the transactional changes to a node:
	/// <ul>
	/// <li><seealso cref="labelDiffSets() Labels"/> that have been <seealso cref="DiffSets.getAdded() added"/>
	/// or <seealso cref="DiffSets.getRemoved() removed"/>.</li>
	/// <li>Added and removed relationships.</li>
	/// <li><seealso cref="PropertyContainerState Changes to properties"/>.</li>
	/// </ul>
	/// </summary>
	public interface NodeState : PropertyContainerState
	{
		 LongDiffSets LabelDiffSets();

		 /// <summary>
		 /// This method counts all directions separately, i.e.
		 /// total count = count(INCOMING) + count(OUTGOING) + count(LOOPS)
		 /// </summary>
		 int AugmentDegree( RelationshipDirection direction, int degree, int typeId );

		 long Id { get; }

		 LongIterator AddedRelationships { get; }

		 LongIterator GetAddedRelationships( RelationshipDirection direction, int relType );
	}

}
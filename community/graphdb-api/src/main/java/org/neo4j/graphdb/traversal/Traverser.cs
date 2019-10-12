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
namespace Org.Neo4j.Graphdb.traversal
{
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;

	/// <summary>
	/// This interface represents the traverser which is used to step through the
	/// results of a traversal. Each step can be represented in different ways. The
	/// default is as <seealso cref="Path"/> objects which all other representations can be
	/// derived from, i.e <seealso cref="Node"/> or <seealso cref="Relationship"/>. Each step
	/// can also be represented in one of those representations directly.
	/// </summary>
	public interface Traverser : ResourceIterable<Path>
	{
		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="Node"/>s. This is a
		 /// convenient way to iterate over <seealso cref="Path"/>s and get the
		 /// <seealso cref="Path.endNode()"/> for each position.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="Node"/> objects. </returns>
		 ResourceIterable<Node> Nodes();

		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="Relationship"/>s. This is a
		 /// convenient way to iterate over <seealso cref="Path"/>s and get the
		 /// <seealso cref="Path.lastRelationship()"/> for each position.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="Relationship"/> objects. </returns>
		 ResourceIterable<Relationship> Relationships();

		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="Path"/>s.
		 /// When a traversal is done and haven't been fully iterated through,
		 /// it should be <seealso cref="ResourceIterator.close() closed"/>.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="Path"/> objects. </returns>
		 ResourceIterator<Path> Iterator();

		 /// <returns> the <seealso cref="TraversalMetadata"/> from the last traversal performed,
		 /// or being performed by this traverser. </returns>
		 TraversalMetadata Metadata();
	}

}
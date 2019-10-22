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
namespace Neo4Net.GraphDb.traversal
{
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;

	/// <summary>
	/// This interface represents the traverser which is used to step through the
	/// results of a traversal. Each step can be represented in different ways. The
	/// default is as <seealso cref="IPath"/> objects which all other representations can be
	/// derived from, i.e <seealso cref="INode"/> or <seealso cref="IRelationship"/>. Each step
	/// can also be represented in one of those representations directly.
	/// </summary>
	public interface Traverser : ResourceIterable<IPath>
	{
		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="INode"/>s. This is a
		 /// convenient way to iterate over <seealso cref="IPath"/>s and get the
		 /// <seealso cref="IPath.endNode()"/> for each position.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="INode"/> objects. </returns>
		 ResourceIterable<INode> Nodes();

		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="IRelationship"/>s. This is a
		 /// convenient way to iterate over <seealso cref="IPath"/>s and get the
		 /// <seealso cref="IPath.lastRelationship()"/> for each position.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="IRelationship"/> objects. </returns>
		 ResourceIterable<IRelationship> Relationships();

		 /// <summary>
		 /// Represents the traversal in the form of <seealso cref="IPath"/>s.
		 /// When a traversal is done and haven't been fully iterated through,
		 /// it should be <seealso cref="ResourceIterator.close() closed"/>.
		 /// </summary>
		 /// <returns> the traversal in the form of <seealso cref="IPath"/> objects. </returns>
		 ResourceIterator<IPath> Iterator();

		 /// <returns> the <seealso cref="TraversalMetadata"/> from the last traversal performed,
		 /// or being performed by this traverser. </returns>
		 TraversalMetadata Metadata();
	}

}
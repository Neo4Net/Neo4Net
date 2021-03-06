﻿using System.Collections.Generic;

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

	/// <summary>
	/// Represents a description of a bidirectional traversal. A Bidirectional
	/// traversal has a start side and an end side and an evaluator to handle
	/// collisions between those two sides, collisions which generates paths
	/// between start and end node(s).
	/// 
	/// A <seealso cref="BidirectionalTraversalDescription"/> is immutable and each
	/// method which adds or modifies the behavior returns a new instances that
	/// includes the new modification, leaving the instance which returns the new
	/// instance intact.
	/// 
	/// The interface is still experimental and may still change significantly.
	/// </summary>
	/// <seealso cref= TraversalDescription </seealso>
	public interface BidirectionalTraversalDescription
	{
		 /// <summary>
		 /// Sets the start side <seealso cref="TraversalDescription"/> of this bidirectional
		 /// traversal. The point of a bidirectional traversal is that the start
		 /// and end side will meet (or collide) in the middle somewhere and
		 /// generate paths evaluated and returned by this traversal. </summary>
		 /// <param name="startSideDescription"> the <seealso cref="TraversalDescription"/> to use
		 /// for the start side traversal. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription StartSide( TraversalDescription startSideDescription );

		 /// <summary>
		 /// Sets the end side <seealso cref="TraversalDescription"/> of this bidirectional
		 /// traversal. The point of a bidirectional traversal is that the start
		 /// and end side will meet (or collide) in the middle somewhere and
		 /// generate paths evaluated and returned by this traversal. </summary>
		 /// <param name="endSideDescription"> the <seealso cref="TraversalDescription"/> to use
		 /// for the end side traversal. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription EndSide( TraversalDescription endSideDescription );

		 /// <summary>
		 /// Sets both the start side and end side of this bidirectional traversal,
		 /// the <seealso cref="startSide(TraversalDescription) start side"/> is assigned the
		 /// {@code sideDescription} and the <seealso cref="endSide(TraversalDescription) end side"/>
		 /// is assigned the same description, although
		 /// <seealso cref="TraversalDescription.reverse() reversed"/>. This will replace any
		 /// traversal description previously set by <seealso cref="startSide(TraversalDescription)"/>
		 /// or <seealso cref="endSide(TraversalDescription)"/>.
		 /// </summary>
		 /// <param name="sideDescription"> the <seealso cref="TraversalDescription"/> to use for both sides
		 /// of the bidirectional traversal. The end side will have it
		 /// <seealso cref="TraversalDescription.reverse() reversed"/> </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription MirroredSides( TraversalDescription sideDescription );

		 /// <summary>
		 /// Sets the collision policy to use during this traversal. Branch collisions
		 /// happen between <seealso cref="TraversalBranch"/>es where start and end branches
		 /// meet and <seealso cref="Path"/>s are generated from it.
		 /// </summary>
		 /// <param name="collisionDetection"> the <seealso cref="BranchCollisionPolicy"/> to use during
		 /// this traversal. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription CollisionPolicy( BranchCollisionPolicy collisionDetection );

		 /// <summary>
		 /// Sets the <seealso cref="Evaluator"/> to use for branch collisions. The outcome
		 /// returned from the evaluator affects the colliding branches. </summary>
		 /// <param name="collisionEvaluator"> the <seealso cref="Evaluator"/> to use for evaluating
		 /// branch collisions. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription CollisionEvaluator( Evaluator collisionEvaluator );

		 /// <summary>
		 /// Sets the <seealso cref="PathEvaluator"/> to use for branch collisions. The outcome
		 /// returned from the evaluator affects the colliding branches. </summary>
		 /// <param name="collisionEvaluator"> the <seealso cref="PathEvaluator"/> to use for evaluating
		 /// branch collisions. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription CollisionEvaluator( PathEvaluator collisionEvaluator );

		 /// <summary>
		 /// In a bidirectional traversal the traverser alternates which side
		 /// (start or end) to move further for each step. This sets the
		 /// <seealso cref="SideSelectorPolicy"/> to use.
		 /// </summary>
		 /// <param name="sideSelector"> the <seealso cref="SideSelectorPolicy"/> to use for this
		 /// traversal. </param>
		 /// <param name="maxDepth"> optional max depth parameter to the side selector.
		 /// Why is max depth a concern of the <seealso cref="SideSelector"/>? Because it has
		 /// got knowledge of both the sides of the traversal at any given point. </param>
		 /// <returns> a new traversal description with the new modifications. </returns>
		 BidirectionalTraversalDescription SideSelector( SideSelectorPolicy sideSelector, int maxDepth );

		 /// <summary>
		 /// Traverse between a given {@code start} and {@code end} node with all
		 /// applied rules and behavior in this traversal description.
		 /// A <seealso cref="Traverser"/> is returned which is used to step through the
		 /// graph and getting results back. The traversal is not guaranteed to
		 /// start before the Traverser is used.
		 /// </summary>
		 /// <param name="start"> <seealso cref="Node"/> to use as starting point for the start
		 /// side in this traversal. </param>
		 /// <param name="end"> <seealso cref="Node"/> to use as starting point for the end
		 /// side in this traversal. </param>
		 /// <returns> a <seealso cref="Traverser"/> used to step through the graph and to get
		 /// results from. </returns>
		 Traverser Traverse( Node start, Node end );

		 /// <summary>
		 /// Traverse between a set of {@code start} and {@code end} nodes with all
		 /// applied rules and behavior in this traversal description.
		 /// A <seealso cref="Traverser"/> is returned which is used to step through the
		 /// graph and getting results back. The traversal is not guaranteed to
		 /// start before the Traverser is used.
		 /// </summary>
		 /// <param name="start"> set of nodes to use as starting points for the start
		 /// side in this traversal. </param>
		 /// <param name="end"> set of nodes to use as starting points for the end
		 /// side in this traversal. </param>
		 /// <returns> a <seealso cref="Traverser"/> used to step through the graph and to get
		 /// results from. </returns>
		 Traverser Traverse( IEnumerable<Node> start, IEnumerable<Node> end );
	}

}
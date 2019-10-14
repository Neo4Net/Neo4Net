using System.Collections.Generic;

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
namespace Neo4Net.Graphalgo.impl.util
{

	/// <summary>
	/// PathInterest decides if a path is of interest or not in priority based traversal such as
	/// <seealso cref="org.neo4j.graphalgo.impl.path.Dijkstra"/> or <seealso cref="org.neo4j.graphalgo.impl.path.AStar"/>.
	/// <seealso cref="comparator()"/> provides a comparator on priority object to be used when ordering paths.
	/// <seealso cref="canBeRuledOut(int, object, object)"/>
	/// @author Anton Persson
	/// </summary>
	public interface PathInterest<P>
	{
		 /// <returns> <seealso cref="System.Collections.IComparer"/> to use when ordering in priority map </returns>
		 IComparer<P> Comparator();

		 /// <summary>
		 /// Decide if a traversal branch with numberOfVisits, pathPriority and oldPriority (based on end node) can be ruled
		 /// out from further traversal or not. </summary>
		 /// <param name="numberOfVisits"> number of times a traversal branch ending on the same node has been traversed from. </param>
		 /// <param name="pathPriority"> priority of traversal branch currently considered. </param>
		 /// <param name="oldPriority"> priority of other traversal branch. </param>
		 /// <returns> true if traversal branch can be ruled out from further traversal, false otherwise. </returns>
		 bool CanBeRuledOut( int numberOfVisits, P pathPriority, P oldPriority );

		 /// <summary>
		 /// Decide if a traversal branch that previously has not been ruled out still is interesting. This would typically
		 /// mean that a certain number of paths are of interest. </summary>
		 /// <param name="numberOfVisits"> </param>
		 /// <returns> true if traversal branch still is of interest </returns>
		 bool StillInteresting( int numberOfVisits );

		 /// <summary>
		 /// Should traversal stop when traversed beyond lowest cost? </summary>
		 /// <returns> true if traversal should stop beyond lowest cost. </returns>
		 bool StopAfterLowestCost();
	}

	 public abstract class PathInterest_PriorityBasedPathInterest<P> : PathInterest<P>
	 {
		 public abstract IComparer<P> Comparator();
		  /// <returns> <seealso cref="BiFunction"/> to be used when deciding if entity can be ruled out or not. </returns>
		  internal abstract System.Func<P, P, bool> InterestFunction();

		  public override bool CanBeRuledOut( int numberOfVisits, P pathPriority, P oldPriority )
		  {
				return !InterestFunction().apply(pathPriority, oldPriority);
		  }

		  public override bool StillInteresting( int numberOfVisits )
		  {
				return true;
		  }

		  /// <returns> true </returns>
		  public override bool StopAfterLowestCost()
		  {
				return true;
		  }
	 }

	 public abstract class PathInterest_VisitCountBasedPathInterest<P> : PathInterest<P>
	 {
		 public abstract IComparer<P> Comparator();
		  internal abstract int NumberOfWantedPaths();

		  /// <summary>
		  /// Use <seealso cref="numberOfWantedPaths()"/> to decide if an entity should be ruled out or not and if an entity
		  /// still is of interest. </summary>
		  /// <param name="numberOfVisits"> number of times a traversal branch ending on the same node has been traversed from. </param>
		  /// <param name="pathPriority"> priority of traversal branch currently considered. </param>
		  /// <param name="oldPriority"> priority of other traversal branch. </param>
		  /// <returns> numberOfVisits > <seealso cref="numberOfWantedPaths()"/> </returns>
		  public override bool CanBeRuledOut( int numberOfVisits, P pathPriority, P oldPriority )
		  {
				return numberOfVisits > NumberOfWantedPaths();
		  }

		  public override bool StillInteresting( int numberOfVisits )
		  {
				return numberOfVisits <= NumberOfWantedPaths();
		  }

		  public override bool StopAfterLowestCost()
		  {
				return false;
		  }
	 }

}
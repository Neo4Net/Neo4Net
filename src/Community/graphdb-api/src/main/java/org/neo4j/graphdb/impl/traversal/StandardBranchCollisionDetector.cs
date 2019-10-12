using System;
using System.Collections.Generic;

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
namespace Neo4Net.Graphdb.impl.traversal
{

	using Predicates = Neo4Net.Function.Predicates;
	using BranchCollisionDetector = Neo4Net.Graphdb.traversal.BranchCollisionDetector;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using Evaluator = Neo4Net.Graphdb.traversal.Evaluator;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;

	public class StandardBranchCollisionDetector : BranchCollisionDetector
	{
		 private readonly IDictionary<Node, ICollection<TraversalBranch>[]> _paths = new Dictionary<Node, ICollection<TraversalBranch>[]>( 1000 );
		 private readonly Evaluator _evaluator;
		 private readonly ISet<Path> _returnedPaths = new HashSet<Path>();
		 private System.Predicate<Path> _pathPredicate = Predicates.alwaysTrue();

		 [Obsolete]
		 public StandardBranchCollisionDetector( Evaluator evaluator )
		 {
			  this._evaluator = evaluator;
		 }

		 public StandardBranchCollisionDetector( Evaluator evaluator, System.Predicate<Path> pathPredicate )
		 {
			  this._evaluator = evaluator;
			  if ( pathPredicate != null )
			  {
					this._pathPredicate = pathPredicate;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public java.util.Collection<org.neo4j.graphdb.Path> evaluate(org.neo4j.graphdb.traversal.TraversalBranch branch, org.neo4j.graphdb.Direction direction)
		 public override ICollection<Path> Evaluate( TraversalBranch branch, Direction direction )
		 {
			  // [0] for paths from start, [1] for paths from end
			  ICollection<TraversalBranch>[] pathsHere = _paths[branch.EndNode()];
			  int index = direction.ordinal();
			  if ( pathsHere == null )
			  {
					pathsHere = new System.Collections.ICollection[]
					{
						new List<TraversalBranch>(),
						new List<TraversalBranch>()
					};
					_paths[branch.EndNode()] = pathsHere;
			  }
			  pathsHere[index].Add( branch );

			  // If there are paths from the other side then include all the
			  // combined paths
			  ICollection<TraversalBranch> otherCollections = pathsHere[index == 0 ? 1 : 0];
			  if ( otherCollections.Count > 0 )
			  {
					ICollection<Path> foundPaths = new List<Path>();
					foreach ( TraversalBranch otherBranch in otherCollections )
					{
						 TraversalBranch startPath = index == 0 ? branch : otherBranch;
						 TraversalBranch endPath = index == 0 ? otherBranch : branch;
						 BidirectionalTraversalBranchPath path = new BidirectionalTraversalBranchPath( startPath, endPath );
						 if ( IsAcceptablePath( path ) )
						 {
							  if ( _returnedPaths.Add( path ) && IncludePath( path, startPath, endPath ) )
							  {
									foundPaths.Add( path );
							  }
						 }
					}

					if ( foundPaths.Count > 0 )
					{
						 return foundPaths;
					}
			  }
			  return null;
		 }

		 private bool IsAcceptablePath( BidirectionalTraversalBranchPath path )
		 {
			  return _pathPredicate.test( path );
		 }

		 protected internal virtual bool IncludePath( Path path, TraversalBranch startPath, TraversalBranch endPath )
		 {
			  Evaluation eval = _evaluator.evaluate( path );
			  if ( !eval.continues() )
			  {
					startPath.Evaluation( eval );
					endPath.Evaluation( eval );
			  }
			  return eval.includes();
		 }
	}

}
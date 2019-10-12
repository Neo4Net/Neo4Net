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
namespace Neo4Net.Kernel.impl.traversal
{
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb;
	using Resource = Neo4Net.Graphdb.Resource;
	using BranchOrderingPolicy = Neo4Net.Graphdb.traversal.BranchOrderingPolicy;
	using BranchSelector = Neo4Net.Graphdb.traversal.BranchSelector;
	using Neo4Net.Graphdb.traversal;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using Neo4Net.Graphdb.traversal;
	using Neo4Net.Graphdb.traversal;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using UniquenessFactory = Neo4Net.Graphdb.traversal.UniquenessFactory;
	using UniquenessFilter = Neo4Net.Graphdb.traversal.UniquenessFilter;

	internal class MonoDirectionalTraverserIterator : AbstractTraverserIterator
	{
		 private readonly BranchSelector _selector;
		 private readonly PathEvaluator _evaluator;
		 private readonly UniquenessFilter _uniqueness;

		 internal MonoDirectionalTraverserIterator( Resource resource, UniquenessFilter uniqueness, PathExpander expander, BranchOrderingPolicy order, PathEvaluator evaluator, IEnumerable<Node> startNodes, InitialBranchState initialState, UniquenessFactory uniquenessFactory ) : base( resource )
		 {
			  this._uniqueness = uniqueness;
			  this._evaluator = evaluator;
			  this._selector = order.Create( new AsOneStartBranch( this, startNodes, initialState, uniquenessFactory ), expander );
		 }

		 public override Evaluation Evaluate( TraversalBranch branch, BranchState state )
		 {
			  return _evaluator.evaluate( branch, state );
		 }

		 protected internal override Path FetchNextOrNull()
		 {
			  TraversalBranch result;
			  while ( true )
			  {
					result = _selector.next( this );
					if ( result == null )
					{
						 Close();
						 return null;
					}
					if ( result.Includes() )
					{
						 NumberOfPathsReturnedConflict++;
						 return result;
					}
			  }
		 }

		 public override bool IsUniqueFirst( TraversalBranch branch )
		 {
			  return _uniqueness.checkFirst( branch );
		 }

		 public override bool IsUnique( TraversalBranch branch )
		 {
			  return _uniqueness.check( branch );
		 }
	}

}
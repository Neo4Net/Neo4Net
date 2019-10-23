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
namespace Neo4Net.Kernel.impl.traversal
{
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using Resource = Neo4Net.GraphDb.Resource;
	using BranchOrderingPolicy = Neo4Net.GraphDb.Traversal.BranchOrderingPolicy;
	using BranchSelector = Neo4Net.GraphDb.Traversal.BranchSelector;
	using Neo4Net.GraphDb.Traversal;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using Neo4Net.GraphDb.Traversal;
	using Neo4Net.GraphDb.Traversal;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;
	using UniquenessFactory = Neo4Net.GraphDb.Traversal.UniquenessFactory;
	using UniquenessFilter = Neo4Net.GraphDb.Traversal.UniquenessFilter;

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
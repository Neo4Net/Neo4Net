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
namespace Neo4Net.GraphDb.impl.traversal
{

	using Evaluator = Neo4Net.GraphDb.Traversal.Evaluator;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;

	public class ShortestPathsBranchCollisionDetector : StandardBranchCollisionDetector
	{
		 private int _depth = -1;

		 public ShortestPathsBranchCollisionDetector( Evaluator evaluator, System.Predicate<IPath> pathPredicate ) : base( evaluator, pathPredicate )
		 {
		 }

		 protected internal override bool IncludePath( IPath path, TraversalBranch startBranch, TraversalBranch endBranch )
		 {
			  if ( !base.IncludePath( path, startBranch, endBranch ) )
			  {
					return false;
			  }

			  if ( _depth == -1 )
			  {
					_depth = path.Length();
					return true;
			  }
			  return path.Length() == _depth;
		 }
	}

}
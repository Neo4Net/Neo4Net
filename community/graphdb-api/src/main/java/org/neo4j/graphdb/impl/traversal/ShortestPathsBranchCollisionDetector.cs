﻿/*
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
namespace Org.Neo4j.Graphdb.impl.traversal
{

	using Evaluator = Org.Neo4j.Graphdb.traversal.Evaluator;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;

	public class ShortestPathsBranchCollisionDetector : StandardBranchCollisionDetector
	{
		 private int _depth = -1;

		 public ShortestPathsBranchCollisionDetector( Evaluator evaluator, System.Predicate<Path> pathPredicate ) : base( evaluator, pathPredicate )
		 {
		 }

		 protected internal override bool IncludePath( Path path, TraversalBranch startBranch, TraversalBranch endBranch )
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
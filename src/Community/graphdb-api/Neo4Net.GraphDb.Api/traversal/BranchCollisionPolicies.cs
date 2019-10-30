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
namespace Neo4Net.GraphDb.Traversal
{

	using ShortestPathsBranchCollisionDetector = Neo4Net.GraphDb.Impl.Traversal.ShortestPathsBranchCollisionDetector;
	using StandardBranchCollisionDetector = Neo4Net.GraphDb.Impl.Traversal.StandardBranchCollisionDetector;

	/// <summary>
	/// A catalogue of convenient branch collision policies
	/// 
	/// Copied from kernel package so that we can hide kernel from the public API.
	/// </summary>
	public enum BranchCollisionPolicies
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: STANDARD { @Override public BranchCollisionDetector create(Evaluator evaluator, java.util.function.Predicate<org.Neo4Net.graphdb.Path> pathPredicate) { return new org.Neo4Net.GraphDb.Impl.Traversal.StandardBranchCollisionDetector(evaluator, pathPredicate); } },
		 STANDARD
		 {
			 public IBranchCollisionDetector create( IEvaluator evaluator, System.Predicate<IPath> pathPredicate ) { return new StandardBranchCollisionDetector( evaluator, pathPredicate ); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: SHORTEST_PATH { @Override public BranchCollisionDetector create(Evaluator evaluator, java.util.function.Predicate<org.Neo4Net.graphdb.Path> pathPredicate) { return new org.Neo4Net.GraphDb.Impl.Traversal.ShortestPathsBranchCollisionDetector(evaluator, pathPredicate); } }
		 SHORTEST_PATH
		 {
			 public BranchCollisionDetector create( Evaluator evaluator, System.Predicate<Path> pathPredicate ) { return new ShortestPathsBranchCollisionDetector( evaluator, pathPredicate ); }
		 }
	}

}
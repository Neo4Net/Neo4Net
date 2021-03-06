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
namespace Org.Neo4j.Graphdb.traversal
{

	/// <summary>
	/// An Evaluator controls what's to be returned from a traversal and also how
	/// pruning is done. It looks at a <seealso cref="Path"/> and decides whether or not it
	/// should be included in the traversal result. It also decides whether the traverser
	/// should continue down that path or if it should be pruned so that the traverser
	/// won't continue down that path.
	/// 
	/// @author Mattias Persson </summary>
	/// <seealso cref= Evaluation </seealso>
	/// <seealso cref= Evaluators </seealso>
	/// <seealso cref= TraversalDescription#evaluator(Evaluator) </seealso>
	public interface Evaluator
	{
		 /// <summary>
		 /// Evaluates a <seealso cref="Path"/> and returns an <seealso cref="Evaluation"/> containing
		 /// information about whether or not to include it in the traversal result,
		 /// i.e return it from the <seealso cref="Traverser"/>. And also whether or not to
		 /// continue traversing down that {@code path} or if it instead should be
		 /// pruned so that the traverser won't continue down that branch represented
		 /// by {@code path}.
		 /// </summary>
		 /// <param name="path"> the <seealso cref="Path"/> to evaluate. </param>
		 /// <returns> an <seealso cref="Evaluation"/> containing information about whether or not
		 /// to return it from the <seealso cref="Traverser"/> and whether or not to continue
		 /// down that path. </returns>
		 Evaluation Evaluate( Path path );

		 /// <summary>
		 /// Exposes an <seealso cref="Evaluator"/> as a <seealso cref="PathEvaluator"/>. </summary>
		 /// @param <STATE> the type of state passed into the evaluator. </param>
	}

	 public class Evaluator_AsPathEvaluator<STATE> : PathEvaluator<STATE>
	 {
		  internal readonly Evaluator Evaluator;

		  public Evaluator_AsPathEvaluator( Evaluator evaluator )
		  {
				this.Evaluator = evaluator;
		  }

		  public override Evaluation Evaluate( Path path, BranchState<STATE> state )
		  {
				return Evaluator.evaluate( path );
		  }

		  public override Evaluation Evaluate( Path path )
		  {
				return Evaluator.evaluate( path );
		  }
	 }

}
using System;

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
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb.traversal;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using Evaluator = Neo4Net.Graphdb.traversal.Evaluator;
	using Neo4Net.Graphdb.traversal;

	/// <summary>
	/// Evaluator which can hold multiple <seealso cref="Evaluator"/>s and delegate to them
	/// all for evaluation requests.
	/// </summary>
	public class MultiEvaluator<STATE> : Neo4Net.Graphdb.traversal.PathEvaluator_Adapter<STATE>
	{
		 private readonly PathEvaluator[] _evaluators;

		 internal MultiEvaluator( params PathEvaluator[] evaluators )
		 {
			  this._evaluators = evaluators;
		 }

		 /// <summary>
		 /// Returns whether or not the {@code position} is to be included and also
		 /// if it's going to be continued.
		 /// 
		 /// The include/exclude part of the returned <seealso cref="Evaluation"/> will be
		 /// {@code include} if all of the internal evaluators think it's going to be
		 /// included, otherwise it will be excluded.
		 /// 
		 /// The continue/prune part of the returned <seealso cref="Evaluation"/> will be
		 /// {@code continue} if all of the internal evaluators think it's going to be
		 /// continued, otherwise it will be pruned.
		 /// </summary>
		 /// <param name="position"> the <seealso cref="Path"/> to evaluate. </param>
		 /// <seealso cref= Evaluator </seealso>
		 public override Evaluation Evaluate( Path position, BranchState<STATE> state )
		 {
			  bool includes = true;
			  bool continues = true;
			  foreach ( PathEvaluator<STATE> evaluator in this._evaluators )
			  {
					Evaluation bla = evaluator.Evaluate( position, state );
					if ( !bla.includes() )
					{
						 includes = false;
						 if ( !continues )
						 {
							  return Evaluation.EXCLUDE_AND_PRUNE;
						 }
					}
					if ( !bla.continues() )
					{
						 continues = false;
						 if ( !includes )
						 {
							  return Evaluation.EXCLUDE_AND_PRUNE;
						 }
					}
			  }
			  return Evaluation.of( includes, continues );
		 }

		 /// <summary>
		 /// Adds {@code evaluator} to the list of evaluators wrapped by the returned
		 /// evaluator. A new <seealso cref="MultiEvaluator"/> instance additionally containing
		 /// the supplied {@code evaluator} is returned and this instance will be
		 /// left intact.
		 /// </summary>
		 /// <param name="evaluator"> the <seealso cref="Evaluator"/> to add to this multi evaluator. </param>
		 /// <returns> a new instance containing the current list of evaluator plus
		 /// the supplied one. </returns>
		 public virtual MultiEvaluator<STATE> Add( PathEvaluator<STATE> evaluator )
		 {
			  PathEvaluator[] newArray = new PathEvaluator[this._evaluators.Length + 1];
			  Array.Copy( this._evaluators, 0, newArray, 0, this._evaluators.Length );
			  newArray[newArray.Length - 1] = evaluator;
			  return new MultiEvaluator<STATE>( newArray );
		 }
	}

}
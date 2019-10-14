using System;

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
namespace Neo4Net.Cypher.Internal.evaluator
{
	/// <summary>
	/// An ExpressionEvaluator takes an arbitrary Cypher expression and evaluates it to a java value.
	/// </summary>
	public interface ExpressionEvaluator
	{
		 /// <summary>
		 /// Evaluates a Cypher expression
		 /// </summary>
		 /// <param name="expression"> The expression to evaluate. </param>
		 /// <param name="type"> The type that we expect the returned value to have </param>
		 /// <returns> The evaluated Cypher expression. </returns>
		 /// <exception cref="EvaluationException"> if the evaluation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T> T evaluate(String expression, Class<T> type) throws EvaluationException;
		 T evaluate<T>( string expression, Type type );
	}

}
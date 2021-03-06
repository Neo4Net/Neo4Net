﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Cypher.result
{
	using QueryStatistics = Org.Neo4j.Cypher.@internal.runtime.QueryStatistics;
	using Org.Neo4j.Graphdb;

	/// <summary>
	/// The result API of a Cypher runtime
	/// </summary>
	public interface RuntimeResult : AutoCloseable
	{

		 /// <summary>
		 /// Names of the returned fields of this result.
		 /// </summary>
		 string[] FieldNames();

		 /// <summary>
		 /// True if this result can be consumed as an iterator. See <seealso cref="RuntimeResult.asIterator"/>.
		 /// </summary>
		 bool Iterable { get; }

		 /// <summary>
		 /// Consume this result as an iterator. Will complain if <seealso cref="RuntimeResult.isIterable()"/> is false.
		 /// </summary>
		 ResourceIterator<IDictionary<string, object>> AsIterator();

		 /// <summary>
		 /// Returns the consumption state of this result. This state changes when the result is served
		 /// either via <seealso cref="RuntimeResult.asIterator"/> or <seealso cref="RuntimeResult.accept(QueryResult.QueryResultVisitor)"/>.
		 /// </summary>
		 RuntimeResult_ConsumptionState ConsumptionState();

		 /// <summary>
		 /// Consume this result using a visitor.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void accept(QueryResult_QueryResultVisitor<E> visitor) throws E;
		 void accept<E>( QueryResult_QueryResultVisitor<E> visitor );

		 /// <summary>
		 /// Get the <seealso cref="QueryStatistics"/> related to this query execution.
		 /// </summary>
		 QueryStatistics QueryStatistics();

		 /// <summary>
		 /// Get the <seealso cref="QueryProfile"/> of this query execution.
		 /// </summary>
		 QueryProfile QueryProfile();

		 void Close();
	}

	 public enum RuntimeResult_ConsumptionState
	 {
		  NotStarted,
		  HasMore,
		  Exhausted
	 }

}
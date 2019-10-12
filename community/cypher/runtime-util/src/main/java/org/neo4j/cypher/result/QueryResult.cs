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
namespace Org.Neo4j.Cypher.result
{
	using ExecutionPlanDescription = Org.Neo4j.Graphdb.ExecutionPlanDescription;
	using Notification = Org.Neo4j.Graphdb.Notification;
	using QueryExecutionType = Org.Neo4j.Graphdb.QueryExecutionType;
	using QueryStatistics = Org.Neo4j.Graphdb.QueryStatistics;
	using AnyValue = Org.Neo4j.Values.AnyValue;

	/// <summary>
	/// The public Result API of Cypher
	/// </summary>
	public interface QueryResult
	{
		 string[] FieldNames();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void accept(QueryResult_QueryResultVisitor<E> visitor) throws E;
		 void accept<E>( QueryResult_QueryResultVisitor<E> visitor );

		 QueryExecutionType ExecutionType();

		 QueryStatistics QueryStatistics();

		 ExecutionPlanDescription ExecutionPlanDescription();

		 IEnumerable<Notification> Notifications { get; }

		 void Close();
	}

	 public interface QueryResult_QueryResultVisitor<E> where E : Exception
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visit(QueryResult_Record row) throws E;
		  bool Visit( QueryResult_Record row );
	 }

	 public interface QueryResult_Record
	 {
		  AnyValue[] Fields();
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default void release()
	//	  {
	//	  }
	 }

}
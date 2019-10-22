using System;
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
namespace Neo4Net.Cypher.result
{
	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using Notification = Neo4Net.GraphDb.Notification;
	using QueryExecutionType = Neo4Net.GraphDb.QueryExecutionType;
	using QueryStatistics = Neo4Net.GraphDb.QueryStatistics;
	using AnyValue = Neo4Net.Values.AnyValue;

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
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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// This exception is thrown from the <seealso cref="GraphDatabaseService.execute(string, System.Collections.IDictionary) execute method"/>
	/// when there is an error during the execution of a query.
	/// </summary>
	public class QueryExecutionException : Exception
	{
		 private readonly string _statusCode;

		 public QueryExecutionException( string message, Exception cause, string statusCode ) : base( message, cause )
		 {
			  this._statusCode = statusCode;
		 }

		 /// <summary>
		 /// The Neo4j error <a href="https://neo4j.com/docs/developer-manual/current/reference/status-codes/">status code</a>.
		 /// </summary>
		 /// <returns> the Neo4j error status code. </returns>
		 public virtual string StatusCode
		 {
			 get
			 {
				  return _statusCode;
			 }
		 }
	}

}
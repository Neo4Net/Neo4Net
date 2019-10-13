﻿/*
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
namespace Neo4Net.Kernel.impl.query
{
	using AnyValue = Neo4Net.Values.AnyValue;

	/// <summary>
	/// Result buffer holding the results of a query execution.
	/// </summary>
	public interface ResultBuffer
	{
		 /// <summary>
		 /// Signal that a new query execution is starting, which will return {@code valuesPerResult}  many values per result.
		 /// </summary>
		 void BeginQueryExecution( int valuesPerResult );

		 /// <summary>
		 /// Prepare the result stage for the next result row.
		 /// </summary>
		 /// <returns> the id of the new row, or -1 if the stage could not be cleared. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long beginResultRow() throws ResultBufferException;
		 long BeginResultRow();

		 /// <summary>
		 /// Write a value of the result stage.
		 /// 
		 /// This method is expected to be called for every valueId in the range [0..valuesPerResult), for every row.
		 /// </summary>
		 /// <param name="columnId"> column id of the value to write. </param>
		 /// <param name="value"> the Value to write. </param>
		 void WriteValue( int columnId, AnyValue value );

		 /// <summary>
		 /// Commit the result row in the result stage to the buffer.
		 /// </summary>
		 /// <returns> true if the buffer can accept the next result row. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean endResultRow() throws ResultBufferException;
		 bool EndResultRow();

	// Suggestion for additional methods: add if needed.

	//    /**
	//     * Signals to the buffer that some error occurred during query execution.
	//     *
	//     * After onError is called, only endQueryExecution() will be called by this query.
	//     *
	//     * @param status The status code of the error
	//     * @param msg The error message associated with the error, or null if no message is provided
	//     */
	//    void onError( Status status, String msg );
	//
	//    /**
	//     * Signal that the current query execution is completed.
	//     */
	//    void endQueryExecution();
	}

}
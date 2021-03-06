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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using DuplicatingLogProvider = Org.Neo4j.Logging.DuplicatingLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StoreLogService = Org.Neo4j.Logging.@internal.StoreLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Classification.DatabaseError;

	/// <summary>
	/// Report received exceptions into the appropriate log (console or debug) and delivery stacktraces to debug.log.
	/// </summary>
	internal class ErrorReporter
	{
		 private readonly Log _userLog;
		 private readonly Log _debugLog;

		 internal ErrorReporter( LogService logging )
		 {
			  this._userLog = logging.GetUserLog( typeof( ErrorReporter ) );
			  this._debugLog = logging.GetInternalLog( typeof( ErrorReporter ) );
		 }

		 internal ErrorReporter( Log userLog, Log debugLog )
		 {
			  this._userLog = userLog;
			  this._debugLog = debugLog;
		 }

		 /// <summary>
		 /// Writes logs about database errors.
		 /// Short one-line message is written to both user and internal log.
		 /// Large message with stacktrace (if available) is written to internal log.
		 /// </summary>
		 /// <param name="error"> the error to log. </param>
		 /// <seealso cref= StoreLogService </seealso>
		 /// <seealso cref= DuplicatingLogProvider </seealso>
		 public virtual void Report( Neo4jError error )
		 {
			  if ( error.Status().code().classification() == DatabaseError )
			  {
					string message = format( "Client triggered an unexpected error [%s]: %s, reference %s.", error.Status().code().serialize(), error.Message(), error.Reference() );

					// Writing to user log gets duplicated to the internal log
					_userLog.error( message );

					// If cause/stacktrace is available write it to the internal log
					if ( error.Cause() != null )
					{
						 _debugLog.error( message, error.Cause() );
					}
			  }
		 }
	}

}
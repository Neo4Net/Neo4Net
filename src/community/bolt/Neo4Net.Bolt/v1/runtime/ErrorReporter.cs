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
namespace Neo4Net.Bolt.v1.runtime
{
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using DuplicatingLogProvider = Neo4Net.Logging.DuplicatingLogProvider;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using StoreLogService = Neo4Net.Logging.Internal.StoreLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.exceptions.Status_Classification.DatabaseError;

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
		 public virtual void Report( Neo4NetError error )
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
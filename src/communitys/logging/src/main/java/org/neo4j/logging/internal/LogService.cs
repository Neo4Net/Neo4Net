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
namespace Neo4Net.Logging.@internal
{

	/// <summary>
	/// Logging service that is used to obtain loggers for different output purposes
	/// </summary>
	public interface LogService
	{
		 /// <returns> a <seealso cref="org.neo4j.logging.LogProvider"/> that providers loggers for user visible messages. </returns>
		 LogProvider UserLogProvider { get; }

		 /// <summary>
		 /// Equivalent to {@code <seealso cref="getUserLogProvider"/>()( loggingClass )} </summary>
		 /// <param name="loggingClass"> the context for the return logger. </param>
		 /// <returns> a <seealso cref="org.neo4j.logging.Log"/> that logs user visible messages with the {@code loggingClass} as context. </returns>
		 Log GetUserLog( Type loggingClass );

		 /// <returns> a <seealso cref="LogProvider"/> that providers loggers for internal messages. </returns>
		 LogProvider InternalLogProvider { get; }

		 /// <summary>
		 /// Equivalent to {@code <seealso cref="getInternalLogProvider"/>()( loggingClass )} </summary>
		 /// <param name="loggingClass"> the context for the return logger. </param>
		 /// <returns> a <seealso cref="Log"/> that logs internal messages with the {@code loggingClass} as context. </returns>
		 Log GetInternalLog( Type loggingClass );
	}

}
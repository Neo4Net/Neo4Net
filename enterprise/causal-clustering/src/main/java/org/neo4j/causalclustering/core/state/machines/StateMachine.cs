﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.core.state.machines
{

	public interface StateMachine<Command>
	{
		 /// <summary>
		 /// Apply command to state machine, modifying its internal state.
		 /// Implementations should be idempotent, so that the caller is free to replay commands from any point in the log. </summary>
		 /// <param name="command"> Command to the state machine. </param>
		 /// <param name="commandIndex"> The index of the command. </param>
		 /// <param name="callback"> To be called when a result is produced. </param>
		 void ApplyCommand( Command command, long commandIndex, System.Action<Result> callback );

		 /// <summary>
		 /// Flushes state to durable storage. </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flush() throws java.io.IOException;
		 void Flush();

		 /// <summary>
		 /// Return the index of the last applied command by this state machine. </summary>
		 /// <returns> the last applied index for this state machine </returns>
		 long LastAppliedIndex();
	}

}
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
namespace Org.Neo4j.Bolt.runtime
{
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;

	public interface BoltStateMachine : AutoCloseable
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void process(org.neo4j.bolt.messaging.RequestMessage message, BoltResponseHandler handler) throws BoltConnectionFatality;
		 void Process( RequestMessage message, BoltResponseHandler handler );

		 bool ShouldStickOnThread();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void validateTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 void ValidateTransaction();

		 bool HasOpenStatement();

		 void Interrupt();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean reset() throws BoltConnectionFatality;
		 bool Reset();

		 void MarkFailed( Neo4jError error );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handleFailure(Throwable cause, boolean fatal) throws BoltConnectionFatality;
		 void HandleFailure( Exception cause, bool fatal );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handleExternalFailure(Neo4jError error, BoltResponseHandler handler) throws BoltConnectionFatality;
		 void HandleExternalFailure( Neo4jError error, BoltResponseHandler handler );

		 void MarkForTermination();

		 bool Closed { get; }

		 void Close();

		 string Id();
	}

}
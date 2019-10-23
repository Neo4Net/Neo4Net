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
namespace Neo4Net.Bolt.runtime
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;

	public interface BoltStateMachine : IDisposable
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void process(org.Neo4Net.bolt.messaging.RequestMessage message, BoltResponseHandler handler) throws BoltConnectionFatality;
		 void Process( RequestMessage message, BoltResponseHandler handler );

		 bool ShouldStickOnThread();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void validateTransaction() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
		 void ValidateTransaction();

		 bool HasOpenStatement();

		 void Interrupt();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean reset() throws BoltConnectionFatality;
		 bool Reset();

		 void MarkFailed( Neo4NetError error );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handleFailure(Throwable cause, boolean fatal) throws BoltConnectionFatality;
		 void HandleFailure( Exception cause, bool fatal );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handleExternalFailure(Neo4NetError error, BoltResponseHandler handler) throws BoltConnectionFatality;
		 void HandleExternalFailure( Neo4NetError error, BoltResponseHandler handler );

		 void MarkForTermination();

		 bool Closed { get; }

		 void Close();

		 string Id();
	}

}
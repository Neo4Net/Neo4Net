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
namespace Neo4Net.Kernel
{
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Signals that a deadlock between two or more transactions has been detected.
	/// </summary>
	public class DeadlockDetectedException : TransientTransactionFailureException, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 public DeadlockDetectedException( string message ) : base( message, null )
		 {
		 }

		 public DeadlockDetectedException( string message, Exception cause ) : base( "Don't panic.\n" + "\n" + "A deadlock scenario has been detected and avoided. This means that two or more transactions, which were " + "holding locks, were wanting to await locks held by one another, which would have resulted in a deadlock " + "between these transactions. This exception was thrown instead of ending up in that deadlock.\n" + "\n" + "See the deadlock section in the Neo4j Java developer reference for how to avoid this: " + "https://neo4j.com/docs/java-reference/current/#transactions-deadlocks\n" + "\n" + "Details: '" + message + "'.", cause )
		 {
		 }

		 public override Status Status()
		 {
			  return Neo4Net.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected;
		 }
	}

}
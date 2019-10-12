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
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Signals that the transaction within which the failed operations ran
	/// has been terminated with <seealso cref="Transaction.terminate()"/>.
	/// </summary>
	public class TransactionTerminatedException : TransactionFailureException, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private readonly Status _status;

		 public TransactionTerminatedException( Status status ) : this( status, "" )
		 {
		 }

		 protected internal TransactionTerminatedException( Status status, string additionalInfo ) : base( "The transaction has been terminated. Retry your operation in a new transaction, " + "and you should see a successful result. " + status.Code().description() + " " + additionalInfo )
		 {
			  this._status = status;
		 }

		 public override Status Status()
		 {
			  return _status;
		 }
	}

}
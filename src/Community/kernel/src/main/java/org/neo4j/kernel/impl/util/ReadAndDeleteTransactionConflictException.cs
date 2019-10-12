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
namespace Neo4Net.Kernel.impl.util
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class ReadAndDeleteTransactionConflictException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private static readonly string _concurrentDeleteMessage = "Database elements (nodes, relationships, properties) were observed during query execution, " +
					"but got deleted by an overlapping committed transaction before the query results could be serialised. " +
					"The transaction might succeed if it is retried.";
		 private static readonly string _deletedInTransactionMessage = "Database elements (nodes, relationships, properties) were deleted in this transaction, " +
					"but were also included in the result set.";

		 private readonly bool _deletedInThisTransaction;

		 public ReadAndDeleteTransactionConflictException( bool deletedInThisTransaction ) : base( deletedInThisTransaction ? _deletedInTransactionMessage : _concurrentDeleteMessage )
		 {
			  this._deletedInThisTransaction = deletedInThisTransaction;
		 }

		 public ReadAndDeleteTransactionConflictException( bool deletedInThisTransaction, Exception cause ) : base( deletedInThisTransaction ? _deletedInTransactionMessage : _concurrentDeleteMessage, cause )
		 {
			  this._deletedInThisTransaction = deletedInThisTransaction;
		 }

		 public override Status Status()
		 {
			  return _deletedInThisTransaction ? Neo4Net.Kernel.Api.Exceptions.Status_Statement.EntityNotFound : Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Outdated;
		 }
	}

}
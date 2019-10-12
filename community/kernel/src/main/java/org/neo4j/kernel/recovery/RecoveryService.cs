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
namespace Org.Neo4j.Kernel.recovery
{

	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using TransactionCursor = Org.Neo4j.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

	public interface RecoveryService
	{
		 void StartRecovery();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.transaction.log.TransactionCursor getTransactions(org.neo4j.kernel.impl.transaction.log.LogPosition recoveryFromPosition) throws java.io.IOException;
		 TransactionCursor GetTransactions( LogPosition recoveryFromPosition );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.transaction.log.TransactionCursor getTransactionsInReverseOrder(org.neo4j.kernel.impl.transaction.log.LogPosition recoveryFromPosition) throws java.io.IOException;
		 TransactionCursor GetTransactionsInReverseOrder( LogPosition recoveryFromPosition );

		 RecoveryStartInformation RecoveryStartInformation { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RecoveryApplier getRecoveryApplier(org.neo4j.storageengine.api.TransactionApplicationMode mode) throws Exception;
		 RecoveryApplier GetRecoveryApplier( TransactionApplicationMode mode );

		 void TransactionsRecovered( CommittedTransactionRepresentation lastRecoveredTransaction, LogPosition positionAfterLastRecoveredTransaction );
	}

}
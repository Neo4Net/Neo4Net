﻿/*
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
namespace Neo4Net.Kernel.impl.transaction.stats
{
	public interface TransactionCounters
	{
		 long PeakConcurrentNumberOfTransactions { get; }

		 long NumberOfStartedTransactions { get; }

		 long NumberOfCommittedTransactions { get; }

		 long NumberOfCommittedReadTransactions { get; }

		 long NumberOfCommittedWriteTransactions { get; }

		 long NumberOfActiveTransactions { get; }

		 long NumberOfActiveReadTransactions { get; }

		 long NumberOfActiveWriteTransactions { get; }

		 long NumberOfTerminatedTransactions { get; }

		 long NumberOfTerminatedReadTransactions { get; }

		 long NumberOfTerminatedWriteTransactions { get; }

		 long NumberOfRolledBackTransactions { get; }

		 long NumberOfRolledBackReadTransactions { get; }

		 long NumberOfRolledBackWriteTransactions { get; }
	}

}
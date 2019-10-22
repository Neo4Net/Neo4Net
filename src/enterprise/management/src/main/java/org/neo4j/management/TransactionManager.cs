﻿/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.management
{
	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	[ManagementInterface(name : TransactionManager_Fields.NAME), Description("Information about the Neo4Net transaction manager")]
	public interface TransactionManager
	{

		 [Description("The number of currently open transactions")]
		 long NumberOfOpenTransactions { get; }

		 [Description("The highest number of transactions ever opened concurrently")]
		 long PeakNumberOfConcurrentTransactions { get; }

		 [Description("The total number started transactions")]
		 long NumberOfOpenedTransactions { get; }

		 [Description("The total number of committed transactions")]
		 long NumberOfCommittedTransactions { get; }

		 [Description("The total number of rolled back transactions")]
		 long NumberOfRolledBackTransactions { get; }

		 [Description("The id of the latest committed transaction")]
		 long LastCommittedTxId { get; }
	}

	public static class TransactionManager_Fields
	{
		 public const string NAME = "Transactions";
	}

}
/*
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
namespace Org.Neo4j.management
{
	using Description = Org.Neo4j.Jmx.Description;
	using ManagementInterface = Org.Neo4j.Jmx.ManagementInterface;

	[ManagementInterface(name : TransactionManager_Fields.NAME), Description("Information about the Neo4j transaction manager")]
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
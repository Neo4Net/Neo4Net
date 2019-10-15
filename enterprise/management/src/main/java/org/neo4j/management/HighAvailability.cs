﻿using System;

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

	[Obsolete, ManagementInterface(name : HighAvailability_Fields.NAME), Description("Information about an instance participating in a HA cluster")]
	public interface HighAvailability
	{

		 [Description("The identifier used to identify this server in the HA cluster")]
		 string InstanceId { get; }

		 [Description("Whether this instance is available or not")]
		 bool Available { get; }

		 [Description("Whether this instance is alive or not")]
		 bool Alive { get; }

		 [Description("The role this instance has in the cluster")]
		 string Role { get; }

		 [Description("The time when the data on this instance was last updated from the master")]
		 string LastUpdateTime { get; }

		 [Description("The latest transaction id present in this instance's store")]
		 long LastCommittedTxId { get; }

		 [Description("Information about all instances in this cluster")]
		 ClusterMemberInfo[] InstancesInCluster { get; }

		 [Description("(If this is a slave) Update the database on this " + "instance with the latest transactions from the master")]
		 string Update();
	}

	public static class HighAvailability_Fields
	{
		 public const string NAME = "High Availability";
	}

}
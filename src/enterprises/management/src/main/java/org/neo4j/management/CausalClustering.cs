/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.management
{
	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	[ManagementInterface(name : CausalClustering_Fields.NAME), Description("Information about an instance participating in a causal cluster")]
	public interface CausalClustering
	{

		 [Description("The current role this member has in the cluster")]
		 string Role { get; }

		 [Description("The total amount of disk space used by the raft log, in bytes")]
		 long RaftLogSize { get; }

		 [Description("The total amount of disk space used by the replicated states, in bytes")]
		 long ReplicatedStateSize { get; }
	}

	public static class CausalClustering_Fields
	{
		 public const string NAME = "Causal Clustering";
	}

}
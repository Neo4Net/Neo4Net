﻿using System.Collections.Generic;

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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{

	/// <summary>
	/// Context for the Learner Paxos state machine.
	/// </summary>
	public interface LearnerContext : TimeoutsContext, LoggingContext, ConfigurationContext
	{
		 /*
		  * How many instances the coordinator will allow to be open before taking drastic action and delivering them
		  */

		 long LastDeliveredInstanceId { get;set; }


		 long LastLearnedInstanceId { get; }

		 long LastKnownLearnedInstanceInCluster { get; }

		 void LearnedInstanceId( long instanceId );

		 bool HasDeliveredAllKnownInstances();

		 void Leave();

		 PaxosInstance GetPaxosInstance( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId instanceId );

		 AtomicBroadcastSerializer NewSerializer();

		 IEnumerable<Org.Neo4j.cluster.InstanceId> Alive { get; }

		 long NextInstanceId { set; }

		 void NotifyLearnMiss( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId instanceId );

		 Org.Neo4j.cluster.InstanceId LastKnownAliveUpToDateInstance { get; }

		 void SetLastKnownLearnedInstanceInCluster( long lastKnownLearnedInstanceInCluster, Org.Neo4j.cluster.InstanceId instanceId );
	}

	public static class LearnerContext_Fields
	{
		 public const int LEARN_GAP_THRESHOLD = 10;
	}

}
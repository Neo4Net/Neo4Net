using System.Collections.Generic;

/*
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
namespace Neo4Net.cluster.protocol.heartbeat
{


	/// <summary>
	/// Context used by the <seealso cref="HeartbeatState"/> state machine.
	/// </summary>
	public interface HeartbeatContext : TimeoutsContext, ConfigurationContext, LoggingContext
	{
		 void Started();

		 /// <returns> True iff the node was suspected </returns>
		 bool Alive( InstanceId node );

		 void Suspect( InstanceId node );

		 void Suspicions( InstanceId from, ISet<InstanceId> suspicions );

		 ISet<InstanceId> Failed { get; }

		 IEnumerable<InstanceId> Alive { get; }

		 void AddHeartbeatListener( HeartbeatListener listener );

		 void RemoveHeartbeatListener( HeartbeatListener listener );

		 void ServerLeftCluster( InstanceId node );

		 bool IsFailedBasedOnSuspicions( InstanceId node );

		 IList<InstanceId> GetSuspicionsOf( InstanceId server );

		 ISet<InstanceId> GetSuspicionsFor( InstanceId uri );

		 IEnumerable<InstanceId> OtherInstances { get; }

		 long LastKnownLearnedInstanceInCluster { get; }

		 long LastLearnedInstanceId { get; }

		 /// <summary>
		 /// Adds an instance in the failed set. Note that the <seealso cref="isFailedBasedOnSuspicions(InstanceId)"/> method does not consult this set
		 /// or adds to it, instead deriving failed status from suspicions.
		 /// </summary>
		 void Failed( InstanceId instanceId );
	}

}
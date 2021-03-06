﻿/*
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
namespace Org.Neo4j.cluster.protocol.heartbeat
{

	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class HeartbeatLeftListener : Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter
	{
		 private readonly HeartbeatContext _heartbeatContext;
		 private readonly Log _log;

		 public HeartbeatLeftListener( HeartbeatContext heartbeatContext, LogProvider logProvider )
		 {
			  this._heartbeatContext = heartbeatContext;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void LeftCluster( InstanceId instanceId, URI member )
		 {
			  if ( _heartbeatContext.isFailedBasedOnSuspicions( instanceId ) )
			  {
					_log.warn( "Instance " + instanceId + " (" + member + ") has left the cluster " + "but is still treated as failed by HeartbeatContext" );

					_heartbeatContext.serverLeftCluster( instanceId );
			  }
		 }
	}

}
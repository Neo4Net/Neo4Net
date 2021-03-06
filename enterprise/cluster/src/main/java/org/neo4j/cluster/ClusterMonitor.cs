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
namespace Org.Neo4j.cluster
{
	using BindingNotifier = Org.Neo4j.cluster.com.BindingNotifier;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using Heartbeat = Org.Neo4j.cluster.protocol.heartbeat.Heartbeat;

	/// <summary>
	/// Bundles up different ways of listening in on events going on in a cluster.
	/// 
	/// <seealso cref="BindingNotifier"/> for notifications about which URI is used for sending
	/// events of the network. <seealso cref="Heartbeat"/> for notifications about failed/alive
	/// members. <seealso cref="addClusterListener(ClusterListener)"/>,
	/// <seealso cref="removeClusterListener(ClusterListener)"/> for getting notified about
	/// cluster membership events.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public interface ClusterMonitor : BindingNotifier, Heartbeat
	{
		 void AddClusterListener( ClusterListener listener );

		 void RemoveClusterListener( ClusterListener listener );
	}

}
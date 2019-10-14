using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.discovery
{

	public class CoreTopologyListenerService
	{
		 private readonly ISet<CoreTopologyService_Listener> _listeners;

		 public CoreTopologyListenerService()
		 {
			  this._listeners = ConcurrentDictionary.newKeySet();
		 }

		 public virtual void AddCoreTopologyListener( CoreTopologyService_Listener listener )
		 {
			  _listeners.Add( listener );
		 }

		 public virtual void RemoveCoreTopologyListener( CoreTopologyService_Listener listener )
		 {
			  _listeners.remove( listener );
		 }

		 public virtual void NotifyListeners( CoreTopology coreTopology )
		 {
			  foreach ( CoreTopologyService_Listener listener in _listeners )
			  {
					string dbName = listener.DbName();

					listener.OnCoreTopologyChange( coreTopology.FilterTopologyByDb( dbName ) );
			  }
		 }
	}

}
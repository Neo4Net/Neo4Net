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
namespace Org.Neo4j.causalclustering.identity
{

	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;

	public class BoundState
	{
		 private readonly ClusterId _clusterId;
		 private readonly CoreSnapshot _snapshot;

		 internal BoundState( ClusterId clusterId ) : this( clusterId, null )
		 {
		 }

		 internal BoundState( ClusterId clusterId, CoreSnapshot snapshot )
		 {
			  this._clusterId = clusterId;
			  this._snapshot = snapshot;
		 }

		 public virtual ClusterId ClusterId()
		 {
			  return _clusterId;
		 }

		 public virtual Optional<CoreSnapshot> Snapshot()
		 {
			  return Optional.ofNullable( _snapshot );
		 }
	}

}
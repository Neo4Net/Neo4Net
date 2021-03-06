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
namespace Org.Neo4j.Kernel.ha.cluster
{

	using InstanceId = Org.Neo4j.cluster.InstanceId;

	/// <summary>
	/// This event represents a change in the cluster members internal state. The possible states
	/// are enumerated in <seealso cref="HighAvailabilityMemberState"/>.
	/// </summary>
	public class HighAvailabilityMemberChangeEvent
	{
		 private readonly HighAvailabilityMemberState _oldState;
		 private readonly HighAvailabilityMemberState _newState;
		 private readonly InstanceId _instanceId;
		 private readonly URI _serverHaUri;

		 public HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState oldState, HighAvailabilityMemberState newState, InstanceId instanceId, URI serverHaUri )
		 {
			  this._oldState = oldState;
			  this._newState = newState;
			  this._instanceId = instanceId;
			  this._serverHaUri = serverHaUri;
		 }

		 public virtual HighAvailabilityMemberState OldState
		 {
			 get
			 {
				  return _oldState;
			 }
		 }

		 public virtual HighAvailabilityMemberState NewState
		 {
			 get
			 {
				  return _newState;
			 }
		 }

		 public virtual InstanceId InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual URI ServerHaUri
		 {
			 get
			 {
				  return _serverHaUri;
			 }
		 }

		 public override string ToString()
		 {
			  return "HA Member State Event[ old state: " + _oldState + ", new state: " + _newState +
						 ", server cluster URI: " + _instanceId + ", server HA URI: " + _serverHaUri + "]";
		 }
	}

}
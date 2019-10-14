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
namespace Neo4Net.Kernel.ha.id
{

	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using KernelTransactionsSnapshot = Neo4Net.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using IdReuseEligibility = Neo4Net.Kernel.impl.store.id.IdReuseEligibility;

	/// <summary>
	/// This <seealso cref="IdReuseEligibility"/> only buffer ids for reuse when we're the master.
	/// This is mostly an optimization since when in slave role the ids are thrown away anyway.
	/// </summary>
	public class HaIdReuseEligibility : IdReuseEligibility
	{
		 private readonly ClusterMembers _members;
		 private readonly Clock _clock;
		 private readonly long _idReuseSafeZone;

		 public HaIdReuseEligibility( ClusterMembers members, Clock clock, long idReuseSafeZone )
		 {
			  this._members = members;
			  this._clock = clock;
			  this._idReuseSafeZone = idReuseSafeZone;
		 }

		 public override bool IsEligible( KernelTransactionsSnapshot snapshot )
		 {
			  switch ( _members.CurrentMemberRole )
			  {
			  case HighAvailabilityModeSwitcher.SLAVE:
					// If we're slave right now then just release them because the id generators in slave mode
					// will throw them away anyway, no need to keep them in memory. The architecture around
					// how buffering is done isn't a 100% fit for HA since the wrapping if IdGeneratorFactory
					// where the buffering takes place is done in a place which is oblivious to HA and roles
					// which means that buffering will always take place. For now we'll have to live with
					// always buffering and only just release them as soon as possible when slave.
					return true;
			  case HighAvailabilityModeSwitcher.MASTER:
					// If we're master then we have to keep these ids around during the configured safe zone time
					// so that slaves have a chance to read consistently as well (slaves will know and compensate
					// for falling outside of safe zone).
					return _clock.millis() - snapshot.SnapshotTime() >= _idReuseSafeZone;
			  default:
					// If we're anything other than slave, i.e. also pending then retain the ids since we're
					// not quite sure what state we're in at the moment and we clear the id buffers anyway
					// during state switch.
					return false;
			  }
		 }
	}

}
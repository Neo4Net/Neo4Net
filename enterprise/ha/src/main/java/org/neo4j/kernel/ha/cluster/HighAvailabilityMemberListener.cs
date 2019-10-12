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
namespace Org.Neo4j.Kernel.ha.cluster
{
	/// <summary>
	/// These callback methods correspond to broadcasted HA events. The supplied event argument contains the
	/// result of the state change and required information, as interpreted by the HA state machine.
	/// </summary>
	public interface HighAvailabilityMemberListener
	{
		 void MasterIsElected( HighAvailabilityMemberChangeEvent @event );

		 void MasterIsAvailable( HighAvailabilityMemberChangeEvent @event );

		 void SlaveIsAvailable( HighAvailabilityMemberChangeEvent @event );

		 void InstanceStops( HighAvailabilityMemberChangeEvent @event );

		 /// <summary>
		 /// This event is different than the rest, in the sense that it is not a response to a broadcasted message,
		 /// rather than the interpretation of the loss of connectivity to other cluster members. This corresponds generally
		 /// to a loss of quorum but a special case is the event of being partitioned away completely from the cluster.
		 /// </summary>
		 void InstanceDetached( HighAvailabilityMemberChangeEvent @event );
	}

	 public class HighAvailabilityMemberListener_Adapter : HighAvailabilityMemberListener
	 {
		  public override void MasterIsElected( HighAvailabilityMemberChangeEvent @event )
		  {
		  }

		  public override void MasterIsAvailable( HighAvailabilityMemberChangeEvent @event )
		  {
		  }

		  public override void SlaveIsAvailable( HighAvailabilityMemberChangeEvent @event )
		  {
		  }

		  public override void InstanceStops( HighAvailabilityMemberChangeEvent @event )
		  {
		  }

		  public override void InstanceDetached( HighAvailabilityMemberChangeEvent @event )
		  {
		  }
	 }

}
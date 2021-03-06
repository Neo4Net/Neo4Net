﻿using System;
using System.Collections.Generic;

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

	using OnlineBackupKernelExtension = Org.Neo4j.backup.OnlineBackupKernelExtension;
	using MemberIsAvailable = Org.Neo4j.cluster.member.paxos.MemberIsAvailable;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;

	/*
	 * Filters existing events in a snapshot while adding new ones. Ensures that the snapshot is consistent in the
	 * face of failures of instances in the cluster.
	 */
	[Serializable]
	public class HANewSnapshotFunction : System.Func<IEnumerable<MemberIsAvailable>, MemberIsAvailable, IEnumerable<MemberIsAvailable>>
	{
		 private const long SERIAL_VERSION_UID = -8065136460852260734L;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.cluster.member.paxos.MemberIsAvailable> apply(Iterable<org.neo4j.cluster.member.paxos.MemberIsAvailable> previousSnapshot, final org.neo4j.cluster.member.paxos.MemberIsAvailable newMessage)
		 public override IEnumerable<MemberIsAvailable> Apply( IEnumerable<MemberIsAvailable> previousSnapshot, MemberIsAvailable newMessage )
		 {
			  /*
			   * If a master event is received, all events that set to slave that instance should be removed. The same
			   * should happen to existing master events and backup events, no matter which instance they are for
			   */
			  if ( newMessage.Role.Equals( MASTER ) )
			  {
					IList<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
					foreach ( MemberIsAvailable existing in previousSnapshot )
					{
						 if ( ( IsSlave( existing ) && SameIds( newMessage, existing ) ) || IsMaster( existing ) )
						 {
							  continue;
						 }
						 result.Add( existing );
					}
					result.Add( newMessage );
					return result;
			  }
			  /*
			   * If a slave event is received, all existing slave events for that instance should be removed. The same for
			   * master and backup, which means remove all events for that instance.
			   */
			  else if ( newMessage.Role.Equals( SLAVE ) )
			  {
					IList<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
					foreach ( MemberIsAvailable existing in previousSnapshot )
					{
						 if ( SameIds( newMessage, existing ) )
						 {
							  continue;
						 }
						 result.Add( existing );
					}
					result.Add( newMessage );
					return result;
			  }
			  else if ( newMessage.Role.Equals( OnlineBackupKernelExtension.BACKUP ) )
			  {
					IList<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
					foreach ( MemberIsAvailable existing in previousSnapshot )
					{
						 if ( existing.Role.Equals( OnlineBackupKernelExtension.BACKUP ) )
						 {
							  continue;
						 }
						 result.Add( existing );
					}
					result.Add( newMessage );
					return result;
			  }
			  return Iterables.append( newMessage, previousSnapshot );
		 }

		 private bool IsMaster( MemberIsAvailable existing )
		 {
			  return existing.Role.Equals( MASTER );
		 }

		 private bool IsSlave( MemberIsAvailable existing )
		 {
			  return existing.Role.Equals( SLAVE );
		 }

		 private bool SameIds( MemberIsAvailable newMessage, MemberIsAvailable existing )
		 {
			  return existing.InstanceId.Equals( newMessage.InstanceId );
		 }
	}

}
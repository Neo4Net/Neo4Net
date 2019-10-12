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
namespace Org.Neo4j.Kernel.impl.enterprise.@lock.forseti
{

	using SimpleBitSet = Org.Neo4j.Kernel.impl.util.collection.SimpleBitSet;

	internal class ExclusiveLock : ForsetiLockManager.Lock
	{
		 private readonly ForsetiClient _owner;

		 internal ExclusiveLock( ForsetiClient owner )
		 {
			  this._owner = owner;
		 }

		 public override void CopyHolderWaitListsInto( SimpleBitSet waitList )
		 {
			  _owner.copyWaitListTo( waitList );
		 }

		 public override int DetectDeadlock( int client )
		 {
			  return _owner.isWaitingFor( client ) ? _owner.id() : -1;
		 }

		 public override string DescribeWaitList()
		 {
			  return "ExclusiveLock[" + _owner.describeWaitList() + "]";
		 }

		 public override void CollectOwners( ISet<ForsetiClient> owners )
		 {
			  owners.Add( _owner );
		 }

		 public override string ToString()
		 {
			  return "ExclusiveLock{" +
						"owner=" + _owner +
						'}';
		 }
	}

}
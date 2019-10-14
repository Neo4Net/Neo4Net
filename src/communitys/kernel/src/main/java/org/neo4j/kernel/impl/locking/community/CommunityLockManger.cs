/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.locking.community
{

	using Config = Neo4Net.Kernel.configuration.Config;

	public class CommunityLockManger : Locks
	{
		 private readonly LockManagerImpl _manager;
		 private volatile bool _closed;

		 public CommunityLockManger( Config config, Clock clock )
		 {
			  _manager = new LockManagerImpl( new RagManager(), config, clock );
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client NewClient()
		 {
			  // We check this volatile closed flag here, which may seem like a contention overhead, but as the time
			  // of writing we apply pooling of transactions and in extension pooling of lock clients,
			  // so this method is called very rarely.
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( this + " already closed" );
			  }
			  return new CommunityLockClient( _manager );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void accept(final org.neo4j.kernel.impl.locking.Locks_Visitor visitor)
		 public override void Accept( Neo4Net.Kernel.impl.locking.Locks_Visitor visitor )
		 {
			  _manager.accept(element =>
			  {
				object resource = element.resource();
				if ( resource is LockResource )
				{
					 LockResource lockResource = ( LockResource ) resource;
					 visitor.Visit( lockResource.Type(), lockResource.ResourceId(), element.describe(), element.maxWaitTime(), System.identityHashCode(lockResource) );
				}
				return false;
			  });
		 }

		 public override void Close()
		 {
			  _closed = true;
		 }
	}

}
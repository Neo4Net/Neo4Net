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
namespace Org.Neo4j.Kernel.ha.com.master
{

	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;

	/// <summary>
	/// Abstraction to hold all client related info on master side.
	/// </summary>
	public class Conversation : AutoCloseable
	{
		 private Org.Neo4j.Kernel.impl.locking.Locks_Client _locks;
		 private volatile bool _active = true;
		 // since some client locks use pooling we need to be sure that
		 // there is no race between client close and stop
		 private ReentrantLock _lockClientCleanupLock = new ReentrantLock();

		 public Conversation( Org.Neo4j.Kernel.impl.locking.Locks_Client locks )
		 {
			  this._locks = locks;
		 }

		 public virtual Org.Neo4j.Kernel.impl.locking.Locks_Client Locks
		 {
			 get
			 {
				  return _locks;
			 }
		 }

		 public override void Close()
		 {
			  _lockClientCleanupLock.@lock();
			  try
			  {
					if ( _locks != null )
					{
						 _locks.close();
						 _locks = null;
						 _active = false;
					}
			  }
			  finally
			  {
					_lockClientCleanupLock.unlock();
			  }
		 }

		 public virtual bool Active
		 {
			 get
			 {
				  return _active;
			 }
		 }

		 public virtual void Stop()
		 {
			  _lockClientCleanupLock.@lock();
			  try
			  {
					if ( _locks != null )
					{
						 _locks.stop();
					}
			  }
			  finally
			  {
					_lockClientCleanupLock.unlock();
			  }
		 }

		 public override string ToString()
		 {
			  string locks = this._locks.ToString();
			  return "Conversation[lockClient: " + locks + "].";
		 }
	}

}
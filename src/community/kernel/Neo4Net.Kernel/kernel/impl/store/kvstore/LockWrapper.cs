using System.Threading;

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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using Logger = Neo4Net.Logging.Logger;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public class LockWrapper : AutoCloseable
	{
		 private static readonly bool _debugLocking = FeatureToggles.flag( typeof( AbstractKeyValueStore ), "debugLocking", false );

		 public static LockWrapper ReadLock( UpdateLock @lock, Logger logger )
		 {
			  return new LockWrapper( @lock.readLock(), @lock, logger );
		 }

		 public static LockWrapper WriteLock( UpdateLock @lock, Logger logger )
		 {
			  return new LockWrapper( @lock.writeLock(), @lock, logger );
		 }

		 private Lock @lock;

		 private LockWrapper( Lock @lock, UpdateLock managingLock, Logger logger )
		 {
			  this.@lock = @lock;
			  if ( _debugLocking )
			  {
					if ( !@lock.tryLock() )
					{
						 logger.Log( Thread.CurrentThread + " may block on " + @lock + " of " + managingLock );
						 while ( !TryLockBlocking( @lock, managingLock, logger ) )
						 {
							  logger.Log( Thread.CurrentThread + " still blocked on " + @lock + " of " + managingLock );
						 }
					}
			  }
			  else
			  {
					@lock.@lock();
			  }
		 }

		 private static bool TryLockBlocking( Lock @lock, UpdateLock managingLock, Logger logger )
		 {
			  try
			  {
					return @lock.tryLock( 1, TimeUnit.HOURS );
			  }
			  catch ( InterruptedException )
			  {
					logger.Log( Thread.CurrentThread + " ignoring interrupt while blocked on " + @lock + " of " + managingLock );
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  if ( @lock != null )
			  {
					@lock.unlock();
					@lock = null;
			  }
		 }

		 public virtual Lock Get()
		 {
			  return @lock;
		 }
	}

}
/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	/// <summary>
	/// Keeps a reference count using CAS.
	/// </summary>
	internal class ReferenceCounter
	{
		 private const int DISPOSED_VALUE = -1;

		 private AtomicInteger _count = new AtomicInteger();

		 internal virtual bool Increase()
		 {
			  while ( true )
			  {
					int pre = _count.get();
					if ( pre == DISPOSED_VALUE )
					{
						 return false;
					}
					else if ( _count.compareAndSet( pre, pre + 1 ) )
					{
						 return true;
					}
			  }
		 }

		 internal virtual void Decrease()
		 {
			  while ( true )
			  {
					int pre = _count.get();
					if ( pre <= 0 )
					{
						 throw new System.InvalidOperationException( "Illegal count: " + pre );
					}
					else if ( _count.compareAndSet( pre, pre - 1 ) )
					{
						 return;
					}
			  }
		 }

		 /// <summary>
		 /// Idempotently try to dispose this reference counter.
		 /// </summary>
		 /// <returns> True if the reference counter was or is now disposed. </returns>
		 internal virtual bool TryDispose()
		 {
			  return _count.get() == DISPOSED_VALUE || _count.compareAndSet(0, DISPOSED_VALUE);
		 }

		 public virtual int Get()
		 {
			  return _count.get();
		 }
	}

}
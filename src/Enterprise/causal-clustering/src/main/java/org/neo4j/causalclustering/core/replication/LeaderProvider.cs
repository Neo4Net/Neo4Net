using System.Threading;

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
namespace Neo4Net.causalclustering.core.replication
{
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	internal class LeaderProvider
	{
		 private MemberId _currentLeader;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized org.neo4j.causalclustering.identity.MemberId awaitLeader() throws InterruptedException
		 internal virtual MemberId AwaitLeader()
		 {
			 lock ( this )
			 {
				  while ( _currentLeader == null )
				  {
						Monitor.Wait( this );
				  }
				  return _currentLeader;
			 }
		 }

		 internal virtual MemberId Leader
		 {
			 set
			 {
				 lock ( this )
				 {
					  this._currentLeader = value;
					  if ( _currentLeader != null )
					  {
							Monitor.PulseAll( this );
					  }
				 }
			 }
		 }

		 internal virtual MemberId CurrentLeader()
		 {
			  return _currentLeader;
		 }
	}

}
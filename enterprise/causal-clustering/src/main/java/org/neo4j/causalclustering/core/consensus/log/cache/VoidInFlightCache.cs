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
namespace Org.Neo4j.causalclustering.core.consensus.log.cache
{

	/// <summary>
	/// A cache which caches nothing. This means that all lookups
	/// will go to the on-disk Raft log, which might be quite good
	/// anyway since recently written items will be in OS page cache
	/// memory generally. But it will incur an unmarshalling overhead.
	/// </summary>
	public class VoidInFlightCache : InFlightCache
	{
		 public override void Enable()
		 {
		 }

		 public override void Put( long logIndex, RaftLogEntry entry )
		 {
		 }

		 public override RaftLogEntry Get( long logIndex )
		 {
			  return null;
		 }

		 public override void Truncate( long fromIndex )
		 {
		 }

		 public override void Prune( long upToIndex )
		 {
		 }

		 public override long TotalBytes()
		 {
			  return 0;
		 }

		 public override int ElementCount()
		 {
			  return 0;
		 }
	}

}
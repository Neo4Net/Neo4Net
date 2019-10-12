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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{

	/// <summary>
	/// This cache is not meant for production use, but it can be useful
	/// in various investigative circumstances.
	/// </summary>
	public class UnboundedInFlightCache : InFlightCache
	{
		 private IDictionary<long, RaftLogEntry> _map = new Dictionary<long, RaftLogEntry>();
		 private bool _enabled;

		 public override void Enable()
		 {
			 lock ( this )
			 {
				  _enabled = true;
			 }
		 }

		 public override void Put( long logIndex, RaftLogEntry entry )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _map[logIndex] = entry;
			 }
		 }

		 public override RaftLogEntry Get( long logIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return null;
				  }
      
				  return _map[logIndex];
			 }
		 }

		 public override void Truncate( long fromIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _map.Keys.removeIf( idx => idx >= fromIndex );
			 }
		 }

		 public override void Prune( long upToIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _map.Keys.removeIf( idx => idx <= upToIndex );
			 }
		 }

		 public override long TotalBytes()
		 {
			 lock ( this )
			 {
				  // not updated correctly
				  return 0;
			 }
		 }

		 public override int ElementCount()
		 {
			 lock ( this )
			 {
				  // not updated correctly
				  return 0;
			 }
		 }
	}

}
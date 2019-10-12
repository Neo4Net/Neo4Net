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
namespace Neo4Net.causalclustering.discovery
{

	public class TopologyDifference
	{
		 private ISet<Difference> _added;
		 private ISet<Difference> _removed;

		 internal TopologyDifference( ISet<Difference> added, ISet<Difference> removed )
		 {
			  this._added = added;
			  this._removed = removed;
		 }

		 internal virtual ISet<Difference> Added()
		 {
			  return _added;
		 }

		 internal virtual ISet<Difference> Removed()
		 {
			  return _removed;
		 }

		 public virtual bool HasChanges()
		 {
			  return _added.Count > 0 || _removed.Count > 0;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{{added={0}, removed={1}}}", _added, _removed );
		 }
	}

}
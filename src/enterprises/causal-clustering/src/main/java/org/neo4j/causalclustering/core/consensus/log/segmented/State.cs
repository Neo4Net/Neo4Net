/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	/// <summary>
	/// Collects all the state that must be recovered after a restart.
	/// </summary>
	public class State
	{
		 internal Segments Segments;
		 internal Terms Terms;

		 internal long PrevIndex = -1;
		 internal long PrevTerm = -1;
		 internal long AppendIndex = -1;

		 public override string ToString()
		 {
			  return "State{" +
						"prevIndex=" + PrevIndex +
						", prevTerm=" + PrevTerm +
						", appendIndex=" + AppendIndex +
						'}';
		 }
	}

}
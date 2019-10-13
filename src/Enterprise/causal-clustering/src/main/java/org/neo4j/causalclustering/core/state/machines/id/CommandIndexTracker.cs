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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	/// <summary>
	/// Keeps track of the raft command index of last applied transaction.
	/// 
	/// As soon as a transaction is successfully applied this will be updated to reflect that.
	/// </summary>
	public class CommandIndexTracker
	{
		 private volatile long _appliedCommandIndex;

		 public virtual long AppliedCommandIndex
		 {
			 set
			 {
				  this._appliedCommandIndex = value;
			 }
			 get
			 {
				  return _appliedCommandIndex;
			 }
		 }

	}

}
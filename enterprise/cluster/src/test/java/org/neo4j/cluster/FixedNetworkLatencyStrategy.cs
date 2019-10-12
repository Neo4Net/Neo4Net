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
namespace Org.Neo4j.cluster
{
	using Org.Neo4j.cluster.com.message;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;

	/// <summary>
	/// Messages never gets lost
	/// </summary>
	public class FixedNetworkLatencyStrategy : NetworkLatencyStrategy
	{
		 private long _delay;

		 public FixedNetworkLatencyStrategy() : this(0)
		 {
		 }

		 public FixedNetworkLatencyStrategy( long delay )
		 {
			  this._delay = delay;
		 }

		 public override long MessageDelay<T1>( Message<T1> message, string serverIdTo ) where T1 : Org.Neo4j.cluster.com.message.MessageType
		 {
			  return _delay;
		 }
	}

}
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
namespace Org.Neo4j.causalclustering.core.state
{

	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;

	public class RaftLogPruner
	{
		 private readonly RaftMachine _raftMachine;
		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly Clock _clock;

		 public RaftLogPruner( RaftMachine raftMachine, CommandApplicationProcess applicationProcess, Clock clock )
		 {

			  this._raftMachine = raftMachine;
			  this._applicationProcess = applicationProcess;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void prune() throws java.io.IOException
		 public virtual void Prune()
		 {
			  _raftMachine.handle(Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantAwareMessage.of(_clock.instant(), new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PruneRequest(_applicationProcess.lastFlushed())
			 ));
		 }
	}

}
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
namespace Org.Neo4j.causalclustering.core.replication
{

	using Org.Neo4j.causalclustering.core.state.machines;

	public class DirectReplicator<Command> : Replicator where Command : ReplicatedContent
	{
		 private readonly StateMachine<Command> _stateMachine;
		 private long _commandIndex;

		 public DirectReplicator( StateMachine<Command> stateMachine )
		 {
			  this._stateMachine = stateMachine;
		 }

		 public override Future<object> Replicate( ReplicatedContent content, bool trackResult )
		 {
			 lock ( this )
			 {
				  AtomicReference<CompletableFuture<object>> futureResult = new AtomicReference<CompletableFuture<object>>( new CompletableFuture<>() );
				  _stateMachine.applyCommand((Command) content, _commandIndex++, result =>
				  {
					if ( trackResult )
					{
						 futureResult.getAndUpdate( result.apply );
					}
				  });
      
				  return futureResult.get();
			 }
		 }
	}

}
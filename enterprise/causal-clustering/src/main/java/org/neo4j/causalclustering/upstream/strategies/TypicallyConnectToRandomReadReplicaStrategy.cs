﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.upstream.strategies
{

	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Service = Org.Neo4j.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.not;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class TypicallyConnectToRandomReadReplicaStrategy extends org.neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
	public class TypicallyConnectToRandomReadReplicaStrategy : UpstreamDatabaseSelectionStrategy
	{
		 public const string IDENTITY = "typically-connect-to-random-read-replica";
		 private readonly ModuloCounter _counter;

		 public TypicallyConnectToRandomReadReplicaStrategy() : this(10)
		 {
		 }

		 public TypicallyConnectToRandomReadReplicaStrategy( int connectToCoreInterval ) : base( IDENTITY )
		 {
			  this._counter = new ModuloCounter( connectToCoreInterval );
		 }

		 public override Optional<MemberId> UpstreamDatabase()
		 {
			  if ( _counter.shouldReturnCoreMemberId() )
			  {
					return RandomCoreMember();
			  }
			  else
			  {
					// shuffled members
					IList<MemberId> readReplicaMembers = new List<MemberId>( TopologyService.localReadReplicas().members().Keys );
					Collections.shuffle( readReplicaMembers );

					IList<MemberId> coreMembers = new List<MemberId>( TopologyService.localCoreServers().members().Keys );
					Collections.shuffle( coreMembers );

					return Stream.concat( readReplicaMembers.stream(), coreMembers.stream() ).filter(not(Myself.equals)).findFirst();
			  }
		 }

		 private Optional<MemberId> RandomCoreMember()
		 {
			  IList<MemberId> coreMembersNotSelf = TopologyService.localCoreServers().members().Keys.Where(not(Myself.equals)).ToList();
			  Collections.shuffle( coreMembersNotSelf );
			  if ( coreMembersNotSelf.Count == 0 )
			  {
					return null;
			  }
			  return coreMembersNotSelf[0];
		 }

		 private class ModuloCounter
		 {
			  internal readonly int Modulo;
			  internal int Counter;

			  internal ModuloCounter( int modulo )
			  {
					this.Modulo = modulo;
			  }

			  internal virtual bool ShouldReturnCoreMemberId()
			  {
					Counter = ( Counter + 1 ) % Modulo;
					return Counter == 0;
			  }
		 }
	}

}
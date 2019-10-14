using System;
using System.Collections.Generic;

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

	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	/// <summary>
	/// Replicates commands to assign next available id range to this member.
	/// </summary>
	public class ReplicatedIdRangeAcquirer
	{
		 private readonly Replicator _replicator;
		 private readonly ReplicatedIdAllocationStateMachine _idAllocationStateMachine;

		 private readonly IDictionary<IdType, int> _allocationSizes;

		 private readonly MemberId _me;
		 private readonly Log _log;

		 public ReplicatedIdRangeAcquirer( Replicator replicator, ReplicatedIdAllocationStateMachine idAllocationStateMachine, IDictionary<IdType, int> allocationSizes, MemberId me, LogProvider logProvider )
		 {
			  this._replicator = replicator;
			  this._idAllocationStateMachine = idAllocationStateMachine;
			  this._allocationSizes = allocationSizes;
			  this._me = me;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 internal virtual IdAllocation AcquireIds( IdType idType )
		 {
			  while ( true )
			  {
					long firstUnallocated = _idAllocationStateMachine.firstUnallocated( idType );
					ReplicatedIdAllocationRequest idAllocationRequest = new ReplicatedIdAllocationRequest( _me, idType, firstUnallocated, _allocationSizes[idType] );

					if ( ReplicateIdAllocationRequest( idType, idAllocationRequest ) )
					{
						 IdRange idRange = new IdRange( EMPTY_LONG_ARRAY, firstUnallocated, _allocationSizes[idType] );
						 return new IdAllocation( idRange, -1, 0 );
					}
					else
					{
						 _log.info( "Retrying ID generation due to conflict. Request was: " + idAllocationRequest );
					}
			  }
		 }

		 private bool ReplicateIdAllocationRequest( IdType idType, ReplicatedIdAllocationRequest idAllocationRequest )
		 {
			  try
			  {
					return ( bool? ) _replicator.replicate( idAllocationRequest, true ).get().Value;
			  }
			  catch ( Exception e )
			  {
					_log.warn( format( "Failed to acquire id range for idType %s", idType ), e );
					throw new IdGenerationException( e );
			  }
		 }
	}

}
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.snapshot
{
	using GlobalSessionTrackerState = Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using Neo4Net.causalclustering.core.state.storage;
	using IdAllocationState = Neo4Net.causalclustering.core.state.machines.id.IdAllocationState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReplicatedLockTokenState = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;

	public sealed class CoreStateType
	{
		 public static readonly CoreStateType LockToken = new CoreStateType( "LockToken", InnerEnum.LockToken, new Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenState.Marshal( new Neo4Net.causalclustering.identity.MemberId.Marshal() ) );
		 public static readonly CoreStateType SessionTracker = new CoreStateType( "SessionTracker", InnerEnum.SessionTracker, new Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState.Marshal( new Neo4Net.causalclustering.identity.MemberId.Marshal() ) );
		 public static readonly CoreStateType IdAllocation = new CoreStateType( "IdAllocation", InnerEnum.IdAllocation, new Neo4Net.causalclustering.core.state.machines.id.IdAllocationState.Marshal() );
		 public static readonly CoreStateType RaftCoreState = new CoreStateType( "RaftCoreState", InnerEnum.RaftCoreState, new RaftCoreState.Marshal() );

		 private static readonly IList<CoreStateType> valueList = new List<CoreStateType>();

		 static CoreStateType()
		 {
			 valueList.Add( LockToken );
			 valueList.Add( SessionTracker );
			 valueList.Add( IdAllocation );
			 valueList.Add( RaftCoreState );
		 }

		 public enum InnerEnum
		 {
			 LockToken,
			 SessionTracker,
			 IdAllocation,
			 RaftCoreState
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public readonly;

		 internal CoreStateType( string name, InnerEnum innerEnum, Neo4Net.causalclustering.core.state.storage.StateMarshal marshal )
		 {
			  this.Marshal = marshal;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		public static IList<CoreStateType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static CoreStateType valueOf( string name )
		{
			foreach ( CoreStateType enumInstance in CoreStateType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}
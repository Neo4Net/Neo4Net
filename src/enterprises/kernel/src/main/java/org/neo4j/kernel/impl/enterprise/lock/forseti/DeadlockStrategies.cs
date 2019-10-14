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
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public abstract class DeadlockStrategies : ForsetiLockManager.DeadlockResolutionStrategy
	{
		 /// <summary>
		 /// When a deadlock occurs, the client with the fewest number of held locks is aborted. If both clients hold the same
		 /// number of
		 /// locks, the client with the lowest client id is aborted.
		 /// <p/>
		 /// This is one side of a long academic argument, where the other says to abort the one with the most locks held,
		 /// since it's old and monolithic and holding up
		 /// the line.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ABORT_YOUNG { public boolean shouldAbort(ForsetiClient clientThatsAsking, ForsetiClient clientWereDeadlockedWith) { if(isSameClient(clientThatsAsking, clientWereDeadlockedWith)) { return true; } long ourCount = clientThatsAsking.activeLockCount(); long otherCount = clientWereDeadlockedWith.activeLockCount(); if(ourCount > otherCount) { return false; } else if(otherCount > ourCount) { return true; } else { return clientThatsAsking.id() >= clientWereDeadlockedWith.id(); } } },

		 /// <summary>
		 /// When a deadlock occurs, the client with the highest number of held locks is aborted. If both clients hold the
		 /// same number of
		 /// locks, the client with the highest client id is aborted.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ABORT_OLD { public boolean shouldAbort(ForsetiClient clientThatsAsking, ForsetiClient clientWereDeadlockedWith) { if(isSameClient(clientThatsAsking, clientWereDeadlockedWith)) { return true; } return !ABORT_YOUNG.shouldAbort(clientThatsAsking, clientWereDeadlockedWith); } },

		 /// <summary>
		 /// When a deadlock occurs, the client that is blocking the lowest number of other clients aborts.
		 /// If both clients have the same sized wait lists, the one with the lowest client id is aborted.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ABORT_SHORT_WAIT_LIST { public boolean shouldAbort(ForsetiClient clientThatsAsking, ForsetiClient clientWereDeadlockedWith) { if(isSameClient(clientThatsAsking, clientWereDeadlockedWith)) { return true; } int ourCount = clientThatsAsking.waitListSize(); int otherCount = clientWereDeadlockedWith.waitListSize(); if(ourCount > otherCount) { return false; } else if(otherCount > ourCount) { return true; } else { return clientThatsAsking.id() > clientWereDeadlockedWith.id(); } } },

		 /// <summary>
		 /// When a deadlock occurs, the client that is blocking the highest number of other clients aborts.
		 /// If both clients have the same sized wait lists, the one with the highest client id is aborted.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ABORT_LONG_WAIT_LIST { public boolean shouldAbort(ForsetiClient clientThatsAsking, ForsetiClient clientWereDeadlockedWith) { if(isSameClient(clientThatsAsking, clientWereDeadlockedWith)) { return true; } return !ABORT_SHORT_WAIT_LIST.shouldAbort(clientThatsAsking, clientWereDeadlockedWith); } };

		 private static readonly IList<DeadlockStrategies> valueList = new List<DeadlockStrategies>();

		 static DeadlockStrategies()
		 {
			 valueList.Add( ABORT_YOUNG );
			 valueList.Add( ABORT_OLD );
			 valueList.Add( ABORT_SHORT_WAIT_LIST );
			 valueList.Add( ABORT_LONG_WAIT_LIST );
		 }

		 public enum InnerEnum
		 {
			 ABORT_YOUNG,
			 ABORT_OLD,
			 ABORT_SHORT_WAIT_LIST,
			 ABORT_LONG_WAIT_LIST
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DeadlockStrategies( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract bool shouldAbort( ForsetiClient clientThatsAsking, ForsetiClient clientWereDeadlockedWith );

		 /// <summary>
		 /// To aid in experimental testing of strategies on different real workloads, allow toggling which strategy to use.
		 /// </summary>
		 public static readonly ForsetiLockManager.DeadlockResolutionStrategy DEFAULT = Neo4Net.Utils.FeatureToggles.flag( DeadlockStrategies.class, "strategy", ABORT_YOUNG );

		 public static readonly DeadlockStrategies private static boolean isSameClient( ForsetiClient a, ForsetiClient b ) { return a.id() == b.id(); } = new DeadlockStrategies("private static boolean isSameClient(ForsetiClient a, ForsetiClient b) { return a.id() == b.id(); }", InnerEnum.private static boolean isSameClient(ForsetiClient a, ForsetiClient b) { return a.id() == b.id(); });

		public static IList<DeadlockStrategies> values()
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

		public static DeadlockStrategies valueOf( string name )
		{
			foreach ( DeadlockStrategies enumInstance in DeadlockStrategies.valueList )
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
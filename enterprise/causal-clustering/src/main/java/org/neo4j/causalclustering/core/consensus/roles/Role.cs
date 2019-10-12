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
namespace Org.Neo4j.causalclustering.core.consensus.roles
{

	public sealed class Role
	{
		 public static readonly Role Follower = new Role( "Follower", InnerEnum.Follower, new Follower() );
		 public static readonly Role Candidate = new Role( "Candidate", InnerEnum.Candidate, new Candidate() );
		 public static readonly Role Leader = new Role( "Leader", InnerEnum.Leader, new Leader() );

		 private static readonly IList<Role> valueList = new List<Role>();

		 static Role()
		 {
			 valueList.Add( Follower );
			 valueList.Add( Candidate );
			 valueList.Add( Leader );
		 }

		 public enum InnerEnum
		 {
			 Follower,
			 Candidate,
			 Leader
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public readonly;

		 internal Role( string name, InnerEnum innerEnum, Org.Neo4j.causalclustering.core.consensus.RaftMessageHandler handler )
		 {
			  this.Handler = handler;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		public static IList<Role> values()
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

		public static Role valueOf( string name )
		{
			foreach ( Role enumInstance in Role.valueList )
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
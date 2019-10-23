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
namespace Neo4Net.causalclustering.discovery
{
	public sealed class IpFamily
	{
		 // this assumes that localhost resolves to IPv4, can be changed if problematic (behaviour was already in place)
		 public static readonly IpFamily Ipv4 = new IpFamily( "Ipv4", InnerEnum.Ipv4, "localhost", "127.0.0.1", "0.0.0.0" );
		 public static readonly IpFamily Ipv6 = new IpFamily( "Ipv6", InnerEnum.Ipv6, "::1", "::1", "::" );

		 private static readonly IList<IpFamily> valueList = new List<IpFamily>();

		 static IpFamily()
		 {
			 valueList.Add( Ipv4 );
			 valueList.Add( Ipv6 );
		 }

		 public enum InnerEnum
		 {
			 Ipv4,
			 Ipv6
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private String;
		 internal Private String;
		 internal Private String;

		 internal IpFamily( string name, InnerEnum innerEnum, string localhostName, string localhostAddress, string wildcardAddress )
		 {
			  this._localhostName = localhostName;
			  this._localhostAddress = localhostAddress;
			  this._wildcardAddress = wildcardAddress;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string LocalhostName()
		 {
			  return _localhostName;
		 }

		 public string LocalhostAddress()
		 {
			  return _localhostAddress;
		 }

		 public string WildcardAddress()
		 {
			  return _wildcardAddress;
		 }

		public static IList<IpFamily> values()
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

		public static IpFamily ValueOf( string name )
		{
			foreach ( IpFamily enumInstance in IpFamily.valueList )
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
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
namespace Neo4Net.causalclustering.routing.load_balancing.procedure
{
	/// <summary>
	/// Enumerates the parameter names used for the GetServers
	/// procedures in a causal cluster.
	/// </summary>
	public sealed class ParameterNames
	{
		 /// <summary>
		 /// Type: IN
		 /// 
		 /// An opaque key-value map for supplying client context.
		 /// 
		 /// Refer to the specific routing plugin deployed to
		 /// understand which specific keys can be utilised.
		 /// </summary>
		 public static readonly ParameterNames Context = new ParameterNames( "Context", InnerEnum.Context, "context" );

		 /// <summary>
		 /// Type: OUT
		 /// 
		 /// Contains a map of endpoints and their associated capability.
		 /// 
		 /// Refer to the protocol specification to understand the
		 /// exact format and how to utilise it.
		 /// </summary>
		 public static readonly ParameterNames Servers = new ParameterNames( "Servers", InnerEnum.Servers, "servers" );

		 /// <summary>
		 /// Type: OUT
		 /// 
		 /// Defines the time-to-live of the returned information,
		 /// after which it shall be refreshed.
		 /// 
		 /// Refer to the specific routing plugin deployed to
		 /// understand the impact of this setting.
		 /// </summary>
		 public static readonly ParameterNames Ttl = new ParameterNames( "Ttl", InnerEnum.Ttl, "ttl" );

		 private static readonly IList<ParameterNames> valueList = new List<ParameterNames>();

		 static ParameterNames()
		 {
			 valueList.Add( Context );
			 valueList.Add( Servers );
			 valueList.Add( Ttl );
		 }

		 public enum InnerEnum
		 {
			 Context,
			 Servers,
			 Ttl
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal ParameterNames( string name, InnerEnum innerEnum, string parameterName )
		 {
			  this._parameterName = parameterName;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string ParameterName()
		 {
			  return _parameterName;
		 }

		public static IList<ParameterNames> values()
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

		public static ParameterNames ValueOf( string name )
		{
			foreach ( ParameterNames enumInstance in ParameterNames.valueList )
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
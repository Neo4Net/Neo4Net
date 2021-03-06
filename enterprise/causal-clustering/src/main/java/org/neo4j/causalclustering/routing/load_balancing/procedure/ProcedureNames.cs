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
namespace Org.Neo4j.causalclustering.routing.load_balancing.procedure
{
	using ProcedureNamesEnum = Org.Neo4j.causalclustering.routing.procedure.ProcedureNamesEnum;

	/// <summary>
	/// This is part of the cluster / driver interface specification and
	/// defines the procedure names involved in the load balancing solution.
	/// 
	/// These procedures are used by cluster driver software to discover endpoints,
	/// their capabilities and to eventually schedule work appropriately.
	/// 
	/// The intention is for this class to eventually move over to a support package
	/// which can be included by driver software.
	/// </summary>
	public sealed class ProcedureNames : ProcedureNamesEnum
	{
		 public static readonly ProcedureNames GetServersV1 = new ProcedureNames( "GetServersV1", InnerEnum.GetServersV1, "getServers" );
		 public static readonly ProcedureNames GetServersV2 = new ProcedureNames( "GetServersV2", InnerEnum.GetServersV2, "getRoutingTable" );

		 private static readonly IList<ProcedureNames> valueList = new List<ProcedureNames>();

		 static ProcedureNames()
		 {
			 valueList.Add( GetServersV1 );
			 valueList.Add( GetServersV2 );
		 }

		 public enum InnerEnum
		 {
			 GetServersV1,
			 GetServersV2
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       private static final String[] nameSpace = new String[]{"dbms", "cluster", "routing"};

		 private static readonly IList<ProcedureNames> valueList = new List<ProcedureNames>();

		 static ProcedureNames()
		 {
			 valueList.Add( GetServersV1 );
			 valueList.Add( GetServersV2 );
			 valueList.Add( private );
		 }

		 public enum InnerEnum
		 {
			 GetServersV1,
			 GetServersV2,
			 private
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 private readonly string name;

		 internal ProcedureNames( string name, InnerEnum innerEnum, string name )
		 {
			  this._name = name;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string ProcedureName()
		 {
			  return _name;
		 }

		 public string[] ProcedureNameSpace()
		 {
			  return _nameSpace;
		 }

		public static IList<ProcedureNames> values()
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

		public static ProcedureNames valueOf( string name )
		{
			foreach ( ProcedureNames enumInstance in ProcedureNames.valueList )
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
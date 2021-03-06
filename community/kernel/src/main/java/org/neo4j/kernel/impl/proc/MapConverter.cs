﻿using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.proc
{

	using DefaultParameterValue = Org.Neo4j.@internal.Kernel.Api.procs.DefaultParameterValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.DefaultParameterValue.ntMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.proc.ParseUtil.parseMap;

	/// <summary>
	/// A naive implementation of a Cypher-map/json parser. If you find yourself using this
	/// for parsing huge json-document in a place where performance matters - you probably need
	/// to rethink your decision.
	/// </summary>
	public class MapConverter : System.Func<string, DefaultParameterValue>
	{
		 public override DefaultParameterValue Apply( string s )
		 {
			  string value = s.Trim();
			  if ( value.Equals( "null", StringComparison.OrdinalIgnoreCase ) )
			  {
					return ntMap( null );
			  }
			  else
			  {
					return ntMap( parseMap( value ) );
			  }
		 }
	}

}
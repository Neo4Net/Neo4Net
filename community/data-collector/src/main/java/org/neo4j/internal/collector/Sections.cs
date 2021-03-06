﻿/*
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
namespace Org.Neo4j.@internal.Collector
{

	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;

	internal class Sections
	{
		 private Sections()
		 { // only static methods
		 }

		 internal const string GRAPH_COUNTS = "GRAPH COUNTS";
		 internal const string TOKENS = "TOKENS";
		 internal const string META = "META";
		 internal const string QUERIES = "QUERIES";

		 private static readonly string[] _sections = new string[] { GRAPH_COUNTS, TOKENS, QUERIES };
		 private static readonly string _names = Arrays.ToString( _sections );

		 internal static InvalidArgumentsException UnknownSectionException( string section )
		 {
			  return new InvalidArgumentsException( string.Format( "Unknown section '{0}', known sections are {1}", section, _names ) );
		 }
	}

}
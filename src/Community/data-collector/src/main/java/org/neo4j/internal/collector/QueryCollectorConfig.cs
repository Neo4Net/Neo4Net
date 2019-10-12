using System.Collections.Generic;

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
namespace Neo4Net.@internal.Collector
{

	using IntOption = Neo4Net.@internal.Collector.DataCollectorOptions.IntOption;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;

	/// <summary>
	/// Configuration of collect('QUERIES', config) procedure call.
	/// </summary>
	internal class QueryCollectorConfig
	{
		 private static readonly IntOption _durationSeconds = new IntOption( "durationSeconds", -1 );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static QueryCollectorConfig of(java.util.Map<String, Object> userMap) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 internal static QueryCollectorConfig Of( IDictionary<string, object> userMap )
		 {
			  return new QueryCollectorConfig( _durationSeconds.parseOrDefault( userMap ) );
		 }

		 internal readonly int CollectSeconds;

		 private QueryCollectorConfig( int collectSeconds )
		 {
			  this.CollectSeconds = collectSeconds;
		 }
	}

}
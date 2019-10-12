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
namespace Neo4Net.Bolt.v3.runtime
{

	using CypherAdapterStream = Neo4Net.Bolt.v1.runtime.CypherAdapterStream;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;

	internal class CypherAdapterStreamV3 : CypherAdapterStream
	{
		 private const string LAST_RESULT_CONSUMED_KEY = "t_last";

		 internal CypherAdapterStreamV3( QueryResult @delegate, Clock clock ) : base( @delegate, clock )
		 {
		 }

		 protected internal override void AddRecordStreamingTime( Neo4Net.Bolt.runtime.BoltResult_Visitor visitor, long time )
		 {
			  visitor.AddMetadata( LAST_RESULT_CONSUMED_KEY, longValue( time ) );
		 }
	}

}
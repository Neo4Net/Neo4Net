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
namespace Neo4Net.Cypher.result
{
	/// <summary>
	/// Profile of a query execution.
	/// </summary>
	public interface QueryProfile
	{
		 /// <summary>
		 /// Get profile for specific operator.
		 /// 
		 /// Note: the operator should really be type as an [[org.neo4j.cypher.internal.v3_5.util.attribution.Id]],
		 /// but this is not possible because of a bug with scala `AnyVal`s and inheritance.
		 /// 
		 /// See https://github.com/lampepfl/dotty/issues/1169 for a discussion of the same issue in Dotty.
		 /// </summary>
		 /// <param name="operatorId"> operator id </param>
		 OperatorProfile OperatorProfile( int operatorId );
	}

	public static class QueryProfile_Fields
	{
		 public static readonly QueryProfile None = operatorId => OperatorProfile.NONE;
	}

}
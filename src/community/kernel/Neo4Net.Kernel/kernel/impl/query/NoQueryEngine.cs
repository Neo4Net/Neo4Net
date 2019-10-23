using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.query
{
	using Result = Neo4Net.GraphDb.Result;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	internal sealed class NoQueryEngine : QueryExecutionEngine
	{
		 public static readonly NoQueryEngine Instance = new NoQueryEngine( "Instance", InnerEnum.Instance );

		 private static readonly IList<NoQueryEngine> valueList = new List<NoQueryEngine>();

		 static NoQueryEngine()
		 {
			 valueList.Add( Instance );
		 }

		 public enum InnerEnum
		 {
			 Instance
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private NoQueryEngine( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public Neo4Net.GraphDb.Result ExecuteQuery( string query, Neo4Net.Values.@virtual.MapValue parameters, TransactionalContext context )
		 {
			  throw NoQueryEngineConflict();
		 }

		 public Neo4Net.GraphDb.Result ProfileQuery( string query, Neo4Net.Values.@virtual.MapValue parameter, TransactionalContext context )
		 {
			  throw NoQueryEngineConflict();
		 }

		 public bool IsPeriodicCommit( string query )
		 {
			  throw NoQueryEngineConflict();
		 }

		 public long ClearQueryCaches()
		 {
			  throw NoQueryEngineConflict();
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 private Exception NoQueryEngineConflict()
		 {
			  return new System.NotSupportedException( "No query engine installed." );
		 }

		public static IList<NoQueryEngine> values()
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

		public static NoQueryEngine ValueOf( string name )
		{
			foreach ( NoQueryEngine enumInstance in NoQueryEngine.valueList )
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
﻿using System;
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
namespace Org.Neo4j.Kernel.impl.query
{
	using Result = Org.Neo4j.Graphdb.Result;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

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

		 public Org.Neo4j.Graphdb.Result ExecuteQuery( string query, Org.Neo4j.Values.@virtual.MapValue parameters, TransactionalContext context )
		 {
			  throw NoQueryEngineConflict();
		 }

		 public Org.Neo4j.Graphdb.Result ProfileQuery( string query, Org.Neo4j.Values.@virtual.MapValue parameter, TransactionalContext context )
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

		public static NoQueryEngine valueOf( string name )
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
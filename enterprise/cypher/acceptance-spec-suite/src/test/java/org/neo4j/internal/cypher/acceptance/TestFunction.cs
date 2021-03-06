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
namespace Org.Neo4j.@internal.cypher.acceptance
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Result = Org.Neo4j.Graphdb.Result;
	using Context = Org.Neo4j.Procedure.Context;
	using Name = Org.Neo4j.Procedure.Name;
	using UserFunction = Org.Neo4j.Procedure.UserFunction;

	public class TestFunction
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction("test.toSet") public java.util.List<Object> toSet(@Name("values") java.util.List<Object> list)
		 [UserFunction("test.toSet")]
		 public virtual IList<object> ToSet( IList<object> list )
		 {
			  return new List<object>( new LinkedHashSet<object>( list ) );
		 }

		 [UserFunction("test.nodeList")]
		 public virtual IList<object> NodeList()
		 {
			  Result result = Db.execute( "MATCH (n) RETURN n LIMIT 1" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  object node = result.Next()["n"];
			  return Collections.singletonList( node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction("test.sum") public double sum(@Name("numbers") java.util.List<Number> list)
		 [UserFunction("test.sum")]
		 public virtual double Sum( IList<Number> list )
		 {
			  double sum = 0;
			  foreach ( Number number in list )
			  {
					sum += number.doubleValue();
			  }
			  return sum;
		 }
	}

}
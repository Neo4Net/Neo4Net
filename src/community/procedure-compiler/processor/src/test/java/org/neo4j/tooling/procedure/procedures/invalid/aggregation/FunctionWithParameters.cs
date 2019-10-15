﻿/*
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
namespace Neo4Net.Tooling.procedure.procedures.invalid.aggregation
{
	using Name = Neo4Net.Procedure.Name;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;

	public class FunctionWithParameters
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MyAggregation aggregateAllTheThings(String shouldNotHaveAnyParam)
		 public virtual MyAggregation AggregateAllTheThings( string shouldNotHaveAnyParam )
		 {
			  return new MyAggregation();
		 }

		 public class MyAggregation
		 {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("foo") long island)
			  public virtual void Update( long island )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public boolean sarahConnor()
			  public virtual bool SarahConnor()
			  {
					return false;
			  }
		 }
	}

}
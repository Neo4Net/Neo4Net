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
namespace Org.Neo4j.Test.randomized
{
	public interface Action<T, F>
	{
		 /// <returns> {@code true} if the value was applied properly and all checks verifies,
		 /// otherwise {@code false}. </returns>
		 F Apply( T target );

		 /// <summary>
		 /// For outputting a test case, this is the code that represents this action.
		 /// </summary>
		 void PrintAsCode( T source, LinePrinter @out, bool includeChecks );
	}

	 public abstract class Action_Adapter<T, F> : Action<T, F>
	 {
		 public abstract F Apply( T target );
		  public override void PrintAsCode( T source, LinePrinter @out, bool includeChecks )
		  {
				@out.Println( this.GetType().Name + "#printAsCode not implemented" );
		  }
	 }

}
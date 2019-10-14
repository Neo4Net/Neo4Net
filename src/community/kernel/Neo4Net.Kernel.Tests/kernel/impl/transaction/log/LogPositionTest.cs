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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class LogPositionTest
	public class LogPositionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public LogPosition logPositionA;
		 public LogPosition LogPositionA;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public LogPosition logPositionB;
		 public LogPosition LogPositionB;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<LogPosition[]> logPositions()
		 public static ICollection<LogPosition[]> LogPositions()
		 {
			  return Arrays.asList(new LogPosition[]
			  {
				  new LogPosition( 0, 1 ),
				  new LogPosition( 0, 0 )
			  }, new LogPosition[]
			  {
				  new LogPosition( 0, 11 ),
				  new LogPosition( 0, 7 )
			  }, new LogPosition[]
			  {
				  new LogPosition( 2, 1 ),
				  new LogPosition( 2, 0 )
			  }, new LogPosition[]
			  {
				  new LogPosition( 2, 17 ),
				  new LogPosition( 2, 15 )
			  }, new LogPosition[]
			  {
				  new LogPosition( 1, 1 ),
				  new LogPosition( 0, 1 )
			  }, new LogPosition[]
			  {
				  new LogPosition( 5, 1 ),
				  new LogPosition( 3, 10 )
			  }, new LogPosition[]
			  {
				  new LogPosition( int.MaxValue, int.MaxValue + 1L ),
				  new LogPosition( int.MaxValue, int.MaxValue )
			  }, new LogPosition[]
			  {
				  new LogPosition( long.MaxValue, long.MaxValue ),
				  new LogPosition( int.MaxValue + 1L, long.MaxValue )
			  }, new LogPosition[]
			  {
				  new LogPosition( long.MaxValue, long.MaxValue ),
				  new LogPosition( long.MaxValue, long.MaxValue - 1 )
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"EqualsWithItself", "SelfComparison"}) @Test public void logPositionComparison()
		 public virtual void LogPositionComparison()
		 {
			  assertEquals( 1, LogPositionA.CompareTo( LogPositionB ) );
			  assertEquals( -1, LogPositionB.CompareTo( LogPositionA ) );

			  assertEquals( 0, LogPositionA.CompareTo( LogPositionA ) );
			  assertEquals( 0, LogPositionB.CompareTo( LogPositionB ) );
		 }
	}

}
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
namespace Neo4Net.Server.web
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.web.JettyThreadCalculator.MAX_THREADS;

	public class JettyThreadCalculatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectAmountOfThreads()
		 public virtual void ShouldHaveCorrectAmountOfThreads()
		 {
			  JettyThreadCalculator jtc = new JettyThreadCalculator( 1 );
			  assertEquals( "Wrong acceptor value for 1 core", 1, jtc.Acceptors );
			  assertEquals( "Wrong selector value for 1 core", 1, jtc.Selectors );
			  assertEquals( "Wrong maxThreads value for 1 core", 12, jtc.MaxThreads );
			  assertEquals( "Wrong minThreads value for 1 core", 6, jtc.MinThreads );
			  assertEquals( "Wrong capacity value for 1 core", 480000, jtc.MaxCapacity );

			  jtc = new JettyThreadCalculator( 4 );
			  assertEquals( "Wrong acceptor value for 4 cores", 1, jtc.Acceptors );
			  assertEquals( "Wrong selector value for 4 cores", 2, jtc.Selectors );
			  assertEquals( "Wrong maxThreads value for 4 cores", 14, jtc.MaxThreads );
			  assertEquals( "Wrong minThreads value for 4 cores", 8, jtc.MinThreads );
			  assertEquals( "Wrong capacity value for 4 cores", 480000, jtc.MaxCapacity );

			  jtc = new JettyThreadCalculator( 16 );
			  assertEquals( "Wrong acceptor value for 16 cores", 2, jtc.Acceptors );
			  assertEquals( "Wrong selector value for 16 cores", 3, jtc.Selectors );
			  assertEquals( "Wrong maxThreads value for 16 cores", 21, jtc.MaxThreads );
			  assertEquals( "Wrong minThreads value for 16 cores", 14, jtc.MinThreads );
			  assertEquals( "Wrong capacity value for 16 cores", 660000, jtc.MaxCapacity );

			  jtc = new JettyThreadCalculator( 64 );
			  assertEquals( "Wrong acceptor value for 64 cores", 4, jtc.Acceptors );
			  assertEquals( "Wrong selector value for 64 cores", 8, jtc.Selectors );
			  assertEquals( "Wrong maxThreads value for 64 cores", 76, jtc.MaxThreads );
			  assertEquals( "Wrong minThreads value for 64 cores", 36, jtc.MinThreads );
			  assertEquals( "Wrong capacity value for 64 cores", 3120000, jtc.MaxCapacity );

			  jtc = new JettyThreadCalculator( MAX_THREADS );
			  assertEquals( "Wrong acceptor value for max cores", 2982, jtc.Acceptors );
			  assertEquals( "Wrong selector value for max cores", 5965, jtc.Selectors );
			  assertEquals( "Wrong maxThreads value for max cores", 53685, jtc.MaxThreads );
			  assertEquals( "Wrong minThreads value for max cores", 26841, jtc.MinThreads );
			  assertEquals( "Wrong capacity value for max cores", 2147460000, jtc.MaxCapacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowLessThanOneThread()
		 public virtual void ShouldNotAllowLessThanOneThread()
		 {
			  try
			  {
					new JettyThreadCalculator( 0 );
					fail( "Should not succeed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					assertEquals( "Max threads can't be less than 1", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMoreThanMaxValue()
		 public virtual void ShouldNotAllowMoreThanMaxValue()
		 {
			  try
			  {
					new JettyThreadCalculator( MAX_THREADS + 1 );
					fail( "Should not succeed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					assertEquals( string.Format( "Max threads can't exceed {0:D}", MAX_THREADS ), e.Message );
			  }
		 }
	}

}
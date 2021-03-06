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
namespace Org.Neo4j.Kernel.impl.store.kvstore
{
	using Test = org.junit.Test;

	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class RotationTimerFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTimer()
		 public virtual void TestTimer()
		 {
			  // GIVEN
			  FakeClock fakeClock = Clocks.fakeClock( 10000, TimeUnit.MILLISECONDS );

			  // WHEN
			  RotationTimerFactory timerFactory = new RotationTimerFactory( fakeClock, 1000 );
			  RotationTimerFactory.RotationTimer timer = timerFactory.CreateTimer();
			  RotationTimerFactory.RotationTimer anotherTimer = timerFactory.CreateTimer();

			  // THEN
			  assertFalse( timer.TimedOut );
			  assertEquals( 0, timer.ElapsedTimeMillis );
			  assertNotSame( "Factory should construct different timers", timer, anotherTimer );

			  // WHEN
			  fakeClock = Clocks.fakeClock();
			  RotationTimerFactory fakeTimerFactory = new RotationTimerFactory( fakeClock, 1000 );
			  RotationTimerFactory.RotationTimer fakeTimer = fakeTimerFactory.CreateTimer();
			  fakeClock.Forward( 1001, TimeUnit.MILLISECONDS );

			  assertTrue( fakeTimer.TimedOut );
			  assertEquals( 1001, fakeTimer.ElapsedTimeMillis );
		 }
	}

}
using System;
using System.Threading;

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
namespace Org.Neo4j.Test
{
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using LongLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	using CpuClock = Org.Neo4j.Resources.CpuClock;

	public class FakeCpuClock : CpuClock, TestRule
	{
		 public static readonly CpuClock NOT_AVAILABLE = new CpuClockAnonymousInnerClass();

		 private class CpuClockAnonymousInnerClass : CpuClock
		 {
			 public override long cpuTimeNanos( long threadId )
			 {
				  return -1;
			 }
		 }
		 private readonly MutableLongLongMap _cpuTimes = new LongLongHashMap();

		 public override long CpuTimeNanos( long threadId )
		 {
			  return Math.Max( 0, _cpuTimes.get( threadId ) );
		 }

		 public virtual FakeCpuClock Add( long delta, TimeUnit unit )
		 {
			  return Add( unit.toNanos( delta ) );
		 }

		 public virtual FakeCpuClock Add( long nanos )
		 {
			  return add( Thread.CurrentThread.Id, nanos );
		 }

		 public virtual FakeCpuClock Add( long threadId, long nanos )
		 {
			  _cpuTimes.put( threadId, CpuTimeNanos( threadId ) + nanos );
			  return this;
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly FakeCpuClock _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( FakeCpuClock outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  @base.evaluate();
			 }
		 }
	}

}
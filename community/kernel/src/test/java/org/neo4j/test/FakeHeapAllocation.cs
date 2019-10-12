using System;

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

	using HeapAllocation = Org.Neo4j.Resources.HeapAllocation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;

	public class FakeHeapAllocation : HeapAllocation, TestRule
	{
		 private readonly MutableLongLongMap _allocation = new LongLongHashMap();

		 public override long AllocatedBytes( long threadId )
		 {
			  return Math.Max( 0, _allocation.get( threadId ) );
		 }

		 public virtual FakeHeapAllocation Add( long bytes )
		 {
			  return Add( currentThread().Id, bytes );
		 }

		 public virtual FakeHeapAllocation Add( long threadId, long bytes )
		 {
			  _allocation.put( threadId, AllocatedBytes( threadId ) + bytes );
			  return this;
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly FakeHeapAllocation _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( FakeHeapAllocation outerInstance, Statement @base )
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
using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.helper
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class CompositeSuspendableTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnableAllAndDisableAllEvenIfTheyThrow()
		 public virtual void ShouldEnableAllAndDisableAllEvenIfTheyThrow()
		 {
			  AtomicInteger count = new AtomicInteger();
			  CompositeSuspendable compositeSuspendable = new CompositeSuspendable();
			  int amountOfSuspendable = 3;
			  for ( int i = 0; i < amountOfSuspendable; i++ )
			  {
					compositeSuspendable.Add( GetSuspendable( count ) );
			  }

			  try
			  {
					compositeSuspendable.Enable();
					fail();
			  }
			  catch ( Exception )
			  {

			  }

			  assertEquals( amountOfSuspendable, count.get() );

			  try
			  {
					compositeSuspendable.Disable();
					fail();
			  }
			  catch ( Exception )
			  {

			  }

			  assertEquals( 0, count.get() );
		 }

		 private Suspendable GetSuspendable( AtomicInteger count )
		 {
			  return new SuspendableAnonymousInnerClass( this, count );
		 }

		 private class SuspendableAnonymousInnerClass : Suspendable
		 {
			 private readonly CompositeSuspendableTest _outerInstance;

			 private AtomicInteger _count;

			 public SuspendableAnonymousInnerClass( CompositeSuspendableTest outerInstance, AtomicInteger count )
			 {
				 this.outerInstance = outerInstance;
				 this._count = count;
			 }

			 public void enable()
			 {
				  _count.incrementAndGet();
				  fail();
			 }

			 public void disable()
			 {
				  _count.decrementAndGet();
				  fail();
			 }
		 }
	}

}
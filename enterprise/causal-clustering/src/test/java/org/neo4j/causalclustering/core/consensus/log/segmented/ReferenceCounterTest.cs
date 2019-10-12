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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ReferenceCounterTest
	{
		 private ReferenceCounter _refCount = new ReferenceCounter();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveValidInitialBehaviour()
		 public virtual void ShouldHaveValidInitialBehaviour()
		 {
			  assertEquals( 0, _refCount.get() );
			  assertTrue( _refCount.tryDispose() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToDisposeWhenActive()
		 public virtual void ShouldNotBeAbleToDisposeWhenActive()
		 {
			  // when
			  _refCount.increase();

			  // then
			  assertFalse( _refCount.tryDispose() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDisposeInactive()
		 public virtual void ShouldBeAbleToDisposeInactive()
		 {
			  // given
			  _refCount.increase();
			  _refCount.increase();

			  // when / then
			  _refCount.decrease();
			  assertFalse( _refCount.tryDispose() );

			  // when / then
			  _refCount.decrease();
			  assertTrue( _refCount.tryDispose() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGiveReferenceWhenDisposed()
		 public virtual void ShouldNotGiveReferenceWhenDisposed()
		 {
			  // given
			  _refCount.tryDispose();

			  // then
			  assertFalse( _refCount.increase() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdjustCounterWithReferences()
		 public virtual void ShouldAdjustCounterWithReferences()
		 {
			  // when / then
			  _refCount.increase();
			  assertEquals( 1, _refCount.get() );

			  // when / then
			  _refCount.increase();
			  assertEquals( 2, _refCount.get() );

			  // when / then
			  _refCount.decrease();
			  assertEquals( 1, _refCount.get() );

			  // when / then
			  _refCount.decrease();
			  assertEquals( 0, _refCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateExceptionWhenDecreasingPastZero()
		 public virtual void ShouldThrowIllegalStateExceptionWhenDecreasingPastZero()
		 {
			  // given
			  _refCount.increase();
			  _refCount.decrease();

			  // when
			  try
			  {
					_refCount.decrease();
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateExceptionWhenDecreasingOnDisposed()
		 public virtual void ShouldThrowIllegalStateExceptionWhenDecreasingOnDisposed()
		 {
			  // given
			  _refCount.tryDispose();

			  // when
			  try
			  {
					_refCount.decrease();
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }
		 }
	}

}
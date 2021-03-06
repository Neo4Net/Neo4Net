﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.index.sampling
{
	using Test = org.junit.Test;

	using Predicates = Org.Neo4j.Function.Predicates;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asArray;

	public class IndexSamplingJobQueueTest
	{
		 public static readonly System.Predicate<object> True = Predicates.alwaysTrue();
		 public static readonly System.Predicate<object> False = Predicates.alwaysFalse();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returnsNullWhenEmpty()
		 public virtual void ReturnsNullWhenEmpty()
		 {
			  assertNull( ( new IndexSamplingJobQueue<>( Predicates.alwaysTrue() ) ).poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnqueueJobWhenEmpty()
		 public virtual void ShouldEnqueueJobWhenEmpty()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobQueue<Object> jobQueue = new IndexSamplingJobQueue<>(TRUE);
			  IndexSamplingJobQueue<object> jobQueue = new IndexSamplingJobQueue<object>( True );
			  jobQueue.Add( false, _something );

			  // when
			  object result = jobQueue.Poll();

			  // then
			  assertEquals( _something, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnqueueJobOnlyOnce()
		 public virtual void ShouldEnqueueJobOnlyOnce()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobQueue<Object> jobQueue = new IndexSamplingJobQueue<>(TRUE);
			  IndexSamplingJobQueue<object> jobQueue = new IndexSamplingJobQueue<object>( True );
			  jobQueue.Add( false, _something );

			  // when
			  jobQueue.Add( false, _something );

			  // then
			  assertEquals( _something, jobQueue.Poll() );
			  assertNull( jobQueue.Poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEnqueueJobOnlyIfForbiddenByThePredicate()
		 public virtual void ShouldNotEnqueueJobOnlyIfForbiddenByThePredicate()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobQueue<Object> jobQueue = new IndexSamplingJobQueue<>(FALSE);
			  IndexSamplingJobQueue<object> jobQueue = new IndexSamplingJobQueue<object>( False );

			  // when
			  jobQueue.Add( false, _something );

			  // then
			  assertNull( jobQueue.Poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceEnqueueOfAnJobEvenIfThePredicateForbidsIt()
		 public virtual void ShouldForceEnqueueOfAnJobEvenIfThePredicateForbidsIt()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobQueue<Object> jobQueue = new IndexSamplingJobQueue<>(FALSE);
			  IndexSamplingJobQueue<object> jobQueue = new IndexSamplingJobQueue<object>( False );

			  // when
			  jobQueue.Add( true, _something );

			  // then
			  assertEquals( _something, jobQueue.Poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDequeueAll()
		 public virtual void ShouldDequeueAll()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object somethingElse = new Object();
			  object somethingElse = new object();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobQueue<Object> jobQueue = new IndexSamplingJobQueue<>(TRUE);
			  IndexSamplingJobQueue<object> jobQueue = new IndexSamplingJobQueue<object>( True );
			  jobQueue.Add( false, _something );
			  jobQueue.Add( false, somethingElse );

			  // when
			  IEnumerable<object> objects = jobQueue.PollAll();

			  // then
			  assertArrayEquals( new object[]{ _something, somethingElse }, asArray( typeof( object ), objects ) );
			  assertNull( jobQueue.Poll() );
		 }

		 private readonly object _something = new object();
	}

}
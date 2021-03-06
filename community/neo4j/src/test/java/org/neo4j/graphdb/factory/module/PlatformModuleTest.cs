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
namespace Org.Neo4j.Graphdb.factory.module
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using BufferingExecutor = Org.Neo4j.Kernel.impl.scheduler.BufferingExecutor;
	using Group = Org.Neo4j.Scheduler.Group;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class PlatformModuleTest
	internal class PlatformModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRunDeferredExecutors() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRunDeferredExecutors()
		 {
			  AtomicInteger counter = new AtomicInteger( 0 );
			  Semaphore @lock = new Semaphore( 1 );

			  BufferingExecutor later = new BufferingExecutor();

			  // add our later executor to the external dependencies
			  GraphDatabaseDependencies externalDependencies = newDependencies().withDeferredExecutor(later, Group.LOG_ROTATION);

			  // Take the lock, we're going to use this to synchronize with tasks that run in the executor
			  @lock.acquire( 1 );

			  // add an increment task to the deferred executor
			  later.Execute( counter.incrementAndGet );
			  later.Execute( @lock.release );

			  // if I try and get the lock it should fail because the deferred executor is still waiting for a real executor implementation.
			  // n.b. this will take the whole timeout time. So don't set this high, even if it means that this test might get lucky and pass
			  assertFalse( @lock.tryAcquire( 1,1, TimeUnit.SECONDS ) );
			  // my counter is still unincremented as well
			  assertThat( counter.get(), equalTo(0) );

			  // When I construct a PlatformModule...
			  PlatformModule pm = new PlatformModule( _testDirectory.storeDir(), Config.defaults(), DatabaseInfo.UNKNOWN, externalDependencies );

			  // then the tasks that I queued up earlier should be run...
			  // the timeout here is really high to ensure that this test does not become flaky because of a slow running JVM
			  // e.g. due to lots of CPU contention or garbage collection.
			  assertTrue( @lock.tryAcquire( 1,60, TimeUnit.SECONDS ) );
			  assertThat( counter.get(), equalTo(1) );
		 }
	}

}
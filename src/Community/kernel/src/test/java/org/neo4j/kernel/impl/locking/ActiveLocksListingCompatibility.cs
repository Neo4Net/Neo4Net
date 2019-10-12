using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.locking
{
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ActiveLock.exclusiveLock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ActiveLock.sharedLock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.RELATIONSHIP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class ActiveLocksListingCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class ActiveLocksListingCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public ActiveLocksListingCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListLocksHeldByTheCurrentClient()
		 public virtual void ShouldListLocksHeldByTheCurrentClient()
		 {
			  // given
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1, 2, 3 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 3, 4, 5 );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.stream.Stream<? extends ActiveLock> locks = clientA.activeLocks();
			  Stream<ActiveLock> locks = ClientA.activeLocks();

			  // then
			  assertEquals( new HashSet<>( asList( exclusiveLock( NODE, 1 ), exclusiveLock( NODE, 2 ), exclusiveLock( NODE, 3 ), sharedLock( NODE, 3 ), sharedLock( NODE, 4 ), sharedLock( NODE, 5 ) ) ), locks.collect( toSet() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountNumberOfActiveLocks()
		 public virtual void ShouldCountNumberOfActiveLocks()
		 {
			  // given
			  ClientA.acquireShared( LockTracer.NONE, LABEL, 0 );
			  ClientA.acquireShared( LockTracer.NONE, RELATIONSHIP, 17 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 12 );

			  // when
			  long count = ClientA.activeLockCount();

			  // then
			  assertEquals( 3, count );
		 }
	}

}
﻿using System;

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
namespace Org.Neo4j.causalclustering.readreplica
{
	using Test = org.junit.Test;

	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_General.ForbiddenOnReadOnlyDatabase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status.statusCodeOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.@lock.LockTracer.NONE;

	public class ReadReplicaLockManagerTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReadReplicaLockManagerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_lockClient = _lockManager.newClient();
		}

		 private ReadReplicaLockManager _lockManager = new ReadReplicaLockManager();
		 private Org.Neo4j.Kernel.impl.locking.Locks_Client _lockClient;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnAcquireExclusiveLock()
		 public virtual void ShouldThrowOnAcquireExclusiveLock()
		 {
			  try
			  {
					// when
					_lockClient.acquireExclusive( NONE, NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertEquals( ForbiddenOnReadOnlyDatabase, statusCodeOf( e ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnTryAcquireExclusiveLock()
		 public virtual void ShouldThrowOnTryAcquireExclusiveLock()
		 {
			  try
			  {
					// when
					_lockClient.tryExclusiveLock( NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertEquals( ForbiddenOnReadOnlyDatabase, statusCodeOf( e ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptSharedLocks()
		 public virtual void ShouldAcceptSharedLocks()
		 {
			  _lockClient.acquireShared( NONE, NODE, 1 );
			  _lockClient.trySharedLock( NODE, 1 );
		 }
	}

}
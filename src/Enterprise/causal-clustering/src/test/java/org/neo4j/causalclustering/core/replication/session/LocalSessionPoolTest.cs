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
namespace Neo4Net.causalclustering.core.replication.session
{
	using Test = org.junit.Test;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;

	public class LocalSessionPoolTest
	{
		private bool InstanceFieldsInitialized = false;

		public LocalSessionPoolTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_globalSession = new GlobalSession( System.Guid.randomUUID(), _memberId );
		}

		 private MemberId _memberId = new MemberId( System.Guid.randomUUID() );
		 private GlobalSession _globalSession;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void poolGivesBackSameSessionAfterRelease()
		 public virtual void PoolGivesBackSameSessionAfterRelease()
		 {
			  LocalSessionPool sessionPool = new LocalSessionPool( _globalSession );

			  OperationContext contextA = sessionPool.AcquireSession();
			  sessionPool.ReleaseSession( contextA );

			  OperationContext contextB = sessionPool.AcquireSession();
			  sessionPool.ReleaseSession( contextB );

			  assertEquals( contextA.LocalSession(), contextB.LocalSession() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sessionAcquirementIncreasesOperationId()
		 public virtual void SessionAcquirementIncreasesOperationId()
		 {
			  LocalSessionPool sessionPool = new LocalSessionPool( _globalSession );
			  OperationContext context;

			  context = sessionPool.AcquireSession();
			  LocalOperationId operationA = context.LocalOperationId();
			  sessionPool.ReleaseSession( context );

			  context = sessionPool.AcquireSession();
			  LocalOperationId operationB = context.LocalOperationId();
			  sessionPool.ReleaseSession( context );

			  assertEquals( operationB.SequenceNumber(), operationA.SequenceNumber() + 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void poolHasIndependentSessions()
		 public virtual void PoolHasIndependentSessions()
		 {
			  LocalSessionPool sessionPool = new LocalSessionPool( _globalSession );

			  OperationContext contextA = sessionPool.AcquireSession();
			  OperationContext contextB = sessionPool.AcquireSession();

			  assertNotEquals( contextA.LocalSession(), contextB.LocalSession() );
		 }
	}

}
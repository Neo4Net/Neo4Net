/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;
	using ConstantRequestContextFactory = Neo4Net.Test.ConstantRequestContextFactory;
	using LongResponse = Neo4Net.Test.LongResponse;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SlaveTransactionCommitProcessTest
	{
		 private AtomicInteger _lastSeenEventIdentifier;
		 private Master _master;
		 private RequestContext _requestContext;
		 private Response<long> _response;
		 private PhysicalTransactionRepresentation _tx;
		 private SlaveTransactionCommitProcess _commitProcess;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _lastSeenEventIdentifier = new AtomicInteger( -1 );
			  _master = mock( typeof( Master ) );
			  _requestContext = new RequestContext( 10, 11, 12, 13, 14 );
			  RequestContextFactory reqFactory = new ConstantRequestContextFactoryAnonymousInnerClass( this, _requestContext );
			  _response = new LongResponse( 42L );
			  _tx = new PhysicalTransactionRepresentation( Collections.emptyList() );
			  _tx.setHeader( new sbyte[]{}, 1, 1, 1, 1, 1, 1337 );

			  _commitProcess = new SlaveTransactionCommitProcess( _master, reqFactory );
		 }

		 private class ConstantRequestContextFactoryAnonymousInnerClass : ConstantRequestContextFactory
		 {
			 private readonly SlaveTransactionCommitProcessTest _outerInstance;

			 public ConstantRequestContextFactoryAnonymousInnerClass( SlaveTransactionCommitProcessTest outerInstance, RequestContext requestContext ) : base( requestContext )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override RequestContext newRequestContext( int eventIdentifier )
			 {
				  _outerInstance.lastSeenEventIdentifier.set( eventIdentifier );
				  return base.newRequestContext( eventIdentifier );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardLockIdentifierToMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardLockIdentifierToMaster()
		 {
			  // Given
			  when( _master.commit( _requestContext, _tx ) ).thenReturn( _response );

			  // When
			  _commitProcess.commit( new TransactionToApply( _tx ), CommitEvent.NULL, TransactionApplicationMode.INTERNAL );

			  // Then
			  assertThat( _lastSeenEventIdentifier.get(), @is(1337) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.TransientTransactionFailureException.class) public void mustTranslateComExceptionsToTransientTransactionFailures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustTranslateComExceptionsToTransientTransactionFailures()
		 {
			  when( _master.commit( _requestContext, _tx ) ).thenThrow( new ComException() );

			  // When
			  _commitProcess.commit( new TransactionToApply( _tx ), CommitEvent.NULL, TransactionApplicationMode.INTERNAL );
			  // Then we assert that the right exception is thrown
		 }
	}

}
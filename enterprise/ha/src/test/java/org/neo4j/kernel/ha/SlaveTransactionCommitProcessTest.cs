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
namespace Org.Neo4j.Kernel.ha
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ComException = Org.Neo4j.com.ComException;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using ConstantRequestContextFactory = Org.Neo4j.Test.ConstantRequestContextFactory;
	using LongResponse = Org.Neo4j.Test.LongResponse;

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
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.TransientTransactionFailureException.class) public void mustTranslateComExceptionsToTransientTransactionFailures() throws Exception
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
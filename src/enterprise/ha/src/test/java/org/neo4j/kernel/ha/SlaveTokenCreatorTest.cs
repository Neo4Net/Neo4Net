using System.Collections.Generic;

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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using ConstantRequestContextFactory = Neo4Net.Test.ConstantRequestContextFactory;
	using IntegerResponse = Neo4Net.Test.IntegerResponse;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SlaveTokenCreatorTest
	public class SlaveTokenCreatorTest
	{
		 public interface ISlaveTokenCreatorFixture
		 {
			  AbstractTokenCreator Build( Master master, RequestContextFactory requestContextFactory );
			  Response<int> CallMasterMethod( Master master, RequestContext ctx, string name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> tokenCreators()
		 public static IEnumerable<object[]> TokenCreators()
		 {
			  SlaveTokenCreatorFixture slaveLabelTokenCreatorFixture = new SlaveTokenCreatorFixtureAnonymousInnerClass();
			  SlaveTokenCreatorFixture slaveRelationshipTypeTokenCreatorFixture = new SlaveTokenCreatorFixtureAnonymousInnerClass2();
			  SlaveTokenCreatorFixture slavePropertyTokenCreatorFixture = new SlaveTokenCreatorFixtureAnonymousInnerClass3();
			  return Arrays.asList( new object[] { "SlaveLabelTokenCreator", slaveLabelTokenCreatorFixture }, new object[] { "SlaveRelationshipTypeTokenCreator", slaveRelationshipTypeTokenCreatorFixture }, new object[] { "SlavePropertyTokenCreator", slavePropertyTokenCreatorFixture } );
		 }

		 private class SlaveTokenCreatorFixtureAnonymousInnerClass : SlaveTokenCreatorFixture
		 {
			 public AbstractTokenCreator build( Master master, RequestContextFactory requestContextFactory )
			 {
				  return new SlaveLabelTokenCreator( master, requestContextFactory );
			 }

			 public Response<int> callMasterMethod( Master master, RequestContext ctx, string name )
			 {
				  return master.CreateLabel( ctx, name );
			 }
		 }

		 private class SlaveTokenCreatorFixtureAnonymousInnerClass2 : SlaveTokenCreatorFixture
		 {
			 public AbstractTokenCreator build( Master master, RequestContextFactory requestContextFactory )
			 {
				  return new SlaveRelationshipTypeCreator( master, requestContextFactory );
			 }

			 public Response<int> callMasterMethod( Master master, RequestContext ctx, string name )
			 {
				  return master.CreateRelationshipType( ctx, name );
			 }
		 }

		 private class SlaveTokenCreatorFixtureAnonymousInnerClass3 : SlaveTokenCreatorFixture
		 {
			 public AbstractTokenCreator build( Master master, RequestContextFactory requestContextFactory )
			 {
				  return new SlavePropertyTokenCreator( master, requestContextFactory );
			 }

			 public Response<int> callMasterMethod( Master master, RequestContext ctx, string name )
			 {
				  return master.CreatePropertyKey( ctx, name );
			 }
		 }

		 private SlaveTokenCreatorFixture _fixture;
		 private Master _master;
		 private RequestContext _requestContext;
		 private RequestContextFactory _requestContextFactory;
		 private string _name;
		 private AbstractTokenCreator _tokenCreator;

		 public SlaveTokenCreatorTest( string name, SlaveTokenCreatorFixture fixture )
		 {
			  this._fixture = fixture;
			  _master = mock( typeof( Master ) );
			  _requestContext = new RequestContext( 1, 2, 3, 4, 5 );
			  this._name = "Poke";
			  _requestContextFactory = new ConstantRequestContextFactory( _requestContext );
			  _tokenCreator = fixture.Build( _master, _requestContextFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.TransientTransactionFailureException.class) public void mustTranslateComExceptionsToTransientTransactionFailures()
		 public virtual void MustTranslateComExceptionsToTransientTransactionFailures()
		 {
			  when( _fixture.callMasterMethod( _master, _requestContext, _name ) ).thenThrow( new ComException() );
			  _tokenCreator.createToken( _name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReturnIdentifierFromMaster()
		 public virtual void MustReturnIdentifierFromMaster()
		 {
			  when( _fixture.callMasterMethod( _master, _requestContext, _name ) ).thenReturn( new IntegerResponse( 13 ) );
			  assertThat( _tokenCreator.createToken( _name ), @is( 13 ) );
		 }
	}

}
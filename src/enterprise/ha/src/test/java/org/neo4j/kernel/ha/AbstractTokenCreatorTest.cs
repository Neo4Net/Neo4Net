using System;

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

	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using Neo4Net.com;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.com.ResourceReleaser_Fields.NO_OP;

	public class AbstractTokenCreatorTest
	{
		 private readonly Master _master = mock( typeof( Master ) );
		 private readonly RequestContextFactory _requestContextFactory = mock( typeof( RequestContextFactory ) );

		 private readonly RequestContext _context = new RequestContext( 1, 2, 3, 4, 5 );

		 private readonly string _label = "A";
		 private readonly Response<int> _response = new TransactionStreamResponse<int>( 42, null, null, NO_OP );

		 private AbstractTokenCreator creator = new AbstractTokenCreatorAnonymousInnerClass( _master, _requestContextFactory );

		 private class AbstractTokenCreatorAnonymousInnerClass : AbstractTokenCreator
		 {
			 public AbstractTokenCreatorAnonymousInnerClass( Master master, RequestContextFactory requestContextFactory ) : base( master, requestContextFactory )
			 {
			 }

			 protected internal override Response<int> create( Master master, RequestContext context, string name )
			 {
				  assertEquals( outerInstance.master, master );
				  assertEquals( outerInstance.context, context );
				  assertEquals( outerInstance.label, name );
				  return outerInstance.response;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _requestContextFactory.newRequestContext() ).thenReturn(_context);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateALabelOnMasterAndApplyItLocally()
		 public virtual void ShouldCreateALabelOnMasterAndApplyItLocally()
		 {
			  // GIVEN
			  int responseValue = _response.response();

			  // WHEN
			  int result = creator.createToken( _label );

			  // THEN
			  assertEquals( responseValue, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfCreateThrowsAnException()
		 public virtual void ShouldThrowIfCreateThrowsAnException()
		 {
			  // GIVEN
			  Exception re = new Exception( "IO" );
			  AbstractTokenCreator throwingCreator = spy( creator );
			  doThrow( re ).when( throwingCreator ).create( any( typeof( Master ) ), any( typeof( RequestContext ) ), anyString() );

			  try
			  {
					// WHEN
					throwingCreator.CreateToken( "A" );
					fail( "Should have thrown" );
			  }
			  catch ( Exception e )
			  {
					// THEN
					assertEquals( re, e );
			  }
		 }
	}

}
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{
	using Test = org.junit.Test;


	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class PoliciesTest
	{
		 private Log _log = mock( typeof( Log ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupplyDefaultUnfilteredPolicyForEmptyContext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupplyDefaultUnfilteredPolicyForEmptyContext()
		 {
			  // given
			  Policies policies = new Policies( _log );

			  // when
			  Policy policy = policies.SelectFor( emptyMap() );
			  ISet<ServerInfo> input = asSet(new ServerInfo(new AdvertisedSocketAddress("bolt", 1), new MemberId(System.Guid.randomUUID()), asSet("groupA")), new ServerInfo(new AdvertisedSocketAddress("bolt", 2), new MemberId(System.Guid.randomUUID()), asSet("groupB"))
			 );

			  ISet<ServerInfo> output = policy.Apply( input );

			  // then
			  assertEquals( input, output );
			  assertEquals( Policies.DefaultPolicyConflict, policy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnUnknownPolicyName()
		 public virtual void ShouldThrowExceptionOnUnknownPolicyName()
		 {
			  // given
			  Policies policies = new Policies( _log );

			  try
			  {
					// when
					policies.SelectFor( stringMap( Policies.POLICY_KEY, "unknown-policy" ) );
					fail();
			  }
			  catch ( ProcedureException e )
			  {
					// then
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, e.Status() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnSelectionOfUnregisteredDefault()
		 public virtual void ShouldThrowExceptionOnSelectionOfUnregisteredDefault()
		 {
			  Policies policies = new Policies( _log );

			  try
			  {
					// when
					policies.SelectFor( stringMap( Policies.POLICY_KEY, Policies.DEFAULT_POLICY_NAME ) );
					fail();
			  }
			  catch ( ProcedureException e )
			  {
					// then
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, e.Status() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOverridingDefaultPolicy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOverridingDefaultPolicy()
		 {
			  Policies policies = new Policies( _log );

			  string defaultPolicyName = Policies.DEFAULT_POLICY_NAME;
			  Policy defaultPolicy = new FilteringPolicy( new AnyGroupFilter( "groupA", "groupB" ) );

			  // when
			  policies.AddPolicy( defaultPolicyName, defaultPolicy );
			  Policy selectedPolicy = policies.SelectFor( emptyMap() );

			  // then
			  assertEquals( defaultPolicy, selectedPolicy );
			  assertNotEquals( Policies.DefaultPolicyConflict, selectedPolicy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowLookupOfAddedPolicy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowLookupOfAddedPolicy()
		 {
			  // given
			  Policies policies = new Policies( _log );

			  string myPolicyName = "china";
			  Policy myPolicy = data => data;

			  // when
			  policies.AddPolicy( myPolicyName, myPolicy );
			  Policy selectedPolicy = policies.SelectFor( stringMap( Policies.POLICY_KEY, myPolicyName ) );

			  // then
			  assertEquals( myPolicy, selectedPolicy );
		 }
	}

}
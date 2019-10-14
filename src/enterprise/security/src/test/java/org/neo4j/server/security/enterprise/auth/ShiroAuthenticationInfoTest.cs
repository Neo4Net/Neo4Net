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
namespace Neo4Net.Server.security.enterprise.auth
{
	using Test = org.junit.Test;

	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.TOO_MANY_ATTEMPTS;

	public class ShiroAuthenticationInfoTest
	{
		 private ShiroAuthenticationInfo _successInfo = new ShiroAuthenticationInfo( "user", "realm", SUCCESS );
		 private ShiroAuthenticationInfo _failureInfo = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
		 private ShiroAuthenticationInfo _tooManyAttemptsInfo = new ShiroAuthenticationInfo( "user", "realm", TOO_MANY_ATTEMPTS );
		 private ShiroAuthenticationInfo _pwChangeRequiredInfo = new ShiroAuthenticationInfo( "user", "realm", PASSWORD_CHANGE_REQUIRED );

		 // These tests are here to remind you that you need to update the ShiroAuthenticationInfo.mergeMatrix[][]
		 // whenever you add/remove/move values in the AuthenticationResult enum

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeMergeMatrixIfAuthenticationResultEnumChanges()
		 public virtual void ShouldChangeMergeMatrixIfAuthenticationResultEnumChanges()
		 {
			  // These are the assumptions made for ShiroAuthenticationInfo.mergeMatrix[][]
			  // which have to stay in sync with the enum
			  assertEquals( ( int )AuthenticationResult.SUCCESS, 0 );
			  assertEquals( ( int )AuthenticationResult.FAILURE, 1 );
			  assertEquals( ( int )AuthenticationResult.TOO_MANY_ATTEMPTS, 2 );
			  assertEquals( ( int )AuthenticationResult.PASSWORD_CHANGE_REQUIRED, 3 );
			  assertEquals( Enum.GetValues( typeof( AuthenticationResult ) ).length, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeTwoSuccessToSameValue()
		 public virtual void ShouldMergeTwoSuccessToSameValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", SUCCESS );
			  info.Merge( _successInfo );

			  assertEquals( info.AuthenticationResult, SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeTwoFailureToSameValue()
		 public virtual void ShouldMergeTwoFailureToSameValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
			  info.Merge( _failureInfo );

			  assertEquals( info.AuthenticationResult, FAILURE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeTwoTooManyAttemptsToSameValue()
		 public virtual void ShouldMergeTwoTooManyAttemptsToSameValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", TOO_MANY_ATTEMPTS );
			  info.Merge( _tooManyAttemptsInfo );

			  assertEquals( info.AuthenticationResult, TOO_MANY_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeTwoPasswordChangeRequiredToSameValue()
		 public virtual void ShouldMergeTwoPasswordChangeRequiredToSameValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", PASSWORD_CHANGE_REQUIRED );
			  info.Merge( _pwChangeRequiredInfo );

			  assertEquals( info.AuthenticationResult, PASSWORD_CHANGE_REQUIRED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeFailureWithSuccessToNewValue()
		 public virtual void ShouldMergeFailureWithSuccessToNewValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
			  info.Merge( _successInfo );

			  assertEquals( info.AuthenticationResult, SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeFailureWithTooManyAttemptsToNewValue()
		 public virtual void ShouldMergeFailureWithTooManyAttemptsToNewValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
			  info.Merge( _tooManyAttemptsInfo );

			  assertEquals( info.AuthenticationResult, TOO_MANY_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeFailureWithPasswordChangeRequiredToNewValue()
		 public virtual void ShouldMergeFailureWithPasswordChangeRequiredToNewValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
			  info.Merge( _pwChangeRequiredInfo );

			  assertEquals( info.AuthenticationResult, PASSWORD_CHANGE_REQUIRED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeToNewValue()
		 public virtual void ShouldMergeToNewValue()
		 {
			  ShiroAuthenticationInfo info = new ShiroAuthenticationInfo( "user", "realm", FAILURE );
			  info.Merge( _pwChangeRequiredInfo );

			  assertEquals( info.AuthenticationResult, PASSWORD_CHANGE_REQUIRED );
		 }
	}

}
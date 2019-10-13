using System;
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
namespace Neo4Net.Server.security.enterprise.auth
{
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using SimpleAuthenticationInfo = org.apache.shiro.authc.SimpleAuthenticationInfo;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using SimpleAuthorizationInfo = org.apache.shiro.authz.SimpleAuthorizationInfo;
	using JndiLdapContextFactory = org.apache.shiro.realm.ldap.JndiLdapContextFactory;
	using LdapContextFactory = org.apache.shiro.realm.ldap.LdapContextFactory;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using SimplePrincipalCollection = org.apache.shiro.subject.SimplePrincipalCollection;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using Any = org.mockito.@internal.matchers.Any;


	using Config = Neo4Net.Kernel.configuration.Config;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class LdapRealmTest
	{
		 internal Config Config = mock( typeof( Config ) );
		 private SecurityLog _securityLog = mock( typeof( SecurityLog ) );
		 private SecureHasher _secureHasher = new SecureHasher();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  // Some dummy settings to pass validation
			  when( Config.get( SecuritySettings.ldap_authorization_user_search_base ) ).thenReturn( "dc=example,dc=com" );
			  when( Config.get( SecuritySettings.ldap_authorization_group_membership_attribute_names ) ).thenReturn( singletonList( "memberOf" ) );

			  when( Config.get( SecuritySettings.ldap_authentication_enabled ) ).thenReturn( true );
			  when( Config.get( SecuritySettings.ldap_authorization_enabled ) ).thenReturn( true );
			  when( Config.get( SecuritySettings.ldap_authentication_cache_enabled ) ).thenReturn( false );
			  when( Config.get( SecuritySettings.ldap_connection_timeout ) ).thenReturn( Duration.ofSeconds( 1 ) );
			  when( Config.get( SecuritySettings.ldap_read_timeout ) ).thenReturn( Duration.ofSeconds( 1 ) );
			  when( Config.get( SecuritySettings.ldap_authorization_connection_pooling ) ).thenReturn( true );
			  when( Config.get( SecuritySettings.ldap_authentication_use_samaccountname ) ).thenReturn( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToBeNull()
		 public virtual void GroupToRoleMappingShouldBeAbleToBeNull()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( null );

			  new LdapRealm( Config, _securityLog, _secureHasher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToBeEmpty()
		 public virtual void GroupToRoleMappingShouldBeAbleToBeEmpty()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "" );

			  new LdapRealm( Config, _securityLog, _secureHasher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveMultipleRoles()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveMultipleRoles()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group=role1,role2,role3" );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping["group"], equalTo( asList( "role1", "role2", "role3" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveMultipleGroups()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveMultipleGroups()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group1=role1;group2=role2,role3;group3=role4" );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping.Keys, equalTo( new SortedSet<>( asList( "group1", "group2", "group3" ) ) ) );
			  assertThat( realm.GroupToRoleMapping["group1"], equalTo( singletonList( "role1" ) ) );
			  assertThat( realm.GroupToRoleMapping["group2"], equalTo( asList( "role2", "role3" ) ) );
			  assertThat( realm.GroupToRoleMapping["group3"], equalTo( singletonList( "role4" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveQuotedKeysAndWhitespaces()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveQuotedKeysAndWhitespaces()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "'group1' = role1;\t \"group2\"\n=\t role2,role3 ;  gr oup3= role4\n ;'group4 '= ; g =r" );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping.Keys, equalTo( new SortedSet<>( asList( "group1", "group2", "gr oup3", "group4 ", "g" ) ) ) );
			  assertThat( realm.GroupToRoleMapping["group1"], equalTo( singletonList( "role1" ) ) );
			  assertThat( realm.GroupToRoleMapping["group2"], equalTo( asList( "role2", "role3" ) ) );
			  assertThat( realm.GroupToRoleMapping["gr oup3"], equalTo( singletonList( "role4" ) ) );
			  assertThat( realm.GroupToRoleMapping["group4 "], equalTo( Collections.emptyList() ) );
			  assertThat( realm.GroupToRoleMapping["g"], equalTo( singletonList( "r" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveTrailingSemicolons()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveTrailingSemicolons()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group=role;;" );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping["group"], equalTo( singletonList( "role" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveTrailingCommas()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveTrailingCommas()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group=role1,role2,role3,,," );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping.Keys, equalTo( Stream.of( "group" ).collect( Collectors.toSet() ) ) );
			  assertThat( realm.GroupToRoleMapping["group"], equalTo( asList( "role1", "role2", "role3" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldBeAbleToHaveNoRoles()
		 public virtual void GroupToRoleMappingShouldBeAbleToHaveNoRoles()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group=," );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping["group"].Count, equalTo( 0 ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldNotBeAbleToHaveInvalidFormat()
		 public virtual void GroupToRoleMappingShouldNotBeAbleToHaveInvalidFormat()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group" );

			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "wrong number of fields" );

			  new LdapRealm( Config, _securityLog, _secureHasher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupToRoleMappingShouldNotBeAbleToHaveEmptyGroupName()
		 public virtual void GroupToRoleMappingShouldNotBeAbleToHaveEmptyGroupName()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "=role" );

			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "wrong number of fields" );

			  new LdapRealm( Config, _securityLog, _secureHasher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupComparisonShouldBeCaseInsensitive()
		 public virtual void GroupComparisonShouldBeCaseInsensitive()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "GrouP=role1,role2,role3" );

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );

			  assertThat( realm.GroupToRoleMapping["group"], equalTo( asList( "role1", "role2", "role3" ) ) );
			  assertThat( realm.GroupToRoleMapping.Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutUserSearchFilterWithoutArgument() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAboutUserSearchFilterWithoutArgument()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_user_search_filter ) ).thenReturn( "" );

			  LdapContext ldapContext = mock( typeof( LdapContext ) );
			  NamingEnumeration result = mock( typeof( NamingEnumeration ) );
			  when( ldapContext.search( anyString(), anyString(), any(), any() ) ).thenReturn(result);
			  when( result.hasMoreElements() ).thenReturn(false);

			  MakeAndInit();

			  verify( _securityLog ).warn( contains( "LDAP user search filter does not contain the argument placeholder {0}" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutUserSearchBaseBeingEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAboutUserSearchBaseBeingEmpty()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_user_search_base ) ).thenReturn( "" );

			  LdapContext ldapContext = mock( typeof( LdapContext ) );
			  NamingEnumeration result = mock( typeof( NamingEnumeration ) );
			  when( ldapContext.search( anyString(), anyString(), any(), any() ) ).thenReturn(result);
			  when( result.hasMoreElements() ).thenReturn(false);

			  assertException( this.makeAndInit, typeof( System.ArgumentException ), "Illegal LDAP user search settings, see security log for details." );

			  verify( _securityLog ).error( contains( "LDAP user search base is empty." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutGroupMembershipsBeingEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAboutGroupMembershipsBeingEmpty()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_group_membership_attribute_names ) ).thenReturn( Collections.emptyList() );

			  LdapContext ldapContext = mock( typeof( LdapContext ) );
			  NamingEnumeration result = mock( typeof( NamingEnumeration ) );
			  when( ldapContext.search( anyString(), anyString(), any(), any() ) ).thenReturn(result);
			  when( result.hasMoreElements() ).thenReturn(false);

			  assertException( this.makeAndInit, typeof( System.ArgumentException ), "Illegal LDAP user search settings, see security log for details." );

			  verify( _securityLog ).error( contains( "LDAP group membership attribute names are empty. " + "Authorization will not be possible." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutAmbiguousUserSearch() throws javax.naming.NamingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAboutAmbiguousUserSearch()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_user_search_filter ) ).thenReturn( "{0}" );

			  LdapContext ldapContext = mock( typeof( LdapContext ) );
			  NamingEnumeration result = mock( typeof( NamingEnumeration ) );
			  SearchResult searchResult = mock( typeof( SearchResult ) );
			  when( ldapContext.search( anyString(), anyString(), any(), any() ) ).thenReturn(result);
			  when( result.hasMoreElements() ).thenReturn(true);
			  when( result.next() ).thenReturn(searchResult);
			  when( searchResult.ToString() ).thenReturn("<ldap search result>");

			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );
			  realm.FindRoleNamesForUser( "username", ldapContext );

			  verify( _securityLog ).warn( contains( "LDAP user search for user principal 'username' is ambiguous" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMultipleGroupMembershipAttributes() throws javax.naming.NamingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowMultipleGroupMembershipAttributes()
		 {
			  when( Config.get( SecuritySettings.ldap_authorization_user_search_filter ) ).thenReturn( "{0}" );
			  when( Config.get( SecuritySettings.ldap_authorization_group_membership_attribute_names ) ).thenReturn( asList( "attr0", "attr1", "attr2" ) );
			  when( Config.get( SecuritySettings.ldap_authorization_group_to_role_mapping ) ).thenReturn( "group1=role1;group2=role2,role3" );

			  LdapContext ldapContext = mock( typeof( LdapContext ) );
			  NamingEnumeration result = mock( typeof( NamingEnumeration ) );
			  SearchResult searchResult = mock( typeof( SearchResult ) );
			  Attributes attributes = mock( typeof( Attributes ) );
			  Attribute attribute1 = mock( typeof( Attribute ) );
			  Attribute attribute2 = mock( typeof( Attribute ) );
			  Attribute attribute3 = mock( typeof( Attribute ) );
			  NamingEnumeration attributeEnumeration = mock( typeof( NamingEnumeration ) );
			  NamingEnumeration groupEnumeration1 = mock( typeof( NamingEnumeration ) );
			  NamingEnumeration groupEnumeration2 = mock( typeof( NamingEnumeration ) );
			  NamingEnumeration groupEnumeration3 = mock( typeof( NamingEnumeration ) );

			  // Mock ldap search result "attr1" contains "group1" and "attr2" contains "group2" (a bit brittle...)
			  // "attr0" is non-existing and should have no effect
			  when( ldapContext.search( anyString(), anyString(), any(), any() ) ).thenReturn(result);
			  when( result.hasMoreElements() ).thenReturn(true, false);
			  when( result.next() ).thenReturn(searchResult);
			  when( searchResult.Attributes ).thenReturn( attributes );
			  when( attributes.All ).thenReturn( attributeEnumeration );
			  when( attributeEnumeration.hasMore() ).thenReturn(true, true, false);
			  when( attributeEnumeration.next() ).thenReturn(attribute1, attribute2, attribute3);

			  when( attribute1.ID ).thenReturn( "attr1" ); // This attribute should yield role1
			  when( attribute1.All ).thenReturn( groupEnumeration1 );
			  when( groupEnumeration1.hasMore() ).thenReturn(true, false);
			  when( groupEnumeration1.next() ).thenReturn("group1");

			  when( attribute2.ID ).thenReturn( "attr2" ); // This attribute should yield role2 and role3
			  when( attribute2.All ).thenReturn( groupEnumeration2 );
			  when( groupEnumeration2.hasMore() ).thenReturn(true, false);
			  when( groupEnumeration2.next() ).thenReturn("group2");

			  when( attribute3.ID ).thenReturn( "attr3" ); // This attribute should have no effect
			  when( attribute3.All ).thenReturn( groupEnumeration3 );
			  when( groupEnumeration3.hasMore() ).thenReturn(true, false);
			  when( groupEnumeration3.next() ).thenReturn("groupWithNoRole");

			  // When
			  LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );
			  ISet<string> roles = realm.FindRoleNamesForUser( "username", ldapContext );

			  // Then
			  assertThat( roles, hasItems( "role1", "role2", "role3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSuccessfulAuthenticationQueries() throws javax.naming.NamingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogSuccessfulAuthenticationQueries()
		 {
			  // Given
			  when( Config.get( SecuritySettings.ldap_use_starttls ) ).thenReturn( false );
			  when( Config.get( SecuritySettings.ldap_authorization_use_system_account ) ).thenReturn( true );

			  LdapRealm realm = new TestLdapRealm( this, Config, _securityLog, false );
			  JndiLdapContextFactory jndiLdapContectFactory = mock( typeof( JndiLdapContextFactory ) );
			  when( jndiLdapContectFactory.Url ).thenReturn( "ldap://myserver.org:12345" );
			  when( jndiLdapContectFactory.getLdapContext( Any.ANY, Any.ANY ) ).thenReturn( null );

			  // When
			  realm.QueryForAuthenticationInfo( new ShiroAuthToken( map( "principal", "olivia", "credentials", "123" ) ), jndiLdapContectFactory );

			  // Then
			  verify( _securityLog ).debug( contains( "{LdapRealm}: Authenticated user 'olivia' against 'ldap://myserver.org:12345'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSuccessfulAuthenticationQueriesUsingStartTLS() throws javax.naming.NamingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogSuccessfulAuthenticationQueriesUsingStartTLS()
		 {
			  // Given
			  when( Config.get( SecuritySettings.ldap_use_starttls ) ).thenReturn( true );

			  LdapRealm realm = new TestLdapRealm( this, Config, _securityLog, false );
			  JndiLdapContextFactory jndiLdapContectFactory = mock( typeof( JndiLdapContextFactory ) );
			  when( jndiLdapContectFactory.Url ).thenReturn( "ldap://myserver.org:12345" );

			  // When
			  realm.QueryForAuthenticationInfo( new ShiroAuthToken( map( "principal", "olivia", "credentials", "123" ) ), jndiLdapContectFactory );

			  // Then
			  verify( _securityLog ).debug( contains( "{LdapRealm}: Authenticated user 'olivia' against 'ldap://myserver.org:12345' using StartTLS" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailedAuthenticationQueries()
		 public virtual void ShouldLogFailedAuthenticationQueries()
		 {
			  // Given
			  when( Config.get( SecuritySettings.ldap_use_starttls ) ).thenReturn( true );

			  LdapRealm realm = new TestLdapRealm( this, Config, _securityLog, true );
			  JndiLdapContextFactory jndiLdapContectFactory = mock( typeof( JndiLdapContextFactory ) );
			  when( jndiLdapContectFactory.Url ).thenReturn( "ldap://myserver.org:12345" );

			  // When
			  assertException( () => realm.QueryForAuthenticationInfo(new ShiroAuthToken(map("principal", "olivia", "credentials", "123")), jndiLdapContectFactory), typeof(NamingException) );

			  // Then
			  // Authentication failures are logged from MultiRealmAuthManager
			  //verify( securityLog ).error( contains(
			  //        "{LdapRealm}: Failed to authenticate user 'olivia' against 'ldap://myserver.org:12345' using StartTLS: " +
			  //                "Simulated failure" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSuccessfulAuthorizationQueries()
		 public virtual void ShouldLogSuccessfulAuthorizationQueries()
		 {
			  // Given
			  when( Config.get( SecuritySettings.ldap_use_starttls ) ).thenReturn( true );

			  LdapRealm realm = new TestLdapRealm( this, Config, _securityLog, false );
			  JndiLdapContextFactory jndiLdapContectFactory = mock( typeof( JndiLdapContextFactory ) );
			  when( jndiLdapContectFactory.Url ).thenReturn( "ldap://myserver.org:12345" );

			  // When
			  realm.DoGetAuthorizationInfo( new SimplePrincipalCollection( "olivia", "LdapRealm" ) );

			  // Then
			  verify( _securityLog ).debug( contains( "{LdapRealm}: Queried for authorization info for user 'olivia'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailedAuthorizationQueries()
		 public virtual void ShouldLogFailedAuthorizationQueries()
		 {
			  // Given
			  when( Config.get( SecuritySettings.ldap_use_starttls ) ).thenReturn( true );

			  LdapRealm realm = new TestLdapRealm( this, Config, _securityLog, true );
			  JndiLdapContextFactory jndiLdapContectFactory = mock( typeof( JndiLdapContextFactory ) );
			  when( jndiLdapContectFactory.Url ).thenReturn( "ldap://myserver.org:12345" );

			  // When
			  AuthorizationInfo info = realm.DoGetAuthorizationInfo( new SimplePrincipalCollection( "olivia", "LdapRealm" ) );

			  // Then
			  assertNull( info );
			  verify( _securityLog ).warn( contains( "{LdapRealm}: Failed to get authorization info: " + "'LDAP naming error while attempting to retrieve authorization for user [olivia].'" + " caused by 'Simulated failure'" ) );
		 }

		 private class TestLdapRealm : LdapRealm
		 {
			 private readonly LdapRealmTest _outerInstance;


			  internal bool FailAuth;

			  internal TestLdapRealm( LdapRealmTest outerInstance, Config config, SecurityLog securityLog, bool failAuth ) : base( config, securityLog, outerInstance.secureHasher )
			  {
				  this._outerInstance = outerInstance;
					this.FailAuth = failAuth;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo queryForAuthenticationInfoUsingStartTls(org.apache.shiro.authc.AuthenticationToken token, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
			  protected internal override AuthenticationInfo QueryForAuthenticationInfoUsingStartTls( AuthenticationToken token, LdapContextFactory ldapContextFactory )
			  {
					if ( FailAuth )
					{
						 throw new NamingException( "Simulated failure" );
					}
					return new SimpleAuthenticationInfo( "olivia", "123", "basic" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authz.AuthorizationInfo queryForAuthorizationInfo(org.apache.shiro.subject.PrincipalCollection principals, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
			  protected internal override AuthorizationInfo QueryForAuthorizationInfo( PrincipalCollection principals, LdapContextFactory ldapContextFactory )
			  {
					if ( FailAuth )
					{
						 throw new NamingException( "Simulated failure" );
					}
					return new SimpleAuthorizationInfo();
			  }
		 }

		 private void MakeAndInit()
		 {
			  try
			  {
					LdapRealm realm = new LdapRealm( Config, _securityLog, _secureHasher );
					realm.Initialize();
			  }
			  catch ( Exception e )
			  {
					throw e;
			  }
			  catch ( Exception t )
			  {
					throw new Exception( t );
			  }
		 }
	}

}
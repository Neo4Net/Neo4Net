using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.configuration.ssl
{
	using Test = org.junit.Test;


	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;

	public class SslPolicyConfigValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private System.Action<String> warnings = mock(System.Action.class);
		 private System.Action<string> _warnings = mock( typeof( System.Action ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptAllValidPolicyKeys()
		 public virtual void ShouldAcceptAllValidPolicyKeys()
		 {
			  // given
			  SslPolicyConfigValidator validator = new SslPolicyConfigValidator();
			  IDictionary<string, string> originalParams = Params( "dbms.ssl.policy.default.base_directory", "xyz", "dbms.ssl.policy.default.allow_key_generation", "xyz", "dbms.ssl.policy.default.trust_all", "xyz", "dbms.ssl.policy.default.private_key", "xyz", "dbms.ssl.policy.default.private_key_password", "xyz", "dbms.ssl.policy.default.public_certificate", "xyz", "dbms.ssl.policy.default.trusted_dir", "xyz", "dbms.ssl.policy.default.revoked_dir", "xyz", "dbms.ssl.policy.default.client_auth", "xyz", "dbms.ssl.policy.default.tls_versions", "xyz", "dbms.ssl.policy.default.ciphers", "xyz" );

			  // when
			  IDictionary<string, string> validatedParams = validator.Validate( originalParams, _warnings );

			  // then
			  assertEquals( originalParams, validatedParams );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnUnknownPolicySetting()
		 public virtual void ShouldThrowOnUnknownPolicySetting()
		 {
			  // given
			  SslPolicyConfigValidator validator = new SslPolicyConfigValidator();
			  IDictionary<string, string> originalParams = Params( "dbms.ssl.policy.default.color", "blue" );

			  // when
			  try
			  {
					validator.Validate( originalParams, _warnings );
					fail();
			  }
			  catch ( InvalidSettingException e )
			  {
					assertTrue( e.Message.contains( "Invalid setting name" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnDirectPolicySetting()
		 public virtual void ShouldThrowOnDirectPolicySetting()
		 {
			  // given
			  SslPolicyConfigValidator validator = new SslPolicyConfigValidator();
			  IDictionary<string, string> originalParams = Params( "dbms.ssl.policy.base_directory", "path" );

			  // when
			  try
			  {
					validator.Validate( originalParams, _warnings );
					fail();
			  }
			  catch ( InvalidSettingException e )
			  {
					assertTrue( e.Message.contains( "Invalid setting name" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreUnknownNonPolicySettings()
		 public virtual void ShouldIgnoreUnknownNonPolicySettings()
		 {
			  // given
			  SslPolicyConfigValidator validator = new SslPolicyConfigValidator();
			  IDictionary<string, string> originalParams = Params( "dbms.ssl.unknown", "xyz", "dbms.ssl.something", "xyz", "dbms.unrelated.totally", "xyz" );

			  // when
			  IDictionary<string, string> validatedParams = validator.Validate( originalParams, _warnings );

			  // then
			  assertTrue( validatedParams.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainWhenMissingMandatoryBaseDirectory()
		 public virtual void ShouldComplainWhenMissingMandatoryBaseDirectory()
		 {
			  // given
			  SslPolicyConfigValidator validator = new SslPolicyConfigValidator();
			  IDictionary<string, string> originalParams = Params( "dbms.ssl.policy.default.private_key", "private.key", "dbms.ssl.policy.default.public_certificate", "public.crt" );

			  // when
			  try
			  {
					validator.Validate( originalParams, _warnings );
					fail();
			  }
			  catch ( InvalidSettingException e )
			  {
					assertTrue( e.Message.contains( "Missing mandatory setting" ) );
			  }
		 }

		 private static IDictionary<string, string> Params( params string[] @params )
		 {
			  return unmodifiableMap( stringMap( @params ) );
		 }
	}

}
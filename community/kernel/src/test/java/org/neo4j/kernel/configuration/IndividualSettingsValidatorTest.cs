using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.configuration
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using Mockito = org.mockito.Mockito;

	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.strict_config_validation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;

	public class IndividualSettingsValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();
		 private Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _log = mock( typeof( Log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonStrictRetainsSettings()
		 public virtual void NonStrictRetainsSettings()
		 {
			  IndividualSettingsValidator iv = new IndividualSettingsValidator( singletonList( strict_config_validation ), true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> rawConfig = stringMap(strict_config_validation.name(), FALSE, "dbms.jibber.jabber", "bla", "external_plugin.foo", "bar");
			  IDictionary<string, string> rawConfig = stringMap( strict_config_validation.name(), FALSE, "dbms.jibber.jabber", "bla", "external_plugin.foo", "bar" );

			  Config config = MockConfig( rawConfig );

			  iv.Validate( config, _log );

			  verify( _log ).warn( "Unknown config option: %s", "dbms.jibber.jabber" );
			  verifyNoMoreInteractions( _log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void strictErrorsOnUnknownSettingsInOurNamespace()
		 public virtual void StrictErrorsOnUnknownSettingsInOurNamespace()
		 {
			  IndividualSettingsValidator iv = new IndividualSettingsValidator( singletonList( strict_config_validation ), true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> rawConfig = stringMap(strict_config_validation.name(), TRUE, "dbms.jibber.jabber", "bla", "external_plugin.foo", "bar");
			  IDictionary<string, string> rawConfig = stringMap( strict_config_validation.name(), TRUE, "dbms.jibber.jabber", "bla", "external_plugin.foo", "bar" );

			  Config config = MockConfig( rawConfig );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( string.Format( "Unknown config option 'dbms.jibber.jabber'. To resolve either remove" + " it from your configuration or set '{0}' to false.", strict_config_validation.name() ) );

			  iv.Validate( config, _log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void strictAllowsStuffOutsideOurNamespace()
		 public virtual void StrictAllowsStuffOutsideOurNamespace()
		 {
			  IndividualSettingsValidator iv = new IndividualSettingsValidator( singletonList( strict_config_validation ), true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> rawConfig = stringMap(strict_config_validation.name(), TRUE, "external_plugin.foo", "bar");
			  IDictionary<string, string> rawConfig = stringMap( strict_config_validation.name(), TRUE, "external_plugin.foo", "bar" );

			  Config config = MockConfig( rawConfig );

			  iv.Validate( config, _log );
			  verifyNoMoreInteractions( _log );
		 }

		 private Config MockConfig( IDictionary<string, string> rawConfig )
		 {
			  Config config = Mockito.mock( typeof( Config ) );

			  when( config.Raw ).thenReturn( rawConfig );
			  when( config.Get( strict_config_validation ) ).thenReturn( Convert.ToBoolean( rawConfig[strict_config_validation.name()] ) );

			  return config;
		 }
	}

}
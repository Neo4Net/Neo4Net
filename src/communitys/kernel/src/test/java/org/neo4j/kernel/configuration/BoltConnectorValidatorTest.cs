using System;
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
namespace Neo4Net.Kernel.configuration
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using EncryptionLevel = Neo4Net.Kernel.configuration.BoltConnector.EncryptionLevel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ConnectorValidator.DEPRECATED_CONNECTOR_MSG;


	public class BoltConnectorValidatorTest
	{
		 internal BoltConnectorValidator Cv = new BoltConnectorValidator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();
		 private System.Action<string> _warningConsumer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _warningConsumer = mock( typeof( System.Action ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotValidateUnrelatedStuff()
		 public virtual void DoesNotValidateUnrelatedStuff()
		 {
			  assertEquals( 0, Cv.validate( stringMap( "dbms.connector.http.enabled", "true", "dbms.blabla.boo", "123" ), _warningConsumer ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyEnabledRequiredWhenNameIsBolt()
		 public virtual void OnlyEnabledRequiredWhenNameIsBolt()
		 {
			  string boltEnabled = "dbms.connector.bolt.enabled";

			  assertEquals( stringMap( boltEnabled, "true" ), Cv.validate( stringMap( boltEnabled, "true" ), _warningConsumer ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requiresTypeWhenNameIsNotBolt()
		 public virtual void RequiresTypeWhenNameIsNotBolt()
		 {
			  string randomEnabled = "dbms.connector.bla.enabled";
			  string randomType = "dbms.connector.bla.type";

			  assertEquals( stringMap( randomEnabled, "true", randomType, BOLT.name() ), Cv.validate(stringMap(randomEnabled, "true", randomType, BOLT.name()), _warningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing mandatory value for 'dbms.connector.bla.type'" );

			  Cv.validate( stringMap( randomEnabled, "true" ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requiresCorrectTypeWhenNameIsNotBolt()
		 public virtual void RequiresCorrectTypeWhenNameIsNotBolt()
		 {
			  string randomEnabled = "dbms.connector.bla.enabled";
			  string randomType = "dbms.connector.bla.type";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "dbms.connector.bla.type' must be one of BOLT, HTTP; not 'woo'" );

			  Cv.validate( stringMap( randomEnabled, "true", randomType, "woo" ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void warnsWhenNameIsNotBolt()
		 public virtual void WarnsWhenNameIsNotBolt()
		 {
			  string randomEnabled = "dbms.connector.bla.enabled";
			  string randomType = "dbms.connector.bla.type";

			  Cv.validate( stringMap( randomEnabled, "true", randomType, "BOLT" ), _warningConsumer );

			  verify( _warningConsumer ).accept( format( DEPRECATED_CONNECTOR_MSG, format( ">  %s%n>  %s%n", randomEnabled, randomType ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorsOnInvalidConnectorSetting1()
		 public virtual void ErrorsOnInvalidConnectorSetting1()
		 {
			  string invalidSetting = "dbms.connector.bla.0.enabled";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Invalid connector setting: dbms.connector.bla.0.enabled" );

			  Cv.validate( stringMap( invalidSetting, "true" ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorsOnInvalidConnectorSetting2()
		 public virtual void ErrorsOnInvalidConnectorSetting2()
		 {
			  string invalidSetting = "dbms.connector.bolt.foobar";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Invalid connector setting: dbms.connector.bolt.foobar" );

			  Cv.validate( stringMap( invalidSetting, "true" ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesTlsLevel()
		 public virtual void ValidatesTlsLevel()
		 {
			  string key = "dbms.connector.bolt.tls_level";

			  assertEquals( stringMap( key, EncryptionLevel.DISABLED.name() ), Cv.validate(stringMap(key, EncryptionLevel.DISABLED.name()), _warningConsumer) );

			  assertEquals( stringMap( key, EncryptionLevel.OPTIONAL.name() ), Cv.validate(stringMap(key, EncryptionLevel.OPTIONAL.name()), _warningConsumer) );

			  assertEquals( stringMap( key, EncryptionLevel.REQUIRED.name() ), Cv.validate(stringMap(key, EncryptionLevel.REQUIRED.name()), _warningConsumer) );

			  key = "dbms.connector.bla.tls_level";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, EncryptionLevel.DISABLED.name(), type, BOLT.name() ), Cv.validate(stringMap(key, EncryptionLevel.DISABLED.name(), type, BOLT.name()), _warningConsumer) );

			  assertEquals( stringMap( key, EncryptionLevel.OPTIONAL.name(), type, BOLT.name() ), Cv.validate(stringMap(key, EncryptionLevel.OPTIONAL.name(), type, BOLT.name()), _warningConsumer) );

			  assertEquals( stringMap( key, EncryptionLevel.REQUIRED.name(), type, BOLT.name() ), Cv.validate(stringMap(key, EncryptionLevel.REQUIRED.name(), type, BOLT.name()), _warningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Bad value 'BOBO' for setting 'dbms.connector.bla.tls_level': must be one of [REQUIRED, OPTIONAL, " + "DISABLED] case sensitive" );

			  Cv.validate( stringMap( key, "BOBO", type, BOLT.name() ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesAddress()
		 public virtual void ValidatesAddress()
		 {
			  string key = "dbms.connector.bolt.address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), _warningConsumer ) );

			  key = "dbms.connector.bla.address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.address\" must be in the format \"hostname:port\" or " + "\":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, BOLT.name() ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesListenAddress()
		 public virtual void ValidatesListenAddress()
		 {
			  string key = "dbms.connector.bolt.listen_address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), _warningConsumer ) );

			  key = "dbms.connector.bla.listen_address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.listen_address\" must be in the format " + "\"hostname:port\" or \":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, BOLT.name() ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesAdvertisedAddress()
		 public virtual void ValidatesAdvertisedAddress()
		 {
			  string key = "dbms.connector.bolt.advertised_address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), _warningConsumer ) );

			  key = "dbms.connector.bla.advertised_address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, BOLT.name() ), Cv.validate(stringMap(key, "localhost:123", type, BOLT.name()), _warningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.advertised_address\" must be in the format " + "\"hostname:port\" or \":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, BOLT.name() ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesType()
		 public virtual void ValidatesType()
		 {
			  string type = "dbms.connector.bla.type";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "'dbms.connector.bla.type' must be one of BOLT, HTTP; not 'BOBO'" );

			  Cv.validate( stringMap( type, "BOBO" ), _warningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnAddress()
		 public virtual void SetsDeprecationFlagOnAddress()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.bolt.address", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( "dbms.connector.bolt.listen_address", setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnType()
		 public virtual void SetsDeprecationFlagOnType()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.bolt.type", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( null, setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnCustomNamedBoltConnectors()
		 public virtual void SetsDeprecationFlagOnCustomNamedBoltConnectors()
		 {
			  IList<Setting<object>> settings = Cv.settings( stringMap( "dbms.connector.0.type", "BOLT", "dbms.connector.0.enabled", "false", "dbms.connector.0.listen_address", "1.2.3.4:123", "dbms.connector.0.advertised_address", "localhost:123", "dbms.connector.0.tls_level", EncryptionLevel.OPTIONAL.ToString() ) );

			  assertEquals( 5, settings.Count );

			  foreach ( Setting s in settings )
			  {
					assertTrue( "every setting should be deprecated: " + s.name(), s.deprecated() );
					string[] parts = s.name().Split("\\.");
					if ( !"type".Equals( parts[3] ) )
					{
						 assertEquals( format( "%s.%s.%s.%s", parts[0], parts[1], "bolt", parts[3] ), s.replacement() );
					}
			  }
		 }
	}

}
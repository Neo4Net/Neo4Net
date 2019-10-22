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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using Neo4Net.GraphDb.config;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Connector.ConnectorType.HTTP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.ConnectorValidator.DEPRECATED_CONNECTOR_MSG;

	public class HttpConnectorValidatorTest
	{
		 internal HttpConnectorValidator Cv = new HttpConnectorValidator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

		 internal System.Action<string> WarningConsumer = mock( typeof( System.Action ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotValidateUnrelatedStuff()
		 public virtual void DoesNotValidateUnrelatedStuff()
		 {
			  assertEquals( 0, Cv.validate( stringMap( "dbms.connector.bolt.enabled", "true", "dbms.blabla.boo", "123" ), WarningConsumer ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyEnabledRequiredWhenNameIsHttpOrHttps()
		 public virtual void OnlyEnabledRequiredWhenNameIsHttpOrHttps()
		 {
			  string httpEnabled = "dbms.connector.http.enabled";
			  string httpsEnabled = "dbms.connector.https.enabled";

			  assertEquals( stringMap( httpEnabled, "true" ), Cv.validate( stringMap( httpEnabled, "true" ), WarningConsumer ) );

			  assertEquals( stringMap( httpsEnabled, "true" ), Cv.validate( stringMap( httpsEnabled, "true" ), WarningConsumer ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requiresTypeWhenNameIsNotHttpOrHttps()
		 public virtual void RequiresTypeWhenNameIsNotHttpOrHttps()
		 {
			  string randomEnabled = "dbms.connector.bla.enabled";
			  string randomType = "dbms.connector.bla.type";

			  assertEquals( stringMap( randomEnabled, "true", randomType, HTTP.name() ), Cv.validate(stringMap(randomEnabled, "true", randomType, HTTP.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing mandatory value for 'dbms.connector.bla.type'" );

			  Cv.validate( stringMap( randomEnabled, "true" ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void warnsWhenNameIsNotHttpOrHttps()
		 public virtual void WarnsWhenNameIsNotHttpOrHttps()
		 {
			  string randomEnabled = "dbms.connector.bla.enabled";
			  string randomType = "dbms.connector.bla.type";

			  Cv.validate( stringMap( randomEnabled, "true", randomType, "HTTP" ), WarningConsumer );

			  verify( WarningConsumer ).accept( format( DEPRECATED_CONNECTOR_MSG, format( ">  %s%n>  %s%n", randomEnabled, randomType ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorsOnInvalidConnectorSetting1()
		 public virtual void ErrorsOnInvalidConnectorSetting1()
		 {
			  string invalidSetting = "dbms.connector.bla.0.enabled";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Invalid connector setting: dbms.connector.bla.0.enabled" );

			  Cv.validate( stringMap( invalidSetting, "true" ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorsOnInvalidConnectorSetting2()
		 public virtual void ErrorsOnInvalidConnectorSetting2()
		 {
			  string invalidSetting = "dbms.connector.http.foobar";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Invalid connector setting: dbms.connector.http.foobar" );

			  Cv.validate( stringMap( invalidSetting, "true" ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesEncryption()
		 public virtual void ValidatesEncryption()
		 {
			  string key = "dbms.connector.bla.encryption";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, Encryption.NONE.name(), type, HTTP.name() ), Cv.validate(stringMap(key, Encryption.NONE.name(), type, HTTP.name()), WarningConsumer) );

			  assertEquals( stringMap( key, Encryption.TLS.name(), type, HTTP.name() ), Cv.validate(stringMap(key, Encryption.TLS.name(), type, HTTP.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Bad value 'BOBO' for setting 'dbms.connector.bla.encryption': must be one of [NONE, TLS] case " + "sensitive" );

			  Cv.validate( stringMap( key, "BOBO", type, HTTP.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void httpsConnectorCanOnlyHaveTLS()
		 public virtual void HttpsConnectorCanOnlyHaveTLS()
		 {
			  string key = "dbms.connector.https.encryption";

			  assertEquals( stringMap( key, Encryption.TLS.name() ), Cv.validate(stringMap(key, Encryption.TLS.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "'dbms.connector.https.encryption' is only allowed to be 'TLS'; not 'NONE'" );
			  Cv.validate( stringMap( key, Encryption.NONE.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void httpConnectorCanNotHaveTLS()
		 public virtual void HttpConnectorCanNotHaveTLS()
		 {
			  string key = "dbms.connector.http.encryption";

			  assertEquals( stringMap( key, Encryption.NONE.name() ), Cv.validate(stringMap(key, Encryption.NONE.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "'dbms.connector.http.encryption' is only allowed to be 'NONE'; not 'TLS'" );
			  Cv.validate( stringMap( key, Encryption.TLS.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesAddress()
		 public virtual void ValidatesAddress()
		 {
			  string key = "dbms.connector.http.address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), WarningConsumer ) );

			  key = "dbms.connector.bla.address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.address\" must be in the format \"hostname:port\" or " + "\":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, HTTP.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesListenAddress()
		 public virtual void ValidatesListenAddress()
		 {
			  string key = "dbms.connector.http.listen_address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), WarningConsumer ) );

			  key = "dbms.connector.bla.listen_address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.listen_address\" must be in the format " + "\"hostname:port\" or \":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, HTTP.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesAdvertisedAddress()
		 public virtual void ValidatesAdvertisedAddress()
		 {
			  string key = "dbms.connector.http.advertised_address";

			  assertEquals( stringMap( key, "localhost:123" ), Cv.validate( stringMap( key, "localhost:123" ), WarningConsumer ) );

			  key = "dbms.connector.bla.advertised_address";
			  string type = "dbms.connector.bla.type";

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  assertEquals( stringMap( key, "localhost:123", type, HTTP.name() ), Cv.validate(stringMap(key, "localhost:123", type, HTTP.name()), WarningConsumer) );

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Setting \"dbms.connector.bla.advertised_address\" must be in the format " + "\"hostname:port\" or \":port\". \"BOBO\" does not conform to these formats" );

			  Cv.validate( stringMap( key, "BOBO", type, HTTP.name() ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatesType()
		 public virtual void ValidatesType()
		 {
			  string type = "dbms.connector.bla.type";

			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "'dbms.connector.bla.type' must be one of BOLT, HTTP; not 'BOBO'" );

			  Cv.validate( stringMap( type, "BOBO" ), WarningConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnAddress()
		 public virtual void SetsDeprecationFlagOnAddress()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.http.address", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( "dbms.connector.http.listen_address", setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnEncryption()
		 public virtual void SetsDeprecationFlagOnEncryption()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.http.encryption", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( null, setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sdfa()
		 public virtual void Sdfa()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.http.type", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( null, setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnType()
		 public virtual void SetsDeprecationFlagOnType()
		 {
			  Setting setting = Cv.getSettingFor( "dbms.connector.http.type", Collections.emptyMap() ).orElseThrow(() => new Exception("missing setting!"));

			  assertTrue( setting.deprecated() );
			  assertEquals( null, setting.replacement() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnCustomNamedHttpConnectors()
		 public virtual void SetsDeprecationFlagOnCustomNamedHttpConnectors()
		 {
			  IList<Setting<object>> settings = Cv.settings( stringMap( "dbms.connector.0.type", "HTTP", "dbms.connector.0.enabled", "false", "dbms.connector.0.listen_address", "1.2.3.4:123", "dbms.connector.0.advertised_address", "localhost:123", "dbms.connector.0.encryption", Encryption.NONE.ToString() ) );

			  assertEquals( 5, settings.Count );

			  foreach ( Setting s in settings )
			  {
					assertTrue( "every setting should be deprecated: " + s.name(), s.deprecated() );
					string[] parts = s.name().Split("\\.");
					if ( !"encryption".Equals( parts[3] ) && !"type".Equals( parts[3] ) )
					{
						 assertEquals( format( "%s.%s.%s.%s", parts[0], parts[1], "http", parts[3] ), s.replacement() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setsDeprecationFlagOnCustomNamedHttpsConnectors()
		 public virtual void SetsDeprecationFlagOnCustomNamedHttpsConnectors()
		 {
			  IList<Setting<object>> settings = Cv.settings( stringMap( "dbms.connector.0.type", "HTTP", "dbms.connector.0.enabled", "false", "dbms.connector.0.listen_address", "1.2.3.4:123", "dbms.connector.0.advertised_address", "localhost:123", "dbms.connector.0.encryption", Encryption.TLS.ToString() ) );

			  assertEquals( 5, settings.Count );

			  foreach ( Setting s in settings )
			  {
					assertTrue( "every setting should be deprecated: " + s.name(), s.deprecated() );
					string[] parts = s.name().Split("\\.");

					if ( !"encryption".Equals( parts[3] ) && !"type".Equals( parts[3] ) )
					{
						 assertEquals( format( "%s.%s.%s.%s", parts[0], parts[1], "https", parts[3] ), s.replacement() );
					}
			  }
		 }
	}

}
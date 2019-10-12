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
namespace Neo4Net.Server.plugins
{
	using Test = org.junit.Test;


	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ConfigAdapterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDefaultPropertyByKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetDefaultPropertyByKey()
		 {
			  // GIVEN
			  Config config = Config.defaults();
			  ConfigAdapter wrappingConfiguration = new ConfigAdapter( config );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object propertyValue = wrappingConfiguration.getProperty(org.neo4j.server.configuration.ServerSettings.rest_api_path.name());
			  object propertyValue = wrappingConfiguration.GetProperty( ServerSettings.rest_api_path.name() );

			  // THEN
			  assertEquals( new URI( ServerSettings.rest_api_path.DefaultValue ), propertyValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetPropertyInRightFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetPropertyInRightFormat()
		 {
			  // GIVEN
			  Config config = Config.defaults();
			  ConfigAdapter wrappingConfiguration = new ConfigAdapter( config );

			  // WHEN
			  wrappingConfiguration.setProperty( ServerSettings.rest_api_path.name(), "http://localhost:7474///db///data///" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object dataPath = wrappingConfiguration.getProperty(org.neo4j.server.configuration.ServerSettings.rest_api_path.name());
			  object dataPath = wrappingConfiguration.GetProperty( ServerSettings.rest_api_path.name() );

			  // THEN
			  assertEquals( new URI( ServerSettings.rest_api_path.DefaultValue ), dataPath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainAllKeysOfPropertiesWithDefaultOrUserDefinedValues()
		 public virtual void ShouldContainAllKeysOfPropertiesWithDefaultOrUserDefinedValues()
		 {
			  // GIVEN

			  Config config = Config.defaults();
			  ConfigAdapter wrappingConfiguration = new ConfigAdapter( config );

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( wrappingConfiguration.Keys.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbleToAccessRegisteredPropertyByName()
		 public virtual void ShouldAbleToAccessRegisteredPropertyByName()
		 {
			  Config config = Config.defaults();
			  ConfigAdapter wrappingConfiguration = new ConfigAdapter( config );

			  assertEquals( Duration.ofSeconds( 60 ), wrappingConfiguration.GetProperty( ServerSettings.transaction_idle_timeout.name() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbleToAccessNonRegisteredPropertyByName()
		 public virtual void ShouldAbleToAccessNonRegisteredPropertyByName()
		 {
			  Config config = Config.defaults( ServerSettings.transaction_idle_timeout, "600ms" );
			  ConfigAdapter wrappingConfiguration = new ConfigAdapter( config );

			  assertEquals( Duration.ofMillis( 600 ), wrappingConfiguration.GetProperty( ServerSettings.transaction_idle_timeout.name() ) );
		 }
	}

}
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
namespace Neo4Net.Configuration
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Graphdb.config;
	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using Neo4Net.Graphdb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	internal class ConfigOptionsTest
	{
		 private Setting<int> setting = new BaseSettingAnonymousInnerClass();

		 private class BaseSettingAnonymousInnerClass : BaseSetting<int>
		 {
			 public override string name()
			 {
				  return "myInt";
			 }

			 public override void withScope( System.Func<string, string> scopingRule )
			 {

			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return "1";
				 }
			 }

			 public override int? from( Configuration config )
			 {
				  return config.Get( this );
			 }

			 public override int? apply( System.Func<string, string> provider )
			 {
				  return int.Parse( provider( name() ) );
			 }

			 public override string valueDescription()
			 {
				  return "a special test integer";
			 }
		 }
		 private ConfigOptions _configOptions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  this._configOptions = new ConfigOptions( setting );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void setting()
		 internal virtual void Setting()
		 {
			  assertEquals( setting, _configOptions.settingGroup() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void asConfigValue()
		 internal virtual void AsConfigValue()
		 {
			  IList<ConfigValue> values = _configOptions.asConfigValues( Collections.singletonMap( "myInt", "123" ) );

			  assertEquals( 1, values.Count );

			  assertEquals( 123, values[0].Value() );
			  assertEquals( "myInt", values[0].Name() );
			  assertEquals( null, values[0].Description() );
			  assertEquals( "a special test integer", values[0].ValueDescription() );
		 }
	}

}
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
namespace Neo4Net.Configuration
{
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.GraphDb.config;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using Neo4Net.GraphDb.config;
	using Neo4Net.GraphDb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	internal class LoadableConfigTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getConfigOptions()
		 internal virtual void getConfigOptions()
		 {
			  IDictionary<string, string> config = stringMap( TestConfig.integer.name(), "123", TestConfig.@string.name(), "bah", TestConfig.oldString.name(), "moo", TestConfig.dynamic.name(), "foo" );

			  TestConfig testSettings = new TestConfig();

			  IList<ConfigOptions> options = testSettings.ConfigOptions;

			  assertEquals( 4, options.Count );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.config.SettingGroup<?> integerSetting = options.get(0).settingGroup();
			  SettingGroup<object> integerSetting = options[0].SettingGroup();
			  assertEquals( 1, integerSetting.Values( emptyMap() )[TestConfig.integer.name()] );
			  assertEquals( 123, integerSetting.Values( config )[TestConfig.integer.name()] );
			  assertEquals( null, integerSetting.Description() );
			  assertFalse( integerSetting.Deprecated() );
			  assertFalse( integerSetting.Dynamic() );
			  assertEquals( null, integerSetting.Replacement() );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.config.SettingGroup<?> stringSetting = options.get(1).settingGroup();
			  SettingGroup<object> stringSetting = options[1].SettingGroup();
			  assertEquals( "bob", stringSetting.Values( emptyMap() )[TestConfig.@string.name()] );
			  assertEquals( "bah", stringSetting.Values( config )[TestConfig.@string.name()] );
			  assertEquals( "A string setting", stringSetting.Description().get() );
			  assertFalse( stringSetting.Deprecated() );
			  assertFalse( stringSetting.Dynamic() );
			  assertEquals( null, stringSetting.Replacement() );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.config.SettingGroup<?> oldStringSetting = options.get(2).settingGroup();
			  SettingGroup<object> oldStringSetting = options[2].SettingGroup();
			  assertEquals( "tim", oldStringSetting.Values( emptyMap() )[TestConfig.oldString.name()] );
			  assertEquals( "moo", oldStringSetting.Values( config )[TestConfig.oldString.name()] );
			  assertEquals( "A deprecated string setting", oldStringSetting.Description().get() );
			  assertTrue( oldStringSetting.Deprecated() );
			  assertFalse( oldStringSetting.Dynamic() );
			  assertEquals( TestConfig.@string.name(), oldStringSetting.Replacement().get() );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.config.SettingGroup<?> dynamicSetting = options.get(3).settingGroup();
			  SettingGroup<object> dynamicSetting = options[3].SettingGroup();
			  assertEquals( "defaultDynamic", dynamicSetting.Values( emptyMap() )[TestConfig.dynamic.name()] );
			  assertEquals( "foo", dynamicSetting.Values( config )[TestConfig.dynamic.name()] );
			  assertEquals( "A dynamic string setting", dynamicSetting.Description().get() );
			  assertFalse( dynamicSetting.Deprecated() );
			  assertTrue( dynamicSetting.Dynamic() );
			  assertEquals( null, dynamicSetting.Replacement() );
		 }

		 private class TestConfig : LoadableConfig
		 {
			  public static readonly Setting<int> integer = new BaseSettingAnonymousInnerClass();

			  private class BaseSettingAnonymousInnerClass : BaseSetting<int>
			  {
				  public override string valueDescription()
				  {
						return "an Integer";
				  }

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
						string val = provider( name() );
						if ( string.ReferenceEquals( val, null ) )
						{
							 val = DefaultValue;
						}
						return int.Parse( val );
				  }

			  }

			  [Description("A string setting")]
			  public static readonly Setting<string> @string = new StringSettingAnonymousInnerClass();

			  private class StringSettingAnonymousInnerClass : StringSetting
			  {
				  public override string apply( System.Func<string, string> provider )
				  {
						string val = provider( name() );
						if ( string.ReferenceEquals( val, null ) )
						{
							 val = DefaultValue;
						}
						return val;
				  }

				  public override string name()
				  {
						return "myString";
				  }

				  public override void withScope( System.Func<string, string> function )
				  {

				  }

				  public override string DefaultValue
				  {
					  get
					  {
							return "bob";
					  }
				  }

				  public override string from( Configuration configuration )
				  {
						return configuration.Get( this );
				  }
			  }

			  [Description("A deprecated string setting"), Obsolete, ReplacedBy("myString")]
			  public static readonly Setting<string> oldString = new StringSettingAnonymousInnerClass2();

			  private class StringSettingAnonymousInnerClass2 : StringSetting
			  {
				  public override string apply( System.Func<string, string> provider )
				  {
						string val = provider( name() );
						if ( string.ReferenceEquals( val, null ) )
						{
							 val = DefaultValue;
						}
						return val;
				  }

				  public override string name()
				  {
						return "myOldString";
				  }

				  public override void withScope( System.Func<string, string> function )
				  {

				  }

				  public override string DefaultValue
				  {
					  get
					  {
							return "tim";
					  }
				  }

				  public override string from( Configuration configuration )
				  {
						return configuration.Get( this );
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("A private setting which is not accessible") private static final org.Neo4Net.graphdb.config.Setting<String> ignoredSetting = new StringSetting()
			  [Description("A private setting which is not accessible")]
			  internal static readonly Setting<string> ignoredSetting = new StringSettingAnonymousInnerClass3();

			  private class StringSettingAnonymousInnerClass3 : StringSetting
			  {
				  public override string apply( System.Func<string, string> provider )
				  {
						return provider( name() );
				  }

				  public override string name()
				  {
						return "myString";
				  }

				  public override void withScope( System.Func<string, string> function )
				  {

				  }

				  public override string DefaultValue
				  {
					  get
					  {
							return "bob";
					  }
				  }

				  public override string from( Configuration configuration )
				  {
						return configuration.Get( this );
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("A dynamic string setting") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<String> dynamic = new StringSetting()
			  [Description("A dynamic string setting")]
			  public static readonly Setting<string> dynamic = new StringSettingAnonymousInnerClass4();

			  private class StringSettingAnonymousInnerClass4 : StringSetting
			  {
				  public override string apply( System.Func<string, string> provider )
				  {
						 string val = provider( name() );
						 if ( string.ReferenceEquals( val, null ) )
						 {
							  val = DefaultValue;
						 }
						 return val;
				  }

					public override string name()
					{
						 return "myDynamicProperty";
					}

					public override void withScope( System.Func<string, string> scopingRule )
					{

					}

					public override string DefaultValue
					{
						get
						{
							 return "defaultDynamic";
						}
					}

					public override string from( Configuration configuration )
					{
						 return configuration.Get( this );
					}
			  }
		 }

		 private abstract class StringSetting : BaseSetting<string>
		 {
			  public override string ValueDescription()
			  {
					return "a String";
			  }
		 }
	}

}
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
namespace Neo4Net.Ext.Udc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using Configuration = Neo4Net.Helpers.Configuration;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcSettings.udc_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Configuration.DEFAULT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UdcSettingsTest
	public class UdcSettingsTest
	{
		 private const string UDC_DISABLE = "dbms.udc.disable";
		 private static readonly UdcSettings _settingsClasses = new UdcSettings();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.helpers.Configuration configuration = new org.neo4j.helpers.Configuration();
		 public readonly Configuration Configuration = new Configuration();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> variations()
		 public static IEnumerable<object[]> Variations()
		 {
			  return Arrays.asList( ( new Variations() ).TrueAs("true").falseAs("false").unknownAs("").parameters(), (new Variations()).TrueAs("True").falseAs("False").unknownAs("no").parameters(), (new Variations()).TrueAs("TRUE").falseAs("FALSE").unknownAs("yes").parameters(), (new Variations()).TrueAs("tRuE").falseAs("fAlSe").unknownAs("foo").parameters() );
		 }

		 private readonly string _trueVariation;
		 private readonly string _falseVariation;
		 private readonly string _unknown;

		 public UdcSettingsTest( Variations variations )
		 {
			  this._trueVariation = variations.TrueVariation;
			  this._falseVariation = variations.FalseVariation;
			  this._unknown = variations.Unknown;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeEnabledByDefault()
		 public virtual void ShouldBeEnabledByDefault()
		 {
			  assertTrue( Configuration.config( _settingsClasses ).get( udc_enabled ) );
			  assertTrue( Config.defaults().get(udc_enabled) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeDisabledByConfigurationProperty()
		 public virtual void ShouldBeDisabledByConfigurationProperty()
		 {
			  assertFalse( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT).config(_settingsClasses).get(udc_enabled) );
			  assertFalse( Config.defaults( singletonMap( udc_enabled.name(), "false" ) ).get(udc_enabled) );
		 }

		 // enabled by default

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableDefault_sysDisableDefault()
		 public virtual void EnableOnSettingDefaultSysEnableDefaultSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

		 // enabled by the setting = true

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableDefault_sysDisableDefault()
		 public virtual void EnableOnSettingTrueSysEnableDefaultSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableDefault_sysDisableFalse()
		 public virtual void EnableOnSettingTrueSysEnableDefaultSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableDefault_sysDisableTrue()
		 public virtual void EnableOnSettingTrueSysEnableDefaultSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableTrue_sysDisableDefault()
		 public virtual void EnableOnSettingTrueSysEnableTrueSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableTrue_sysDisableFalse()
		 public virtual void EnableOnSettingTrueSysEnableTrueSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableTrue_sysDisableTrue()
		 public virtual void EnableOnSettingTrueSysEnableTrueSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableFalse_sysDisableDefault()
		 public virtual void EnableOnSettingTrueSysEnableFalseSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableFalse_sysDisableFalse()
		 public virtual void EnableOnSettingTrueSysEnableFalseSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableFalse_sysDisableTrue()
		 public virtual void EnableOnSettingTrueSysEnableFalseSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _trueVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

		 // enabled by the enabled=true system property

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableTrue_sysDisableDefault()
		 public virtual void EnableOnSettingDefaultSysEnableTrueSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableTrue_sysDisableFalse()
		 public virtual void EnableOnSettingDefaultSysEnableTrueSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableTrue_sysDisableTrue()
		 public virtual void EnableOnSettingDefaultSysEnableTrueSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableTrue_sysDisableDefault()
		 public virtual void EnableOnSettingFalseSysEnableTrueSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableTrue_sysDisableFalse()
		 public virtual void EnableOnSettingFalseSysEnableTrueSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableTrue_sysDisableTrue()
		 public virtual void EnableOnSettingFalseSysEnableTrueSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _trueVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

		 // enabled by the disabled=false system property

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableDefault_sysDisableFalse()
		 public virtual void EnableOnSettingDefaultSysEnableDefaultSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableFalse_sysDisableFalse()
		 public virtual void EnableOnSettingDefaultSysEnableFalseSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableDefault_sysDisableFalse()
		 public virtual void EnableOnSettingFalseSysEnableDefaultSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableFalse_sysDisableFalse()
		 public virtual void EnableOnSettingFalseSysEnableFalseSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

		 // disabled

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingFalse_sysEnableFalse_sysDisableTrue()
		 public virtual void DisableOnSettingFalseSysEnableFalseSysDisableTrue()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingFalse_sysEnableDefault_sysDisableDefault()
		 public virtual void DisableOnSettingFalseSysEnableDefaultSysDisableDefault()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingDefault_sysEnableFalse_sysDisableDefault()
		 public virtual void DisableOnSettingDefaultSysEnableFalseSysDisableDefault()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingDefault_sysEnableDefault_sysDisableTrue()
		 public virtual void DisableOnSettingDefaultSysEnableDefaultSysDisableTrue()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingFalse_sysEnableFalse_sysDisableDefault()
		 public virtual void DisableOnSettingFalseSysEnableFalseSysDisableDefault()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingFalse_sysEnableDefault_sysDisableTrue()
		 public virtual void DisableOnSettingFalseSysEnableDefaultSysDisableTrue()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, _falseVariation ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disableOn_settingDefault_sysEnableFalse_sysDisableTrue()
		 public virtual void DisableOnSettingDefaultSysEnableFalseSysDisableTrue()
		 {
			  AssertDisabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _falseVariation ).withSystemProperty(UDC_DISABLE, _trueVariation) );
		 }

		 // unknown values enables UDC

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableDefault_sysDisableDefault()
		 public virtual void EnableOnSettingUnknownSysEnableDefaultSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableDefault_sysDisableFalse()
		 public virtual void EnableOnSettingUnknownSysEnableDefaultSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableDefault_sysDisableTrue()
		 public virtual void EnableOnSettingUnknownSysEnableDefaultSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, _falseVariation) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableFalse_sysDisableDefault()
		 public virtual void EnableOnSettingUnknownSysEnableFalseSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableFalse_sysDisableFalse()
		 public virtual void EnableOnSettingUnknownSysEnableFalseSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableFalse_sysDisableTrue()
		 public virtual void EnableOnSettingUnknownSysEnableFalseSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableTrue_sysDisableFalse()
		 public virtual void EnableOnSettingUnknownSysEnableTrueSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingUnknown_sysEnableTrue_sysDisableTrue()
		 public virtual void EnableOnSettingUnknownSysEnableTrueSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableUnknown_sysDisableDefault()
		 public virtual void EnableOnSettingDefaultSysEnableUnknownSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableUnknown_sysDisableFalse()
		 public virtual void EnableOnSettingDefaultSysEnableUnknownSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableUnknown_sysDisableTrue()
		 public virtual void EnableOnSettingDefaultSysEnableUnknownSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableUnknown_sysDisableDefault()
		 public virtual void EnableOnSettingTrueSysEnableUnknownSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableUnknown_sysDisableFalse()
		 public virtual void EnableOnSettingTrueSysEnableUnknownSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableUnknown_sysDisableTrue()
		 public virtual void EnableOnSettingTrueSysEnableUnknownSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableUnknown_sysDisableDefault()
		 public virtual void EnableOnSettingFalseSysEnableUnknownSysDisableDefault()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableUnknown_sysDisableFalse()
		 public virtual void EnableOnSettingFalseSysEnableUnknownSysDisableFalse()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableUnknown_sysDisableTrue()
		 public virtual void EnableOnSettingFalseSysEnableUnknownSysDisableTrue()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, DEFAULT ).withSystemProperty( udc_enabled.name(), _unknown ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableDefault_sysDisableUnknown()
		 public virtual void EnableOnSettingDefaultSysEnableDefaultSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableTrue_sysDisableUnknown()
		 public virtual void EnableOnSettingDefaultSysEnableTrueSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingDefault_sysEnableFalse_sysDisableUnknown()
		 public virtual void EnableOnSettingDefaultSysEnableFalseSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableDefault_sysDisableUnknown()
		 public virtual void EnableOnSettingTrueSysEnableDefaultSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableTrue_sysDisableUnknown()
		 public virtual void EnableOnSettingTrueSysEnableTrueSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingTrue_sysEnableFalse_sysDisableUnknown()
		 public virtual void EnableOnSettingTrueSysEnableFalseSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableDefault_sysDisableUnknown()
		 public virtual void EnableOnSettingFalseSysEnableDefaultSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableTrue_sysDisableUnknown()
		 public virtual void EnableOnSettingFalseSysEnableTrueSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableOn_settingFalse_sysEnableFalse_sysDisableUnknown()
		 public virtual void EnableOnSettingFalseSysEnableFalseSysDisableUnknown()
		 {
			  AssertEnabled( Configuration.with( udc_enabled, _unknown ).withSystemProperty( udc_enabled.name(), DEFAULT ).withSystemProperty(UDC_DISABLE, DEFAULT) );
		 }

		 // DSL

		 private static void AssertEnabled( Configuration configuration )
		 {
			  assertTrue( "should be enabled", configuration.Config( _settingsClasses ).get( udc_enabled ) );
		 }

		 private static void AssertDisabled( Configuration configuration )
		 {
			  assertFalse( "should be disabled", configuration.Config( _settingsClasses ).get( udc_enabled ) );
		 }

		 internal sealed class Variations
		 {
			  internal string TrueVariation;
			  internal string FalseVariation;
			  internal string Unknown;

			  internal Variations TrueAs( string trueVariation )
			  {
					this.TrueVariation = trueVariation;
					return this;
			  }

			  internal Variations FalseAs( string falseVariation )
			  {
					this.FalseVariation = falseVariation;
					return this;
			  }

			  internal Variations UnknownAs( string unknown )
			  {
					this.Unknown = unknown;
					return this;
			  }

			  internal object[] Parameters()
			  {
					if ( string.ReferenceEquals( TrueVariation, null ) || string.ReferenceEquals( FalseVariation, null ) || string.ReferenceEquals( Unknown, null ) )
					{
						 throw new System.InvalidOperationException( "Undefined variations." );
					}
					return new object[]{ this };
			  }

			  public override string ToString()
			  {
					return string.Format( "trueAs({0}).falseAs({1}).unknownAs({2})", TrueVariation, FalseVariation, Unknown );
			  }
		 }
	}

}
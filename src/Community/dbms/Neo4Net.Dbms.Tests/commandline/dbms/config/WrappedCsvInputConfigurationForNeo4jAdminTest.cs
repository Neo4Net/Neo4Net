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
namespace Neo4Net.Dbms.CommandLine.Config
{
	using Test = org.junit.jupiter.api.Test;

	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;

	internal class WrappedCsvInputConfigurationForNeo4NetAdminTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateArrayDelimiter()
		 internal virtual void ShouldDelegateArrayDelimiter()
		 {
			  shouldDelegate(expected => new Configuration_OverriddenAnonymousInnerClass(this, COMMAS)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::arrayDelimiter, 'a', 'b');
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override char arrayDelimiter()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateDelimiter()
		 internal virtual void ShouldDelegateDelimiter()
		 {
			  shouldDelegate(expected => new Configuration_OverriddenAnonymousInnerClass2(this, COMMAS)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::delimiter, 'a', 'b');
		 }

		 private class Configuration_OverriddenAnonymousInnerClass2 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass2( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override char delimiter()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateQuoteCharacter()
		 internal virtual void ShouldDelegateQuoteCharacter()
		 {
			  shouldDelegate(expected => new Configuration_OverriddenAnonymousInnerClass3(this, COMMAS)
			 , Configuration.quotationCharacter, 'a', 'b');
		 }

		 private class Configuration_OverriddenAnonymousInnerClass3 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass3( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override char quotationCharacter()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOverrideTrimStrings()
		 internal virtual void ShouldOverrideTrimStrings()
		 {
			  shouldOverride(expected => new Configuration_OverriddenAnonymousInnerClass4(this, COMMAS)
			 , Configuration.trimStrings, true, false);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass4 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass4( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool trimStrings()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOverrideBufferSize()
		 internal virtual void ShouldOverrideBufferSize()
		 {
			  shouldOverride(expected => new Configuration_OverriddenAnonymousInnerClass5(this, COMMAS)
			 , Configuration.bufferSize, 100, 200);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass5 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass5( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override int bufferSize()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateMultiLineFields()
		 internal virtual void ShouldDelegateMultiLineFields()
		 {
			  shouldDelegate(expected => new Configuration_OverriddenAnonymousInnerClass6(this, COMMAS)
			 , Configuration.multilineFields, true, false);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass6 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass6( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool multilineFields()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOverrideEmptyQuotedStringsAsNull()
		 internal virtual void ShouldOverrideEmptyQuotedStringsAsNull()
		 {
			  shouldOverride(expected => new Configuration_OverriddenAnonymousInnerClass7(this, COMMAS)
			 , Configuration.emptyQuotedStringsAsNull, true, false);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass7 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass7( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool emptyQuotedStringsAsNull()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOverrideLegacyStyleQuoting()
		 internal virtual void ShouldOverrideLegacyStyleQuoting()
		 {
			  shouldOverride(expected => new Configuration_OverriddenAnonymousInnerClass8(this, COMMAS)
			 , Configuration.legacyStyleQuoting, true, false);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass8 : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4NetAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass8( WrappedCsvInputConfigurationForNeo4NetAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool legacyStyleQuoting()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldDelegate(System.Func<T,org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration> configFactory, System.Func<org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration,T> getter, T... expectedValues)
		 private void ShouldDelegate<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] expectedValues )
		 {
			  foreach ( T expectedValue in expectedValues )
			  {
					// given
					Configuration configuration = configFactory( expectedValue );

					// when
					WrappedCsvInputConfigurationForNeo4NetAdmin wrapped = new WrappedCsvInputConfigurationForNeo4NetAdmin( configuration );

					// then
					assertEquals( expectedValue, getter( wrapped ) );
			  }

			  // then
			  assertEquals( getter( COMMAS ), getter( new WrappedCsvInputConfigurationForNeo4NetAdmin( COMMAS ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldOverride(System.Func<T,org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration> configFactory, System.Func<org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration,T> getter, T... values)
		 private void ShouldOverride<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] values )
		 {
			  foreach ( T value in values )
			  {
					// given
					Configuration configuration = configFactory( value );
					WrappedCsvInputConfigurationForNeo4NetAdmin vanilla = new WrappedCsvInputConfigurationForNeo4NetAdmin( COMMAS );

					// when
					WrappedCsvInputConfigurationForNeo4NetAdmin wrapped = new WrappedCsvInputConfigurationForNeo4NetAdmin( configuration );

					// then
					assertEquals( getter( vanilla ), getter( wrapped ) );
			  }
		 }
	}

}
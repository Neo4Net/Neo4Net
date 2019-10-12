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
namespace Org.Neo4j.Commandline.dbms.config
{
	using Test = org.junit.jupiter.api.Test;

	using Configuration = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;

	internal class WrappedCsvInputConfigurationForNeo4jAdminTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateArrayDelimiter()
		 internal virtual void ShouldDelegateArrayDelimiter()
		 {
			  shouldDelegate(expected => new Configuration_OverriddenAnonymousInnerClass(this, COMMAS)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::arrayDelimiter, 'a', 'b');
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass2 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass2( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass3 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass3( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass4 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass4( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass5 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass5( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass6 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass6( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass7 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass7( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
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

		 private class Configuration_OverriddenAnonymousInnerClass8 : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 private readonly WrappedCsvInputConfigurationForNeo4jAdminTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass8( WrappedCsvInputConfigurationForNeo4jAdminTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool legacyStyleQuoting()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldDelegate(System.Func<T,org.neo4j.unsafe.impl.batchimport.input.csv.Configuration> configFactory, System.Func<org.neo4j.unsafe.impl.batchimport.input.csv.Configuration,T> getter, T... expectedValues)
		 private void ShouldDelegate<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] expectedValues )
		 {
			  foreach ( T expectedValue in expectedValues )
			  {
					// given
					Configuration configuration = configFactory( expectedValue );

					// when
					WrappedCsvInputConfigurationForNeo4jAdmin wrapped = new WrappedCsvInputConfigurationForNeo4jAdmin( configuration );

					// then
					assertEquals( expectedValue, getter( wrapped ) );
			  }

			  // then
			  assertEquals( getter( COMMAS ), getter( new WrappedCsvInputConfigurationForNeo4jAdmin( COMMAS ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldOverride(System.Func<T,org.neo4j.unsafe.impl.batchimport.input.csv.Configuration> configFactory, System.Func<org.neo4j.unsafe.impl.batchimport.input.csv.Configuration,T> getter, T... values)
		 private void ShouldOverride<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] values )
		 {
			  foreach ( T value in values )
			  {
					// given
					Configuration configuration = configFactory( value );
					WrappedCsvInputConfigurationForNeo4jAdmin vanilla = new WrappedCsvInputConfigurationForNeo4jAdmin( COMMAS );

					// when
					WrappedCsvInputConfigurationForNeo4jAdmin wrapped = new WrappedCsvInputConfigurationForNeo4jAdmin( configuration );

					// then
					assertEquals( getter( vanilla ), getter( wrapped ) );
			  }
		 }
	}

}
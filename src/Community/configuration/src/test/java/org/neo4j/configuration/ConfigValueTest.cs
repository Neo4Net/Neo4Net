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
namespace Neo4Net.Configuration
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.configuration.ConfigValue.valueToString;

	internal class ConfigValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesEmptyValue()
		 internal virtual void HandlesEmptyValue()
		 {
			  ConfigValue value = new ConfigValue( "name", null, null, null, "description", false, false, false, null, false );

			  assertEquals( null, value.Value() );
			  assertEquals( "null", value.ToString() );
			  assertFalse( value.Deprecated() );
			  assertEquals( null, value.Replacement() );
			  assertFalse( value.Internal() );
			  assertFalse( value.Secret() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesInternal()
		 internal virtual void HandlesInternal()
		 {
			  ConfigValue value = new ConfigValue( "name", null, null, null, "description", true, false, false, null, false );

			  assertTrue( value.Internal() );
			  assertFalse( value.Secret() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesNonEmptyValue()
		 internal virtual void HandlesNonEmptyValue()
		 {
			  ConfigValue value = new ConfigValue( "name", null, null, 1, "description", false, false, false, null, false );

			  assertEquals( 1, value.Value() );
			  assertEquals( "1", value.ToString() );
			  assertFalse( value.Deprecated() );
			  assertEquals( null, value.Replacement() );
			  assertFalse( value.Internal() );
			  assertFalse( value.Secret() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesDeprecationAndReplacement()
		 internal virtual void HandlesDeprecationAndReplacement()
		 {
			  ConfigValue value = new ConfigValue( "old_name", null, null, 1, "description", false, false, true, "new_name", false );

			  assertEquals( 1, value.Value() );
			  assertEquals( "1", value.ToString() );
			  assertTrue( value.Deprecated() );
			  assertEquals( "new_name", value.Replacement().get() );
			  assertFalse( value.Internal() );
			  assertFalse( value.Secret() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesValueDescription()
		 internal virtual void HandlesValueDescription()
		 {
			  ConfigValue value = new ConfigValue( "old_name", null, null, 1, "a simple integer", false, false, true, "new_name", false );

			  assertEquals( 1, value.Value() );
			  assertEquals( "1", value.ToString() );
			  assertTrue( value.Deprecated() );
			  assertEquals( "new_name", value.Replacement().get() );
			  assertFalse( value.Internal() );
			  assertFalse( value.Secret() );
			  assertEquals( "a simple integer", value.ValueDescription() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void handlesSecretValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HandlesSecretValue()
		 {
			  ConfigValue value = new ConfigValue( "name", null, null, "secret", "description", false, false, false, null, true );

			  assertEquals( "secret", value.Value() );
			  assertEquals( Secret.OBSFUCATED, value.ToString() );
			  assertFalse( value.Deprecated() );
			  assertEquals( null, value.Replacement() );
			  assertFalse( value.Internal() );
			  assertTrue( value.Secret() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void durationValueIsRepresentedWithUnit()
		 internal virtual void DurationValueIsRepresentedWithUnit()
		 {
			  assertEquals( "120000ms", valueToString( Duration.ofMinutes( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringValueIsRepresentedAsString()
		 internal virtual void StringValueIsRepresentedAsString()
		 {
			  assertEquals( "bob", valueToString( "bob" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intValueIsRepresentedAsInt()
		 internal virtual void IntValueIsRepresentedAsInt()
		 {
			  assertEquals( "7", valueToString( 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullIsHandled()
		 internal virtual void NullIsHandled()
		 {
			  assertEquals( "null", valueToString( null ) );
		 }
	}

}
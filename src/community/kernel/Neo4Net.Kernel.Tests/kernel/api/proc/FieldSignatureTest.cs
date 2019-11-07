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
namespace Neo4Net.Kernel.Api.Procs
{
	using Test = org.junit.Test;

	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.ntString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.FieldSignature.inputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.FieldSignature.outputField;

	public class FieldSignatureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsShouldConsiderName()
		 public virtual void EqualsShouldConsiderName()
		 {
			  assertEquals( "input without default", inputField( "name", Neo4NetTypes.NTString ), inputField( "name", Neo4NetTypes.NTString ) );
			  assertNotEquals( "input without default", inputField( "name", Neo4NetTypes.NTString ), inputField( "other", Neo4NetTypes.NTString ) );

			  assertEquals( "input with default", inputField( "name", Neo4NetTypes.NTString, ntString( "hello" ) ), inputField( "name", Neo4NetTypes.NTString, ntString( "hello" ) ) );
			  assertNotEquals( "input with default", inputField( "name", Neo4NetTypes.NTString, ntString( "hello" ) ), inputField( "other", Neo4NetTypes.NTString, ntString( "hello" ) ) );

			  assertEquals( "output", outputField( "name", Neo4NetTypes.NTString, false ), outputField( "name", Neo4NetTypes.NTString, false ) );
			  assertNotEquals( "output", outputField( "name", Neo4NetTypes.NTString, false ), outputField( "other", Neo4NetTypes.NTString, false ) );

			  assertEquals( "deprecated output", outputField( "name", Neo4NetTypes.NTString, true ), outputField( "name", Neo4NetTypes.NTString, true ) );
			  assertNotEquals( "deprecated output", outputField( "name", Neo4NetTypes.NTString, true ), outputField( "other", Neo4NetTypes.NTString, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTypeCheckDefaultValue()
		 public virtual void ShouldTypeCheckDefaultValue()
		 {
			  // when
			  try
			  {
					inputField( "name", Neo4NetTypes.NTInteger, ntString( "bad" ) );
					fail( "expected exception" );
			  }
			  // then
			  catch ( System.ArgumentException e )
			  {
					assertEquals( e.Message, "Default value does not have a valid type, field type was INTEGER?, but value type was STRING?." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsShouldConsiderType()
		 public virtual void EqualsShouldConsiderType()
		 {
			  assertEquals( "input without default", inputField( "name", Neo4NetTypes.NTString ), inputField( "name", Neo4NetTypes.NTString ) );
			  assertNotEquals( "input without default", inputField( "name", Neo4NetTypes.NTString ), inputField( "name", Neo4NetTypes.NTInteger ) );

			  assertEquals( "output", outputField( "name", Neo4NetTypes.NTString, false ), outputField( "name", Neo4NetTypes.NTString, false ) );
			  assertNotEquals( "output", outputField( "name", Neo4NetTypes.NTString, false ), outputField( "name", Neo4NetTypes.NTInteger, false ) );

			  assertEquals( "deprecated output", outputField( "name", Neo4NetTypes.NTString, true ), outputField( "name", Neo4NetTypes.NTString, true ) );
			  assertNotEquals( "deprecated output", outputField( "name", Neo4NetTypes.NTString, true ), outputField( "name", Neo4NetTypes.NTInteger, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsShouldConsiderDefaultValue()
		 public virtual void EqualsShouldConsiderDefaultValue()
		 {
			  assertEquals( inputField( "name", Neo4NetTypes.NTString, ntString( "foo" ) ), inputField( "name", Neo4NetTypes.NTString, ntString( "foo" ) ) );
			  assertNotEquals( inputField( "name", Neo4NetTypes.NTString, ntString( "bar" ) ), inputField( "name", Neo4NetTypes.NTString, ntString( "baz" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsShouldConsiderDeprecation()
		 public virtual void EqualsShouldConsiderDeprecation()
		 {
			  assertEquals( outputField( "name", Neo4NetTypes.NTString, true ), outputField( "name", Neo4NetTypes.NTString, true ) );
			  assertEquals( outputField( "name", Neo4NetTypes.NTString, false ), outputField( "name", Neo4NetTypes.NTString, false ) );
			  assertNotEquals( outputField( "name", Neo4NetTypes.NTString, true ), outputField( "name", Neo4NetTypes.NTString, false ) );
		 }
	}

}
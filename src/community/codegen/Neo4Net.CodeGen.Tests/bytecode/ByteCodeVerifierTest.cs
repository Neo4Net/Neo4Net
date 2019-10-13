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
namespace Neo4Net.CodeGen.bytecode
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.CodeGenerationTest.PACKAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.CodeGenerator.generateCode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Parameter.param;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.bytecode.ByteCode.BYTECODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.bytecode.ByteCode.VERIFY_GENERATED_BYTECODE;

	public class ByteCodeVerifierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVerifyBytecode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVerifyBytecode()
		 {
			  // given
			  CodeGenerator generator = generateCode( BYTECODE, VERIFY_GENERATED_BYTECODE );

			  ClassHandle handle;
			  using ( ClassGenerator clazz = generator.generateClass( PACKAGE, "SimpleClass" ), CodeBlock code = clazz.generateMethod( typeof( Integer ), "box", param( typeof( int ), "value" ) ) )
			  {
					handle = clazz.Handle();
					code.Returns( code.Load( "value" ) );
			  }

			  // when
			  try
			  {
					handle.LoadClass();
					fail( "Should have thrown exception" );
			  }
			  // then
			  catch ( CompilationFailureException expected )
			  {
					assertThat( expected.ToString(), containsString("box(I)") );
			  }
		 }
	}

}
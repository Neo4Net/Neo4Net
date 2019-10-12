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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Neo4Net.@internal.Kernel.Api.procs.FieldSignature;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;

	public class MethodSignatureCompilerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 public class MyOutputRecord
		 {
			  public string Name;

			  internal MyOutputRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class UnmappableRecord
		 {
			  public UnmappableRecord Wat;
		 }

		 public class ClassWithProcedureWithSimpleArgs
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> echo(@Name("name") String in)
			  public virtual Stream<MyOutputRecord> Echo( string @in )
			  {
					return Stream.of( new MyOutputRecord( @in ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> echoWithoutAnnotations(@Name("name") String in1, String in2)
			  public virtual Stream<MyOutputRecord> EchoWithoutAnnotations( string in1, string in2 )
			  {
					return Stream.of( new MyOutputRecord( in1 + in2 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> echoWithInvalidType(@Name("name") UnmappableRecord in)
			  public virtual Stream<MyOutputRecord> EchoWithInvalidType( UnmappableRecord @in )
			  {
					return Stream.of( new MyOutputRecord( "echo" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapSimpleRecordWithString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMapSimpleRecordWithString()
		 {
			  // When
			  System.Reflection.MethodInfo echo = typeof( ClassWithProcedureWithSimpleArgs ).GetMethod( "echo", typeof( string ) );
			  IList<FieldSignature> signature = ( new MethodSignatureCompiler( new TypeMappers() ) ).SignatureFor(echo);

			  // THen
			  assertThat( signature, contains( FieldSignature.inputField( "name", Neo4jTypes.NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapSimpleFunctionWithString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMapSimpleFunctionWithString()
		 {
			  // When
			  System.Reflection.MethodInfo echo = typeof( ClassWithProcedureWithSimpleArgs ).GetMethod( "echo", typeof( string ) );
			  IList<Neo4jTypes.AnyType> signature = ( new MethodSignatureCompiler( new TypeMappers() ) ).InputTypesFor(echo);

			  // THen
			  assertThat( signature, contains( Neo4jTypes.NTString ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnUnmappable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnUnmappable()
		 {
			  // Given
			  System.Reflection.MethodInfo echo = typeof( ClassWithProcedureWithSimpleArgs ).GetMethod( "echoWithInvalidType", typeof( UnmappableRecord ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Argument `name` at position 0 in `echoWithInvalidType` with%n" + "type `UnmappableRecord` cannot be converted to a Neo4j type: Don't know how to map " + "`org.neo4j.kernel.impl.proc.MethodSignatureCompilerTest$UnmappableRecord` to " + "the Neo4j Type System.%n" + "Please refer to to the documentation for full details.%n" + "For your reference, known types are:" ) );

			  // When
			  ( new MethodSignatureCompiler( new TypeMappers() ) ).SignatureFor(echo);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnMissingAnnotations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnMissingAnnotations()
		 {
			  // Given
			  System.Reflection.MethodInfo echo = typeof( ClassWithProcedureWithSimpleArgs ).GetMethod( "echoWithoutAnnotations", typeof( string ), typeof( string ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Argument at position 1 in method `echoWithoutAnnotations` is missing an `@Name` " + "annotation.%n" + "Please add the annotation, recompile the class and try again." ) );

			  // When
			  ( new MethodSignatureCompiler( new TypeMappers() ) ).SignatureFor(echo);
		 }
	}

}
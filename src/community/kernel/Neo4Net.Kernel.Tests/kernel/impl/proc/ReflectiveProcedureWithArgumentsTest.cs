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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Neo4Net.Collections;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using ResourceTracker = Neo4Net.Kernel.Api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.Api.StubResourceManager;
	using BasicContext = Neo4Net.Kernel.Api.Procs.BasicContext;
	using CallableProcedure = Neo4Net.Kernel.Api.Procs.CallableProcedure;
	using NullLog = Neo4Net.Logging.NullLog;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureSignature;

	public class ReflectiveProcedureWithArgumentsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileSimpleProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileSimpleProcedure()
		 {
			  // When
			  IList<CallableProcedure> procedures = Compile( typeof( ClassWithProcedureWithSimpleArgs ) );

			  // Then
			  assertEquals( 1, procedures.Count );
			  assertThat( procedures[0].Signature(), equalTo(procedureSignature("org", "Neo4Net", "kernel", "impl", "proc", "listCoolPeople").@in("name", Neo4NetTypes.NTString).@in("age", Neo4NetTypes.NTInteger).@out("name", Neo4NetTypes.NTString).build()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleProcedure()
		 {
			  // Given
			  CallableProcedure procedure = Compile( typeof( ClassWithProcedureWithSimpleArgs ) )[0];

			  // When
			  RawIterator<object[], ProcedureException> @out = procedure.Apply( new BasicContext(), new object[]{ "Pontus", 35L }, _resourceTracker );

			  // Then
			  IList<object[]> collect = new IList<object[]> { @out };
			  assertThat( collect[0][0], equalTo( "Pontus is 35 years old." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunGenericProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunGenericProcedure()
		 {
			  // Given
			  CallableProcedure procedure = Compile( typeof( ClassWithProcedureWithGenericArgs ) )[0];

			  // When
			  RawIterator<object[], ProcedureException> @out = procedure.Apply( new BasicContext(), new object[]{ Arrays.asList("Roland", "Eddie", "Susan", "Jake"), Arrays.asList(1000L, 23L, 29L, 12L) }, _resourceTracker );

			  // Then
			  IList<object[]> collect = new IList<object[]> { @out };
			  assertThat( collect[0][0], equalTo( "Roland is 1000 years old." ) );
			  assertThat( collect[1][0], equalTo( "Eddie is 23 years old." ) );
			  assertThat( collect[2][0], equalTo( "Susan is 29 years old." ) );
			  assertThat( collect[3][0], equalTo( "Jake is 12 years old." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMissingAnnotations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMissingAnnotations()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Argument at position 0 in method `listCoolPeople` " + "is missing an `@Name` annotation.%n" + "Please add the annotation, recompile the class and try again." ) );

			  // When
			  Compile( typeof( ClassWithProcedureWithoutAnnotatedArgs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMisplacedDefaultValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMisplacedDefaultValue()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Non-default argument at position 2 with name c in method defaultValues follows default argument. " + "Add a default value or rearrange arguments so that the non-default values comes first." );

			  // When
			  Compile( typeof( ClassWithProcedureWithMisplacedDefault ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfWronglyTypedDefaultValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfWronglyTypedDefaultValue()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Argument `a` at position 0 in `defaultValues` with%n" + "type `long` cannot be converted to a Neo4Net type: Default value `forty-two` could not be parsed as a " + "Long" ) );

			  // When
			  Compile( typeof( ClassWithProcedureWithBadlyTypedDefault ) );
		 }

		 public class MyOutputRecord
		 {
			  public string Name;

			  public MyOutputRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class ClassWithProcedureWithSimpleArgs
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople(@Name("name") String name, @Name("age") long age)
			  public virtual Stream<MyOutputRecord> ListCoolPeople( string name, long age )
			  {
					return Stream.of( new MyOutputRecord( name + " is " + age + " years old." ) );
			  }
		 }

		 public class ClassWithProcedureWithGenericArgs
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople(@Name("names") java.util.List<String> names, @Name("age") java.util.List<long> ages)
			  public virtual Stream<MyOutputRecord> ListCoolPeople( IList<string> names, IList<long> ages )
			  {
					IEnumerator<string> nameIterator = names.GetEnumerator();
					IEnumerator<long> ageIterator = ages.GetEnumerator();
					IList<MyOutputRecord> result = new List<MyOutputRecord>( names.Count );
					while ( nameIterator.MoveNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 long age = ageIterator.hasNext() ? ageIterator.next() : -1;
						 result.Add( new MyOutputRecord( nameIterator.Current + " is " + age + " years old." ) );
					}
					return result.stream();
			  }
		 }

		 public class ClassWithProcedureWithoutAnnotatedArgs
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople(String name, int age)
			  public virtual Stream<MyOutputRecord> ListCoolPeople( string name, int age )
			  {
					return Stream.of( new MyOutputRecord( name + " is " + age + " years old." ) );
			  }
		 }

		 public class ClassWithProcedureWithDefaults
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> defaultValues(@Name(value = "a", defaultValue = "a") String a, @Name(value = "b", defaultValue = "42") long b, @Name(value = "c", defaultValue = "3.14") double c)
			  public virtual Stream<MyOutputRecord> DefaultValues( string a, long b, double c )
			  {
					return Stream.empty();
			  }
		 }

		 public class ClassWithProcedureWithMisplacedDefault
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> defaultValues(@Name("a") String a, @Name(value = "b", defaultValue = "42") long b, @Name("c") Object c)
			  public virtual Stream<MyOutputRecord> DefaultValues( string a, long b, object c )
			  {
					return Stream.empty();
			  }
		 }

		 public class ClassWithProcedureWithBadlyTypedDefault
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> defaultValues(@Name(value = "a", defaultValue = "forty-two") long b)
			  public virtual Stream<MyOutputRecord> DefaultValues( long b )
			  {
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<Neo4Net.kernel.api.proc.CallableProcedure> compile(Class clazz) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private IList<CallableProcedure> Compile( Type clazz )
		 {
			  return ( new ReflectiveProcedureCompiler( new TypeMappers(), new ComponentRegistry(), new ComponentRegistry(), NullLog.Instance, ProcedureConfig.Default ) ).compileProcedure(clazz, null, true);
		 }
	}

}
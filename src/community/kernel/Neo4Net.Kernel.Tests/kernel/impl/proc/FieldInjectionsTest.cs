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

	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Context = Neo4Net.Procedure.Context;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class FieldInjectionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowClassesWithNonInjectedFields() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowClassesWithNonInjectedFields()
		 {
			  // Given
			  FieldInjections injections = new FieldInjections( new ComponentRegistry() );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Field `someState` on `ProcedureWithNonInjectedMemberFields` " + "is not annotated as a @Context and is not static. " + "If you want to store state along with your procedure, " + "please use a static field." );

			  // When
			  injections.Setters( typeof( ProcedureWithNonInjectedMemberFields ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonPublicFieldsForInjection() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonPublicFieldsForInjection()
		 {
			  // Given
			  FieldInjections injections = new FieldInjections( new ComponentRegistry() );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Field `someState` on `ProcedureWithPrivateMemberField` must be non-final and public." );

			  // When
			  injections.Setters( typeof( ProcedureWithPrivateMemberField ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void staticFieldsAreAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StaticFieldsAreAllowed()
		 {
			  // Given
			  FieldInjections injections = new FieldInjections( new ComponentRegistry() );

			  // When
			  IList<FieldInjections.FieldSetter> setters = injections.Setters( typeof( ProcedureWithStaticFields ) );

			  // Then
			  assertEquals( 0, setters.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void inheritanceIsAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void InheritanceIsAllowed()
		 {
			  // Given
			  ComponentRegistry components = new ComponentRegistry();
			  components.Register( typeof( int ), ctx => 1337 );
			  FieldInjections injections = new FieldInjections( components );

			  // When
			  IList<FieldInjections.FieldSetter> setters = injections.Setters( typeof( ChildProcedure ) );

			  // Then
			  ChildProcedure childProcedure = new ChildProcedure();
			  foreach ( FieldInjections.FieldSetter setter in setters )
			  {
					setter.Apply( null, childProcedure );
			  }

			  assertEquals( 1337, childProcedure.ChildField );
			  assertEquals( 1337, childProcedure.ParentField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void syntheticsAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SyntheticsAllowed()
		 {
			  // Given
			  ComponentRegistry components = new ComponentRegistry();
			  components.Register( typeof( int ), ctx => 1337 );
			  FieldInjections injections = new FieldInjections( components );

			  // When
			  IList<FieldInjections.FieldSetter> setters = injections.Setters( typeof( Outer.ClassWithSyntheticField ) );

			  // Then
			  Outer.ClassWithSyntheticField syntheticField = ( new Outer() ).ClassWithSyntheticField();
			  foreach ( FieldInjections.FieldSetter setter in setters )
			  {
					setter.Apply( null, syntheticField );
			  }

			  assertEquals( 1337, syntheticField.InnerField );
		 }

		 public class ProcedureWithNonInjectedMemberFields
		 {
			  public bool SomeState;
		 }

		 public class ProcedureWithPrivateMemberField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context private boolean someState;
			  internal bool SomeState;
		 }

		 public class ProcedureWithStaticFields
		 {
			  internal static bool SomeState;
		 }

		 public class ParentProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public int parentField;
			  public int ParentField;
		 }

		 public class ChildProcedure : ParentProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public int childField;
			  public int ChildField;
		 }

		 //The outer class is just here to force a synthetic field in the inner class.
		 //This is not a realistic scenario but we merely want to make sure the loader
		 //does not choke on synthetic fields since compilers, e.g. groovy, can generate
		 //these.
		 public class Outer
		 {
			  internal virtual ClassWithSyntheticField ClassWithSyntheticField()
			  {
					return new ClassWithSyntheticField( this );
			  }

			  public class ClassWithSyntheticField
			  {
				  private readonly FieldInjectionsTest.Outer _outerInstance;

				  public ClassWithSyntheticField( FieldInjectionsTest.Outer outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					//this class will have a generated field:
					//synthetic Outer this$0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public int innerField;
					public int InnerField;
			  }
		 }
	}

}
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
namespace Neo4Net.Tooling.procedure.validators
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Procedure = Neo4Net.Procedure.Procedure;
	using CustomNameExtractor = Neo4Net.Tooling.procedure.compilerutils.CustomNameExtractor;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using DefaultProcedureA = Neo4Net.Tooling.procedure.validators.examples.DefaultProcedureA;
	using DefaultProcedureB = Neo4Net.Tooling.procedure.validators.examples.DefaultProcedureB;
	using OverriddenProcedureB = Neo4Net.Tooling.procedure.validators.examples.OverriddenProcedureB;
	using OverriddenProcedureA = Neo4Net.Tooling.procedure.validators.examples.@override.OverriddenProcedureA;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.groups.Tuple.tuple;

	public class DuplicatedExtensionValidatorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilation = new com.google.testing.compile.CompilationRule();
		 public CompilationRule Compilation = new CompilationRule();

		 private Elements _elements;
		 private System.Func<ICollection<Element>, Stream<CompilationMessage>> _validator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  _elements = Compilation.Elements;
			  _validator = new DuplicatedExtensionValidator<ICollection<Element>, Stream<CompilationMessage>>( _elements, typeof( Procedure ), proc => CustomNameExtractor.getName( proc.name, proc.value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detects_duplicate_procedure_with_default_names()
		 public virtual void DetectsDuplicateProcedureWithDefaultNames()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Element procedureA = ProcedureMethod( typeof( DefaultProcedureA ).FullName );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Element procedureB = ProcedureMethod( typeof( DefaultProcedureB ).FullName );
			  ICollection<Element> duplicates = asList( procedureA, procedureB );

			  Stream<CompilationMessage> errors = _validator.apply( duplicates );

			  string procedureName = "org.Neo4Net.tooling.procedure.validators.examples.procedure";
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).containsExactlyInAnyOrder( tuple( Diagnostic.Kind.ERROR, procedureA, "Procedure|function name <" + procedureName + "> is already defined 2 times. It should be defined " + "only once!" ), tuple( Diagnostic.Kind.ERROR, procedureB, "Procedure|function name <" + procedureName + "> is already defined 2 times. It should be defined only once!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detects_duplicate_procedure_with_overridden_names()
		 public virtual void DetectsDuplicateProcedureWithOverriddenNames()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Element procedureA = ProcedureMethod( typeof( OverriddenProcedureA ).FullName );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Element procedureB = ProcedureMethod( typeof( OverriddenProcedureB ).FullName );
			  ICollection<Element> duplicates = asList( procedureA, procedureB );

			  Stream<CompilationMessage> errors = _validator.apply( duplicates );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).containsExactlyInAnyOrder( tuple( Diagnostic.Kind.ERROR, procedureA, "Procedure|function name <override> is already defined 2 times. It should be defined only once!" ), tuple( Diagnostic.Kind.ERROR, procedureB, "Procedure|function name <override> is already defined 2 times. It should be defined only " + "once!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void does_not_detect_duplicates_if_duplicate_procedure_has_custom_name()
		 public virtual void DoesNotDetectDuplicatesIfDuplicateProcedureHasCustomName()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  ICollection<Element> duplicates = asList( ProcedureMethod( typeof( DefaultProcedureA ).FullName ), ProcedureMethod( typeof( OverriddenProcedureB ).FullName ) );

			  Stream<CompilationMessage> errors = _validator.apply( duplicates );

			  assertThat( errors ).Empty;
		 }

		 private Element ProcedureMethod( string name )
		 {
			  TypeElement typeElement = _elements.getTypeElement( name );
			  ICollection<Element> procedures = FindProcedures( typeElement );
			  if ( procedures.Count != 1 )
			  {
					throw new AssertionError( "Test procedure class should only have 1 defined procedure" );
			  }
			  return procedures.GetEnumerator().next();
		 }

		 private ICollection<Element> FindProcedures( TypeElement typeElement )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return typeElement.EnclosedElements.Where( element => element.getAnnotation( typeof( Procedure ) ) != null ).collect( Collectors.toList<Element>() );
		 }

	}

}
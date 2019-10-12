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
namespace Org.Neo4j.Kernel.impl.proc
{

	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using DefaultParameterValue = Org.Neo4j.@internal.Kernel.Api.procs.DefaultParameterValue;
	using FieldSignature = Org.Neo4j.@internal.Kernel.Api.procs.FieldSignature;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using DefaultValueConverter = Org.Neo4j.Kernel.impl.proc.TypeMappers.DefaultValueConverter;
	using Name = Org.Neo4j.Procedure.Name;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.FieldSignature.inputField;

	/// <summary>
	/// Given a java method, figures out a valid <seealso cref="ProcedureSignature"/> field signature.
	/// Basically, it takes the java signature and spits out the same signature described as Neo4j types.
	/// </summary>
	public class MethodSignatureCompiler
	{
		 private readonly TypeMappers _typeMappers;

		 public MethodSignatureCompiler( TypeMappers typeMappers )
		 {
			  this._typeMappers = typeMappers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.neo4j.internal.kernel.api.procs.Neo4jTypes.AnyType> inputTypesFor(Method method) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual IList<Neo4jTypes.AnyType> InputTypesFor( System.Reflection.MethodInfo method )
		 {
			  Type[] types = method.GenericParameterTypes;
			  IList<Neo4jTypes.AnyType> neoTypes = new List<Neo4jTypes.AnyType>( types.Length );
			  foreach ( Type type in types )
			  {
					neoTypes.Add( _typeMappers.toNeo4jType( type ) );
			  }

			  return neoTypes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.neo4j.internal.kernel.api.procs.FieldSignature> signatureFor(Method method) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual IList<FieldSignature> SignatureFor( System.Reflection.MethodInfo method )
		 {
			  Parameter[] @params = method.Parameters;
			  Type[] types = method.GenericParameterTypes;
			  IList<FieldSignature> signature = new List<FieldSignature>( @params.Length );
			  bool seenDefault = false;
			  for ( int i = 0; i < @params.Length; i++ )
			  {
					Parameter param = @params[i];
					Type type = types[i];

					if ( !param.isAnnotationPresent( typeof( Name ) ) )
					{
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Argument at position %d in method `%s` is missing an `@%s` annotation.%n" + "Please add the annotation, recompile the class and try again.", i, method.Name, typeof( Name ).Name );
					}
					Name parameter = param.getAnnotation( typeof( Name ) );
					string name = parameter.value();

					if ( name.Trim().Length == 0 )
					{
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Argument at position %d in method `%s` is annotated with a name,%n" + "but the name is empty, please provide a non-empty name for the argument.", i, method.Name );
					}

					try
					{
						 DefaultValueConverter valueConverter = _typeMappers.converterFor( type );
						 Optional<DefaultParameterValue> defaultValue = valueConverter.DefaultValue( parameter );
						 //it is not allowed to have holes in default values
						 if ( seenDefault && !defaultValue.Present )
						 {
							  throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Non-default argument at position %d with name %s in method %s follows default argument. " + "Add a default value or rearrange arguments so that the non-default values comes first.", i, parameter.value(), method.Name );
						 }

						 seenDefault = defaultValue.Present;

						 // Currently only byte[] is not supported as a Cypher type, so we have specific conversion here.
						 // Should we add more unsupported types we should generalize this.
						 if ( type == typeof( sbyte[] ) )
						 {
							  FieldSignature.InputMapper mapper = new ByteArrayConverter();
							  signature.Add( defaultValue.map( neo4jValue => inputField( name, valueConverter.Type(), neo4jValue, mapper ) ).orElseGet(() => inputField(name, valueConverter.Type(), mapper)) );
						 }
						 else
						 {
							  signature.Add( defaultValue.map( neo4jValue => inputField( name, valueConverter.Type(), neo4jValue ) ).orElseGet(() => inputField(name, valueConverter.Type())) );
						 }
					}
					catch ( ProcedureException e )
					{
						 throw new ProcedureException( e.Status(), "Argument `%s` at position %d in `%s` with%n" + "type `%s` cannot be converted to a Neo4j type: %s", name, i, method.Name, param.Type.SimpleName, e.Message );
					}

			  }

			  return signature;
		 }
	}

}
using System;
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
namespace Neo4Net.Codegen
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.MethodDeclaration.TypeParameter.NO_PARAMETERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.MethodDeclaration.constructor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.MethodDeclaration.method;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.MethodReference.methodReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.NO_TYPES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.typeReference;

	public class ClassGenerator : AutoCloseable
	{
		 private readonly ClassHandle _handle;
		 private ClassEmitter _emitter;
		 private IDictionary<string, FieldReference> _fields;
		 private bool _hasConstructor;

		 internal ClassGenerator( ClassHandle handle, ClassEmitter emitter )
		 {
			  this._handle = handle;
			  this._emitter = emitter;
		 }

		 public override void Close()
		 {
			  if ( !_hasConstructor )
			  {
					Generate( MethodTemplate.Constructor().invokeSuper().build() );
			  }
			  _emitter.done();
			  _handle.generator.closeClass();
			  _emitter = InvalidState.CLASS_DONE;
		 }

		 public virtual ClassHandle Handle()
		 {
			  return _handle;
		 }

		 public virtual FieldReference Field( Type type, string name )
		 {
			  return Field( typeReference( type ), name );
		 }

		 public virtual FieldReference Field( TypeReference type, string name )
		 {
			  return EmitField( Modifier.PUBLIC, type, name, null );
		 }

		 public virtual FieldReference StaticField( Type type, string name, Expression value )
		 {
			  return StaticField( typeReference( type ), name, value );
		 }

		 public virtual FieldReference StaticField( TypeReference type, string name )
		 {
			  return EmitField( Modifier.PUBLIC | Modifier.STATIC, type, name, null );
		 }

		 public virtual FieldReference StaticField( TypeReference type, string name, Expression value )
		 {
			  return EmitField( Modifier.PRIVATE | Modifier.STATIC | Modifier.FINAL, type, name, Objects.requireNonNull( value ) );
		 }

		 private FieldReference EmitField( int modifiers, TypeReference type, string name, Expression value )
		 {
			  if ( _fields == null )
			  {
					_fields = new Dictionary<string, FieldReference>();
			  }
			  else if ( _fields.ContainsKey( name ) )
			  {
					throw new System.ArgumentException( _handle + " already has a field '" + name + "'" );
			  }
			  FieldReference field = new FieldReference( modifiers, _handle, type, name );
			  _fields[name] = field;
			  _emitter.field( field, value );
			  return field;
		 }

		 public virtual MethodReference Generate( MethodTemplate template, params Binding[] bindings )
		 {
			  using ( CodeBlock generator = Generate( template.Declaration( _handle ) ) )
			  {
					template.Generate( generator );
			  }
			  return methodReference( _handle, template.ReturnType(), template.Name(), template.Modifiers(), template.ParameterTypes() );
		 }

		 public virtual CodeBlock GenerateConstructor( params Parameter[] parameters )
		 {
			  return Generate( constructor( _handle, parameters,NO_TYPES, Modifier.PUBLIC, NO_PARAMETERS ) );
		 }

		 public virtual CodeBlock GenerateConstructor( int modifiers, params Parameter[] parameters )
		 {
			  return Generate( constructor( _handle, parameters,NO_TYPES, modifiers, NO_PARAMETERS ) );
		 }

		 public virtual CodeBlock GenerateMethod( Type returnType, string name, params Parameter[] parameters )
		 {
			  return generateMethod( typeReference( returnType ), name, Modifier.PUBLIC, parameters );
		 }

		 public virtual CodeBlock GenerateMethod( Type returnType, string name, int modifiers, params Parameter[] parameters )
		 {
			  return generateMethod( typeReference( returnType ), name, modifiers, parameters );
		 }

		 public virtual CodeBlock GenerateMethod( TypeReference returnType, string name, params Parameter[] parameters )
		 {
			  return Generate( method( _handle, returnType, name, parameters,NO_TYPES, Modifier.PUBLIC, NO_PARAMETERS ) );
		 }

		 public virtual CodeBlock GenerateMethod( TypeReference returnType, string name, int modifiers, params Parameter[] parameters )
		 {
			  return Generate( method( _handle, returnType, name, parameters,NO_TYPES, modifiers, NO_PARAMETERS ) );
		 }

		 public virtual CodeBlock Generate( MethodDeclaration.Builder builder )
		 {
			  return Generate( builder.Build( _handle ) );
		 }

		 private CodeBlock Generate( MethodDeclaration declaration )
		 {
			  if ( declaration.Constructor )
			  {
					_hasConstructor = true;
			  }
			  return new CodeBlock( this, _emitter.method( declaration ), declaration.Parameters() );
		 }

		 internal virtual FieldReference GetField( string name )
		 {
			  return _fields == null ? null : _fields[name];
		 }
	}

}
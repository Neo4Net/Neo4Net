using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.CodeGen
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeVisitor_Fields.DO_NOTHING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.CodeGenerationStrategy.codeGenerator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.OBJECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReferences;

	public abstract class CodeGenerator
	{
		 private readonly CodeLoader _loader;
		 private long _generation;
		 private long _classes;
		 private ByteCodeVisitor _byteCodeVisitor = DO_NOTHING;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CodeGenerator generateCode(CodeGenerationStrategy<?> strategy, CodeGeneratorOption... options) throws CodeGenerationNotSupportedException
		 public static CodeGenerator GenerateCode<T1>( CodeGenerationStrategy<T1> strategy, params CodeGeneratorOption[] options )
		 {
			  return GenerateCode( Thread.CurrentThread.ContextClassLoader, strategy, options );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CodeGenerator generateCode(ClassLoader loader, CodeGenerationStrategy<?> strategy, CodeGeneratorOption... options) throws CodeGenerationNotSupportedException
		 public static CodeGenerator GenerateCode<T1>( ClassLoader loader, CodeGenerationStrategy<T1> strategy, params CodeGeneratorOption[] options )
		 {
			  return codeGenerator( requireNonNull( loader, "ClassLoader" ), strategy, options );
		 }

		 public CodeGenerator( ClassLoader loader )
		 {
			  this._loader = new CodeLoader( loader );
		 }

		 public virtual ClassGenerator GenerateClass( string packageName, string name, Type firstInterface, params Type[] more )
		 {
			  return generateClass( packageName, name, typeReferences( firstInterface, more ) );
		 }

		 public virtual ClassGenerator GenerateClass( Type @base, string packageName, string name, params Type[] interfaces )
		 {
			  return generateClass( typeReference( @base ), packageName, name, typeReferences( interfaces ) );
		 }

		 public virtual ClassGenerator GenerateClass( string packageName, string name, params TypeReference[] interfaces )
		 {
			  return generateClass( OBJECT, packageName, name, interfaces );
		 }

		 public virtual ClassGenerator GenerateClass( TypeReference @base, string packageName, string name, params TypeReference[] interfaces )
		 {
			  return GenerateClass( MakeHandle( packageName, name, @base ), @base, interfaces );
		 }

		 private ClassHandle MakeHandle( string packageName, string name, TypeReference parent )
		 {
			 lock ( this )
			 {
				  _classes++;
				  return new ClassHandle( packageName, name, parent, this, _generation );
			 }
		 }

		 private ClassGenerator GenerateClass( ClassHandle handle, TypeReference @base, params TypeReference[] interfaces )
		 {
			  return new ClassGenerator( handle, Generate( handle, @base, interfaces ) );
		 }

		 protected internal abstract ClassEmitter Generate( TypeReference type, TypeReference @base, params TypeReference[] interfaces );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract Iterable<? extends ByteCodes> compile(ClassLoader classpathLoader) throws CompilationFailureException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal abstract IEnumerable<ByteCodes> Compile( ClassLoader classpathLoader );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized Class loadClass(String name, long generation) throws CompilationFailureException
		 internal virtual Type LoadClass( string name, long generation )
		 {
			 lock ( this )
			 {
				  if ( generation == this._generation )
				  {
						if ( _classes != 0 )
						{
							 throw new System.InvalidOperationException( "Compilation has not completed." );
						}
						this._generation++;
						_loader.addSources( Compile( _loader.Parent ), _byteCodeVisitor );
				  }
				  try
				  {
						return _loader.findClass( name );
				  }
				  catch ( ClassNotFoundException e )
				  {
						throw new System.InvalidOperationException( "Could not find defined class.", e );
				  }
			 }
		 }

		 internal virtual void CloseClass()
		 {
			 lock ( this )
			 {
				  _classes--;
			 }
		 }

		 internal virtual ByteCodeVisitor ByteCodeVisitor
		 {
			 set
			 {
				  this._byteCodeVisitor = value;
			 }
		 }
	}

}
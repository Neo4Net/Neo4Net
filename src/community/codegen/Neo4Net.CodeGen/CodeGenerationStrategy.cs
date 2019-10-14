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
namespace Neo4Net.CodeGen
{

	public abstract class CodeGenerationStrategy<Configuration> : CodeGeneratorOption
	{
		 protected internal abstract Configuration CreateConfigurator( ClassLoader loader );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract CodeGenerator createCodeGenerator(ClassLoader loader, Configuration configuration) throws CodeGenerationStrategyNotSupportedException;
		 protected internal abstract CodeGenerator CreateCodeGenerator( ClassLoader loader, Configuration configuration );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static CodeGenerator codeGenerator(ClassLoader loader, CodeGenerationStrategy<?> strategy, CodeGeneratorOption... options) throws CodeGenerationNotSupportedException
		 internal static CodeGenerator CodeGenerator<T1>( ClassLoader loader, CodeGenerationStrategy<T1> strategy, params CodeGeneratorOption[] options )
		 {
			  return ApplyTo( new Choice( strategy ), options ).generateCode( loader, options );
		 }

		 public override void ApplyTo( object target )
		 {
			  if ( target is Choice )
			  {
					( ( Choice ) target ).Strategy = this;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private CodeGenerator generateCode(ClassLoader loader, CodeGeneratorOption... options) throws CodeGenerationStrategyNotSupportedException
		 private CodeGenerator GenerateCode( ClassLoader loader, params CodeGeneratorOption[] options )
		 {
			  Configuration configurator = CreateConfigurator( loader );
			  return CreateCodeGenerator( loader, ApplyTo( configurator, options ) );
		 }

		 public override string ToString()
		 {
			  return "CodeGenerationStrategy:" + Name();
		 }

		 protected internal abstract string Name();

		 private class Choice : ByteCodeVisitor_Configurable
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private CodeGenerationStrategy<?> strategy;
			  internal CodeGenerationStrategy<object> StrategyConflict;
			  internal IList<ByteCodeVisitor> Visitors;

			  internal Choice( CodeGeneratorOption option )
			  {
					option.ApplyTo( this );
			  }

			  internal virtual CodeGenerationStrategy<T1> Strategy<T1>
			  {
				  set
				  {
						this.StrategyConflict = value;
				  }
			  }

			  public override void AddByteCodeVisitor( ByteCodeVisitor visitor )
			  {
					if ( Visitors == null )
					{
						 Visitors = new List<ByteCodeVisitor>();
					}
					Visitors.Add( visitor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CodeGenerator generateCode(ClassLoader loader, CodeGeneratorOption[] options) throws CodeGenerationNotSupportedException
			  internal virtual CodeGenerator GenerateCode( ClassLoader loader, CodeGeneratorOption[] options )
			  {
					CodeGenerator generator = StrategyConflict.generateCode( loader, options );
					if ( Visitors != null )
					{
						 if ( Visitors.Count == 1 )
						 {
							  generator.ByteCodeVisitor = Visitors[0];
						 }
						 else
						 {
							  generator.ByteCodeVisitor = new ByteCodeVisitor_Multiplex( Visitors.ToArray() );
						 }
					}
					return generator;
			  }
		 }

		 private static Target ApplyTo<Target>( Target target, CodeGeneratorOption[] options )
		 {
			  if ( target is object[] )
			  {
					foreach ( object @object in ( object[] ) target )
					{
						 ApplyTo( @object, options );
					}
			  }
			  else
			  {
					foreach ( CodeGeneratorOption option in options )
					{
						 option.ApplyTo( target );
					}
			  }
			  return target;
		 }
	}

}
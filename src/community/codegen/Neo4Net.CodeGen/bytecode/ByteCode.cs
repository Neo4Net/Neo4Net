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
namespace Neo4Net.CodeGen.ByteCode
{
	using Neo4Net.CodeGen;

	public sealed class ByteCode : CodeGeneratorOption
	{
		 public static readonly ByteCode  = new ByteCode( "", InnerEnum. );

		 private static readonly IList<ByteCode> valueList = new List<ByteCode>();

		 static ByteCode()
		 {
			 valueList.Add();
		 }

		 public enum InnerEnum
		 {
          
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private ByteCode( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }
		 public static readonly Neo4Net.CodeGen.CodeGenerationStrategy<JavaToDotNetGenericWildcard> BYTECODE = new CodeGenerationStrategyAnonymousInnerClass();
		 public static readonly Neo4Net.CodeGen.CodeGeneratorOption VERIFY_GENERATED_BYTECODE = load( "Verifier" );

		 public void ApplyTo( object target )
		 {
			  if ( target is Configuration )
			  {
					( ( Configuration ) target ).WithFlag( this );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class CodeGenerationStrategyAnonymousInnerClass extends Neo4Net.codegen.CodeGenerationStrategy<Configuration>
	//	 {
	//		 @@Override protected Configuration createConfigurator(ClassLoader loader)
	//		 {
	//			  return new Configuration();
	//		 }
	//
	//		 @@Override protected CodeGenerator createCodeGenerator(ClassLoader loader, Configuration configuration)
	//		 {
	//			  return new ByteCodeGenerator(loader, configuration);
	//		 }
	//
	//		 @@Override protected String name()
	//		 {
	//			  return "BYTECODE";
	//		 }
	//	 }

		 private static Neo4Net.CodeGen.CodeGeneratorOption Load( string option )
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return ( CodeGeneratorOption ) Type.GetType( typeof( ByteCode ).FullName + option ).getDeclaredMethod( "load" + option ).invoke( null );
			  }
			  catch ( Exception )
			  {
					return BLANK_OPTION;
			  }
		 }

		public static IList<ByteCode> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ByteCode ValueOf( string name )
		{
			foreach ( ByteCode enumInstance in ByteCode.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}
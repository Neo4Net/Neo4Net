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
namespace Neo4Net.Test.runner
{
	using Test = org.junit.Test;
	using Runner = org.junit.runner.Runner;
	using BlockJUnit4ClassRunner = org.junit.runners.BlockJUnit4ClassRunner;
	using Suite = org.junit.runners.Suite;
	using InitializationError = org.junit.runners.model.InitializationError;
	using RunnerBuilder = org.junit.runners.model.RunnerBuilder;


	public class ParameterizedSuiteRunner : Suite
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public ParameterizedSuiteRunner(Class testClass) throws org.junit.runners.model.InitializationError
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public ParameterizedSuiteRunner( Type testClass ) : this( testClass, new ParameterBuilder( testClass ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ParameterizedSuiteRunner(Class testClass, ParameterBuilder builder) throws org.junit.runners.model.InitializationError
		 internal ParameterizedSuiteRunner( Type testClass, ParameterBuilder builder ) : base( builder, testClass, builder.SuiteClasses() )
		 {
		 }

		 private class ParameterBuilder : RunnerBuilder
		 {
			  internal readonly IDictionary<Type, Parameterization> Parameterizations = new Dictionary<Type, Parameterization>();
			  internal readonly Type SuiteClass;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ParameterBuilder(Class suiteClass) throws org.junit.runners.model.InitializationError
			  internal ParameterBuilder( Type suiteClass )
			  {
					this.SuiteClass = suiteClass;
					bool ok = false;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Constructor<?> suiteConstructor : suiteClass.getConstructors())
					foreach ( System.Reflection.ConstructorInfo<object> suiteConstructor in suiteClass.GetConstructors() )
					{
						 if ( suiteConstructor.ParameterTypes.length == 0 )
						 {
							  if ( Modifier.isPublic( suiteConstructor.Modifiers ) )
							  {
									ok = true;
							  }
							  break;
						 }
					}
					IList<Exception> errors = new List<Exception>();
					if ( !ok )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 errors.Add( new System.ArgumentException( "Suite class (" + suiteClass.FullName + ") does not have a public zero-arg constructor." ) );
					}
					if ( Modifier.isAbstract( suiteClass.Modifiers ) )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 errors.Add( new System.ArgumentException( "Suite class (" + suiteClass.FullName + ") is abstract." ) );
					}
					BuildParameterizations( Parameterizations, suiteClass, errors );
					if ( errors.Count > 0 )
					{
						 throw new InitializationError( errors );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.junit.runner.Runner runnerForClass(Class testClass) throws Throwable
			  public override Runner RunnerForClass( Type testClass )
			  {
					if ( testClass == this.SuiteClass )
					{
						 return new BlockJUnit4ClassRunner( testClass );
					}
					return Parameterizations[testClass];
			  }

			  internal virtual Type[] SuiteClasses()
			  {
					List<Type> classes = new List<Type>( Parameterizations.Keys );
					foreach ( System.Reflection.MethodInfo method in SuiteClass.GetMethods() )
					{
						 if ( method.getAnnotation( typeof( Test ) ) != null )
						 {
							  classes.Add( SuiteClass );
						 }
					}
					return classes.ToArray();
			  }

			  internal virtual void BuildParameterizations( IDictionary<Type, Parameterization> result, Type type, IList<Exception> errors )
			  {
					if ( type == typeof( object ) )
					{
						 return;
					}
					BuildParameterizations( result, type.BaseType, errors );
					SuiteClasses annotation = type.getAnnotation( typeof( SuiteClasses ) );
					if ( annotation != null )
					{
						 foreach ( Type test in annotation.value() )
						 {
							  if ( !result.ContainsKey( test ) )
							  {
									try
									{
										 result[test] = new Parameterization( this, test.GetConstructor( type ) );
									}
									catch ( InitializationError failure )
									{
										 ( ( IList<Exception> )errors ).AddRange( failure.Causes );
									}
									catch ( NoSuchMethodException e )
									{
										 errors.Add( e );
									}
							  }
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object newSuiteInstance() throws Exception
			  internal virtual object NewSuiteInstance()
			  {
					return System.Activator.CreateInstance( SuiteClass );
			  }
		 }

		 private class Parameterization : BlockJUnit4ClassRunner
		 {
			  internal readonly ParameterBuilder Builder;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Constructor<?> constructor;
			  internal readonly System.Reflection.ConstructorInfo<object> Constructor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Parameterization(ParameterBuilder builder, Constructor<?> constructor) throws org.junit.runners.model.InitializationError
			  internal Parameterization<T1>( ParameterBuilder builder, System.Reflection.ConstructorInfo<T1> constructor ) : base( constructor.DeclaringClass )
			  {
					this.Builder = builder;
					this.Constructor = constructor;
			  }

			  protected internal override void ValidateConstructor( IList<Exception> errors )
			  {
					// constructor is already verified
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Object createTest() throws Exception
			  protected internal override object CreateTest()
			  {
					return Constructor.newInstance( Builder.newSuiteInstance() );
			  }
		 }
	}

}
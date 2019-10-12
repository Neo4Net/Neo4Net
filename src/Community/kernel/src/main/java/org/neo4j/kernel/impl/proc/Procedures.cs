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
namespace Neo4Net.Kernel.impl.proc
{

	using Neo4Net.Collection;
	using Neo4Net.Function;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureHandle = Neo4Net.@internal.Kernel.Api.procs.ProcedureHandle;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using UserAggregator = Neo4Net.@internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionHandle = Neo4Net.@internal.Kernel.Api.procs.UserFunctionHandle;
	using UserFunctionSignature = Neo4Net.@internal.Kernel.Api.procs.UserFunctionSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using SpecialBuiltInProcedures = Neo4Net.Kernel.builtinprocs.SpecialBuiltInProcedures;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;

	/// <summary>
	/// This is the coordinating service for procedures in the database. It loads procedures from a specified
	/// directory at startup, but also allows programmatic registration of them - and then, of course, allows
	/// invoking procedures.
	/// </summary>
	public class Procedures : LifecycleAdapter
	{
		 private readonly ProcedureRegistry _registry = new ProcedureRegistry();
		 private readonly TypeMappers _typeMappers;
		 private readonly ComponentRegistry _safeComponents = new ComponentRegistry();
		 private readonly ComponentRegistry _allComponents = new ComponentRegistry();
		 private readonly ReflectiveProcedureCompiler _compiler;
		 private readonly ThrowingConsumer<Procedures, ProcedureException> _builtin;
		 private readonly File _pluginDir;
		 private readonly Log _log;

		 /// <summary>
		 /// Used by testing.
		 /// </summary>
		 public Procedures() : this(null, new SpecialBuiltInProcedures("N/A", "N/A"), null, NullLog.Instance, ProcedureConfig.Default)
		 {
		 }

		 public Procedures( EmbeddedProxySPI proxySPI, ThrowingConsumer<Procedures, ProcedureException> builtin, File pluginDir, Log log, ProcedureConfig config )
		 {
			  this._builtin = builtin;
			  this._pluginDir = pluginDir;
			  this._log = log;
			  this._typeMappers = new TypeMappers( proxySPI );
			  this._compiler = new ReflectiveProcedureCompiler( _typeMappers, _safeComponents, _allComponents, log, config );
		 }

		 /// <summary>
		 /// Register a new procedure. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="proc"> the procedure. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableProcedure proc) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableProcedure proc )
		 {
			  Register( proc, false );
		 }

		 /// <summary>
		 /// Register a new function. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserFunction function )
		 {
			  Register( function, false );
		 }

		 /// <summary>
		 /// Register a new function. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserAggregationFunction function) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserAggregationFunction function )
		 {
			  Register( function, false );
		 }

		 /// <summary>
		 /// Register a new procedure. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserFunction function, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserFunction function, bool overrideCurrentImplementation )
		 {
			  _registry.register( function, overrideCurrentImplementation );
		 }

		 /// <summary>
		 /// Register a new procedure. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserAggregationFunction function, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserAggregationFunction function, bool overrideCurrentImplementation )
		 {
			  _registry.register( function, overrideCurrentImplementation );
		 }

		 /// <summary>
		 /// Register a new procedure. This method must not be called concurrently with <seealso cref="procedure(QualifiedName)"/>. </summary>
		 /// <param name="proc"> the procedure. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableProcedure proc, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableProcedure proc, bool overrideCurrentImplementation )
		 {
			  _registry.register( proc, overrideCurrentImplementation );
		 }

		 /// <summary>
		 /// Register a new internal procedure defined with annotations on a java class. </summary>
		 /// <param name="proc"> the procedure class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerProcedure(Class proc) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterProcedure( Type proc )
		 {
			  RegisterProcedure( proc, false );
		 }

		 /// <summary>
		 /// Register a new internal procedure defined with annotations on a java class. </summary>
		 /// <param name="proc"> the procedure class </param>
		 /// <param name="overrideCurrentImplementation"> set to true if procedures within this class should override older procedures with the same name </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerProcedure(Class proc, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterProcedure( Type proc, bool overrideCurrentImplementation )
		 {
			  RegisterProcedure( proc, overrideCurrentImplementation, null );
		 }

		 /// <summary>
		 /// Register a new internal procedure defined with annotations on a java class. </summary>
		 /// <param name="proc"> the procedure class </param>
		 /// <param name="overrideCurrentImplementation"> set to true if procedures within this class should override older procedures with the same name </param>
		 /// <param name="warning"> the warning the procedure should generate when called </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerProcedure(Class proc, boolean overrideCurrentImplementation, String warning) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterProcedure( Type proc, bool overrideCurrentImplementation, string warning )
		 {
			  foreach ( CallableProcedure procedure in _compiler.compileProcedure( proc, warning, true ) )
			  {
					Register( procedure, overrideCurrentImplementation );
			  }
		 }

		 /// <summary>
		 /// Register a new function defined with annotations on a java class. </summary>
		 /// <param name="func"> the function class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerBuiltInFunctions(Class func) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterBuiltInFunctions( Type func )
		 {
			  foreach ( CallableUserFunction function in _compiler.withoutNamingRestrictions().compileFunction(func) )
			  {
					Register( function, false );
			  }
		 }

		 /// <summary>
		 /// Register a new function defined with annotations on a java class. </summary>
		 /// <param name="func"> the function class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerFunction(Class func) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterFunction( Type func )
		 {
			  RegisterFunction( func, false );
		 }

		 /// <summary>
		 /// Register a new aggregation function defined with annotations on a java class. </summary>
		 /// <param name="func"> the function class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerAggregationFunction(Class func, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterAggregationFunction( Type func, bool overrideCurrentImplementation )
		 {
			  foreach ( CallableUserAggregationFunction function in _compiler.compileAggregationFunction( func ) )
			  {
					Register( function, overrideCurrentImplementation );
			  }
		 }

		 /// <summary>
		 /// Register a new aggregation function defined with annotations on a java class. </summary>
		 /// <param name="func"> the function class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerAggregationFunction(Class func) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterAggregationFunction( Type func )
		 {
			  RegisterAggregationFunction( func, false );
		 }

		 /// <summary>
		 /// Register a new function defined with annotations on a java class. </summary>
		 /// <param name="func"> the function class </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerFunction(Class func, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterFunction( Type func, bool overrideCurrentImplementation )
		 {
			  foreach ( CallableUserFunction function in _compiler.compileFunction( func ) )
			  {
					Register( function, overrideCurrentImplementation );
			  }
		 }

		 /// <summary>
		 /// Registers a type and its mapping to Neo4jTypes
		 /// </summary>
		 /// <param name="javaClass">
		 ///         the class of the native type </param>
		 /// <param name="type">
		 ///         the mapping to Neo4jTypes </param>
		 public virtual void RegisterType( Type javaClass, Neo4jTypes.AnyType type )
		 {
			  _typeMappers.registerType( javaClass, new TypeMappers.DefaultValueConverter( type, javaClass ) );
		 }

		 /// <summary>
		 /// Registers a component, these become available in reflective procedures for injection. </summary>
		 /// <param name="cls"> the type of component to be registered (this is what users 'ask' for in their field declaration) </param>
		 /// <param name="provider"> a function that supplies the component, given the context of a procedure invocation </param>
		 /// <param name="safe"> set to false if this component can bypass security, true if it respects security </param>
		 public virtual void RegisterComponent<T>( Type cls, ComponentRegistry.Provider<T> provider, bool safe )
		 {
				 cls = typeof( T );
			  if ( safe )
			  {
					_safeComponents.register( cls, provider );
			  }
			  _allComponents.register( cls, provider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.ProcedureHandle procedure(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual ProcedureHandle Procedure( QualifiedName name )
		 {
			  return _registry.procedure( name );
		 }

		 public virtual UserFunctionHandle Function( QualifiedName name )
		 {
			  return _registry.function( name );
		 }

		 public virtual UserFunctionHandle AggregationFunction( QualifiedName name )
		 {
			  return _registry.aggregationFunction( name );
		 }

		 public virtual ISet<ProcedureSignature> AllProcedures
		 {
			 get
			 {
				  return _registry.AllProcedures;
			 }
		 }

		 public virtual ISet<UserFunctionSignature> AllFunctions
		 {
			 get
			 {
				  return _registry.AllFunctions;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual RawIterator<object[], ProcedureException> CallProcedure( Context ctx, QualifiedName name, object[] input, ResourceTracker resourceTracker )
		 {
			  return _registry.callProcedure( ctx, name, input, resourceTracker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(org.neo4j.kernel.api.proc.Context ctx, int id, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual RawIterator<object[], ProcedureException> CallProcedure( Context ctx, int id, object[] input, ResourceTracker resourceTracker )
		 {
			  return _registry.callProcedure( ctx, id, input, resourceTracker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue callFunction(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual AnyValue CallFunction( Context ctx, QualifiedName name, AnyValue[] input )
		 {
			  return _registry.callFunction( ctx, name, input );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue callFunction(org.neo4j.kernel.api.proc.Context ctx, int id, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual AnyValue CallFunction( Context ctx, int id, AnyValue[] input )
		 {
			  return _registry.callFunction( ctx, id, input );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator createAggregationFunction(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual UserAggregator CreateAggregationFunction( Context ctx, QualifiedName name )
		 {
			  return _registry.createAggregationFunction( ctx, name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator createAggregationFunction(org.neo4j.kernel.api.proc.Context ctx, int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual UserAggregator CreateAggregationFunction( Context ctx, int id )
		 {
			  return _registry.createAggregationFunction( ctx, id );
		 }

		 public virtual ValueMapper<object> ValueMapper()
		 {
			  return _typeMappers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {

			  ProcedureJarLoader loader = new ProcedureJarLoader( _compiler, _log );
			  ProcedureJarLoader.Callables callables = loader.LoadProceduresFromDir( _pluginDir );
			  foreach ( CallableProcedure procedure in callables.Procedures() )
			  {
					Register( procedure );
			  }

			  foreach ( CallableUserFunction function in callables.Functions() )
			  {
					Register( function );
			  }

			  foreach ( CallableUserAggregationFunction function in callables.AggregationFunctions() )
			  {
					Register( function );
			  }

			  // And register built-in procedures
			  _builtin.accept( this );
		 }
	}

}
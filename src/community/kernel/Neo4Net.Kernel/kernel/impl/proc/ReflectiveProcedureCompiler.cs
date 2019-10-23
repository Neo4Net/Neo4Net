using System;
using System.Collections.Generic;
using System.Reflection;

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
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;


	using Neo4Net.Collections;
	using Resource = Neo4Net.GraphDb.Resource;
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using FieldSignature = Neo4Net.Kernel.Api.Internal.procs.FieldSignature;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.Kernel.Api.Internal.procs.QualifiedName;
	using UserAggregator = Neo4Net.Kernel.Api.Internal.procs.UserAggregator;
	using UserFunctionSignature = Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using IOUtils = Neo4Net.Io.IOUtils;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using ComponentInjectionException = Neo4Net.Kernel.Api.Exceptions.ComponentInjectionException;
	using ResourceCloseFailureException = Neo4Net.Kernel.Api.Exceptions.ResourceCloseFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using FailedLoadAggregatedFunction = Neo4Net.Kernel.api.proc.FailedLoadAggregatedFunction;
	using FailedLoadFunction = Neo4Net.Kernel.api.proc.FailedLoadFunction;
	using FailedLoadProcedure = Neo4Net.Kernel.api.proc.FailedLoadProcedure;
	using OutputMapper = Neo4Net.Kernel.impl.proc.OutputMappers.OutputMapper;
	using Log = Neo4Net.Logging.Log;
	using Admin = Neo4Net.Procedure.Admin;
	using Description = Neo4Net.Procedure.Description;
	using Mode = Neo4Net.Procedure.Mode;
	using PerformsWrites = Neo4Net.Procedure.PerformsWrites;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.procedure_unrestricted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asRawIterator;

	/// <summary>
	/// Handles converting a class into one or more callable <seealso cref="CallableProcedure"/>.
	/// </summary>
	internal class ReflectiveProcedureCompiler
	{
		 private readonly MethodHandles.Lookup _lookup = MethodHandles.lookup();
		 private readonly OutputMappers _outputMappers;
		 private readonly MethodSignatureCompiler _inputSignatureDeterminer;
		 private readonly FieldInjections _safeFieldInjections;
		 private readonly FieldInjections _allFieldInjections;
		 private readonly Log _log;
		 private readonly TypeMappers _typeMappers;
		 private readonly ProcedureConfig _config;
		 private readonly NamingRestrictions _restrictions;

		 internal ReflectiveProcedureCompiler( TypeMappers typeMappers, ComponentRegistry safeComponents, ComponentRegistry allComponents, Log log, ProcedureConfig config ) : this( new MethodSignatureCompiler( typeMappers ), new OutputMappers( typeMappers ), new FieldInjections( safeComponents ), new FieldInjections( allComponents ), log, typeMappers, config, ReflectiveProcedureCompiler.rejectEmptyNamespace )
		 {
		 }

		 private ReflectiveProcedureCompiler( MethodSignatureCompiler inputSignatureCompiler, OutputMappers outputMappers, FieldInjections safeFieldInjections, FieldInjections allFieldInjections, Log log, TypeMappers typeMappers, ProcedureConfig config, NamingRestrictions restrictions )
		 {
			  this._inputSignatureDeterminer = inputSignatureCompiler;
			  this._outputMappers = outputMappers;
			  this._safeFieldInjections = safeFieldInjections;
			  this._allFieldInjections = allFieldInjections;
			  this._log = log;
			  this._typeMappers = typeMappers;
			  this._config = config;
			  this._restrictions = restrictions;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.proc.CallableUserFunction> compileFunction(Class fcnDefinition) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal virtual IList<CallableUserFunction> CompileFunction( Type fcnDefinition )
		 {
			  try
			  {
					IList<System.Reflection.MethodInfo> functionMethods = java.util.fcnDefinition.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ).Where( m => m.isAnnotationPresent( typeof( UserFunction ) ) ).ToList();

					if ( functionMethods.Count == 0 )
					{
						 return emptyList();
					}

					MethodHandle constructor = constructor( fcnDefinition );

					List<CallableUserFunction> @out = new List<CallableUserFunction>( functionMethods.Count );
					foreach ( System.Reflection.MethodInfo method in functionMethods )
					{
						 string valueName = method.getAnnotation( typeof( UserFunction ) ).value();
						 string definedName = method.getAnnotation( typeof( UserFunction ) ).name();
						 QualifiedName funcName = ExtractName( fcnDefinition, method, valueName, definedName );
						 if ( _config.isWhitelisted( funcName.ToString() ) )
						 {
							  @out.Add( CompileFunction( fcnDefinition, constructor, method, funcName ) );
						 }
						 else
						 {
							  _log.warn( string.Format( "The function '{0}' is not on the whitelist and won't be loaded.", funcName.ToString() ) );
						 }
					}
					@out.sort( System.Collections.IComparer.comparing( a => a.signature().name().ToString() ) );
					return @out;
			  }
			  catch ( KernelException e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, e, "Failed to compile function defined in `%s`: %s", fcnDefinition.Name, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.proc.CallableUserAggregationFunction> compileAggregationFunction(Class fcnDefinition) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal virtual IList<CallableUserAggregationFunction> CompileAggregationFunction( Type fcnDefinition )
		 {
			  try
			  {
					IList<System.Reflection.MethodInfo> methods = java.util.fcnDefinition.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ).Where( m => m.isAnnotationPresent( typeof( UserAggregationFunction ) ) ).ToList();

					if ( methods.Count == 0 )
					{
						 return emptyList();
					}

					MethodHandle constructor = constructor( fcnDefinition );

					List<CallableUserAggregationFunction> @out = new List<CallableUserAggregationFunction>( methods.Count );
					foreach ( System.Reflection.MethodInfo method in methods )
					{
						 string valueName = method.getAnnotation( typeof( UserAggregationFunction ) ).value();
						 string definedName = method.getAnnotation( typeof( UserAggregationFunction ) ).name();
						 QualifiedName funcName = ExtractName( fcnDefinition, method, valueName, definedName );

						 if ( _config.isWhitelisted( funcName.ToString() ) )
						 {
							  @out.Add( CompileAggregationFunction( fcnDefinition, constructor, method, funcName ) );
						 }
						 else
						 {
							  _log.warn( string.Format( "The function '{0}' is not on the whitelist and won't be loaded.", funcName.ToString() ) );
						 }

					}
					@out.sort( System.Collections.IComparer.comparing( a => a.signature().name().ToString() ) );
					return @out;
			  }
			  catch ( KernelException e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, e, "Failed to compile function defined in `%s`: %s", fcnDefinition.Name, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.proc.CallableProcedure> compileProcedure(Class procDefinition, String warning, boolean fullAccess) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal virtual IList<CallableProcedure> CompileProcedure( Type procDefinition, string warning, bool fullAccess )
		 {
			  try
			  {
					IList<System.Reflection.MethodInfo> procedureMethods = java.util.procDefinition.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ).Where( m => m.isAnnotationPresent( typeof( Procedure ) ) ).ToList();

					if ( procedureMethods.Count == 0 )
					{
						 return emptyList();
					}

					MethodHandle constructor = constructor( procDefinition );

					List<CallableProcedure> @out = new List<CallableProcedure>( procedureMethods.Count );
					foreach ( System.Reflection.MethodInfo method in procedureMethods )
					{
						 string valueName = method.getAnnotation( typeof( Procedure ) ).value();
						 string definedName = method.getAnnotation( typeof( Procedure ) ).name();
						 QualifiedName procName = ExtractName( procDefinition, method, valueName, definedName );

						 if ( fullAccess || _config.isWhitelisted( procName.ToString() ) )
						 {
							  @out.Add( CompileProcedure( procDefinition, constructor, method, warning, fullAccess, procName ) );
						 }
						 else
						 {
							  _log.warn( string.Format( "The procedure '{0}' is not on the whitelist and won't be loaded.", procName.ToString() ) );
						 }
					}
					@out.sort( System.Collections.IComparer.comparing( a => a.signature().name().ToString() ) );
					return @out;
			  }
			  catch ( KernelException e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, e, "Failed to compile procedure defined in `%s`: %s", procDefinition.Name, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.proc.CallableProcedure compileProcedure(Class procDefinition, MethodHandle constructor, Method method, String warning, boolean fullAccess, org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName procName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 private CallableProcedure CompileProcedure( Type procDefinition, MethodHandle constructor, System.Reflection.MethodInfo method, string warning, bool fullAccess, QualifiedName procName )
		 {
			  IList<FieldSignature> inputSignature = _inputSignatureDeterminer.signatureFor( method );
			  OutputMapper outputMapper = _outputMappers.mapper( method );

			  string description = description( method );
			  Procedure procedure = method.getAnnotation( typeof( Procedure ) );
			  Mode mode = procedure.mode();
			  bool admin = method.isAnnotationPresent( typeof( Admin ) );
			  if ( method.isAnnotationPresent( typeof( PerformsWrites ) ) )
			  {
					if ( procedure.mode() != Mode.DEFAULT )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Conflicting procedure annotation, cannot use PerformsWrites and mode" );
					}
					else
					{
						 mode = Mode.WRITE;
					}
			  }

			  string deprecated = deprecated( method, procedure.deprecatedBy, "Use of @Procedure(deprecatedBy) without @Deprecated in " + procName );

			  IList<FieldInjections.FieldSetter> setters = _allFieldInjections.setters( procDefinition );
			  if ( !fullAccess && !_config.fullAccessFor( procName.ToString() ) )
			  {
					try
					{
						 setters = _safeFieldInjections.setters( procDefinition );
					}
					catch ( ComponentInjectionException )
					{
						 description = DescribeAndLogLoadFailure( procName );
						 ProcedureSignature signature = new ProcedureSignature( procName, inputSignature, outputMapper.Signature(), Mode.DEFAULT, admin, null, new string[0], description, warning, procedure.eager(), false );
						 return new FailedLoadProcedure( signature );
					}
			  }

			  ProcedureSignature signature = new ProcedureSignature( procName, inputSignature, outputMapper.Signature(), mode, admin, deprecated, _config.rolesFor(procName.ToString()), description, warning, procedure.eager(), false );
			  return new ReflectiveProcedure( signature, constructor, method, outputMapper, setters );
		 }

		 private string DescribeAndLogLoadFailure( QualifiedName name )
		 {
			  string nameStr = name.ToString();
			  string description = nameStr + " is unavailable because it is sandboxed and has dependencies outside of the sandbox. " +
						 "Sandboxing is controlled by the " + procedure_unrestricted.name() + " setting. " +
						 "Only unrestrict procedures you can trust with access to database internals.";
			  _log.warn( description );
			  return description;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.proc.CallableUserFunction compileFunction(Class procDefinition, MethodHandle constructor, Method method, org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName procName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException, IllegalAccessException
		 private CallableUserFunction CompileFunction( Type procDefinition, MethodHandle constructor, System.Reflection.MethodInfo method, QualifiedName procName )
		 {
			  _restrictions.verify( procName );

			  IList<FieldSignature> inputSignature = _inputSignatureDeterminer.signatureFor( method );
			  Type returnType = method.ReturnType;
			  TypeMappers.TypeChecker typeChecker = _typeMappers.checkerFor( returnType );
			  string description = description( method );
			  UserFunction function = method.getAnnotation( typeof( UserFunction ) );
			  string deprecated = deprecated( method, function.deprecatedBy, "Use of @UserFunction(deprecatedBy) without @Deprecated in " + procName );

			  IList<FieldInjections.FieldSetter> setters = _allFieldInjections.setters( procDefinition );
			  if ( !_config.fullAccessFor( procName.ToString() ) )
			  {
					try
					{
						 setters = _safeFieldInjections.setters( procDefinition );
					}
					catch ( ComponentInjectionException )
					{
						 description = DescribeAndLogLoadFailure( procName );
						 UserFunctionSignature signature = new UserFunctionSignature( procName, inputSignature, typeChecker.Type(), deprecated, _config.rolesFor(procName.ToString()), description, false );
						 return new FailedLoadFunction( signature );
					}
			  }

			  UserFunctionSignature signature = new UserFunctionSignature( procName, inputSignature, typeChecker.Type(), deprecated, _config.rolesFor(procName.ToString()), description, false );

			  return new ReflectiveUserFunction( signature, constructor, method, typeChecker, _typeMappers, setters );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.proc.CallableUserAggregationFunction compileAggregationFunction(Class definition, MethodHandle constructor, Method method, org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName funcName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException, IllegalAccessException
		 private CallableUserAggregationFunction CompileAggregationFunction( Type definition, MethodHandle constructor, System.Reflection.MethodInfo method, QualifiedName funcName )
		 {
			  _restrictions.verify( funcName );

			  //find update and result method
			  System.Reflection.MethodInfo update = null;
			  System.Reflection.MethodInfo result = null;
			  Type aggregator = method.ReturnType;
			  foreach ( System.Reflection.MethodInfo m in aggregator.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
			  {
					if ( m.isAnnotationPresent( typeof( UserAggregationUpdate ) ) )
					{
						 if ( update != null )
						 {
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Class '%s' contains multiple methods annotated with '@%s'.", aggregator.Name, typeof( UserAggregationUpdate ).Name );
						 }
						 update = m;

					}
					if ( m.isAnnotationPresent( typeof( UserAggregationResult ) ) )
					{
						 if ( result != null )
						 {
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Class '%s' contains multiple methods annotated with '@%s'.", aggregator.Name, typeof( UserAggregationResult ).Name );
						 }
						 result = m;
					}
			  }
			  if ( result == null || update == null )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Class '%s' must contain methods annotated with both '@%s' as well as '@%s'.", aggregator.Name, typeof( UserAggregationResult ).Name, typeof( UserAggregationUpdate ).Name );
			  }
			  if ( update.ReturnType != typeof( void ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Update method '%s' in %s has type '%s' but must have return type 'void'.", update.Name, aggregator.Name, update.ReturnType.SimpleName );

			  }
			  if ( !Modifier.isPublic( method.Modifiers ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Aggregation method '%s' in %s must be public.", method.Name, definition.Name );
			  }
			  if ( !Modifier.isPublic( aggregator.Modifiers ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Aggregation class '%s' must be public.", aggregator.Name );
			  }
			  if ( !Modifier.isPublic( update.Modifiers ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Aggregation update method '%s' in %s must be public.", update.Name, aggregator.Name );
			  }
			  if ( !Modifier.isPublic( result.Modifiers ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Aggregation result method '%s' in %s must be public.", result.Name, aggregator.Name );
			  }

			  IList<FieldSignature> inputSignature = _inputSignatureDeterminer.signatureFor( update );
			  Type returnType = result.ReturnType;
			  TypeMappers.TypeChecker valueConverter = _typeMappers.checkerFor( returnType );
			  MethodHandle creator = _lookup.unreflect( method );
			  MethodHandle resultMethod = _lookup.unreflect( result );

			  string description = description( method );
			  UserAggregationFunction function = method.getAnnotation( typeof( UserAggregationFunction ) );

			  string deprecated = deprecated( method, function.deprecatedBy, "Use of @UserAggregationFunction(deprecatedBy) without @Deprecated in " + funcName );

			  IList<FieldInjections.FieldSetter> setters = _allFieldInjections.setters( definition );
			  if ( !_config.fullAccessFor( funcName.ToString() ) )
			  {
					try
					{
						 setters = _safeFieldInjections.setters( definition );
					}
					catch ( ComponentInjectionException )
					{
						 description = DescribeAndLogLoadFailure( funcName );
						 UserFunctionSignature signature = new UserFunctionSignature( funcName, inputSignature, valueConverter.Type(), deprecated, _config.rolesFor(funcName.ToString()), description, false );

						 return new FailedLoadAggregatedFunction( signature );
					}
			  }

			  UserFunctionSignature signature = new UserFunctionSignature( funcName, inputSignature, valueConverter.Type(), deprecated, _config.rolesFor(funcName.ToString()), description, false );

			  return new ReflectiveUserAggregationFunction( signature, constructor, creator, update, resultMethod, valueConverter, setters );
		 }

		 private string Deprecated( System.Reflection.MethodInfo method, System.Func<string> supplier, string warning )
		 {
			  string deprecatedBy = supplier();
			  string deprecated = null;
			  if ( method.isAnnotationPresent( typeof( Deprecated ) ) )
			  {
					deprecated = deprecatedBy;
			  }
			  else if ( deprecatedBy.Length > 0 )
			  {
					_log.warn( warning );
					deprecated = deprecatedBy;
			  }

			  return deprecated;
		 }

		 private string Description( System.Reflection.MethodInfo method )
		 {
			  if ( method.isAnnotationPresent( typeof( Description ) ) )
			  {
				  return method.getAnnotation( typeof( Description ) ).value();
			  }
			  else
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private MethodHandle constructor(Class procDefinition) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 private MethodHandle Constructor( Type procDefinition )
		 {
			  try
			  {
					return _lookup.unreflectConstructor( procDefinition.GetConstructor() );
			  }
			  catch ( Exception e ) when ( e is IllegalAccessException || e is NoSuchMethodException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, e, "Unable to find a usable public no-argument constructor in the class `%s`. " + "Please add a valid, public constructor, recompile the class and try again.", procDefinition.Name );
			  }
		 }

		 private QualifiedName ExtractName( Type procDefinition, System.Reflection.MethodInfo m, string valueName, string definedName )
		 {
			  string procName = definedName.Trim().Length == 0 ? valueName : definedName;
			  if ( procName.Trim().Length > 0 )
			  {
					string[] split = procName.Split( "\\.", true );
					if ( split.Length == 1 )
					{
						 return new QualifiedName( new string[0], split[0] );
					}
					else
					{
						 int lastElement = split.Length - 1;
						 return new QualifiedName( Arrays.copyOf( split, lastElement ), split[lastElement] );
					}
			  }
			  Package pkg = procDefinition.Assembly;
			  // Package is null if class is in root package
			  string[] @namespace = pkg == null ? new string[0] : pkg.Name.Split( "\\." );
			  string name = m.Name;
			  return new QualifiedName( @namespace, name );
		 }

		 public virtual ReflectiveProcedureCompiler WithoutNamingRestrictions()
		 {
			  return new ReflectiveProcedureCompiler(_inputSignatureDeterminer, _outputMappers, _safeFieldInjections, _allFieldInjections, _log, _typeMappers, _config, name =>
			  {
						  // all ok
			  });
		 }

		 private abstract class ReflectiveBase
		 {

			  internal readonly IList<FieldInjections.FieldSetter> FieldSetters;
			  internal readonly ValueMapper<object> Mapper;

			  internal ReflectiveBase( ValueMapper<object> mapper, IList<FieldInjections.FieldSetter> fieldSetters )
			  {
					this.Mapper = mapper;
					this.FieldSetters = fieldSetters;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void inject(org.Neo4Net.kernel.api.proc.Context ctx, Object object) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  protected internal virtual void Inject( Context ctx, object @object )
			  {
					foreach ( FieldInjections.FieldSetter setter in FieldSetters )
					{
						 setter.Apply( ctx, @object );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Object[] mapToObjects(String type, org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, java.util.List<org.Neo4Net.Kernel.Api.Internal.procs.FieldSignature> inputSignature, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  protected internal virtual object[] MapToObjects( string type, QualifiedName name, IList<FieldSignature> inputSignature, AnyValue[] input )
			  {
					// Verify that the number of passed arguments matches the number expected in the mthod signature
					if ( inputSignature.Count != input.Length )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "%s `%s` takes %d arguments but %d was provided.", type, name, inputSignature.Count, input.Length );
					}

					object[] args = new object[input.Length];
					for ( int i = 0; i < input.Length; i++ )
					{
						 args[i] = inputSignature[i].Map( input[i], Mapper );
					}
					return args;
			  }
		 }

		 private class ReflectiveProcedure : ReflectiveBase, CallableProcedure
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ProcedureSignature SignatureConflict;
			  internal readonly OutputMapper OutputMapper;
			  internal readonly MethodHandle Constructor;
			  internal readonly System.Reflection.MethodInfo ProcedureMethod;
			  internal readonly int[] IndexesToMap;

			  internal ReflectiveProcedure( ProcedureSignature signature, MethodHandle constructor, System.Reflection.MethodInfo procedureMethod, OutputMapper outputMapper, IList<FieldInjections.FieldSetter> fieldSetters ) : base( null, fieldSetters )
			  {
					this.Constructor = constructor;
					this.ProcedureMethod = procedureMethod;
					this.SignatureConflict = signature;
					this.OutputMapper = outputMapper;
					this.IndexesToMap = ComputeIndexesToMap( signature.InputSignature() );
			  }

			  public override ProcedureSignature Signature()
			  {
					return SignatureConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> apply(org.Neo4Net.kernel.api.proc.Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			  {
					// For now, create a new instance of the class for each invocation. In the future, we'd like to keep
					// instances local to
					// at least the executing session, but we don't yet have good interfaces to the kernel to model that with.
					try
					{
						 IList<FieldSignature> inputSignature = SignatureConflict.inputSignature();
						 if ( inputSignature.Count != input.Length )
						 {
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Procedure `%s` takes %d arguments but %d was provided.", SignatureConflict.name(), inputSignature.Count, input.Length );
						 }
						 // Some input fields are not supported by Cypher and need to be mapped
						 foreach ( int indexToMap in IndexesToMap )
						 {
							  input[indexToMap] = inputSignature[indexToMap].Map( input[indexToMap] );
						 }

						 object cls = Constructor.invoke();
						 //API injection
						 Inject( ctx, cls );

						 // Admin check
						 if ( SignatureConflict.admin() )
						 {
							  SecurityContext securityContext = ctx.Get( Neo4Net.Kernel.api.proc.Context_Fields.SecurityContext );
							  securityContext.AssertCredentialsNotExpired();
							  if ( !securityContext.Admin )
							  {
									throw new AuthorizationViolationException( PERMISSION_DENIED );
							  }
						 }

						 // Call the method
						 object rs = ProcedureMethod.invoke( cls, input );

						 // This also handles VOID
						 if ( rs == null )
						 {
							  return asRawIterator( emptyIterator() );
						 }
						 else
						 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new MappingIterator(((java.util.stream.Stream<?>) rs).iterator(), ((java.util.stream.Stream<?>) rs)::close, resourceTracker);
							  return new MappingIterator( this, ( ( Stream<object> ) rs ).GetEnumerator(), ((Stream<object>) rs).close, resourceTracker );
						 }
					}
					catch ( Exception throwable )
					{
						 throw NewProcedureException( throwable );
					}
			  }

			  private class MappingIterator : RawIterator<object[], ProcedureException>, Resource
			  {
				  private readonly ReflectiveProcedureCompiler.ReflectiveProcedure _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Iterator<?> out;
					internal readonly IEnumerator<object> Out;
					internal Resource CloseableResource;
					internal ResourceTracker ResourceTracker;

					internal MappingIterator<T1>( ReflectiveProcedureCompiler.ReflectiveProcedure outerInstance, IEnumerator<T1> @out, Resource closeableResource, ResourceTracker resourceTracker )
					{
						this._outerInstance = outerInstance;
						 this.Out = @out;
						 this.CloseableResource = closeableResource;
						 this.ResourceTracker = resourceTracker;
						 resourceTracker.RegisterCloseableResource( closeableResource );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasNext() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
					public override bool HasNext()
					{
						 try
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  bool hasNext = Out.hasNext();
							  if ( !hasNext )
							  {
									Close();
							  }
							  return hasNext;
						 }
						 catch ( Exception throwable )
						 {
							  throw CloseAndCreateProcedureException( throwable );
						 }
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object[] next() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
					public override object[] Next()
					{
						 try
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  object record = Out.next();
							  return outerInstance.OutputMapper.apply( record );
						 }
						 catch ( Exception throwable )
						 {
							  throw CloseAndCreateProcedureException( throwable );
						 }
					}

					public override void Close()
					{
						 if ( CloseableResource != null )
						 {
							  // Make sure we reset closeableResource before doing anything which may throw an exception that may
							  // result in a recursive call to this close-method
							  Resource resourceToClose = CloseableResource;
							  CloseableResource = null;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
							  IOUtils.close( ResourceCloseFailureException::new, () => ResourceTracker.unregisterCloseableResource(resourceToClose), resourceToClose );
						 }
					}

					internal virtual ProcedureException CloseAndCreateProcedureException( Exception t )
					{
						 ProcedureException procedureException = outerInstance.NewProcedureException( t );

						 try
						 {
							  Close();
						 }
						 catch ( Exception exceptionDuringClose )
						 {
							  try
							  {
									procedureException.addSuppressed( exceptionDuringClose );
							  }
							  catch ( Exception )
							  {
							  }
						 }
						 return procedureException;
					}
			  }

			  internal virtual ProcedureException NewProcedureException( Exception throwable )
			  {
					// Unwrap the wrapped exception we get from invocation by reflection
					if ( throwable is InvocationTargetException )
					{
						 throwable = throwable.InnerException;
					}

					if ( throwable is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
					{
						 return new ProcedureException( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) throwable ).Status(), throwable, throwable.Message );
					}
					else
					{
						 Exception cause = ExceptionUtils.getRootCause( throwable );
						 return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Failed to invoke procedure `%s`: %s", SignatureConflict.name(), "Caused by: " + (cause != null ? cause : throwable) );
					}
			  }
		 }

		 private class ReflectiveUserFunction : ReflectiveBase, CallableUserFunction
		 {
			  internal readonly TypeMappers.TypeChecker TypeChecker;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly UserFunctionSignature SignatureConflict;
			  internal readonly MethodHandle Constructor;
			  internal readonly System.Reflection.MethodInfo UdfMethod;
			  internal readonly int[] IndexesToMap;

			  internal ReflectiveUserFunction( UserFunctionSignature signature, MethodHandle constructor, System.Reflection.MethodInfo udfMethod, TypeMappers.TypeChecker typeChecker, ValueMapper<object> mapper, IList<FieldInjections.FieldSetter> fieldSetters ) : base( mapper, fieldSetters )
			  {
					this.Constructor = constructor;
					this.UdfMethod = udfMethod;
					this.SignatureConflict = signature;
					this.TypeChecker = typeChecker;
					IndexesToMap = ComputeIndexesToMap( signature.InputSignature() );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return SignatureConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue apply(org.Neo4Net.kernel.api.proc.Context ctx, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  public override AnyValue Apply( Context ctx, AnyValue[] input )
			  {
					// For now, create a new instance of the class for each invocation. In the future, we'd like to keep
					// instances local to
					// at least the executing session, but we don't yet have good interfaces to the kernel to model that with.
					try
					{
						 object cls = Constructor.invoke();
						 //API injection
						 Inject( ctx, cls );

						 // Call the method
						 object rs = UdfMethod.invoke( cls, MapToObjects( "Function", SignatureConflict.name(), SignatureConflict.inputSignature(), input ) );

						 return TypeChecker.toValue( rs );
					}
					catch ( Exception throwable )
					{
						 if ( throwable is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
						 {
							  throw new ProcedureException( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) throwable ).Status(), throwable, throwable.Message, throwable );
						 }
						 else
						 {
							  Exception cause = ExceptionUtils.getRootCause( throwable );
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Failed to invoke function `%s`: %s", SignatureConflict.name(), "Caused by: " + (cause != null ? cause : throwable) );
						 }
					}
			  }
		 }

		 private class ReflectiveUserAggregationFunction : ReflectiveBase, CallableUserAggregationFunction
		 {

			  internal readonly TypeMappers.TypeChecker TypeChecker;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly UserFunctionSignature SignatureConflict;
			  internal readonly MethodHandle Constructor;
			  internal readonly MethodHandle Creator;
			  internal readonly System.Reflection.MethodInfo UpdateMethod;
			  internal readonly MethodHandle ResultMethod;
			  internal readonly int[] IndexesToMap;

			  internal ReflectiveUserAggregationFunction( UserFunctionSignature signature, MethodHandle constructor, MethodHandle creator, System.Reflection.MethodInfo updateMethod, MethodHandle resultMethod, TypeMappers.TypeChecker typeChecker, IList<FieldInjections.FieldSetter> fieldSetters ) : base( null, fieldSetters )
			  {
					this.Constructor = constructor;
					this.Creator = creator;
					this.UpdateMethod = updateMethod;
					this.ResultMethod = resultMethod;
					this.SignatureConflict = signature;
					this.TypeChecker = typeChecker;
					this.IndexesToMap = ComputeIndexesToMap( signature.InputSignature() );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return SignatureConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.UserAggregator create(org.Neo4Net.kernel.api.proc.Context ctx) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  public override UserAggregator Create( Context ctx )
			  {
					// For now, create a new instance of the class for each invocation. In the future, we'd like to keep
					// instances local to
					// at least the executing session, but we don't yet have good interfaces to the kernel to model that with.
					try
					{

						 object cls = Constructor.invoke();
						 //API injection
						 Inject( ctx, cls );
						 object aggregator = Creator.invoke( cls );
						 IList<FieldSignature> inputSignature = SignatureConflict.inputSignature();
						 int expectedNumberOfInputs = inputSignature.Count;

						 return new UserAggregatorAnonymousInnerClass( this, aggregator, inputSignature, expectedNumberOfInputs );

					}
					catch ( Exception throwable )
					{
						 if ( throwable is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
						 {
							  throw new ProcedureException( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) throwable ).Status(), throwable, throwable.Message );
						 }
						 else
						 {
							  Exception cause = ExceptionUtils.getRootCause( throwable );
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Failed to invoke function `%s`: %s", SignatureConflict.name(), "Caused by: " + (cause != null ? cause : throwable) );
						 }
					}
			  }

			  private class UserAggregatorAnonymousInnerClass : UserAggregator
			  {
				  private readonly ReflectiveUserAggregationFunction _outerInstance;

				  private object _aggregator;
				  private IList<FieldSignature> _inputSignature;
				  private int _expectedNumberOfInputs;

				  public UserAggregatorAnonymousInnerClass( ReflectiveUserAggregationFunction outerInstance, object aggregator, IList<FieldSignature> inputSignature, int expectedNumberOfInputs )
				  {
					  this.outerInstance = outerInstance;
					  this._aggregator = aggregator;
					  this._inputSignature = inputSignature;
					  this._expectedNumberOfInputs = expectedNumberOfInputs;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void update(Object[] input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
				  public void update( object[] input )
				  {
						try
						{
							 if ( _expectedNumberOfInputs != input.Length )
							 {
								  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Function `%s` takes %d arguments but %d was provided.", _outerInstance.signature.name(), _expectedNumberOfInputs, input.Length );
							 }
							 // Some input fields are not supported by Cypher and need to be mapped
							 foreach ( int indexToMap in _outerInstance.indexesToMap )
							 {
								  input[indexToMap] = _inputSignature[indexToMap].map( input[indexToMap] );
							 }

							 // Call the method
							 _outerInstance.updateMethod.invoke( _aggregator, input );
						}
						catch ( Exception throwable )
						{
							 if ( throwable is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
							 {
								  throw new ProcedureException( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) throwable ).Status(), throwable, throwable.Message );
							 }
							 else
							 {
								  Exception cause = ExceptionUtils.getRootCause( throwable );
								  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Failed to invoke function `%s`: %s", _outerInstance.signature.name(), "Caused by: " + (cause != null ? cause : throwable) );
							 }
						}
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object result() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
				  public object result()
				  {
						try
						{
							 return _outerInstance.typeChecker.typeCheck( _outerInstance.resultMethod.invoke( _aggregator ) );
						}
						catch ( Exception throwable )
						{
							 if ( throwable is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
							 {
								  throw new ProcedureException( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) throwable ).Status(), throwable, throwable.Message );
							 }
							 else
							 {
								  Exception cause = ExceptionUtils.getRootCause( throwable );
								  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Failed to invoke function `%s`: %s", _outerInstance.signature.name(), "Caused by: " + (cause != null ? cause : throwable) );
							 }
						}

				  }

			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void rejectEmptyNamespace(org.Neo4Net.Kernel.Api.Internal.procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 private static void RejectEmptyNamespace( QualifiedName name )
		 {
			  if ( name.Namespace() == null || name.Namespace().Length == 0 )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "It is not allowed to define functions in the root namespace please use a namespace, " + "e.g. `@UserFunction(\"org.example.com.%s\")", name.Name() );
			  }
		 }

		 private static int[] ComputeIndexesToMap( IList<FieldSignature> inputSignature )
		 {
			  List<int> integers = new List<int>();
			  for ( int i = 0; i < inputSignature.Count; i++ )
			  {
					if ( inputSignature[i].NeedsMapping() )
					{
						 integers.Add( i );
					}
			  }
			  return integers.Select( i => i ).ToArray();
		 }
	}

}
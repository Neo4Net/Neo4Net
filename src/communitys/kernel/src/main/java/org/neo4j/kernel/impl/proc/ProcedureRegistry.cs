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

	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Neo4Net.@internal.Kernel.Api.procs.FieldSignature;
	using ProcedureHandle = Neo4Net.@internal.Kernel.Api.procs.ProcedureHandle;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using UserAggregator = Neo4Net.@internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionHandle = Neo4Net.@internal.Kernel.Api.procs.UserFunctionHandle;
	using UserFunctionSignature = Neo4Net.@internal.Kernel.Api.procs.UserFunctionSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using AnyValue = Neo4Net.Values.AnyValue;

	public class ProcedureRegistry
	{

		 private readonly ProcedureHolder<CallableProcedure> _procedures = new ProcedureHolder<CallableProcedure>();
		 private readonly ProcedureHolder<CallableUserFunction> _functions = new ProcedureHolder<CallableUserFunction>();
		 private readonly ProcedureHolder<CallableUserAggregationFunction> _aggregationFunctions = new ProcedureHolder<CallableUserAggregationFunction>();

		 /// <summary>
		 /// Register a new procedure.
		 /// </summary>
		 /// <param name="proc"> the procedure. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableProcedure proc, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableProcedure proc, bool overrideCurrentImplementation )
		 {
			  ProcedureSignature signature = proc.Signature();
			  QualifiedName name = signature.Name();

			  string descriptiveName = signature.ToString();
			  ValidateSignature( descriptiveName, signature.InputSignature(), "input" );
			  ValidateSignature( descriptiveName, signature.OutputSignature(), "output" );

			  if ( !signature.Void && signature.OutputSignature().Count == 0 )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Procedures with zero output fields must be declared as VOID" );
			  }

			  CallableProcedure oldImplementation = _procedures.get( name );
			  if ( oldImplementation == null )
			  {
					_procedures.put( name, proc, signature.CaseInsensitive() );
			  }
			  else
			  {
					if ( overrideCurrentImplementation )
					{
						 _procedures.put( name, proc, signature.CaseInsensitive() );
					}
					else
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Unable to register procedure, because the name `%s` is already in use.", name );
					}
			  }
		 }

		 /// <summary>
		 /// Register a new function.
		 /// </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserFunction function, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserFunction function, bool overrideCurrentImplementation )
		 {
			  UserFunctionSignature signature = function.Signature();
			  QualifiedName name = signature.Name();

			  CallableUserFunction oldImplementation = _functions.get( name );
			  if ( oldImplementation == null )
			  {
					_functions.put( name, function, signature.CaseInsensitive() );
			  }
			  else
			  {
					if ( overrideCurrentImplementation )
					{
						 _functions.put( name, function, signature.CaseInsensitive() );
					}
					else
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Unable to register function, because the name `%s` is already in use.", name );
					}
			  }
		 }

		 /// <summary>
		 /// Register a new function.
		 /// </summary>
		 /// <param name="function"> the function. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void register(org.neo4j.kernel.api.proc.CallableUserAggregationFunction function, boolean overrideCurrentImplementation) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void Register( CallableUserAggregationFunction function, bool overrideCurrentImplementation )
		 {
			  UserFunctionSignature signature = function.Signature();
			  QualifiedName name = signature.Name();

			  CallableUserFunction oldImplementation = _functions.get( name );
			  if ( oldImplementation == null )
			  {
					_aggregationFunctions.put( name, function, signature.CaseInsensitive() );
			  }
			  else
			  {
					if ( overrideCurrentImplementation )
					{
						 _aggregationFunctions.put( name, function, signature.CaseInsensitive() );
					}
					else
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Unable to register aggregation function, because the name `%s` is already in use.", name );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateSignature(String descriptiveName, java.util.List<org.neo4j.internal.kernel.api.procs.FieldSignature> fields, String fieldType) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private void ValidateSignature( string descriptiveName, IList<FieldSignature> fields, string fieldType )
		 {
			  ISet<string> names = new HashSet<string>();
			  foreach ( FieldSignature field in fields )
			  {
					if ( !names.Add( field.Name() ) )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Procedure `%s` cannot be registered, because it contains a duplicated " + fieldType + " field, '%s'. " + "You need to rename or remove one of the duplicate fields.", descriptiveName, field.Name() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.ProcedureHandle procedure(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual ProcedureHandle Procedure( QualifiedName name )
		 {
			  CallableProcedure proc = _procedures.get( name );
			  if ( proc == null )
			  {
					throw NoSuchProcedure( name );
			  }
			  return new ProcedureHandle( proc.Signature(), _procedures.idOf(name) );
		 }

		 public virtual UserFunctionHandle Function( QualifiedName name )
		 {
			  CallableUserFunction func = _functions.get( name );
			  if ( func == null )
			  {
					return null;
			  }
			  return new UserFunctionHandle( func.Signature(), _functions.idOf(name) );
		 }

		 public virtual UserFunctionHandle AggregationFunction( QualifiedName name )
		 {
			  CallableUserAggregationFunction func = _aggregationFunctions.get( name );
			  if ( func == null )
			  {
					return null;
			  }
			  return new UserFunctionHandle( func.Signature(), _aggregationFunctions.idOf(name) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual RawIterator<object[], ProcedureException> CallProcedure( Context ctx, QualifiedName name, object[] input, ResourceTracker resourceTracker )
		 {
			  CallableProcedure proc = _procedures.get( name );
			  if ( proc == null )
			  {
					throw NoSuchProcedure( name );
			  }
			  return proc.Apply( ctx, input, resourceTracker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> callProcedure(org.neo4j.kernel.api.proc.Context ctx, int id, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual RawIterator<object[], ProcedureException> CallProcedure( Context ctx, int id, object[] input, ResourceTracker resourceTracker )
		 {
			  CallableProcedure proc;
			  try
			  {
					proc = _procedures.get( id );
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
					throw NoSuchProcedure( id );
			  }
			  return proc.Apply( ctx, input, resourceTracker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue callFunction(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual AnyValue CallFunction( Context ctx, QualifiedName name, AnyValue[] input )
		 {
			  CallableUserFunction func = _functions.get( name );
			  if ( func == null )
			  {
					throw NoSuchFunction( name );
			  }
			  return func.Apply( ctx, input );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue callFunction(org.neo4j.kernel.api.proc.Context ctx, int functionId, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual AnyValue CallFunction( Context ctx, int functionId, AnyValue[] input )
		 {
			  CallableUserFunction func;
			  try
			  {
					func = _functions.get( functionId );
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
					throw NoSuchFunction( functionId );
			  }
			  return func.Apply( ctx, input );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator createAggregationFunction(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual UserAggregator CreateAggregationFunction( Context ctx, QualifiedName name )
		 {
			  CallableUserAggregationFunction func = _aggregationFunctions.get( name );
			  if ( func == null )
			  {
					throw NoSuchFunction( name );
			  }
			  return func.Create( ctx );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.procs.UserAggregator createAggregationFunction(org.neo4j.kernel.api.proc.Context ctx, int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual UserAggregator CreateAggregationFunction( Context ctx, int id )
		 {
			  CallableUserAggregationFunction func = null;
			  try
			  {
					func = _aggregationFunctions.get( id );
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
				  throw NoSuchFunction( id );
			  }
			  return func.Create( ctx );
		 }

		 private ProcedureException NoSuchProcedure( QualifiedName name )
		 {
			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureNotFound, "There is no procedure with the name `%s` registered for this database instance. " + "Please ensure you've spelled the procedure name correctly and that the " + "procedure is properly deployed.", name );
		 }

		 private ProcedureException NoSuchProcedure( int id )
		 {
			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureNotFound, "There is no procedure with the internal id `%d` registered for this database instance.", id );
		 }

		 private ProcedureException NoSuchFunction( QualifiedName name )
		 {
			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureNotFound, "There is no function with the name `%s` registered for this database instance. " + "Please ensure you've spelled the function name correctly and that the " + "function is properly deployed.", name );
		 }

		 private ProcedureException NoSuchFunction( int id )
		 {
			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureNotFound, "There is no function with the internal id `%d` registered for this database instance.", id );
		 }

		 public virtual ISet<ProcedureSignature> AllProcedures
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				  return _procedures.all().Select(CallableProcedure::signature).collect(Collectors.toSet());
			 }
		 }

		 public virtual ISet<UserFunctionSignature> AllFunctions
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return Stream.concat( _functions.all().Select(CallableUserFunction::signature), _aggregationFunctions.all().Select(CallableUserAggregationFunction::signature) ).collect(Collectors.toSet());
			 }
		 }
	}

}
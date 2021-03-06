﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@internal.Kernel.Api
{

	using Org.Neo4j.Collection;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureHandle = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureHandle;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using UserAggregator = Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionHandle = Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionHandle;
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Org.Neo4j.Values;

	public interface Procedures
	{
		 /// <summary>
		 /// Get a handle to the given function </summary>
		 /// <param name="name"> the name of the function </param>
		 /// <returns> A handle to the function </returns>
		 UserFunctionHandle FunctionGet( QualifiedName name );

		 /// <summary>
		 /// Get a handle to the given aggregation function </summary>
		 /// <param name="name"> the name of the function </param>
		 /// <returns> A handle to the function </returns>
		 UserFunctionHandle AggregationFunctionGet( QualifiedName name );

		 /// <summary>
		 /// Fetch a procedure handle </summary>
		 /// <param name="name"> the name of the procedure </param>
		 /// <returns> a procedure handle </returns>
		 /// <exception cref="ProcedureException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.ProcedureHandle procedureGet(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 ProcedureHandle ProcedureGet( QualifiedName name );

		 /// <summary>
		 /// Fetch all procedures </summary>
		 /// <returns> all procedures </returns>
		 /// <exception cref="ProcedureException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<org.neo4j.internal.kernel.api.procs.ProcedureSignature> proceduresGetAll() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 ISet<ProcedureSignature> ProceduresGetAll();

		 /// <summary>
		 /// Invoke a read-only procedure by id. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallRead(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallRead( int id, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read-only procedure by id, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the procedure execution. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallReadOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallReadOverride( int id, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read/write procedure by id. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWrite(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallWrite( int id, object[] arguments, ProcedureCallContext context );
		 /// <summary>
		 /// Invoke a read/write procedure by id, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.WRITE WRITE"/> for the duration of the procedure execution. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWriteOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( int id, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a schema write procedure by id. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchema(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallSchema( int id, object[] arguments, ProcedureCallContext context );
		 /// <summary>
		 /// Invoke a schema write procedure by id, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.FULL FULL"/> for the duration of the procedure execution. </summary>
		 /// <param name="id"> the id of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchemaOverride(int id, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( int id, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read-only procedure by name. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallRead(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallRead( QualifiedName name, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read-only procedure by name, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the procedure execution. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallReadOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallReadOverride( QualifiedName name, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read/write procedure by name. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWrite(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallWrite( QualifiedName name, object[] arguments, ProcedureCallContext context );
		 /// <summary>
		 /// Invoke a read/write procedure by name, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.WRITE WRITE"/> for the duration of the procedure execution. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallWriteOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallWriteOverride( QualifiedName name, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a schema write procedure by name. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchema(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallSchema( QualifiedName name, object[] arguments, ProcedureCallContext context );
		 /// <summary>
		 /// Invoke a schema write procedure by name, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.FULL FULL"/> for the duration of the procedure execution. </summary>
		 /// <param name="name"> the name of the procedure. </param>
		 /// <param name="arguments"> the procedure arguments. </param>
		 /// <param name="context"> the procedure call context. </param>
		 /// <returns> an iterator containing the procedure results. </returns>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallSchemaOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] arguments, org.neo4j.internal.kernel.api.procs.ProcedureCallContext context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride( QualifiedName name, object[] arguments, ProcedureCallContext context );

		 /// <summary>
		 /// Invoke a read-only function by id </summary>
		 /// <param name="id"> the id of the function. </param>
		 /// <param name="arguments"> the function arguments. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.AnyValue functionCall(int id, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 AnyValue FunctionCall( int id, AnyValue[] arguments );

		 /// <summary>
		 /// Invoke a read-only function by name </summary>
		 /// <param name="name"> the name of the function. </param>
		 /// <param name="arguments"> the function arguments. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.AnyValue functionCall(org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 AnyValue FunctionCall( QualifiedName name, AnyValue[] arguments );

		 /// <summary>
		 /// Invoke a read-only function by id, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
		 /// <param name="id"> the id of the function. </param>
		 /// <param name="arguments"> the function arguments. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.AnyValue functionCallOverride(int id, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 AnyValue FunctionCallOverride( int id, AnyValue[] arguments );

		 /// <summary>
		 /// Invoke a read-only function by name, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
		 /// <param name="name"> the name of the function. </param>
		 /// <param name="arguments"> the function arguments. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.AnyValue functionCallOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name, org.neo4j.values.AnyValue[] arguments) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 AnyValue FunctionCallOverride( QualifiedName name, AnyValue[] arguments );

		 /// <summary>
		 /// Create a read-only aggregation function by id </summary>
		 /// <param name="id"> the id of the function </param>
		 /// <returns> the aggregation function </returns>
		 /// <exception cref="ProcedureException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 UserAggregator AggregationFunction( int id );

		 /// <summary>
		 /// Create a read-only aggregation function by name </summary>
		 /// <param name="name"> the name of the function </param>
		 /// <returns> the aggregation function </returns>
		 /// <exception cref="ProcedureException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunction(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 UserAggregator AggregationFunction( QualifiedName name );

		 /// <summary>
		 /// Invoke a read-only aggregation function by id, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
		 /// <param name="id"> the id of the function. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunctionOverride(int id) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 UserAggregator AggregationFunctionOverride( int id );

		 /// <summary>
		 /// Invoke a read-only aggregation function by name, and set the transaction's access mode to
		 /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
		 /// <param name="name"> the name of the function. </param>
		 /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.UserAggregator aggregationFunctionOverride(org.neo4j.internal.kernel.api.procs.QualifiedName name) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 UserAggregator AggregationFunctionOverride( QualifiedName name );

		 /// <summary>
		 /// Retrieve a value mapper for mapping values to regular Java objects. </summary>
		 /// <returns> a value mapper that maps to Java objects. </returns>
		 ValueMapper<object> ValueMapper();
	}

}
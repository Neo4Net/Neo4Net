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

namespace Neo4Net.Kernel.Api.Internal
{
   using AccessMode = Neo4Net.Kernel.Api.Internal.security.AccessMode;
   using AnyValue = Neo4Net.Values.AnyValue;
   using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext;
   using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
   using ProcedureHandle = Neo4Net.Kernel.Api.Internal.Procs.ProcedureHandle;
   using ProcedureSignature = Neo4Net.Kernel.Api.Internal.Procs.ProcedureSignature;
   using QualifiedName = Neo4Net.Kernel.Api.Internal.Procs.QualifiedName;
   using IUserAggregator = Neo4Net.Kernel.Api.Internal.Procs.IUserAggregator;
   using UserFunctionHandle = Neo4Net.Kernel.Api.Internal.Procs.UserFunctionHandle;

   public interface IProcedures
   {
      /// <summary>
      /// Get a handle to the given function </summary>
      /// <param name="name"> the name of the function </param>
      /// <returns> A handle to the function </returns>
      UserFunctionHandle FunctionGet(QualifiedName name);

      /// <summary>
      /// Get a handle to the given aggregation function </summary>
      /// <param name="name"> the name of the function </param>
      /// <returns> A handle to the function </returns>
      UserFunctionHandle AggregationFunctionGet(QualifiedName name);

      /// <summary>
      /// Fetch a procedure handle </summary>
      /// <param name="name"> the name of the procedure </param>
      /// <returns> a procedure handle </returns>
      /// <exception cref="ProcedureException"> </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureHandle procedureGet(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      ProcedureHandle ProcedureGet(QualifiedName name);

      /// <summary>
      /// Fetch all procedures </summary>
      /// <returns> all procedures </returns>
      /// <exception cref="ProcedureException"> </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: java.util.Set<org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureSignature> proceduresGetAll() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      ISet<ProcedureSignature> ProceduresGetAll();

      /// <summary>
      /// Invoke a read-only procedure by id. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallRead(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallRead(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read-only procedure by id, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the procedure execution. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallReadOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallReadOverride(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read/write procedure by id. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWrite(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallWrite(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read/write procedure by id, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.WRITE WRITE"/> for the duration of the procedure execution. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWriteOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallWriteOverride(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a schema write procedure by id. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchema(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallSchema(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a schema write procedure by id, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.FULL FULL"/> for the duration of the procedure execution. </summary>
      /// <param name="id"> the id of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchemaOverride(int id, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride(int id, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read-only procedure by name. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallRead(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallRead(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read-only procedure by name, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the procedure execution. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallReadOverride(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallReadOverride(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read/write procedure by name. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWrite(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallWrite(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read/write procedure by name, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.WRITE WRITE"/> for the duration of the procedure execution. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallWriteOverride(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallWriteOverride(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a schema write procedure by name. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchema(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallSchema(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a schema write procedure by name, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.FULL FULL"/> for the duration of the procedure execution. </summary>
      /// <param name="name"> the name of the procedure. </param>
      /// <param name="arguments"> the procedure arguments. </param>
      /// <param name="context"> the procedure call context. </param>
      /// <returns> an iterator containing the procedure results. </returns>
      /// <exception cref="ProcedureException"> if there was an exception thrown during procedure execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallSchemaOverride(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, Object[] arguments, org.Neo4Net.Kernel.Api.Internal.Procs.ProcedureCallContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallSchemaOverride(QualifiedName name, object[] arguments, ProcedureCallContext context);

      /// <summary>
      /// Invoke a read-only function by id </summary>
      /// <param name="id"> the id of the function. </param>
      /// <param name="arguments"> the function arguments. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.AnyValue functionCall(int id, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      AnyValue FunctionCall(int id, AnyValue[] arguments);

      /// <summary>
      /// Invoke a read-only function by name </summary>
      /// <param name="name"> the name of the function. </param>
      /// <param name="arguments"> the function arguments. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.AnyValue functionCall(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      AnyValue FunctionCall(QualifiedName name, AnyValue[] arguments);

      /// <summary>
      /// Invoke a read-only function by id, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
      /// <param name="id"> the id of the function. </param>
      /// <param name="arguments"> the function arguments. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.AnyValue functionCallOverride(int id, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      AnyValue FunctionCallOverride(int id, AnyValue[] arguments);

      /// <summary>
      /// Invoke a read-only function by name, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
      /// <param name="name"> the name of the function. </param>
      /// <param name="arguments"> the function arguments. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.AnyValue functionCallOverride(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name, org.Neo4Net.values.AnyValue[] arguments) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      AnyValue FunctionCallOverride(QualifiedName name, AnyValue[] arguments);

      /// <summary>
      /// Create a read-only aggregation function by id </summary>
      /// <param name="id"> the id of the function </param>
      /// <returns> the aggregation function </returns>
      /// <exception cref="ProcedureException"> </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.Procs.UserAggregator aggregationFunction(int id) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      IUserAggregator AggregationFunction(int id);

      /// <summary>
      /// Create a read-only aggregation function by name </summary>
      /// <param name="name"> the name of the function </param>
      /// <returns> the aggregation function </returns>
      /// <exception cref="ProcedureException"> </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.Procs.UserAggregator aggregationFunction(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      IUserAggregator AggregationFunction(QualifiedName name);

      /// <summary>
      /// Invoke a read-only aggregation function by id, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
      /// <param name="id"> the id of the function. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.Procs.UserAggregator aggregationFunctionOverride(int id) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      IUserAggregator AggregationFunctionOverride(int id);

      /// <summary>
      /// Invoke a read-only aggregation function by name, and set the transaction's access mode to
      /// <seealso cref="AccessMode.Static.READ READ"/> for the duration of the function execution. </summary>
      /// <param name="name"> the name of the function. </param>
      /// <exception cref="ProcedureException"> if there was an exception thrown during function execution. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.Procs.UserAggregator aggregationFunctionOverride(org.Neo4Net.Kernel.Api.Internal.Procs.QualifiedName name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      IUserAggregator AggregationFunctionOverride(QualifiedName name);

      /// <summary>
      /// Retrieve a value mapper for mapping values to regular Java objects. </summary>
      /// <returns> a value mapper that maps to Java objects. </returns>
      ValueMapper<object> ValueMapper();
   }
}
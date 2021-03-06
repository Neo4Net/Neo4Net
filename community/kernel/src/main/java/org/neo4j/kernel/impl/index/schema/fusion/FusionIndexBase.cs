﻿using System;
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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.fusion
{

	using Org.Neo4j.Function;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

	/// <summary>
	/// Acting as a simplifier for the multiplexing that is going in inside a fusion index. A fusion index consists of multiple parts,
	/// each handling one or more value groups. Each instance, be it a reader, populator or accessor should extend this class
	/// to get that multiplexing at a low cost. All parts will live in an array with specific slot constants to each specific part.
	/// </summary>
	/// @param <T> type of instances </param>
	public abstract class FusionIndexBase<T>
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 internal static System.Func<Value, ValueGroup> GroupOf = Value::valueGroup;

		 internal readonly SlotSelector SlotSelector;
		 internal readonly InstanceSelector<T> InstanceSelector;

		 internal FusionIndexBase( SlotSelector slotSelector, InstanceSelector<T> instanceSelector )
		 {
			  this.SlotSelector = slotSelector;
			  this.InstanceSelector = instanceSelector;
		 }

		 /// <summary>
		 /// See <seealso cref="forAll(ThrowingConsumer, object[])"/>
		 /// 
		 /// Method for calling a lambda function on many objects when it is expected that the function might
		 /// throw an exception. First exception will be thrown and subsequent will be suppressed.
		 /// 
		 /// For example, in FusionIndexAccessor:
		 /// <pre>
		 ///    public void drop() throws IOException
		 ///    {
		 ///        forAll( IndexAccessor::drop, accessorList );
		 ///    }
		 /// </pre>
		 /// </summary>
		 /// <param name="consumer"> lambda function to call on each object passed </param>
		 /// <param name="subjects"> <seealso cref="System.Collections.IEnumerable"/> of objects to call the function on </param>
		 /// @param <E> the type of exception anticipated, inferred from the lambda </param>
		 /// <exception cref="E"> if consumption fails with this exception </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T, E extends Exception> void forAll(org.neo4j.function.ThrowingConsumer<T,E> consumer, Iterable<T> subjects) throws E
		 public static void ForAll<T, E>( ThrowingConsumer<T, E> consumer, IEnumerable<T> subjects ) where E : Exception
		 {
			  Iterables.safeForAll( consumer, subjects );
		 }

		 /// <summary>
		 /// See <seealso cref="forAll(ThrowingConsumer, System.Collections.IEnumerable)"/>
		 /// 
		 /// Method for calling a lambda function on many objects when it is expected that the function might
		 /// throw an exception. First exception will be thrown and subsequent will be suppressed.
		 /// 
		 /// For example, in FusionIndexAccessor:
		 /// <pre>
		 ///    public void drop() throws IOException
		 ///    {
		 ///        forAll( IndexAccessor::drop, firstAccessor, secondAccessor, thirdAccessor );
		 ///    }
		 /// </pre>
		 /// </summary>
		 /// <param name="consumer"> lambda function to call on each object passed </param>
		 /// <param name="subjects"> varargs array of objects to call the function on </param>
		 /// @param <E> the type of exception anticipated, inferred from the lambda </param>
		 /// <exception cref="E"> if consumption fails with this exception </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T, E extends Exception> void forAll(org.neo4j.function.ThrowingConsumer<T,E> consumer, T[] subjects) throws E
		 public static void ForAll<T, E>( ThrowingConsumer<T, E> consumer, T[] subjects ) where E : Exception
		 {
			  ForAll( consumer, Arrays.asList( subjects ) );
		 }

		 public static bool ConsistencyCheck<T>( IEnumerable<T> checkables, ReporterFactory reporterFactory ) where T : Org.Neo4j.Kernel.Impl.Index.Schema.ConsistencyCheckable
		 {
			  bool result = true;
			  foreach ( ConsistencyCheckable part in checkables )
			  {
					result &= part.ConsistencyCheck( reporterFactory );
			  }
			  return result;
		 }
	}

}
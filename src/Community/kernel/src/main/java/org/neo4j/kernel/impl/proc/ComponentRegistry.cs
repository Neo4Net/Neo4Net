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

	using Neo4Net.Function;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Context = Neo4Net.Kernel.api.proc.Context;

	/// <summary>
	/// Tracks components that can be injected into compiled procedures.
	/// </summary>
	public class ComponentRegistry
	{
		 /// <summary>
		 /// Given the context of a procedure call, provide some component. </summary>
		 public interface Provider<T> : ThrowingFunction<Context, T, ProcedureException>
		 {
			  // This interface intentionally empty, alias for the Function generic above
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<Class, Provider<?>> suppliers = new java.util.HashMap<>();
		 private readonly IDictionary<Type, Provider<object>> _suppliers = new Dictionary<Type, Provider<object>>();

		 public ComponentRegistry()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") <T> Provider<T> providerFor(Class<T> type)
		 internal virtual Provider<T> ProviderFor<T>( Type type )
		 {
				 type = typeof( T );
			  return ( Provider<T> ) _suppliers[type];
		 }

		 public virtual void Register<T>( Type cls, Provider<T> supplier )
		 {
				 cls = typeof( T );
			  _suppliers[cls] = supplier;
		 }
	}

}
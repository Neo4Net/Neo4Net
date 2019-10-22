using System.Threading;

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
namespace Neo4Net.Kernel.api.proc
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

	/// <summary>
	/// The context in which a procedure is invoked. This is a read-only map-like structure.
	/// For instance, a read-only transactional procedure might have access to the current statement it is being invoked
	/// in through this.
	/// 
	/// The context is entirely defined by the caller of the procedure,
	/// so what is available in the context depends on the context of the call.
	/// </summary>
	public interface Context
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T> T get(Key<T> key) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException;
		 T get<T>( Key<T> key );
		 T getOrElse<T>( Key<T> key, T orElse );
	}

	public static class Context_Fields
	{
		 public static readonly Key<DependencyResolver> DependencyResolver = Key.key( "DependencyResolver", typeof( DependencyResolver ) );
		 public static readonly Key<GraphDatabaseAPI> DatabaseApi = Key.key( "DatabaseAPI", typeof( GraphDatabaseAPI ) );
		 public static readonly Key<KernelTransaction> KernelTransaction = Key.key( "KernelTransaction", typeof( KernelTransaction ) );
		 public static readonly Key<SecurityContext> SecurityContext = Key.key( "SecurityContext", typeof( SecurityContext ) );
		 public static readonly Key<ProcedureCallContext> ProcedureCallContext = Key.key( "ProcedureCallContext", typeof( ProcedureCallContext ) );
		 public static readonly Key<Thread> Thread = Key.key( "Thread", typeof( Thread ) );
		 public static readonly Key<Clock> SystemClock = Key.key( "SystemClock", typeof( Clock ) );
		 public static readonly Key<Clock> StatementClock = Key.key( "StatementClock", typeof( Clock ) );
		 public static readonly Key<Clock> TransactionClock = Key.key( "TransactionClock", typeof( Clock ) );
	}

}
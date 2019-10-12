using System;

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
namespace Org.Neo4j.Harness
{
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AnonymousContext = Org.Neo4j.Kernel.api.security.AnonymousContext;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Log = Org.Neo4j.Logging.Log;

	public class MyCoreAPI
	{
		 private readonly GraphDatabaseAPI _graph;
		 private readonly ThreadToStatementContextBridge _txBridge;
		 private readonly Log _log;

		 public MyCoreAPI( GraphDatabaseAPI graph, ThreadToStatementContextBridge txBridge, Log log )
		 {
			  this._graph = graph;
			  this._txBridge = txBridge;
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long makeNode(String label) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual long MakeNode( string label )
		 {
			  long result;
			  try
			  {
					  using ( Transaction tx = _graph.beginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.write() ) )
					  {
						KernelTransaction ktx = _txBridge.getKernelTransactionBoundToThisThread( true );
						long nodeId = ktx.DataWrite().nodeCreate();
						int labelId = ktx.TokenWrite().labelGetOrCreateForName(label);
						ktx.DataWrite().nodeAddLabel(nodeId, labelId);
						result = nodeId;
						tx.Success();
					  }
			  }
			  catch ( Exception e )
			  {
					_log.error( "Failed to create node: " + e.Message );
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Failed to create node: " + e.Message, e );
			  }
			  return result;
		 }

		 public virtual long CountNodes()
		 {
			  long result;
			  using ( Transaction tx = _graph.beginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.read() ) )
			  {
					KernelTransaction kernelTransaction = this._txBridge.getKernelTransactionBoundToThisThread( true );
					result = kernelTransaction.DataRead().countsForNode(-1);
					tx.Success();
			  }
			  return result;
		 }
	}

}
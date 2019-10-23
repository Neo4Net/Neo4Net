using System;

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
namespace Neo4Net.Harness
{
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;

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
//ORIGINAL LINE: public long makeNode(String label) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public virtual long MakeNode( string label )
		 {
			  long result;
			  try
			  {
					  using ( Transaction tx = _graph.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.write() ) )
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
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Failed to create node: " + e.Message, e );
			  }
			  return result;
		 }

		 public virtual long CountNodes()
		 {
			  long result;
			  using ( Transaction tx = _graph.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.read() ) )
			  {
					KernelTransaction kernelTransaction = this._txBridge.getKernelTransactionBoundToThisThread( true );
					result = kernelTransaction.DataRead().countsForNode(-1);
					tx.Success();
			  }
			  return result;
		 }
	}

}
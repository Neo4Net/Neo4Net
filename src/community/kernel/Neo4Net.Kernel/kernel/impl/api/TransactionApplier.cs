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
namespace Neo4Net.Kernel.Impl.Api
{

	using Neo4Net.Helpers.Collections;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;

	/// <summary>
	/// Responsible for a single transaction. See also <seealso cref="BatchTransactionApplier"/> which returns an implementation of
	/// this class. Should typically be used in a try-with-resources block, f.ex.:
	/// <pre>
	/// {@code
	///     try ( TransactionApplier txApplier = batchTxApplier.startTx( txToApply )
	///     {
	///         // Apply the transaction
	///         txToApply.transactionRepresentation().accept( txApplier );
	///         // Or apply other commands
	///         // txApplier.visit( command );
	///     }
	/// }
	/// </pre>
	/// </summary>
	public interface TransactionApplier : Visitor<StorageCommand, IOException>, CommandVisitor, IDisposable
	{
		 /// <summary>
		 /// A <seealso cref="TransactionApplier"/> which does nothing.
		 /// </summary>

		 /// <summary>
		 /// Delegates to individual visit methods (see <seealso cref="CommandVisitor"/>) which need to be implemented, as well as
		 /// <seealso cref="close()"/> if applicable.
		 /// </summary>
	}

	public static class TransactionApplier_Fields
	{
		 public static readonly TransactionApplier Empty = new TransactionApplier_Adapter();
	}

	 public class TransactionApplier_Adapter : CommandVisitor_Adapter, TransactionApplier
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		  public override void Close()
		  {
				// Do nothing
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.Neo4Net.storageengine.api.StorageCommand element) throws java.io.IOException
		  public override bool Visit( StorageCommand element )
		  {
				return ( ( Command )element ).handle( this );
		  }
	 }

}
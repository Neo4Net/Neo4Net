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
namespace Neo4Net.Kernel.Api.txstate.auxiliary
{

	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

	/// <summary>
	/// The container of, and facade to, the auxiliary transaction state that a transaction may hold.
	/// <para>
	/// Instances of this interface are obtained from the <seealso cref="AuxiliaryTransactionStateManager.openStateHolder()"/> method.
	/// </para>
	/// </summary>
	public interface AuxiliaryTransactionStateHolder : IDisposable
	{
		 /// <summary>
		 /// Get the auxiliary transaction state identified by the given <seealso cref="AuxiliaryTransactionStateProvider.getIdentityKey() provider identity key"/>.
		 /// <para>
		 /// If this transaction does not yet have transaction state from the given provider, then the provider is used to create an
		 /// <seealso cref="AuxiliaryTransactionState"/> instance, which is then cached for the remainder of the transaction.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="providerIdentityKey"> The <seealso cref="AuxiliaryTransactionStateProvider.getIdentityKey() provider identity key"/> that the desired provider is identified
		 /// by. </param>
		 /// <returns> The transaction state from the given provider, either cached, or newly created. </returns>
		 AuxiliaryTransactionState GetState( object providerIdentityKey );

		 /// <summary>
		 /// Used by the <seealso cref="KernelTransactionImplementation"/> to determine if the auxiliary transaction state may have any commands that needs to be extracted.
		 /// <para>
		 /// This would be the case if any of the internal auxiliary transaction state instances claims to
		 /// <seealso cref="AuxiliaryTransactionState.hasChanges() have changes"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if calling <seealso cref="extractCommands(System.Collections.ICollection)"/> would yield commands. </returns>
		 bool HasChanges();

		 /// <summary>
		 /// Extract commands, if any, from the auxiliary transaction state instances.
		 /// <para>
		 /// This method delegates to the <seealso cref="AuxiliaryTransactionState.extractCommands(System.Collections.ICollection)"/> method of all of the internal auxiliary transaction states
		 /// that claim to have changes ready for extraction.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="extractedCommands"> The collection to add the extracted commands to. </param>
		 /// <exception cref="TransactionFailureException"> If the transaction state wanted to produce commands, but is somehow unable to do so. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void extractCommands(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StorageCommand> extractedCommands) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
		 void ExtractCommands( ICollection<StorageCommand> extractedCommands );

		 /// <summary>
		 /// Close all of the internal <seealso cref="AuxiliaryTransactionState"/> instances, and release all of their resources.
		 /// </summary>
		 /// <exception cref="AuxiliaryTransactionStateCloseException"> if something went wrong when closing the internal auxiliary transaction states. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws AuxiliaryTransactionStateCloseException;
		 void Close();
	}

}
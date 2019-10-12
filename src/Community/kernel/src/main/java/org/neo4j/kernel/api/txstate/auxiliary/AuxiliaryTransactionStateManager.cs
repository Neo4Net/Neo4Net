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
namespace Neo4Net.Kernel.api.txstate.auxiliary
{
	/// <summary>
	/// The auxiliary transaction state manager keeps track of what auxiliary transaction state providers are available, and is responsible for creating
	/// <seealso cref="AuxiliaryTransactionStateHolder"/> instances, which are the per-transaction containers of auxiliary transaction state.
	/// </summary>
	public interface AuxiliaryTransactionStateManager
	{
		 /// <summary>
		 /// Register a new <seealso cref="AuxiliaryTransactionStateProvider"/>.
		 /// <para>
		 /// This method is thread-safe. Only <seealso cref="AuxiliaryTransactionStateHolder"/> instances that are opened after the completion of a provider registration,
		 /// will have that provider available.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="provider"> The provider to register. </param>
		 void RegisterProvider( AuxiliaryTransactionStateProvider provider );

		 /// <summary>
		 /// Unregister the given <seealso cref="AuxiliaryTransactionStateProvider"/>.
		 /// <para>
		 /// This method is thread-safe. The transaction state provider will still be referenced by existing <seealso cref="AuxiliaryTransactionStateHolder"/> instances,
		 /// but will not be available to holders that are opened after the unregistration completes.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="provider"> The provider to unregister. </param>
		 void UnregisterProvider( AuxiliaryTransactionStateProvider provider );

		 /// <summary>
		 /// Open a new <seealso cref="AuxiliaryTransactionStateHolder"/> for a transaction.
		 /// </summary>
		 /// <returns> A new auxiliary transaction state holder. </returns>
		 AuxiliaryTransactionStateHolder OpenStateHolder();
	}

}